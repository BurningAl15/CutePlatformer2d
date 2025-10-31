using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Grabbable2D : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;
    private bool isGrabbed;
    private Transform currentParent;
    
    [SerializeField] SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite baseSprite, grabbedSprite;
    
    public bool IsGrabbed => isGrabbed;
    

    private bool isKey;
    [SerializeField] private int id;
    public int ID => id;

    private Vector2 initialPoint;

    [SerializeField] private GameObject instructions;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialPoint = transform.position;
    }

    public void Grab(Transform grabPoint, Collider2D playerCollider)
    {
        isGrabbed = true;
        currentParent = grabPoint;

        spriteRenderer.sprite = grabbedSprite;
        col.isTrigger = true;
        instructions.SetActive(false);

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;

        transform.SetParent(grabPoint);
        transform.localPosition = Vector3.zero;

        if (playerCollider != null && col != null)
        {
            Physics2D.IgnoreCollision(col, playerCollider, true);
        }
    }

    public void FollowPoint(Vector3 point)
    {
        if (!isGrabbed) return;
    }

    public void Throw(Vector2 force, Collider2D playerCollider)
    {
        isGrabbed = false;

        spriteRenderer.sprite = baseSprite;
        instructions.SetActive(true);
        transform.SetParent(null);
        
        col.isTrigger = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        // rb.AddForce(force, ForceMode2D.Impulse);

        if (playerCollider != null && col != null)
        {
            Physics2D.IgnoreCollision(col, playerCollider, false);
        }
    }

    public void ResetPositionToInitial()
    {
        transform.SetParent(null);
        transform.position = initialPoint;
    }
}