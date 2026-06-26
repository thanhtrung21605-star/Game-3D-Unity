using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossShaunAI : MonoBehaviour
{
    [Header("=== Cấu Hình Giai Đoạn (Boss) ===")]
    public GameObject phase1Model; 
    public GameObject phase2Model;
    public int hpPhase1 = 10;
    public int hpPhase2 = 20;
    private bool isPhase2 = false;

    [Header("=== Cấu Hình Chiêu Weapon (Phase 2) ===")]
    public float weaponInterval = 3.0f;
    public float weaponDuration = 1.5f;
    public GameObject weaponExplosionPrefab; 
    public float explosionRadius = 1.5f;     
    public float explosionScale = 1.0f;      
    public int explosionDamage = 1;          
    public BoxCollider arenaBounds;          // Hãy kéo sàn vào đây

    [Header("=== Cấu Hình Hiệu Ứng (VFX) ===")]
    public GameObject transformationVFX; 
    public GameObject spawnVFX;          
    public GameObject deathVFXPhase1;    
    public GameObject deathVFX;          
    public float vfxDuration = 1.0f;     
    public float vfxDelay = 0.5f;        
    
    [Header("=== Cấu Hình Hiệu Ứng Bị Đánh ===")]
    public float hitEffectDuration = 0.5f; 
    public float hitEffectDelay = 0.0f;
    public float hitEffectScale = 1.0f;

    [Header("=== Cấu Hình Bất Tử & Xuất Hiện (Phase 2) ===")]
    public float invulnerabilityDuration = 2.0f; 
    public float spawnWeaponDelay = 1.0f; 
    public GameObject invulnerabilityVFX; 
    public float invulnerabilityVFXScale = 1.0f;
    private bool isInvulnerable = false;
    private GameObject currentInvulnerableEffect;

    [Header("=== Chỉ Số Gốc ===")]
    public int biteDamage = 1;          
    public GameObject hitEffectPrefab; 
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
    private bool isPerformingWeapon = false; 
    private int directionSign = 1; 
    private Coroutine weaponCycle;
    private int currentHP;

    enum CrabState { Idling, Walking, Chasing, Attacking, Dead }
    CrabState currentState = CrabState.Idling;

    // --- HÀM NỔ CỐ ĐỊNH TRONG ARENA BOUNDS ---
    Vector3 GetRandomPointInBounds(BoxCollider col)
    {
        if (col == null) return transform.position;
        Bounds b = col.bounds;
        return new Vector3(
            Random.Range(b.min.x, b.max.x),
            b.max.y, // Nổ trên bề mặt sàn
            Random.Range(b.min.z, b.max.z)
        );
    }

    IEnumerator ExecuteWeaponExplosions()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 randomPos = GetRandomPointInBounds(arenaBounds);
            
            if (weaponExplosionPrefab != null)
            {
                GameObject explosion = Instantiate(weaponExplosionPrefab, randomPos, Quaternion.identity);
                explosion.transform.localScale = Vector3.one * explosionScale;
                Destroy(explosion, 2.0f);
            }

            Collider[] hits = Physics.OverlapSphere(randomPos, explosionRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                    hit.SendMessageUpwards("TakeDamage", explosionDamage, SendMessageOptions.DontRequireReceiver);
            }
            yield return new WaitForSeconds(0.4f);
        }
    }

    IEnumerator WeaponRoutine()
    {
        while (isPhase2 && !isDead)
        {
            yield return new WaitForSeconds(weaponInterval);
            if (currentHP <= 0) break;
            isPerformingWeapon = true;
            Animator activeAnim = GetActiveAnimator();
            if (activeAnim != null) activeAnim.SetTrigger("WeaponTrigger");
            
            StartCoroutine(ExecuteWeaponExplosions());
            
            yield return new WaitForSeconds(weaponDuration);
            isPerformingWeapon = false;
        }
    }

    // --- CÁC HÀM XỬ LÝ KHÁC (GIỮ NGUYÊN) ---
    private Animator GetActiveAnimator() { GameObject activeModel = isPhase2 ? phase2Model : phase1Model; return activeModel != null ? activeModel.GetComponentInChildren<Animator>() : null; }
    private Vector3 GetHitEffectPosition() { GameObject activeModel = isPhase2 ? phase2Model : phase1Model; Transform hitPoint = activeModel.transform.Find("HitPoint"); return (hitPoint != null) ? hitPoint.position : transform.position + Vector3.up * 0.5f; }
    void Start() { currentHP = hpPhase1; phase2Model.SetActive(false); if (useCrabPositionAsCenter) centerPosition = transform.position; patrolDirection.Normalize(); StartCoroutine(MainAIRoutine()); }
    void Update() { if (isDead || currentHP <= 0) return; Transform p = GetPlayer(); if (p == null) return; float distToPlayer = Vector3.Distance(transform.position, p.position); if (distToPlayer <= attackRange) currentState = CrabState.Attacking; else if (distToPlayer <= detectRange) currentState = CrabState.Chasing; else if (!isPatrolling) currentState = CrabState.Idling; Animator activeAnim = GetActiveAnimator(); if (activeAnim != null) activeAnim.SetBool("isFlying", !isDead); }
    public void TakeDamage(int damage) { if (isDead || currentHP <= 0 || isInvulnerable) return; currentHP -= damage; if (hitEffectPrefab != null) StartCoroutine(PlayHitEffectDelayed()); if (currentHP <= 0) { currentState = CrabState.Dead; if (!isPhase2) { if (deathVFXPhase1 != null) Instantiate(deathVFXPhase1, transform.position, Quaternion.identity); StartCoroutine(TransformationRoutine()); } else Die(); } else { Animator activeAnim = GetActiveAnimator(); if (activeAnim != null) activeAnim.SetTrigger("HitRecieve"); } }
    IEnumerator PlayHitEffectDelayed() { yield return new WaitForSeconds(hitEffectDelay); GameObject effect = Instantiate(hitEffectPrefab, GetHitEffectPosition(), Quaternion.identity); effect.transform.localScale = Vector3.one * hitEffectScale; Destroy(effect, hitEffectDuration); }
    IEnumerator TransformationRoutine() { isDead = true; GameObject vfxInstance = null; if (transformationVFX != null) vfxInstance = Instantiate(transformationVFX, transform.position, Quaternion.identity); Animator anim1 = phase1Model.GetComponentInChildren<Animator>(); if (anim1 != null) anim1.SetTrigger("Transform"); yield return new WaitForSeconds(2.0f); yield return new WaitForSeconds(vfxDelay); phase1Model.SetActive(false); phase2Model.SetActive(true); isPhase2 = true; currentHP = hpPhase2; isInvulnerable = true; if (invulnerabilityVFX != null) { currentInvulnerableEffect = Instantiate(invulnerabilityVFX, transform.position, Quaternion.identity); currentInvulnerableEffect.transform.SetParent(transform); currentInvulnerableEffect.transform.localScale = Vector3.one * invulnerabilityVFXScale; } if (spawnVFX != null) { GameObject spawnEffect = Instantiate(spawnVFX, transform.position, Quaternion.identity); Destroy(spawnEffect, vfxDuration); } yield return new WaitForSeconds(spawnWeaponDelay); Animator anim2 = phase2Model.GetComponentInChildren<Animator>(); if (anim2 != null) { anim2.Rebind(); anim2.SetTrigger("WeaponTrigger"); StartCoroutine(ExecuteWeaponExplosions()); } if (vfxInstance != null) Destroy(vfxInstance, vfxDuration); yield return new WaitForSeconds(invulnerabilityDuration); if (currentInvulnerableEffect != null) Destroy(currentInvulnerableEffect); isInvulnerable = false; isDead = false; weaponCycle = StartCoroutine(WeaponRoutine()); StartCoroutine(MainAIRoutine()); }
    void Die() { if (isDead) return; isDead = true; StopAllCoroutines(); if (deathVFX != null) Instantiate(deathVFX, transform.position, Quaternion.identity); Collider[] cols = GetComponentsInChildren<Collider>(); foreach(Collider c in cols) c.enabled = false; Rigidbody rb = GetComponent<Rigidbody>(); if (rb != null) rb.isKinematic = true; Animator activeAnim = GetActiveAnimator(); if (activeAnim != null) { activeAnim.SetBool("isFlying", false); activeAnim.CrossFade("CharacterArmature|Death", 0.1f); } if (heartPrefab != null) Instantiate(heartPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity); Destroy(gameObject, 5f); }
    IEnumerator MainAIRoutine() { while (!isDead) { if (currentHP <= 0) break; switch (currentState) { case CrabState.Idling: isPatrolling = true; yield return new WaitForSeconds(1.5f); if (currentState == CrabState.Idling) currentState = CrabState.Walking; break; case CrabState.Walking: isPatrolling = true; targetPosition = centerPosition + patrolDirection * (patrolDistance * directionSign); directionSign *= -1; Vector3 moveDir = (targetPosition - transform.position).normalized; if (moveDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(moveDir); Vector3 startPos = transform.position; float journeyLength = Vector3.Distance(startPos, targetPosition); float startTime = Time.time; while (currentState == CrabState.Walking) { if (currentHP <= 0) break; if (journeyLength > 0) { float fractionOfJourney = ((Time.time - startTime) * moveSpeed) / journeyLength; transform.position = Vector3.Lerp(startPos, targetPosition, fractionOfJourney); if (fractionOfJourney >= 1f) break; } Transform p = GetPlayer(); if (p != null && Vector3.Distance(transform.position, p.position) <= detectRange) break; yield return null; } if (currentState == CrabState.Walking) currentState = CrabState.Idling; break; case CrabState.Chasing: Transform pChase = GetPlayer(); if (pChase == null) { currentState = CrabState.Idling; break; } isPatrolling = false; Vector3 targetPos = new Vector3(pChase.position.x, transform.position.y, pChase.position.z); transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * 1.2f * Time.deltaTime); Vector3 chaseDir = (targetPos - transform.position).normalized; chaseDir.y = 0; if (chaseDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(chaseDir); yield return null; break; case CrabState.Attacking: isPatrolling = false; if (canAttack) yield return StartCoroutine(AttackRoutine()); else yield return null; break; default: yield return null; break; } } }
    IEnumerator AttackRoutine() { if (isPerformingWeapon || currentHP <= 0) yield break; Transform p = GetPlayer(); if (p == null) { canAttack = true; yield break; } canAttack = false; Vector3 dirToPlayer = (p.position - transform.position).normalized; transform.rotation = Quaternion.LookRotation(new Vector3(dirToPlayer.x, 0, dirToPlayer.z)); Animator activeAnim = GetActiveAnimator(); if (activeAnim != null) activeAnim.SetTrigger("Bite"); yield return new WaitForSeconds(0.3f); RaycastHit hit; if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer, out hit, attackRange)) { if (hit.collider.CompareTag("Player")) { hit.collider.SendMessageUpwards("TakeDamage", biteDamage, SendMessageOptions.DontRequireReceiver); } } yield return new WaitForSeconds(attackCooldown - 0.3f); canAttack = true; }
    private Transform GetPlayer() { if (player == null) { GameObject pObj = GameObject.FindGameObjectWithTag("Player"); if (pObj != null) player = pObj.transform; } return player; }
    void OnDrawGizmosSelected() { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectRange); Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange); }
}