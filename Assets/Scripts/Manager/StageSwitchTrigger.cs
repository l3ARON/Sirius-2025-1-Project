using UnityEngine;

public class StageSwitchTrigger : MonoBehaviour
{
    public StageSequenceManager manager;
    public string playerTag = "Player";
    public bool oneTimeUse = true;

    private bool used = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used && oneTimeUse) return;
        if (!other.CompareTag(playerTag)) return;

        if (manager != null)
        {
            manager.SwitchToNextStage();
            used = true;
        }
        else
        {
            Debug.LogWarning("[StageSwitchTrigger] manager가 비어있음");
        }
    }
}
