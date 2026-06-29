using UnityEngine;

public class AutoMovingPlatform : MonoBehaviour
{
    [Header("=== Cấu Hình Di Chuyển ===")]
    public Vector3 movementDirection = Vector3.right; // Hướng di chuyển (ví dụ Vector3.right là trục X)
    public float distance = 5f;                       // Khoảng cách di chuyển
    public float speed = 2f;                          // Tốc độ

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Di chuyển qua lại bằng hàm Sine để nó mượt mà
        transform.position = startPosition + movementDirection.normalized * Mathf.Sin(Time.time * speed) * distance;
    }
}