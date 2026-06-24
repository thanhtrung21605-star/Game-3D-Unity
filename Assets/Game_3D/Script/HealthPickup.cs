using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0, 50 * Time.deltaTime, 0); // Xoay trái tim cho đẹp
    }
}