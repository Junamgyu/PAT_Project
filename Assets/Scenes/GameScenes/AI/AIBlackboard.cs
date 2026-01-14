using System.Collections.Generic;
using UnityEngine;

public class AIBlackboard : MonoBehaviour
{
    public static AIBlackboard Instance;                //singleton (모든 Ai가 접근)

    public bool playerDetect = false;
    public Vector3 lastPos;                                  //마지막으로 감지된 플레이어 위치 
    private float lastUpdateTime = 0f;

    public List<Transform> aiAgents = new List<Transform>();                //모든 AI등록 리스트
    public float neighborRadius = 5f;           //? 근처 AI 탐지 변경
    public float separationWeight = 1.5f;       //? 거리 유지
    public float alignmentWeight = 1f;          //? 방향 정렬
    public float cohesionWeight = 1f;           //? 집단 중심 이동
    public float FormationRadius = 5f;          //? 플레이어를 둘러싸는 반경
    public Vector3 formationCenter;             //? 포메이션 중심 (플레이어 근처)

    //각 Ai의 고정된 포메이션 각도 저장
    private Dictionary<Transform, float> assingedAngles = new Dictionary<Transform, float>();
    private float angleOffset = 0f;         //더 자연스러온 포위를 위한 포메이션 각도 오프셋
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    void Awake()
    {
        //singleton
        if (Instance == null)               //인스턴스 비어있을 경우 지금 객체를 해당 인스턴스로 지정
            Instance = this;
        else
            Destroy(gameObject);                //아닐 경우 중복생성을 위해 파괴, 게임 전체에 하나의 AIBlackboard를 가지기 위함
    }

    public void RegisterAI(Transform ai)
    {
        if (!aiAgents.Contains(ai))
        {
            aiAgents.Add(ai);
            ReassignFormationAngles();          //새 AI 추가 시 각도 재할당
            //Debug.Log($"{ai.name} AI 등록됨. 총  AI 수 : {aiAgents.Count}");
        }
    }

    public void UnregisterAI(Transform ai)
    {
        if (aiAgents.Contains(ai))
        {
            aiAgents.Remove(ai);
            assingedAngles.Remove(ai);
            ReassignFormationAngles();       // AI 제거 시 각도 재할당
            //Debug.Log($"{ai.name} AI 등록 해제됨. 총  AI 수 : {aiAgents.Count}");
        }
    }

    // 각 AI에게 고정된 포메이션 각도 할당 (균등 분배)
    private void ReassignFormationAngles()
    {
        assingedAngles.Clear();

        if (aiAgents.Count == 0) return;

        // AI 개수에 따라 균등하게 각도 분배
        float angleStep = 360f / aiAgents.Count;

        for (int i = 0; i < aiAgents.Count; i++)
        {
            //45도 오프셋 추가 (정면 / 후면이 아닌 대각선 배치)
            float angle = (angleStep * i) + angleOffset;
            assingedAngles[aiAgents[i]] = angle;
        }
    }
    
    public void ReportPlayer(Vector3 playerPos)     //플레이어를 인식한 Ai가 메서드 호출
    {
        //위치 갱신 빈도 제한 (0.1초 마다 갱신)
        if (Time.deltaTime - lastUpdateTime < 0.1 && playerDetect)
            return;

        playerDetect = true;
        lastPos = playerPos;
        formationCenter = playerPos;        //포메이션 중심도 함께 갱신
        lastUpdateTime = Time.time;
    }

    public void ClearDetection()            //플레이어를 놓쳤을 때 호출
    {
        playerDetect = false;
        Debug.Log("블랙보드 : 플레이어 감지 초기화");
    }

    // 포메이션 목표 위치 계산 (고정된 각도 사용)
    public Vector3 GetFormationPosition(Transform self, Vector3 centerPos, float radius)
    {
        if (!assingedAngles.ContainsKey(self))
        {
            return centerPos;
        }

        float angle = assingedAngles[self];

        //원형 배치 계산
        Vector3 offset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * radius;

        Vector3 formationPos = centerPos + offset;

        // NavMesh 위의 유효한 위치로 조정
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(formationPos, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }

        return formationPos;
    }

    //Flocking 방향 계산 (포메이션과 분리)
    public Vector3 GetFlockingDir(Transform self)
    {
        Vector3 separation = Vector3.zero;          //분리
        Vector3 alignment = Vector3.zero;           //정렬
        Vector3 cohesion = Vector3.zero;            //응집
        int neighborCount = 0;

        foreach (var agent in aiAgents)
        {
            if (agent == null || agent == self) continue;

            float distance = Vector3.Distance(agent.position, self.position);

            //근처 이웃만 고려
            if (distance < neighborRadius && distance > 0.01f)
            {
                // Separation (분리) - 너무 가까우면 밀어냄
                Vector3 awayDir = (self.position - agent.position).normalized;
                //거리가 가까울 수록 더 강한 힘
                float separationStrength = 1f / (distance * distance);
                separation += awayDir * separationStrength;

                //Alignment (이웃 방향 정렬)    
                if (agent.GetComponent<Ai_Script>() != null)
                {
                    alignment += agent.forward;
                }

                // Cohesion (그룹 중심으로 이동)
                cohesion += agent.position;

                neighborCount++;
            }
        }

        //평균 계산
        if (neighborCount > 0)
        {
            alignment = (alignment / neighborCount).normalized;
            cohesion = ((cohesion / neighborCount) - self.position).normalized;
        }

        //가중치 적용하여 최종 방향 계산

        Vector3 flockingDir =
            separation * separationWeight +
            alignment * alignmentWeight +
            cohesion * cohesionWeight;

        return flockingDir.normalized;
    }

    public void DebugFormationStatus()
    {
        Debug.Log($"=== 포메이션 상태 ===");
        Debug.Log($"플레이어 감지: {playerDetect}");
        Debug.Log($"마지막 위치: {lastPos}");
        Debug.Log($"등록된 AI 수 : {aiAgents.Count}");

        foreach (var kvp in assingedAngles)
        {
            if (kvp.Key != null)
                Debug.Log($"{kvp.Key.name}: 각도 {kvp.Value}");
        }
    }

    private void OnDrawGizmos()
    {
        if (!playerDetect || aiAgents.Count == 0) return;

        //포메이션 중심
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(formationCenter, 0.5f);

        // 각 AI의 포메이션 위치
        foreach (var kvp in assingedAngles)
        {
            if (kvp.Key == null) continue;

            Vector3 formationPos = GetFormationPosition(kvp.Key, formationCenter, FormationRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(formationPos, 0.3f);
            Gizmos.DrawLine(formationCenter, formationPos);

            //AI 현재 위치에서 포메이션 위치로 선 그리기
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(kvp.Key.position, formationPos);
        }
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        DrawCircle(formationCenter, FormationRadius, 32);
    }
    

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0) * radius);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
