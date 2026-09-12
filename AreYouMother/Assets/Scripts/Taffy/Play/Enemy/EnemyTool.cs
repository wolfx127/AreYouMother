using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Taffy.Play.Enemy
{
    public static class EnemyTool
    {
        public static void LinkEntity(this EnemyData enemy)
        {
            if (World.DefaultGameObjectInjectionWorld == null) return;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity e = em.CreateEntity(typeof(EnemyTag));

            em.AddComponent<EnemyTag>(e);
            em.AddComponent<DirectionComp>(e);
            em.AddComponent<InjuryComp>(e);
            em.AddComponentData(e, new HealthComp { Value = enemy.HP });
            foreach (var b in enemy.Behavior)
            {
                switch (b.type)
                {
                    case EnemyBehavior.CloseAttack:
                        em.AddComponentData(e, new CloseAttackComp { ATK = b.value });
                        break;
                    case EnemyBehavior.RemoteAttack:
                        em.AddComponentData(e, new RemoteAttackComp { ATK = b.value });
                        break;
                    case EnemyBehavior.Poison:
                        em.AddComponentData(e, new PoisonComp { ATK = b.value });
                        break;
                    case EnemyBehavior.SecondPhase:
                        em.AddComponentData(e, new SecondPhaseComp { ATK = b.value });
                        break;
                }
            }

            em.AddComponentData(e, new AttackSpeedComp { speed = enemy.AttackSpeed });
            em.AddComponentData(e, new AttackRadiusComp { radius = enemy.AttackRadius });
            em.AddComponentData(e, new DefendComp { DEF = enemy.DEF });
            em.AddComponentData(e, new WalkSpeedComp { speed = enemy.walkSpeed });
            em.AddComponentData(e, new PursueSpeedComp { speed = enemy.pursueSpeed });
            em.AddComponentData(e, new IdleTimeComp{ time = enemy.idleTime });
            em.AddComponentData(e, new WalkTimeComp{time = enemy.walkTime });
            em.AddComponentData(e,new TimeCounterComp {residualTime = 0});
            em.AddComponentData(e, new MoveStateComp { state = MoveState.Idle });
            em.AddComponentData(e, new MoveSpeedComp { speed = enemy.walkSpeed });

            em.SetName(e,enemy.name);
            
            enemy.entity = e;
        }

        public static void UnlinkEntity(this EnemyData enemy)
        {
            if (World.DefaultGameObjectInjectionWorld == null) return; 
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity e = enemy.entity;
            if (e != Entity.Null && em.Exists(e))
            {
                em.DestroyEntity(e);
                enemy.entity = Entity.Null;
            }
        }
    }
}
