using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private MoveController moveController;

    private bool movable = true;

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
        if (!movable)
        {
            moveController.SetMoveInput(Vector2.zero);
            return;
        }
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
        if (!movable)
        {
            moveController.SetLookInput(Vector2.zero);
            return;
        }
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Vector2 mouseDelta = new Vector2(mouseX, mouseY);

        moveController.SetLookInput(mouseDelta);
    }

    public void SetMovable(bool movable) { this.movable = movable; }
    public bool GetMovable() => movable;
}