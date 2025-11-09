using UnityEngine;

public class StageSequenceManager : MonoBehaviour
{
    [Header("스테이지들 (순서대로 넣기)")]
    public GameObject[] stages;   // Stage1, Stage2, Stage3 ...

    [Header("각 스테이지별 플레이어 스폰 위치")]
    public Transform[] spawnPoints;   // Stage1, 2, 3 시작 위치

    [Header("플레이어")]
    public Transform player;

    [Header("전환 효과")]
    public float delay = 0.0f;       // 트리거 후 살짝 대기
    public float fadeInTime = 0.5f;  // 화면 어두워지는 시간 (0 → 1)
    public float fadeOutTime = 0.5f; // 화면 밝아지는 시간 (1 → 0)
    public float holdTime = 0.2f;    // 완전 어두운 상태 유지 시간
    public bool loop = false;        // 마지막에서 다시 첫 스테이지로 갈지 여부

    [Header("카메라 가림용 오브젝트 (전체 화면 스프라이트)")]
    public SpriteRenderer fadeCover;

    private int currentIndex = 0;
    private bool isSwitching = false;

    private void Start()
    {
        // 시작할 때 현재 인덱스 스테이지만 활성화
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i] != null)
                stages[i].SetActive(i == currentIndex);
        }

        // 페이드 오브젝트는 항상 투명으로 시작
        if (fadeCover != null)
        {
            Color c = fadeCover.color;
            c.a = 0f;
            fadeCover.color = c;
        }
    }

    /// <summary>
    /// 트리거에서 호출하는 함수 (다음 스테이지로 전환)
    /// </summary>
    public void SwitchToNextStage()
    {
        if (isSwitching) return;

        if (stages == null || stages.Length == 0)
        {
            Debug.LogWarning("[StageSequenceManager] stages가 비어있음");
            return;
        }

        int nextIndex = currentIndex + 1;

        if (nextIndex >= stages.Length)
        {
            if (loop)
            {
                nextIndex = 0; // 루프 모드면 다시 처음으로
            }
            else
            {
                Debug.Log("[StageSequenceManager] 마지막 스테이지, 더 이상 전환 없음");
                return;
            }
        }

        StartCoroutine(SwitchStageRoutine(nextIndex));
    }

    private System.Collections.IEnumerator SwitchStageRoutine(int nextIndex)
    {
        isSwitching = true;

        // 0) 필요하면 약간 대기
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        // 1) 화면 어두워지기 (0 → 1)
        if (fadeCover != null)
            yield return FadeAlpha(0f, 1f, fadeInTime);

        // 2) 완전히 가려진 상태 잠깐 유지
        if (holdTime > 0f)
            yield return new WaitForSeconds(holdTime);

        // 3) 스테이지 전환
        GameObject cur = stages[currentIndex];
        GameObject next = stages[nextIndex];

        if (cur != null)
            cur.SetActive(false);

        if (next != null)
            next.SetActive(true);

        currentIndex = nextIndex;

        // 4) 플레이어 위치를 다음 스테이지 스폰 포인트로 이동
        if (player != null &&
            spawnPoints != null &&
            nextIndex < spawnPoints.Length &&
            spawnPoints[nextIndex] != null)
        {
            player.position = spawnPoints[nextIndex].position;
        }

        // 5) 화면 다시 밝아지기 (1 → 0)
        if (fadeCover != null)
            yield return FadeAlpha(1f, 0f, fadeOutTime);

        isSwitching = false;
    }

    private System.Collections.IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (fadeCover == null || duration <= 0f)
            yield break;

        float t = 0f;
        Color c = fadeCover.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);
            c.a = alpha;
            fadeCover.color = c;
            yield return null;
        }

        c.a = to;
        fadeCover.color = c;
    }
}
