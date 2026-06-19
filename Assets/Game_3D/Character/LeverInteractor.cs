using UnityEngine;

[RequireComponent(typeof(Transform))]
public class LeverInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 2f;
    public bool useRaycast = true; // if true, raycasts from camera center; otherwise uses proximity sphere
    public Camera raycastCamera;

    void Start()
    {
        if (raycastCamera == null) raycastCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Try raycast first (look-to-interact)
            if (useRaycast)
            {
                Ray ray;
                if (raycastCamera != null)
                    ray = raycastCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
                else
                    ray = new Ray(transform.position, transform.forward);

                if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
                {
                    Lever lever = hit.collider.GetComponent<Lever>();
                    if (lever != null)
                    {
                        lever.Interact();
                        return;
                    }
                }
            }

            // Fallback: proximity overlap
            Collider[] cols = Physics.OverlapSphere(transform.position, interactRange);
            foreach (var c in cols)
            {
                Lever lever = c.GetComponent<Lever>();
                if (lever != null)
                {
                    lever.Interact();
                    return;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
