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
        // Mình đã xóa lệnh khóa và làm tàng hình chuột ở đây
        // Bây giờ con trỏ chuột của bạn sẽ luôn hiển thị bình thường
    }

    void Update()
    {
        // Kiểm tra xem người chơi có đang BẤM GIỮ chuột trái hay không
        // Số 0 là chuột trái, 1 là chuột phải, 2 là nút cuộn giữa
        if (Input.GetMouseButton(0))
        {
            // Chỉ khi đang giữ chuột trái thì mới lấy tín hiệu di chuyển chuột
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