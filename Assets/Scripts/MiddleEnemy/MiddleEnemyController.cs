using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MiddleEnemyController : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 3;
    [SerializeField] int currentHP;

    [Header("피격 연출")]
    public float invincibleTime = 0.5f;
    public float blinkInterval = 0.1f;
    public float knockbackPower = 4f;

    [Header("아군 전환 설정")]
    public Material friendlyMaterial; // 👈 여기에 인스펙터에서 '아군용 머티리얼'을 넣으세요!

    // 컴포넌트 참조
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

    // 플레이어 공격 스크립트에서 호출
    // 👇 [중요!] 원래 TakeDamage였던 이 함수의 이름을 ApplyDamage로 바꿔야 합니다!
    public void ApplyDamage(int dmg, bool isMelee, Vector2 hitPos) 
    {
        if (isFriendly || isInvincible) return;

        // 패링 로직
        if (attack != null && attack.isAttacking && currentHP <= 1 && isMelee)
        {
            BecomeFriendly();
            return;
        }

        // 피격 로직
        currentHP -= dmg;
        Debug.Log($"[MiddleEnemy] 피격! 남은 HP: {currentHP}");

        if (currentHP <= 0)
            Die();
        else
            StartCoroutine(HitEffectRoutine(hitPos));
    }

    // 2️⃣ [도우미 함수] 이름은 TakeDamage (재료 1개)
    // ⚠️ 이 함수가 2개 있으면 안 됩니다! 딱 1개만 있어야 해요.
    public void TakeDamage(int dmg)
    {
        // 메인 함수(ApplyDamage)에게 전달
        ApplyDamage(dmg, true, transform.position);
    }

    IEnumerator HitEffectRoutine(Vector2 hitPos)
    {
        isInvincible = true;

        // 1. 피격 시 행동 강제 중단 (공격 캔슬, 이동 멈춤)
        if (movement != null) movement.PauseMovement();
        if (attack != null)   attack.ForceStopAttack(); 

        // 2. 넉백
        if (rigid != null)
        {
            rigid.velocity = Vector2.zero;
            float dirX = Mathf.Sign(transform.position.x - hitPos.x);
            rigid.AddForce(new Vector2(dirX * knockbackPower, knockbackPower * 0.5f), ForceMode2D.Impulse);
        }

        // 3. 깜빡임
        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            if (sprite != null) sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        if (sprite != null) sprite.enabled = true;

        isInvincible = false;

        // 4. 다시 행동 재개 (아군이 아닐 때만)
        if (!isFriendly && movement != null)
        {
            movement.ResumeMovement();
        }
    }

    void BecomeFriendly()
    {
        Debug.Log("🛡️ [패링 성공] 몬스터가 아군으로 전환됩니다!");
        isFriendly = true;
        isInvincible = false;
        StopAllCoroutines(); 

        // 공격 기능 끄기
        if (attack != null)
        {
            attack.ForceStopAttack();
            attack.enabled = false;
        }

        // 이동을 아군 모드로 변경
        if (movement != null) movement.SetFriendlyMode();

        // 🔥 비주얼 변경 (머티리얼 교체)
        if (sprite != null) 
        {
            sprite.enabled = true;

            // 1. 머티리얼 교체
            if (friendlyMaterial != null)
            {
                sprite.material = friendlyMaterial;
            }
            else
            {
                Debug.LogWarning("아군용 머티리얼(Friendly Material)이 할당되지 않았습니다!");
            }

            // 2. 색상 초기화 (중요!)
            // 기존에 피격 등으로 색이 변해있거나, 위 코드처럼 파란색 틴트를 섞는 게 아니라면
            // 머티리얼 본연의 느낌을 살리기 위해 흰색(기본)으로 돌려주는 게 좋습니다.
            sprite.color = Color.white; 
        }

        // 태그/레이어 변경
        gameObject.tag = "Untagged";
        gameObject.layer = LayerMask.NameToLayer("Default"); 
    }
    
    void Die()
    {
        if (movement != null) movement.PauseMovement();
        StopAllCoroutines();
        GetComponent<Collider2D>().enabled = false; 
        Destroy(gameObject, 0.5f);
    }
}