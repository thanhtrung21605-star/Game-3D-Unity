using UnityEngine;
using System.Collections;

public class CrabAI : MonoBehaviour
{
    private Animator anim;
    
    [Header("=== Chỉ Số Gốc ===")]
    public int biteDamage = 1;          
    public int enemyHP = 3;             

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

    enum CrabState { Idling, Walking, Chasing, Attacking, Dead }
    CrabState currentState = CrabState.Idling;

    // HÀM BẢO VỆ: Đảm bảo player luôn tồn tại
    private Transform GetPlayer()
    {
        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }
        return player;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
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

        if (distToPlayer <= attackRange) currentState = CrabState.Attacking;
        else if (distToPlayer <= detectRange) currentState = CrabState.Chasing;
        else if (!isPatrolling) currentState = CrabState.Idling;
    }

    IEnumerator MainAIRoutine()
    {
        while (!isDead)
        {
            switch (currentState)
            {
                case CrabState.Idling:
                    isPatrolling = true;
                    if (anim != null) anim.SetBool("isWalking", false);
                    yield return new WaitForSeconds(1.5f); 
                    if (currentState == CrabState.Idling) currentState = CrabState.Walking;
                    break;

                case CrabState.Walking:
                    isPatrolling = true;
                    if (anim != null) anim.SetBool("isWalking", true);
                    targetPosition = centerPosition + patrolDirection * (patrolDistance * directionSign);
                    directionSign *= -1; 
                    Vector3 moveDir = (targetPosition - transform.position).normalized;
                    if (moveDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(moveDir);
                    
                    Vector3 startPos = transform.position;
                    float journeyLength = Vector3.Distance(startPos, targetPosition);
                    float startTime = Time.time;
                    
                    while (currentState == CrabState.Walking)
                    {
                        if (journeyLength > 0)
                        {
                            float fractionOfJourney = ((Time.time - startTime) * moveSpeed) / journeyLength;
                            transform.position = Vector3.Lerp(startPos, targetPosition, fractionOfJourney);
                            if (fractionOfJourney >= 1f) break;
                        }
                        Transform p = GetPlayer();
                        if (p != null && Vector3.Distance(transform.position, p.position) <= detectRange) break;
                        yield return null;
                    }
                    if (currentState == CrabState.Walking) currentState = CrabState.Idling;
                    break;

                case CrabState.Chasing:
                    Transform pChase = GetPlayer();
                    if (pChase == null) { currentState = CrabState.Idling; break; }
                    isPatrolling = false;
                    if (anim != null) anim.SetBool("isWalking", true);
                    Vector3 targetPos = new Vector3(pChase.position.x, transform.position.y, pChase.position.z);
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * 1.2f * Time.deltaTime);
                    Vector3 chaseDir = (targetPos - transform.position).normalized;
                    chaseDir.y = 0;
                    if (chaseDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(chaseDir);
                    yield return null;
                    break;

                case CrabState.Attacking:
                    isPatrolling = false;
                    if (canAttack) yield return StartCoroutine(AttackRoutine());
                    else yield return null;
                    break;
                default: yield return null; break;
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        Transform p = GetPlayer();
        if (p == null) { canAttack = true; yield break; }

        canAttack = false;
        if (anim != null) anim.SetBool("isWalking", false);
        
        Vector3 dirToPlayer = (p.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(new Vector3(dirToPlayer.x, 0, dirToPlayer.z));
        if (anim != null) anim.SetTrigger("Bite");
        
        yield return new WaitForSeconds(0.3f); 

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer, out hit, attackRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.SendMessageUpwards("TakeDamage", biteDamage, SendMessageOptions.DontRequireReceiver);
            }
        }
        yield return new WaitForSeconds(attackCooldown - 0.3f);
        canAttack = true;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        enemyHP -= damage;
        if (anim != null) anim.SetTrigger("HitRecieve"); 
        if (enemyHP <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();
        if (anim != null) { anim.SetBool("isWalking", false); anim.SetTrigger("Death"); }
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Destroy(gameObject, 2f); 
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}