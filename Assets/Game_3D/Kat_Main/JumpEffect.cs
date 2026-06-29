using UnityEngine;

public class JumpEffect : MonoBehaviour
{
    public GameObject jumpEffectPrefab; // Kéo Prefab hiệu ứng của bạn vào đây
    public float effectOffset = 0.05f; // Chỉnh để hiệu ứng không bị chìm vào sàn

    public void SpawnJumpEffect()
    {
        if (jumpEffectPrefab != null)
        {
            // Sinh hiệu ứng ngay tại vị trí chân của Kat
            Vector3 pos = transform.position + new Vector3(0, effectOffset, 0);
            GameObject effect = Instantiate(jumpEffectPrefab, pos, Quaternion.Euler(90, 0, 0));
            Destroy(effect, 1.0f); // Tự xóa sau 1 giây
        }
    }
}