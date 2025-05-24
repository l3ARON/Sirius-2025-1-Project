using System.Collections;
using UnityEngine;

public class middleEnemyAttack : MonoBehaviour
{
    public Transform player;

    public float dashRange = 3f;     // A: 돌진 시작 거리
    public float followRange = 1.5f; // B: 따라다니기 시작 거리

    private Rigidbody2D rigid;
    private middleEnemyMove moveScript;

    private bool isDetected = false;
    private bool isDashed = false;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        moveScript = GetComponent<middleEnemyMove>();

        moveScript.enabled = true;
    }

    [SerializeField] private float height = 1f; // 감지 박스 높이 (Y축 범위)

    void Update()
    {
        if (isDashed) return;

        Vector2 delta = player.position - transform.position;
        float absDx = Mathf.Abs(delta.x);
        float absDy = Mathf.Abs(delta.y);

        bool inHeight = absDy <= height * 0.5f;

        if (!inHeight)
        {
            // Y축 범위를 벗어났으면 무시
            if (isDetected)
            {
                Debug.Log("🟥 Y축 범위 초과 → 감지 해제");
                OnPlayerLost();
            }
            return;
        }

        if (absDx > dashRange)
        {
            if (isDetected)
            {
                Debug.Log("🟥 X축 too far → 감지 해제");
                OnPlayerLost();
            }
        }
        else if (absDx > followRange)
        {
            if (!isDetected)
            {
                Debug.Log("🟨 돌진 범위 진입!");
                OnPlayerDetected();
            }
        }
        else
        {
            if (!isDetected || !moveScript.enabled)
            {
                Debug.Log("🟩 따라다니기 범위 진입!");
                FollowPlayer();
            }
        }
    }

    void FollowPlayer()
    {
        isDetected = true;
        isDashed = false;
        moveScript.enabled = true;

        Debug.Log("➡ 따라다니기 (로그만 출력 중)");
    }

    void OnPlayerDetected()
    {
        isDetected = true;
        moveScript.enabled = false;
        rigid.velocity = Vector2.zero;

        Debug.Log("💥 돌진 준비 (로그만 출력 중)");
    }

    void OnPlayerLost()
    {
        isDetected = false;
        isDashed = false;
        moveScript.enabled = true;

        Debug.Log("⏹ 감지 해제 및 상태 초기화");
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Vector3 center = transform.position;

        // dashRange 박스
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, new Vector3(dashRange * 2, height, 0));

        // followRange 박스
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, new Vector3(followRange * 2, height, 0));

        // 중심 점
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(center, 0.05f);
    }
}