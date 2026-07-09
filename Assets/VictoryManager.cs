using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    public GameObject victoryPanel; // Kéo Canvas Victory vào đây

    private void Start()
    {
        victoryPanel.SetActive(false); // Đảm bảo ẩn khi mới bắt đầu
    }

    // Hàm này sẽ được gọi khi bạn thắng game
    public void ShowVictoryScreen()
    {
        victoryPanel.SetActive(true);
        Time.timeScale = 0f; // Dừng thời gian game (tùy chọn)
    }

    public void OnNextLevelClick()
    {
        Time.timeScale = 1f;
        // Ví dụ chuyển sang Level 2
        SceneManager.LoadScene("Level 2"); 
    }

    public void OnHomeClick()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
    public class PlayerController : MonoBehaviour
{
    // Kéo VictoryManager vào đây trong Inspector
    public VictoryManager victoryManager; 

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem có chạm vào vạch đích không
        if (other.CompareTag("Finish")) 
        {
            // GỌI SCRIPT CHIẾN THẮNG Ở ĐÂY
            if (victoryManager != null)
            {
                victoryManager.ShowVictoryScreen();
            }
        }
    }
}

}