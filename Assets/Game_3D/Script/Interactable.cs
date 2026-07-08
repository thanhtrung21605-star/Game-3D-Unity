using UnityEngine;

public class Interactable : MonoBehaviour
{
    public GameObject promptUI; // Kéo Text InteractPrompt vào đây
    public float interactionRange = 2.0f;
    public Transform player;

    void Update()
    {
        if (player == null) {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if(p != null) player = p.transform;
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        
        // Nếu lại gần thì hiện chữ, đi xa thì tắt chữ
        if (dist <= interactionRange) {
            if (!promptUI.activeSelf) promptUI.SetActive(true);
        } else {
            if (promptUI.activeSelf) promptUI.SetActive(false);
        }
    }
}