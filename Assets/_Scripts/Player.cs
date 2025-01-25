using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{   
    [SerializeField] private InputActionReference moveInput;
    [SerializeField] private InputActionReference lookInput;
    [SerializeField] private Transform followTarget;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float mouseSensitivityHorizontal = 100f;
    [SerializeField] private float mouseSensitivityVertical = 100f;
    [SerializeField] private float minVerticalLookAngle = -45f;
    [SerializeField] private float maxVerticalLookAngle = 70f;

    private CharacterController controller;
    private PlayerInput playerInput;
    
    private Vector3 velocity; // Tracks vertical movement (gravity)
    private Vector2 moveInputValue; // Tracks input from the Input Actions
    private Vector2 lookInputValue;
    private float cameraTiltValue; // Tracks vertical camera rotation
    
    public Transform FollowTarget => followTarget;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        playerInput.enabled = false;
        playerInput.enabled = true;
        
        moveInput.action.Enable();
        moveInput.action.performed += OnMoveInput_Performed;
        moveInput.action.canceled += OnMoveInput_Canceled;
        
        lookInput.action.Enable();
        lookInput.action.performed += OnRotateInput_Performed;
        lookInput.action.canceled += OnRotateInput_Canceled;
    }

    private void OnDisable()
    {
        moveInput.action.Disable();
        moveInput.action.performed -= OnMoveInput_Performed;
        moveInput.action.canceled -= OnMoveInput_Canceled;
        
        lookInput.action.Disable();
        lookInput.action.performed -= OnRotateInput_Performed;
        lookInput.action.canceled -= OnRotateInput_Canceled;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Convert input movement (Vector2) into world space movement
        Vector3 move = transform.right * moveInputValue.x + transform.forward * moveInputValue.y;

        // Apply movement speed and move the character
        controller.Move(move * (moveSpeed * Time.deltaTime));

        // Apply gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }
        velocity.y += gravity * Time.deltaTime;

        // Apply gravity-based downward movement
        controller.Move(velocity * Time.deltaTime);
    }
    
    private void HandleMouseLook()
    {
        // Apply sensitivity scaling
        float mouseX = lookInputValue.x * mouseSensitivityHorizontal * Time.deltaTime;
        float mouseY = lookInputValue.y * mouseSensitivityVertical * Time.deltaTime;

        // Rotate the player along the Y-axis (horizontal rotation)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera along the X-axis (vertical rotation)
        cameraTiltValue -= mouseY; // Invert y-axis for natural mouse movement
        cameraTiltValue = Mathf.Clamp(cameraTiltValue, minVerticalLookAngle, maxVerticalLookAngle); // Clamp rotation to avoid over-rotation

        // Apply the calculated rotation to the camera
        followTarget.localRotation = Quaternion.Euler(cameraTiltValue, 0f, 0f);
    }

    // Event callback when movement input is performed
    private void OnMoveInput_Performed(InputAction.CallbackContext context)
    {
        moveInputValue = context.ReadValue<Vector2>();
    }

    // Event callback when movement input is canceled (no movement)
    private void OnMoveInput_Canceled(InputAction.CallbackContext context)
    {
        moveInputValue = Vector2.zero;
    }

    private void OnRotateInput_Performed(InputAction.CallbackContext obj)
    {
        lookInputValue = obj.ReadValue<Vector2>();
    }

    private void OnRotateInput_Canceled(InputAction.CallbackContext obj)
    {
        lookInputValue = Vector2.zero;
    }
}