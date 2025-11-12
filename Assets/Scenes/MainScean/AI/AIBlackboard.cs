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
            aiAgents.Add(ai);
    }

    public void UnregisterAI(Transform ai)
    {
        if (aiAgents.Contains(ai))
            aiAgents.Remove(ai);
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

    //각 AiI가 Flocking + Formation 이동 시 참고할 목표 방향 계산 함수
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

            if (distance < neighborRadius)
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

        // 플레이어를 기준으로 한 Formation 위치 계산
        Vector3 formationOffset = Vector3.zero;
        if (playerDetect && aiAgents.Count > 0)
        {
            int index = aiAgents.IndexOf(self);
            float angle = (360f / aiAgents.Count) * index;
            formationOffset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad), 0,
                Mathf.Sin(angle * Mathf.Deg2Rad)) * FormationRadius;
        }

        Vector3 finalDir = separation * separationWeight +
        alignment * alignmentWeight +
        cohesion * cohesionWeight;

        if (playerDetect)
            finalDir += (formationCenter + formationOffset - self.position).normalized;

        return finalDir.normalized;
    }
}
