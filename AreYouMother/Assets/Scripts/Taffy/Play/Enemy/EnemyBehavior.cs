using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Taffy.Play.Enemy
{
    public enum EnemyType
    {
        [InspectorName("无")] Default,
        [InspectorName("远程攻击")] Remote_Attack,
        [InspectorName("近战攻击")] Close_Attack,
        [InspectorName("不会攻击")] Cannot_Attack,
        [InspectorName("有掉落物")] ContainProp,
        [InspectorName("打敌人会中毒")] Poison,
        [InspectorName("亡语")] LostWord,
    }

    [Serializable]
    public struct EnemyBehavior_value
    {
        public EnemyType type;
        public float value;
    }

    public interface IEnemyBehavior
    {
        public event Action Event;
        public void Execute(Enemy e, float value);
    }
    
    public class Default_EnemyBehavior : IEnemyBehavior
    {
        public event Action Event;
        public void Execute(Enemy e, float value)
        {
            Event?.Invoke();
        }
    }

    public class Remote_Attack_EnemyBehavior : IEnemyBehavior
    {
        public event Action Event;
        public void Execute(Enemy e, float value)
        {
            Event?.Invoke();
        }
    }

    public class Close_Attack_EnemyBehavior : IEnemyBehavior
    {
        public event Action Event;
        public void Execute(Enemy e, float value)
        {
            Event?.Invoke();
        }
    }

    public class Cannot_Attack_EnemyBehavior : IEnemyBehavior
    {
        public event Action Event;
        public void Execute(Enemy e, float value)
        {
            Event?.Invoke();
        }
    }

    public class ContainProp_EnemyBehavior : IEnemyBehavior
    {
        public event Action Event;
        public void Execute(Enemy e, float value)
        {
            Event?.Invoke();
        }
    }

    public class Poison_EnemyBehavior : IEnemyBehavior
    {
        public event Action Event;
        public void Execute(Enemy e, float value)
        {
            Event?.Invoke();
        }
    }

    public class LostWord_EnemyBehavior : IEnemyBehavior
    {
        public event Action Event;
        public void Execute(Enemy e, float value)
        {
            Event?.Invoke();
        }
    }
}
