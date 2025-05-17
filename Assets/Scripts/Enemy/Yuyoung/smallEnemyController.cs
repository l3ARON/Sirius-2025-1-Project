using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class smallEnemyController : MonoBehaviour
{
    // 플레이어의 위치를 참조
    public Transform player;

    // 플레이어를 감지할 반경
    public float detectionRadius = 3f;

    // 돌진 속도
    public float dashSpeed = 5f;

    // 돌진 전 대기 시간
    public float waitForAttack = 1f;

    private Rigidbody2D rigid;           // Rigidbody2D 컴포넌트
    private bool hasDashed = false;      // 돌진 중인지 여부 (중복 방지용)
    private Vector2 dashDirection;       // 돌진할 방향 벡터

    void Start()
    {
        // Rigidbody 컴포넌트 가져오기
        rigid = GetComponent<Rigidbody2D>();

        // 평소 움직임 스크립트 활성화
        GetComponent<smallEnemyMove>().enabled = true;
    }

    void FixedUpdate()
    {
        // 플레이어와의 거리 계산
        float distance = Vector2.Distance(transform.position, player.position);

        // 아직 돌진하지 않았고, 감지 범위 안에 들어왔을 경우
        if (!hasDashed && distance <= detectionRadius)
        {
            hasDashed = true;                  // 한 번만 실행되도록 설정
            StartCoroutine(PerformDash());     // 돌진 루틴 실행
        }
    }

    // 돌진 준비 및 실행 루틴
    IEnumerator PerformDash()
    {
        // 대기 시간
        yield return new WaitForSeconds(waitForAttack);
        
        // 감지된 시점의 플레이어 위치 기억
        Vector2 targetPosition = player.position;

        // '감지된 시점 위치'로 돌진
        dashDirection = (targetPosition - (Vector2)transform.position).normalized;
        rigid.velocity = dashDirection * dashSpeed;

        // 이후 충돌에서 정지 및 복귀 처리됨
    }

    // 충돌 처리: ground에 닿으면 튕긴 뒤 정지
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 돌진 중이고 충돌한 것이 Ground 태그일 경우
        if (hasDashed && collision.collider.CompareTag("Ground"))
        {
            // 튕겨 나가는 반사 방향 계산 (물리 반응 느낌 추가)
            Vector2 incoming = rigid.velocity;
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflected = Vector2.Reflect(incoming, normal);
            rigid.velocity = reflected * 0.5f; // 튕기되 속도는 절반으로 감소

            // 튕긴 직후 정지 + idle 복귀 처리
            StartCoroutine(StopAfterBounce());
        }
    }

    // 튕긴 후 멈추고 다시 부유 움직임으로 돌아가기
    IEnumerator StopAfterBounce()
    {
        yield return new WaitForSeconds(0.1f); // 살짝 튕긴 시간 확보
        rigid.velocity = Vector2.zero;         // 정지
        GetComponent<smallEnemyMove>().enabled = true;  // 부유 이동 재개
        hasDashed = false;                     // 다시 감지할 수 있게 초기화
        Debug.Log("Dash complete. Returning to idle.");
    }

    // 에디터 상에서 감지 범위를 시각적으로 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
