using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 근접 공격 + 원거리 공격
/// </summary>
public class PlayerAttack2D : MonoBehaviour
{
    [Header("Melee")]
    public float attackDelay = 0.5f;    // 공격 지속 시간
    public float attackRange = 1f;      // 근접 공격 범위
    public int attackDamage = 1;       // 데미지
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();

    [Header("Ranged")]
    public GameObject longRangeAttackPrefab;
    public float rangedAttackDelay = 0.5f;
    private float rangedAttackTimer = 0f;
    private bool isNextAttackRanged = false;

    [Header("FX")]
    public GameObject slashPrefab;      // 근접 공격 슬라이스 이펙트
    public float slashOffset = 0.5f;    // 플레이어 앞쪽으로 얼마만큼 띄울지

    PlayerRefs refs;

    void Start()
    {
        refs = GetComponent<PlayerRefs>();
    }

    void Update()
    {
        HandleAttackInput();
        HandleMeleeProgress();
    }

    /// <summary>
    /// 다음 공격을 원거리로 바꾸고 싶을 때 외부에서 호출
    /// </summary>
    public void EnableNextRangedAttack()
    {
        isNextAttackRanged = true;
    }

    // 공격 키 입력
    void HandleAttackInput()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            // 원거리 공격 조건
            if (isNextAttackRanged && longRangeAttackPrefab != null && refs.firePoint != null)
            {
                DoRangedAttack();
            }
            else
            {
                DoMeleeAttack();
            }
        }
    }

    // 근접 공격 시작
    void DoMeleeAttack()
    {
        isAttacking = true;
        attackTimer = attackDelay;
        damagedEnemies.Clear();

        if (refs.anim != null)
            refs.anim.SetBool("isAttack", true);
        if (refs.attackClip != null)
            refs.attackClip.Play();

        // 🔥 슬라이스 이펙트 생성 + 자동 삭제
        if (slashPrefab != null)
        {
            float dir = refs.spriteRenderer != null && refs.spriteRenderer.flipX ? 1f : -1f;

            Vector3 basePos = refs.attackPoint != null
                ? refs.attackPoint.position
                : transform.position;

            Vector3 spawnPos = basePos + Vector3.right * (-dir) * slashOffset;
            float yRot = (refs.spriteRenderer != null && refs.spriteRenderer.flipX) ? 0f : 180f;
            Quaternion rot = Quaternion.Euler(0, yRot, 0);

            GameObject slash = Instantiate(slashPrefab, spawnPos, rot);

            // 👉 애니메이션 길이에 맞춰 적당히 0.3 ~ 0.5f 정도로
            Destroy(slash, 0.4f);
        }
    }

    // 원거리 공격 실행
    void DoRangedAttack()
    {
        if (refs.attackClip != null)
            refs.attackClip.Play();

        GameObject proj = Instantiate(longRangeAttackPrefab, refs.firePoint.position, Quaternion.identity);

        // 발사 방향 설정
        var lr = proj.GetComponent<LongRangeAttack>();
        if (lr != null)
        {
            // flipX 기준으로 방향 정하기
            lr.direction = refs.spriteRenderer.flipX ? Vector2.right : Vector2.left;
        }

        isNextAttackRanged = false;
        rangedAttackTimer = rangedAttackDelay;

        Debug.Log("🎯 원거리 공격 발사");
    }

    // 근접 공격이 진행 중일 때 범위 내 적에게 데미지 주기
    void HandleMeleeProgress()
    {
        if (!isAttacking) return;

        attackTimer -= Time.deltaTime;

        // 공격 범위 검색
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(refs.attackPoint.position, attackRange);
        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Enemy") && !damagedEnemies.Contains(collider.gameObject))
            {
                // Enemy에 TakeDamage가 있으면 호출
                var components = collider.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    var method = component.GetType().GetMethod("TakeDamage");
                    if (method != null)
                    {
                        method.Invoke(component, new object[] { attackDamage });
                        damagedEnemies.Add(collider.gameObject);
                        Debug.Log($"💥 {collider.gameObject.name}에게 {attackDamage} 데미지!");
                        break;
                    }
                }
            }
        }

        // 공격 끝나면 초기화
        if (attackTimer <= 0)
        {
            isAttacking = false;
            damagedEnemies.Clear();
            if (refs.anim != null)
                refs.anim.SetBool("isAttack", false);
        }
    }
}
