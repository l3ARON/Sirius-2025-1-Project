using UnityEngine;

public class MiddleEnemyMovement : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    
    [Header("설정")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.5f;
    public float followDistance = 2.0f; // 아군일 때 유지 거리

    [Header("감지")]
    public float detectWidth = 8f;
    public float detectHeight = 3f;
    public LayerMask playerMask;

    [Header("상태")]
    public bool isMovementPaused = false;
    public bool isFriendlyMode = false; // 아군 여부

    Transform player;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim  = GetComponent<Animator>();
    }

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (isMovementPaused || player == null) 
        {
            UpdateAnim(false);
            return;
        }

        if (isFriendlyMode)
        {
            // 아군 로직: 플레이어 따라가기
            FollowPlayerLogic();
        }
        else
        {
            // 적군 로직: 순찰 및 추적
            EnemyLogic();
        }
    }

    void EnemyLogic()
    {
        bool detected = DetectPlayerBox();
        float moveSpeed = detected ? chaseSpeed : patrolSpeed;
        
        float dirX = 0f;
        if (detected)
        {
            dirX = Mathf.Sign(player.position.x - transform.position.x);
        }
        else
        {
            // 순찰 (간단히 현재 속도 방향 유지)
            dirX = (rigid.velocity.x == 0) ? 1f : Mathf.Sign(rigid.velocity.x);
        }

        rigid.velocity = new Vector2(moveSpeed * dirX, rigid.velocity.y);
        Flip(dirX);
        UpdateAnim(true);
    }

    void FollowPlayerLogic()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        // 일정 거리 이상 떨어지면 이동
        if (dist > followDistance)
        {
            float dirX = Mathf.Sign(player.position.x - transform.position.x);
            rigid.velocity = new Vector2(chaseSpeed * dirX, rigid.velocity.y);
            Flip(dirX);
            UpdateAnim(true);
        }
        else
        {
            rigid.velocity = Vector2.zero;
            UpdateAnim(false);
        }
    }

    // Controller에서 호출
    public void SetFriendlyMode()
    {
        isFriendlyMode = true;
        isMovementPaused = false;
    }

    public void PauseMovement()
    {
        isMovementPaused = true;
        rigid.velocity = Vector2.zero;
        UpdateAnim(false);
    }

    public void ResumeMovement()
    {
        isMovementPaused = false;
    }

    void Flip(float dir)
    {
        if (dir == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (dir > 0 ? 1 : -1);
        transform.localScale = scale;
    }

    void UpdateAnim(bool isMoving)
    {
        if (anim == null) return;
        anim.SetBool("isWalk", isMoving);
        
        // 아군이거나 플레이어 감지되면 Run 상태
        bool runCondition = isFriendlyMode ? isMoving : DetectPlayerBox();
        checkmove(isMoving, runCondition);
    }

    bool DetectPlayerBox()
    {
        if (isFriendlyMode) return false; 
        Vector2 center = transform.position;
        Vector2 size = new Vector2(detectWidth, detectHeight);
        return Physics2D.OverlapBox(center, size, 0f, playerMask) != null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(detectWidth, detectHeight, 1));
    }

    void checkmove(bool isMoving, bool runCondition){
        anim.SetBool("isRun", isMoving && runCondition);
        anim.SetBool("isWalk", !(isMoving && runCondition));
    }
}