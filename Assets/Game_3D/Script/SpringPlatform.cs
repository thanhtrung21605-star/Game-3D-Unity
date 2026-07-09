using System.Collections;
using UnityEngine;

public class SpringPlatform : MonoBehaviour
{
    // Lực bật nảy cho nhân vật
    public float bounceForce = 15f; 

    // --- PHẦN MỚI: CÁC BIẾN CHO HIỆU ỨNG NÉN ---
    public float compressAmount = 0.4f; // Độ lún (0.4 nghĩa là lún xuống còn 40% chiều cao)
    public float bounceSpeed = 15f;     // Tốc độ nén xuống và nảy lên

    private Vector3 originalScale;      // Lưu lại kích thước gốc của lò xo
    private bool isBouncing = false;    // Ngăn chặn việc lò xo bị lỗi nếu dẫm lên liên tục

    void Start()
    {
        // Ghi nhớ kích thước chuẩn của lò xo ngay khi bắt đầu game
        originalScale = transform.localScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // Xóa gia tốc rơi hiện tại và đẩy nhân vật lên
                playerRb.velocity = new Vector3(playerRb.velocity.x, 0f, playerRb.velocity.z);
                playerRb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);

                // Nếu lò xo đang đứng im thì bắt đầu chạy hiệu ứng lún
                if (!isBouncing)
                {
                    StartCoroutine(SquishEffect());
                }
            }
        }
    }

    // --- HÀM TẠO HIỆU ỨNG NÉN VÀ NẢY BẰNG COROUTINE ---
    private IEnumerator SquishEffect()
    {
        isBouncing = true;

        // Tính toán kích thước khi bị bẹp (Chỉ bẹp trục Y, giữ nguyên X và Z)
        Vector3 compressedScale = new Vector3(originalScale.x, originalScale.y * compressAmount, originalScale.z);

        // 1. Ép lò xo lún xuống thật nhanh
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime * bounceSpeed;
            // Dùng Lerp để chuyển đổi mượt mà từ kích thước gốc sang kích thước bẹp
            transform.localScale = Vector3.Lerp(originalScale, compressedScale, progress);
            yield return null; // Đợi 1 khung hình rồi lặp lại
        }

        // 2. Bật lò xo giãn trở lại như cũ
        progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime * bounceSpeed;
            transform.localScale = Vector3.Lerp(compressedScale, originalScale, progress);
            yield return null;
        }

        // Đảm bảo lò xo quay về đúng 100% kích thước gốc để không bị sai lệch
        transform.localScale = originalScale;
        isBouncing = false;
    }
}