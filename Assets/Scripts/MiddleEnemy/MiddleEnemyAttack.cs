using System.Collections;
using UnityEngine;

public class MiddleEnemyAttack : MonoBehaviour
{
    [Header("타겟 레이어")]
    public LayerMask playerLayer; 
    public LayerMask enemyLayer;  
    private LayerMask currentTargetMask;

    [Header("공격 정보")]
    public int damage = 1;
    // 🔥 이 범위(HitBox)가 이제 실제 타격 범위가 됩니다.
    public Vector2 hitBoxSize = new Vector2(2.5f, 1.5f); 
    public Vector2 hitBoxOffset = new Vector2(1.5f, 0f); // 늑대 앞쪽으로 설정
    
    [Header("타이밍")]
    public float windUpTime = 0.5f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.4f;
    public float cooldown = 1.5f;
    public float hitStart = 0.0f;      
    public float hitEnd = 0.4f;

    // 상태 변수
    Transform currentTarget;
    [HideInInspector] public bool isAttacking = false;
    private bool hasHitThisTurn = false; // 이번 턴 타격 여부
    
    Rigidbody2D rigid;
    MiddleEnemyMovement move;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        move = GetComponent<MiddleEnemyMovement>();
        anim = GetComponent<Animator>();
        currentTargetMask = playerLayer; 
    }

    void Update()
    {
        if (isAttacking || currentTarget == null) return;

        // 공격 사거리 체크 (이건 공격 시작 조건)
        if (CheckHitBox(false)) // false = 데미지 안 줌, 체크만 함
        {
            StartCoroutine(AttackRoutine());
        }
    }

    public void SetTarget(Transform target) => currentTarget = target;

    public void SetFriendlyMode()
    {
        currentTargetMask = enemyLayer; 
        ForceStopAttack();
    }

    public void ForceStopAttack()
    {
        StopAllCoroutines();
        isAttacking = false;
        hasHitThisTurn = false;
        if (rigid != null) rigid.velocity = Vector2.zero;
        if (anim != null) { anim.SetBool("isReady", false); anim.SetBool("isAttack", false); }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        hasHitThisTurn = false; // 초기화
        
        move.PauseMovement();
        rigid.velocity = Vector2.zero;

        // 1. 준비
        anim.SetBool("isWalk", false);
        anim.SetBool("isRun", false);
        anim.SetBool("isReady", true);
        yield return new WaitForSeconds(windUpTime);

        // 2. 돌진
        anim.SetBool("isReady", false);
        anim.SetBool("isAttack", true);

        if (currentTarget != null)
        {
            Vector2 dir = (currentTarget.position - transform.position).normalized;
            rigid.velocity = dir * dashSpeed;
        }
        else
        {
            float dirX = Mathf.Sign(transform.localScale.x);
            rigid.velocity = new Vector2(dirX * dashSpeed, 0);
        }

        // 3. 히트 판정 루프 (직접 범위 검사)
        float t = 0f;
        while (t < dashDuration)
        {
            // 타격 시간대 + 아직 안 때렸으면 -> 범위 검사 시도
            if (t >= hitStart && t <= hitEnd && !hasHitThisTurn)
            {
                // 🔥 여기서 실제 데미지를 줍니다 (true = 데미지 줌)
                if (CheckHitBox(true)) 
                {
                    hasHitThisTurn = true; // 때렸으니 플래그 ON
                }
            }

            t += Time.deltaTime;
            yield return null;
        }

        // 4. 종료
        rigid.velocity = Vector2.zero;
        anim.SetBool("isAttack", false);

        yield return new WaitForSeconds(cooldown);

        isAttacking = false;
        move.ResumeMovement();
    }

    // 🔥 [핵심] 물리 접촉이 아니라, 수학적 범위 검사로 때림
    bool CheckHitBox(bool dealDamage)
    {
        float dir = Mathf.Sign(transform.localScale.x);
        Vector2 center = (Vector2)transform.position + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);
        
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, hitBoxSize, 0f, currentTargetMask);

        bool hitSomething = false;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue; 

            if (!dealDamage) return true; 

            bool success = false;

            // 1️⃣ 더미
            var dummyCtrl = hit.GetComponent<dummy>();
            if (dummyCtrl != null)
            {
                dummyCtrl.TakeDamage(damage);
                success = true;
                Debug.Log("🎯 [RangeCheck] 더미 타격!");
            }
            // 2️⃣ 태그가 'Player'인 경우 (진짜 플레이어 OR 아군 늑대)
            else if (hit.CompareTag("Player"))
            {
                 if ((currentTargetMask & (1 << hit.gameObject.layer)) != 0)
                 {
                     // A. 진짜 플레이어인지 확인
                     var pd = hit.GetComponent<PlayerDamage2D>();
                     if (pd != null) 
                     {
                         pd.OnDamaged(transform.position);
                         success = true;
                         Debug.Log("🎯 [RangeCheck] 플레이어 타격!");
                     }
                     
                     // 🔥 B. [추가] 아군 늑대인지 확인 (태그는 Player지만 스크립트는 Controller)
                     var friendlyWolf = hit.GetComponent<MiddleEnemyController>();
                     if (friendlyWolf != null)
                     {
                         // 아군 늑대에게 데미지 전달
                         friendlyWolf.ApplyDamage(damage, true, transform.position);
                         success = true;
                         Debug.Log("🎯 [RangeCheck] 아군 늑대 타격!");
                     }
                 }
            }
            // 3️⃣ 태그가 'Enemy'인 경우 (적 늑대, 보스 등)
            else if (hit.CompareTag("Enemy") || hit.CompareTag("Boss"))
            {
                if ((currentTargetMask & (1 << hit.gameObject.layer)) != 0)
                {
                    var mid = hit.GetComponent<MiddleEnemyController>();
                    if (mid != null) mid.ApplyDamage(damage, true, transform.position);

                    var small = hit.GetComponent<SmallEnemyController>();
                    if (small != null) 
                    {
                        // 🔥 [수정] 마지막에 'false' 추가 -> "나는 플레이어가 아님"
                        // 늑대가 죽여도 원거리 공격이 충전되지 않음
                        small.TakeDamage(damage, true, false); 
                    }
                    
                    success = true;
                    Debug.Log($"🎯 [RangeCheck] 적({hit.name}) 타격!");
                }
            }

            if (success) hitSomething = true;
        }

        return hitSomething;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; // 공격 범위는 빨간색으로 표시
        float dir = Mathf.Sign(transform.localScale.x);
        Vector2 center = (Vector2)transform.position + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);
        Gizmos.DrawWireCube(center, hitBoxSize);
    }
}