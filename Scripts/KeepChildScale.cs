using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepChildScale : MonoBehaviour
{
    private Vector3 initialScale;

    void Start()
    {
        // ����������ĳ�ʼ����
        initialScale = transform.localScale;
    }

    void Update()
    {
        // ��ȡ�����������
        Vector3 parentScale = transform.parent.localScale;

        // ����������������Ų���
        transform.localScale = new Vector3(
            initialScale.x / parentScale.x,
            initialScale.y / parentScale.y,
            initialScale.z / parentScale.z
        );
    }
}

