using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人进入攻击状态
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
        //当前敌人没有目标时，切换回巡逻状态；
        if(enemy.attackList.Count == 0)
        {
            enemy.TransitionToState(enemy.patrolState);
        }
        
        //当敌人只有一个攻击目标，就只找List里面的那个
        else if (enemy.attackList.Count == 1)
        {
            enemy.targetPoint = enemy.attackList[0];
        }
        //当前敌人有目标，但是存在多个目标，找距离最近的攻击目标
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

        //敌人攻击玩家
        if(enemy.targetPoint.tag == "Player")
        {
            //敌人对玩家进行攻击
            enemy.AttackAction();
        }


        enemy.MoveToTarget(enemy.targetPoint.position);
    }
}
