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
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Grab")]
    [SerializeField] private Transform grabPoint;    
    [SerializeField] private float grabRange = 0.6f; 
    [SerializeField] private LayerMask grabbableLayer;
    [SerializeField] private float throwForce = 7f;

    private Rigidbody2D rb;
    private InputSystem_Actions input; 
    private Vector2 moveInput;
    private bool jumpRequested;
    private bool isGrounded;
    private bool grabHeld;
    
    [SerializeField] Animator animator;

    private Grabbable2D grabbedObject;
    
    private Collider2D playerCollider;

    private bool waitingRespawn = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
        
        rb = GetComponent<Rigidbody2D>();

        input = new InputSystem_Actions();
        
        playerCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        
        Debug.Log("[Player2DController] Awake completed");
    }

    public void CallRespawn(bool isWaitingRespawn, Vector3 position, bool isMovingPlayer = false)
    {
        Debug.Log($"[Player2DController] CallRespawn - Waiting: {isWaitingRespawn}, Position: {position}, Moving: {isMovingPlayer}");
        waitingRespawn = isWaitingRespawn;
        
        if (waitingRespawn)
        {
            moveInput = Vector2.zero;
            jumpRequested = false;
            grabHeld = false;
        }
        
        if (isMovingPlayer)
            transform.position = position;
    }

    public void PlayerRigidbody(bool isKinematic)
    {
        RigidbodyType2D newType = isKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        Debug.Log($"[Player2DController] Changing Rigidbody from {rb.bodyType} to {newType}");
        rb.bodyType = newType;
        
        if (!isKinematic)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMove;

        input.Player.Jump.performed += OnJump;

        input.Player.Grab.performed += OnGrabPerformed;
        input.Player.Grab.canceled += OnGrabCanceled;
        
        Debug.Log("[Player2DController] Input enabled");
    }

    private void OnDisable()
    {
        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMove;

        input.Player.Jump.performed -= OnJump;

        input.Player.Grab.performed -= OnGrabPerformed;
        input.Player.Grab.canceled -= OnGrabCanceled;

        input.Disable();
        
        Debug.Log("[Player2DController] Input disabled");
    }

    private void Update()
    {
        CheckGround();

        if (!CanControlPlayer())
        {
            return;
        }

        if (grabHeld && grabbedObject == null)
        {
            TryGrab();
        }
        else if (!grabHeld && grabbedObject != null)
        {
            Throw();
        }
    }

    private void FixedUpdate()
    {
        if (!CanControlPlayer())
        {
            if (rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            animator.SetBool(IsMoving, false);
            return;
        }

        float targetVelX = moveInput.x * moveSpeed;
        rb.linearVelocity = new Vector2(targetVelX, rb.linearVelocity.y);

        animator.SetBool(IsMoving, Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        
        if (jumpRequested)
        {
            jumpRequested = false;
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }

        if (grabbedObject != null)
        {
            grabbedObject.FollowPoint(grabPoint.position);
        }

        if (moveInput.x > 0.05f) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < -0.05f) transform.localScale = new Vector3(-1, 1, 1);
    }

    private bool CanControlPlayer()
    {
        if (GameManager.instance == null) return false;
        
        if (GameManager.instance.CurrentState != GameManager.GameState.Playing)
        {
            return false;
        }
        
        if (waitingRespawn)
        {
            return false;
        }
        
        return true;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (!CanControlPlayer())
        {
            moveInput = Vector2.zero;
            Debug.Log("[Player2DController] Move blocked - cannot control player");
            return;
        }

        moveInput = ctx.ReadValue<Vector2>();
        Debug.Log($"[Player2DController] Move input: {moveInput}");
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!CanControlPlayer())
        {
            jumpRequested = false;
            Debug.Log("[Player2DController] Jump blocked - cannot control player");
            return;
        }

        if (ctx.performed)
        {
            jumpRequested = true;
            Debug.Log("[Player2DController] Jump requested");
        }
    }

    private void OnGrabPerformed(InputAction.CallbackContext ctx)
    {
        if (!CanControlPlayer())
        {
            grabHeld = false;
            return;
        }

        grabHeld = true;
    }

    private void OnGrabCanceled(InputAction.CallbackContext ctx)
    {
        if (!CanControlPlayer())
        {
            grabHeld = false;
            return;
        }

        grabHeld = false;
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        animator.SetBool(IsGrounded, isGrounded);
    }

    private void TryGrab()
    {
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

    private void Throw()
    {
        Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        grabbedObject.Throw(dir * throwForce, playerCollider); 
        grabbedObject = null;
    }
    
    public void ForceReleaseGrabbedObject()
    {
        if (grabbedObject != null)
        {
            Debug.Log("[Player2DController] Force releasing grabbed object");
            grabbedObject.Throw(Vector2.zero, playerCollider);
            grabbedObject = null;
            grabHeld = false;
        }
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
