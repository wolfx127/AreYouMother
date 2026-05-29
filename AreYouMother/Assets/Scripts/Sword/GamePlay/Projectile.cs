using UnityEngine;
using Taffy.Data;
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

    /// <summary>
    /// 初始化投射物
    /// </summary>
    public void Init(int damage, Vector3 direction, float specialEffectChance, BuffType specialEffectType, PropOwner owner)
    {
        _damage = damage;
        _direction = direction.normalized;
        _specialEffectChance = specialEffectChance;
        _specialEffectType = specialEffectType;
        _owner = owner;
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
    }

    private void OnHitPlayer(MonoBehaviour player, PropOwner playerOwner)
    {
        // 检查从属关系 - 如果敌人有特定归属，只攻击对应玩家
        if (_owner != PropOwner.Public && _owner != playerOwner)
        {
            return;
        }

        // 造成伤害
        var stateCtrl = PlayerCurrentStateController.Instance;
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
