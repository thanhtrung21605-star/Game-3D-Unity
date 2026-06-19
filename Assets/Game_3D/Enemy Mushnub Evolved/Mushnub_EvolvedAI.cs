using UnityEngine;
using System.Collections;

public class MushroomAI : MonoBehaviour
{
    private Animator anim;
    
    [Header("=== Chỉ Số Gốc ===")]
    public int biteDamage = 1;          
    public int enemyHP = 3;             

    [Header("=== Cấu Hình Tuần Tra ===")]
    public float moveSpeed = 2f;
    public Vector3 patrolDirection = new Vector3(0, 0, 1); 

    [Header("=== Cấu Hình Tấn Công ===")]
    public Transform player;            
    public float detectRange = 5f;    
    public float attackRange = 2.5f;  
    public float attackCooldown = 1.5f; 

    private bool canAttack = true;
    private bool isDead = false;

    enum MushroomState { Idling, Walking, Chasing, Attacking, Dead }
    MushroomState currentState = MushroomState.Idling;

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
        // Tự động tìm Animator ở bất cứ đâu trên đối tượng
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        
        StartCoroutine(MainAIRoutine()); 
    }

    void Update()
    {
        if (isDead) return;
        Transform p = GetPlayer();
        if (p == null) return;
        
        float distToPlayer = Vector3.Distance(transform.position, p.position);

        if (distToPlayer <= attackRange) currentState = MushroomState.Attacking;
        else if (distToPlayer <= detectRange) currentState = MushroomState.Chasing;
        else currentState = MushroomState.Idling;
    }

    IEnumerator MainAIRoutine()
    {
        while (!isDead)
        {
            switch (currentState)
            {
                case MushroomState.Idling:
                    if (anim != null) anim.SetBool("isWalking", false);
                    yield return new WaitForSeconds(1.5f); 
                    if (currentState == MushroomState.Idling) currentState = MushroomState.Walking;
                    break;

                case MushroomState.Walking:
                    if (anim != null) anim.SetBool("isWalking", true);
                    transform.position += patrolDirection * moveSpeed * Time.deltaTime;
                    yield return null; 
                    break;

                case MushroomState.Chasing:
                    Transform pChase = GetPlayer();
                    if (pChase == null) { currentState = MushroomState.Idling; break; }
                    if (anim != null) anim.SetBool("isWalking", true);
                    transform.position = Vector3.MoveTowards(transform.position, pChase.position, moveSpeed * Time.deltaTime);
                    yield return null;
                    break;

                case MushroomState.Attacking:
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
        
        // Gửi trigger tấn công
        if (anim != null) anim.SetTrigger("Bite_Front"); 
        
        yield return new WaitForSeconds(0.5f); 

        if (p != null && Vector3.Distance(transform.position, p.position) <= attackRange)
        {
            p.SendMessageUpwards("TakeDamage", biteDamage, SendMessageOptions.DontRequireReceiver);
        }

        yield return new WaitForSeconds(attackCooldown);
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
        if (anim != null) 
        { 
            anim.SetBool("isWalking", false); 
            anim.SetTrigger("Death"); 
        }
        Destroy(gameObject, 2f); 
    }
}