using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrollingEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool startMovingRight = true;

    [Header("Edge Detection")] 
    [SerializeField] private Transform leftLimit, rightLimit;
    
    [Header("Damage")]
    [SerializeField] private int damageAmount = 1;

    private Rigidbody2D rb;
    private bool movingRight;
    [SerializeField] private int direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movingRight = startMovingRight;
    }

    private void FixedUpdate()
    {
        Move();
        CheckForTurn();
    }

    private void Move()
    {
        direction = movingRight ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        transform.localScale = new Vector3(direction, 1f, 1f);
    }

    private void CheckForTurn()
    {
        bool shouldTurn = false;

        if (direction == 1)
        {
            if(transform.position.x >= rightLimit.position.x)
                shouldTurn = true;
        }
        else if (direction == -1)
        {
            if (transform.position.x <= leftLimit.position.x)
                shouldTurn = true;
        }


        if (shouldTurn)
        {
            Flip();
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }
}
