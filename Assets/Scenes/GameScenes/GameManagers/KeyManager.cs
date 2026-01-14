using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance {get; private set;}

    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI keyCountText;  // "3 / 5" 이런식으로 표현할 예정
    [SerializeField] private GameObject gameClearPannel;    //게임 클리어 패널
    [SerializeField] private Button mainMenuButton;     //메인메뉴 버튼

    private int collectedKeys = 0; //현재 수집한 열쇠 갯수
    private int requiredKeys = 0;   //필요한 열쇠 갯수
    private bool isGameCleard = false;

    void Awake()
    {
        //싱글톤 패턴
        if(Instance == null)
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
        //현재 필요한 열쇠 갯수를 난이도에 따라 가져오기
        if(GameDifficulty.Instance != null)
        {
            requiredKeys = GameDifficulty.Instance.GetRequiredKey();
        }
        else
        {
            requiredKeys = 5;
        }

        //게임클리어 패널 숨기기
        if(gameClearPannel != null)
        {
            gameClearPannel.SetActive(false);
        }

        if(mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        UpdateKeyUI();
    }
    public void CollectKey()
    {
        if (isGameCleard) return;

        collectedKeys++;
        UpdateKeyUI();

        if (collectedKeys >= requiredKeys)
        {
            GameClear();
        }
    }
    void UpdateKeyUI()
    {
        if(keyCountText != null)  keyCountText.text = $"{collectedKeys} / {requiredKeys}";
        
    }
    void GameClear()
    {
        isGameCleard = true;
        
        //타이머 정지
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StopTimer();
        }
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if(gameClearPannel != null)
        {
            gameClearPannel.SetActive(true);
        }
    }

    void GoToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MainMenu");
    }

    public int GetCollectedKeys()
    {
        return collectedKeys;
    }

    public int GetRequiredKeys()
    {
        return requiredKeys;
    }

    public bool IsGameCleard()
    {
        return isGameCleard;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
