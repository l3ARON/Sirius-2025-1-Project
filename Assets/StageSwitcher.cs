using UnityEngine;

public class StageSwitcher : MonoBehaviour
{
    [Header("스테이지 설정")]
    public GameObject currentStage;
    public GameObject nextStage;

    [Header("전환 효과")]
    public float delay = 0.5f;
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 0.5f;
    public float holdTime = 0.2f;

    [Header("카메라 가림용 오브젝트")]
    public SpriteRenderer fadeCover;

    private bool isSwitching = false;

    public void StartSwitch()
    {
        if (isSwitching) return;
        isSwitching = true;
        StartCoroutine(SwitchStageRoutine());
    }

    private System.Collections.IEnumerator SwitchStageRoutine()
    {
        Debug.Log("[StageSwitcher] 🕒 전환 루틴 시작");

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        // 🔥 0 → 255
        if (fadeCover != null)
            yield return FadeAlpha(0f, 1f, fadeInTime);

        if (holdTime > 0f)
            yield return new WaitForSeconds(holdTime);

        // 🔁 스테이지 전환
        if (currentStage != null)
        {
            Debug.Log($"[StageSwitcher] ❌ 비활성화: {currentStage.name}");
            currentStage.SetActive(false);
        }

        if (nextStage != null)
        {
            Debug.Log($"[StageSwitcher] ✅ 활성화: {nextStage.name}");
            nextStage.SetActive(true);
        }

        // 🔥 255 → 0
        if (fadeCover != null)
            yield return FadeAlpha(1f, 0f, fadeOutTime);

        Debug.Log("[StageSwitcher] 🌟 전환 완료");
        isSwitching = false;
    }

    private System.Collections.IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (fadeCover == null || duration <= 0f)
            yield break;

        float t = 0f;
        Color c = fadeCover.color;

        Debug.Log($"[StageSwitcher] ▶ FadeAlpha 시작 (from={from * 255:F0}, to={to * 255:F0})");

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);
            c.a = alpha;
            fadeCover.color = c;
            Debug.Log($"[StageSwitcher] α={(int)(alpha * 255)}");
            yield return null;
        }

        c.a = to;
        fadeCover.color = c;
        Debug.Log($"[StageSwitcher] 🔚 FadeAlpha 완료 (최종 α={(int)(to * 255)})");
    }
}
