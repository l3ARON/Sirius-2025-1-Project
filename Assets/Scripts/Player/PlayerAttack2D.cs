using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack2D : MonoBehaviour
{
    [Header("Melee")]
    public float attackDelay = 0.5f;    
    public float attackRange = 1f;      
    public int attackDamage = 1;        
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();

    [Header("Ranged")]
    public GameObject longRangeAttackPrefab;
    public float rangedAttackDelay = 0.5f;
    private float rangedAttackTimer = 0f;
    private bool isNextAttackRanged = false;

    [Header("FX")]
    public GameObject slashPrefab;
    public float slashOffset = 0.5f;

    [Header("UI Weapon")]
    public GameObject weapon2;   

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

    // 외부에서 호출(소형 몬스터 처치 시)
    public void EnableNextRangedAttack()
    {
        isNextAttackRanged = true;

        if (weapon2 != null)
            weapon2.SetActive(true);
    }

    // 공격 입력 처리
    void HandleAttackInput()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
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

    // 근거리 공격 시작
    void DoMeleeAttack()
    {
        isAttacking = true;
        attackTimer = attackDelay;
        damagedEnemies.Clear();

        if (refs.anim != null)
            refs.anim.SetBool("isAttack", true);

        if (refs.attackClip != null)
            refs.attackClip.Play();

        // 슬래시 이펙트
        if (slashPrefab != null)
        {
            float dir = refs.spriteRenderer != null && refs.spriteRenderer.flipX ? 1f : -1f;

            Vector3 basePos = refs.attackPoint != null
                ? refs.attackPoint.position
                : transform.position;

            Vector3 spawnPos = basePos + Vector3.right * (-dir) * slashOffset;
            float yRot = refs.spriteRenderer != null && refs.spriteRenderer.flipX ? 0f : 180f;
            Quaternion rot = Quaternion.Euler(0, yRot, 0);

            GameObject slash = Instantiate(slashPrefab, spawnPos, rot);
            Destroy(slash, 0.4f);
        }
    }

    // 원거리 공격
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

        // weapon2 비활성화
        if (weapon2 != null)
            weapon2.SetActive(false);

        Debug.Log("🎯 원거리 공격 발사");
    }

    // 공격 애니메이션 해제 처리
    void HandleMeleeProgress()
    {
        if (!isAttacking) return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0)
        {
            isAttacking = false;
            if (refs.anim != null)
                refs.anim.SetBool("isAttack", false);
        }
    }

    // 실제 적에게 데미지를 넣는 함수
    public void DealDamageToEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        Debug.Log("[PlayerAttack2D] 근접 타격, target = " + enemy.name);

        // 1) 소형 몬스터 전용
        SmallEnemyController small = enemy.GetComponentInParent<SmallEnemyController>();
        if (small != null)
        {
            small.TakeDamage(attackDamage, true);   
            Debug.Log($"💥 (Small) {small.gameObject.name}에게 {attackDamage} 데미지!");
            return;
        }

        // 2) 중형 몬스터 전용
        MiddleEnemyController mid = enemy.GetComponentInParent<MiddleEnemyController>();
        if (mid != null)
        {
            mid.TakeDamage(attackDamage, true);  // melee = true
            Debug.Log($"💥 (Middle) {mid.gameObject.name}에게 {attackDamage} 데미지!");
            return;
        }

        // 3) 일반 몬스터(리플렉션)
        var components = enemy.GetComponentsInParent<MonoBehaviour>();
        bool found = false;

        foreach (var component in components)
        {
            var method = component.GetType().GetMethod("TakeDamage");
            if (method != null)
            {
                method.Invoke(component, new object[] { attackDamage });
                Debug.Log($"💥 (Generic) {component.gameObject.name}에게 {attackDamage} 데미지!");
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"[DealDamageToEnemy] {enemy.name}에 TakeDamage 없음");
        }
    }
}
