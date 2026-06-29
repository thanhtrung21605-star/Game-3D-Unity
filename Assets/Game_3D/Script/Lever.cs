using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [Header("=== Cấu hình Cần Gạt ===")]
    public Transform handle;           
    public float rotationSpeed = 10f;  
    public float rotationAngle = 180f; // [MỚI] Chỉnh độ xoay tại đây
    public bool isActivated = false;
    public UnityEvent onActivate;      
    public UnityEvent onDeactivate;    

    private Quaternion startRotation;
    private Quaternion targetRotation;
    private bool isRotating = false;

    void Start()
    {
        if (handle != null)
        {
            startRotation = handle.localRotation;
            targetRotation = startRotation;
        }
    }

    void Update()
    {
        if (isRotating && handle != null)
        {
            handle.localRotation = Quaternion.Lerp(handle.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
            if (Quaternion.Angle(handle.localRotation, targetRotation) < 0.1f)
            {
                isRotating = false;
            }
        }
    }

    public void Interact()
    {
        isActivated = !isActivated;
        isRotating = true;

        if (isActivated)
        {
            // Xoay quanh trục Z theo số độ bạn nhập vào
            targetRotation = startRotation * Quaternion.Euler(0, 0, rotationAngle); 
            onActivate?.Invoke();
        }
        else
        {
            targetRotation = startRotation; 
            onDeactivate?.Invoke();
        }
    }
}