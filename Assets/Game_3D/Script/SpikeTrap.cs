using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public int damage = 1; // Sát thương mỗi lần dẫm trúng

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem vật thể dẫm vào có gắn mác "Player" không
        if (other.CompareTag("Player"))
        {
            // Lấy não bộ của nhân vật và gọi lệnh trừ máu
            PlayerMovement playerScript = other.GetComponent<PlayerMovement>();
            
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
                Debug.Log("Á! Dẫm trúng bẫy gai rồi!");
            }
        }
    }
}