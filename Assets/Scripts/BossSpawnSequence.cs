using System.Collections;
using UnityEngine;

[System.Serializable]
public class SpawnStep
{
    public string name;            // 로그용 이름(예: Small1)
    public GameObject prefab;      // 생성할 프리팹
    public Transform spawnPoint;   // 생성할 위치 (하나만)
    public float delayAfter = 1f;  // 생성 후 다음 단계까지 대기할 시간
}

public class BossSpawnSequence : MonoBehaviour
{
    [Header("스폰 시퀀스 설정 (순서대로 실행됨)")]
    public SpawnStep[] steps;

    [Header("루프 여부")]
    public bool loop = true;

    void Start()
    {
        if (steps == null || steps.Length == 0)
        {
            //Debug.LogWarning("[BossSpawnSequence] steps가 비어있습니다!");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            for (int i = 0; i < steps.Length; i++)
            {
                SpawnStep step = steps[i];

                // 생성
                if (step.prefab != null && step.spawnPoint != null)
                {
                    Instantiate(step.prefab, step.spawnPoint.position, Quaternion.identity);
                    //Debug.Log($"[Spawn] {step.name} 생성 (delayAfter={step.delayAfter})");
                }
                else
                {
                    //Debug.LogWarning("[SpawnStep] prefab 또는 spawnPoint가 비어있음");
                }

                // 다음 스텝 전까지 대기
                yield return new WaitForSeconds(step.delayAfter);
            }

            if (!loop)
                break; // 루프 끄면 종료
        }
    }
}
