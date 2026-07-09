using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc để chuyển cảnh

public class PauseMenu : MonoBehaviour 
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI; // Kéo Panel Pause vào đây trong Inspector

    void Update()
    {
        // Nhấn phím Escape để đóng/mở menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Thời gian chạy lại bình thường
        GameIsPaused = false;
        
        // Khóa chuột lại khi chơi tiếp
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Dừng thời gian trong game
        GameIsPaused = true;
        
        // Hiện chuột để chọn menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Restart()
    {
        Time.timeScale = 1f; // Phải reset timeScale về 1 trước khi load lại
        GameIsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Load lại màn hiện tại
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f; // Reset thời gian
        GameIsPaused = false;
        SceneManager.LoadScene("Main Menu"); // Lưu ý: Tên scene phải khớp với Build Settings
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}