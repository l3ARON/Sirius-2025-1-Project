using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class middleEnemyIdle : MonoBehaviour
{
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;

    // 현재 이동 방향 (-1: 왼쪽, 0: 정지, 1: 오른쪽)
    public int nextMove;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 처음 이동 방향 결정 (2초 후부터 시작)
        Invoke("Think", 2);
    }

    void FixedUpdate()
    {
        // 현재 방향으로 이동
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        // 스프라이트 방향 전환
        FlipSprite();

        // 앞에 발 밑에 땅이 있는지 확인
        Vector2 frontVec = new Vector2(rigid.position.x + (nextMove * 0.5f), rigid.position.y);
        Debug.DrawRay(frontVec, Vector2.down * 1f, Color.green);

        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector2.down, 1f);
        if (rayHit.collider == null)
        {
            Debug.Log("No Platform - 방향 전환");
            nextMove *= -1;
            CancelInvoke();          // 기존 Think 예약 취소
            Invoke("Think", 5);      // 다시 생각 시작
        }
    }

    void Think()
    {
        // -1(왼쪽), 0(정지), 1(오른쪽) 중 선택
        nextMove = Random.Range(-1, 2);

        // 다음 Think 예약
        Invoke("Think", 2);
    }

    void FlipSprite()
    {
        // 왼쪽이면 flipX = true, 오른쪽이면 false
        if (nextMove != 0)
        {
            spriteRenderer.flipX = nextMove == -1;
        }
    }
}
