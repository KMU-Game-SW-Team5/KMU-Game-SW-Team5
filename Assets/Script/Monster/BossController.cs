using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class BossController : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float detectionRange = 150f;
    public float attackRange = 30f;
    public float moveSpeed = 30f;
    public int maxHealth = 1000;
    private int currentHealth;

    [Header("Combat")]
    public int attackDamage = 10;
    public float attackCooldown = 2f;
    private float lastAttackTime;

    [Header("State")]
    private bool hasEnteredPhase2 = false;
    private float deathAnimationDuration = 3f;
    private bool isDead = false;
    
    private Transform player;
    private Player playerScript;
    private Rigidbody rb;
    private Animator animator;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] walkClips; 
    [SerializeField] private AudioClip[] phase2WalkClips; 
    [SerializeField] private AudioClip deathClip;

    
    [Header("Attack Audio Settings")]
    [SerializeField] private AudioClip basicAttackClip; 
    [SerializeField] private AudioClip clawAttackClip;  
    [SerializeField] private AudioClip flameAttackClip; 
    [SerializeField] private AudioClip flyAttackClip;   

    private float lastNormalizedTime; 

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        InvokeRepeating("FindPlayer", 0f, 0.5f);
    }

    void FixedUpdate()
    {
        if (isDead) return; 

        if (player != null && !player.gameObject.activeInHierarchy)
        {
            player = null;
            playerScript = null;
        }
        
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= detectionRange && distance > attackRange)
            {
                MoveTowardsPlayer();
                animator.SetFloat("Speed", 1f); 
                
                CheckAnimationLoopAndPlaySound();
            }
            else if (distance <= attackRange)
            {
                AttackPlayer();
                animator.SetFloat("Speed", 0f);
            }
            else animator.SetFloat("Speed", 0f);
        }
        else
        {
            if(animator != null) animator.SetFloat("Speed", 0f);
        }
    }

    void CheckAnimationLoopAndPlaySound()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if ((int)stateInfo.normalizedTime > (int)lastNormalizedTime)
        {
            PlayWalkSound();
        }

        lastNormalizedTime = stateInfo.normalizedTime;
    }

    void PlayWalkSound()
    {
        if (audioSource == null) return;

        AudioClip[] currentClips = hasEnteredPhase2 ? phase2WalkClips : walkClips;
        
        if (currentClips == null || currentClips.Length == 0) return;

        int index = Random.Range(0, currentClips.Length);
        
        if (currentClips[index] != null)
        {
            audioSource.pitch = Random.Range(0.8f, 0.95f);
            audioSource.PlayOneShot(currentClips[index]);
        }
    }

    // 공격 사운드 재생
    void PlayAttackSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(clip);
        }
    }
    
    // 플레이어 찾기
    void FindPlayer()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                playerScript = playerObject.GetComponent<Player>();
            }
        }
    }

    // 플레이어에게 이동
    void MoveTowardsPlayer()
    {
        transform.LookAt(player);
        Vector3 targetPosition = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(targetPosition);
    }

    // 플레이어 공격
    void AttackPlayer()
    {
        transform.LookAt(player);

        if (Time.time > lastAttackTime + attackCooldown)
        {
            if (playerScript != null)
            {
                Debug.Log("보스 공격"); 
                
                
                if(currentHealth > 800) 
                { 
                    animator.SetTrigger("BasicAttack"); 
                    PlayAttackSound(basicAttackClip); // 기본 공격
                    playerScript.TakeDamage(attackDamage); 
                }
                else if(currentHealth > 600) 
                { 
                    animator.SetTrigger("ClawAttack"); 
                    PlayAttackSound(clawAttackClip);
                    playerScript.TakeDamage(attackDamage*2); 
                }
                else if(currentHealth > 500) 
                { 
                    animator.SetTrigger("FlameAttack"); 
                    PlayAttackSound(flameAttackClip);
                    playerScript.TakeDamage(attackDamage*3); 
                }
                else 
                { 
                    animator.SetTrigger("FlyAttack"); 
                    PlayAttackSound(flyAttackClip);
                    playerScript.TakeDamage(attackDamage*4); 
                }
                

                lastAttackTime = Time.time;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; 
        currentHealth -= damage;
        
        if (!hasEnteredPhase2 && currentHealth <= 500 && currentHealth > 0)
        {
            attackRange = 100f;
            hasEnteredPhase2 = true; 
            animator.SetBool("isPhase2", true);
            animator.SetTrigger("startPhase2");
        }
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        if (deathClip != null)
        {
             AudioSource.PlayClipAtPoint(deathClip, transform.position, 1.0f);
        }

        animator.SetTrigger("Die"); 
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, deathAnimationDuration);
    }
}