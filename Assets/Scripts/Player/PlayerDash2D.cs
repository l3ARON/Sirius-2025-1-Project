using System.Collections;
using UnityEngine;

/// <summary>
/// 대시 + 쿨타임 (isJump로 애니 공유)
/// </summary>
public class PlayerDash2D : MonoBehaviour
{
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 5f;

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

        StartCoroutine(DashCooldown());
    }

    void EndDash()
    {
        isDashing = false;

        // 이동 다시 켜기
        if (move != null) move.enabled = true;

        if (refs.anim != null)
            refs.anim.SetBool("isJump", false);
    }

    IEnumerator DashCooldown()
    {
        isCooldown = true;
        int remain = Mathf.CeilToInt(dashCooldown);
        while (remain > 0)
        {
            Debug.Log($"⏳ Dash Cooldown: {remain}");
            yield return new WaitForSeconds(1f);
            remain--;
        }
        isCooldown = false;
        Debug.Log("✅ Dash Ready!");
    }
}
