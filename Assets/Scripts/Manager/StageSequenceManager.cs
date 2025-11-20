using UnityEngine;

public class StageSequenceManager : MonoBehaviour
{
    [Header("스테이지들 (순서대로 넣기)")]
    public GameObject[] stages;

    [Header("각 스테이지별 플레이어 스폰 위치")]
    public Transform[] spawnPoints;

    [Header("플레이어")]
    public Transform player;

    [Header("전환 효과")]
    public float delay = 0.0f;
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 0.5f;
    public float holdTime = 0.2f;
    public bool loop = false;

    [Header("카메라 가림용 오브젝트")]
    public SpriteRenderer fadeCover;

    private int currentIndex = 0;
    private bool isSwitching = false;

    // ================================
    // ★ 숫자키 3회 입력 감지 변수들
    // ================================
    private float keyResetTime = 0.7f;   // 0.7초 안에 3번 누르면 성공
    private int[] keyPressCount = new int[10];
    private float[] lastPressTime = new float[10];


    private void Start()
    {
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i] != null)
                stages[i].SetActive(i == currentIndex);
        }

        if (fadeCover != null)
        {
            Color c = fadeCover.color;
            c.a = 0f;
            fadeCover.color = c;
        }
    }


    private void Update()
    {
        DetectQuickNumberKey();
    }

    // ================================================
    // ★ 숫자키 3번 입력하여 해당 스테이지로 이동
    // ================================================
    void DetectQuickNumberKey()
    {
        for (int i = 0; i <= 9; i++)
        {
            KeyCode code = KeyCode.Alpha0 + i;

            if (Input.GetKeyDown(code))
            {
                float now = Time.time;

                // 시간이 너무 지나면 카운트 초기화
                if (now - lastPressTime[i] > keyResetTime)
                    keyPressCount[i] = 0;

                lastPressTime[i] = now;
                keyPressCount[i]++;

                Debug.Log($"Key {i} pressed {keyPressCount[i]} times");

                // 3번 눌림 → 해당 스테이지로 점프
                if (keyPressCount[i] >= 3)
                {
                    keyPressCount[i] = 0;

                    // 스테이지 범위 안이면 이동
                    if (i < stages.Length)
                    {
                        Debug.Log($"Quick jump to stage {i}");
                        JumpToStage(i);
                    }
                }
            }
        }
    }

    // ================================================
    // ★ 특정 스테이지로 즉시 전환하는 기능
    // ================================================
    public void JumpToStage(int targetIndex)
    {
        if (isSwitching) return;
        StartCoroutine(SwitchStageRoutine(targetIndex));
    }

    // ================================================
    // 기존 스테이지 전환 루틴
    // ================================================
    public void SwitchToNextStage()
    {
        if (isSwitching) return;

        int nextIndex = currentIndex + 1;

        if (nextIndex >= stages.Length)
        {
            if (loop) nextIndex = 0;
            else return;
        }

        StartCoroutine(SwitchStageRoutine(nextIndex));
    }


    private System.Collections.IEnumerator SwitchStageRoutine(int nextIndex)
    {
        isSwitching = true;

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (fadeCover != null)
            yield return FadeAlpha(0f, 1f, fadeInTime);

        if (holdTime > 0f)
            yield return new WaitForSeconds(holdTime);

        if (stages[currentIndex] != null)
            stages[currentIndex].SetActive(false);

        if (stages[nextIndex] != null)
            stages[nextIndex].SetActive(true);

        currentIndex = nextIndex;

        if (player != null &&
            spawnPoints != null &&
            nextIndex < spawnPoints.Length &&
            spawnPoints[nextIndex] != null)
        {
            player.position = spawnPoints[nextIndex].position;
        }

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
            c.a = Mathf.Lerp(from, to, t / duration);
            fadeCover.color = c;
            yield return null;
        }

        c.a = to;
        fadeCover.color = c;
    }
}
