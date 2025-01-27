using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{   
    [Header("Input Settings")]
    [SerializeField] private InputActionReference moveInput;
    [SerializeField] private InputActionReference lookInput;
    [SerializeField] private InputActionReference interactInput;
    [SerializeField] private InputActionReference sprintInput;
    
    [Header("Internal References")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Animator animator;
    
    [Header("Interaction Settings")]
    [SerializeField] private Bubble bubblePrefab; // Bubble prefab to instantiate
    [SerializeField] private float interactRange = 3f; // Maximum range for interaction
    [SerializeField] private LayerMask interactionLayer; 

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Mouse Settings")]
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
    private readonly Collider[] hitColliders = new Collider[10];
    private IInteractable closestInteractable;
    private bool isSprinting;
    
    public Transform FollowTarget => followTarget;
    public IInteractable ClosestInteractable => closestInteractable;

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
        
        interactInput.action.Enable();
        interactInput.action.performed += OnInteractInput_Performed;
        
        sprintInput.action.Enable();
        sprintInput.action.performed += OnSprintInput_Performed;
        sprintInput.action.canceled += OnSprintInput_Canceled;
    }

    private void OnDisable()
    {
        moveInput.action.Disable();
        moveInput.action.performed -= OnMoveInput_Performed;
        moveInput.action.canceled -= OnMoveInput_Canceled;
        
        lookInput.action.Disable();
        lookInput.action.performed -= OnRotateInput_Performed;
        lookInput.action.canceled -= OnRotateInput_Canceled;
        
        interactInput.action.Disable();
        interactInput.action.performed -= OnInteractInput_Performed;
        
        sprintInput.action.Disable();
        sprintInput.action.performed -= OnSprintInput_Performed;
        sprintInput.action.canceled -= OnSprintInput_Canceled;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
        
        animator.SetBool("isGrounded", controller.isGrounded);
        animator.SetFloat("horizontalSpeed", controller.velocity.ToXZ().sqrMagnitude);
        
        GetClosestInteractable();
    }

    private void HandleMovement()
    {
        Vector3 move = transform.right * moveInputValue.x + transform.forward * moveInputValue.y;

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = gravity;
        }
        velocity.y += gravity * Time.deltaTime;
        move *= isSprinting ? sprintSpeed : moveSpeed;
        move.y = velocity.y;
        controller.Move(move * Time.deltaTime);
    }
    
    private void HandleMouseLook()
    {
        float mouseX = lookInputValue.x * mouseSensitivityHorizontal * Time.deltaTime;
        float mouseY = lookInputValue.y * mouseSensitivityVertical * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        cameraTiltValue -= mouseY;
        cameraTiltValue = Mathf.Clamp(cameraTiltValue, minVerticalLookAngle, maxVerticalLookAngle);
        followTarget.localRotation = Quaternion.Euler(cameraTiltValue, 0f, 0f);
    }
    
    private void GetClosestInteractable()
    {
        var hitCount = Physics.OverlapSphereNonAlloc(followTarget.position, interactRange, hitColliders, interactionLayer);
        closestInteractable = null;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < hitCount && i < hitColliders.Length; i++)
        {
            var hitCollider = hitColliders[i];
            Vector3 directionToObject = (hitCollider.transform.position - followTarget.position).normalized;

            float angle = Vector3.Angle(transform.forward, directionToObject);
            if (angle <= 45f) // either side of the forward vector
            {
                float distance = Vector3.Distance(followTarget.position, hitCollider.transform.position);
                IInteractable interactable = hitCollider.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract && distance < closestDistance)
                {
                    closestInteractable = interactable;
                    closestDistance = distance;
                }
            }
        }
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

    private void OnInteractInput_Performed(InputAction.CallbackContext obj)
    {
        closestInteractable?.Interact();
    }

    private void OnSprintInput_Performed(InputAction.CallbackContext obj)
    {
        isSprinting = true;
    }

    private void OnSprintInput_Canceled(InputAction.CallbackContext obj)
    {
        isSprinting = false;
    }
}