using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player2DController : MonoBehaviour
{
    public static Player2DController instance;
    
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int IsGrounded = Animator.StringToHash("isGrounded");

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float airControlMultiplier = 0.8f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Grab")]
    [SerializeField] private Transform grabPoint;
    [SerializeField] private float grabRange = 0.6f;
    [SerializeField] private LayerMask grabbableLayer;
    [SerializeField] private float throwForce = 7f;

    [Header("References")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private InputSystem_Actions input;
    private Grabbable2D grabbedObject;

    private Vector2 moveInput;
    private bool jumpHeld;
    private bool isGrounded;
    private bool wasGrounded;
    
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    
    private bool isAbducted;
    private bool canMove = true;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMove;
        input.Player.Jump.performed += OnJumpPressed;
        input.Player.Jump.canceled += OnJumpReleased;
        input.Player.Grab.performed += ctx => TryGrab();
        input.Player.Grab.canceled += ctx => TryThrow();
    }

    private void OnDisable()
    {
        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMove;
        input.Player.Jump.performed -= OnJumpPressed;
        input.Player.Jump.canceled -= OnJumpReleased;
        input.Disable();
    }

    private void Update()
    {
        CheckGroundState();
        UpdateTimers();
        HandleJump();
    }

    private void FixedUpdate()
    {
        if (!CanPlayerMove())
        {
            if (rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            animator.SetBool(IsMoving, false);
            return;
        }

        HandleMovement();
        UpdateVisuals();
    }

    private void CheckGroundState()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        animator.SetBool(IsGrounded, isGrounded);

        if (isGrounded && !wasGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
    }

    private void UpdateTimers()
    {
        if (!isGrounded)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void HandleJump()
    {
        if (!CanPlayerMove()) return;

        bool canCoyoteJump = coyoteTimeCounter > 0f;
        bool hasJumpBuffer = jumpBufferCounter > 0f;

        if (hasJumpBuffer && canCoyoteJump)
        {
            PerformJump();
            jumpBufferCounter = 0f;
        }

        if (!jumpHeld && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    private void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        coyoteTimeCounter = 0f;
    }

    private void HandleMovement()
    {
        float controlMultiplier = isGrounded ? 1f : airControlMultiplier;
        float targetVelX = moveInput.x * moveSpeed * controlMultiplier;
        rb.linearVelocity = new Vector2(targetVelX, rb.linearVelocity.y);

        if (grabbedObject != null)
        {
            grabbedObject.FollowPoint(grabPoint.position);
        }
    }

    private void UpdateVisuals()
    {
        animator.SetBool(IsMoving, Mathf.Abs(rb.linearVelocity.x) > 0.1f);

        if (moveInput.x > 0.05f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < -0.05f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private bool CanPlayerMove()
    {
        if (GameManager.instance == null) return false;
        if (GameManager.instance.CurrentState != GameManager.GameState.Playing) return false;
        if (isAbducted) return false;
        if (!canMove) return false;
        return true;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (!CanPlayerMove())
        {
            moveInput = Vector2.zero;
            return;
        }
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnJumpPressed(InputAction.CallbackContext ctx)
    {
        if (!CanPlayerMove()) return;
        jumpHeld = true;
        jumpBufferCounter = jumpBufferTime;
    }

    private void OnJumpReleased(InputAction.CallbackContext ctx)
    {
        jumpHeld = false;
    }

    private void TryGrab()
    {
        if (!CanPlayerMove() || grabbedObject != null) return;

        Collider2D col = Physics2D.OverlapCircle(grabPoint.position, grabRange, grabbableLayer);
        if (col != null)
        {
            Grabbable2D grabbable = col.GetComponent<Grabbable2D>();
            if (grabbable != null)
            {
                grabbedObject = grabbable;
                grabbedObject.Grab(grabPoint, playerCollider);
            }
        }
    }

    private void TryThrow()
    {
        if (grabbedObject == null) return;

        Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        grabbedObject.Throw(dir * throwForce, playerCollider);
        grabbedObject = null;
    }

    public void BeAbducted()
    {
        isAbducted = true;
        canMove = false;
        
        moveInput = Vector2.zero;
        jumpHeld = false;
        jumpBufferCounter = 0f;
        
        ForceReleaseGrabbedObject();
        
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        Debug.Log("[Player2DController] Player abducted");
    }

    public void ReleaseFromAbduction()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        isAbducted = false;
        canMove = true;
        
        Debug.Log("[Player2DController] Player released from abduction");
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void ForceReleaseGrabbedObject()
    {
        if (grabbedObject != null)
        {
            grabbedObject.Throw(Vector2.zero, playerCollider);
            grabbedObject = null;
        }
    }

    [System.Obsolete("Use BeAbducted() and ReleaseFromAbduction() instead")]
    public void CallRespawn(bool isWaitingRespawn, Vector3 position, bool isMovingPlayer = false)
    {
        if (isWaitingRespawn)
            BeAbducted();
        else
            ReleaseFromAbduction();

        if (isMovingPlayer)
            SetPosition(position);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }

        if (grabPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(grabPoint.position, grabRange);
        }
    }
}
