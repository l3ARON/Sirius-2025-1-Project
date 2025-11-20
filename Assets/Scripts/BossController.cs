using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss HP")]
    public int maxHP = 30;
    private int currentHP;
    private bool isDead = false;

    [Header("Damage Flash FX")]
    public float flashDuration = 0.4f;     // 총 깜박임 시간
    public float flashInterval = 0.1f;     // 깜박임 간격
    private SpriteRenderer sprite;

    [Header("Slam Attack Settings")]
    public Collider2D slamHitbox;
    public float slamAirTime = 1f;
    public float slamHeight = 2f;
    public int slamDamage = 3;
    public LayerMask slamTargetMask;

    [Header("Slam FX")]
    public GameObject dustEffect;
    public Transform dustSpawnPoint;
    public AudioSource slamSound;
    public AudioSource jumpSound;

    private bool isSlamming = false;

    void Awake()
    {
        currentHP = maxHP;
        sprite = GetComponent<SpriteRenderer>();   // ⚡ spriteRenderer 캐싱
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        // 🔥 피격 깜박임 시작
        StartCoroutine(DamageFlash());

        // 🔥 체력이 5 단위로 줄어들면 Slam Attack
        if (currentHP > 0 && currentHP % 5 == 0 && !isSlamming)
        {
            StartCoroutine(SlamAttackRoutine());
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < flashDuration)
        {
            visible = !visible;
            if (sprite != null)
                sprite.enabled = visible;

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // 깜박임 종료 → 다시 보이게
        if (sprite != null)
            sprite.enabled = true;
    }

    IEnumerator SlamAttackRoutine()
    {
        if (isSlamming) yield break;
        isSlamming = true;

        Vector3 startPos = transform.position;
        Vector3 upPos = startPos + Vector3.up * slamHeight;

        float halfTime = slamAirTime * 0.5f;

        float t = 0f;

        if (jumpSound != null)
            jumpSound.Play();

        while (t < halfTime)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, upPos, t / halfTime);
            yield return null;
        }

        t = 0f;
        while (t < halfTime)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(upPos, startPos, t / halfTime);
            yield return null;
        }

        // 💥 Slam Impact
        DoSlamDamage();
        SpawnDustEffect();

        isSlamming = false;
    }

    void SpawnDustEffect()
    {
        if (dustEffect != null)
        {
            GameObject dust = Instantiate(
                dustEffect,
                dustSpawnPoint != null ? dustSpawnPoint.position : transform.position,
                Quaternion.identity
            );

            Destroy(dust, 3f);
        }

        if (slamSound != null)
            slamSound.Play();
    }

    void DoSlamDamage()
    {
        if (slamHitbox == null) return;

        Bounds b = slamHitbox.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(b.center, b.size, 0f, slamTargetMask);

        foreach (var hit in hits)
        {
            // 플레이어
            PlayerDamage2D pd = hit.GetComponent<PlayerDamage2D>();
            if (pd != null)
            {
                pd.OnDamaged(transform.position);
                continue;
            }

            // 기타 적 TakeDamage 지원
            var comps = hit.GetComponentsInParent<MonoBehaviour>();
            foreach (var comp in comps)
            {
                var method = comp.GetType().GetMethod("TakeDamage");
                if (method != null)
                {
                    method.Invoke(comp, new object[] { slamDamage });
                    break;
                }
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        slamSound.Play();
        Destroy(gameObject);
    }
}
