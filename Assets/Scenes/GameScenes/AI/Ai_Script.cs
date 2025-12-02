using Unity.Mathematics;
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
    public float ChaseTime = 5f;         //시야를 잃은 후 추적 유지 시간
    public float loseTimer = 0f;        //시야 잃은 후 시간 측정

    [Header("Formation Settings")]
    public float formationDistance = 12f;       //포위 시작 거리
    public float directChaseDistance = 8f;      //직접 추격 시작 거리
    public float formationReachThreshold = 2f;  //포메이션 위치 도달 판정 거리
    public float modeTransitionBuffer = 1.5f;
    
    

    // == 내부 변수 ==
    private NavMeshAgent agent;     //추적 오브젝트
    private int currentPatrolIndex = 0;     //추적 지점 인덱스
    private bool isChasing = false;         //기본 추격 상태 = false
    private Animator animator;

    // == 순찰 관련 변수 ==
    private float waitTimer = 0f;           //0초 부터 waitDuration초 까지 타이머
    private float waitDuration = 3f;        //순찰 지점 기다림 3초

    // == 포위 군집 관련 변수 ==
    private float updateTimer = 0f;         //? Flocking 갱신 주기
    private float updateInterval = 0.1f;       //? 계산 주기 (성능 최적화용)

    //Formation status
    private bool isInFormation = false;
    private Vector3 currentFormationTarget;
    private float formationUpdateTimer = 0f;        //? 포메이션 지속 갱신용

    private enum ChaseMode { WideFormation, NarrowFormation, DirectChase}
    private ChaseMode currentChaseMode = ChaseMode.WideFormation;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        //블랙보드에 등록
        if (AIBlackboard.Instance != null)
            AIBlackboard.Instance.RegisterAI(transform);

        if (patrolPoint.Length > 0)
        {
            agent.SetDestination(patrolPoint[0].position);
        }
        animator.SetBool("isWalk", true);
    }

    void OnDestroy()        //Formation 추가
    {
        if (AIBlackboard.Instance != null)
            AIBlackboard.Instance.UnregisterAI(transform);        
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!isChasing && (PlayerInSight() || distanceToPlayer <= chaseDis))     //플레이어가 시야 안에 있거나 일정거리 내에 들어왔을 때 추격 시작
        {
            agent.isStopped = false;    //이동 재게
            isChasing = true;
            AIBlackboard.Instance.ReportPlayer(player.position);        //? 공유 알림
        }

        //블랙보드 기반 추격
        if (!isChasing && AIBlackboard.Instance.playerDetect)    //? 순찰중 플레이어 위치 공유 받을때 
        {
            isChasing = true;
            agent.isStopped = false;
            agent.SetDestination(AIBlackboard.Instance.lastPos);
            Debug.Log($"{gameObject.name} 플레이어 정보 공유 받음!");
        }

        // == 추적 상태 == //
        if (isChasing)      //추적 중이라면 시야 및 거리 기반으로 유지/해제
        {
            agent.isStopped = false;  //추격중에는 항상 이동 가능하게

            animator.SetBool("isChase", true);
            animator.SetBool("isWalk", false);

            //시야에 있거나 가까이 있으면 위치 갱신
            if (PlayerInSight() || (distanceToPlayer <= detectionRange))
            {
                loseTimer = 0f;     //시야 안이면 타이머 초기화
                AIBlackboard.Instance.ReportPlayer(player.position);    //? 플레이어 위치 갱신
                AIBlackboard.Instance.formationCenter = player.position;   //? Forma
            }
            else
            {
                loseTimer += Time.deltaTime;    //시야를 잃은 상태면 타이머 증가            
            }

            //거리별 행동 결정
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;

                //현재 모드에 따라 다른 임계값 사용 (왔다갔다 방지)
                switch (currentChaseMode)
                {
                    case ChaseMode.WideFormation:
                        if(distanceToPlayer <= formationDistance - modeTransitionBuffer)
                        {
                            currentChaseMode = ChaseMode.NarrowFormation;
                            MoveToFormation(6f);
                        }
                        else
                        {
                            MoveToFormation(12f);
                        }
                        break;
                    
                    case ChaseMode.NarrowFormation:
                        if(distanceToPlayer > formationDistance + modeTransitionBuffer)
                        {
                            currentChaseMode = ChaseMode.WideFormation;
                            MoveToFormation(12f);
                        }
                        else if(distanceToPlayer <= directChaseDistance - modeTransitionBuffer)
                        {
                            currentChaseMode = ChaseMode.DirectChase;
                            DirectChase();
                        }
                        else
                        {
                            MoveToFormation(6f);
                        }
                        break;
                    
                    case ChaseMode.DirectChase:
                        if(distanceToPlayer > directChaseDistance + modeTransitionBuffer)
                        {
                            currentChaseMode = ChaseMode.NarrowFormation;
                            MoveToFormation(6f);
                        }
                        else
                        {
                            DirectChase();
                        }
                        break;
                }
            }
            //포메이션 상태일 때도 지속적으로 위치 갱신
            if (isInFormation)
            {
                formationUpdateTimer += Time.deltaTime;
                if (formationUpdateTimer >= 0.2f)           //0.2초 마다 포메이션 재계산
                {
                    formationUpdateTimer = 0f;

                    //현재 거리에 맞는 반경으로 포메이션 위치 재계산
                    float currentRadius = distanceToPlayer > formationDistance ? 12f : 6f;
                    Vector3 newFormationPos = AIBlackboard.Instance.GetFormationPosition(transform, player.position, currentRadius);    //항상 최신 플레이어 위치 사용

                    //새 포메이션 위치가 현재 위치에서 일정 거리 이상 떨어져있으면 이동
                    if (Vector3.Distance(transform.position, newFormationPos) > formationReachThreshold)
                    {
                        isInFormation = false;          //다시 이동 모드로
                        agent.SetDestination(newFormationPos);
                    }
                }
            }

            //추격 중단 조건
            if (loseTimer >= ChaseTime && distanceToPlayer >= stopChaseDis && !PlayerInSight())
            {
                isChasing = false;
                isInFormation = false;
                currentChaseMode = ChaseMode.WideFormation;
                animator.SetBool("isChase", false);
                animator.SetBool("isWalk", true);
                loseTimer = 0f;
                formationUpdateTimer = 0f;

                //마지막 Ai가 추격을 포기할 때만 블랙보드 초기화
                bool anyOtherAIChasing = false;
                foreach (var ai in AIBlackboard.Instance.aiAgents)
                {
                    if (ai != transform && ai.GetComponent<Ai_Script>().isChasing)
                    {
                        anyOtherAIChasing = true;
                        break;
                    }
                }
                
                if (!anyOtherAIChasing)
                {
                    AIBlackboard.Instance.ClearDetection();
                }

                AIBlackboard.Instance.ClearDetection();
                Debug.Log($"{gameObject.name} 추격 실패");
            }
        }
        else
        {
            Patrol();

        }
    }

    void MoveToFormation(float radius)
    {
        if (AIBlackboard.Instance == null || !AIBlackboard.Instance.playerDetect)
            return;

        // 항상 최신 플레이어 위치 기준으로 포메이션 계산
        Vector3 formationPos = AIBlackboard.Instance.GetFormationPosition(
            transform,
            player.position,    // formationCenter 대신 Player.position 직접 사용
            radius
        );

        currentFormationTarget = formationPos;
        //포메이션 위치에 도달했는지 확인
        float distToFormation = Vector3.Distance(transform.position, formationPos);

        if (distToFormation > formationReachThreshold)
        {
            // 포메이션 위치로 이동 중
            isInFormation = false;

            // Flocking 효과 적용 (충돌 방지)
            Vector3 flockingDir = AIBlackboard.Instance.GetFlockingDir(transform);
            Vector3 toFormation = (formationPos - transform.position).normalized;

            // 포메이션 70% + Flocking 30%
            Vector3 finalDir = (toFormation * 0.7f + flockingDir * 0.3f).normalized;

            agent.SetDestination(formationPos);

            // 이동 방향으로 회전
            if (finalDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(finalDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }

        }
        else
        {
            // 포메이션 위치에 도착 - 플레이어를 향해 회전만
            isInFormation = true;

            //플레이어 방향으로 회전
            Vector3 lookDir = (player.position - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }

            // 제자리에 멈추지 않고 포메이션 위치 유지
            agent.SetDestination(formationPos);
        }
    }
    
    void DirectChase()
    {
        //직접 추격 모드 - 플레이어를 바로 쫓아감
        isInFormation = false;

        //약간의 Flocking 효과만 추가 (다른 AI와 충돌 방지)
        Vector3 flockingDir = AIBlackboard.Instance.GetFlockingDir(transform);
        Vector3 toPlayer = (player.position - transform.position).normalized;

        //플레이어 추적 80프로 + Flocking 20프로
        Vector3 finalDir = (toPlayer * 0.8f + flockingDir * 0.2f).normalized;

        //플레이어 바로 뒤 1m 지점을 목표로
        Vector3 targetPos = player.position + (transform.position - player.position).normalized * 1f;

        agent.SetDestination(targetPos);

        //이동 방향으로 회전
        if (finalDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(finalDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
        }
    
    }
    

    void Patrol()       //순찰모드 
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

