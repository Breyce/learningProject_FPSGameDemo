using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���˽��빥��״̬
/// </summary>
public class AttackState : EnemyBaseState
{
    public override void EnemyState(Enemy enemy)
    {
        enemy.animState = 2;
        enemy.targetPoint = enemy.attackList[0];
    }

    public override void OnUpdate(Enemy enemy)
    {
        //��ǰ����û��Ŀ��ʱ���л���Ѳ��״̬��
        if(enemy.attackList.Count == 0)
        {
            enemy.TransitionToState(enemy.patrolState);
        }
        
        //������ֻ��һ������Ŀ�꣬��ֻ��List������Ǹ�
        else if (enemy.attackList.Count == 1)
        {
            enemy.targetPoint = enemy.attackList[0];
        }
        //��ǰ������Ŀ�꣬���Ǵ��ڶ��Ŀ�꣬�Ҿ�������Ĺ���Ŀ��
        else if (enemy.attackList.Count > 1)
        {
            for(int i = 0; i < enemy.attackList.Count; i++)
            {
                float distanceNow = Vector3.Distance(enemy.transform.position, enemy.targetPoint.transform.position);
                float distance = Vector3.Distance(enemy.transform.position, enemy.attackList[i].transform.position);
                if(distance < distanceNow)
                {
                    enemy.targetPoint = enemy.attackList[i];
                }
            }
        }

        //���˹������
        if(enemy.targetPoint.tag == "Player")
        {
            //���˶���ҽ��й���
            enemy.AttackAction();
        }


        enemy.MoveToTarget(enemy.targetPoint.position);
    }
}
