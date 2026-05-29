using UnityEngine;

/// <summary>
/// A类敌人 - 远程攻击，20%概率中毒
/// </summary>
public class EnemyA : EnemyBase
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    private EnemyA_SO _rangedData;

    protected override void Start()
    {
        // 转换数据类型并检测配置
        _rangedData = enemyData as EnemyA_SO;
        if (_rangedData == null)
        {
            Debug.LogError($"{gameObject.name} 的数据不是 EnemyA_SO 类型！请检查Inspector中拖拽的SO文件。");
            return;
        }

        base.Start();
    }

    protected override void InitFSM()
    {
        // 创建数据黑板
        _dataBoard = new EnemyDataBoard(enemyData, transform, this);

        // 创建FSM
        _fsm = new Fsm(_dataBoard);

        // 注册状态（追击状态注册到基类 ChaseState 的 key 下，
        // 这样 PatrolState 里的 SwitchState<ChaseState>() 才能命中）
        _fsm.AddState<PatrolState>(fsm => new PatrolState(fsm));
        _fsm.AddState<ChaseState>(fsm => new EnemyA_ChaseState(fsm));
        _fsm.AddState<RangedAttackState>(fsm => new RangedAttackState(fsm));
        _fsm.AddState<DeathState>(fsm => new DeathState(fsm));

        // 初始状态
        _fsm.SwitchState<PatrolState>();
    }

    public override void OnRangedAttack()
    {
        if (_rangedData == null) return;

        // 计算发射方向（朝向玩家当前位置）
        Vector3 targetPos = _dataBoard.TargetPlayer != null ?
            _dataBoard.TargetPlayer.position : transform.position + transform.forward * 10f;

        Vector3 fireDirection = (targetPos - GetFirePosition()).normalized;

        // 创建投射物
        FireProjectile(fireDirection);
    }

    private void FireProjectile(Vector3 direction)
    {
        GameObject projectileObj = null;

        // 优先使用预制体引用，否则尝试加载
        if (projectilePrefab != null)
        {
            projectileObj = Instantiate(projectilePrefab, GetFirePosition(), Quaternion.identity);
        }
        else
        {
            // 尝试从路径加载
            var prefab = Resources.Load<GameObject>(_rangedData.ProjectilePrefabPath);
            if (prefab != null)
            {
                projectileObj = Instantiate(prefab, GetFirePosition(), Quaternion.identity);
            }
        }

        if (projectileObj != null)
        {
            var projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Init(
                    _rangedData.atk,
                    direction,
                    _rangedData.SpecialEffectChance,
                    _rangedData.SpecialEffectType,
                    _rangedData.owner
                );
            }
            else
            {
                Debug.LogWarning($"投射物预制体没有 Projectile 组件！");
            }
        }
        else
        {
            Debug.LogWarning($"无法创建投射物，请检查预制体设置！");
        }
    }

    private Vector3 GetFirePosition()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }
        return transform.position + transform.forward * 0.5f + Vector3.up * 1f;
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制视野范围
        if (enemyData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.aggroRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.loseAggroRadius);
        }

        // 绘制发射点
        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(firePoint.position, 0.1f);
        }
    }
}
