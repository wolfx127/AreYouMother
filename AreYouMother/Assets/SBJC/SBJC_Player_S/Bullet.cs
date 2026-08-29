using Taffy.Data;
using Taffy.Data.PropData;
using Taffy.Play.Player;
using UnityEngine;

namespace SBJC.SBJC_Player_S
{
    /// <summary>
    /// 子弹（玩家A 与 敌人 共用同一对象池）。
    /// 靠 isEnemyLaunched 区分命中规则：
    ///   false = 玩家A 发射，仅命中 EnemyB；
    ///   true  = 敌人发射，命中所有玩家（死亡玩家穿过）。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] float speed = 15f;
        [SerializeField] float maxDistance = 50f;   // 超过此飞行距离自动回收，避免无限飞

        [Header("子弹精灵")]
        [SerializeField] private Sprite playerBulletSprite;   // 玩家A发射时使用
        [SerializeField] private Sprite enemyBulletSprite;    // 敌人发射时使用

        private Rigidbody rb;
        private SpriteRenderer _spriteRenderer;
        private int _damage;
        private bool _isEnemyLaunched;
        private bool _recycled;       // 防止同一帧多次触发导致重复回收
        private Vector3 _startPos;
        private Transform _firer;     // 发射者，用于跳过自己的碰撞体

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// 发射子弹。
        /// </summary>
        /// <param name="dir">飞行方向（无需归一化，会在水平面上归一化）</param>
        /// <param name="isEnemyLaunched">true=敌人发射(打玩家)，false=玩家A发射(只打EnemyB)</param>
        /// <param name="enemyDamage">敌人发射时的伤害；玩家发射时忽略(自动取玩家A攻击力)</param>
        public void Launch(Vector3 dir, bool isEnemyLaunched, int enemyDamage = 0, Transform firer = null)
        {
            _isEnemyLaunched = isEnemyLaunched;
            _firer = firer;
            _recycled = false;
            _startPos = transform.position;

            // 根据发射者切换精灵图
            if (_spriteRenderer != null)
                _spriteRenderer.sprite = isEnemyLaunched ? enemyBulletSprite : playerBulletSprite;

            if (isEnemyLaunched)
            {
                _damage = enemyDamage;
            }
            else
            {
                // 玩家A 发射：伤害取玩家A当前攻击力
                _damage = PlayerCurrentStateController.Instance != null
                    ? PlayerCurrentStateController.Instance.GetAtk_A()
                    : 0;
            }

            Vector3 d = dir;
            d.y = 0;
            d = d.sqrMagnitude > 0.0001f ? d.normalized : transform.forward;
            transform.rotation = Quaternion.LookRotation(d);
            rb.linearVelocity = d * speed;

            gameObject.SetActive(true);
        }

        void FixedUpdate()
        {
            if (_recycled) return;

            // 超出最大飞行距离 → 回收
            if ((transform.position - _startPos).sqrMagnitude >= maxDistance * maxDistance)
            {
                Recycle();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (_recycled) return;

            // 跳过发射者自身的碰撞体，避免子弹立刻撞到自己
            if (_firer != null && (other.transform == _firer || other.transform.IsChildOf(_firer)))
                return;

            if (_isEnemyLaunched)
            {
                // 敌人子弹：打所有玩家
                if (other.GetComponent<PlayingHandler_A>() != null) { TryHitPlayer(PropOwner.A); return; }
                if (other.GetComponent<PlayingHandler_B>() != null) { TryHitPlayer(PropOwner.B); return; }
                // 打到障碍物 → 回收
                Recycle();
            }
            else
            {
                // 玩家A子弹：仅命中 EnemyB
                if (!other.CompareTag("Enemy")) { Recycle(); return; }
                var enemy = other.GetComponentInParent<EnemyB>();
                if (enemy == null) { Recycle(); return; }   // EnemyA 等视为障碍物
                enemy.TakeDamage(_damage);
                Recycle();
            }
        }

        private void TryHitPlayer(PropOwner owner)
        {
            var ctrl = PlayerCurrentStateController.Instance;
            if (ctrl == null) return;

            // 死亡玩家不被攻击，子弹穿过继续飞
            if (ctrl.GetIsDead(owner)) return;

            if (owner == PropOwner.A) ctrl.Injury_A(_damage);
            else if (owner == PropOwner.B) ctrl.Injury_B(_damage);

            Recycle();
        }

        private void Recycle()
        {
            _recycled = true;
            rb.linearVelocity = Vector3.zero;

            if (BulletPool.Instance != null) BulletPool.Instance.Back(gameObject);
            else gameObject.SetActive(false);
        }

        /// <summary>
        /// 对象池回收时调用：归零速度并失活。不覆盖 Inspector 配置的 speed/maxDistance。
        /// </summary>
        public void ResetBullet()
        {
            _recycled = true;
            if (rb == null) rb = GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            gameObject.SetActive(false);
        }
    }
}
