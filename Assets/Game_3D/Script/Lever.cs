using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [Header("Cấu hình Cần Gạt")]
    public bool isActivated = false;
    public UnityEvent onActivate;   // Sự kiện chạy khi gạt xuống
    public UnityEvent onDeactivate; // Sự kiện chạy khi gạt lên

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Hàm này được gọi khi nhân vật nhấn phím tương tác (ví dụ phím 'E')
    public void Interact()
    {
        isActivated = !isActivated; // Đảo trạng thái

        Debug.Log($"Lever.Interact: {gameObject.name} newState={isActivated}");

        if (anim != null) anim.SetBool("IsOn", isActivated);

        if (isActivated)
        {
            onActivate?.Invoke(); // Chạy các hàm đã gán ở sự kiện onActivate
        }
        else
        {
            onDeactivate?.Invoke();
        }
    }
}