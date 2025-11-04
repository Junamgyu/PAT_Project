using UnityEngine;
using UnityEngine.AI;
public class Ai_Script : MonoBehaviour
{
    // == 공개 변수 설정 ==
    public Transform player;    //플레이어 오브젝트
    public Transform[] patrolPoint;     //순찰 지점

    [Header("chase/patrol distance")]
    public float chaseDis = 10.0f;     //추적 시작 거리
    public float stopChaseDis = 15.0f;     //추적 중단 거리
    public float detectionRange = 20.0f;        //시야를 통해 플레이어를 감지할 수 있는 최대 거리

    [Header("Angle option")]
    public float aiAngle = 90.0f;       //Ai 시야각 90.0f는 정면 180도

    // == 내부 변수 ==
    private NavMeshAgent agent;     //추적 오브젝트
    private int currentPatrolIndex = 0;     //추적 지점 인덱스
    private bool isChasing = false;
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
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
        bool inSight = PlayerInSight();

        //플레이어가 가까우면 추격 시작
        if (!isChasing && distanceToPlayer <= chaseDis || inSight)
        {
            isChasing = true;
            animator.SetBool("isChase", true);
            animator.SetBool("isWalk", false);
            Debug.Log("Chasing now! inSight!");
        }
        //추격 중단 조건 (플레이어가 완전히 멀어졌을 때만 순찰 시작)
        else if (isChasing && distanceToPlayer >= stopChaseDis && !inSight)  
        {
            isChasing = false;
            animator.SetBool("isChase", false);
            animator.SetBool("isWalk", true);
            Debug.Log("Patrol Start!!");
        }
        //추적거리는 멀어지고 시각으로만 보고 추격해 올때 

        if (isChasing)      //추적 모드
        {
            agent.SetDestination(player.position);
        }
        else                //순찰 모드
        {
            patrol();
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
    
    bool PlayerInSight()        //플레이어가 Ai의 시야각 및 시야 감지 거리 내에 있는지 확인
    {
        Vector3 dirToPlayer = player.position - transform.position;             //Ai에서 플레이어로 향하는 방향 벡터 계산
        float angle = Vector3.Angle(dirToPlayer, transform.forward);            //Ai의 정면과 플레이어 방향 사이의 각도 계산

        //계산된 각도가 설정한 시야각 보다 작을때
        if (angle < aiAngle * 0.5f)         //aiAngle / 2 보다 작을때
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, dirToPlayer.normalized, out hit, detectionRange))
            {
                if (hit.transform == player)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
