using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 
using UnityEngine.UI; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float jumpForce = 6f;
    public float climbSpeed = 3f;

    [Header("Combat Settings")]
    public Collider punchHitbox;
    public int punchDamage = 1; 
    public Collider skillHitbox; 
    public int skillDamage = 3; 

    [Header("Spawn & Jump Effects")] 
    public GameObject spawnEffectPrefab; 
    public Vector3 spawnOffset = new Vector3(0, 0.5f, 0); 
    public float spawnEffectDuration = 1.0f;
    public GameObject jumpEffectPrefab; // [TÍCH HỢP]

    public GameObject attackEffectPrefab; 
    public Transform effectSpawnPoint;
    public float effectScale = 0.5f; 

    [Header("Skill Q Effect")]
    public GameObject skillEffectPrefab;
    public Transform skillEffectSpawnPoint;
    public float skillEffectScale = 1.0f;

    public TrailRenderer dashTrail; 

    [Header("Item UI")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI crystalText;
    public TextMeshProUGUI keyText;

    [Header("Skill Cooldown UI")] 
    public Image skillCooldownImage;            
    public TextMeshProUGUI skillCooldownText; 

    [Header("Heart Health UI")] 
    public Image[] heartFills; 

    [Header("Win UI")]
    public GameObject winTextObject;

    [Header("Hit Effect Settings")]
    public Renderer katRenderer; 
    public float flashDuration = 0.2f;
    public Color flashColor = Color.red;
    public GameObject hitEffectPrefab; 
    public Transform hitEffectSpawnPoint;

    [Header("Win Condition Settings")]
    public int requiredCoins = 25;
    public int requiredCrystals = 3;
    public int requiredKeys = 1;

    public float attackDelay = 0.08f;
    public float attackCooldown = 0.14f;

    public float fallThreshold = -10f;
    public int maxHealth = 6; 
    public int currentHealth;
    private bool isDead = false;
    private bool isWon = false; 

    public int coinCount = 0;
    public int starCount = 0;
    public int crystalCount = 0;
    public int keyCount = 0;

    private float idleTimer = 0f;
    public float timeToWait = 5f; 
    private const string AnimatorParamIdle2Trigger = "PlayIdle2";
    
    private bool isRunning = false;
    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f;
    private const string AnimatorParamSpeed = "Speed";

    private bool isUsingSkill = false;
    private float lastSkillTime = -10f; 
    public float skillCooldown = 7f;

    private Animator animator;
    private Rigidbody rb;
    private bool isGrounded = true;
    private bool isAttacking = false;
    private bool isClimbing = false;

    private Transform mainCamera;

    private const string AnimatorParamIsRunning = "IsRunning";
    private const string AnimatorParamJumpTrigger = "JumpTrigger";
    private const string AnimatorParamAttackTrigger = "AttackTrigger";
    private const string AnimatorParamHitTrigger = "HitTrigger";
    private const string AnimatorParamDeathTrigger = "DeathTrigger";
    private const string AnimatorParamSkillTrigger = "SpecialSkillTrigger"; 

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        
        if (katRenderer == null) katRenderer = GetComponentInChildren<Renderer>();
        
        if (spawnEffectPrefab != null)
        {
            GameObject spawnEffect = Instantiate(spawnEffectPrefab, transform.position + spawnOffset, Quaternion.identity);
            Destroy(spawnEffect, spawnEffectDuration);
        }
        
        if (skillHitbox != null) skillHitbox.enabled = false;
        if (dashTrail != null) dashTrail.emitting = false; 
        if (skillCooldownImage != null) skillCooldownImage.fillAmount = 0f;
        if (skillCooldownText != null) skillCooldownText.gameObject.SetActive(false);
        if (winTextObject != null) winTextObject.SetActive(false);
        if (Camera.main != null) mainCamera = Camera.main.transform;
        
        UpdateUI(); 
        UpdateHeartUI(); 
    }

    void Update()
    {
        if (isDead || isWon) return; 
        if (transform.position.y < fallThreshold) { Die(); return; }

        if (Time.time < lastSkillTime + skillCooldown)
        {
            float timeRemaining = (lastSkillTime + skillCooldown) - Time.time;
            if (skillCooldownImage != null) skillCooldownImage.fillAmount = timeRemaining / skillCooldown;
            if (skillCooldownText != null)
            {
                skillCooldownText.gameObject.SetActive(true);
                skillCooldownText.text = timeRemaining.ToString("F0");
            }
        }
        else
        {
            if (skillCooldownImage != null) skillCooldownImage.fillAmount = 0f;
            if (skillCooldownText != null) skillCooldownText.gameObject.SetActive(false);
        }

        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        if (isUsingSkill) return; 

        if (isClimbing)
        {
            rb.velocity = new Vector3(rb.velocity.x, verticalInput * climbSpeed, rb.velocity.z);
            return;
        }

        bool isMoving = (horizontalInput != 0 || verticalInput != 0);
        if (!isMoving) isRunning = false;

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (Time.time - lastTapTime <= doubleTapThreshold) isRunning = !isRunning;
            lastTapTime = Time.time;
        }

        Vector3 movementDirection = Vector3.zero;
        if (mainCamera != null)
        {
            Vector3 cameraForward = mainCamera.forward;
            Vector3 cameraRight = mainCamera.right;
            cameraForward.y = 0f; cameraRight.y = 0f;
            cameraForward.Normalize(); cameraRight.Normalize();
            movementDirection = (cameraForward * verticalInput) + (cameraRight * horizontalInput);
        }

        if (!isMoving && isGrounded)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= timeToWait) { SetAnimatorTrigger(AnimatorParamIdle2Trigger); idleTimer = 0f; }
        } else { idleTimer = 0f; }

        float targetSpeed = isMoving ? (isRunning ? 1.0f : 0.5f) : 0f;
        if (animator != null) animator.SetFloat(AnimatorParamSpeed, targetSpeed);

        if (movementDirection.magnitude > 1f) movementDirection.Normalize();
        transform.Translate(movementDirection * speed * (isRunning ? 1.5f : 1f) * Time.deltaTime, Space.World);

        if (movementDirection != Vector3.zero) transform.forward = movementDirection;

        SetAnimatorBool(AnimatorParamIsRunning, isMoving);

        // [TÍCH HỢP HIỆU ỨNG NHẢY]
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            SetAnimatorTrigger(AnimatorParamJumpTrigger);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            
            if (jumpEffectPrefab != null)
            {
                GameObject effect = Instantiate(jumpEffectPrefab, transform.position + new Vector3(0, 0.05f, 0), Quaternion.Euler(90, 0, 0));
                Destroy(effect, 1.0f);
            }
        }

        if (Input.GetMouseButtonDown(1) && !isAttacking)
        {
            SetAnimatorTrigger(AnimatorParamAttackTrigger);
            if (punchHitbox != null) StartCoroutine(ActivateHitbox());
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 2f))
            {
                Lever lever = hit.collider.GetComponent<Lever>();
                if (lever != null) lever.Interact();
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isUsingSkill && Time.time >= lastSkillTime + skillCooldown)
        {
            StartCoroutine(SkillSequence());
        }
    }

    public void PlayAttackEffect()
    {
        if (attackEffectPrefab != null && effectSpawnPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
            effect.transform.localScale = new Vector3(effectScale, effectScale, effectScale);
            Destroy(effect, 1.0f);
        }
    }

    private IEnumerator SkillSequence()
    {
        isUsingSkill = true;
        lastSkillTime = Time.time;
        
        SetAnimatorTrigger(AnimatorParamSkillTrigger);
        if (dashTrail != null) dashTrail.emitting = true; 
        yield return StartCoroutine(SkillMovementRoutine());
        
        yield return new WaitForSeconds(0.7f); 

        if (skillEffectPrefab != null && skillEffectSpawnPoint != null)
        {
            GameObject effect = Instantiate(skillEffectPrefab, skillEffectSpawnPoint.position, skillEffectSpawnPoint.rotation);
            effect.transform.localScale = new Vector3(skillEffectScale, skillEffectScale, skillEffectScale);
            Destroy(effect, 1.0f);
        }

        if (skillHitbox != null) skillHitbox.enabled = true;
        yield return new WaitForSeconds(0.3f); 
        
        if (skillHitbox != null) skillHitbox.enabled = false;
        if (dashTrail != null) dashTrail.emitting = false; 
        
        isUsingSkill = false;
    }

    private IEnumerator SkillMovementRoutine()
    {
        float duration = 0.4f; 
        float distance = 2.5f; 
        Vector3 direction = transform.forward;
        float elapsed = 0;
        while (elapsed < duration)
        {
            transform.position += direction * (distance / duration) * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder")) { isClimbing = true; rb.useGravity = false; rb.velocity = Vector3.zero; }
        else if (other.CompareTag("Coin")) { coinCount++; Destroy(other.gameObject); UpdateUI(); }
        else if (other.CompareTag("Star")) { starCount++; Destroy(other.gameObject); }
        else if (other.CompareTag("Crystal")) { crystalCount++; Destroy(other.gameObject); UpdateUI(); }
        else if (other.CompareTag("Key")) { keyCount++; Destroy(other.gameObject); UpdateUI(); }
        else if (other.CompareTag("Heart")) { Heal(1); Destroy(other.gameObject); }
        
        if (other.CompareTag("Finish")) { if (CheckWinCondition()) WinGame(); else Debug.Log("Chưa đủ vật phẩm!"); }
        
        if (isUsingSkill && other.CompareTag("Enemy")) 
        {
            BossShaunAI boss = other.GetComponentInParent<BossShaunAI>();
            if (boss != null) boss.TakeDamage(skillDamage);
            else other.SendMessage("TakeDamage", skillDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    private IEnumerator ActivateHitbox() 
    { 
        isAttacking = true; 
        yield return new WaitForSeconds(attackDelay); 
        punchHitbox.enabled = true; 

        Collider[] hitEnemies = Physics.OverlapBox(punchHitbox.transform.position, punchHitbox.bounds.extents, punchHitbox.transform.rotation);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                BossShaunAI boss = enemy.GetComponentInParent<BossShaunAI>();
                if (boss != null) boss.TakeDamage(punchDamage); 
                else enemy.SendMessage("TakeDamage", punchDamage, SendMessageOptions.DontRequireReceiver); 
            }
        }

        yield return new WaitForSeconds(0.2f); 
        punchHitbox.enabled = false; 
        yield return new WaitForSeconds(attackCooldown); 
        isAttacking = false; 
    }

    public void Heal(int amount)
    {
        if (isDead || isWon) return;
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHeartUI();
    }

    private void UpdateUI()
    {
        if (coinText != null) coinText.text = coinCount.ToString() + "/" + requiredCoins;
        if (crystalText != null) crystalText.text = crystalCount.ToString() + "/" + requiredCrystals;
        if (keyText != null) keyText.text = keyCount.ToString() + "/" + requiredKeys;
    }

    private void UpdateHeartUI()
    {
        for (int i = 0; i < heartFills.Length; i++)
        {
            if (heartFills[i] != null) heartFills[i].gameObject.SetActive(i < currentHealth);
        }
    }

    private void WinGame()
    {
        isWon = true;
        rb.velocity = Vector3.zero; 
        if (animator != null) animator.SetFloat(AnimatorParamSpeed, 0f); 
        if (winTextObject != null) winTextObject.SetActive(true);
    }

    private void OnTriggerStay(Collider other) { if (other.CompareTag("Ladder")) { isClimbing = true; rb.useGravity = false; rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); } }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Ladder")) { isClimbing = false; rb.useGravity = true; } }
    private void SetAnimatorBool(string parameterName, bool value) { if (animator != null && HasAnimatorParameter(parameterName)) animator.SetBool(parameterName, value); }
    private void SetAnimatorTrigger(string parameterName) { if (animator != null && HasAnimatorParameter(parameterName)) animator.SetTrigger(parameterName); }
    private bool HasAnimatorParameter(string parameterName) { if (animator == null) return false; foreach (AnimatorControllerParameter param in animator.parameters) if (param.name == parameterName) return true; return false; }
    
    public void TakeDamage(int damage) 
    { 
        if (isDead || isWon) return; 
        currentHealth -= damage; 
        UpdateHeartUI(); 
        SetAnimatorTrigger(AnimatorParamHitTrigger); 
        
        StartCoroutine(FlashRoutine());
        
        if (hitEffectPrefab != null)
        {
            Vector3 spawnPos = (hitEffectSpawnPoint != null) ? hitEffectSpawnPoint.position : transform.position + Vector3.up;
            GameObject effect = Instantiate(hitEffectPrefab, spawnPos, Quaternion.identity);
            Destroy(effect, 1.0f);
        }
        
        if (currentHealth <= 0) Die(); 
    }

    IEnumerator FlashRoutine()
    {
        if (katRenderer != null)
        {
            Color originalColor = katRenderer.material.color;
            katRenderer.material.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            katRenderer.material.color = originalColor;
        }
    }

    private void Die() { isDead = true; SetAnimatorTrigger(AnimatorParamDeathTrigger); StartCoroutine(RestartGameRoutine()); }
    private IEnumerator RestartGameRoutine() { yield return new WaitForSeconds(2f); SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    private void OnCollisionEnter(Collision collision) { isGrounded = true; }
    
    private bool CheckWinCondition() 
    { 
        return coinCount >= requiredCoins && crystalCount >= requiredCrystals && keyCount >= requiredKeys; 
    }
}