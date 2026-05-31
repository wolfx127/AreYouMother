using UnityEngine;
using SBJC.SBJC_Player_S;

/// <summary>
/// A类敌人 - 远程攻击，20%概率中毒
/// </summary>
public class EnemyA : EnemyBase
{
    [SerializeField] private Transform firePoint;

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

        if (BulletPool.Instance == null)
        {
            Debug.LogWarning($"{gameObject.name} 找不到 BulletPool，无法远程攻击");
            return;
        }

        // 计算发射方向（朝向玩家当前位置）
        Vector3 targetPos = _dataBoard.TargetPlayer != null ?
            _dataBoard.TargetPlayer.position : transform.position + transform.forward * 10f;

        Vector3 fireDirection = (targetPos - GetFirePosition()).normalized;

        // 从共用对象池取子弹发射（敌人发射 → 打所有玩家）
        FireBullet(fireDirection);
    }

    private void FireBullet(Vector3 direction)
    {
        GameObject bulletGo = BulletPool.Instance.Get();
        if (bulletGo == null) return;

        bulletGo.transform.position = GetFirePosition();
        bulletGo.transform.rotation = Quaternion.LookRotation(direction);

        var bullet = bulletGo.GetComponent<Bullet>();
        if (bullet == null)
        {
            Debug.LogWarning("BulletPool 给出的对象没有 Bullet 组件！");
            return;
        }

        // isEnemyLaunched=true：命中所有玩家，伤害取敌人攻击力
        // 注：旧 Projectile 的中毒概率(SpecialEffectChance)暂未迁移到 Bullet
        bullet.Launch(direction, true, _rangedData.atk, transform);
    }

    private Vector3 GetFirePosition()
    {
        // 水平位置取 firePoint（没有则取自身前方一点）
        Vector3 pos = firePoint != null
            ? firePoint.position
            : transform.position + transform.forward * 0.5f;

        // 高度与发射方(敌人本体)一致：子弹平飞，不从头顶射出
        pos.y = transform.position.y;
        return pos;
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
