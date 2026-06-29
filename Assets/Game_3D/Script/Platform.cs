using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Khi nhân vật đứng lên nền tảng, cho nhân vật làm "con" của nền tảng
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Khi nhân vật rời nền tảng, bỏ gán "con"
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}