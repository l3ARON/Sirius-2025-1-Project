using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss HP")]
    public int maxHP = 30;
    private int currentHP;
    private bool isDead = false;

    [Header("Slam Attack Settings")]
    public Collider2D slamHitbox;
    public float slamAirTime = 1f;
    public float slamHeight = 2f;
    public int slamDamage = 3;
    public LayerMask slamTargetMask;

    private bool isSlamming = false;

    [Header("Slam Sound Effects")]
    public AudioSource slamJumpSound;   // 🔼 올라갈 때 사운드
    public AudioSource slamImpactSound; // 🔽 착지할 때 사운드

    void Awake()
    {
        currentHP = maxHP;
    }

    void Start()
    {
        Debug.Log("[BossController] Start() 호출됨");
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        Debug.Log($"{gameObject.name} took {damage} damage! HP = {currentHP}");

        // 🔥 HP가 5의 배수일 때마다 Slam 발동
        if (currentHP > 0 &&
            currentHP % 5 == 0 &&
            !isSlamming)
        {
            StartCoroutine(SlamAttackRoutine());
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        Debug.Log($"{gameObject.name} died!");
        isDead = true;
        Destroy(gameObject);
    }

    IEnumerator SlamAttackRoutine()
    {
        if (isSlamming) yield break;
        isSlamming = true;

        Vector3 startPos = transform.position;
        Vector3 upPos = startPos + Vector3.up * slamHeight;

        float halfTime = slamAirTime / 2f;
        float t = 0f;

        // 🔊 올라가기 사운드
        if (slamJumpSound != null){
            /// Debug.Log("???");
            // slamJumpSound.Play();
            slamImpactSound.Play();
        }

        // 🔼 위로 이동
        while (t < halfTime)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / halfTime);
            transform.position = Vector3.Lerp(startPos, upPos, lerp);
            yield return null;
        }

        // 🔽 아래로 이동
        t = 0f;
        while (t < halfTime)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / halfTime);
            transform.position = Vector3.Lerp(upPos, startPos, lerp);
            yield return null;
        }

        // 🔥 착지 사운드!
        // if (slamImpactSound != null) slamImpactSound.Play();

        // 🌋 땅에 닿은 순간 → 데미지
        DoSlamDamage();

        isSlamming = false;
    }

    void DoSlamDamage()
    {
        if (slamHitbox == null) return;

        Bounds b = slamHitbox.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(b.center, b.size, 0f, slamTargetMask);

        foreach (var hit in hits)
        {
            PlayerDamage2D pd = hit.GetComponent<PlayerDamage2D>();

            if (pd != null)
            {
                pd.OnDamaged(transform.position);
                Debug.Log("Boss Slam → Player에게 데미지!");
                continue;
            }

            var comps = hit.GetComponentsInParent<MonoBehaviour>();
            foreach (var comp in comps)
            {
                var method = comp.GetType().GetMethod("TakeDamage");
                if (method != null)
                {
                    method.Invoke(comp, new object[] { slamDamage });
                    Debug.Log($"{hit.name}에게 {slamDamage} 데미지");
                    break;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (slamHitbox != null)
        {
            Gizmos.color = new Color(1, 0.3f, 0.3f, 0.3f);
            Bounds b = slamHitbox.bounds;
            Gizmos.DrawCube(b.center, b.size);
        }
    }
}
