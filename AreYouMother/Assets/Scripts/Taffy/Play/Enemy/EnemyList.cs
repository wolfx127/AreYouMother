using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Taffy.Play.Enemy
{
    public static class EnemyList
    {
        private static readonly Dictionary<string, GameObject> enemyTable = new Dictionary<string, GameObject>();


        static EnemyList()
        {
            Build();
        }

        private static void Build()
        {
            var handle = Addressables.LoadAssetsAsync<GameObject>("EnemyPrefab");
            handle.WaitForCompletion();
            foreach (var go in handle.Result)
            {
                if (go.GetComponent<EnemyData>() != null)
                {
                    enemyTable.TryAdd(go.name, go);
                }
            }
        }

        public static GameObject GetEnemyByName(string name)
        {
            GameObject target = null;
            if (enemyTable.TryGetValue(name, out GameObject go))
            {
                target = GameObject.Instantiate(go);
            }

            return target;
        }

        public static void CopyInfosTo(string name, GameObject target)
        {
            if (enemyTable.TryGetValue(name, out GameObject go))
            {
                go.GetComponent<EnemyData>().CopyInfosTo(target.GetComponent<EnemyData>());
                CopyVisual(go, target);

                return;
            }

            Debug.Log(name + "字段复制失败");
        }

        /// <summary>
        /// 把源预制体的 Sprite 和 Animator 表现拷到目标实例上
        /// </summary>
        private static void CopyVisual(GameObject source, GameObject target)
        {
            SpriteRenderer srcSprite = source.GetComponent<SpriteRenderer>();
            SpriteRenderer dstSprite = target.GetComponent<SpriteRenderer>();
            if (srcSprite != null && dstSprite != null)
            {
                dstSprite.sprite = srcSprite.sprite;
                dstSprite.size = srcSprite.size;
                dstSprite.color = srcSprite.color;
                dstSprite.flipX = srcSprite.flipX;
                dstSprite.sortingOrder = srcSprite.sortingOrder;
            }

            Animator srcAnimator = source.GetComponent<Animator>();
            if (srcAnimator != null)
            {
                Animator dstAnimator = target.GetComponent<Animator>();
                if (dstAnimator == null) dstAnimator = target.AddComponent<Animator>();
                dstAnimator.runtimeAnimatorController = srcAnimator.runtimeAnimatorController;
                dstAnimator.avatar = srcAnimator.avatar;
                dstAnimator.updateMode = srcAnimator.updateMode;
                dstAnimator.cullingMode = srcAnimator.cullingMode;
            }

            CapsuleCollider srcCol = source.GetComponent<CapsuleCollider>();
            CapsuleCollider dstCol = target.GetComponent<CapsuleCollider>();
            if (srcCol != null && dstCol != null)
            {
                dstCol.radius = srcCol.radius;
                dstCol.height = srcCol.height;
                dstCol.center = srcCol.center;
                dstCol.direction = srcCol.direction;
                dstCol.isTrigger = srcCol.isTrigger;
            }
        }
    }
}
