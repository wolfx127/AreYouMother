using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

namespace Taffy.Play.Enemy
{
    public class EnemyData : MonoBehaviour
    {
        public Entity entity = Entity.Null;
        public SpriteRenderer sprite;
        
        [Header("敌人名__要保证唯一")]public string name = "";
        [Header("血量")]public int HP = 0;
        [Header("类型列表")]public List<EnemyBehavior_value> Behavior = new List<EnemyBehavior_value>();
        [Header("攻速")]public float AttackSpeed = 0;
        [Header("攻击半径")]public float AttackRadius = 0;
        [Header("攻击力")]public int ATK = 0;
        [Header("防御力")]public int DEF = 0;
        
        public bool isPursue = false;
        public bool isWalk = false;
        public bool isIdle = false;
        
        [Header("游荡速度")]public float walkSpeed = 0;
        [Header("游荡最远时间")]public float walkTime = 0;
        [Header("追击速度")]public float pursueSpeed = 0;
        [Header("停留时间")]public float idleTime = 0;

        public void CopyInfosTo(EnemyData e)
        {
            e.name = name;
            e.HP = HP;
            e.Behavior.Clear();
            e.Behavior.AddRange(Behavior);
            e.AttackSpeed = AttackSpeed;
            e.AttackRadius = AttackRadius;
            e.ATK = ATK;
            e.DEF = DEF;
            e.walkSpeed = walkSpeed;
            e.pursueSpeed = pursueSpeed;
            e.idleTime = idleTime;
        }

        private void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            
        }
        
        private void Update()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null|| entity == Entity.Null) return;
            var em = world.EntityManager;
            if (!em.Exists(entity)) return;
            var dir = em.GetComponentData<DirectionComp>(entity);
            if (sprite)
            {
                sprite.flipX = dir.x;
            }
        }

        public void Pursue()
        {
            
        }

        private readonly Dictionary<EnemyBehavior, float> BehaviorTable = new Dictionary<EnemyBehavior, float>();
        
    }
}