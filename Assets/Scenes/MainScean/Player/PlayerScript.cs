using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float runSpeed;
    public float rotationSpeed = 10f;
    Animator animator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");   //앞뒤 w일 경우 +1 s 일 경우 -1
        float moveZ = Input.GetAxisRaw("Vertical");     //좌우 a일 경우 -1 d 일 경우 +1

        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;                 //이동 방향 벡터 계산
        transform.Translate(move * runSpeed * Time.deltaTime, Space.World);     //이동처리


        if (Input.GetKey(KeyCode.LeftShift))
        {
            runSpeed = 6f;                         //달릴때 속도
            animator.SetBool("isRun", true);       //?달리기 true
        }
        else
        {
            runSpeed = 3f;                       // 걸을때 속도
            animator.SetBool("isRun", false);    //? 달리기 false
        }

        //플레이어 이동에 따른 회전
        if (move != Vector3.zero)
        {
            animator.SetBool("isWalk", true);
            Quaternion targetRotation = Quaternion.LookRotation(move); //이동 방향을 바라보는 회전 계산
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        }
        else
        {
            animator.SetBool("isWalk", false);
        }

    }
}
