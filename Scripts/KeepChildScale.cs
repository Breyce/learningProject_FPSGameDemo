using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepChildScale : MonoBehaviour
{
    private Vector3 initialScale;

    void Start()
    {
        // 保存子物体的初始缩放
        initialScale = transform.localScale;
    }

    void Update()
    {
        // 获取父物体的缩放
        Vector3 parentScale = transform.parent.localScale;

        // 保持子物体相对缩放不变
        transform.localScale = new Vector3(
            initialScale.x / parentScale.x,
            initialScale.y / parentScale.y,
            initialScale.z / parentScale.z
        );
    }
}

