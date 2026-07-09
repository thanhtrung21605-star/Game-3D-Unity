using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    // Cách 1: Chuyển thẳng đến một màn cố định (dùng cho nút Next Level)
    public void LoadNextLevel()
    {
        SceneManager.LoadScene("Level 2");
    }

    // Cách 2: Chuyển đến bất kỳ màn nào dựa trên tên (linh hoạt hơn)
    // Bạn có thể nhập tên màn trực tiếp trong Inspector của Unity
    public void LoadGameLevel(string levelName)
    {
        if (!string.IsNullOrEmpty(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.LogError("Tên màn chơi không hợp lệ!");
        }
    }
}