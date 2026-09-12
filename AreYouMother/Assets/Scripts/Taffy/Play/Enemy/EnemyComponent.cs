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
        Pursue,
    }

    public struct EnemyBehavior_value
    {
        public EnemyBehavior type;
        public int value;
    }

    public struct CloseAttackComp : IComponentData
    {
        public int ATK;
    }

    public struct RemoteAttackComp : IComponentData
    {
        public int ATK;
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

    public struct AttackRadiusComp : IComponentData
    {
        public float radius;
    }

    public struct PursueTargetComp : IComponentData
    {
        public Char player;
    }

    public struct EnemyTag : IComponentData { }
    public struct DeadTag : IComponentData { }

    public struct DirectionComp : IComponentData
    {
        public bool x;
        public bool y;
    }

    public struct TimeCounterComp : IComponentData
    {
        public float residualTime;
    }

}
