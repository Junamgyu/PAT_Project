using UnityEngine;
using UnityEngine.AI;
public class Ai_Script : MonoBehaviour
{
    public Transform player;    //플레이어 오브젝트
    public Transform[] patrolPoint;     //순찰 지점
    private NavMeshAgent agent;     //추적 오브젝트
    private int currentPatrolIndex = 0;
    private bool isChasing = false;    

    public float chaseDistance = 10.0f;     //추적 시작 거리
    public float stopChaseDistance = 15.0f;     //추적 중단 거리
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if(patrolPoint.Length > 0)
        {
            agent.SetDestination(patrolPoint[0].position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //플레이어가 가까우면 추격 시작
        if (distanceToPlayer <= chaseDistance)
        {
            isChasing = true;
        }
        else if (distanceToPlayer >= stopChaseDistance)  //플레이어가 멀어지면 순찰 시작
        {
            isChasing = false;
        }

        if (isChasing)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            patrol();       //순찰 모드
        }

    }

    void patrol()       //순찰모드 
    {
        if (patrolPoint.Length == 0) return;

        //목적지에 거의 도착하면 다음 순찰 지점으로 변경
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoint.Length;
            agent.SetDestination(patrolPoint[currentPatrolIndex].position);
        }
    }
}
