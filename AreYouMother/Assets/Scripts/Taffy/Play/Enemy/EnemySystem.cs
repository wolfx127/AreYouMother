using System;
using TaffyFrame.EventBus;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Taffy.Play.Enemy
{
    /// MoveSystem ///
#region MoveSystem
    
    public partial struct MoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new MoveJob{deltaTime = SystemAPI.Time.DeltaTime}.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        public float deltaTime;
        public void Execute(
            ref WalkTimeCounterComp walkTimeCounter,
            in WalkTimeComp walkTime,
            ref IdleTimeCounterComp idleTimeCounter,
            in IdleTimeComp idleTime,
            ref MoveStateComp state,
            ref DirectionComp dir,
            in MoveSpeedComp speed,
            ref LocalTransform lt,
            in IsPursueComp isPursue)
        {
            if (isPursue.value) return;
            if (state.state == MoveState.Walk)
            {
                lt.Position.x += speed.speed * deltaTime * dir.x;
                lt.Position.z += speed.speed * deltaTime * dir.y;
                walkTimeCounter.residualTime += deltaTime;
                if (walkTimeCounter.residualTime >= walkTime.time)
                {
                    state.state = MoveState.Idle;
                    walkTimeCounter.residualTime = 0f;
                    dir.x = -dir.x;
                    dir.y = -dir.y;
                }
            }

            if (state.state == MoveState.Idle)
            {
                idleTimeCounter.residualTime += deltaTime;
                if (idleTimeCounter.residualTime >= idleTime.time)
                {
                    state.state = MoveState.Walk;
                    idleTimeCounter.residualTime = 0f;
                }
            }
        }
    }

    #endregion
    
    /// PursueSystem ///
#region PursueSystem

    public partial struct PursueSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new PursueJob().ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct PursueJob : IJobEntity
    {
        public void Execute(
            in PursueSpeedComp speed,
            in IsPursueComp isPursue,
            in PursueDirComp dir,
            ref LocalTransform transform,
            in CanAttackComp attack
        )
        {
            if (isPursue.value && !attack.value)
            {
                transform.Position += new float3(dir.x, 0, dir.z) * speed.speed;
            }
        }
    }

    #endregion

    /// InjurySystem ///
#region InjurySystem

    public partial struct InjurySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        { 
            state.Dependency = new InjuryJob().ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct InjuryJob : IJobEntity
    {
        public void Execute(
            ref HealthComp hp,
            ref InjuryComp injury,
            ref DeadComp dead,
            in ReturnDirectionComp dir,
            ref LocalTransform transform)
        {
            if(injury.injury != 0)
            {
                hp.Value -= injury.injury;
                injury.injury = 0;
                transform.Position += new float3(dir.x * 0.2f, 0, dir.y * 0.2f);
                if (hp.Value <= 0)
                {
                    dead.isDead = true;
                }
            }
        }
    }

    #endregion

    /// DeadSystem ///

    #region DeadSystem

    public partial struct DeadSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged)
                .AsParallelWriter();
            state.Dependency = new DeadJob{ECB = ecb}.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct DeadJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public void Execute(
            Entity e,
            [EntityIndexInQuery] int sortKey,
            in DeadComp dead)
        {
            if (dead.isDead)
            {
                ECB.DestroyEntity(sortKey, e);
            }
        }
    }

    #endregion

    /// LinkToMonoSystem ///

    #region LinkToMonoSystem
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct LinkToEnemyMonoSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (localTransform,moveStateComp,healthComp,directionComp,enemyComp) in SystemAPI.Query<
                         RefRO<LocalTransform>,
                         RefRO<MoveStateComp>,
                         RefRO<HealthComp>,
                         RefRO<DirectionComp>,
                         EnemyComp>())
            {
                if (enemyComp.data == null) continue;
                enemyComp.data.transform.position = new Vector3(localTransform.ValueRO.Position.x, enemyComp.data.transform.position.y, localTransform.ValueRO.Position.z);
                enemyComp.data.state = moveStateComp.ValueRO.state switch
                {
                    MoveState.Idle => MoveState.Idle,
                    MoveState.Walk => MoveState.Walk,
                    _ => enemyComp.data.state
                };
                enemyComp.data.dirX = directionComp.ValueRO.x;
                enemyComp.data.HP = healthComp.ValueRO.Value;
            }
        }
    }

    #endregion

    

    /// Movesystem ///
#region MoveSystem
    
    #endregion

    /// Movesystem ///
#region MoveSystem
    
    #endregion

}
