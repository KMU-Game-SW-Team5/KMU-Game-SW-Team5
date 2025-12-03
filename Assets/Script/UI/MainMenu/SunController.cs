using System;
using UnityEngine;

[DisallowMultipleComponent]
public class SunController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light directionalLight;

    [Header("Time source")]
    [Tooltip("음수면 시스템 시각 사용")]
    [SerializeField] private float manualHour = -1f;

    [Header("Sun time control")]
    [SerializeField] private bool useManualSunTimes = false;
    [Range(0f, 24f)] [SerializeField] private float sunriseHour = 6f;
    [Range(0f, 24f)] [SerializeField] private float sunsetHour = 18f;

    [Header("Latitude-based (used when useManualSunTimes == false)")]
    [Tooltip("위도(도). 양수 = 북반구, 음수 = 남반구")]
    [SerializeField] private float latitude = 37.0f;
    [Tooltip("0이면 DateTime.Now.DayOfYear 사용")]
    [SerializeField] private int dayOfYear = 0;

    [Header("Sun transform")]
    [Tooltip("방위각(yaw)")]
    [SerializeField] private float azimuth = 170f;

    [Header("Light intensity")]
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 1f;
    [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Color over 24h (0..1 = 0:00..24:00)")]
    [SerializeField] private Gradient colorOverDay;

    private void Awake()
    {
        if (directionalLight == null)
        {
            directionalLight = GetComponent<Light>();
            if (directionalLight == null)
            {
                foreach (var l in FindObjectsOfType<Light>())
                {
                    if (l.type == LightType.Directional)
                    {
                        directionalLight = l;
                        break;
                    }
                }
            }
        }

        if (colorOverDay == null || colorOverDay.colorKeys.Length == 0)
        {
            colorOverDay = new Gradient();
            var keys = new GradientColorKey[5];
            var alphas = new GradientAlphaKey[5];

            keys[0].color = new Color(0.02f, 0.04f, 0.12f); keys[0].time = 0f;
            alphas[0].alpha = 1f; alphas[0].time = 0f;

            keys[1].color = new Color(1f, 0.55f, 0.2f); keys[1].time = 0.25f;
            alphas[1].alpha = 1f; alphas[1].time = 0.25f;

            keys[2].color = new Color(1f, 0.95f, 0.9f); keys[2].time = 0.5f;
            alphas[2].alpha = 1f; alphas[2].time = 0.5f;

            keys[3].color = new Color(1f, 0.45f, 0.12f); keys[3].time = 0.75f;
            alphas[3].alpha = 1f; alphas[3].time = 0.75f;

            keys[4].color = new Color(0.02f, 0.04f, 0.12f); keys[4].time = 1f;
            alphas[4].alpha = 1f; alphas[4].time = 1f;

            colorOverDay.SetKeys(keys, alphas);
        }
    }

    private void Start()
    {
        ApplySunByCurrentTime();
    }

    private void Update()
    {
        ApplySunByCurrentTime();
    }

    public void ApplySunByCurrentTime()
    {
        float hour = GetCurrentHour();
        float usedSunrise = sunriseHour;
        float usedSunset = sunsetHour;

        if (!useManualSunTimes)
        {
            int doy = dayOfYear > 0 ? dayOfYear : DateTime.Now.DayOfYear;
            ComputeSunriseSunsetFromLatitude(latitude, doy, out usedSunrise, out usedSunset);
        }

        // 안전성: 최소 간격 보장
        if (usedSunset <= usedSunrise + 0.01f)
        {
            usedSunset = usedSunrise + 0.5f;
        }

        // t: 0..1 during day (sunrise->sunset). outside -> <0 or >1
        float t = (hour - usedSunrise) / (usedSunset - usedSunrise);

        // elevation: 0..180 where 0 = horizon at sunrise, 90 = noon overhead, 180 = horizon at sunset
        float elevation = t * 180f;

        // apply rotation
        transform.rotation = Quaternion.Euler(elevation, azimuth, 0f);

        // intensity: use curve on clamped day progress so only day part contributes
        float clampedT = Mathf.Clamp01(t);
        float sunHeight = Mathf.Sin(clampedT * Mathf.PI); // 0..1 (sunrise->noon->sunset)
        float intensityT = intensityCurve.Evaluate(sunHeight);
        if (directionalLight != null)
        {
            directionalLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, intensityT);
            // color gradient evaluated on 24h normalized hour
            directionalLight.color = colorOverDay.Evaluate(Mathf.Clamp01(hour / 24f));
        }
        else
        {
            Debug.LogWarning("[SunController] Directional Light not found.");
        }
    }

    private float GetCurrentHour()
    {
        if (manualHour >= 0f && manualHour < 24f) return manualHour;
        DateTime now = DateTime.Now;
        return now.Hour + now.Minute / 60f + now.Second / 3600f;
    }

    /// <summary>
    /// 간단한 근사 공식을 사용해 일출/일몰 시간(시 단위) 계산.
    /// declination 사용: δ = 23.44° * sin(2π * (284 + N) / 365)
    /// 일출각 H0 = acos( -tan φ * tan δ )  -> 일출/일몰 시간은 noon ± H0(시간 단위로 변환)
    /// dayLengthHours = 24 * H0 / π
    /// sunrise = 12 - dayLength/2, sunset = 12 + dayLength/2
    /// (정확한 천문 계산은 아님, 근사 목적)
    /// </summary>
    private void ComputeSunriseSunsetFromLatitude(float latDeg, int dayOfYear, out float sunrise, out float sunset)
    {
        float phi = latDeg * Mathf.Deg2Rad;
        float decDeg = 23.44f * Mathf.Sin(2f * Mathf.PI * (284f + dayOfYear) / 365f);
        float dec = decDeg * Mathf.Deg2Rad;

        float x = -Mathf.Tan(phi) * Mathf.Tan(dec);

        // 클램프 및 특수 케이스 처리 (극야/백야)
        if (x >= 1f)
        {
            // 해가 항상 아래(극야)
            sunrise = 12f;
            sunset = 12f;
            return;
        }
        if (x <= -1f)
        {
            // 해가 항상 위(백야)
            sunrise = 0f;
            sunset = 24f;
            return;
        }

        float H0 = Mathf.Acos(Mathf.Clamp(x, -1f, 1f)); // radians
        float dayLengthHours = 24f * H0 / Mathf.PI;
        float noon = 12f;
        sunrise = noon - dayLengthHours * 0.5f;
        sunset = noon + dayLengthHours * 0.5f;
    }
}