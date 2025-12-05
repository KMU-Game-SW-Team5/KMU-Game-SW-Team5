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

    private float RunSpeed => walkSpeed * 2f;

    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = -20.0f;

    [Header("Animator Settings")]
    private PlayerAnimation playerAnimation;

    [Header("Skill Move Settings")]
    [SerializeField] private bool isSkillMoving = false;
    [SerializeField] private bool skillUsesGravity = true;
    private Vector3 skillVelocity;
    private float skillMoveRemaining = 0f;
    private bool useSkillMoveTimer = false;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private float xRotation = 0f;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] walkClips;
    [SerializeField] private AudioClip[] runClips;
    [SerializeField] private AudioClip jumpClip;

    [Range(0.1f, 2.0f)]
    [SerializeField] private float walkDelay = 0.6f;

    [Range(0.1f, 2.0f)]
    [SerializeField] private float runDelay = 0.35f;
    private float stepTimer = 0f;

    [Header("Leap Impulse Settings")]
    [SerializeField] private float leapImpulseMultiplier = 0.1f;

    // 안전 범위: 0.1 ~ 10.0 (슬라이더와 동일하게)
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
        // base lookSensitivity는 인스펙터 값 유지
        // SettingsService에 저장된 값(이미 multiplier 적용된 값)을 그대로 사용
        float mouseSens = Mathf.Max(EffectiveMin, SettingsService.MouseSensitivity);
        float camSens = Mathf.Max(EffectiveMin, SettingsService.CameraSensitivity);

        horizontalLookSensitivity = lookSensitivity * mouseSens;
        verticalLookSensitivity = lookSensitivity * camSens;

        Debug.Log($"[MoveController] Init sens: horiz={horizontalLookSensitivity:F4} vert={verticalLookSensitivity:F4} (base {lookSensitivity})");

        SettingsService.OnMouseSensitivityChanged += ApplyMouseSensitivity;
        SettingsService.OnCameraSensitivityChanged += ApplyCameraSensitivity;
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
        // 마우스 회전 적용
        transform.Rotate(Vector3.up * lookInput.x * horizontalLookSensitivity * Time.deltaTime);

        float yaw = transform.eulerAngles.y;
        InGameUIManager.Instance.UpdateRotation(yaw);

        xRotation -= lookInput.y * verticalLookSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);
        if (cameraMount != null)
            cameraMount.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (isSkillMoving)
        {
            UpdateSkillMove();
            return;
        }

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float currentSpeed = isRunning ? RunSpeed : walkSpeed;
        Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        HandleFootstepsWithDelay();

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        UpdateMoveAnimation(moveDirection);
    }

    public void StartSkillMove(Vector3 worldDirection, float horizontalSpeed,
        float upwardSpeed, float duration, bool useGravity = true)
    {
        if (controller == null) return;

        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude < 0.0001f)
            worldDirection = transform.forward;
        worldDirection.Normalize();

        skillVelocity = worldDirection * horizontalSpeed;
        skillVelocity.y = upwardSpeed;

        if (duration > 0f)
        {
            skillMoveRemaining = duration;
            useSkillMoveTimer = true;
        }
        else
        {
            skillMoveRemaining = 0f;
            useSkillMoveTimer = false;
        }

        skillUsesGravity = useGravity;
        isSkillMoving = true;

        moveInput = Vector2.zero;
        velocity = Vector3.zero;

        if (playerAnimation != null)
        {
            playerAnimation.SetAnimation(AnimationType.Jump);
        }
    }

    private void UpdateSkillMove()
    {
        if (controller == null)
        {
            isSkillMoving = false;
            return;
        }

        Vector3 horizontal = new Vector3(skillVelocity.x, 0f, skillVelocity.z);
        controller.Move(horizontal * Time.deltaTime);

        if (skillUsesGravity)
        {
            skillVelocity.y += gravity * Time.deltaTime;
        }
        controller.Move(Vector3.up * skillVelocity.y * Time.deltaTime);

        if (useSkillMoveTimer)
        {
            skillMoveRemaining -= Time.deltaTime;
        }

        UpdateMoveAnimation(horizontal);

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
        if (!controller.isGrounded) return;

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

    public void Leap(Vector3 direction, float power)
    {
        if (controller == null) return;

        if (direction.sqrMagnitude < 0.0001f)
            direction = transform.forward;

        direction.Normalize();

        skillVelocity = direction * power * leapImpulseMultiplier;

        isSkillMoving = true;
        skillUsesGravity = true;

        useSkillMoveTimer = false;
        skillMoveRemaining = 0f;
            
        moveInput = Vector2.zero;
        velocity = Vector3.zero;

        if (playerAnimation != null)
        {
            playerAnimation.SetAnimation(AnimationType.Jump);
        }
    }

    public void AddMoveSpeed(float percent)
    {
        moveSpeedPercent += percent;
        if (moveSpeedPercent < 0f) moveSpeedPercent = 0f;
        walkSpeed = defaultWalkSpeed * (moveSpeedPercent / 100f);
    }

    public float GetMoveSpeedPercent() => moveSpeedPercent;
    public string GetMoveSpeedText() => GetMoveSpeedPercent().ToString("F0") + "%";
}