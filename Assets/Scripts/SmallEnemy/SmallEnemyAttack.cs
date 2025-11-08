using System.Collections;
using UnityEngine;

/// <summary>
/// 소형 적: 플레이어 감지 → 잠시 정지 → 돌진 공격
/// (공격 중에는 SmallEnemyMovement를 멈추고, 끝나면 다시 순찰)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SmallEnemyAttack : MonoBehaviour
{
    [Header("감지 / 돌진 설정")]
    public float detectionRadius = 3.5f;   // 플레이어 감지 범위
    public float waitBeforeDash = 0.5f;    // 감지 후 돌진 전 대기 시간
    public float dashSpeed = 6f;           // 돌진 속도
    public float dashDuration = 0.8f;      // 돌진 유지 시간
    public float attackCooldown = 1.5f;    // 공격 쿨타임
    public int damage = 1;                 // 플레이어에게 줄 데미지

    [Header("사운드")]
    public AudioSource attackSound;

    Transform player;
    Rigidbody2D rigid;
    SmallEnemyMovement movement;
    Animator anim;
    SpriteRenderer sprite;

    bool canAttack = true;
    bool isDashing = false;
    bool hasHitPlayer = false; // ⚡ 플레이어 중복 타격 방지

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        movement = GetComponent<SmallEnemyMovement>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // 돌진 중에는 새 감지 안 함
        if (!isDashing)
            DetectAndAttack();
    }

    void DetectAndAttack()
    {
        if (player == null || !canAttack) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= detectionRadius)
        {
            StartCoroutine(DashAttackRoutine());
        }
    }

    IEnumerator DashAttackRoutine()
    {
        canAttack = false;
        hasHitPlayer = false; // ⚡ 돌진 시작 시 리셋

        // 1️⃣ 이동 멈추기
        if (movement != null) movement.PauseMovement();
        rigid.velocity = Vector2.zero;

        Debug.Log("[SmallEnemyAttack] 플레이어 감지 → 대기");
        yield return new WaitForSeconds(waitBeforeDash);

        if (player == null)
        {
            EndAttack();
            yield return new WaitForSeconds(attackCooldown);
            canAttack = true;
            yield break;
        }

        // 2️⃣ 돌진 시작
        isDashing = true;
        if (attackSound != null) attackSound.Play();

        Vector2 dir = (player.position - transform.position).normalized;
        if (sprite != null)
            sprite.flipX = dir.x > 0;

        if (anim != null) anim.SetBool("isAttack", true);
        rigid.velocity = dir * dashSpeed;

        // 3️⃣ 돌진 유지 시간 동안 플레이어와 충돌을 기다림
        yield return new WaitForSeconds(dashDuration);

        // 4️⃣ 돌진 종료
        rigid.velocity = Vector2.zero;
        isDashing = false;

        // 5️⃣ 공격 종료 처리
        EndAttack();

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // ⚔️ 실제 충돌 시 데미지 적용
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDashing || hasHitPlayer == true) return; // 돌진 중이 아니거나 이미 타격한 경우 무시

        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("[SmallEnemyAttack] 플레이어와 충돌 → 데미지 적용");
            hasHitPlayer = true; // 중복 데미지 방지

            PlayerDamage2D playerDamage = collision.collider.GetComponent<PlayerDamage2D>();
            if (playerDamage != null)
            {
                playerDamage.OnDamaged(transform.position);
            }

            // 돌진 즉시 멈춤
            rigid.velocity = Vector2.zero;
            isDashing = false;

            EndAttack();
        }
    }

    void EndAttack()
    {
        if (anim != null)
            anim.SetBool("isAttack", false);

        if (movement != null)
            movement.ResumeMovement();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
