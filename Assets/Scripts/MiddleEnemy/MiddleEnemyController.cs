using System.Collections;
using UnityEngine;

public class MiddleEnemyController : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 3;
    int currentHP;

    [Header("피격 연출")]
    public float invincibleTime = 0.8f;
    public float blinkInterval = 0.1f;
    public float knockbackPower = 6f;
    public float knockbackUpPower = 1f;

    MiddleEnemyAttack attack;
    MiddleEnemyMovement movement;
    SpriteRenderer sprite;
    Rigidbody2D rigid;
    bool isInvincible = false;

    public bool isFriendly = false;

    void Awake()
    {
        currentHP = maxHP;

        attack = GetComponent<MiddleEnemyAttack>();
        movement = GetComponent<MiddleEnemyMovement>();
        sprite = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// dmg: 데미지
    /// isMelee: 플레이어의 근접 공격인지?
    /// hitPos: 공격자가 있는 위치 (넉백 방향 계산용)
    /// </summary>
    public void TakeDamage(int dmg, bool isMelee, Vector2 hitPos = default)
    {
        if (isFriendly || isInvincible) return;

        // 패링(아군 전환) 조건
        if (attack.isAttacking && currentHP == 1 && isMelee)
        {
            BecomeFriendly();
            return;
        }

        currentHP -= dmg;

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitEffectRoutine(hitPos));
        }
    }

    IEnumerator HitEffectRoutine(Vector2 hitPos)
    {
        isInvincible = true;

        // 이동/공격 중단
        movement.PauseMovement();
        rigid.velocity = Vector2.zero;

        // 🌪 넉백 방향 계산
        int dir = transform.position.x - hitPos.x > 0 ? 1 : -1;

        rigid.AddForce(
            new Vector2(dir * knockbackPower, knockbackUpPower * knockbackPower),
            ForceMode2D.Impulse
        );

        // 🌟 깜빡임 연출 시작
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < invincibleTime)
        {
            visible = !visible;
            sprite.enabled = visible;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // 복구
        sprite.enabled = true;
        movement.ResumeMovement();
        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("🐺 중형 몬스터 사망");
        Destroy(gameObject);
    }

    void BecomeFriendly()
    {
        Debug.Log("🐺 패링 성공 → 아군 전환!");

        isFriendly = true;

        attack.enabled = false;

        movement.detectRange = 30f;
        movement.chaseSpeed = 4f;

        sprite.color = new Color(0.6f, 0.8f, 1f); // 파란 톤
        gameObject.tag = "Friendly";
    }
}
