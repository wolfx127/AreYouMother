using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Taffy.Play.Enemy
{
    public enum EnemyBehavior
    {
        None,
        CloseAttack,
        RemoteAttack,
        Poison,
        SecondPhase,
    }

    public enum MoveState
    {
        Idle,
        Walk,
    }

    public struct EnemyBehavior_value
    {
        public EnemyBehavior type;
        public int value;
    }
    
    public struct IsPursueComp : IComponentData
    {
        public bool value;
    }

    public struct PursueDirComp : IComponentData
    {
        public float x;
        public float z;
    }

    public struct CanAttackComp : IComponentData
    {
        public bool value;
    }

    public struct DefendComp : IComponentData
    {
        public int DEF;
    }

    public struct PoisonComp : IComponentData
    {
        public int ATK;
    }

    public struct SecondPhaseComp : IComponentData
    {
        public int ATK;
    }

    public struct HealthComp : IComponentData
    {
        public int Value;
    }

    public struct InjuryComp : IComponentData
    {
        public int injury;
    }

    public struct IdleTimeComp : IComponentData
    {
        public float time;
    }

    public struct WalkTimeComp : IComponentData
    {
        public float time;
    }

    public struct WalkSpeedComp : IComponentData
    {
        public float speed;
    }

    public struct PursueSpeedComp : IComponentData
    {
        public float speed;
    }

    public struct MoveSpeedComp : IComponentData
    {
        public float speed;
    }

    public struct MoveStateComp : IComponentData
    {
        public MoveState state;
    }
    
    public struct AttackSpeedComp : IComponentData
    {
        public float speed;
    }

    public class EnemyComp : IComponentData
    {
        public EnemyData data;
    }

    public struct DeadComp : IComponentData
    {
        public bool isDead;
    }

    public struct DirectionComp : IComponentData
    {
        public float x;
        public float y;
    }

    public struct WalkTimeCounterComp : IComponentData
    {
        public float residualTime;
    }

    public struct IdleTimeCounterComp : IComponentData
    {
        public float residualTime;
    }

    public struct ReturnDirectionComp : IComponentData
    {
        public float x;
        public float y;
    }
}
