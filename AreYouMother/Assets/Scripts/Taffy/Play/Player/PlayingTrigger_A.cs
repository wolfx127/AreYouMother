using System;
using System.Collections.Generic;
using SBJC.SBJC_Player_S;
using Taffy.Data;
using Taffy.OverAllManager;
using Taffy.Play.Container;
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

        // ===== PlayerA 远程：从共用 BulletPool 发射 Bullet，子弹仅命中 EnemyB =====
        [SerializeField] private Transform firePoint;   // 发射点（留空则用本物体位置+半身高）

        /// <summary>
        /// 朝 facing 方向发射一颗子弹（玩家A 发射，子弹只打 EnemyB）。
        /// 伤害由 Bullet 内部按玩家A攻击力结算。
        /// </summary>
        /// <param name="facing">玩家当前移动朝向（已在水平面，无需归一化）</param>
        public void GetAttackEnemies(Vector3 facing)
        {
            int atk = PlayerCurrentStateController.Instance != null
                ? PlayerCurrentStateController.Instance.GetAtk_A()
                : 0;

            if (atk <= 0)
            {
                Debug.LogWarning("[A攻击] 攻击力为 0（可能没装备武器），不发射子弹");
                return;
            }

            if (BulletPool.Instance == null)
            {
                Debug.LogWarning("[A攻击] 场景中没有 BulletPool，无法发射");
                return;
            }

            // 朝向在水平面上归一化；为零时退化为当前面朝方向
            facing.y = 0;
            Vector3 dir = facing.sqrMagnitude > 0.0001f ? facing.normalized : transform.forward;

            // 发射位置：水平用 firePoint 或本物体（已在角色前方），Y 用角色半身高
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            float halfHeight = GetPlayerHalfHeight();
            spawnPos.y = (transform.parent != null ? transform.parent.position.y : transform.position.y) + halfHeight;

            GameObject bulletGo = BulletPool.Instance.Get();
            if (bulletGo == null) return;

            bulletGo.transform.position = spawnPos;
            bulletGo.transform.rotation = Quaternion.LookRotation(dir);
            bulletGo.GetComponent<Bullet>().Launch(dir, false, 0, transform.parent);

            Debug.Log("[A攻击] 发射子弹");
        }
        // ===== PlayerA 远程结束 =====

        /// <summary>从父级玩家 BoxCollider 获取中心高度（bounds.center 正确处理 offset）</summary>
        private float GetPlayerHalfHeight()
        {
            if (transform.parent != null)
            {
                var col = transform.parent.GetComponent<BoxCollider>();
                if (col != null) return col.bounds.center.y - transform.parent.position.y;
            }
            return 1f;
        }
    }
}
