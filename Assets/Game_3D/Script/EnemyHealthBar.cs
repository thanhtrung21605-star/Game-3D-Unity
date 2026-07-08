using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealthBar : MonoBehaviour
{
    public Image fillImage;
    public GameObject canvasObject;
    public float hideDelay = 3.0f;
    private Coroutine hideCoroutine;

    void Start() {
        if(canvasObject != null) canvasObject.SetActive(false);
    }

    void LateUpdate() {
        // Luôn luôn đối diện Camera theo trục Y để không bị lật ngược
        if(Camera.main != null) {
            Vector3 directionToCamera = transform.position - Camera.main.transform.position;
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }

    public void ShowAndHealth(float currentHP, float maxHP) {
        Debug.Log("HealthBar: Đang cập nhật máu cho quái..."); // [KIỂM TRA]
        
        if(canvasObject != null) {
            canvasObject.SetActive(true);
            Debug.Log("HealthBar: Canvas đã được bật!");
        }
        
        if(fillImage != null) {
            fillImage.fillAmount = currentHP / maxHP;
        }

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay() {
        yield return new WaitForSeconds(hideDelay);
        if(canvasObject != null) {
            canvasObject.SetActive(false);
            Debug.Log("HealthBar: Đã tự ẩn.");
        }
    }
}