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
    /// isMelee: true면 근접 공격, false면 원거리 공격
    /// </summary>
    public void TakeDamage(int dmg, bool isMelee)
    {
        currentHP -= dmg;
        Debug.Log($"[SmallEnemyController] 피격! HP: {currentHP}/{maxHP} / melee={isMelee}");

        if (currentHP <= 0)
        {
            Die(isMelee);
        }
    }

    void Die(bool killedByMelee)
    {
        Debug.Log("[SmallEnemy] 사망");

        // 🔥 근접 공격으로 죽였을 때만 버프 지급
        if (killedByMelee)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                PlayerAttack2D pa = playerObj.GetComponent<PlayerAttack2D>();
                if (pa != null)
                {
                    pa.EnableNextRangedAttack();
                    Debug.Log("[SmallEnemy] (근접 킬) 다음 공격은 원거리로 전환!");
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
