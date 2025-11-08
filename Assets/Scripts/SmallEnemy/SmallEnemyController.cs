using UnityEngine;

/// <summary>
/// 소형 적 전체 관리: HP, 사망 처리 등
/// </summary>
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

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        Debug.Log($"[SmallEnemyController] 피격! HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("[SmallEnemy] 사망");

        // 🔥 여기서 플레이어에게 '다음 공격은 원거리' 플래그 주기
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            PlayerAttack2D pa = playerObj.GetComponent<PlayerAttack2D>();
            if (pa != null)
            {
                pa.EnableNextRangedAttack();
                Debug.Log("[SmallEnemy] 다음 공격은 원거리로 전환!");
            }
        }

        // 이펙트 있으면 여기서 Instantiate
        Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
