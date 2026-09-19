using System;
using System.Collections.Generic;
using UnityEngine;

namespace Taffy.Play.Enemy
{
    public class EnemyCreator : MonoBehaviour
    {
        public List<GameObject> enemyCreatPlaceList = new List<GameObject>();

        private void Awake()
        {
            foreach (Transform child in transform)
            {
                enemyCreatPlaceList.Add(child.gameObject);
            }
        }
    }
}
