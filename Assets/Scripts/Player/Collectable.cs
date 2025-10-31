using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Header("Collectable Settings")]
    [SerializeField] private int pointValue = 1;
    [SerializeField] private bool autoRegister = true;

    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;

    private Vector3 startPosition;
    private bool isCollected = false;

    private void Start()
    {
        startPosition = transform.position;

        if (autoRegister && LevelManager.instance != null)
        {
            LevelManager.instance.RegisterCollectable(this);
        }
    }

    private void Update()
    {
        if (isCollected) return;

        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        isCollected = true;

        if (LevelManager.instance != null)
        {
            LevelManager.instance.CollectItem();
        }

        Destroy(gameObject);
    }
}