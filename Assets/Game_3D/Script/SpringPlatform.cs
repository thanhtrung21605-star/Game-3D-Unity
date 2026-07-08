using System.Collections;
using UnityEngine;

public class SpringPlatform : MonoBehaviour
{
    // Lực bật nảy cho nhân vật
    public float bounceForce = 15f; 

    // Các biến cho hiệu ứng nén
    public float compressAmount = 0.4f; 
    public float bounceSpeed = 15f;     

    // [MỚI] Âm thanh lò xo
    [Header("Audio Settings")]
    public AudioClip springSound;

    private Vector3 originalScale;      
    private bool isBouncing = false;    

    void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // [MỚI] Phát âm thanh khi va chạm
            if (springSound != null)
            {
                AudioSource.PlayClipAtPoint(springSound, transform.position, 1.0f);
            }

            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

            if (player != null && playerRb != null)
            {
                // Kích hoạt trạng thái đang dùng lò xo để khóa nút nhảy
                player.isUsingSpring = true;

                // Xóa gia tốc rơi hiện tại và đẩy nhân vật lên
                playerRb.velocity = new Vector3(playerRb.velocity.x, 0f, playerRb.velocity.z);
                playerRb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);

                // Khởi động hiệu ứng lò xo và thời gian hồi phục cho nhân vật
                if (!isBouncing)
                {
                    StartCoroutine(SquishEffect());
                }
                
                // Sau 0.5s cho phép nhảy trở lại
                StartCoroutine(ResetSpringState(player));
            }
        }
    }

    private IEnumerator ResetSpringState(PlayerMovement player)
    {
        yield return new WaitForSeconds(0.5f);
        player.isUsingSpring = false;
    }

    private IEnumerator SquishEffect()
    {
        isBouncing = true;

        Vector3 compressedScale = new Vector3(originalScale.x, originalScale.y * compressAmount, originalScale.z);

        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime * bounceSpeed;
            transform.localScale = Vector3.Lerp(originalScale, compressedScale, progress);
            yield return null;
        }

        progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime * bounceSpeed;
            transform.localScale = Vector3.Lerp(compressedScale, originalScale, progress);
            yield return null;
        }

        transform.localScale = originalScale;
        isBouncing = false;
    }
}