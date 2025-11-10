using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image cooldownFill;
    [SerializeField] private float cooldownTime;
    [SerializeField] private Image icon;
    private bool isCoolingDown = false;
    private float elapsed = 0f;

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
        icon.sprite = sprite;
    }
}
