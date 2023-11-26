using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// ������
/// ʵ��״̬�л������ص���Ѳ��·��
/// </summary>
public class Enemy : MonoBehaviour
{
    public Animator animator;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    

    public GameObject[] wayPointObj; // ��ŵ��˲�ͬ·��
    public List<Vector3> wayPoints = new List<Vector3>(); // ���Ѳ��·�ߵ�ÿ��Ѳ�ߵ�
    public Transform targetPoint;
    
    [Header("Route parameter")]
    public int index; //�±�ֵ
    public int nameIndex;
    public int animState; // ����״̬��ʶ��0 idle 1 walk 2 attack

    [Header("Enemy parameter")]
    public float enemyHealth;
    public Slider slider;
    public Text getDamageText;
    public GameObject deadEffect;
    private bool isDead;

    public EnemyBaseState currentState; // �洢���˵�ǰ��״̬
    public PatrolState patrolState; //�������Ѳ��״̬
    public AttackState attackState; //������˹���״̬

    Vector3 targetPosition;

    [Header("Enemy Attack")]
    //���˵Ĺ���Ŀ�꣬����������ң����б�洢
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
        //��ȡ���
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        patrolState = transform.gameObject.AddComponent<PatrolState>();
        attackState = transform.gameObject.AddComponent<AttackState>();

        //������ֵ
        index = 0;
        isDead = false;
        slider.minValue = 0;
        slider.maxValue = enemyHealth;
        slider.value = enemyHealth;

        //��Ϸһ��ʼ����Ѳ��״̬
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
    /// �������ŵ������ƶ�
    /// </summary>
    public void MoveToTarget(Vector3 targetPoint)
    {
        //if (attackList.Count == 0)
        //{
        //    targetPosition = Vector3.MoveTowards(transform.position, wayPoints[index], agent.speed * Time.deltaTime);
        //}
        //else
        //{
        //    //ɨ�赽���
        //    targetPosition = Vector3.MoveTowards(transform.position, attackList[0].transform.position, agent.speed * Time.deltaTime * 3);
        //}
        if (attackList.Count == 0)
            targetPosition = Vector3.MoveTowards(transform.position, targetPoint, agent.speed * Time.deltaTime);
        else
            targetPosition = Vector3.MoveTowards(transform.position, targetPoint, agent.speed * Time.deltaTime);

        agent.destination = targetPosition;
    }

    /// <summary>
    /// ����·��
    /// </summary>
    /// <param name="go">Ŀ���</param>
    public void LoadPath(GameObject go)
    {
        wayPoints.Clear();//����·��֮ǰ���ԭ��·��

        //����·�����е��������List����
        foreach (Transform T in go.transform) 
        {
            wayPoints.Add(T.position);
        }
    }

    /// <summary>
    /// �л����˵�״̬
    /// </summary>
    public void TransitionToState(EnemyBaseState state)
    {
        currentState = state;
        currentState.EnemyState(this);
    }

    /// <summary>
    /// ����Ѫ��״̬
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
    /// ���˵Ĺ�������
    /// </summary>
    public void AttackAction()
    {
        if(isDead) return;

        //�����˺���Ҿ���ܽ���ʱ�򴥷���������
        float distance = Vector3.Distance(transform.position, targetPoint.position);
        if(distance < attackRange)
        {
            if(Time.time > nextAttack)
            {
                //��������
                animator.SetTrigger("attack");
                //������һ�ι�����ֵ
                nextAttack = Time.time + attackRate;
            }
        }
    }

    /// <summary>
    /// Boss��������
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
