using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveJunHyung : MonoBehaviour
{
    Rigidbody2D rigid;                    // 물리 연산을 위한 Rigidbody2D 참조
    SpriteRenderer spriteRenderer;        // 캐릭터 방향 좌우 반전을 위한 SpriteRenderer
    Animator anim;                        // 애니메이션 전환을 위한 Animator

    // ────────────── 이동 & 점프 관련 변수 ──────────────
    public float maxSpeed;                 // 걷기 최대 속도
    public float jumpPower;               // 점프 시 위로 튀는 힘의 크기

    // ────────────── 공격 관련 변수 ──────────────
    private bool isAttacking = false;     // 현재 공격 중인지 여부
    public float attackDelay = 0.5f;      // 공격 지속 시간 (애니메이션 + 판정 유지 시간)
    private float attackTimer = 0f;       // 남은 공격 시간 카운터
    public LayerMask enemyLayer;          // 어떤 레이어를 적으로 인식할지 설정
    public Transform attackPoint;         // 공격 판정 중심점 (손/무기 위치로 설정)
    public float attackRange = 1f;        // 공격 범위 반지름
    public int attackDamage = 20;         // 적에게 줄 데미지
    private HashSet<Collider2D> damagedEnemies = new HashSet<Collider2D>(); // 동일 공격 중 중복 타격 방지

    // ────────────── 대시 관련 변수 ──────────────
    public float dashSpeed = 15f;         // 대시 시 속도
    public float dashDuration = 0.2f;     // 대시 지속 시간 (초)
    private bool isDashing = false;       // 대시 상태 여부
    private float dashTimer = 0f;         // 대시 지속 시간 카운터
    public float dashCooldown = 5f;       // 대시 쿨타임 (초)
    private float lastDashTime = -999f;   // 마지막 대시 시각
    private bool isCooldown = false;      // 대시 쿨타임 진행 중 여부

    // 대시 쿨타임 카운트다운 로그 출력 코루틴
    private IEnumerator DashCooldownCountdown()
    {
        isCooldown = true;
        int remaining = Mathf.CeilToInt(dashCooldown);

        while (remaining > 0)
        {
            Debug.Log($"⏳ Dash Cooldown: {remaining}");  // 1초 단위 남은 시간 출력
            yield return new WaitForSeconds(1f);
            remaining--;
        }

        isCooldown = false;
        Debug.Log("✅ Dash Ready!"); // 대시 가능 상태 알림
    }

    // ───── 컴포넌트 초기화 ─────
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // ───── 키 입력 및 애니메이션 제어 ─────
    void Update()
    {
        // 점프 입력 처리 (공중 아닐 때만 가능)
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJump"))
        {

            rigid.velocity = new Vector2(rigid.velocity.x, 0f); // Y속도 초기화 (더블 점프 방지)
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse); // 점프 힘 가하기
            anim.SetBool("isJump", true); // 점프 상태로 전환


        }

        // 입력이 없을 경우 감속 처리 (자연스러운 정지 구현)
        float deceleration = 0.9f;
        if (Input.GetAxisRaw("Horizontal") == 0)
        {
            rigid.velocity = new Vector2(rigid.velocity.x * deceleration, rigid.velocity.y);
        }

        // 좌우 입력이 있을 경우 캐릭터 방향 반전 및 공격 포인트 반전
        float h = Input.GetAxisRaw("Horizontal");
        if (h != 0)
        {
            spriteRenderer.flipX = h > 0; // 왼쪽 입력 시 flipX = true

            // 공격 범위 위치도 방향에 맞춰 좌우 반전
            float attackX = Mathf.Abs(attackPoint.localPosition.x);
            attackPoint.localPosition = new Vector3(
                spriteRenderer.flipX ? attackX : -attackX,
                attackPoint.localPosition.y,
                attackPoint.localPosition.z
            );
        }

        // 걷기 애니메이션 상태 업데이트 (속도에 따라)
        anim.SetBool("isWalk", !anim.GetBool("isJump") && Mathf.Abs(rigid.velocity.x) >= 0.3f);

        // 공격 시작 입력 처리
        if (Input.GetButtonDown("Fire1") && !isAttacking && !isDashing)
        {
            isAttacking = true;
            anim.SetBool("isAttack", true);
            attackTimer = attackDelay;
            damagedEnemies.Clear(); // 중복 타격 방지용 HashSet 초기화
        }

        // 공격 지속 시간 동안 적 타격 처리
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;

            // 공격 범위 내 적 탐지 및 데미지 부여
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
            foreach (Collider2D enemy in hitEnemies)
            {
                if (!damagedEnemies.Contains(enemy))
                {
                    // Monster monster = enemy.GetComponent<Monster>();
                    // if (monster != null)
                    // {
                    //     monster.TakeDamage(attackDamage);
                    //     damagedEnemies.Add(enemy);
                    // }
                }
            }

            // 공격 종료 처리
            if (attackTimer <= 0)
            {
                isAttacking = false;
                anim.SetBool("isAttack", false);
                damagedEnemies.Clear();
            }
        }

        // 대시 입력 처리 (쿨타임과 중복 대시 방지 포함)
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && !isCooldown)
        {
            isDashing = true;
            dashTimer = dashDuration;
            lastDashTime = Time.time;
            anim.SetBool("isDashed", true);

            StartCoroutine(DashCooldownCountdown()); // 쿨타임 카운트 시작
        }
    }

    // ───── 물리 이동 처리 ─────
    void FixedUpdate()
    {
        // 대시 중일 때 빠르게 방향 이동 (Y속도 고정으로 위 튐 방지)
        if (isDashing)
        {
            float dashDirection = spriteRenderer.flipX ? 1f : -1f;
            rigid.velocity = new Vector2(dashDirection * dashSpeed, 0f);

            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                anim.SetBool("isDashed", false);
            }
            return; // 대시 중 일반 이동 무시
        }

        // 일반 이동 처리 (방향키 입력에 따라 속도 조절)
        float h = Input.GetAxisRaw("Horizontal");
        rigid.velocity = new Vector2(h * maxSpeed, rigid.velocity.y);

        // 속도가 최대값을 넘지 않도록 제한
        if (rigid.velocity.x > maxSpeed)
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if (rigid.velocity.x < -maxSpeed)
            rigid.velocity = new Vector2(-maxSpeed, rigid.velocity.y);

        // 점프 후 낙하 중일 때 바닥 착지 판정
        if (rigid.velocity.y < -0.3)
        {
            Vector2 pos = rigid.position;   // Ray 시작 지점
            float rayLength = 1.5f;
            LayerMask groundMask = LayerMask.GetMask("flatform");

            RaycastHit2D center = Physics2D.Raycast(pos, Vector2.down, rayLength, groundMask);
            RaycastHit2D left = Physics2D.Raycast(pos + Vector2.left * 0.3f, Vector2.down, rayLength, groundMask);
            RaycastHit2D right = Physics2D.Raycast(pos + Vector2.right * 0.3f, Vector2.down, rayLength, groundMask);

            if ((center.collider != null && center.normal.y > 0.7f) ||
                (left.collider != null && left.normal.y > 0.7f) ||
                (right.collider != null && right.normal.y > 0.7f))
            {
                anim.SetBool("isJump", false);
            }
        }
    }

    void Start() { }

    // ───── 공격 범위 디버그용 시각화 ─────
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
