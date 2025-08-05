using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Ground Check (Raycast)")]
    public float rayLength = 1.5f;
    public LayerMask groundMask;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 이동
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // 방향 전환
        if (moveInput != 0)
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);

        // 점프
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    // 점프 가능 상태인지 확인 (낙하 중 아님 + 바닥에 있음)
    bool IsGrounded()
    {
        Vector2 pos = rb.position;
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, rayLength, groundMask);
        return hit.collider != null && hit.normal.y > 0.7f;
    }
}
