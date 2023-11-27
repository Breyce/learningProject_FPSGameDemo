using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���˽���Ѳ��״̬
/// </summary>
public class PatrolState : EnemyBaseState
{
    public override void EnemyState(Enemy enemy)
    {
        enemy.animState = 0; 
        //����·��
        enemy.LoadPath(enemy.wayPointObj[WayPointManager.instance.usingIndex[enemy.nameIndex]]);
    }

    public override void OnUpdate(Enemy enemy)
    {
        //�жϵ�ǰ�Ƿ��ڲ��ž�ֹ����
        //Debug.Log(enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
        if (enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && enemy.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95)
        {
            enemy.animState = 1;
        }

        if(enemy.animState == 1) enemy.MoveToTarget(enemy.wayPoints[enemy.index]);
        //������˺͵�����ľ���
        float distance = Vector3.Distance(enemy.transform.position, enemy.wayPoints[enemy.index]);

        //�����Сʱ�����Ѿ����˵�����
        if(distance < 0.5f)
        {
            enemy.animState = 0;
            enemy.animator.Play("Idle");

            enemy.index++; //������һ��������
            if(enemy.index >= enemy.wayPoints.Count)
            {
                enemy.index = 0;
            }
        }
        //Debug.Log(distance);

        //����Ѳ��ɨ�跶Χ�ڳ��ֵ��ˣ���ʱ���빥��״̬
        if (enemy.attackList.Count > 0)
        {
            enemy.TransitionToState(enemy.attackState);
        }

    }
}
