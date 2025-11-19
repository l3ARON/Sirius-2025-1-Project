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

    public GameObject weapon2;   // UI나 무기 오브젝트


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

        if (weapon2 != null)
            weapon2.SetActive(true);
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

        var lr = proj.GetComponent<LongRangeAttack>();
        if (lr != null)
        {
            lr.direction = refs.spriteRenderer.flipX ? Vector2.right : Vector2.left;
        }

        isNextAttackRanged = false;
        rangedAttackTimer = rangedAttackDelay;

        // 🔥 발사 후 weapon2 비활성화
        if (weapon2 != null)
            weapon2.SetActive(false);

        Debug.Log("🎯 원거리 공격 발사");
    }


    // 근접 공격이 진행 중일 때
    void HandleMeleeProgress()
    {
        if (!isAttacking) return;

        attackTimer -= Time.deltaTime;

        // 공격 끝나면 애니메이션 끄기
        if (attackTimer <= 0)
        {
            isAttacking = false;
            if (refs.anim != null)
                refs.anim.SetBool("isAttack", false);
        }
    }

    public void DealDamageToEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        Debug.Log("[PlayerAttack2D] 근접 타격, target = " + enemy.name);

        // 1) 소형 몬스터 체크
        SmallEnemyController small = enemy.GetComponentInParent<SmallEnemyController>();
        if (small != null)
        {
            small.TakeDamage(attackDamage, true);
            Debug.Log($"💥 [Small] {small.gameObject.name}에게 {attackDamage} 데미지!");
            return;
        }

        // 2) 🔥 [추가] 중형 몬스터(Middle) 체크 (이제 직접 찾아서 부릅니다)
        MiddleEnemyController middle = enemy.GetComponentInParent<MiddleEnemyController>();
        if (middle != null)
        {
            // 3개짜리 정식 함수를 올바르게 호출!
            // (데미지, 근접공격=true, 내 위치)
            middle.ApplyDamage(attackDamage, true, transform.position); 
            
            Debug.Log($"💥 [Middle] {middle.gameObject.name}에게 {attackDamage} 데미지!");
            return;
        }

        // 3) 보스나 기타 몬스터 (여전히 리플렉션이 필요하다면 유지)
        var components = enemy.GetComponentsInParent<MonoBehaviour>();
        foreach (var component in components)
        {
            // 혹시 모르니 'ApplyDamage'를 먼저 찾아봄
            var method = component.GetType().GetMethod("ApplyDamage");
            if (method == null) method = component.GetType().GetMethod("TakeDamage");

            if (method != null)
            {
                // 매개변수 개수를 확인해서 1개면 1개만, 3개면 3개를 넣어주는 똑똑한 로직
                var parameters = method.GetParameters();
                
                if (parameters.Length == 1)
                {
                    method.Invoke(component, new object[] { attackDamage });
                }
                else if (parameters.Length == 3)
                {
                    method.Invoke(component, new object[] { attackDamage, true, transform.position });
                }
                
                Debug.Log($"💥 [Generic] {component.gameObject.name}에게 데미지!");
                return;
            }
        }
    }


}
