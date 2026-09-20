using System;
using System.Collections.Generic;
using System.Numerics;
using Taffy.Play.Bullet;
using Taffy.Play.Trigger;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Taffy.Play.Enemy
{
    public class EnemyData : MonoBehaviour
    {
        private World world;
        private EntityManager em;
        public Entity entity = Entity.Null;
        public SpriteRenderer sprite;
        public Collider collider;
        [Header("近战触发器（池出池时会用它预置 ATK）")]public GameObject attacktrigger;

        public GameObject goA;
        public GameObject goB;
        
        public Transform playerA => goA.transform;
        public Transform playerB => goB.transform;

        public bool isHateA = false;
        public bool isHateB = false;

        private NavMeshPath path;
        Vector3[] corners = new Vector3[64];
        [Header("多远之内才会追")]public float PursueDistance = 20f;

        /// <summary>重寻路间隔（秒）：每隔这么久重新采样并算一次路径，把新方向推给 ECS</summary>
        [Header("重寻路间隔")]public float RepathInterval = 0.3f;

        /// <summary>到 NavMesh 的距离超过这个值就算采不到点，退化成朝目标直走</summary>
        [Header("NavMesh采样半径")]public float SampleRadius = 5f;

        private float lastRepathTime = -999f;

        [Header("调试：开启敌人寻路日志（查追击问题时打开，不用就关）")]
        public bool debugPursue = true;

        private float lastDebugTime = -999f;
        private float lastDebugPosX, lastDebugPosZ;
        
        [Header("敌人名__要保证唯一")]public string name = "";
        [Header("血量")]public int HP = 0;
        [Header("类型列表")]public List<EnemyBehavior_value> Behavior = new List<EnemyBehavior_value>();
        private bool isRemoteAttack = true;
        [Header("攻速")]public float AttackSpeed = 0;
        public float AttackCounter;
        [Header("攻击半径")]public float AttackRadius = 0;
        [Header("攻击力")]public int ATK = 0;
        [Header("防御力")]public int DEF = 0;
        
        public MoveState state = MoveState.Idle;
        
        [Header("游荡速度")]public float walkSpeed = 0;
        [Header("游荡最远时间")]public float walkTime = 0;
        [Header("追击速度")]public float pursueSpeed = 0;
        [Header("停留时间")]public float idleTime = 0;

        public float dirX
        {
            set
            {
                if (!sprite) return;
                bool shouldFlip = value < 0f;
                if (sprite.flipX != shouldFlip)
                {
                    sprite.flipX = shouldFlip;
                }
            }
        }

        //给敌人死亡上锁，因为mono这边不知道dots那死没死，只知道dots是不是把e销毁了
        private bool isDead = false;
        public bool dead
        {
            get => isDead;
            set
            { 
                isDead = value;
                Dead();
            }
        }
        public bool isAlive = false;
        public float deadAnimTime = 0.3f;
        public float deadAnimTimeCounter = 0.3f;

        public event Action DeadEvent;

        public void CopyInfosTo(EnemyData e)
        {
            e.name = name;
            e.HP = HP;
            e.Behavior.Clear();
            e.Behavior.AddRange(Behavior);
            e.AttackSpeed = AttackSpeed;
            e.AttackRadius = AttackRadius;
            e.ATK = ATK;
            e.DEF = DEF;
            e.walkSpeed = walkSpeed;
            e.pursueSpeed = pursueSpeed;
            e.idleTime = idleTime;
            e.walkTime = walkTime;
            // 远程/近战是跟着 Behavior 走的，拷完必须重算：
            // 池底预制体 Enemy_Default 的 Behavior 是空的，Awake 里只会算出"远程"，
            // 不重算的话 Bandit 也会走远程分支，永远不放近战触发器、也不会贴脸
            e.RefreshAttackKind();
        }

        /// <summary>
        /// 依据 Behavior 重新判定远程/近战。Behavior 变化后（拷贝配置、改预制体）都要调一次。
        /// </summary>
        public void RefreshAttackKind()
        {
            bool remote = true;
            foreach (var b in Behavior)
            {
                if (b.type == EnemyBehavior.CloseAttack)
                {
                    remote = false;
                    break;
                }
            }
            isRemoteAttack = remote;
            SyncMeleeTriggerATK();
        }

        /// <summary>
        /// 把当前的 ATK 同步到近战触发器上。
        /// 近战触发器在 Awake 就是激活状态（接触即触发），而它自己的 ATK 默认是 0，
        /// 只在 Attack() 里被赋值 —— 不在这里预先同步的话，第一次接触会打出 0 伤害。
        /// </summary>
        public void SyncMeleeTriggerATK()
        {
            if (attacktrigger == null) return;
            var t = attacktrigger.GetComponent<EnemyAttackTrigger>();
            if (t != null) t.ATK = ATK;

            // 近战命中范围要跟 AttackRadius 对齐：触发器默认半径只有 0.5，而近战敌人
            // 在 AttackRadius（默认 2）处就停下"挥刀"，不把半径同步过去就会永远挥空。
            if (!isRemoteAttack)
            {
                var col = attacktrigger.GetComponent<CapsuleCollider>();
                if (col != null) col.radius = AttackRadius;
            }
        }

        private void Awake()
        {
            debugPursue = true;   // 调试期强制开启：prefab 里可能序列化了 false，会覆盖字段默认值
            path = new NavMeshPath();
            sprite = GetComponent<SpriteRenderer>();
            collider = gameObject.GetComponent<Collider>();
            if (collider != null) collider.enabled = true;
            attacktrigger = transform.Find("EnemyAttackTrigger").gameObject;
            attacktrigger.SetActive(true);
            world =  World.DefaultGameObjectInjectionWorld;
            if(world is not null) em = world.EntityManager;

            RefreshAttackKind();
        }

        private void OnEnable()
        {
            isAlive = true;
            isDead = false;
            deadAnimTimeCounter = deadAnimTime;
        }

        private void Update()
        {
            if (world is null || entity == Entity.Null) return;
            if(!em.Exists(entity)) dead = true;

            if (AttackCounter > 0)
            {
                AttackCounter -= Time.deltaTime;

                // 攻击硬直结束：解除"不许动"的锁。
                // CanAttackComp 只在 Attack() 里被置 true，以前没有任何地方清回 false，
                // 而 PursueJob 只在 !attack 时才位移 —— 所以敌人攻击一次就永久冻住了。
                if (AttackCounter <= 0f && em.Exists(entity))
                {
                    bool chasing = isHateA || isHateB;
                    em.SetComponentData(entity, new CanAttackComp { value = false });
                    em.SetComponentData(entity, new IsPursueComp  { value = chasing });
                    if (!chasing)
                    {
                        // 已经脱仇了，把方向清掉，交回给游荡的 MoveJob
                        em.SetComponentData(entity, new PursueDirComp { x = 0, z = 0 });
                    }
                    lastRepathTime = -999f;   // 让下一次 StartPursue 立刻重新算路径
                }
            }
        }

        private void OnDisable()
        {
            isAlive = false;
            isDead = true;
            DeadEvent = null;
        }

        private void OnDestroy()
        {
            this.UnlinkEntity();
        }
        
        private void Dead()
        {
            if (isAlive && isDead)
            {
                if(deadAnimTimeCounter >= deadAnimTime) collider.enabled = false;
                deadAnimTimeCounter -= Time.deltaTime;
                if (deadAnimTimeCounter <= 0)
                {
                    DeadEvent?.Invoke();
                    EnemyPool.Instance.RecycleEnemy(gameObject);
                }
            }
        }

        public void Injury(int ATK, Vector2 position)
        {
            if (isDead || entity == Entity.Null || world is null) return;
            int atk = em.GetComponentData<InjuryComp>(entity).injury + ATK;
            em.SetComponentData(entity,new InjuryComp{injury = atk});
            em.SetComponentData(entity,new ReturnDirectionComp {x = position.x,y = position.y});

            // 受击变红 0.2 秒
            if (sprite != null)
            {
                Color original = sprite.color;
                sprite.color = new Color(0.9f, 0f, 0.2f, 1f);
                TaskMgr.AddTask(0.2f, () =>
                {
                    if (sprite != null) sprite.color = original;
                });
            }
        }
        
//////// 寻路 ///////////////////////////////////////////////////////

        public void StartPursue()
        {
            if (!isHateA && !isHateB) return;

            // 每 RepathInterval 秒重新采样 + 重算一次路径，把新方向推给 ECS（默认 0.3 秒）
            if (Time.time - lastRepathTime < RepathInterval) return;
            lastRepathTime = Time.time;

            Vector3 target;
            if (isHateA && isHateB)
            {
                float lenA = GetPathLength(playerA.position);
                float lenB = GetPathLength(playerB.position);
                // lenA 是有限值说明 A 可达，优先选可达的那个，避免选到算不出路径的玩家
                if (float.IsPositiveInfinity(lenB)) target = playerA.position;
                else if (float.IsPositiveInfinity(lenA)) target = playerB.position;
                else target = lenA <= lenB ? playerA.position : playerB.position;
            }
            else target = isHateA ? playerA.position : playerB.position;

            // 无论走哪个分支都重算一次：否则上面比长度失败时 corners 里会残留上一个目标的旧路径
            GetPathLength(target);

            float direct = Vector3.Distance(transform.position, target);
            float pathLen = PathLengthFromCorners();
            bool noPath = float.IsPositiveInfinity(pathLen);

            // 攻击范围用"实际要走的路程"判断：隔着墙/绕远路时路径长度会远大于直线距离，
            // 这种情况本来就不该出手。只有算不出路径时才退回直线距离，避免采样失败时发呆。
            float approach = noPath ? direct : pathLen;

            // 太远就不管。两个距离都超出才放弃。
            // 注意：PursueTrigger 半径 20，但敌人自己的 collider 还有半径（最大 1.3），
            // 所以敌人中心在 20~21.3 就会触发仇恨；chaseRange 必须盖过这段，否则敌人
            // 刚进仇恨就被判"太远"退回游荡，表现成"进了仇恨范围但不追击"。
            float chaseRange = Mathf.Max(PursueDistance, SampleRadius * 2f) + 2f;
            if (direct > chaseRange && approach > chaseRange)
            {
                Debug.Log($"[寻路]{name} 结果: 太远放弃 直线={direct:F1} 路径={(noPath ? -1f : pathLen):F1} 上限={chaseRange:F1}，退回游荡");
                // 这里直接 return 的话，如果攻击硬直刚结束就把方向清了，敌人会一直冻着。
                // 交回给游荡：解除攻击锁、退出追击状态。
                if (AttackCounter <= 0f)
                {
                    em.SetComponentData(entity, new CanAttackComp { value = false });
                    em.SetComponentData(entity, new IsPursueComp  { value = false });
                }
                return;
            }

            bool canSee = !Physics.Linecast(transform.position, target, out _, 1 << 0, QueryTriggerInteraction.Ignore);
            bool canAttackNow = AttackCounter <= 0f;

            // 位移监控：每 0.5 秒打印一次敌人实际移动量。=0 说明 ECS 根本没让它动
            if (debugPursue && Time.time - lastDebugTime >= 0.5f)
            {
                lastDebugTime = Time.time;
                float moved = Vector3.Distance(new Vector3(lastDebugPosX, 0f, lastDebugPosZ),
                                               new Vector3(transform.position.x, 0f, transform.position.z));
                lastDebugPosX = transform.position.x;
                lastDebugPosZ = transform.position.z;
                Debug.Log($"[寻路]{name} 位移监控: 0.5秒移动={moved:F2}（=0说明敌人没在动）");
            }

            if (debugPursue)
            {
                Debug.Log($"[寻路]{name} 决策: 敌人=({transform.position.x:F1},{transform.position.z:F1}) " +
                          $"目标=({target.x:F1},{target.z:F1}) 直线={direct:F1} 路径={(noPath ? -1f : pathLen):F1} " +
                          $"采用={approach:F1} 攻距={AttackRadius:F1} 可见={canSee} 可攻={canAttackNow}");
            }

            if (approach <= AttackRadius && canSee && canAttackNow)
            {
                Debug.Log($"[寻路]{name} 结果: 攻击");
                Attack(target);
            }
            else
            {
                Debug.Log($"[寻路]{name} 结果: 追击");
                Pursue(target, noPath);
            }
        }

        /// <summary>
        /// 采样并计算到目标的路径，结果留在 path / corners 里。
        /// 成功返回路径长度；采不到点或路径不完整返回 PositiveInfinity（调用方退化成直走）。
        /// </summary>
        private float GetPathLength(Vector3 targetPosition)
        {
            if (!NavMesh.SamplePosition(transform.position, out var s, SampleRadius, NavMesh.AllAreas))
            {
                Debug.Log($"[寻路]{name} NavMesh采样失败: 敌人位置({transform.position.x:F1},{transform.position.z:F1}) 不在NavMesh附近(半径{SampleRadius})");
                return float.PositiveInfinity;
            }
            if (!NavMesh.SamplePosition(targetPosition,   out var t, SampleRadius, NavMesh.AllAreas))
            {
                Debug.Log($"[寻路]{name} NavMesh采样失败: 目标位置({targetPosition.x:F1},{targetPosition.z:F1}) 不在NavMesh附近(半径{SampleRadius})");
                return float.PositiveInfinity;
            }
            if (!NavMesh.CalculatePath(s.position, t.position, NavMesh.AllAreas, path))
            {
                Debug.Log($"[寻路]{name} NavMesh路径计算失败");
                return float.PositiveInfinity;
            }

            if (path.status != NavMeshPathStatus.PathComplete)
            {
                Debug.Log($"[寻路]{name} NavMesh路径不完整 status={path.status}（会退化成直线追击）");
                return float.PositiveInfinity;
            }

            return PathLengthFromCorners();
        }

        /// <summary>把当前 path 的拐点刷进 corners 并返回总长度</summary>
        private float PathLengthFromCorners()
        {
            int count = path.GetCornersNonAlloc(corners);
            if (count == 0) return float.PositiveInfinity;

            float len = 0f;
            Vector3 prev = transform.position;
            for (int i = 0; i < count; i++) { len += Vector3.Distance(prev, corners[i]); prev = corners[i]; }
            return len;
        }

        /// <summary>
        /// 取路径上的下一个拐点方向；拐点全都被走过（count &lt; 2）时退化成朝目标直走。
        /// </summary>
        private Vector3 GetSteerDirection(Vector3 target)
        {
            int count = path.GetCornersNonAlloc(corners);
            if (count < 2) return target - transform.position;

            Vector3 next = corners[count - 1];
            for (int i = 0; i < count; i++)
            {
                // 只用 XZ 平面距离判断拐点是否"已走过"，不能算 Y：
                // 敌人是物理下落的，和 NavMesh 采样点之间有固定高度差，
                // 用三维距离会把"起点采样点（XZ 和敌人重合、仅 Y 不同）"误判成还没走过，
                // 最终返回纯 Y 向量，被 Pursue 里 dir.y=0 清成零向量 → 方向永远不更新。
                float dx = corners[i].x - transform.position.x;
                float dz = corners[i].z - transform.position.z;
                if (dx * dx + dz * dz > 0.16f)
                {
                    next = corners[i];
                    break;
                }
            }
            return next - transform.position;
        }

        /// <summary>
        /// 把移动方向推给 ECS：优先沿路径的下一个拐点，拐点不可用时朝目标直走。
        /// </summary>
        private void Pursue(Vector3 target, bool noPath)
        {
            Vector3 dir = noPath ? (target - transform.position) : GetSteerDirection(target);

            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
            {
                Debug.Log($"[寻路]{name} 追击方向为零，跳过（敌人已在目标点？）");
                return;
            }

            Vector3 v = dir.normalized;
            // 面朝前进方向：dirX 是只写属性，全工程只有 DOTS 同步在写，这里补一次才有人给 sprite 翻面
            dirX = v.x;
            em.SetComponentData(entity, new IsPursueComp  { value = true });
            em.SetComponentData(entity, new PursueDirComp { x = v.x, z = v.z });
            em.SetComponentData(entity, new CanAttackComp { value = false });

            Debug.Log($"[寻路]{name} 已把追击方向({v.x:F2},{v.z:F2})推进ECS，IsPursueComp=true");
        }

        public void StopPursue()
        {
            Debug.Log($"[寻路]{name} 停止追击（脱仇/退出仇恨圈）");
            // 可能被已经回收/已销毁的敌人调到（entity 已是 Entity.Null），先挡一下
            if (world is null || entity == Entity.Null || !em.Exists(entity))
            {
                lastRepathTime = -999f;
                return;
            }

            em.SetComponentData(entity, new IsPursueComp  { value = false });
            em.SetComponentData(entity, new PursueDirComp { x = 0, z = 0 });
            em.SetComponentData(entity, new CanAttackComp { value = false });
            lastRepathTime = -999f;   // 下次进入仇恨范围时立刻重算一次，不用等间隔到点
        }
        
/////// 攻击 /////////////////////////////////////////////////////
        private void Attack(Vector3 target)
        {
            Debug.Log($"[寻路]{name} 攻击（{(isRemoteAttack ? "远程" : "近战")}）ATK={ATK}");
            AttackCounter = AttackSpeed;
            // 攻击硬直：把"不许动"打开，等 AttackCounter 走完由 Update() 解除
            em.SetComponentData(entity, new IsPursueComp  { value = true });
            em.SetComponentData(entity, new PursueDirComp { x = 0, z = 0 });
            em.SetComponentData(entity, new CanAttackComp { value = true });
            lastRepathTime = -999f;   // 攻击完立刻重新决策一次

            // 面朝玩家
            Vector3 face = target - transform.position;
            face.y = 0f;
            if (face.sqrMagnitude > 0.0001f) dirX = face.normalized.x;

            if(!isRemoteAttack)
            {
                attacktrigger.GetComponent<EnemyAttackTrigger>().ATK = ATK;
                attacktrigger.SetActive(true);
            }
            else
            {
                Vector3 v = (target - transform.position).normalized;
                // 敌人子弹要打玩家，并且必须带上 ATK（否则命中也是 0 伤害）
                BulletPool.Instance.GetBullet(transform, v.x, v.z, AttackRadius,
                    target: BulletData.PlayerTag, atk: ATK);
            }
        }
    }
}