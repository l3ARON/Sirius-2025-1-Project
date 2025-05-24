using System.Collections;
using UnityEngine;

public class middleEnemyAttack : MonoBehaviour
{
    // 플레이어 트랜스폼 (타겟)
    public Transform player;

    // 플레이어 감지 범위
    public float detectionRadius = 3f;

    // 돌진 속도 및 돌진 전 대기 시간
    public float dashSpeed = 5f;
    public float waitForAttack = 1f;

    private Rigidbody2D rigid;                  // Rigidbody2D 컴포넌트 참조
    private middleEnemyMove moveScript;          // 평소 이동 스크립트 참조
    private bool isDetected = false;            // 플레이어 감지 여부
    private bool isDashed = false;              // 현재 돌진 중 여부
    private Vector2 dashDirection;              // 돌진할 방향

    void Start()
    {
        // 컴포넌트 가져오기
        rigid = GetComponent<Rigidbody2D>();
        moveScript = GetComponent<middleEnemyMove>();

        // 시작 시 평소 이동 스크립트 활성화
        moveScript.enabled = true;
    }

    void Update()
    {
        if (isDashed) return;

        // 플레이어와의 위치 차이
        Vector2 delta = player.position - transform.position;

        // 타원 정규화 (찌그러진 범위 기준)
        float dx = delta.x / (detectionRadius * 1.5f);  // 가로 감지 범위
        float dy = delta.y / (detectionRadius * 0.7f);  // 세로 감지 범위

        // 타원 내부 여부 판단 (x/a)^2 + (y/b)^2 <= 1
        bool inEllipse = (dx * dx + dy * dy) <= 1f;

        if (inEllipse && !isDetected)
        {
            Debug.Log("감지됨!");
            OnPlayerDetected(); // 감지 시 행동
        }
        else if (!inEllipse && isDetected)
        {
            Debug.Log("범위 밖으로 나감");
            OnPlayerLost(); // 범위 벗어남 시 행동
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
    void StopDash()
    {
        rigid.velocity = Vector2.zero;      // 정지
        isDashed = false;                   // 돌진 상태 해제
        
        moveScript.enabled = true;          // 평소 이동 재개
    }

    // 에디터에서 감지 반경 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // 현재 위치를 기준으로 타원의 중심 설정
        Vector3 position = transform.position;

        // 비율 설정 (가로/세로 비율 다르게)
        float radiusX = detectionRadius * 1.5f;  // 가로 감지 반경
        float radiusY = detectionRadius * 0.7f;  // 세로 감지 반경

        // 타원 그리기 위해 좌표계를 스케일링
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(radiusX, radiusY, 1f));
        Gizmos.DrawWireSphere(Vector3.zero, 1f); // 스케일된 좌표계에서 단위 원을 그림
        Gizmos.matrix = originalMatrix;          // 원래 좌표계로 복원
    }

}