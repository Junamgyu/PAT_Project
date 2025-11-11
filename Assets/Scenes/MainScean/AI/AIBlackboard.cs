using UnityEngine;

public class AIBlackboard : MonoBehaviour
{
    public static AIBlackboard Instance;    //singleton (모든 Ai가 접근)

    public bool playerDetect = false;
    public Vector3 lastPos;             //마지막으로 감지된 플레이어 위치 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //singleton
        if (Instance == null)               //인스턴스 비어있을 경우 지금 객체를 해당 인스턴스로 지정
            Instance = this;
        else
            Destroy(gameObject);                //아닐 경우 중복생성을 위해 파괴, 게임 전체에 하나의 AIBlackboard를 가지기 위함
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
}
