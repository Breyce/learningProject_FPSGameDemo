using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// 敌人类
/// 实现状态切换，加载敌人巡逻路线
/// </summary>
public class Enemy : MonoBehaviour
{
    public Animator animator;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    

    public GameObject[] wayPointObj; // 存放敌人不同路线
    public List<Vector3> wayPoints = new List<Vector3>(); // 存放巡逻路线的每个巡逻点
    public Transform targetPoint;
    
    [Header("Route parameter")]
    public int index; //下标值
    public int nameIndex;
    public int animState; // 动画状态标识：0 idle 1 walk 2 attack

    [Header("Enemy parameter")]
    public float enemyHealth;
    public Slider slider;
    public Text getDamageText;
    public GameObject deadEffect;
    private bool isDead;

    public EnemyBaseState currentState; // 存储敌人当前的状态
    public PatrolState patrolState; //定义敌人巡逻状态
    public AttackState attackState; //定义敌人攻击状态

    Vector3 targetPosition;

    [Header("Enemy Attack")]
    //敌人的攻击目标，场景中有玩家，用列表存储
    public float attackRate;
    public float attackRange;
    public GameObject attackParticle01;
    public Transform attackParticle01Position;
    public AudioClip attackSound;
    public List<Transform> attackList = new List<Transform>();
    private float nextAttack = 0;


    // Start is called before the first frame update
    void Start()
    {
        //获取组件
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        patrolState = transform.gameObject.AddComponent<PatrolState>();
        attackState = transform.gameObject.AddComponent<AttackState>();

        //参数赋值
        index = 0;
        isDead = false;
        slider.minValue = 0;
        slider.maxValue = enemyHealth;
        slider.value = enemyHealth;

        //游戏一开始进入巡逻状态
        TransitionToState(patrolState);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        currentState.OnUpdate(this);
        animator.SetInteger("state", animState);
    }

    /// <summary>
    /// 敌人向着导航点移动
    /// </summary>
    public void MoveToTarget(Vector3 targetPoint)
    {
        //if (attackList.Count == 0)
        //{
        //    targetPosition = Vector3.MoveTowards(transform.position, wayPoints[index], agent.speed * Time.deltaTime);
        //}
        //else
        //{
        //    //扫描到玩家
        //    targetPosition = Vector3.MoveTowards(transform.position, attackList[0].transform.position, agent.speed * Time.deltaTime * 3);
        //}
        if (attackList.Count == 0)
            targetPosition = Vector3.MoveTowards(transform.position, targetPoint, agent.speed * Time.deltaTime);
        else
            targetPosition = Vector3.MoveTowards(transform.position, targetPoint, agent.speed * Time.deltaTime);

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

    /// <summary>
    /// 敌人血量状态
    /// </summary>
    public void Health(float damage)
    {
        if (isDead) return;

        getDamageText.text = "-" + Mathf.Round(damage);
        enemyHealth -= damage;
        slider.value = enemyHealth;

        if(enemyHealth <= 0)
        {
            isDead = true;
            animator.SetTrigger("dying");
            Destroy(Instantiate(deadEffect, transform.position, transform.rotation), 3f);
        }
    }

    /// <summary>
    /// 敌人的攻击方法
    /// </summary>
    public void AttackAction()
    {
        if(isDead) return;

        //当敌人和玩家距离很近的时候触发攻击动画
        float distance = Vector3.Distance(transform.position, targetPoint.position);
        if(distance < attackRange)
        {
            if(Time.time > nextAttack)
            {
                //触发攻击
                animator.SetTrigger("attack");
                //更新下一次攻击的值
                nextAttack = Time.time + attackRate;
            }
        }
    }

    /// <summary>
    /// Boss攻击方法
    /// </summary>
    public void PlayerMutantAttackEffect()
    {


    }

    public void OnTriggerEnter(Collider other)
    {
        if(!attackList.Contains(other.transform) && !isDead && (other.tag == "Player"))
        {
            attackList.Add(other.transform);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player") attackList.Remove(other.transform);
    }
}
