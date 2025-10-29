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
    [SerializeField] private Animator animator;


    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private float xRotation = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
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
        }
    }
}