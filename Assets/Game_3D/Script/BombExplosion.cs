using UnityEngine;

public class BombExplosion : MonoBehaviour
{
    [Header("=== Cấu Hình Bom ===")]
    public GameObject explosionVFX; // Kéo hiệu ứng nổ vào đây
    public int damage = 2;          // Sát thương gây ra là 2
    public float explosionDelay = 0.1f; // Thời gian trễ trước khi xóa bom

    private bool hasExploded = false;

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem vật va chạm có phải là Player không
        if (other.CompareTag("Player") && !hasExploded)
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;

        // 1. Tạo hiệu ứng nổ
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        // 2. Gây sát thương cho Player
        // Giả sử script nhận sát thương của Player tên là "PlayerHealth" hoặc có hàm "TakeDamage"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        // 3. Xóa quả bom khỏi màn hình
        Destroy(gameObject, explosionDelay);
    }
}