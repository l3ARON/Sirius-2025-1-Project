using System.Collections;
using UnityEngine;

public class SmallEnemy : MonoBehaviour
{
    // 체력 관련 변수
    public int maxHP = 100;
    private int currentHP;

    // 기본 이동 관련 변수
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    public float xSpeed = 1f;            // x축 속도
    public float ySpeed = 1f;            // y축 시작 속도
    public float minRayLength = 2.3f;    // ground기준 최소 하강 높이
    public float maxRayLength = 4f;      // ground기준 최대 상승 높이

    // 플레이어 감지 및 돌진 관련 변수
    public Transform player;
    public float detectionRadius = 3.8f;
    public float dashSpeed = 5f;
    public float waitForAttack = 0.5f;
    private bool isDetected = false;
    private bool isDashed = false;
    private Vector2 dashDirection;
    private bool isMovementPaused = false;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHP = maxHP;

        StartCoroutine(ChangeYSpeedRoutine());
        StartCoroutine(ChangeXSpeedRoutine());
    }

    void Update()
    {
        if (isDashed) return;

        // 플레이어 감지 로직
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= detectionRadius && !isDetected)
        {
            OnPlayerDetected();
        }
        else if (distance > detectionRadius && isDetected)
        {
            OnPlayerLost();
        }
    }

    void FixedUpdate()
    {
        if (isDashed || isMovementPaused) return;

        // 기본 이동 로직
        spriteRenderer.flipX = xSpeed > 0;
        rigid.velocity = new Vector2(xSpeed, ySpeed);

        LayerMask groundMask = LayerMask.GetMask("Platform");

        // 최대 상승 체크
        Debug.DrawRay(rigid.position, Vector3.down * maxRayLength, Color.red);
        RaycastHit2D rayHitHigh = Physics2D.Raycast(rigid.position, Vector3.down, maxRayLength, groundMask);
        if (rayHitHigh.collider == null || IsSelfOrChild(rayHitHigh.collider.gameObject))
        {
            ySpeed = -1 * Mathf.Abs(ySpeed); // 무조건 아래로
        }

        // 최대 하강 체크
        Debug.DrawRay(rigid.position, Vector3.down * minRayLength, Color.green);
        RaycastHit2D rayHitLow = Physics2D.Raycast(rigid.position, Vector3.down, minRayLength, groundMask);
        if (rayHitLow.collider != null && !IsSelfOrChild(rayHitLow.collider.gameObject))
        {
            ySpeed = Mathf.Abs(ySpeed); // 무조건 위로
        }
    }

    // 데미지 처리
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log("몬스터 피해! 현재 HP: " + currentHP);

        if (currentHP <= 0)
        {
            Debug.Log("몬스터 사망");
            Destroy(gameObject);
        }
    }

    // 플레이어 감지 시 동작
    void OnPlayerDetected()
    {
        isDetected = true;
        isMovementPaused = true;
        rigid.velocity = Vector2.zero;
        StartCoroutine(PerformDash());
    }

    // 플레이어 감지 해제 시 동작
    void OnPlayerLost()
    {
        isDetected = false;
        isDashed = false;
        isMovementPaused = false;
    }

    // 돌진 실행
    IEnumerator PerformDash()
    {
        Debug.Log("돌진 대기");
        yield return new WaitForSeconds(waitForAttack);

        isDashed = true;
        Debug.Log("돌진 시작");

        Vector2 targetPosition = player.position;
        dashDirection = (targetPosition - (Vector2)transform.position).normalized;
        rigid.velocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(1f);

        if (isDashed)
        {
            Debug.Log("돌진 시간 만료, 자동 정지");
            StopDash();
        }
    }

    // 충돌 감지
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDashed) return;

        string tag = collision.collider.tag;
        if (tag == "Ground" || tag == "Player")
        {
            Debug.Log($"{tag}와(과) 충돌");
            StopDash();
        }
    }

    // 돌진 중지
    void StopDash()
    {
        rigid.velocity = Vector2.zero;
        isDashed = false;
        isMovementPaused = false;
    }

    // Y축 속도 변경 코루틴
    IEnumerator ChangeYSpeedRoutine()
    {
        while (true)
        {
            if (!isMovementPaused && !isDashed)
            {
                ySpeed *= -1;
            }
            yield return new WaitForSeconds(Random.Range(0.5f, 3.0f));
        }
    }

    // X축 속도 변경 코루틴
    IEnumerator ChangeXSpeedRoutine()
    {
        while (true)
        {
            if (!isMovementPaused && !isDashed)
            {
                xSpeed *= -1;
            }
            yield return new WaitForSeconds(5f);
        }
    }

    // 감지 범위 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // 자기 자신 또는 자신의 자식 오브젝트라면 무시
    bool IsSelfOrChild(GameObject obj)
    {
        return obj == this.gameObject || obj.transform.IsChildOf(this.transform);
    }
} 