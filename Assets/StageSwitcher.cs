using UnityEngine;

public class StageSwitcher : MonoBehaviour
{
    [Header("스테이지 설정")]
    public GameObject currentStage;   // 현재 스테이지 오브젝트
    public GameObject nextStage;      // 다음 스테이지 오브젝트

    [Header("플레이어 태그")]
    public string playerTag = "Player";

    [Header("전환 효과 (선택사항)")]
    public float delay = 0.5f;        // 전환 전 대기 시간

    private bool isSwitching = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isSwitching) return;

        if (collision.CompareTag(playerTag))
        {
            isSwitching = true;
            Debug.Log($"🔁 {currentStage.name} → {nextStage.name} 로 전환");

            StartCoroutine(SwitchStageRoutine());
        }
    }

    private System.Collections.IEnumerator SwitchStageRoutine()
    {
        yield return new WaitForSeconds(delay);

        if (currentStage != null)
            currentStage.SetActive(false);

        if (nextStage != null)
            nextStage.SetActive(true);

        isSwitching = false;
    }
}
