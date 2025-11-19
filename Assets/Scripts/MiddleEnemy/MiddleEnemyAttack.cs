using System.Collections;
using UnityEngine;

/// <summary>
/// 중형 몬스터 2차 근접 공격 + 패링 타이밍
/// Idle/Walk/Run → ready → attack → Idle
/// </summary>
public class MiddleEnemyAttack : MonoBehaviour
{
    [Header("2차 공격 범위")]
    public float closeRange = 2f;          // 공격 발동 범위

    [Header("공격 설정")]
    public float windUpTime = 0.8f;       // ready 상태로 준비하는 시간
    public float dashSpeed = 6f;          // 돌진 속도
    public float dashDuration = 1.0f;     // 돌진 유지 시간
    public float cooldown = 2.0f;         // 공격 후 다시 공격 가능해지기까지 대기 시간

    [Header("히트/패링 타이밍")]
    [Tooltip("돌진 시작 후 몇 초 뒤에 실제 타격 판정을 열지")]
    public float hitStartTime = 0.3f;
    [Tooltip("실제 타격이 유효한 시간 (패링 타이밍 길이)")]
    public float hitWindow = 0.2f;

    [Header("데미지")]
    public int damage = 1;

    Transform player;
    Rigidbody2D rigid;
    MiddleEnemyMovement move;
    MiddleEnemyController controller;
    Animator anim;

    [HideInInspector] public bool isAttacking = false; // 전체 공격 루틴 중인지
    bool canHitPlayer = false;                         // 실제로 데미지 줄 수 있는 타이밍인지

    void Awake()
    {
        rigid      = GetComponent<Rigidbody2D>();
        move       = GetComponent<MiddleEnemyMovement>();
        controller = GetComponent<MiddleEnemyController>();
        anim       = GetComponent<Animator>();
    }

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (isAttacking) return;
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // 2차 근접 범위 안에 들어오면 공격 시작
        if (dist <= closeRange)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        canHitPlayer = false;

        // 0️⃣ 이동 멈추기
        if (move != null)
            move.PauseMovement();

        // 1️⃣ ready 애니메이션으로 준비 (isReady = true)
        if (anim != null)
        {
            anim.SetBool("isWalk", false);
            anim.SetBool("isRun",  false);
            anim.SetBool("isAttack", false);
            anim.SetBool("isReady",  true);   // ← ready 상태로 진입
        }

        // ready 애니메이션 동안 대기 (텔레그래프 + 패링 준비 시간)
        yield return new WaitForSeconds(windUpTime);

        if (player == null)
        {
            EndAttack();
            yield break;
        }

        // 2️⃣ ready 종료, 실제 공격 애니로 전환
        if (anim != null)
        {
            anim.SetBool("isReady",  false);
            anim.SetBool("isAttack", true);   // Animator에서 ready → wolf_attack 전이는 Exit Time=1로!
        }

        // 3️⃣ 돌진 시작
        Vector2 dir = (player.position - transform.position).normalized;
        rigid.velocity = dir * dashSpeed;

        float elapsed = 0f;
        canHitPlayer = false;

        // 4️⃣ 돌진 전체 시간 동안 진행 + 그 안에서 일부만 유효타임
        while (elapsed < dashDuration)
        {
            // hitStartTime ~ hitStartTime+hitWindow 구간 안에서만 판정 ON
            if (elapsed >= hitStartTime && elapsed <= hitStartTime + hitWindow)
            {
                canHitPlayer = true;   // 이 구간이 패링 타이밍
            }
            else
            {
                canHitPlayer = false;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 5️⃣ 돌진 종료
        rigid.velocity = Vector2.zero;
        EndAttack();

        // 6️⃣ 쿨타임
        yield return new WaitForSeconds(cooldown);

        isAttacking = false;
        canHitPlayer = false;
    }

    void EndAttack()
    {
        if (anim != null)
        {
            anim.SetBool("isAttack", false);
            anim.SetBool("isReady",  false);
        }

        if (move != null)
            move.ResumeMovement();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 전체 공격 중이면서 + 히트 윈도우 안인 경우에만 데미지
        if (!isAttacking || !canHitPlayer) return;

        if (collision.collider.CompareTag("Player"))
        {
            PlayerDamage2D pd = collision.collider.GetComponent<PlayerDamage2D>();
            if (pd != null)
            {
                pd.OnDamaged(transform.position);
            }

            // 맞추면 바로 멈추고 공격 종료
            rigid.velocity = Vector2.zero;
            canHitPlayer = false;
            EndAttack();
            isAttacking = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, closeRange);
    }
}
