using UnityEngine;
using System.Collections.Generic;

public class SBJC_Camera_OcclusionHandler : MonoBehaviour
{
    public Transform player1;
    public Transform player2;
    public Material transparentMaterial;   // 确保这个材质手动拖到 Cube 上已经透明
    public LayerMask occlusionMask = ~0;

    private Dictionary<Renderer, Material[]> originalMats = new Dictionary<Renderer, Material[]>();
    private List<Renderer> occluded = new List<Renderer>();

    void Start()
    {
        occlusionMask &= ~(1 << LayerMask.NameToLayer("Player_Tag"));
    }

    void Update()
    {
        if (player1 == null || player2 == null) return;
        RestoreAll();
        CheckPlayer(player1);
        CheckPlayer(player2);
    }

    void CheckPlayer(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        float dist = dir.magnitude;

        RaycastHit[] hits = Physics.RaycastAll(transform.position, dir, dist, occlusionMask);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.isTrigger || hit.collider.transform == target) continue;
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == null) continue;

            // 如果是第一次遮挡，备份原始材质（引用）
            if (!originalMats.ContainsKey(rend))
                originalMats[rend] = rend.sharedMaterials;

            // 全部替换为透明材质的实例
            Material[] newMats = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < newMats.Length; i++)
            {
                newMats[i] = Instantiate(transparentMaterial); // 使用 Instantiate 更安全
            }
            rend.materials = newMats;

            if (!occluded.Contains(rend))
                occluded.Add(rend);
        }
    }

    void RestoreAll()
    {
        foreach (var rend in occluded)
        {
            if (originalMats.ContainsKey(rend))
                rend.materials = originalMats[rend];
        }
        occluded.Clear();
        originalMats.Clear();
    }
}