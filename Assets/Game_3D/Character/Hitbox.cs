using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public int damage = 1; // Sát thương mỗi cú đấm

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra nếu chạm vào vật có gắn Tag "Enemy" (con Cua)
        if (other.CompareTag("Enemy"))
        {
            // Chỉ cần tìm script CrabAI trên con cua và trừ máu
            CrabAI crabScript = other.GetComponent<CrabAI>();
            if (crabScript != null)
            {
                crabScript.TakeDamage(damage);
            }
        }
    }
}