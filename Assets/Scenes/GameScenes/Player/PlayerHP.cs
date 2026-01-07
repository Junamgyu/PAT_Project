using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;

public class PlayerHP : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private float maxHP = 100f;
    private float currentHP;

    [Header("UI 참조")]
    [SerializeField] private Slider hpSlider;   //hp bar
    [SerializeField] TextMeshProUGUI hpText;    //hp Text

    [Header("무적시간")]
    [SerializeField] private float invincibleTime = 1f;     //피격시 무적 시간 1초
    private float invincibleTimer = 0f;
    private bool isInvincible = false;
    private bool isDead = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isDead = false;
        currentHP = maxHP;

        UpdateHPUI();
    }

    // Update is called once per frame
    void Update()
    {
        //무적 시간 처리
        if(isInvincible)
        {
            invincibleTimer = -Time.deltaTime;
            if(invincibleTimer <= 0)
            {
                isInvincible = false;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if(isDead || isInvincible) return;

        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);    //0 이하로 내려가지 않게 하기 위함
        
        //무적 시간은 1초
        isInvincible = true;
        invincibleTimer = invincibleTime;

        UpdateHPUI();

        Debug.Log($"플레이어 피격 현재 HP : {currentHP} / {maxHP}");

        if(currentHP <= 0)
        {
            Die();
        }
    }

    //체력 UI업데이트
    void UpdateHPUI()
    {
        if(hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;
        }

        if(hpText != null)
        {
            hpText.text = $"{currentHP:F0} / {maxHP:F0}";
        }
    }

    void Die()
    {
        if(isDead) return;

        isDead = true;
        Debug.Log("플레이어 사망!");
        
        if(GameTimer.Instance != null)
        {
            GameTimer.Instance.PlayerDead();
        }
        
    }

    // 외부에서 변수 확인용 //
    public float GetCurrentHP()
    {
        return currentHP;
    }
    public float GetMaxHP()
    {
        return maxHP;
    }
    public bool IsDead()
    {
        return isDead;
    }
    public bool IsInvincible()
    {
        return isInvincible;
    }
}
