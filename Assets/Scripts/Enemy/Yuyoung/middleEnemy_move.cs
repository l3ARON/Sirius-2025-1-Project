using System.Collections;
using UnityEngine;

public class middleEnemyMove : MonoBehaviour
{
    // 컴포넌트 참조 변수들
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    // 이동 속도
    public float xSpeed = 1f;
    // 따라갈때 이동속도도
    public float xSpeedToFollow = 3f; 

    // 감지할 대상(플레이어 또는 다른 태그 대상)
    public Transform player;
    public string targetTag = "Player"; // 감지 대상의 태그 지정
    public float detectRangeX = 10f;    // 일반 감지 범위 (가로)
    public float detectRangeY = 1f;     // 일반 감지 범위 (세로)

    // 좁은 근접 감지 범위
    public float closeRangeX = 2f;      // 근접 감지 범위 (가로)
    public float closeRangeY = 1f;      // 근접 감지 범위 (세로)

    // 시작 시 컴포넌트 할당
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // 물리 연산 프레임마다 호출
    void FixedUpdate()
    {
        // 현재 이동 여부에 따라 걷기 애니메이션 설정
        anim.SetBool("isWalk", xSpeed != 0);

        // 감지 범위 안에 타겟이 있을 경우 → 해당 방향으로 이동
        if (IsTargetInRange() && player != null)
        {
            Debug.Log("감지!!");
            float direction = Mathf.Sign(player.position.x - transform.position.x); // 방향 결정 (+/-1)
            xSpeed = Mathf.Abs(xSpeedToFollow) * direction; // 방향에 따라 속도 부호 결정
        }

        // 좁은 범위 안에 플레이어가 있을 경우 로그 출력
        if (IsPlayerVeryClose())
        {
            Debug.Log("플레이어가 가까이 있음!");
            xSpeed = 0;
            anim.SetBool("isAttack", true);
        }else
            anim.SetBool("isAttack", false);

        // 실제 이동 처리
            rigid.velocity = new Vector2(xSpeed, rigid.velocity.y);

        // 이동 방향에 따라 스프라이트 반전
        FlipSprite();

        // 앞 위치 계산 (바닥 및 벽 감지를 위해)
        Vector2 frontVec = new Vector2(rigid.position.x + (xSpeed * 1f), rigid.position.y);
        Vector2 checkDir = (xSpeed > 0) ? Vector2.right : Vector2.left;

        // 낭떠러지, 벽 등 조건 충족 시 방향 반전
        if (!IsGroundAhead(frontVec) || IsWallAhead(frontVec, checkDir))
        {
            xSpeed *= -1;
        }
    }

    // 지정한 태그를 가진 대상이 감지 범위 내에 있는지 확인
    bool IsTargetInRange()
    {
        Vector2 center = transform.position;
        Vector2 size = new Vector2(detectRangeX * 2, detectRangeY * 2);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(targetTag))
            {
                player = hit.transform; // 감지된 대상 설정
                return true;
            }
        }

        player = null;
        return false;
    }

    // 좁은 범위 안에 플레이어가 있는지 확인
    bool IsPlayerVeryClose()
    {
        Vector2 center = transform.position;
        Vector2 size = new Vector2(closeRangeX * 2, closeRangeY * 2);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    // 이동 방향에 따라 스프라이트 반전
    void FlipSprite()
    {
        if (xSpeed != 0)
            spriteRenderer.flipX = xSpeed > 0;
    }

    // 앞에 땅이 있는지(낭떠러지 여부) 감지
    bool IsGroundAhead(Vector2 origin)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 2f);
        Debug.DrawRay(origin, Vector2.down * 2f, Color.green);
        return hit.collider != null;
    }

    // 앞에 벽이 있는지 감지
    bool IsWallAhead(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 1f);
        Debug.DrawRay(origin, direction * 1f, Color.red);

        if (hit.collider != null && hit.collider.gameObject != this.gameObject)
        {
            Debug.Log("앞이 막힘! → " + hit.collider.gameObject.name);
            return true;
        }

        return false;
    }

    // 감지 범위 시각화 (Scene 뷰에서 확인용)
    void OnDrawGizmosSelected()
    {
        // 일반 감지 범위
        Gizmos.color = Color.cyan;
        Vector2 center = transform.position;
        Vector2 size = new Vector2(detectRangeX * 2, detectRangeY * 2);
        Gizmos.DrawWireCube(center, size);

        // 좁은 감지 범위
        Gizmos.color = Color.magenta;
        Vector2 closeSize = new Vector2(closeRangeX * 2, closeRangeY * 2);
        Gizmos.DrawWireCube(center, closeSize);
    }
}
