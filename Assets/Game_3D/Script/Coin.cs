using UnityEngine;

public class Coin : MonoBehaviour
{
    public float spinSpeed = 150f; // Tốc độ xoay

    void Update()
    {
        // Xoay đồng xu quanh trục Y
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
    }
}