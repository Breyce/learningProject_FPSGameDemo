using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 工具类，由全局调用。
/// </summary>
public class WayPointManager : MonoBehaviour
{
    public static WayPointManager instance;
    
    //用两个List随机生成不同路线进行赋值，交给不同敌人
    public List<int> usingIndex = new List<int>(); // 每个敌人分配用到的路线ID
    public List<int> rawIndex = new List<int>();

    private void Awake()
    {
        instance = this;
        
        //随机分配路线
        int temp = rawIndex.Count;

        while (rawIndex.Count > 0)
        {
            int tempIndex = Random.Range(0, temp);
            usingIndex.Add(rawIndex[tempIndex]);
            rawIndex.RemoveAt(tempIndex);

            temp = rawIndex.Count;
        }
    }
}
