using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Taffy.Play.Enemy
{
    /// WalkSystem ///
#region WalkSystem
    
    [UpdateAfter(typeof(MoveStateSystem))]
    public partial struct WalkSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new WalkJob{deltaTime = SystemAPI.Time.DeltaTime}.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    [WithNone(typeof(DeadTag))]
    public partial struct WalkJob : IJobEntity
    {
        public float deltaTime;
        public void Execute(
            ref TimeCounterComp timeCounter,
            in WalkTimeComp time,
            ref MoveStateComp state,
            in DirectionComp dir,
            in MoveSpeedComp speed,
            ref LocalTransform lt)
        {
            if (state.state == MoveState.Walk)
            {
                if(dir.x) lt.Position.x += speed.speed * deltaTime;
                else lt.Position.x -= speed.speed*deltaTime;
                if(dir.y) lt.Position.y += speed.speed*deltaTime;
                else lt.Position.y -= speed.speed*deltaTime;
                timeCounter.residualTime += deltaTime;
                if (timeCounter.residualTime >= time.time)
                {
                    state.state = MoveState.Idle;
                    timeCounter.residualTime = 0f;
                }
            }
            else timeCounter.residualTime = 0f;
        }
    }

    #endregion
    
    /// MoveStateSystem ///
#region MoveStateSystem
    public partial struct MoveStateSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new MoveStateJob().ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    [WithNone(typeof(DeadTag))]
    public partial struct MoveStateJob : IJobEntity
    {
        public void Execute(
            in MoveStateComp state,
            in WalkSpeedComp walk,
            in PursueSpeedComp pursue,
            ref MoveSpeedComp speed)
        {
            switch (state.state)
            {
                case MoveState.Walk:
                    speed.speed = walk.speed;
                    break;
                case MoveState.Pursue:
                    speed.speed = pursue.speed;
                    break;
                default:
                    speed.speed = 0f;
                    break;
            }
        }
    }
    #endregion

    /// IdleSystem ///
#region IdleSystem

    [UpdateAfter(typeof(MoveStateSystem))]
    public partial struct IdleSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new IdleJob {deltaTime = SystemAPI.Time.DeltaTime}.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    [WithNone(typeof(DeadTag))]
    public partial struct IdleJob : IJobEntity
    {
        public float deltaTime;
        public void Execute(
            in IdleTimeComp sumTime,
            ref TimeCounterComp timeCounter,
            ref MoveStateComp state)
        {
            if(state.state == MoveState.Idle)
            {
                timeCounter.residualTime += deltaTime;
                if (sumTime.time <= timeCounter.residualTime)
                {
                    state.state = MoveState.Walk;
                    timeCounter.residualTime = 0f;
                }
            }
            else timeCounter.residualTime = 0f;
        }
    }

    #endregion

    /// PursueSystem ///
#region PursueSystem

    [UpdateAfter(typeof(MoveStateSystem))]
    public partial struct PursueSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new PursueJob().ScheduleParallel(state.Dependency);
        }
    }

    public partial struct PursueJob : IJobEntity
    {
        public void Execute(
            in PursueSpeedComp speed, 
            MoveStateComp state
        )
        {
            if (state.state == MoveState.Pursue)
            {
                
            }
            else
            {
                
            }
        }
    }

    #endregion

    /// InjurySystem ///
#region InjurySystem

    public partial struct InjurySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        { 
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            state.Dependency = new InjuryJob{Ecb = ecb.AsParallelWriter()}.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    [WithNone(typeof(DeadTag))]
    public partial struct InjuryJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public void Execute(
            Entity e,
            [EntityIndexInQuery] int sortKey,
            ref HealthComp hp,
            ref InjuryComp injury)
        {
            hp.Value -= injury.injury;
            injury.injury = 0;
            if (hp.Value <= 0)
            {
                Ecb.AddComponent<DeadTag>(sortKey, e);
            }
        }
    }

    #endregion

    /// DeadSystem ///

    #region DeadSystem

    public partial struct DeadSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            state.Dependency = new DeadJob { Ecb = ecb.AsParallelWriter() }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct DeadJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public void Execute(
            Entity e,
            [EntityIndexInQuery] int sortKey,
            in DeadTag dead)
        {
            Ecb.DestroyEntity(sortKey, e);
        }
    }

    #endregion

    /// Movesystem ///
#region MoveSystem
    
    #endregion

    /// Movesystem ///
#region MoveSystem
    
    #endregion

    /// Movesystem ///
#region MoveSystem
    
    #endregion

    /// Movesystem ///
#region MoveSystem
    
    #endregion

}
