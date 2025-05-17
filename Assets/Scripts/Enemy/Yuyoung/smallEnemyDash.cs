using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class smallEnemyDash : MonoBehaviour
{
    // 플레이어의 위치를 참조
    public Transform player;
    private Rigidbody2D rigid;           // Rigidbody2D 컴포넌트

    // 플레이어를 감지할 반경
    public float detectionRadius = 3f;
    void Start()
    {
        // 평소 움직임 스크립트 활성화
        GetComponent<smallEnemyMove>().enabled = true;
        rigid = GetComponent<Rigidbody2D>();
    }

    private bool isDetected = false; // 플레이어 감지 여부
    private Vector2 dashDirection;       // 돌진할 방향 벡터
    void Update()
    {
        // 플레이어와의 거리 계산
        float distance = Vector2.Distance(transform.position, player.position);

        if (!isDashed)  // 대쉬 중이 아닐 때
        {
            if (distance <= detectionRadius && !isDetected) // 범위 안에 들어옴
            {
                Debug.Log("플레이어 감지!");
                isDetected = true; // 플레이어 감지
                
                GetComponent<smallEnemyMove>().enabled = false; // 평소 움직임 스크립트 비활성화
                rigid.velocity = Vector2.zero;  // 이동 정지 및 패턴 중단

                StartCoroutine(PerformDash());     // 돌진 루틴 실행
            }
            else if (distance > detectionRadius && isDetected)  // 범위 벗어남
            {
                Debug.Log("Detecting NULL");
                isDetected = false; // 플레이어 비감지

                GetComponent<smallEnemyMove>().enabled = true; // idle 스크립트 활성화
            }
        }
        
    }
    private bool isDashed = false; // 돌진 여부
    public float dashSpeed = 5f;    // 돌진 속도
    public float waitForAttack = 1f;     // 돌진 전 대기 시간
    IEnumerator PerformDash()
    {
        // 대기 시간
        yield return new WaitForSeconds(waitForAttack);

        isDashed = true; // 돌진 중

        // 감지된 시점의 플레이어 위치 기억
        Vector2 targetPosition = player.position;

        // '감지된 시점 위치'로 돌진
        dashDirection = (targetPosition - (Vector2)transform.position).normalized;
        rigid.velocity = dashDirection * dashSpeed;

        isDashed = false; // 돌진 종료

    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 돌진 중이고 충돌한 것이 Ground 태그일 경우
        if (isDashed && collision.collider.CompareTag("Ground"))
        {
            Debug.Log("바닥이랑 충돌");
            isDashed = false;
        }
        if (isDashed && collision.collider.CompareTag("Player"))
        {
            Debug.Log("플레이어와 충돌");
            isDashed = false;
        }
    }

    // 에디터 상에서 감지 범위를 시각적으로 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
