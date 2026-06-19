using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    public Transform targetPosition; // Vị trí muốn vật thể di chuyển đến
    public float moveSpeed = 2f;
    public float waitAtTarget = 3f; // thời gian đứng yên khi tới đích trước khi quay về
    private bool shouldMove = false;
    private Vector3 startPosition;
    private bool movingToTarget = true;
    private bool waiting = false;
    private Vector3 currentTargetPos;
    private System.Collections.IEnumerator waitCoroutine = null;

    void Update()
    {
        if (shouldMove)
        {
            float dist = Vector3.Distance(transform.position, currentTargetPos);
            transform.position = Vector3.MoveTowards(transform.position, currentTargetPos, moveSpeed * Time.deltaTime);

            // reached destination
            if (dist <= 0.01f)
            {
                if (movingToTarget && !waiting)
                {
                    waiting = true;
                    Debug.Log($"ObjectMover reached target on {gameObject.name}, waiting {waitAtTarget}s before returning.");
                    waitCoroutine = WaitThenReturn();
                    StartCoroutine(waitCoroutine);
                }
                else if (!movingToTarget)
                {
                    // returned to start; wait then start next cycle back to target (repeat) unless stopped
                    if (!waiting)
                    {
                        waiting = true;
                        Debug.Log($"ObjectMover reached start on {gameObject.name}, waiting {waitAtTarget}s before going to target.");
                        waitCoroutine = WaitThenGoToTarget();
                        StartCoroutine(waitCoroutine);
                    }
                }
            }
        }
    }

    void Start()
    {
        startPosition = transform.position;
        if (targetPosition == null)
        {
            Debug.LogWarning($"ObjectMover on {gameObject.name} has no targetPosition assigned.");
        }
        else if (targetPosition == transform)
        {
            Debug.LogWarning($"ObjectMover on {gameObject.name}: targetPosition is the same object; platform will not move. Assign a different Transform (e.g. an empty GameObject) as the destination.");
            targetPosition = null;
        }
        currentTargetPos = targetPosition != null ? targetPosition.position : startPosition;
    }

    // Hàm này sẽ được gán vào sự kiện onActivate của cần gạt
    public void StartMoving()
    {
        if (targetPosition == null)
        {
            Debug.LogWarning($"ObjectMover.StartMoving called but targetPosition is null on {gameObject.name}.");
            return;
        }

        shouldMove = true;
        movingToTarget = true;
        waiting = false;
        currentTargetPos = targetPosition.position;
        Debug.Log($"ObjectMover.StartMoving called on {gameObject.name}");
    }

    // Hàm này có thể được gán vào onDeactivate để dừng di chuyển
    public void StopMoving()
    {
        shouldMove = false;
        // cancel any pending wait coroutine
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
        waiting = false;
        movingToTarget = true;
        currentTargetPos = startPosition;
        Debug.Log($"ObjectMover.StopMoving called on {gameObject.name}");
    }

    private System.Collections.IEnumerator WaitThenReturn()
    {
        yield return new WaitForSeconds(waitAtTarget);
        waiting = false;
        movingToTarget = false;
        currentTargetPos = startPosition;
        Debug.Log($"ObjectMover starting return to start on {gameObject.name}.");
        waitCoroutine = null;
    }

    private System.Collections.IEnumerator WaitThenGoToTarget()
    {
        yield return new WaitForSeconds(waitAtTarget);
        waiting = false;
        movingToTarget = true;
        if (targetPosition != null)
        {
            currentTargetPos = targetPosition.position;
            Debug.Log($"ObjectMover starting next cycle to target on {gameObject.name}.");
        }
        else
        {
            // no target -> stop
            shouldMove = false;
            Debug.LogWarning($"ObjectMover on {gameObject.name}: targetPosition is null when trying to go to target.");
        }
        waitCoroutine = null;
    }
}