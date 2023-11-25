using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人进入巡逻状态
/// </summary>
public class PatrolState : EnemyBaseState
{
    public override void EnemyState(Enemy enemy)
    {
        enemy.animState = 0; 
        //加载路线
        enemy.LoadPath(enemy.wayPointObj[0]);
    }

    public override void OnUpdate(Enemy enemy)
    {
        //判断当前是否还在播放静止动画
        Debug.Log(enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
        if (enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && enemy.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95)
        {
            enemy.animState = 1;
        }

        if(enemy.animState == 1) enemy.MoveToTarget();
        //计算敌人和导航点的距离
        float distance = Vector3.Distance(enemy.transform.position, enemy.wayPoints[enemy.index]);

        //距离很小时表名已经到了导航点
        if(distance < 0.5f)
        {
            enemy.animState = 0;
            enemy.animator.Play("Idle");

            enemy.index++; //设置下一个导航点
            if(enemy.index >= enemy.wayPoints.Count)
            {
                enemy.index = 0;
            }
        }
        //Debug.Log(distance);

        //敌人巡逻扫描范围内出现敌人，此时进入攻击状态
    }
}
