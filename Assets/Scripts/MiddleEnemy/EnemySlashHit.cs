using UnityEngine;

public class EnemySlashHit : MonoBehaviour
{
    private MiddleEnemyAttack enemyAttack;

    void Awake()
    {
        // 부모 오브젝트에서 공격 스크립트를 찾습니다.
        enemyAttack = GetComponentInParent<MiddleEnemyAttack>();

        if (enemyAttack == null)
            Debug.LogWarning($"[EnemySlashHit] 부모({transform.parent?.name})에 MiddleEnemyAttack 컴포넌트가 없습니다!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 공격 판정은 본체(MiddleEnemyAttack)에게 위임
        if (enemyAttack != null)
        {
            enemyAttack.OnSlashHitPlayer(other);
        }
    }
}