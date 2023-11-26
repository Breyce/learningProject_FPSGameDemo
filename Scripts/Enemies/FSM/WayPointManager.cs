using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �����࣬��ȫ�ֵ��á�
/// </summary>
public class WayPointManager : MonoBehaviour
{
    public static WayPointManager instance;
    
    //������List������ɲ�ͬ·�߽��и�ֵ��������ͬ����
    public List<int> usingIndex = new List<int>(); // ÿ�����˷����õ���·��ID
    public List<int> rawIndex = new List<int>();

    private void Awake()
    {
        instance = this;
        
        //�������·��
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
