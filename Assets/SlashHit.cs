using UnityEngine;

/// <summary>
/// 슬래시 프리팹이 Enemy와 충돌했을 때 로그 출력 (Player는 무시)
/// </summary>
public class SlashHit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player 태그는 무시
        if (collision.CompareTag("Player"))
            return;

        // Enemy 태그만 로그 출력
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log($"[SlashHit] Enemy hit! → {collision.name}");
        }
    }
}
