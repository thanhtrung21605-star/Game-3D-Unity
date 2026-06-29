using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; 
    public float distance = 5f; 
    public float sensitivity = 3f; 

    public float minY = -15f;
    public float maxY = 60f;

    private float currentX = 0f;
    private float currentY = 0f;

    void Start()
    {
        // [MỚI] Khởi tạo góc nhìn dựa trên vị trí camera hiện tại trong Scene
        if (target != null)
        {
            Vector3 relativePos = transform.position - target.position;
            currentX = transform.eulerAngles.y;
            currentY = transform.eulerAngles.x;
            
            // Nếu góc X > 180, chuyển sang giá trị âm để Clamp hoạt động đúng
            if (currentY > 180) currentY -= 360;
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            currentX += Input.GetAxis("Mouse X") * sensitivity;
            currentY -= Input.GetAxis("Mouse Y") * sensitivity;

            currentY = Mathf.Clamp(currentY, minY, maxY);
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 direction = new Vector3(0, 0, -distance);
            
            transform.position = target.position + rotation * direction;
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
    }
}