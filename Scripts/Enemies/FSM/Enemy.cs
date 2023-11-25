using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ������
/// ʵ��״̬�л������ص���Ѳ��·��
/// </summary>
public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    public Animator animator;

    public GameObject[] wayPointObj; // ��ŵ��˲�ͬ·��
    public List<Vector3> wayPoints = new List<Vector3>(); // ���Ѳ��·�ߵ�ÿ��Ѳ�ߵ�

    public int index; //�±�ֵ
    public int animState; // ����״̬��ʶ��0 idle 1 walk 2 attack

    public EnemyBaseState currentState; // �洢���˵�ǰ��״̬
    private PatrolState patrolState = new PatrolState(); //�������Ѳ��״̬
    private AttackState attackState = new AttackState(); //������˹���״̬

    Vector3 targetPosition;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        index = 0;

        //��Ϸһ��ʼ����Ѳ��״̬
        TransitionToState(patrolState);
    }

    // Update is called once per frame
    void Update()
    {
        currentState.OnUpdate(this);
        animator.SetInteger("state", animState);
    }

    /// <summary>
    /// �������ŵ������ƶ�
    /// </summary>
    public void MoveToTarget()
    {
        targetPosition = Vector3.MoveTowards(transform.position, wayPoints[index], agent.speed * Time.deltaTime);
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

}
