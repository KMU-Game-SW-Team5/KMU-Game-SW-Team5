using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class MoveController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Transform cameraMount;
    [SerializeField] private float lookSensitivity = 2.0f; // inspector 기준(베이스)
    [SerializeField] private float maxLookAngle = 7f;
    [SerializeField] private float minLookAngle = -6f;

    // 런타임에 실제로 사용할 수평/수직 감도 (SettingsService로부터 갱신)
    private float horizontalLookSensitivity;
    private float verticalLookSensitivity;

    [Header("Movement Settings")]
    [SerializeField, Tooltip("기본(디폴트) 걷기 속도. 게임 시작 및 '디폴트 기준' 증감은 이 값을 기준으로 합니다.")]
    private float defaultWalkSpeed = 5.0f;

    // 런타임에서 사용하는 현재 걷기 속도(디폴트와 구분)
    private float walkSpeed;

    // 현재 이동속도를 백분율로 저장 (초기값 100 = 100%)
    private float moveSpeedPercent = 100f;

    // runSpeed는 항상 walkSpeed의 2배가 되어야 하므로 값을 직접 저장하지 않고 프로퍼티로 계산함
    private float RunSpeed => walkSpeed * 2f;

    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = -20.0f;

    [Header("Animator Settings")]
    private PlayerAnimation playerAnimation;

    [Header("Skill Move Settings")]
    [SerializeField] private bool isSkillMoving = false;   // 스킬로 인한 특수 이동 중인지
    [SerializeField] private bool skillUsesGravity = true; // 특수 이동에 중력을 적용할지 여부
    private Vector3 skillVelocity;                         // 스킬 이동용 속도 벡터
    private float skillMoveRemaining = 0f;                 // 남은 스킬 이동 시간
    private bool useSkillMoveTimer = false;                // 타이머 기반 스킬인지(대시 등), 아니면 Impulse형(도약 등)인지

    private CharacterController controller;
    private Vector3 velocity;      // 일반 이동에 쓰는 속도(특히 y축)
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private float xRotation = 0f;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] walkClips; // 걷는 소리
    [SerializeField] private AudioClip[] runClips;  // 뛰는 소리
    [SerializeField] private AudioClip jumpClip;    // 점프 소리

    [Range(0.1f, 2.0f)]
    [SerializeField] private float walkDelay = 0.6f; // 걷기 발소리 간격

    [Range(0.1f, 2.0f)]
    [SerializeField] private float runDelay = 0.35f; // 달리기 발소리 간격
    private float stepTimer = 0f;

    [Header("Leap Impulse Settings")]
    [SerializeField] private float leapImpulseMultiplier = 0.1f; // AS_Leap에서 오는 power(데미지)를 속도로 바꾸는 배율

    // 안전 범위: 0.2 ~ 3.0 (UI와 SettingsService 매핑에 따라 조정됨)
    private const float SensMin = 0.2f;
    private const float SensMax = 3.0f;

    private const float EffectiveMin = 0.0001f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        // base lookSensitivity 는 인스펙터 값 유지
        // SettingsService에 저장된 값(이미 multiplier 적용된 값)을 그대로 사용
        float mouseSens = Mathf.Clamp(SettingsService.MouseSensitivity, EffectiveMin, float.MaxValue);
        float camSens = Mathf.Clamp(SettingsService.CameraSensitivity, EffectiveMin, float.MaxValue);

        horizontalLookSensitivity = lookSensitivity * mouseSens;
        verticalLookSensitivity = lookSensitivity * camSens;

        Debug.Log($"[MoveController] Init sens: horiz={horizontalLookSensitivity:F3} vert={verticalLookSensitivity:F3} (base {lookSensitivity})");

        // 변경 이벤트 구독
        SettingsService.OnMouseSensitivityChanged += ApplyMouseSensitivity;
        SettingsService.OnCameraSensitivityChanged += ApplyCameraSensitivity;

        // 즉시 현재 저장값으로 초기화 보장
        ApplyMouseSensitivity(SettingsService.MouseSensitivity);
        ApplyCameraSensitivity(SettingsService.CameraSensitivity);
    }

    private void OnDisable()
    {
        SettingsService.OnMouseSensitivityChanged -= ApplyMouseSensitivity;
        SettingsService.OnCameraSensitivityChanged -= ApplyCameraSensitivity;
    }

    private void ApplyMouseSensitivity(float v)
    {
        float sens = Mathf.Max(EffectiveMin, v);
        horizontalLookSensitivity = lookSensitivity * sens;
        Debug.Log($"[MoveController] horizontalLookSensitivity = {horizontalLookSensitivity:F4} (setting {sens})");
    }

    private void ApplyCameraSensitivity(float v)
    {
        float sens = Mathf.Max(EffectiveMin, v);
        verticalLookSensitivity = lookSensitivity * sens;
        horizontalLookSensitivity = verticalLookSensitivity;
        Debug.Log($"[MoveController] verticalLookSensitivity = {verticalLookSensitivity:F4} (setting {sens})");
    }

    private void Start()
    {
        playerAnimation = GetComponent<PlayerAnimation>();

        if (defaultWalkSpeed < 0f) defaultWalkSpeed = 0f;

        moveSpeedPercent = 100f;
        walkSpeed = defaultWalkSpeed * (moveSpeedPercent / 100f);
    }

    void Update()
    {
        // 마우스 회전(시야)은 스킬 이동 중에도 그대로 유지
        transform.Rotate(Vector3.up * lookInput.x * horizontalLookSensitivity * Time.deltaTime);

        // minmap UI
        float yaw = transform.eulerAngles.y;
        InGameUIManager.Instance.UpdateRotation(yaw);

        xRotation -= lookInput.y * verticalLookSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);
        if (cameraMount != null)
            cameraMount.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 1) 스킬 특수 이동 중이면 일반 이동 로직을 건너뛰고 여기서만 이동 처리
        if (isSkillMoving)
        {
            UpdateSkillMove();
            return;
        }

        // 2) 일반 이동 처리
        if (controller.isGrounded && velocity.y < 0)
        {
            // 살짝 아래로 눌러줘서 땅에 붙어 있게
            velocity.y = -2f;
        }

        float currentSpeed = isRunning ? RunSpeed : walkSpeed;
        Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        HandleFootstepsWithDelay();

        // 중력 적용
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 이동 애니메이션 갱신
        UpdateMoveAnimation(moveDirection);
    }

    // 외부(스킬)에서 호출하는 "일반" 특수 이동 시작 함수 (대시 등 시간 기반 스킬용)
    public void StartSkillMove(Vector3 worldDirection, float horizontalSpeed,
        float upwardSpeed, float duration, bool useGravity = true)
    {
        if (controller == null) return;

        // 월드 방향을 수평으로 제한
        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude < 0.0001f)
            worldDirection = transform.forward;
        worldDirection.Normalize();

        // 스킬 이동 속도 설정
        skillVelocity = worldDirection * horizontalSpeed;
        skillVelocity.y = upwardSpeed;

        if (duration > 0f)
        {
            skillMoveRemaining = duration;
            useSkillMoveTimer = true;   // 시간 기반 스킬
        }
        else
        {
            skillMoveRemaining = 0f;
            useSkillMoveTimer = false;  // 시간 미사용 (착지 등으로만 끝내고 싶을 때)
        }

        skillUsesGravity = useGravity;
        isSkillMoving = true;

        // 일반 입력 이동 초기화
        moveInput = Vector2.zero;
        velocity = Vector3.zero;

        // 애니메이션: 기본적으로 점프 포즈 재생
        if (playerAnimation != null)
        {
            playerAnimation.SetAnimation(AnimationType.Jump);
        }
    }

    // 스킬 이동 업데이트
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

        // 타이머 기반 스킬이면 시간 감소
        if (useSkillMoveTimer)
        {
            skillMoveRemaining -= Time.deltaTime;
        }

        // 애니메이션 갱신 (뛰는 상태로 유지)
        UpdateMoveAnimation(horizontal);

        // 종료 조건:
        //  - 시간 기반 스킬: 시간이 끝나면 종료
        //  - Impulse형 스킬(Leap): 착지했고, 더 이상 위로 올라가는 중이 아니면 종료
        bool timeOver = useSkillMoveTimer && skillMoveRemaining <= 0f;
        bool landed = controller.isGrounded && skillVelocity.y <= 0f;

        if (timeOver || landed)
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
        // 스킬 이동 중에는 점프 금지
        if (isSkillMoving) return;

        if (controller.isGrounded)
        {
            if (playerAnimation != null)
                playerAnimation.SetAnimation(AnimationType.Jump);

            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(jumpClip);
        }
    }

    private void HandleFootstepsWithDelay()
    {
        // 입력이 없으면 발소리 타이머 리셋
        if (moveInput == Vector2.zero)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;

        float currentDelay = isRunning ? runDelay : walkDelay;

        if (stepTimer >= currentDelay)
        {
            PlayFootstepAudio();
            stepTimer = 0f;
        }
    }

    // 발소리 재생
    private void PlayFootstepAudio()
    {
        if (audioSource == null) return;
        AudioClip[] clips = isRunning ? runClips : walkClips;

        if (clips != null && clips.Length > 0)
        {
            int index = Random.Range(0, clips.Length);

            if (clips[index] != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(clips[index]);
            }
        }
    }

    private void UpdateMoveAnimation(Vector3 moveDirection)
    {
        if (playerAnimation == null) return;
        if (!controller.isGrounded) return; // 공중에서는 Jump 애니메이션 유지

        Vector2 planar = new Vector2(moveDirection.x, moveDirection.z);

        if (planar.sqrMagnitude > 0.001f)
        {
            if (isRunning)
                playerAnimation.SetAnimation(AnimationType.Run);
            else
                playerAnimation.SetAnimation(AnimationType.Walk);
        }
        else
        {
            playerAnimation.SetAnimation(AnimationType.Idle);
        }
    }

    // AS_Leap에서 호출하는 도약 함수
    // 전달받은 방향 벡터 + 수치(power)만으로 Impulse처럼 초기 속도만 준다.
    public void Leap(Vector3 direction, float power)
    {
        if (controller == null) return;

        // 방향이 0이면 전방으로 대체
        if (direction.sqrMagnitude < 0.0001f)
            direction = transform.forward;

        // AS_Leap에서 이미 각도까지 포함해서 만들어준 방향이므로 y는 건드리지 않고 그대로 사용
        direction.Normalize();

        // AddForce(direction * power, ForceMode.Impulse) 느낌:
        // power(예: 데미지)를 속도 값으로 변환하기 위해 배율(leapImpulseMultiplier)을 곱해준다.
        skillVelocity = direction * power * leapImpulseMultiplier;

        isSkillMoving = true;
        skillUsesGravity = true;

        // Impulse형: 타이머 사용 안 함 → 착지 시점에만 종료
        useSkillMoveTimer = false;
        skillMoveRemaining = 0f;

        // 일반 입력/속도 초기화
        moveInput = Vector2.zero;
        velocity = Vector3.zero;

        if (playerAnimation != null)
        {
            playerAnimation.SetAnimation(AnimationType.Jump);
        }
    }

    // 디폴트 속도를 기준으로 현재 이동 속도의 퍼센트를 변경.
    // moveSpeedPercent는 기본 100이며, AddMoveSpeed(5) 호출 시 5만큼 증가 => 105.
    // percent는 퍼센트 포인트 단위로 입력 (예: 20 => +20%).
    public void AddMoveSpeed(float percent)
    {
        moveSpeedPercent += percent;
        if (moveSpeedPercent < 0f) moveSpeedPercent = 0f;

        // 퍼센트 변경에 따라 현재 walkSpeed 갱신 (기본 속도를 기준으로)
        walkSpeed = defaultWalkSpeed * (moveSpeedPercent / 100f);
    }

    // 현재 이동속도 퍼센트 반환 (예: 105)
    public float GetMoveSpeedPercent() => moveSpeedPercent;

    // 현재 이동속도 퍼센트를 텍스트로 반환. 예: "105%"
    public string GetMoveSpeedText() => GetMoveSpeedPercent().ToString("F0") + "%";
}