using System.Collections;
using UnityEngine;

public class smallEnemyDash : MonoBehaviour
{
    // 플레이어 트랜스폼 (타겟)
    public Transform player;

    // 플레이어 감지 범위
    public float detectionRadius = 3f;

    // 돌진 속도 및 돌진 전 대기 시간
    public float dashSpeed = 5f;
    public float waitForAttack = 1f;

    private Rigidbody2D rigid;                  // Rigidbody2D 컴포넌트 참조
    private smallEnemyMove moveScript;          // 평소 이동 스크립트 참조
    private bool isDetected = false;            // 플레이어 감지 여부
    private bool isDashed = false;              // 현재 돌진 중 여부
    private Vector2 dashDirection;              // 돌진할 방향

    private Animator anim; // Animator 컴포넌트

    void Start()
    {
        // 컴포넌트 가져오기
        rigid = GetComponent<Rigidbody2D>();
        moveScript = GetComponent<smallEnemyMove>();
        anim = GetComponent<Animator>(); // Animator 가져오기

        // 시작 시 평소 이동 스크립트 활성화
        moveScript.enabled = true;
    }

    void Update()
    {
        // 돌진 중이면 감지 체크 중단
        if (isDashed) return;

        // 플레이어와의 거리 계산
        float distance = Vector2.Distance(transform.position, player.position);

        // 감지 범위 안에 들어오고 아직 감지되지 않았을 경우
        if (distance <= detectionRadius && !isDetected)
        {
            OnPlayerDetected();
        }
        // 감지되었는데 다시 범위 밖으로 나갔을 경우
        else if (distance > detectionRadius && isDetected)
        {
            OnPlayerLost();
        }
    }

    // 플레이어 감지 시 동작
    void OnPlayerDetected()
    {
        isDetected = true;

        // 이동 중단 및 이동 스크립트 비활성화
        moveScript.enabled = false;
        rigid.velocity = Vector2.zero;

        // 돌진 실행
        StartCoroutine(PerformDash());
    }

    // 플레이어 감지 해제 시 동작
    void OnPlayerLost()
    {
        isDetected = false;
        isDashed = false;
        moveScript.enabled = true; // 이동 재개
    }

    // 일정 시간 대기 후 플레이어 방향으로 돌진
    IEnumerator PerformDash()
    {
        Debug.Log("돌진 대기");
        // 돌진 전 대기 시간
        yield return new WaitForSeconds(waitForAttack);

        isDashed = true; // 돌진 시작

        Debug.Log("돌진 시작");

        // 감지 순간 플레이어 위치를 기준으로 방향 계산
        Vector2 targetPosition = player.position;
        dashDirection = (targetPosition - (Vector2)transform.position).normalized;

        // 해당 방향으로 속도 부여
        rigid.velocity = dashDirection * dashSpeed;
        anim.SetBool("isAttack", true); // ✅ 공격 애니메이션 ON

        // 일정 시간 후 자동 정지 (예: 1초 돌진)
        yield return new WaitForSeconds(1f);

        // 만약 아직도 돌진 상태라면 수동으로 종료
        if (isDashed)
        {
            Debug.Log("돌진 시간 만료, 자동 정지");
            StopDash();
        }
    }

    // 충돌 감지 함수
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDashed) return; // 돌진 중이 아닐 경우 무시

        string tag = collision.collider.tag;

        // 바닥과 충돌한 경우
        if (tag == "Ground")
        {
            Debug.Log("바닥이랑 충돌");
            StopDash();
        }
        // 플레이어와 충돌한 경우
        else if (tag == "Player")
        {
            Debug.Log("플레이어와 충돌");
            StopDash();
        }
    }

    // 돌진 멈추고 상태 초기화
    public void StopDash()
    {
        rigid.velocity = Vector2.zero;      // 정지
        isDashed = false;                   // 돌진 상태 해제
        anim.SetBool("isAttack", false); // ✅ 공격 애니메이션 OFF
        
        moveScript.enabled = true;          // 평소 이동 재개
    }

    // 에디터에서 감지 반경 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
