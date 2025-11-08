using UnityEngine;

/// <summary>
/// 플레이어가 적과 부딪혔을 때 데미지를 입히는 트리거
/// </summary>
public class PlayerHitTrigger : MonoBehaviour
{
    private PlayerDamage2D playerDamage;   // 피격 처리 스크립트 참조

    void Awake()
    {
        playerDamage = GetComponent<PlayerDamage2D>();
        if (playerDamage == null)
        {
            Debug.LogError("[PlayerHitTrigger] PlayerDamage2D 컴포넌트가 없습니다!");
        }
    }

    /// <summary>
    /// 물리 충돌 시 데미지 처리
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 적의 위치를 전달하여 넉백 방향 계산에 사용
            Vector2 hitPos = collision.transform.position;
            playerDamage.OnDamaged(hitPos);
        }
    }

    /// <summary>
    /// 트리거 충돌(Trigger Collider가 사용된 경우)
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Vector2 hitPos = collision.transform.position;
            playerDamage.OnDamaged(hitPos);
        }
    }
}
