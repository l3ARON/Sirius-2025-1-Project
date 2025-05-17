using System.Collections;
using UnityEngine;

public class middleEnemyIdle : MonoBehaviour
{
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;

    public float xSpeed = 1f; // 이동 속도

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        // 1. 이동
        rigid.velocity = new Vector2(xSpeed, rigid.velocity.y);

        // 2. 스프라이트 방향 전환
        FlipSprite();

        // 3. 앞 위치 기준 계산
        Vector2 frontVec = new Vector2(rigid.position.x + (xSpeed * 1f), rigid.position.y);
        Vector2 checkDir = (xSpeed > 0) ? Vector2.right : Vector2.left;

        // 4. 낭떠러지 체크 → 방향 전환
        if (!IsGroundAhead(frontVec))
        {
            xSpeed *= -1;
            return;
        }

        // 5. 벽 체크 → 방향 전환
        if (IsWallAhead(frontVec, checkDir))
        {
            xSpeed *= -1;
        }
    }

    // 스프라이트 반전 함수
    void FlipSprite()
    {
        if (xSpeed != 0)
            spriteRenderer.flipX = xSpeed > 0;
    }

    // 앞에 땅이 있는지 확인
    bool IsGroundAhead(Vector2 origin)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 1.5f);
        Debug.DrawRay(origin, Vector2.down * 1.5f, Color.green);

        if (hit.collider == null)
        {
            Debug.Log("낭떠러지!");
            return false;
        }
        else
        {
            Debug.Log("발밑 오브젝트 이름: " + hit.collider.gameObject.name);
            return true;
        }
    }

    // 앞에 벽이 있는지 확인
    bool IsWallAhead(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 0.1f);
        Debug.DrawRay(origin, direction * 0.1f, Color.red);

        if (hit.collider != null)
        {
            Debug.Log("앞이 막힘! → " + hit.collider.gameObject.name);
            return true;
        }

        return false;
    }
}
