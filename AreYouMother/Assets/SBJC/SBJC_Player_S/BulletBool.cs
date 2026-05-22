using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] int poolSize = 20;
    private Queue<GameObject> pool = new();
    void Awake()
    {
        Instance = this;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject b = Instantiate(bulletPrefab);
            b.SetActive(false);
            pool.Enqueue(b);
        }
    }
    public GameObject Get()
    {
        GameObject b = pool.Dequeue();
        b.SetActive(true);
        pool.Enqueue(b);
        return b;
    }
}