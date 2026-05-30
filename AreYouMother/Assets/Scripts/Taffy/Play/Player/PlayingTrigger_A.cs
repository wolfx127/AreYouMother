using System;
using System.Collections.Generic;
using Taffy.Data;
using Taffy.OverAllManager;
using UnityEngine;

namespace Taffy.Play.Player
{
    public class PlayingTrigger_A:MonoBehaviour
    {
        public bool disableOpenContainer = true;
        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Container"))
            {
                disableOpenContainer = false;
                EventBus.Publish(new GiveContainer_AEvent(other.gameObject.GetComponent<ContainerData>()));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.CompareTag("Container"))
            {
                disableOpenContainer = true;
                EventBus.Publish(new GiveContainer_AEvent(null));
            }
        }

        // ===== 临时测试代码：PlayerA 近战，仅能攻击 EnemyB，后续整块删除 =====
        [SerializeField] private float attackRadius = 1.8f;   // 攻击半圆的半径（圆心是玩家）
        private Vector3 _attackFacing = Vector3.forward;      // 最近一次攻击朝向，供 Gizmos 绘制

        /// <summary>
        /// 以玩家为圆心、facing 方向的前方半圆（180°）为攻击范围，仅命中 EnemyB。
        /// </summary>
        /// <param name="facing">玩家当前移动朝向（已在水平面，无需归一化）</param>
        public void GetAttackEnemies(Vector3 facing)
        {
            int atk = PlayerCurrentStateController.Instance != null
                ? PlayerCurrentStateController.Instance.GetAtk_A()
                : 0;

            if (atk <= 0)
            {
                Debug.LogWarning("[A攻击] 攻击力为 0（可能没装备武器），无法造成伤害");
                return;
            }

            // 朝向在水平面上归一化；为零时退化为当前面朝方向，避免半圆没有方向
            facing.y = 0;
            _attackFacing = facing.sqrMagnitude > 0.0001f ? facing.normalized : transform.forward;

            Vector3 center = transform.parent != null ? transform.parent.position : transform.position;
            Collider[] hits = Physics.OverlapSphere(center, attackRadius);

            int damaged = 0;
            var hitEnemies = new HashSet<EnemyB>();   // 去重：一个敌人可能有多个 Collider
            foreach (var hit in hits)
            {
                if (!hit.CompareTag("Enemy")) continue;

                // 只打前方半圆：敌人方向与朝向夹角 ≤ 90°（点积 ≥ 0）
                Vector3 toEnemy = hit.transform.position - center;
                toEnemy.y = 0;
                if (Vector3.Dot(_attackFacing, toEnemy) < 0f) continue;

                // 仅 EnemyB 类型可被攻击（不碰 EnemyA）
                var enemy = hit.GetComponentInParent<EnemyB>();
                if (enemy == null || hitEnemies.Contains(enemy)) continue;
                hitEnemies.Add(enemy);
                enemy.TakeDamage(atk);
                damaged++;
            }

            Debug.Log($"[A攻击] 攻击力={atk}，命中 EnemyB 数={damaged}");
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = transform.parent != null ? transform.parent.position : transform.position;
            Vector3 facing = _attackFacing.sqrMagnitude > 0.0001f ? _attackFacing.normalized : transform.forward;

            Gizmos.color = Color.cyan;
            Vector3 prev = center + facing * attackRadius;
            for (int i = 1; i <= 18; i++)
            {
                float angle = -90f + (180f / 18f) * i;     // -90° → +90°
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * facing;
                Vector3 cur = center + dir * attackRadius;
                Gizmos.DrawLine(prev, cur);
                prev = cur;
            }
            Gizmos.DrawLine(center, center + Quaternion.AngleAxis(-90f, Vector3.up) * facing * attackRadius);
            Gizmos.DrawLine(center, center + Quaternion.AngleAxis( 90f, Vector3.up) * facing * attackRadius);
        }
        // ===== 临时测试代码结束 =====
    }
}
