using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Taffy.Play.Enemy
{
    public static class EnemyTool
    {
        public static void LinkEntity(this EnemyData enemy,Vector3 position)
        {
            if (World.DefaultGameObjectInjectionWorld == null) return;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity e = em.CreateEntity();

            em.AddComponentData(e, new EnemyComp{ data = enemy });
            em.AddComponentData(e, LocalTransform.FromPositionRotation(position, enemy.transform.rotation));
            em.AddComponentData(e, new LocalToWorld
            {
                Value = float4x4.TRS(position, enemy.transform.rotation, enemy.transform.lossyScale)
            });
            enemy.transform.position = position;
            
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

            Vector2 v = RandomDirection();
            em.AddComponentData(e, new ReturnDirectionComp { x = 0f, y = 0f });
            em.AddComponentData(e, new DirectionComp{x = v.x, y = v.y});
            em.AddComponentData(e, new DeadComp{isDead = false});
            em.AddComponentData(e, new AttackSpeedComp { speed = enemy.AttackSpeed });
            em.AddComponentData(e, new AttackRadiusComp { radius = enemy.AttackRadius });
            em.AddComponentData(e, new DefendComp { DEF = enemy.DEF });
            em.AddComponentData(e, new WalkSpeedComp { speed = enemy.walkSpeed });
            em.AddComponentData(e, new PursueSpeedComp { speed = enemy.pursueSpeed });
            em.AddComponentData(e, new IdleTimeComp{ time = enemy.idleTime });
            em.AddComponentData(e, new IdleTimeCounterComp {residualTime = 0});
            em.AddComponentData(e, new WalkTimeComp{time = enemy.walkTime });
            em.AddComponentData(e, new WalkTimeCounterComp {residualTime = 0});
            em.AddComponentData(e, new MoveStateComp { state = MoveState.Idle });
            em.AddComponentData(e, new MoveSpeedComp { speed = enemy.walkSpeed });
            em.AddComponentData(e, new IsPursueComp { value = false });

            em.SetName(e,enemy.name);
            
            enemy.entity = e;
            Debug.Log("敌人连接ecs成功");
        }

        private static Vector2 RandomDirection()
        {
            Vector2 v = new Vector2();
            float r = UnityEngine.Random.Range(0, 4);
            if (r > 3) v.x = 1;
            else if (r > 2) v.x = -1;
            else if (r > 1) v.y = 1;
            else v.y = -1;
            return v;
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

        public static void SortByWeight(this List<EnemyCreatProbability> list)
        {
            SortByWeight(list,0,list.Count-1);
        }

        private static void SortByWeight(List<EnemyCreatProbability> list, int left, int right)
        {
            if (right - left <= 0) return;

            float signal = list[left].Weight;
            int leftPivot = left;
            int rightPivot = right;
            while (leftPivot <= rightPivot)
            {
                while (list[leftPivot].Weight < signal) leftPivot++;
                while (list[rightPivot].Weight > signal) rightPivot--;
                if (leftPivot < rightPivot)
                {
                    (list[leftPivot], list[rightPivot]) = (list[rightPivot], list[leftPivot]);
                    leftPivot++;
                    rightPivot--;
                }
                else break;
            }
            
            SortByWeight(list,left,rightPivot);
            SortByWeight(list,rightPivot+1,right);
        }
    }
}
