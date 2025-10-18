using UnityEngine;

[RequireComponent(typeof(CharacterController))]     //CharactorController 컴포넌트가 자동으로 추가되도록 보장
public class PlayerScript : MonoBehaviour
{
    public float runSpeed;
    public float rotationSpeed = 10f;


    Animator animator;
    private Transform Cam; // 카메라 transform을 저장할 변수
    private CharacterController controller;     //CharacterController 참조

    //중력 처리를 위한 변수
    private Vector3 PlayerVelocity;
    private float gravityValue = -9.81f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        Cam = Camera.main.transform; // 메인 카메라의 트렌스폼을 가져옴
        controller = GetComponent<CharacterController>();   //CharacterController 컴포넌트를 가져옴
    }

    // Update is called once per frame
    void Update()
    {
        //중력 처리
        //CharacterCOntroller가 땅에 닿아있는지 확인
        bool isGrounded = controller.isGrounded;
        if (isGrounded && PlayerVelocity.y > 0)
        {
            //땅에 닿아있으면 수직 속도를 0에 가깝게 리셋 (지면에 붙어있도록)
            PlayerVelocity.y = -2f;
        }

        //입력 받기
        float moveX = Input.GetAxisRaw("Horizontal");   //앞뒤 w일 경우 +1 s 일 경우 -1
        float moveZ = Input.GetAxisRaw("Vertical");     //좌우 a일 경우 -1 d 일 경우 +1
        Vector3 inputDirection = new Vector3(moveX, 0, moveZ).normalized;                 //이동 방향 벡터 계산

        Vector3 currentMoveDirection = Vector3.zero; //수평 이동 방향 (기본 0)

        //입력 방향 백터 (백터 자체는 카메라와 무관함)

        //! 이동 입력이 있을 경우에만 카메라 기준으로 방향 백터 잡기
        if (inputDirection.magnitude >= 0.1f)     //이동입력이 있을 경우
        {
            if(Cam != null)
            {
                Vector3 CamForward = Cam.forward;   //카메라의 전방 백터를 가져옴
                CamForward.y = 0;                   //캐릭터가 땅에 붙어 있도록 y축을 0으로 만들기 //수평 이동만
                CamForward.Normalize();             //길이 1의 백터로 정규화

                //? 카메라 우측 방향 가져와 위와 같이 정규화, y축 0 
                Vector3 CamRight = Cam.right;
                CamRight.y = 0;
                CamRight.Normalize();

                 //---------최종 이동 방향 백터---------------------------
                currentMoveDirection = CamForward * moveZ + CamRight * moveX;       //moveZ, moveX를 통해 카메라의 전방/우측 벡터를 곱해 월드 공간의 이동 백터 desiremovedirection
                currentMoveDirection.Normalize();                                           //최종 방향을 정규화해 속도 일정하게 보정          
            }   

            //? 회전 처리
            Quaternion targetRotation = Quaternion.LookRotation(currentMoveDirection); //이동 방향을 바라보는 회전 계산
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            //플레이어 이동 애니메이션
            animator.SetBool("isWalk", true);

        }
        else    //이동하지 않을경우
        {
            animator.SetBool("isWalk", false);
        }

        //달리는 애니메이션 처리 
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

        Vector3 horizontalMove = currentMoveDirection * runSpeed;
        PlayerVelocity.y += gravityValue * Time.deltaTime;

        Vector3 finalMove = horizontalMove;
        finalMove.y = PlayerVelocity.y;

        controller.Move(finalMove * Time.deltaTime);


    }
}
