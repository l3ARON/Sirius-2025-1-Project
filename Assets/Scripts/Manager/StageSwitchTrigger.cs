using UnityEngine;

public class StageSwitchTrigger : MonoBehaviour
{
    public StageSequenceManager manager;
    public string playerTag = "Player";
    public bool oneTimeUse = true;

    private bool used = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"무언가 닿았습니다! 닿은 물체 이름: {other.name}, 태그: {other.tag}");
        if (used && oneTimeUse) return;
        if (!other.CompareTag(playerTag)) return;

        if (manager != null)
        {
            Debug.Log("변환 대기중");
            manager.SwitchToNextStage();
            used = true;
        }
        else
        {
            Debug.LogWarning("[StageSwitchTrigger] manager가 비어있음");
        }
    }
}
