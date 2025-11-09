using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class BossController : MonoBehaviour
{
    [Header("보스 기본 설정")]
    public int maxHealth = 100;
    public Transform target;

    [Header("탐지 / 패턴 거리")]
    public float detectRange = 12f;  // 보스가 깨어나는 거리
    public float slamRange = 5f;     // 점프 찍기 패턴 거리
    public float breathRange = 9f;   // 브레스 패턴 거리

    [Header("보스 근접 판정 (찍기 데미지)")]
    public BoxCollider2D meleeArea;  // isTrigger = true

    [Header("카메라 효과")]
    public CameraShake cameraShake;

    [Header("사운드")]
    public AudioSource growlClip;    // 으르렁

    [Header("패턴 스크립트")]
    public BossPatterns patterns;

    // 내부 상태
    int curHealth;
    bool isDead = false;
    bool aiStarted = false;

    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;

    Coroutine aiRoutine;
    Coroutine spawnRoutine;

    // 패턴에서 쓰기 위한 프로퍼티
    public bool IsDead => isDead;
    public Rigidbody2D Rigid => rigid;
    public Animator Anim => anim;
    public SpriteRenderer Sprite => spriteRenderer;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        curHealth = maxHealth;
    }

    void FixedUpdate()
    {
        if (isDead || target == null) return;

        // 보스는 제자리지만 플레이어 방향만 바라봄
        float dirX = target.position.x - transform.position.x;
        transform.rotation = Quaternion.Euler(0, dirX < 0 ? 0 : 180, 0);

        // 감지 범위 들어오면 AI 시작
        if (!aiStarted)
        {
            float distance = Vector2.Distance(transform.position, target.position);
            if (distance <= detectRange)
            {
                aiStarted = true;

                if (patterns != null)
                    spawnRoutine = StartCoroutine(patterns.SpawnLoop(this)); // 패턴 1

                aiRoutine = StartCoroutine(AIRoutine());                     // 패턴 2,3
            }
        }
    }

    IEnumerator AIRoutine()
    {
        while (!isDead)
        {
            if (Random.value < 0.15f && growlClip != null)
                growlClip.Play();

            if (target == null)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            float dist = Vector2.Distance(transform.position, target.position);

            // 2. 가까우면 점프 찍기 우선
            if (dist <= slamRange && patterns != null && !patterns.IsActing)
            {
                yield return StartCoroutine(patterns.JumpSlam(this));
            }
            // 3. 그 다음 범위면 브레스
            else if (dist <= breathRange && patterns != null && !patterns.IsActing)
            {
                yield return StartCoroutine(patterns.Breath(this));
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    /// <summary>
    /// - Melee 태그: 보스가 피해 받음  
    /// - Player 태그: meleeArea가 켜져 있을 때 플레이어가 피해 받음
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // 1) 플레이어 무기에게 맞았을 때 → 보스 피해
        if (other.CompareTag("Melee"))
        {
            Weapon weapon = other.GetComponent<Weapon>();
            if (weapon == null) return;

            Vector2 reactVec = (Vector2)(transform.position - other.transform.position);
            StartCoroutine(DamageRoutine(weapon.damage, reactVec));
        }
        // 2) 플레이어가 보스 근접 공격 범위에 들어왔을 때 → 플레이어 피해
        else if (other.CompareTag("Player"))
        {
            // 점프 찍는 타이밍에만 데미지
            if (meleeArea != null && meleeArea.enabled)
            {
                var pd = other.GetComponent<PlayerDamage2D>();
                if (pd != null)
                {
                    // hitPos = 보스 위치 (공격이 날아오는 위치)
                    pd.OnDamaged(transform.position);
                }
            }
        }
    }

    IEnumerator DamageRoutine(int dmg, Vector2 reactVec)
    {
        curHealth -= dmg;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;

        if (curHealth <= 0)
        {
            isDead = true;

            if (aiRoutine != null) StopCoroutine(aiRoutine);
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);

            spriteRenderer.color = Color.gray;
            gameObject.layer = 13;     // 더 이상 피격 안 되게
            anim.SetTrigger("doDdie");

            reactVec = reactVec.normalized + Vector2.up;
            rigid.AddForce(reactVec * 2, ForceMode2D.Impulse);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slamRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, breathRange);
    }
}
