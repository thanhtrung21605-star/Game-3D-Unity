using UnityEngine;

public class Coin : MonoBehaviour
{
    public float spinSpeed = 150f;
    
    [Header("=== Hiệu ứng tại vật phẩm ===")]
    public GameObject collectEffect; 
    public AudioClip collectSound;   

    // [MỚI] Biến tĩnh để giữ trạng thái cao độ (pitch) cho toàn bộ chuỗi xu
    private static float currentPitch = 1.0f;
    private static float lastPlayTime = 0f;

    void Update()
    {
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Tạo hiệu ứng nổ hạt tại vị trí đồng xu
            if (collectEffect != null)
            {
                GameObject effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
                Destroy(effect, 1.0f);
            }

            // 2. Phát âm thanh mượt mà với Pitch Shifting
            if (collectSound != null)
            {
                // Nếu nhặt liên tục trong vòng 0.2s, tăng pitch
                if (Time.time - lastPlayTime < 0.2f)
                {
                    currentPitch += 0.1f; 
                }
                else
                {
                    currentPitch = 1.0f; // Reset nếu ngắt quãng
                }

                currentPitch = Mathf.Clamp(currentPitch, 1.0f, 1.5f);
                lastPlayTime = Time.time;

                // Tạo đối tượng âm thanh tạm thời
                GameObject audioObj = new GameObject("CoinSound");
                AudioSource source = audioObj.AddComponent<AudioSource>();
                source.clip = collectSound;
                source.pitch = currentPitch;
                source.spatialBlend = 1.0f; // Để âm thanh 3D tại vị trí đồng xu
                source.transform.position = transform.position;
                source.Play();
                
                Destroy(audioObj, collectSound.length);
            }

            // 3. Gọi hiệu ứng trên nhân vật
            PickupEffectHandler effectHandler = other.GetComponent<PickupEffectHandler>();
            if (effectHandler != null)
            {
                effectHandler.PlayPickupEffect();
            }

            // 4. Logic cộng xu
            // GameController.instance.AddCoin(1);

            // 5. Xóa đồng xu
            Destroy(gameObject);
        }
    }
}