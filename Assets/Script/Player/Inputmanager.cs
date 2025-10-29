using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private MoveController moveController;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MoveInput();
        MouseInput();
    }

    private void MoveInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 moveInput = new Vector2(horizontal, vertical);
        
        moveController.SetMoveInput(moveInput);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            moveController.Jump();
        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        moveController.SetRunning(isRunning);
    }

    
    private void MouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Vector2 mouseDelta = new Vector2(mouseX, mouseY);
        moveController.SetLookInput(mouseDelta);
    }
}