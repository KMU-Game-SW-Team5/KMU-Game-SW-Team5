using UnityEngine;
using System.Collections;

public class ExplosionEffectComponent : MonoBehaviour
{
    [SerializeField] private GameObject effectPrefabRef; // 자신이 어떤 프리팹에서 생성됐는지
    [SerializeField] private float lifetime = 2f;        // 자동 반환까지의 시간 (Particle 수명 등)

    private ParticleSystem ps;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Projectile"); // 동일 레이어로 지정
        ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        // 파티클 지속시간을 자동으로 계산
        if (ps != null)
        {
            float dur = ps.main.duration + ps.main.startLifetime.constantMax;
            lifetime = Mathf.Max(lifetime, dur);
        }

        // 자동 반환 코루틴 시작
        StartCoroutine(AutoDespawn());
    }

    /// <summary>
    /// ObjectPooler가 Despawn 시 어떤 prefab의 큐로 돌려보낼지 알아야 함.
    /// ProjectileComponent에서 설정해줘야 함.
    /// </summary>
    public void SetPrefabRef(GameObject prefab)
    {
        effectPrefabRef = prefab;
    }

    /// <summary>
    /// 필요한 경우 외부에서 수명을 지정할 수도 있음.
    /// </summary>
    public void SetLifetime(float time)
    {
        lifetime = time;
    }

    private IEnumerator AutoDespawn()
    {
        yield return new WaitForSeconds(lifetime);
        if (ObjectPooler.Instance != null)
            ObjectPooler.Instance.Despawn(effectPrefabRef, gameObject);
        else
            Destroy(gameObject);
    }
}
