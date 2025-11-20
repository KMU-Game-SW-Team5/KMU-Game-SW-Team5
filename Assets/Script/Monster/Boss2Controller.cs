using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Boss2Controller : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float detectionRange = 300f;
    [SerializeField] private float attackRange = 30f;
    public float moveSpeed = 30f;
    public int maxHealth = 1000;
    private int currentHealth;

    [Header("Combat")]
    public int attackDamage = 10;
    public float attackCooldown = 2f;
    private float lastAttackTime;

    [Header("State")]
    private float deathAnimationDuration = 3f;
    private bool isDead = false;
    
    private Transform player;
    private Player playerScript;
    private Rigidbody rb;
    private Animator animator;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveLoopClip;
    [SerializeField] private float fadeDuration = 0.5f; 
    
    [SerializeField] private AudioClip attack1Clip;
    [SerializeField] private AudioClip attack2Clip;
    [SerializeField] private AudioClip deathClip;

    private float originalVolume; 
    private Coroutine currentFadeCoroutine; 
    private bool isMovingState = false; 

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false; 
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponentInChildren<Animator>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        audioSource.loop = true;
        audioSource.clip = moveLoopClip;
        audioSource.playOnAwake = false;
        
        
        originalVolume = 0.3f; 
        audioSource.volume = 0f;

        InvokeRepeating("FindPlayer", 0f, 0.5f);
    }

    void FixedUpdate()
    {
        if (isDead) 
        {
            if (audioSource.isPlaying) audioSource.Stop();
            return; 
        }

        if (player != null && !player.gameObject.activeInHierarchy)
        {
            player = null;
            playerScript = null;
        }
        
        bool currentIsMoving = false;

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            float stopThreshold = attackRange;
            float startThreshold = attackRange + 1.0f; 

            bool shouldMove = isMovingState 
                ? (distance > stopThreshold) 
                : (distance > startThreshold);

            if (distance <= detectionRange && shouldMove)
            {
                MoveTowardsPlayer();
                if(animator != null) animator.SetFloat("Speed", 1f); 
                
                currentIsMoving = true;
                isMovingState = true;
            }
            else
            {
                if (distance <= attackRange) AttackPlayer();
                if(animator != null) animator.SetFloat("Speed", 0f);
                
                currentIsMoving = false;
                isMovingState = false; 
            }
        }
        else
        {
            if(animator != null) animator.SetFloat("Speed", 0f);
            currentIsMoving = false;
        }

        if(currentHealth <= 500)
        {
            attackDamage = 20;
            attackCooldown = 2f;
            attackRange = 130f;
        }

        HandleFadeSound(currentIsMoving);
    }

    void HandleFadeSound(bool isMoving)
    {
        if (audioSource == null || moveLoopClip == null) return;

        if (isMoving)
        {
            if (!audioSource.isPlaying || audioSource.volume < originalVolume)
            {
                if (!audioSource.isPlaying) audioSource.Play(); 
                StartFade(originalVolume); 
            }
        }
        else
        {
            if (audioSource.isPlaying && audioSource.volume > 0f)
            {
                StartFade(0f);
            }
        }
    }

    void StartFade(float targetVolume)
    {
        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = StartCoroutine(FadeAudio(targetVolume));
    }

    IEnumerator FadeAudio(float targetVolume)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (targetVolume <= 0.01f)
        {
            audioSource.Stop();
        }
    }

    // 전역에서 들리는 사운드
    void PlayGlobalSound(AudioClip clip)
    {
        if (clip == null) return;

        
        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.position = transform.position;

        
        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
        tempSource.clip = clip;
        
        
        tempSource.spatialBlend = 0f;
        tempSource.volume = 0.5f;    

        tempSource.Play();

        // 재생 끝나면 자동 삭제
        Destroy(tempAudio, clip.length);
    }
    

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
        Vector3 lookPosition = player.position;
        lookPosition.y = transform.position.y;
        transform.LookAt(lookPosition);
        
        Vector3 targetPosition = player.position;
        targetPosition.y = transform.position.y;

        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }
    
    void AttackPlayer()
    {
        Vector3 lookPosition = player.position;
        lookPosition.y = transform.position.y;
        transform.LookAt(lookPosition);

        if (Time.time > lastAttackTime + attackCooldown)
        {
            if (playerScript != null)
            {
                if (currentHealth > maxHealth / 2)
                {
                    animator.SetTrigger("Attack1");
                    
                    
                    PlayGlobalSound(attack1Clip);
                    
                    playerScript.TakeDamage(attackDamage);
                }
                else
                {
                    animator.SetTrigger("Attack2");
                    
                    
                    PlayGlobalSound(attack2Clip);

                    playerScript.TakeDamage(attackDamage * 2);
                }
                lastAttackTime = Time.time;
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return; 
        currentHealth -= damage;

        Debug.Log("보스 2 체력: " + currentHealth);
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("보스 사망");
        
        StopAllCoroutines(); 
        if (audioSource != null) audioSource.Stop();

        
        PlayGlobalSound(deathClip);

        animator.SetTrigger("Die"); 
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, deathAnimationDuration);
    }
}