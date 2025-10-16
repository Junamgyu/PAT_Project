
using UnityEngine;

public class Player_Camera : MonoBehaviour
{
    [Header("Target Setthing")]
    public Transform Target;

    [Header("Distance Setthing")]
    public float Distance = 3.0f;
    public float Height = 1.0f;

    [Header("Max Rotation")]
    public float MouseSensitivity = 3.0f;   //마우스 회전 속도
    public float minYAngle = -35f;          //최소 아래쪽 각도
    public float maxYAngle = 60f;           //최대 위쪽 각도

    [Header("Collision Setthing")]
    public float collisionBuffer = 0.2f;    //충돌 지점에서 카메라를 떼어 놓을 거리 (벽뚫림 방지)
    public LayerMask collisionLayer;         //카메라 충돌을 감지할 레이어 (기본적으로 모두 설정)
   
   //마우스 입력 값;
    [Header("Smooth speed")]
    public float smoothSpeed = 10f;
    private float mosY;     //마우스 위아래
    private float mosX;     //마우스 좌우

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(collisionLayer.value == 0)           //충돌 레이어를 설정하지 않으면 기본값으로 모든 레이어 설정
        {
            collisionLayer = ~0;
        }

        Cursor.lockState = CursorLockMode.Locked;   // 마우스 커서 잠금
        Cursor.visible = false;                     // 마우스 안보이게 하기
    }

    void LateUpdate()                               //LateUpdate 에서 카메라 이동 문제 프레임 동기화 문제를 해결  
    {
        //카메라 벽뚫 버그를 위해 LateUpdate로 수정 진행해야 함
        if (Target == null) return;
        

        //마우스 값 입력받기
        // + 또는 - 하는 이유는 값이 계속 누적해야 되기 때문 // = 만 하면 각도가 안움직이는 것 처럼 보임.
        mosX += Input.GetAxisRaw("Mouse X") * MouseSensitivity;     //오른쪽 + 왼쪽 -   //오른쪽으로 돌리면 + 우측으로 돌아감
        mosY -= Input.GetAxisRaw("Mouse Y") * MouseSensitivity;     //위 + 아래 -       //-=를 통해 방향 반대로 전환해주기

        mosY = Mathf.Clamp(mosY, minYAngle, maxYAngle);     //상하 회전 제한

        Quaternion rotation = Quaternion.Euler(mosY, mosX, 0); //회전 계산

        
        Vector3 CameraPosition = Target.position - (rotation * Vector3.forward * Distance) + Vector3.up * Height; // 카메라가 있을 위치
        transform.position = Vector3.Lerp(transform.position, CameraPosition, smoothSpeed * Time.deltaTime); //부드럽게 이동하기

        transform.LookAt(Target.position + Vector3.up * 1.5f);

        
    }


    // Update is called once per frame
    void Update()
    {

    }
}
