using UnityEngine;

/// <summary>
/// 이동, 점프, 좌우 보기 전환
/// </summary>
public class PlayerMovement2D : MonoBehaviour
{
    public float maxSpeed = 5f;     // 걷기 속도
    public float jumpPower = 8f;    // 점프 힘

    public string groundLayerName = "flatform"; // 착지 체크할 레이어 이름
    public float groundRayLength = 1.5f;        // 바닥 체크 레이 길이

    PlayerRefs refs;

    void Start()
    {
        refs = GetComponent<PlayerRefs>();
    }

    void Update()
    {
        HandleJump();
        HandleHorizontal();
    }

    void FixedUpdate()
    {
        HandleMove();
        HandleLanding();
    }

    // 점프 입력
    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && !refs.anim.GetBool("isJump"))
        {
            // 위로 튀게
            refs.rigid.velocity = new Vector2(refs.rigid.velocity.x, 0f);
            refs.rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            refs.anim.SetBool("isJump", true);
        }
    }

    float hInput = 0f;

    // 좌우 입력 + 좌우 반전 + 공격포인트 뒤집기
    void HandleHorizontal()
    {
        hInput = Input.GetAxisRaw("Horizontal");

        // 감속 (입력 없을 때만)
        if (hInput == 0)
        {
            refs.rigid.velocity = new Vector2(refs.rigid.velocity.x * 0.9f, refs.rigid.velocity.y);
            refs.anim.SetBool("isWalk", false);
            return;
        }

        // 이동 애니메이션
        refs.anim.SetBool("isWalk", true);

        // 방향에 따라 스프라이트 뒤집기
        bool lookRight = hInput > 0;
        refs.spriteRenderer.flipX = lookRight;

        // 공격/발사 포인트도 같이 뒤집기
        if (refs.attackPoint != null)
        {
            float attackX = Mathf.Abs(refs.attackPoint.localPosition.x);
            refs.attackPoint.localPosition = new Vector3(
                lookRight ? attackX : -attackX,
                refs.attackPoint.localPosition.y,
                refs.attackPoint.localPosition.z
            );
        }

        if (refs.firePoint != null)
        {
            float fireX = Mathf.Abs(refs.firePoint.localPosition.x);
            refs.firePoint.localPosition = new Vector3(
                lookRight ? fireX : -fireX,
                refs.firePoint.localPosition.y,
                refs.firePoint.localPosition.z
            );
        }

        // 걷기 사운드 (원하면 조건 더 넣어도 됨)
        if (refs.walkClip != null && !refs.walkClip.isPlaying)
            refs.walkClip.Play();
    }

    // 실제 이동 (물리)
    void HandleMove()
    {
        // 대시 중일 때는 이동 안 하게 하고 싶으면 여기서 체크
        refs.rigid.velocity = new Vector2(hInput * maxSpeed, refs.rigid.velocity.y);

        // 속도 제한
        if (refs.rigid.velocity.x > maxSpeed)
            refs.rigid.velocity = new Vector2(maxSpeed, refs.rigid.velocity.y);
        else if (refs.rigid.velocity.x < -maxSpeed)
            refs.rigid.velocity = new Vector2(-maxSpeed, refs.rigid.velocity.y);
    }

    // 바닥 체크해서 점프 상태 해제
    void HandleLanding()
    {
        // 떨어지는 중일 때만 체크
        if (refs.rigid.velocity.y >= -0.3f) return;

        Vector2 pos = refs.rigid.position;
        LayerMask groundMask = LayerMask.GetMask(groundLayerName);

        RaycastHit2D center = Physics2D.Raycast(pos, Vector2.down, groundRayLength, groundMask);
        RaycastHit2D left = Physics2D.Raycast(pos + Vector2.left * 0.3f, Vector2.down, groundRayLength, groundMask);
        RaycastHit2D right = Physics2D.Raycast(pos + Vector2.right * 0.3f, Vector2.down, groundRayLength, groundMask);

        if ((center.collider != null && center.normal.y > 0.7f) ||
            (left.collider != null && left.normal.y > 0.7f) ||
            (right.collider != null && right.normal.y > 0.7f))
        {
            refs.anim.SetBool("isJump", false);
        }
    }
}
