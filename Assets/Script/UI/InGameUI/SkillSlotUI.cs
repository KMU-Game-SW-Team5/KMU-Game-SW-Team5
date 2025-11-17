using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image cooldownFill;
    [SerializeField] private float cooldownTime;
    private SpriteRenderer iconSR;
    Image iconImage;
    private bool isCoolingDown = false;
    private float elapsed = 0f;

    private void Awake()
    {
        //iconSR = GetComponentInChildren<SpriteRenderer>();
        iconImage = GetComponentInChildren<Image>();
    }


    // 남은 쿨타임 비율을 입력받아서 UI를 세팅함.
    public void SetCooldownRatio(float ratio)
    {
        // ratio는 0~1 사이의 값이어야 함.
        cooldownFill.fillAmount = Mathf.Clamp01(ratio); // 비율을 0-1 범위로 제한
    }

    public void ActivateCooldown(float time)
    {
        if (isCoolingDown) return;
        cooldownTime = time;
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;
        elapsed = 0f;
        cooldownFill.fillAmount = 1f;

        while (elapsed < cooldownTime)
        {
            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / cooldownTime);
            cooldownFill.fillAmount = 1f - ratio;

            yield return null;
        }

        cooldownFill.fillAmount = 0f;
        isCoolingDown = false;
    }

    public void SetIcon(Sprite sprite)
    {
        iconImage.sprite = sprite;
        iconImage.color = Color.white;
    }
}
