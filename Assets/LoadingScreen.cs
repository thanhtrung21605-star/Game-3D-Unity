using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string targetLevel = "Level 1";
    [SerializeField] private float totalLoadTime = 3f; // Tổng thời gian load

    [Header("UI")]
    [SerializeField] private Image progressBar; 
    [SerializeField] private Text textPercent;

    private void Start()
    {
        StartCoroutine(LoadSimulation());
    }

    private IEnumerator LoadSimulation()
    {
        // Khởi tạo
        float timer = 0f;
        progressBar.fillAmount = 0f;

        // Bắt đầu load ngầm
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetLevel);
        operation.allowSceneActivation = false;

        // Vòng lặp chạy thanh loading
        while (timer < totalLoadTime)
        {
            timer += Time.deltaTime;
            float progress = timer / totalLoadTime;

            // Tùy biến cách hiển thị % (Nhảy số hoặc mượt)
            UpdateUI(progress);

            yield return null;
        }

        // Đợi load xong hoàn toàn rồi chuyển
        progressBar.fillAmount = 1f;
        textPercent.text = "100%";
        operation.allowSceneActivation = true;
    }

    private void UpdateUI(float progress)
    {
        // NẾU BẠN MUỐN NHẢY MỐC (VD: 20-50-100)
        // int displayPercent = 0;
        // if (progress < 0.3f) displayPercent = 20;
        // else if (progress < 0.7f) displayPercent = 50;
        // else displayPercent = 100;
        // textPercent.text = displayPercent + "%";

        // NẾU BẠN MUỐN CHẠY MƯỢT TỪ 0-100
        progressBar.fillAmount = progress;
        textPercent.text = Mathf.RoundToInt(progress * 100) + "%";
    }
}