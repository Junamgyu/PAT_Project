using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void StartGame()
    {
        SceneManager.LoadScene("GameScenes");
    }

    public void QuitGame()
    {
        Application.Quit();     //빌드된 게임에서만 적용
    }
    void Start()
    {
    }




    // Update is called once per frame
    void Update()
    {
        
    }
}
