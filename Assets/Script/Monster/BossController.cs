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
    
    // [핵심] 이전 프레임의 애니메이션 시간 저장용 변수
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
                
                // ★ [여기!] 코드에서 애니메이션을 감시해서 소리 재생
                CheckAnimationLoopAndPlaySound();
            }
            else if (distance <= attackRange)
            {
                AttackPlayer();
                animator.SetFloat("Speed", 0f);
            }
            else
            {
                animator.SetFloat("Speed", 0f);
            }
        }
        else
        {
            if(animator != null) animator.SetFloat("Speed", 0f);
        }
    }

    // [핵심 기능] 애니메이션이 한 바퀴 돌 때마다 소리 재생
    void CheckAnimationLoopAndPlaySound()
    {
        // 1. 현재 애니메이션 상태 정보 가져오기 (0번 레이어)
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 2. 걷거나 뛰는 애니메이션인지 확인 (Tag나 이름으로 확인 가능하지만, 여기선 Speed 파라미터가 0보다 클 때로 가정)
        // 혹은 현재 재생 중인 애니메이션 이름이 "Run"이나 "Walk"인지 확인해도 됩니다.
        // 여기서는 간단하게 '반복되는 애니메이션'이면 소리를 내도록 합니다.
        
        // 3. 정수 부분 비교: (현재 시간의 정수값) > (이전 시간의 정수값) 이면 루프가 돌았다는 뜻!
        if ((int)stateInfo.normalizedTime > (int)lastNormalizedTime)
        {
            PlayWalkSound();
        }

        // 4. 현재 시간을 저장해서 다음 프레임에 비교
        lastNormalizedTime = stateInfo.normalizedTime;
    }

    void PlayWalkSound()
    {
        if (audioSource == null || walkClips == null || walkClips.Length == 0) return;

        int index = Random.Range(0, walkClips.Length);
        if (walkClips[index] != null)
        {
            audioSource.pitch = Random.Range(0.8f, 0.95f);
            audioSource.PlayOneShot(walkClips[index]);
        }
    }

    // ... (아래 FindPlayer, MoveTowardsPlayer, AttackPlayer, TakeDamage, Die는 기존과 동일) ...
    
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

    void MoveTowardsPlayer()
    {
        transform.LookAt(player);
        Vector3 targetPosition = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(targetPosition);
    }

    void AttackPlayer()
    {
        transform.LookAt(player);

        if (Time.time > lastAttackTime + attackCooldown)
        {
            if (playerScript != null)
            {
                Debug.Log("보스 공격"); 
                if(currentHealth > 800) { animator.SetTrigger("BasicAttack"); playerScript.TakeDamage(attackDamage); }
                else if(currentHealth > 600) { animator.SetTrigger("ClawAttack"); playerScript.TakeDamage(attackDamage*2); }
                else if(currentHealth > 500) { animator.SetTrigger("FlameAttack"); playerScript.TakeDamage(attackDamage*3); }
                else { animator.SetTrigger("FlyAttack"); playerScript.TakeDamage(attackDamage*4); }
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
        animator.SetTrigger("Die"); 
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, deathAnimationDuration);
    }
}