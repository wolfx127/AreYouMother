using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TaffyFrame.Trigger
{
    /// <summary>
    /// 调用前按需在调用类里添加标签防御，防止其他标签的collider进入victims
    /// ...子类实现一个封装了GetVictimList的方法，用于按需进一步筛选
    /// </summary>
    public abstract class BaseTrigger : MonoBehaviour
    {
        private Collider Box;
        
        private readonly Dictionary<int,GameObject> victimTable = new Dictionary<int,GameObject>();
        
        [SerializeField]private readonly List<GameObject> victimList = new List<GameObject>();
        
        private void Awake()
        { 
            Box = GetComponent<Collider>();
            Box.isTrigger = true;
            CloseTrigger();
        }

        private void OnTriggerEnter(Collider other)
        {
            if(!victimTable.ContainsKey(other.gameObject.GetInstanceID()) && isObjectlegal(other.gameObject)) 
                victimTable[other.gameObject.GetInstanceID()] = other.gameObject;
        }

        private void OnTriggerExit(Collider other)
        {
            victimTable.Remove(other.gameObject.GetInstanceID());
        }

        /// <summary>
        /// 检测这个游戏对象应不应该在victims里，如果不是并依旧存在直接就移除了
        /// </summary>
        /// <returns></returns>
        private bool isObjectlegal(GameObject GO)
        {
            return GO.activeInHierarchy;
        }
        
        protected List<GameObject> GetVictimList()
        {
            victimList.Clear();
            List<int> illegalKey = new List<int>();
            foreach (int id in victimTable.Keys)
            {
                if(isObjectlegal(victimTable[id])) victimList.Add(victimTable[id]);
                else illegalKey.Add(id);
            }

            foreach (int id in illegalKey)
            {
                victimTable.Remove(id);
            }

            return victimList;
        }

        public void OpenTrigger()
        {
            victimTable.Clear();
            victimList.Clear();
            Box.enabled = true;
        }

        public void CloseTrigger()
        {
            victimTable.Clear();
            victimList.Clear();
            Box.enabled = false;
        }

        /// <summary>
        /// 做进一步筛选，没有筛选必要的话，就直接把GetVictimList()填进去
        /// </summary>
        /// <returns></returns>
        public abstract List<GameObject> GetVictims();
    }
}
