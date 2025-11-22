using UnityEngine;
public class CombatUIManager : MonoBehaviour
{
    public static CombatUIManager Instance { get; private set; }

    [Header("Aim")]
    [SerializeField] public AimPointerUI aimPointer;

    [Header("Damage Popup")]
    [SerializeField] private GameObject damageTextPrefab;

    [Header("Screen Effects")]
    [SerializeField] private CameraEffectorUI cameraEffector; // 🔹 여기 하나만 참조

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 적이 받은 피해 표시(MonsterBase.TakeDamage()에서 호출)
    public void ShowDamageText(float damage, Transform target, bool isCritical = false)
    {
        if (damageTextPrefab == null || target == null) return;

        Transform canvasT = GetComponentInParent<Canvas>().transform;
        GameObject go = Instantiate(damageTextPrefab, canvasT);

        DamageTextUI ui = go.GetComponent<DamageTextUI>();
        if (ui != null)
        {
            ui.Setup(damage, target, isCritical);
        }
    }

    // ========== 화면 효과 연동 함수들 ==========

    // 딸피 진입
    public void StartLowHpEffect()
    {
        cameraEffector?.StartLowHpBlink();
    }

    // 딸피 해제
    public void StopLowHpEffect()
    {
        cameraEffector?.StopLowHpBlink();
    }

    // 피격 플래시
    public void PlayHitEffect(float intensity01 = 1f)
    {
        cameraEffector?.PlayHitFlash(intensity01);
    }
}


