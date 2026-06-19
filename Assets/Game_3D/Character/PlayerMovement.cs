using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 6f;
    public float climbSpeed = 3f; // Tốc độ leo

    public Collider punchHitbox;
    public float attackDelay = 0.08f; // Delay trước khi hitbox gây damage
    public float attackCooldown = 0.14f; // Thời gian hồi trước khi đánh lại, có thể điều chỉnh trong Inspector

    public float fallThreshold = -10f;
    public int maxHealth = 3;
    public int currentHealth;
    private bool isDead = false;

    public int coinCount = 0;
    public int starCount = 0;
    public int crystalCount = 0;
    public int keyCount = 0;

    private Animator animator;
    private Rigidbody rb;
    private bool isGrounded = true;
    private bool isAttacking = false;
    private bool isClimbing = false; // Trạng thái leo thang

    private Transform mainCamera;

    private const string AnimatorParamIsRunning = "IsRunning";
    private const string AnimatorParamJumpTrigger = "JumpTrigger";
    private const string AnimatorParamAttackTrigger = "AttackTrigger";
    private const string AnimatorParamHitTrigger = "HitTrigger";
    private const string AnimatorParamDeathTrigger = "DeathTrigger";

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        if (Camera.main != null) mainCamera = Camera.main.transform;
    }

    void Update()
    {
        if (isDead) return;

        if (transform.position.y < fallThreshold)
        {
            Die();
            return;
        }

        // --- XỬ LÝ LEO THANG ---
        float verticalInput = Input.GetAxis("Vertical");
        if (isClimbing)
        {
            rb.velocity = new Vector3(rb.velocity.x, verticalInput * climbSpeed, rb.velocity.z);
            return; // Dừng các logic bên dưới khi đang leo
        }

        // --- DI CHUYỂN BÌNH THƯỜNG ---
        float horizontalInput = Input.GetAxis("Horizontal");

        Vector3 movementDirection = Vector3.zero;
        if (mainCamera != null)
        {
            Vector3 cameraForward = mainCamera.forward;
            Vector3 cameraRight = mainCamera.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();
            movementDirection = (cameraForward * verticalInput) + (cameraRight * horizontalInput);
        }

        if (movementDirection.magnitude > 1f) movementDirection.Normalize();
        transform.Translate(movementDirection * speed * Time.deltaTime, Space.World);

        if (movementDirection != Vector3.zero) transform.forward = movementDirection;

        bool isMoving = (horizontalInput != 0 || verticalInput != 0);
        SetAnimatorBool(AnimatorParamIsRunning, isMoving);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            SetAnimatorTrigger(AnimatorParamJumpTrigger);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        if (Input.GetMouseButtonDown(1) && !isAttacking)
        {
            SetAnimatorTrigger(AnimatorParamAttackTrigger);
            if (punchHitbox != null) StartCoroutine(ActivateHitbox());
        }

        if (Input.GetKeyDown(KeyCode.E)) // Nhấn E để gạt
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f))
        {
            Lever lever = hit.collider.GetComponent<Lever>();
            if (lever != null) lever.Interact();
        }
    }
    }

    // --- PHẦN XỬ LÝ LEO THANG (TRIGGER) ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isClimbing = true;
            rb.useGravity = false; // Tắt trọng lực để không bị rơi khi leo
            rb.velocity = Vector3.zero;
        }

        if (other.CompareTag("Coin")) { coinCount++; Destroy(other.gameObject); }
        else if (other.CompareTag("Star")) { starCount++; Destroy(other.gameObject); CheckWinCondition(); }
        else if (other.CompareTag("Crystal")) { crystalCount++; Destroy(other.gameObject); CheckWinCondition(); }
        else if (other.CompareTag("Key")) { keyCount++; Destroy(other.gameObject); CheckWinCondition(); }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isClimbing = true;
            rb.useGravity = false; // Tắt trọng lực để không bị rơi khi leo
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isClimbing = false;
            rb.useGravity = true; // Bật lại trọng lực khi rời thang
        }
    }

    private bool HasAnimatorParameter(string parameterName)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == parameterName) return true;
        }
        return false;
    }

    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (animator == null) return;
        if (HasAnimatorParameter(parameterName))
        {
            animator.SetBool(parameterName, value);
        }
        else
        {
                Debug.LogWarning($"Animator parameter '{parameterName}' not found on '{gameObject.name}'. Add it to the Animator controller.");
        }
    }

    private void SetAnimatorTrigger(string parameterName)
    {
        if (animator == null) return;
        if (HasAnimatorParameter(parameterName))
        {
            animator.SetTrigger(parameterName);
        }
        else
        {
            Debug.LogWarning($"Animator parameter '{parameterName}' not found on '{gameObject.name}'. Add it to the Animator controller.");
        }
    }

    // --- CÁC HÀM CŨ GIỮ NGUYÊN ---
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        SetAnimatorTrigger(AnimatorParamHitTrigger);
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        SetAnimatorTrigger(AnimatorParamDeathTrigger);
        StartCoroutine(RestartGameRoutine());
    }

    private IEnumerator RestartGameRoutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator ActivateHitbox()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackDelay);
        punchHitbox.enabled = true;
        yield return new WaitForSeconds(0.2f);
        punchHitbox.enabled = false;
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    private void CheckWinCondition()
    {
        if (starCount >= 1 && crystalCount >= 3 && keyCount >= 1)
            Debug.Log("🎉 CHÚC MỪNG! BẠN ĐÃ ĐỦ ĐIỀU KIỆN QUA MÀN! 🎉");
    }
}