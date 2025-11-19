using System.Collections;
using UnityEngine;

public class MiddleEnemyAttack : MonoBehaviour
{
    [Header("2차 공격 범위")]
    public float closeRange = 2f;

    [Header("공격 설정")]
    public float windUpTime = 0.4f;   // 공격 준비 시간
    public float dashSpeed = 6f;
    public float dashDuration = 0.6f;
    public float cooldown = 1.5f;

    [Header("데미지")]
    public int damage = 1;

    Transform player;
    Rigidbody2D rigid;
    MiddleEnemyMovement move;
    MiddleEnemyController controller;
    Animator anim;

    public bool isAttacking = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        move = GetComponent<MiddleEnemyMovement>();
        controller = GetComponent<MiddleEnemyController>();
        anim = GetComponent<Animator>();
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

        if (dist <= closeRange)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        move.PauseMovement();

        anim.SetBool("isAttack", true);

        yield return new WaitForSeconds(windUpTime);

        // 공격 돌진
        Vector2 dir = (player.position - transform.position).normalized;
        rigid.velocity = dir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        // 공격 종료
        rigid.velocity = Vector2.zero;
        anim.SetBool("isAttack", false);

        isAttacking = false;
        move.ResumeMovement();

        yield return new WaitForSeconds(cooldown);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isAttacking) return;

        if (collision.collider.CompareTag("Player"))
        {
            PlayerDamage2D pd = collision.collider.GetComponent<PlayerDamage2D>();
            if (pd != null)
            {
                pd.OnDamaged(transform.position);
            }
        }
    }
}
