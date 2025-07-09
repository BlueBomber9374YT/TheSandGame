using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;
    private Transform cameraTransform;
    public bool ShiftToggle = false;

   
    private Rigidbody rb;
    public float MoveSpeed = 5f;
    private float moveHorizontal;
    private float moveForward;

   
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f; 
    public float ascendMultiplier = 2f; 
    private bool isGrounded = true;
    public LayerMask groundLayer;
    private float groundCheckTimer = 0f;
    private float groundCheckDelay = 0.3f;
    private float playerHeight;
    private float raycastDistance;
    Animator animator;
    private bool Shift;
    [HideInInspector] public StaminaController _staminaController;

    void Start()
    {
        _staminaController = GetComponent<StaminaController>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        cameraTransform = Camera.main.transform;

        
        playerHeight = GetComponent<CapsuleCollider>().height * transform.localScale.y;
        raycastDistance = (playerHeight / 2) + 0.2f;

       
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void setRunSpeed(float speed)
    {
       MoveSpeed  = speed;
    }
    void Update()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveForward = Input.GetAxisRaw("Vertical");

        RotateCamera();

        if(Input.GetKeyDown(KeyCode.LeftShift) && moveForward > 0)
        {
            if (ShiftToggle)
                ShiftToggle = false;
            else
                ShiftToggle = true;
        }

        if (moveForward <= 0)
            ShiftToggle = false;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        
        if (!isGrounded && groundCheckTimer <= 0f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, raycastDistance, groundLayer);
        }
        else
        {
            groundCheckTimer -= Time.deltaTime;
        }

    }

    void FixedUpdate()
    {
        MovePlayer();
        ApplyJumpPhysics();
    }

    void MovePlayer()
    {
        if (MoveSpeed <= 5)
        {
            _staminaController.weAreSprinting = false;
        }
        if (MoveSpeed >= 10)
        {
            _staminaController.weAreSprinting = true;
            _staminaController.sprinting();
        }
        else
        {
            MoveSpeed = 5;
        }

        Shift = Input.GetKeyDown(KeyCode.LeftShift);

        Vector3 movement = (transform.right * moveHorizontal + transform.forward * moveForward).normalized;
        Vector3 targetVelocity = movement * MoveSpeed;

        
        Vector3 velocity = rb.linearVelocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;
        rb.linearVelocity = velocity;




        if (isGrounded && moveHorizontal == 0 && moveForward == 0)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            animator.SetFloat("Speed_f", 0);

        }

        else if (ShiftToggle && _staminaController.playerStamina > 1)
        {
            animator.SetFloat("Speed_f", 3);
            MoveSpeed = 10;
        }

        else
        {
            ShiftToggle = false;
            animator.SetFloat("Speed_f", 1);
            MoveSpeed = 5;
        }
    }
       

    void RotateCamera()
    {
        float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, horizontalRotation, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    public void PlayerJump()
    {
        isGrounded = false;
        groundCheckTimer = groundCheckDelay;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    void Jump()
    {
        _staminaController.StaminaJump();

    }

    void ApplyJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            
            rb.linearVelocity += Vector3.up * Physics.gravity.y * fallMultiplier * Time.fixedDeltaTime;
        } 
        else if (rb.linearVelocity.y > 0)
        {
           
            rb.linearVelocity += Vector3.up * Physics.gravity.y * ascendMultiplier * Time.fixedDeltaTime;

        }
    }
}