using System.Collections.Generic;
using UnityEngine;

public class AIBlackboard : MonoBehaviour
{
    public static AIBlackboard Instance;    //singleton (모든 Ai가 접근)

    public bool playerDetect = false;
    public Vector3 lastPos;             //마지막으로 감지된 플레이어 위치 


    // == Formation Flocking Settings ==
    public List<Transform> aiAgents = new List<Transform>();    //모든 AI등록 리스트
    public float neighborRadius = 5f;           //? 근처 AI 탐지 변경
    public float separationWeight = 1.5f;       //? 거리 유지
    public float alignmentWeight = 1f;          //? 방향 정렬
    public float cohesionWeight = 1f;           //? 집단 중심 이동
    public float FormationRadius = 5f;          //? 플레이어를 둘러싸는 반경
    public Vector3 formationCenter;             //? 포메이션 중심 (플레이어 근처)

    //각 Ai의 고정된 포메이션 각도 저장
    private Dictionary<Transform, float> assingedAngles = new Dictionary<Transform, float>();    // Start is called once before the first execution of Update after the MonoBehaviour is created


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
        }
    }

    public void UnregisterAI(Transform ai)
    {
        if (aiAgents.Contains(ai))
        {
            aiAgents.Remove(ai);
            assingedAngles.Remove(ai);
            ReassignFormationAngles();       // AI 제거 시 각도 재할당
        }
    }

    // 각 AI에게 고정된 포메이션 각도 할당
    private void ReassignFormationAngles()
    {
        assingedAngles.Clear();
        for (int i = 0; i < aiAgents.Count; i++)
        {
            float angle = (360f / aiAgents.Count) * i;
            assingedAngles[aiAgents[i]] = angle;
        }
    }
    
    public void ReportPlayer(Vector3 playerPos)     //플레이어를 인식한 Ai가 메서드 호출
    {
        playerDetect = true;
        lastPos = playerPos;
    }

    public void ClearDetection()            //플레이어를 놓쳤을 때 호출
    {
        playerDetect = false;
    }

    // 포메이션 목표 위치 계산 (고정된 각도 사용)
    public Vector3 GetFormationPosition(Transform self, Vector3 centerPos, float radius)
    {
        if (!assingedAngles.ContainsKey(self))
            return centerPos;

        float angle = assingedAngles[self];
        Vector3 offset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * radius;

        return centerPos + offset;
    }
    
    //Flocking 방향 계산 (포메이션과 분리)
    public Vector3 GetFlockingDir(Transform self)
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int neighborCount = 0;

        foreach (var agent in aiAgents)
        {
            if (agent == self) continue;
            float distance = Vector3.Distance(agent.position, self.position);

            if (distance < neighborRadius && distance > 0.01f)
            {
                // Separation (충돌 방지)
                separation += (self.position - agent.position).normalized / distance;

                //Alignment (이웃 방향 정렬)
                alignment += agent.forward;

                // Cohesion (그룹 중심으로 이동)
                cohesion += agent.position;

                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            alignment /= neighborCount;
            cohesion = (cohesion / neighborCount - self.position).normalized;
        }

        Vector3 flockingDir =
            separation * separationWeight +
            alignment * alignmentWeight +
            cohesion * cohesionWeight;

        return flockingDir.normalized;
    }
}
