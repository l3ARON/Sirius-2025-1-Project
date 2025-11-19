using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MiddleEnemyController : MonoBehaviour
{
   [Header("프리팹 설정 (중요)")]
    public bool isNativeFriendly = false; 
    public GameObject friendlyPrefab;     

    [Header("HP")]
    public int maxHP = 3;
    [SerializeField] int currentHP;

    [Header("피격 연출")]
    public float invincibleTime = 0.5f;
    public float blinkInterval = 0.1f;
    public float knockbackPower = 4f;

    MiddleEnemyAttack attack;
    MiddleEnemyMovement movement;
    SpriteRenderer sprite;
    Rigidbody2D rigid;
    
    bool isInvincible = false;
    [HideInInspector] public bool isFriendly = false;

    void Awake()
    {
        currentHP = maxHP;
        attack    = GetComponent<MiddleEnemyAttack>();
        movement  = GetComponent<MiddleEnemyMovement>();
        sprite    = GetComponent<SpriteRenderer>();
        rigid     = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (isNativeFriendly) InitFriendlyMode();
    }

    // 🔥 [수정] isRealPlayer 매개변수 추가 (기본값 false)
    public void ApplyDamage(int dmg, bool isMelee, Vector2 hitPos, bool isRealPlayer = false)
    {
        if (isInvincible) return;
        if (isFriendly && !isMelee) return; 

        // ⭐ [패링 로직 수정]
        // 조건: 적 상태 + 공격 중 + HP 1 이하 + 근접 피격 + 🔥진짜 플레이어의 공격(isRealPlayer)
        if (!isFriendly && attack != null && attack.isAttacking && currentHP <= 1 && isMelee && isRealPlayer)
        {
            SwapToFriendlyWolf(); 
            return;
        }

        currentHP -= dmg;
        Debug.Log($"[{gameObject.name}] 피격! 남은 HP: {currentHP}");

        if (currentHP <= 0) Die();
        else StartCoroutine(HitEffectRoutine(hitPos));
    }

    // 호환성용 (함정이 때릴 때 등은 false 처리됨)
    public void TakeDamage(int dmg) => ApplyDamage(dmg, true, transform.position, false);

    // ... (나머지 SwapToFriendlyWolf, InitFriendlyMode 등은 기존과 동일) ...
    void SwapToFriendlyWolf()
    {
        if (friendlyPrefab != null)
        {
            GameObject newWolf = Instantiate(friendlyPrefab, transform.position, transform.rotation);
            newWolf.transform.localScale = transform.localScale;
            Debug.Log("✨ 늑대가 길들여졌습니다! (오브젝트 교체)");
        }
        else
        {
            Debug.LogError("❌ [MiddleEnemyController] Friendly Prefab이 할당되지 않았습니다!");
        }
        Destroy(gameObject);
    }

    void InitFriendlyMode()
    {
        isFriendly = true;
        gameObject.tag = "Player"; 
        if (attack != null) attack.SetFriendlyMode();
        if (movement != null) movement.SetFriendlyMode();
    }

    IEnumerator HitEffectRoutine(Vector2 hitPos)
    {
        isInvincible = true;
        if (movement != null) movement.PauseMovement();
        if (attack != null) attack.ForceStopAttack();

        if (rigid != null)
        {
            rigid.velocity = Vector2.zero;
            float dirX = Mathf.Sign(transform.position.x - hitPos.x);
            rigid.AddForce(new Vector2(dirX * knockbackPower, knockbackPower * 0.5f), ForceMode2D.Impulse);
        }

        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            if (sprite != null) sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        if (sprite != null) sprite.enabled = true;

        isInvincible = false;
        if (movement != null) movement.ResumeMovement();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
