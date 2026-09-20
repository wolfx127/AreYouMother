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
        private GameObject attacktrigger;

        public GameObject goA;
        public GameObject goB;
        
        public Transform playerA => goA.transform;
        public Transform playerB => goB.transform;

        public bool isHateA = false;
        public bool isHateB = false;

        private Transform targetPosition;
        private NavMeshPath path;
        Vector3[] corners = new Vector3[64];
        public float PursueDistance = 20f;

        private float pursueCounter = 0;
        private float pursueCD = 0.2f;
        
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
                if (sprite.flipX != value < 0)
                {
                    sprite.flipX = value < 0;
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
        }

        private void Awake()
        {
            path = new NavMeshPath();
            sprite = GetComponent<SpriteRenderer>();
            collider = gameObject.GetComponent<Collider>();
            collider.enabled = true;
            attacktrigger = transform.Find("EnemyAttackTrigger").gameObject;
            attacktrigger.SetActive(true);
            world =  World.DefaultGameObjectInjectionWorld;
            if(world is not null) em = world.EntityManager;

            foreach (var b in Behavior)
            {
                if (b.type == EnemyBehavior.CloseAttack)
                {
                    isRemoteAttack = false;
                    break;
                }
            }
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
        }
        
//////// 寻路 ///////////////////////////////////////////////////////

        public void StartPursue()
        {
            if (!isHateA && !isHateB) return;
            if (pursueCounter >= pursueCD) return;
            else if (pursueCounter > 0 && pursueCounter < pursueCD)
            {
                pursueCounter -= Time.deltaTime;
                return;
            }

            Vector3 target;
            float length = 0;
            if (isHateA && isHateB)
            {
                float lenA = GetPathLength(playerA.position);
                float lenB = GetPathLength(playerB.position);
                target = lenA <= lenB ? playerA.position : playerB.position;
            }
            else target = isHateA ? playerA.position : playerB.position;
            
            length = GetPathLength(target);

            if (length > PursueDistance) return;

            bool canSee = !Physics.Linecast(transform.position, target, out _, 1 << 0, QueryTriggerInteraction.Ignore);

            if (length <= AttackRadius && canSee && AttackCounter <= 0) Attack(target);
            else Pursue();

            pursueCounter = pursueCD;
        }

        private float GetPathLength(Vector3 targetPosition)
        {
            if (!NavMesh.SamplePosition(transform.position, out var s, 2f, NavMesh.AllAreas)) return float.PositiveInfinity;
            if (!NavMesh.SamplePosition(targetPosition,   out var t, 2f, NavMesh.AllAreas)) return float.PositiveInfinity;
            if (!NavMesh.CalculatePath(s.position, t.position, NavMesh.AllAreas, path)) return float.PositiveInfinity;

            if (path.status != NavMeshPathStatus.PathComplete) return float.PositiveInfinity;

            int count = path.GetCornersNonAlloc(corners);
            if (count == 0) return float.PositiveInfinity;

            float len = 0f;
            Vector3 prev = s.position;
            for (int i = 0; i < count; i++) { len += Vector3.Distance(prev, corners[i]); prev = corners[i]; }
            return len;
        }

        private void Pursue()
        {
            int count = path.GetCornersNonAlloc(corners);
            if (count < 2) { StopPursue(); return; }

            Vector3 next = corners[count - 1];
            for (int i = 0; i < count; i++)
                if ((corners[i] - transform.position).sqrMagnitude > 0.16f) { next = corners[i]; break; }

            Vector3 v = (next - transform.position).normalized;

            em.SetComponentData(entity, new IsPursueComp  { value = true });
            em.SetComponentData(entity, new PursueDirComp { x = v.x, z = v.z });
            em.SetComponentData(entity, new CanAttackComp { value = false });
        }

        public void StopPursue()
        {
            em.SetComponentData(entity, new IsPursueComp  { value = false });
            em.SetComponentData(entity, new PursueDirComp { x = 0, z = 0 });
            em.SetComponentData(entity, new CanAttackComp { value = false });
            pursueCounter = 0;
        }
        
/////// 攻击 /////////////////////////////////////////////////////
        private void Attack(Vector3 target)
        {
            AttackCounter = AttackSpeed;
            em.SetComponentData(entity, new IsPursueComp  { value = true });
            em.SetComponentData(entity, new PursueDirComp { x = 0, z = 0 });
            em.SetComponentData(entity, new CanAttackComp { value = true });
            pursueCounter = 0;

            if(!isRemoteAttack)
            {
                attacktrigger.GetComponent<EnemyAttackTrigger>().ATK = ATK;
                attacktrigger.SetActive(true);
            }
            else
            {
                Vector3 v = (target - transform.position).normalized;
                BulletPool.Instance.GetBullet(transform, v.x, v.z, AttackRadius, target: "player");
            }
        }
    }
}