using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using NUnit.Framework;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance {get; private set;} //싱글톤 패턴
    [Header("타이머 설정")] 
    [SerializeField] private float gameTime = 180f;  // 180초 
    [Header("UI참조")]
    [SerializeField] private TextMeshProUGUI timerText;     //타이머
    [SerializeField] private GameObject gameOverPanel;  //게임오버
    [SerializeField] private Button reStartButton;      //다시하기 버튼
    [SerializeField] private Button mainMenuButton;     //메인메뉴로 돌아가기 버튼

    private float remainingTime;
    private bool isTimerRunning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        remainingTime = gameTime;
        isTimerRunning = true;          //게임 시작시 카운트 시작
        
        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);                                             //시작할 경우 게임오버 패널 가리기
        }
        
        if(reStartButton != null)               // 다시하기
        {
            reStartButton.onClick.AddListener(RestartGame);
        }
        if (mainMenuButton != null)             // 메인메뉴로 돌아가기
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        UpdateTimerDisplay();
    }
    void Update()
    {
        if(isTimerRunning)
        {
            remainingTime -= Time.deltaTime;

            if(remainingTime <= 0)
            {
                remainingTime = 0;
                isTimerRunning = false;
                TimeOut();
                
            }
            UpdateTimerDisplay();
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);

        if(timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if(remainingTime <= 30f)
            {
                timerText.color = Color.red;
            }
        }
    }

    void TimeOut()
    {
        
        Debug.Log("TimeOut, Game Fail");

        //게임 정지
        Time.timeScale = 0f;

        if(gameOverPanel != null)
        {
          gameOverPanel.SetActive(true);  
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestartGame()
    {
        Time.timeScale = 1f;    //게임시간 재개

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);                 //현재 씬 재시작

    }
    void GoToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        SceneManager.LoadScene("MainMenu");                                     //메인메뉴 씬으로 이동
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void PlayerDead()
    {
        TimeOut();
    }
}
