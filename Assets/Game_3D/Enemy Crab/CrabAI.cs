using UnityEngine;
using System.Collections;

public class CrabAI : MonoBehaviour
{
    private Animator anim;
    
    [Header("=== Chỉ Số Gốc ===")]
    public int biteDamage = 1;          
    public int enemyHP = 3;             
    public GameObject hitEffectPrefab;  
    
    [Header("=== Cấu Hình Hiệu Ứng Bị Đánh ===")]
    public float hitEffectDuration = 0.5f; 
    public float hitEffectDelay = 0.0f;    
    public float hitEffectScale = 1.0f;
    public Renderer[] crabRenderers; 
    public float flashDuration = 0.1f;
    public Color flashColor = Color.red;

    [Header("=== Vật Phẩm Rơi ===")]
    public GameObject heartPrefab; 

    [Header("=== Cấu Hình Tuần Tra ===")]
    public float moveSpeed = 2f;
    public float patrolDistance = 1.2f; 
    public Vector3 patrolDirection = new Vector3(0, 0, 1); 

    [Header("=== Cấu Hình Vị Trí Tâm ===")]
    public Vector3 centerPosition; 
    public bool useCrabPositionAsCenter = true; 

    [Header("=== Cấu Hình Tấn Công ===")]
    public Transform player;            
    public float detectRange = 5f;    
    public float attackRange = 2.5f;  
    public float attackCooldown = 1.5f; 

    private Vector3 targetPosition;
    private bool isPatrolling = true;
    private bool canAttack = true;
    private bool isDead = false;
    private int directionSign = 1; 
    private bool isInvulnerable = false; 

    enum CrabState { Idling, Walking, Chasing, Attacking, Dead }
    CrabState currentState = CrabState.Idling;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        
        if (crabRenderers == null || crabRenderers.Length == 0)
            crabRenderers = GetComponentsInChildren<Renderer>();
        
        if (useCrabPositionAsCenter) centerPosition = transform.position;
        patrolDirection.Normalize();
        StartCoroutine(MainAIRoutine()); 
    }

    void Update()
    {
        if (isDead) return;
        Transform p = GetPlayer();
        if (p == null) return;
        
        float distToPlayer = Vector3.Distance(transform.position, p.position);

        if (distToPlayer <= attackRange) 
        {
            currentState = CrabState.Attacking;
        }
        else if (distToPlayer <= detectRange) 
        {
            currentState = CrabState.Chasing;
        }
        else 
        {
            if (currentState == CrabState.Chasing || currentState == CrabState.Attacking)
            {
                currentState = CrabState.Idling;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable) return;
        
        enemyHP -= damage;
        StartCoroutine(InvulnerabilityRoutine(0.2f));

        StopCoroutine("FlashRoutine");
        StartCoroutine(FlashRoutine());

        if (hitEffectPrefab != null) StartCoroutine(PlayHitEffectDelayed());

        if (enemyHP <= 0) Die();
        else if (anim != null) anim.SetTrigger("HitRecieve"); 
    }

    IEnumerator FlashRoutine()
    {
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        // Đổi sang màu đỏ cho tất cả các Renderer
        foreach (Renderer r in crabRenderers)
        {
            if (r != null)
            {
                r.GetPropertyBlock(propBlock);
                propBlock.SetColor("_BaseColor", flashColor); // Dùng _BaseColor cho URP
                r.SetPropertyBlock(propBlock);
            }
        }

        yield return new WaitForSeconds(flashDuration);

        // Xóa sạch hiệu ứng để trả về đúng màu Texture gốc
        foreach (Renderer r in crabRenderers)
        {
            if (r != null)
            {
                r.SetPropertyBlock(null);
            }
        }
    }

    IEnumerator InvulnerabilityRoutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    IEnumerator PlayHitEffectDelayed()
    {
        yield return new WaitForSeconds(hitEffectDelay);
        GameObject effect = Instantiate(hitEffectPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        effect.transform.localScale = Vector3.one * hitEffectScale;
        Destroy(effect, hitEffectDuration);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // DỪNG HẲN HIỆU ỨNG NHẤP NHÁY
        StopCoroutine("FlashRoutine");
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        if (anim != null) { 
            anim.SetBool("isWalking", false); 
            anim.SetTrigger("Death"); 
        }

        if (heartPrefab != null) Instantiate(heartPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        Destroy(gameObject, 3f); 
    }

    // Các hàm AI giữ nguyên...
    IEnumerator MainAIRoutine() { while (!isDead) { switch (currentState) { case CrabState.Idling: isPatrolling = true; if (anim != null) anim.SetBool("isWalking", false); yield return new WaitForSeconds(1.5f); if (currentState == CrabState.Idling) currentState = CrabState.Walking; break; case CrabState.Walking: isPatrolling = true; if (anim != null) anim.SetBool("isWalking", true); targetPosition = centerPosition + patrolDirection * (patrolDistance * directionSign); directionSign *= -1; Vector3 moveDir = (targetPosition - transform.position).normalized; if (moveDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(moveDir); Vector3 startPos = transform.position; float journeyLength = Vector3.Distance(startPos, targetPosition); float startTime = Time.time; while (currentState == CrabState.Walking) { if (journeyLength > 0) { float fractionOfJourney = ((Time.time - startTime) * moveSpeed) / journeyLength; transform.position = Vector3.Lerp(startPos, targetPosition, fractionOfJourney); if (fractionOfJourney >= 1f) break; } Transform p = GetPlayer(); if (p != null && Vector3.Distance(transform.position, p.position) <= detectRange) break; yield return null; } if (currentState == CrabState.Walking) currentState = CrabState.Idling; break; case CrabState.Chasing: Transform pChase = GetPlayer(); if (pChase == null) { currentState = CrabState.Idling; break; } isPatrolling = false; if (anim != null) anim.SetBool("isWalking", true); Vector3 targetPos = new Vector3(pChase.position.x, transform.position.y, pChase.position.z); transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * 1.2f * Time.deltaTime); Vector3 chaseDir = (targetPos - transform.position).normalized; chaseDir.y = 0; if (chaseDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(chaseDir); yield return null; break; case CrabState.Attacking: isPatrolling = false; if (canAttack) yield return StartCoroutine(AttackRoutine()); else yield return null; break; default: yield return null; break; } yield return null; } }
    IEnumerator AttackRoutine() { Transform p = GetPlayer(); if (p == null) { canAttack = true; yield break; } canAttack = false; if (anim != null) anim.SetBool("isWalking", false); Vector3 dirToPlayer = (p.position - transform.position).normalized; transform.rotation = Quaternion.LookRotation(new Vector3(dirToPlayer.x, 0, dirToPlayer.z)); if (anim != null) anim.SetTrigger("Bite"); yield return new WaitForSeconds(0.3f); RaycastHit hit; if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer, out hit, attackRange)) { if (hit.collider.CompareTag("Player")) { hit.collider.SendMessageUpwards("TakeDamage", biteDamage, SendMessageOptions.DontRequireReceiver); } } yield return new WaitForSeconds(attackCooldown - 0.3f); canAttack = true; }
    private Transform GetPlayer() { if (player == null) { GameObject pObj = GameObject.FindGameObjectWithTag("Player"); if (pObj != null) player = pObj.transform; } return player; }
    void OnDrawGizmosSelected() { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectRange); Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange); }
}