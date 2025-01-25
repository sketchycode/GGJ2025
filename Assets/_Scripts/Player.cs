using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    private CharacterController controller;
    
    [SerializeField] private InputActionReference moveInput;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    private Vector3 velocity; // Tracks vertical movement (gravity)
    private Vector2 inputMovement; // Tracks input from the Input Actions

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        // Enable the input actions
        moveInput.action.Enable();

        // Subscribe to the Move action
        moveInput.action.performed += OnMoveInput_Performed;
        moveInput.action.canceled += OnMoveInput_Canceled;
    }

    private void OnDisable()
    {
        // Disable the input actions
        moveInput.action.Disable();

        // Unsubscribe from the Move action
        moveInput.action.performed -= OnMoveInput_Performed;
        moveInput.action.canceled -= OnMoveInput_Canceled;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Convert input movement (Vector2) into world space movement
        Vector3 move = transform.right * inputMovement.x + transform.forward * inputMovement.y;

        // Apply movement speed and move the character
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Apply gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }
        velocity.y += gravity * Time.deltaTime;

        // Apply gravity-based downward movement
        controller.Move(velocity * Time.deltaTime);
    }

    // Event callback when movement input is performed
    private void OnMoveInput_Performed(InputAction.CallbackContext context)
    {
        inputMovement = context.ReadValue<Vector2>();
    }

    // Event callback when movement input is canceled (no movement)
    private void OnMoveInput_Canceled(InputAction.CallbackContext context)
    {
        inputMovement = Vector2.zero;
    }
}