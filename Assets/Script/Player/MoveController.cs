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
    [SerializeField] private Animator animator;


    private CharacterController controller;
    private Vector3 velocity;
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

    void Update()
    {
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity * Time.deltaTime);

        xRotation -= lookInput.y * lookSensitivity * Time.deltaTime;
        
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);
        
        cameraMount.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

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
    }

    public void SetMoveInput(Vector2 input) => moveInput = input;
    public void SetLookInput(Vector2 input) => lookInput = input;
    public void SetRunning(bool running) => isRunning = running;
    public void Jump()
    {
        if (controller.isGrounded)
        {
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
                Debug.Log($"[Sound] 발소리 재생! (시간: {Time.time:F2}초 / 파일명: {clips[index].name})");
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(clips[index]);
            }
        }
    }
    
}