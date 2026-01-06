using Unity.VisualScripting;
using UnityEngine;

public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

public class GameDifficulty : MonoBehaviour
{
    public static GameDifficulty Instance {get; private set;}

    [Header("현재 난이도")]
    public Difficulty currentDifficulty = Difficulty.Normal;

    [Header("난이도 별 필요 열쇠 갯수")]
    public int easyKey = 3;
    public int normalKey = 5;
    public int hardKey = 8;

    void Awake()
    {
        //싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);      //씬 전환 시에도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }
    //난이도 설정
    public void SetDifficulty(Difficulty difficulty)
    {
        currentDifficulty = difficulty;
        Debug.Log($"난이도 설정 : {difficulty}");

    }

    //현재 난이도에 필요한 열쇠 갯수 가져오기
    public int GetRequiredKey()
    {
        switch(currentDifficulty)
        {
            case Difficulty.Easy:
                return easyKey;
            case Difficulty.Normal:
                return normalKey;
            case Difficulty.Hard:
                return hardKey;
            default:
                return normalKey;
        }
    }
    
    //현재 난이도 가져오기
    public Difficulty GetCurrentDifficulty()
    {
        return currentDifficulty;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
