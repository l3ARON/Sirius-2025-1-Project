using System.Collections;
using UnityEngine;

public class MiddleEnemyAttack : MonoBehaviour
{
    [Header("범위 설정")]
    public Vector2 hitBoxSize = new Vector2(3f, 1.5f);
    public Vector2 hitBoxOffset = new Vector2(1f, 0f);
    public LayerMask playerMask;

    [Header("공격 타이밍")]
    public float windUpTime = 0.8f;    
    public float dashSpeed = 6f;       
    public float dashDuration = 0.5f;  
    public float cooldown = 1.0f;      

    [Header("타격 타이밍")]
    public float hitStartTime = 0.1f;  
    public float hitWindow = 0.3f;     

    [Header("이펙트")]
    public GameObject slashPrefab;
    public Vector2 slashOffset = new Vector2(1f, 0f);
    public float slashLifeTime = 0.4f;

    // 컴포넌트 & 상태
    Transform player;
    Rigidbody2D rigid;
    MiddleEnemyMovement move;
    Animator anim;

    [HideInInspector] public bool isAttacking = false;
    private bool canHitPlayer = false; 
    public bool CanHitPlayer => canHitPlayer;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        move  = GetComponent<MiddleEnemyMovement>();
        anim  = GetComponent<Animator>();
    }

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        // 스크립트가 꺼져있거나(아군), 이미 공격중이거나, 플레이어가 없으면 리턴
        if (!this.enabled || isAttacking || player == null) return;

        if (IsPlayerInHitBox())
        {
            StartCoroutine(AttackRoutine());
        }
    }

    // 🔥 Controller에서 호출: 피격/패링 시 공격 강제 중단
    public void ForceStopAttack()
    {
        StopAllCoroutines();
        isAttacking = false;
        canHitPlayer = false;

        if (rigid != null) rigid.velocity = Vector2.zero;
        
        if (anim != null)
        {
            anim.SetBool("isReady", false);
            anim.SetBool("isAttack", false);
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        canHitPlayer = false;

        // 1. 준비
        move?.PauseMovement();
        rigid.velocity = Vector2.zero;

        anim?.SetBool("isWalk", false);
        anim?.SetBool("isRun", false);
        anim?.SetBool("isReady", true);

        yield return new WaitForSeconds(windUpTime);

        // 2. 공격 시작
        anim?.SetBool("isReady", false);
        anim?.SetBool("isAttack", true);

        CreateSlashEffect();

        // 3. 돌진
        if (player != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rigid.velocity = dir * dashSpeed;
        }

        // 4. 히트 윈도우
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            if (elapsed >= hitStartTime && elapsed <= hitStartTime + hitWindow)
                canHitPlayer = true;
            else
                canHitPlayer = false;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 5. 종료
        rigid.velocity = Vector2.zero;
        anim?.SetBool("isAttack", false);
        canHitPlayer = false;

        if (cooldown > 0f) yield return new WaitForSeconds(cooldown);

        isAttacking = false;

        // 6. 반복 결정
        if (IsPlayerInHitBox())
            StartCoroutine(AttackRoutine());
        else
            move?.ResumeMovement();
    }

    void CreateSlashEffect()
    {
        if (slashPrefab == null) return;

        // 몬스터의 Scale.x를 기준으로 방향 판단 (양수:오른쪽, 음수:왼쪽)
        float dirSign = Mathf.Sign(transform.localScale.x);
        Vector3 spawnPos = transform.position + new Vector3(slashOffset.x * dirSign, slashOffset.y, 0);

        // 오른쪽이면 0도, 왼쪽이면 180도 회전
        float yRot = (dirSign > 0) ? 0f : 180f;
        Quaternion rot = Quaternion.Euler(0, yRot, 0);

        GameObject slash = Instantiate(slashPrefab, spawnPos, rot, transform);
        Destroy(slash, slashLifeTime);
    }

    bool IsPlayerInHitBox()
    {
        float dir = Mathf.Sign(transform.localScale.x);
        Vector2 center = (Vector2)transform.position + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);
        return Physics2D.OverlapBox(center, hitBoxSize, 0f, playerMask) != null;
    }

    public void OnSlashHitPlayer(Collider2D other)
    {
        if (!isAttacking || !canHitPlayer) return;
        if (!other.CompareTag("Player")) return;

        PlayerDamage2D pd = other.GetComponent<PlayerDamage2D>();
        if (pd != null)
        {
            Debug.Log($"⚔️ [MiddleEnemy] 플레이어 적중!");
            pd.OnDamaged(transform.position);
            
            // 다단 히트 방지
            canHitPlayer = false; 
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float dir = Mathf.Sign(transform.localScale.x);
        Vector2 center = Application.isPlaying 
            ? (Vector2)transform.position + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y)
            : (Vector2)transform.position + hitBoxOffset;
        
        Gizmos.DrawWireCube(center, hitBoxSize);
    }
}