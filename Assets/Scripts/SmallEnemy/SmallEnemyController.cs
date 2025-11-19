using UnityEngine;

[RequireComponent(typeof(SmallEnemyMovement), typeof(SmallEnemyAttack))]
public class SmallEnemyController : MonoBehaviour
{
    [Header("HP 설정")]
    public int maxHP = 5;
    int currentHP;

    [Header("사망 이펙트")]
    public GameObject deathEffect;

    SmallEnemyMovement movement;
    SmallEnemyAttack attack;

    void Awake()
    {
        movement = GetComponent<SmallEnemyMovement>();
        attack = GetComponent<SmallEnemyAttack>();
        currentHP = maxHP;
    }

    /// <summary>
    /// dmg: 데미지
    /// isMelee: 근접 공격 여부
    /// isRealPlayer: 🔥 [추가] 진짜 플레이어가 때렸는지 여부 (기본값 false)
    /// </summary>
    public void TakeDamage(int dmg, bool isMelee, bool isRealPlayer = false)
    {
        currentHP -= dmg;
        // Debug.Log($"[SmallEnemy] 피격! HP: {currentHP}/{maxHP} / Melee: {isMelee} / Player: {isRealPlayer}");

        if (currentHP <= 0)
        {
            Die(isMelee, isRealPlayer);
        }
    }

    void Die(bool killedByMelee, bool killedByRealPlayer)
    {
        Debug.Log($"[SmallEnemy] 사망 (Killer: {(killedByRealPlayer ? "Player" : "Ally/Other")})");

        // 🔥 [조건 수정] 근접 공격이고 + 진짜 플레이어가 죽였을 때만 버프 지급
        if (killedByMelee && killedByRealPlayer)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                PlayerAttack2D pa = playerObj.GetComponent<PlayerAttack2D>();
                if (pa != null)
                {
                    pa.EnableNextRangedAttack();
                    Debug.Log("🏹 [SmallEnemy] 플레이어 근접 킬! -> 원거리 공격 충전 완료");
                }
            }
        }

        // 이펙트
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}