
using UnityEngine;

public class Player_Camera : MonoBehaviour
{
    [Header("Target Setthing")]
    public Transform Target;

    [Header("Target Look Point")]
    public float TargetVerticalOffset = 0.4f;   //카메라가 바라볼 플레이어의 수직 중심점 (기본 0.4f)

    [Header("Distance Setthing")]
    public float Distance = 4.0f;
    public float Height = 2.0f;

    [Header("Max Rotation")]
    public float MouseSensitivity = 3.0f;   //마우스 회전 속도
    public float minYAngle = -35f;          //최소 아래쪽 각도
    public float maxYAngle = 60f;           //최대 위쪽 각도

    [Header("Collision Setthing")]
    public float collisionBuffer = 0.2f;    //충돌 지점에서 카메라를 떼어 놓을 거리 (벽뚫림 방지)
    public LayerMask collisionLayer;         //카메라 충돌을 감지할 레이어 (기본적으로 모두 설정)
   
   //마우스 입력 값;
    [Header("Smooth speed")]
    public float smoothTime = 0.1f;    //카메라가 목표 위치에 도달하는 데 걸리는 시간 (작을 수록 빠름)
    private float mosY;     //마우스 위아래
    private float mosX;     //마우스 좌우
    private Vector3 currentVelocity = Vector3.zero;     //SmoothDamp 사용을 위한 속도 참조 변수
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

        Quaternion rotation = Quaternion.Euler(mosY, mosX, 0); //회전 계산 Pitch, Yaw, Roll

        //회전 기준으로 플레이어로부터 떨어진 이상적인 위치 계산
        Vector3 idealOffset = rotation * Vector3.forward;
        Vector3 idealPosition = Target.position - (idealOffset * Distance) + Vector3.up * Height;

        // 충돌 감지를 위한 레이캐스트
        Vector3 targetCenter = Target.position + Vector3.up * TargetVerticalOffset; //플레이어의 눈높이
        Vector3 rayDirection = idealPosition - targetCenter;
        float rayDistance = rayDirection.magnitude;
        rayDirection.Normalize();

        RaycastHit hit;
        Vector3 finalCameraPosition;
        float currentSmoothTime = smoothTime;

        if (Physics.Raycast(targetCenter, rayDirection, out hit, rayDistance, collisionLayer, QueryTriggerInteraction.Ignore))
        {
            finalCameraPosition = hit.point + hit.normal * collisionBuffer;     //충돌 시: 충돌 지점(hit.point)에서 뒤로 살짝 물러난 위치를 최종 위치로 설정

            currentSmoothTime = 0.01f;

        }
        else
        {
            finalCameraPosition = idealPosition;        //충돌 없을 시: 이상적인 위치를 최종 위치로 설정
        }
        //카메라 위치 부드럽게 이동 (SmoothDamp) // 충돌 여부에 따라 동적으로 변경된 currentSmoothTIme 사용
        transform.position = Vector3.SmoothDamp(transform.position, finalCameraPosition, ref currentVelocity, currentSmoothTime);

        transform.LookAt(Target.position + Vector3.up * TargetVerticalOffset); //대상을 바라보게 하기
    }


    // Update is called once per frame
    void Update()
    {

    }
}
