using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.Entities;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Taffy.Play.Enemy
{
    public class EnemyData : MonoBehaviour
    {
        private World world;
        private EntityManager em;
        public Entity entity = Entity.Null;
        public SpriteRenderer sprite;
        public Collider collider;
        
        [Header("敌人名__要保证唯一")]public string name = "";
        [Header("血量")]public int HP = 0;
        [Header("类型列表")]public List<EnemyBehavior_value> Behavior = new List<EnemyBehavior_value>();
        [Header("攻速")]public float AttackSpeed = 0;
        [Header("攻击半径")]public float AttackRadius = 0;
        [Header("攻击力")]public int ATK = 0;
        [Header("防御力")]public int DEF = 0;
        
        public MoveState state = MoveState.Idle;
        
        [Header("游荡速度")]public float walkSpeed = 0;
        [Header("游荡最远时间")]public float walkTime = 0;
        [Header("追击速度")]public float pursueSpeed = 0;
        [Header("停留时间")]public float idleTime = 0;

        public float dirX
        {
            set
            {
                if (!sprite) return;
                if (sprite.flipX != value < 0)
                {
                    sprite.flipX = value < 0;
                }
            }
        }

        //给敌人死亡上锁，因为mono这边不知道dots那死没死，只知道dots是不是把e销毁了
        private bool isDead = false;
        public bool dead
        {
            get => isDead;
            set
            { 
                isDead = value;
                Dead();
            }
        }
        public bool isAlive = false;
        public float deadAnimTime = 0.3f;
        public float deadAnimTimeCounter = 0.3f;

        public event Action DeadEvent;

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
            e.walkTime = walkTime;
        }

        private void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            collider = gameObject.GetComponent<Collider>();
            collider.enabled = true;
            world =  World.DefaultGameObjectInjectionWorld;
            if(world is not null) em = world.EntityManager;
        }

        private void OnEnable()
        {
            isAlive = true;
            isDead = false;
            deadAnimTimeCounter = deadAnimTime;
        }

        private void Update()
        {
            if (world is null || entity == Entity.Null) return;
            if(!em.Exists(entity)) dead = true;
        }

        private void OnDisable()
        {
            isAlive = false;
            isDead = true;
            DeadEvent = null;
        }
        
        private void Dead()
        {
            if (isAlive && isDead)
            {
                if(deadAnimTimeCounter >= deadAnimTime) collider.enabled = false;
                deadAnimTimeCounter -= Time.deltaTime;
                if (deadAnimTimeCounter <= 0)
                {
                    DeadEvent?.Invoke();
                    EnemyPool.Instance.RecycleEnemy(gameObject);
                }
            }
        }

        public void Injury(int ATK, Vector2 position)
        {
            if (isDead || entity == Entity.Null || world is null) return;
            int atk = em.GetComponentData<InjuryComp>(entity).injury + ATK;
            em.SetComponentData(entity,new InjuryComp{injury = atk});
            em.SetComponentData(entity,new ReturnDirectionComp {x = position.x,y = position.y});
        }
    }
}