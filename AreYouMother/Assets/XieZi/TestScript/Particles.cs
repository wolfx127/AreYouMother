using System;
using Unity.VisualScripting;
using UnityEngine;

public class Particles : MonoBehaviour
{
    [SerializeField] private Vector3 nowPosition;
    public Vector3 toPosition;
    
    public bool isChangePosition;
    
    private Transform transform;
    public ParticleSystem particleStart;
    public ParticleSystem particleTo;
    
    private ParticleSystem.MainModule _mainModuleStart;
    private ParticleSystem.MainModule _mainModuleTo;
    

    private void OnEnable()
    {
        transform = GetComponent<Transform>();
        nowPosition = transform.position;
        
        // 避免重复获取模块
        _mainModuleStart = particleStart.main;
        _mainModuleStart.playOnAwake = false;
        
        _mainModuleTo = particleTo.main;
        _mainModuleTo.playOnAwake = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = toPosition;
        if (nowPosition != toPosition)
        {
            isChangePosition = true;
            particleStart.transform.position = nowPosition;
            particleStart.Play();
            particleTo.Play();
            Debug.Log("交换位置");
        }
        else
        {
            isChangePosition = false;
        }
        
        nowPosition = toPosition;
    }
}
