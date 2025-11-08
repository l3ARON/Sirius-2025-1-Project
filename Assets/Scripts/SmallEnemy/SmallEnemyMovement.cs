using System.Collections;
using UnityEngine;

/// <summary>
/// 소형 적 기본 이동 전용 스크립트
/// (상하/좌우 이동 + 레이캐스트로 높이 제한)
/// </summary>
public class SmallEnemyMovement : MonoBehaviour
{
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;

    [Header("이동 속도")]
    public float xSpeed = 1f;            // x축 속도
    public float ySpeed = 1f;            // y축 시작 속도

    [Header("높이 제한 (ground 기준)")]
    public float minRayLength = 2.3f;    // 최소 하강 높이
    public float maxRayLength = 4f;      // 최대 상승 높이

    bool isMovementPaused = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(ChangeYSpeedRoutine());
        StartCoroutine(ChangeXSpeedRoutine());
    }

    void FixedUpdate()
    {
        if (isMovementPaused) return;

        // 기본 이동
        spriteRenderer.flipX = xSpeed > 0;
        rigid.velocity = new Vector2(xSpeed, ySpeed);

        LayerMask groundMask = LayerMask.GetMask("flatform");

        // 최대 상승 체크
        Debug.DrawRay(rigid.position, Vector3.down * maxRayLength, Color.red);
        RaycastHit2D rayHitHigh = Physics2D.Raycast(rigid.position, Vector3.down, maxRayLength, groundMask);
        if (rayHitHigh.collider == null || IsSelfOrChild(rayHitHigh.collider.gameObject))
        {
            ySpeed = -1 * Mathf.Abs(ySpeed);
        }

        // 최소 하강 체크
        Debug.DrawRay(rigid.position, Vector3.down * minRayLength, Color.green);
        RaycastHit2D rayHitLow = Physics2D.Raycast(rigid.position, Vector3.down, minRayLength, groundMask);
        if (rayHitLow.collider != null && !IsSelfOrChild(rayHitLow.collider.gameObject))
        {
            ySpeed = Mathf.Abs(ySpeed);
        }
    }

    IEnumerator ChangeYSpeedRoutine()
    {
        while (true)
        {
            if (!isMovementPaused)
                ySpeed *= -1;

            yield return new WaitForSeconds(Random.Range(0.5f, 3.0f));
        }
    }

    IEnumerator ChangeXSpeedRoutine()
    {
        while (true)
        {
            if (!isMovementPaused)
                xSpeed *= -1;

            yield return new WaitForSeconds(5f);
        }
    }

    bool IsSelfOrChild(GameObject obj)
    {
        return obj == gameObject || obj.transform.IsChildOf(transform);
    }

    public void PauseMovement()
    {
        isMovementPaused = true;
        rigid.velocity = Vector2.zero;
    }

    public void ResumeMovement()
    {
        isMovementPaused = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * maxRayLength);
    }
}
