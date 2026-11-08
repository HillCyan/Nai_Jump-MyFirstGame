using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    [SerializeField] GameObject[] FloorPrefabs;
    [SerializeField] float minSpacing = 0.9f;   // 新地板/尖刺之间的最小水平间距

    const float SpawnY = -5.7f;

    // Start is called before the first frame update
    /*void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */
    public void SpawnFloor()
    {
        // 概率：下标 0 = 50%，下标 1 = 30%，下标 2 = 20%（降低 JianCi 出现率）
        int roll = Random.Range(0, 100);   // 0~99
        int r;
        if (roll < 50)
            r = 0;
        else if (roll < 80)
            r = 1;
        else
            r = 2;

        // 兜底：万一数组元素少于 3 个，退回均匀随机
        if (r >= FloorPrefabs.Length)
            r = Random.Range(0, FloorPrefabs.Length);

        // 找一个不会和已有地板/尖刺重叠的位置（最多试 20 次）
        float x = Random.Range(-4.2f, 4.2f);
        bool found = false;
        float bestX = x;
        float bestDist = -1f;

        for (int attempt = 0; attempt < 20; attempt++)
        {
            float candidate = Random.Range(-4.2f, 4.2f);
            float dist = NearestFloorDistance(candidate);

            if (dist >= minSpacing)
            {
                x = candidate;
                found = true;
                break;
            }

            // 记录“最不挤”的位置，实在找不到空位时用它
            if (dist > bestDist)
            {
                bestDist = dist;
                bestX = candidate;
            }
        }

        // 找不到空位也照放：用 20 次里最不挤的位置，保证地板不断档
        if (!found)
            x = bestX;

        GameObject floor = Instantiate(FloorPrefabs[r], transform);
        floor.transform.position = new Vector3(x, SpawnY, 0f);
    }

    // 这个 x 到出生高度附近最近的地板/尖刺的距离（附近没东西时返回极大值）
    float NearestFloorDistance(float x)
    {
        float nearest = float.MaxValue;
        foreach (Transform child in transform)
        {
            // 只算地板/尖刺，且只算还在出生高度附近的
            if (child.GetComponent<FloorMoveUp>() == null) continue;
            if (Mathf.Abs(child.position.y - SpawnY) > 1.2f) continue;

            float d = Mathf.Abs(child.position.x - x);
            if (d < nearest)
                nearest = d;
        }
        return nearest;
    }
}
