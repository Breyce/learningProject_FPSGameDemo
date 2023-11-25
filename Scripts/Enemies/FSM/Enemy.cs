using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敌人类
/// 实现状态切换，加载敌人巡逻路线
/// </summary>
public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    public Animator animator;

    public GameObject[] wayPointObj; // 存放敌人不同路线
    public List<Vector3> wayPoints = new List<Vector3>(); // 存放巡逻路线的每个巡逻点

    public int index; //下标值
    public int animState; // 动画状态标识：0 idle 1 walk 2 attack

    public EnemyBaseState currentState; // 存储敌人当前的状态
    private PatrolState patrolState = new PatrolState(); //定义敌人巡逻状态
    private AttackState attackState = new AttackState(); //定义敌人攻击状态

    Vector3 targetPosition;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        index = 0;

        //游戏一开始进入巡逻状态
        TransitionToState(patrolState);
    }

    // Update is called once per frame
    void Update()
    {
        currentState.OnUpdate(this);
        animator.SetInteger("state", animState);
    }

    /// <summary>
    /// 敌人向着导航点移动
    /// </summary>
    public void MoveToTarget()
    {
        targetPosition = Vector3.MoveTowards(transform.position, wayPoints[index], agent.speed * Time.deltaTime);
        agent.destination = targetPosition;
    }

    /// <summary>
    /// 加载路线
    /// </summary>
    /// <param name="go">目标点</param>
    public void LoadPath(GameObject go)
    {
        wayPoints.Clear();//加载路线之前清空原旧路线

        //便利路线所有导航点放入List当中
        foreach (Transform T in go.transform) 
        {
            wayPoints.Add(T.position);
        }
    }

    /// <summary>
    /// 切换敌人的状态
    /// </summary>
    public void TransitionToState(EnemyBaseState state)
    {
        currentState = state;
        currentState.EnemyState(this);
    }

}
