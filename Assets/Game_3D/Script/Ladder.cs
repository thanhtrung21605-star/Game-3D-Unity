using UnityEngine;

public class Ladder : MonoBehaviour
{
    // Điểm chính giữa của thang để nhân vật đứng vào đó
    public Transform climbPosition; 

    void Start()
    {
        // Nếu bạn chưa tạo điểm climbPosition, tự tạo một điểm ở giữa thang
        if (climbPosition == null)
        {
            GameObject point = new GameObject("ClimbPoint");
            point.transform.SetParent(transform);
            point.transform.localPosition = Vector3.zero;
            climbPosition = point.transform;
        }
    }
}