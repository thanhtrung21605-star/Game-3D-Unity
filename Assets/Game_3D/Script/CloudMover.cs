using UnityEngine;

public class ContinuousCloudChain : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float speed = 1f;
    public float loopDistance = 100f; // Khoảng cách giữa 2 đám mây khi nối đuôi

    [Header("Đối tượng mây")]
    public Transform cloudA;
    public Transform cloudB;

    [Header("Giới hạn reset")]
    public float resetThreshold = 50f; // Khi Z vượt quá giá trị này thì reset

    void Update()
    {
        // 1. Cho cả 2 mây cùng trôi
        Vector3 movement = Vector3.forward * speed * Time.deltaTime;
        cloudA.position += movement;
        cloudB.position += movement;

        // 2. Logic nối đuôi: Nếu Cloud A bay quá xa, đưa nó về sau Cloud B
        if (cloudA.position.z > resetThreshold) 
        {
            cloudA.position = new Vector3(cloudA.position.x, cloudA.position.y, cloudB.position.z - loopDistance);
        }

        // 3. Nếu Cloud B bay quá xa, đưa nó về sau Cloud A
        if (cloudB.position.z > resetThreshold) 
        {
            cloudB.position = new Vector3(cloudB.position.x, cloudB.position.y, cloudA.position.z - loopDistance);
        }
    }
}