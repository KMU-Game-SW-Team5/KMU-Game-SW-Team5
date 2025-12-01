using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
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
    [SerializeField] private bool isSkillMoving = false;   // ��ų Ư�� �̵� ������
    [SerializeField] private bool skillUsesGravity = true; // Ư�� �̵����� �߷��� ��������
    private Vector3 skillVelocity;                         // Ư�� �̵� �ӵ�
    private float skillMoveRemaining = 0f;                 // ���� Ư�� �̵� �ð�

    private CharacterController controller;
    private Vector3 velocity;      // �Ϲ� �̵��� ���� �ӵ�(y) ��
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private float xRotation = 0f;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] walkClips; // 걷는 소리 (여러 개 넣어서 랜덤 재생)
    [SerializeField] private AudioClip[] runClips;  // 뛰는 소리
    [SerializeField] private AudioClip jumpClip;    // 점프 소리


    [Range(0.1f, 2.0f)] 
    [SerializeField] private float walkDelay = 0.6f; // 걷을 때 소리 간격 (초)
    
    [Range(0.1f, 2.0f)] 
    [SerializeField] private float runDelay = 0.35f; // 뛸 때 소리 간격 (초)
    private float stepTimer = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (audioSource == null) 
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    void Update()
    {
        // ���콺 ȸ��(�þ�)�� Ư�� �̵� �߿��� �״�� ����
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity * Time.deltaTime);

        // minmap UI
        float yaw = transform.eulerAngles.y;
        InGameUIManager.Instance.UpdateRotation(yaw);

        xRotation -= lookInput.y * lookSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);
        if (cameraMount != null)
            cameraMount.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 1) ��ų Ư�� �̵� ���̸� ���⼭�� �̵� ó���ϰ� ����
        if (isSkillMoving)
        {
            UpdateSkillMove();
            return;
        }

        // 2) �Ϲ� �̵� ���� (���� �ڵ�)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        HandleFootstepsWithDelay();

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // �̵� �ִϸ��̼� ���
        UpdateMoveAnimation(moveDirection);
    }

    // �ܺ�(��ų)���� ȣ���� Ư�� �̵� ���� �Լ�
    public void StartSkillMove(Vector3 worldDirection, float horizontalSpeed, 
        float upwardSpeed, float duration, bool useGravity = true)
    {
        if (controller == null) return;

        // ���� ����
        worldDirection.y = 0f;
        if (worldDirection.sqrMagnitude < 0.0001f)
            worldDirection = transform.forward;
        worldDirection.Normalize();

        // Ư�� �̵� �ӵ� ����
        skillVelocity = worldDirection * horizontalSpeed;
        skillVelocity.y = upwardSpeed;

        skillMoveRemaining = Mathf.Max(0f, duration);
        skillUsesGravity = useGravity;
        isSkillMoving = true;

        // �Ϲ� �̵� ���� �ʱ�ȭ(�ɼ������� ���� �ʼ�)
        moveInput = Vector2.zero;
        velocity = Vector3.zero;

        // �ִϸ��̼ǵ� ���⼭ ����/��� ���� �� ���� �� ����
        if (playerAnimation != null)
        {
            playerAnimation.SetAnimation(AnimationType.Jump);
        }
    }

    // Ư�� �̵� ������Ʈ
    private void UpdateSkillMove()
    {
        if (controller == null)
        {
            isSkillMoving = false;
            return;
        }

        // ���� �̵�
        Vector3 horizontal = new Vector3(skillVelocity.x, 0f, skillVelocity.z);
        controller.Move(horizontal * Time.deltaTime);

        // ���� �̵�
        if (skillUsesGravity)
        {
            skillVelocity.y += gravity * Time.deltaTime;
        }
        controller.Move(Vector3.up * skillVelocity.y * Time.deltaTime);

        // �ܿ� �ð� ����
        skillMoveRemaining -= Time.deltaTime;

        // �ִϸ��̼� ���� (����/���� ���δ� �Լ����� üũ)
        UpdateMoveAnimation(horizontal);

        // ���� ����: �ð� �� �� �Ǵ� �ٴڿ� �����ϸ鼭 �������� ��
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
        // Ư�� �̵� �߿��� ���� ����
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

    // 오디오 재생 함수
    private void PlayFootstepAudio()
    {
        if (audioSource == null) return;
        AudioClip[] clips = isRunning ? runClips : walkClips;
        
        if (clips != null && clips.Length > 0)
        {
            int index = Random.Range(0, clips.Length);
            
            if (clips[index] != null)
            {
                Debug.Log("발소리 출력중");
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(clips[index]);
            }
        }
    }

    private void UpdateMoveAnimation(Vector3 moveDirection)
    {
        if (playerAnimation == null) return;
        if (!controller.isGrounded) return; // ����/���� �߿��� Jump �ִϸ��̼ǿ� �ñ�

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
