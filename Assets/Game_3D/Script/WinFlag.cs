using UnityEngine;

public class WinFlag : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem người chơi đã chạm vào cờ chưa
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                // Gọi hàm kiểm tra xem đã đủ vật phẩm chưa
                if (player.starCount >= 1 && player.crystalCount >= 3 && player.keyCount >= 1)
                {
                    Debug.Log("🎉 CHÚC MỪNG! BẠN ĐÃ VỀ ĐÍCH THÀNH CÔNG! 🎉");
                    // Ở đây bạn có thể thêm lệnh load màn hình "Bạn Thắng" hoặc "Level sau"
                }
                else
                {
                    Debug.Log("⚠️ Bạn chưa thu thập đủ vật phẩm! Cần: 1 Sao, 3 Pha lê, 1 Chìa khóa.");
                }
            }
        }
    }
}