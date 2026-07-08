using UnityEngine;

public class PickupEffectHandler : MonoBehaviour
{
    public GameObject pickupVFXPrefab; 
    
    [Header("=== Chỉnh vị trí hiệu ứng ===")]
    [Tooltip("Điều chỉnh tọa độ này để hiệu ứng hiển thị đúng vị trí mong muốn trên nhân vật")]
    public Vector3 effectOffset = new Vector3(0, 1.0f, 0); // Mặc định cao 1 mét

    public void PlayPickupEffect()
    {
        if (pickupVFXPrefab != null)
        {
            // Tạo hiệu ứng
            GameObject effect = Instantiate(pickupVFXPrefab, transform.position, Quaternion.identity);
            
            // Gán làm "con" để đi theo Kat
            effect.transform.SetParent(transform); 
            
            // Dùng biến offset để chỉnh vị trí
            effect.transform.localPosition = effectOffset; 
            
            // Tự hủy hiệu ứng sau 1 giây
            Destroy(effect, 1.0f);
        }
    }
}