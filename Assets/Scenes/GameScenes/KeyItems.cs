using Unity.VisualScripting;
using UnityEngine;

public class KeyItems : MonoBehaviour
{
    [Header("난이도 설정")]
    public Difficulty requireDifficulty = Difficulty.Easy; //열쇠가 활성화 되는 최소 난이도

    [Header("회전 애니메이션")]
    public bool rotateKey = true;
    public float rotationSpeed = 50f;
    private bool isCollected = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //현재 난이도에 따라 열쇠 활성/비활성
        CheckDifficultyActive();
    }

    // Update is called once per frame
    void Update()
    {
        if(rotateKey && !isCollected)
        {
            transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
        }
    }
    void CheckDifficultyActive()
    {
        if(GameDifficulty.Instance == null)
        {
            Debug.LogWarning($"GameDifficulty 인스턴스가 없습니다!");
            return;
        }
        Difficulty currentDifficulty = GameDifficulty.Instance.GetCurrentDifficulty();

        bool shouldActive = false;

        switch(requireDifficulty)
        {
            case Difficulty.Easy:       //난이도가 쉬움일 경우 항상 활성화
                shouldActive = true;
                break;
            case Difficulty.Normal:
                shouldActive = (currentDifficulty == Difficulty.Normal || currentDifficulty == Difficulty.Hard);
                break;
            case Difficulty.Hard:
                shouldActive = (currentDifficulty == Difficulty.Hard);
                break;
        }
        gameObject.SetActive(shouldActive);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if(other.CompareTag("Player"))
        {
            CollectKey();
        }
    }

    void CollectKey()
    {
        isCollected = false;

        if(KeyManager.Instance != null)
        {
            KeyManager.Instance.CollectKey();
        }
        Destroy(gameObject);
    }
}
