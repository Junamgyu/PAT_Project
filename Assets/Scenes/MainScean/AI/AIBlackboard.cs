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
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
