using UnityEngine;
using Taffy.Data;
using Taffy.Data.PropData;
using Taffy.Play.Player;

/// <summary>
/// 敌人发射的投射物
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;

    private int _damage;
    private float _specialEffectChance;
    private BuffType _specialEffectType;
    private Vector3 _direction;
    private float _spawnTime;
    private PropOwner _owner;
    private Transform _firer;     // 发射者，用于跳过自己的碰撞体

    /// <summary>
    /// 初始化投射物
    /// </summary>
    public void Init(int damage, Vector3 direction, float specialEffectChance, BuffType specialEffectType, PropOwner owner, Transform firer = null)
    {
        _damage = damage;
        _direction = direction.normalized;
        _specialEffectChance = specialEffectChance;
        _specialEffectType = specialEffectType;
        _owner = owner;
        _firer = firer;
        _spawnTime = Time.time;

        // 面向飞行方向
        if (_direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(_direction);
        }
    }

    private void Update()
    {
        // 移动
        transform.position += _direction * speed * Time.deltaTime;

        // 生命周期检查
        if (Time.time - _spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 跳过发射者自身的碰撞体，避免投射物立刻撞到自己
        if (_firer != null && (other.transform == _firer || other.transform.IsChildOf(_firer)))
            return;

        // 检查是否击中玩家
        PlayingHandler_A playerA = other.GetComponent<PlayingHandler_A>();
        PlayingHandler_B playerB = other.GetComponent<PlayingHandler_B>();

        if (playerA != null)
        {
            OnHitPlayer(playerA, PropOwner.A);
        }
        else if (playerB != null)
        {
            OnHitPlayer(playerB, PropOwner.B);
        }
        else
        {
            // 打到障碍物 → 销毁
            Destroy(gameObject);
        }
    }

    private void OnHitPlayer(MonoBehaviour player, PropOwner playerOwner)
    {
        // 所有敌人都可以攻击所有玩家，不再按归属过滤

        // 死亡的玩家不会被攻击，弹丸穿过继续飞行
        var stateCtrl = PlayerCurrentStateController.Instance;
        if (stateCtrl != null && stateCtrl.GetIsDead(playerOwner))
        {
            return;
        }

        // 造成伤害
        if (stateCtrl != null)
        {
            switch (playerOwner)
            {
                case PropOwner.A:
                    stateCtrl.Injury_A(_damage);
                    break;
                case PropOwner.B:
                    stateCtrl.Injury_B(_damage);
                    break;
            }
        }
        Debug.Log($"投射物击中玩家 {playerOwner}，造成 {_damage} 点伤害");

        // 触发特殊效果（中毒）
        if (Random.value < _specialEffectChance)
        {
            ApplyBuff(playerOwner, _specialEffectType);
        }

        // 销毁投射物
        Destroy(gameObject);
    }

    private void ApplyBuff(PropOwner targetOwner, BuffType buffType)
    {
        // TODO: 通过EventBus发送添加Buff事件
        Debug.Log($"对玩家 {targetOwner} 施加 {buffType} 效果");

        // 示例：可以定义一个事件结构体
        // EventBus.Publish(new ApplyBuffEvent { Target = targetOwner, BuffType = buffType });
    }
}
