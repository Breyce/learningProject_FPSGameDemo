using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������չʵ�ֵ��˵Ļ���״̬
/// </summary>
public abstract class EnemyBaseState : MonoBehaviour
{
    public abstract void EnemyState(Enemy enemy); // �״ν���״̬

    public abstract void OnUpdate(Enemy enemy);// ����ִ��
}
