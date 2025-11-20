using System.Collections;
using UnityEngine;

/// <summary>
/// 대시 + 쿨타임 (isJump로 애니 공유) + 잔상 효과
/// </summary>
public class PlayerDash2D : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 5f;

    [Header("Dash Ghost")]
    public GameObject ghostPrefab;          // 👈 여기 프리팹 넣어주기
    public float ghostSpawnInterval = 0.05f;// 잔상 생성 간격 (작을수록 촘촘)

    bool isDashing = false;
    bool isCooldown = false;
    float dashTimer = 0f;

    PlayerRefs refs;
    PlayerMovement2D move;   // ← 이동 스크립트 참조

    void Start()
    {
        refs = GetComponent<PlayerRefs>();
        move = GetComponent<PlayerMovement2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && !isCooldown)
        {
            StartDash();
        }
    }

    void FixedUpdate()
    {
        if (!isDashing) return;

        // 방향 구하기 (SpriteRenderer 없을 때도 대비)
        float dir;
        if (refs.spriteRenderer != null)
            dir = refs.spriteRenderer.flipX ? 1f : -1f;
        else
            dir = transform.localScale.x >= 0 ? 1f : -1f;

        // 대시 속도 적용
        refs.rigid.velocity = new Vector2(dir * dashSpeed, 0f);

        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0f)
        {
            EndDash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;

        // 이동 스크립트 잠깐 끄기 → 덮어쓰기 방지
        if (move != null) move.enabled = false;

        // 소리
        if (refs.dashClip != null && refs.dashClip.enabled && refs.dashClip.gameObject.activeInHierarchy)
            refs.dashClip.Play();

        // 애니 (점프랑 공유)
        if (refs.anim != null)
            refs.anim.SetBool("isJump", true);

        // 🔥 대쉬 시작할 때 잔상 코루틴 시작
        if (ghostPrefab != null)
            StartCoroutine(DashGhostRoutine());

        StartCoroutine(DashCooldown());
    }

    void EndDash()
    {
        isDashing = false;

        // 이동 다시 켜기
        if (move != null) move.enabled = true;

        // ❗ 여기 로직 수정
        // 공중에 있을 때는 isJump를 false로 만들지 않고,
        // "발이 땅에 닿았을 때(HandleLanding에서)"만 isJump를 false로 바꾸게 함.
        if (refs.anim != null)
        {
            bool isAirborne = Mathf.Abs(refs.rigid.velocity.y) > 0.1f;

            // 땅에 거의 붙어있는 상태(수직속도 0 근처)에서만 점프 상태 해제
            if (!isAirborne)
            {
                refs.anim.SetBool("isJump", false);
            }
            // 공중이면 그대로 유지 → HandleLanding()에서만 false로 바뀜
        }
    }


    IEnumerator DashCooldown()
    {
        isCooldown = true;
        int remain = Mathf.CeilToInt(dashCooldown);
        while (remain > 0)
        {
           // Debug.Log($"⏳ Dash Cooldown: {remain}");
            yield return new WaitForSeconds(1f);
            remain--;
        }
        isCooldown = false;
       // Debug.Log("✅ Dash Ready!");
    }

    // ========================= 잔상 관련 코드 =========================

    IEnumerator DashGhostRoutine()
    {
        // isDashing이 true인 동안, 일정 간격으로 잔상 생성
        while (isDashing)
        {
            SpawnGhost();
            yield return new WaitForSeconds(ghostSpawnInterval);
        }
    }

    void SpawnGhost()
    {
        if (ghostPrefab == null || refs.spriteRenderer == null)
            return;

        // 플레이어 현재 위치에 잔상 생성
        GameObject g = Instantiate(
            ghostPrefab,
            refs.spriteRenderer.transform.position,
            Quaternion.identity
        );

        SpriteRenderer ghostSr = g.GetComponent<SpriteRenderer>();
        if (ghostSr != null)
        {
            // 현재 스프라이트 복사
            ghostSr.sprite = refs.spriteRenderer.sprite;

            // 좌우 반전 상태도 복사
            ghostSr.flipX = refs.spriteRenderer.flipX;

            // 정렬 레이어 / 순서 맞추기 (살짝 뒤에)
            ghostSr.sortingLayerID = refs.spriteRenderer.sortingLayerID;
            ghostSr.sortingOrder = refs.spriteRenderer.sortingOrder - 1;
        }
    }
}
