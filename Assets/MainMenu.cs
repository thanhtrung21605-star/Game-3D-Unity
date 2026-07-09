using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenu : MonoBehaviour
{
    // Gán hàm này vào nút PLAY
    public void PlayGame()
    {
        // Thay vì vào thẳng "Level 1", ta chuyển sang màn hình chờ trước
        SceneManager.LoadScene("LoadingScene"); 
    }

    // Gán hàm này vào nút QUIT
    public void QuitGame()
    {
        Debug.Log("Game đã thoát");
        Application.Quit(); 
    }
}