using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MoveController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Transform cameraMount;
    [SerializeField] private float lookSensitivity = 2.0f;
    [SerializeField] private float maxLookAngle = 7f;
    [SerializeField] private float minLookAngle = -6f;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float runSpeed = 10.0f;
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = -20.0f;

    [Header("Animator Settings")]
    private PlayerAnimation playerAnimation;

    [Header("Skill Move Settings")]
    [SerializeField] private bool isSkillMoving = false;   // 스킬 특수 이동 중인지
    [SerializeField] private bool skillUsesGravity = true; // 특수 이동에도 중력을 적용할지
    private Vector3 skillVelocity;                         // 특수 이동 속도
    private float skillMoveRemaining = 0f;                 // 남은 특수 이동 시간

    private CharacterController controller;
    private Vector3 velocity;      // 일반 이동용 수직 속도(y) 등
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private float xRotation = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    void Update()
    {
        // 마우스 회전(시야)은 특수 이동 중에도 그대로 유지
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity * Time.deltaTime);

        xRotation -= lookInput.y * lookSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);
        if (cameraMount != null)
            cameraMount.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 1) 스킬 특수 이동 중이면 여기서만 이동 처리하고 종료
        if (isSkillMoving)
        {
            UpdateSkillMove();
            return;
        }

        // 2) 일반 이동 로직 (원래 코드)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 이동 애니메이션 출력
        UpdateMoveAnimation(moveDirection);
    }

    // 외부(스킬)에서 호출할 특수 이동 시작 함수
    public void StartSkillMove(Vector3 worldDirection, float horizontalSpeed, 
        float upwardSpeed, float duration, bool useGravity = true)
    {
        if (controller == null) return;

        // 방향 정리
        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude < 0.0001f)
            worldDirection = transform.forward;
        worldDirection.Normalize();

        // 특수 이동 속도 설정
        skillVelocity = worldDirection * horizontalSpeed;
        skillVelocity.y = upwardSpeed;

        skillMoveRemaining = Mathf.Max(0f, duration);
        skillUsesGravity = useGravity;
        isSkillMoving = true;

        // 일반 이동 상태 초기화(옵션이지만 거의 필수)
        moveInput = Vector2.zero;
        velocity = Vector3.zero;

        // 애니메이션도 여기서 점프/대시 같은 걸 켜줄 수 있음
        if (playerAnimation != null)
        {
            playerAnimation.SetAnimation(AnimationType.Jump);
        }
    }

    // 특수 이동 업데이트
    private void UpdateSkillMove()
    {
        if (controller == null)
        {
            isSkillMoving = false;
            return;
        }

        // 수평 이동
        Vector3 horizontal = new Vector3(skillVelocity.x, 0f, skillVelocity.z);
        controller.Move(horizontal * Time.deltaTime);

        // 수직 이동
        if (skillUsesGravity)
        {
            skillVelocity.y += gravity * Time.deltaTime;
        }
        controller.Move(Vector3.up * skillVelocity.y * Time.deltaTime);

        // 잔여 시간 감소
        skillMoveRemaining -= Time.deltaTime;

        // 애니메이션 갱신 (지상/공중 여부는 함수에서 체크)
        UpdateMoveAnimation(horizontal);

        // 종료 조건: 시간 다 됨 또는 바닥에 착지하면서 내려가는 중
        if (skillMoveRemaining <= 0f || (controller.isGrounded && skillVelocity.y <= 0f))
        {
            isSkillMoving = false;
            skillVelocity = Vector3.zero;
        }
    }

    public void SetMoveInput(Vector2 input) => moveInput = input;
    public void SetLookInput(Vector2 input) => lookInput = input;
    public void SetRunning(bool running) => isRunning = running;

    public void Jump()
    {
        // 특수 이동 중에는 점프 막기
        if (isSkillMoving) return;

        if (controller.isGrounded)
        {
            if (playerAnimation != null)
                playerAnimation.SetAnimation(AnimationType.Jump);

            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    private void UpdateMoveAnimation(Vector3 moveDirection)
    {
        if (playerAnimation == null) return;
        if (!controller.isGrounded) return; // 점프/도약 중에는 Jump 애니메이션에 맡김

        Vector2 planar = new Vector2(moveDirection.x, moveDirection.z);

        if (planar.sqrMagnitude > 0.001f)
        {
            playerAnimation.SetAnimation(AnimationType.Run);
        }
        else
        {
            playerAnimation.SetAnimation(AnimationType.Idle);
        }
    }
}
