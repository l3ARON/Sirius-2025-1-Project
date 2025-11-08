using UnityEngine;

public class dummy : MonoBehaviour
{
    [Header("HP 설정")]
    public int maxHP = 5;
    int currentHP;

    void Awake()
    {
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
        Debug.Log("[SmallEnemyController] 적 사망");

        Destroy(gameObject);
    }
}
