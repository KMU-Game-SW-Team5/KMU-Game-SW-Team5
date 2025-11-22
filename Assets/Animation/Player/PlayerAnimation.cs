using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;

    // 현재 Base Layer가 어떤 상태인지 (Idle / Run / Jump)
    public AnimationType currentBaseLayerState = AnimationType.Idle;

    private Coroutine straightCoroutine;

    // 해시 캐싱 (옵션이지만 성능상 이득)
    private static readonly int HashRunning = Animator.StringToHash("Running");
    private static readonly int HashStraight = Animator.StringToHash("Straight");

    private static readonly int HashJump = Animator.StringToHash("Jump");
    private static readonly int HashRight = Animator.StringToHash("Right");
    private static readonly int HashLeft = Animator.StringToHash("Left");
    private static readonly int HashUp = Animator.StringToHash("Up");
    private static readonly int HashDown = Animator.StringToHash("Down");
    private static readonly int HashShot = Animator.StringToHash("Shot");
    private static readonly int HashStick = Animator.StringToHash("Stick");
    private static readonly int HashBlock = Animator.StringToHash("Block");
    private static readonly int HashHit = Animator.StringToHash("Hit");

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("[PlayerAnimation] Animator가 없음");
    }

    public void SetAnimation(AnimationType animationType)
    {
        switch (animationType)
        {
            // =====================
            // Base Layer 제어
            // =====================
            case AnimationType.Idle:
                SetBaseIdle();
                break;

            case AnimationType.Run:
                SetBaseRun();
                break;

            case AnimationType.Jump:
                PlayJump();
                break;

            // =====================
            // Action Layer (지팡이 액션 등)
            // =====================
            case AnimationType.Straight:
                animator.SetBool(HashStraight, true);
                break;

            case AnimationType.Right:
                animator.SetTrigger(HashRight);
                break;

            case AnimationType.Left:
                animator.SetTrigger(HashLeft);
                break;

            case AnimationType.Up:
                animator.SetTrigger(HashUp);
                break;

            case AnimationType.Down:
                animator.SetTrigger(HashDown);
                break;

            case AnimationType.Shot:
                animator.SetTrigger(HashShot);
                break;

            case AnimationType.Stick:
                animator.SetTrigger(HashStick);
                break;

            case AnimationType.Block:
                animator.SetTrigger(HashBlock);
                break;

            // =====================
            // Add Layer (피격 등)
            // =====================
            case AnimationType.Hit:
                animator.SetTrigger(HashHit);
                break;
        }
    }

    private void SetBaseIdle()
    {
        if (animator == null) return;
        currentBaseLayerState = AnimationType.Idle;

        // Run 끄기
        animator.SetBool(HashRunning, false);
        // 필요하다면 Idle 전용 트리거가 있다면 여기서 SetTrigger도 가능
    }

    private void SetBaseRun()
    {
        if (animator == null) return;
        currentBaseLayerState = AnimationType.Run;

        // ※ AnimationType은 Run이지만
        //    Animator 파라미터 이름은 "Running"이라고 가정
        animator.SetBool(HashRunning, true);
    }

    private void PlayJump()
    {
        if (animator == null) return;

        currentBaseLayerState = AnimationType.Jump;
        animator.SetTrigger(HashJump);
        // Running bool을 그대로 둬서
        // 착지 후 자연스럽게 Idle/Run 블렌드 되도록 설정해도 되고
        // 여기서 Running을 false로 끄는 식으로 조정해도 됨(취향/상태머신 구조에 따라)
    }

    // -------------------------------
    // Action Layer helpers
    // -------------------------------


    // Straight를 일정 시간 동안만 켜두고, 그 후 자동으로 꺼주는 함수
    public void PlayStraightFor(float duration)
    {
        // 이전에 돌고 있던 Straight 코루틴이 있으면 멈춤
        if (straightCoroutine != null)
            StopCoroutine(straightCoroutine);

        // Straight 애니메이션 켜기
        animator.SetBool("Straight", true);

        // duration이 0 이하면 바로 끄는 건 호출하는 쪽에서 알아서 처리해도 되지만,
        // 그냥 코루틴을 사용해도 문제 없음
        straightCoroutine = StartCoroutine(ResetStraightAfter(duration));
    }
    // 위 함수에서 호출
    private System.Collections.IEnumerator ResetStraightAfter(float duration)
    {
        // 지정된 시전 시간만큼 대기
        yield return new WaitForSeconds(duration);

        // Straight 끄기
        animator.SetBool("Straight", false);
        straightCoroutine = null;
    }

    // 수동으로 straight를 끄는 함수
    public void ResetStraight()
    {
        if (animator == null) return;
        animator.SetBool(HashStraight, false);
    }
}
