using Unity.VisualScripting;
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
    public float ChaseTime = 3f;         //시야를 잃은 후 추적 유지 시간
    public float loseTimer = 0f;        //시야 잃은 후 시간 측정

    // == 내부 변수 ==
    private NavMeshAgent agent;     //추적 오브젝트
    private int currentPatrolIndex = 0;     //추적 지점 인덱스
    private bool isChasing = false;         //기본 추격 상태 = false
    private Animator animator;

    // == 순찰 관련 변수 ==
    private float waitTimer = 0f;           //0초 부터 waitDuration초 까지 타이머
    private float waitDuration = 3f;        //순찰 지점 기다림 3초
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (patrolPoint.Length > 0)
        {
            agent.SetDestination(patrolPoint[0].position);
        }
        animator.SetBool("isWalk", true);
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!isChasing && (PlayerInSight() || distanceToPlayer <= chaseDis))     //플레이어가 시야 안에 있거나 일정거리 내에 들어왔을 때 추격 시작
        {
            agent.isStopped = false;    //이동 재게

            isChasing = true;
            //Debug.Log("Chasing now!");
        }

        if (isChasing)      //추적 중이라면 시야 및 거리 기반으로 유지/해제
        {
            agent.isStopped = false;  //추격중에는 항상 이동 가능하게

            agent.SetDestination(player.position);

            animator.SetBool("isChase", true);
            animator.SetBool("isWalk", false);

            if (PlayerInSight())
            {
                loseTimer = 0f;     //시야 안이면 타이머 초기화
            }
            else
            {
                loseTimer += Time.deltaTime;    //시야를 잃은 상태면 타이머 증가            
            }

            if (!PlayerInSight() && (loseTimer >= ChaseTime) && distanceToPlayer >= stopChaseDis)
            {
                isChasing = false;
                animator.SetBool("isChase", false);
                animator.SetBool("isWalk", true);
            }
        }
        else
        {
            patrol();
            //Debug.Log("Patrol Start!");
        }
    }

    void patrol()       //순찰모드 
    {
        if (patrolPoint.Length == 0) return;
        if (agent.pathPending) return;               //경로 계산중 이거나 목적지까지 도달하지 않았음 이동

        //도착 거리 확인
        if (agent.remainingDistance < 0.5f)
        {
            waitTimer += Time.deltaTime;        //! 지점 도착후 타이머 시작

            //지점 도착후 3초 이상일 경우 다른 지점으로 이동   
            if (waitTimer >= waitDuration)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoint.Length;         //현재 순찰 인덱스를 다음 인덱스로 변환
                agent.SetDestination(patrolPoint[currentPatrolIndex].position);             //해당 인덱스로 이동
                waitTimer = 0f;
            }
            else
            {
                agent.isStopped = true; //대기중엔 멈춰있기
                animator.SetBool("isWalk", false);
            }
        }
        else
        {
            agent.isStopped = false;    //이동중엔 경로 계속 갱신
            animator.SetBool("isWalk", true);
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


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDis);    //! 추적 거리 빨강

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopChaseDis); //? 추적 중단 파랑

        //todo 시야각 노랑 
        Gizmos.color = Color.yellow;
        Vector3 rightLimit = Quaternion.Euler(0, aiAngle / 2, 0) * transform.forward * detectionRange;
        Vector3 leftLimit = Quaternion.Euler(0, -aiAngle / 2, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + rightLimit);
        Gizmos.DrawLine(transform.position, transform.position + leftLimit);
    }
}

