using TMPro;
using UnityEngine;
public class CombatUIManager : MonoBehaviour
{
    public static CombatUIManager Instance { get; private set; }

    [Header("Aim")]
    [SerializeField] public AimPointerUI aimPointer;

    [Header("Damage Popup")]
    [SerializeField] private GameObject damageTextPrefab;

    [Header("Screen Effects")]
    [SerializeField] private CameraEffectorUI cameraEffector;

    [Header("Skill Panel")]
    [SerializeField] private GameObject skillPanel;

    [Header("Stat Panel")]
    [SerializeField] private TextMeshProUGUI magicStatText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            skillPanel.SetActive(true);
        }
        else
        {
            skillPanel.SetActive(false);
        }

        // 추후 최적화 할 것
        magicStatText.text = SkillManager.Instance.GetMagicStat().ToString();
        attackSpeedText.text = SkillManager.Instance.GetAttackSpeed().ToString();
    }

    // 적이 받은 피해 표시(MonsterBase.TakeDamage()에서 호출)
    public void ShowDamageText(float damage, Transform target, bool isCritical = false)
    {
        if (damageTextPrefab == null || target == null) return;

        // 1) 카메라 확보
        Camera cam = SkillManager.cam != null ? SkillManager.cam : Camera.main;
        if (cam == null) return;

        // 2) 몬스터가 화면 안에 있는지 체크
        Vector3 viewportPos = cam.WorldToViewportPoint(target.position);

        // z <= 0 이면 카메라 뒤쪽
        if (viewportPos.z <= 0f)
            return;

        // 화면 밖(좌/우/위/아래)이면 표시 안 함
        if (viewportPos.x < 0f || viewportPos.x > 1f ||
            viewportPos.y < 0f || viewportPos.y > 1f)
            return;

        Transform canvasT = GetComponentInParent<Canvas>().transform;
        GameObject go = ObjectPooler.Instance.SpawnInParent(damageTextPrefab, canvasT);

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


    // 카메라 진동 효과
    public void PlayCameraShake(float intensity, float duration)
    {
        cameraEffector?.StartCameraShake(intensity, duration);
    }
}


