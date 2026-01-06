using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("게임 Panel")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject difficultyPanel;

    [Header("메인 화면 버튼")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button difficultyButton;
    [SerializeField] private Button quitButton;

    [Header("난이도 선택 버튼")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button backButton;

    [Header("난이도 표시 텍스트")]
    [SerializeField] private TextMeshProUGUI difficultyText; 
    private Difficulty selectedDifficulty = Difficulty.Normal;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //메인 메뉴 버튼
        if(startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
        if(difficultyButton != null)
        {
            difficultyButton.onClick.AddListener(OpenDifficultyPanel);
        }
        if(quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        //난이도 선택 버튼
        if(easyButton != null) easyButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Easy));
        if(normalButton != null) normalButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Normal));
        if(hardButton != null) hardButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Hard));
        if(backButton != null) backButton.onClick.AddListener(OpenMainPanel);

        //초기 설정 메인 패널만 보이게 하기
        ShowMainPanel();
        UpdateDifficultyDisplay();

    }

    //메인 패널 표시
    void ShowMainPanel()
    {
        if(mainPanel != null) mainPanel.SetActive(true);
        if(difficultyPanel != null) difficultyPanel.SetActive(false);
    }

    // 난이도 설정 패널 열기
    void OpenDifficultyPanel()
    {
        if(mainPanel != null) mainPanel.SetActive(false);
        if(difficultyPanel != null) difficultyPanel.SetActive(true);

        UpdateDifficultyDisplay();
    }

    //메인 패널로 돌아가기
    void OpenMainPanel()
    {
        ShowMainPanel();
    }

    void SelectDifficulty(Difficulty difficulty)
    {
        selectedDifficulty = difficulty;
        UpdateDifficultyDisplay();
        HighLightButton();
    }

    //난이도 표시 업데이트
    void UpdateDifficultyDisplay()
    {
        if(difficultyText != null)
        {
            string difficultyInfo = "";

            switch(selectedDifficulty)
            {
                case Difficulty.Easy:
                    difficultyInfo = "Easy - Key : 3";
                    break;
                case Difficulty.Normal:
                    difficultyInfo = "Normal - Key : 5";
                    break;
                case Difficulty.Hard:
                    difficultyInfo = "Hard - Key : 8";
                    break;
            }
            difficultyText.text = $"Mode : {difficultyInfo}";
        }
    }

    //선택된 버튼 하이라이트
    void HighLightButton()
    {
        ResetButtonColor();

        ColorBlock highLightColors = new ColorBlock
        {
            normalColor = Color.yellow,
            highlightedColor = Color.yellow,
            pressedColor = new Color(1f, 0.8f, 0f),
            selectedColor = Color.yellow,
            disabledColor = Color.gray,
            colorMultiplier = 1,
            fadeDuration = 0.1f
        };

        switch(selectedDifficulty)
        {
            case Difficulty.Easy:
                if(easyButton != null) easyButton.colors = highLightColors;
                break;
            case Difficulty.Normal:
                if(normalButton != null) normalButton.colors = highLightColors;
                break;
            case Difficulty.Hard :
                if(hardButton != null) hardButton.colors = highLightColors;
                break;
        }
    }

    void ResetButtonColor()
    {
        ColorBlock defaultColors = ColorBlock.defaultColorBlock;

        if(easyButton != null) easyButton.colors = defaultColors;
        if(normalButton != null) normalButton.colors = defaultColors;
        if(hardButton != null) hardButton.colors = defaultColors;
    }


    public void StartGame()
    {
        if(GameDifficulty.Instance != null)
        {
            GameDifficulty.Instance.SetDifficulty(selectedDifficulty);
        }
        else
        {
            Debug.Log($"GameDifficulty 인스턴스를 찾을 수 없습니다.");
        }

        SceneManager.LoadScene("GameScenes");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();     //빌드된 게임에서만 적용
        #endif
    }


    
}
