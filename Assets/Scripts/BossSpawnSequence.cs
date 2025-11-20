using System.Collections;
using System.Collections.Generic;
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

    [Header("현재 살아있는 스폰 오브젝트 관리")]
    // 이 스크립트가 생성한 몬스터들을 추적
    private List<GameObject> spawnedObjects = new List<GameObject>();

    [Header("비상 스폰 (현재 생성된 오브젝트 수가 0일 때)")]
    public bool enableEmergencySpawn = true;      // 켜고 끌 수 있음
    public GameObject emergencyPrefab;            // 비상으로 생성할 프리팹
    public Transform emergencySpawnPoint;         // 비상 생성 위치
    public float emergencyCheckInterval = 2f;     // 몇 초마다 체크할지

    void Start()
    {
        if (steps == null || steps.Length == 0)
        {
            //Debug.LogWarning("[BossSpawnSequence] steps가 비어있습니다!");
            return;
        }

        StartCoroutine(SpawnRoutine());

        // 비상 스폰 기능이 켜져 있고, 설정이 제대로 되어 있을 때만 체크 루프 시작
        if (enableEmergencySpawn && emergencyPrefab != null && emergencySpawnPoint != null)
        {
            StartCoroutine(EmergencySpawnCheck());
        }
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
                    GameObject obj = Instantiate(step.prefab, step.spawnPoint.position, Quaternion.identity);
                    spawnedObjects.Add(obj);
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

    /// <summary>
    /// 현재 살아있는 스폰 오브젝트가 0개일 때, emergencyPrefab을 1개 생성
    /// </summary>
    IEnumerator EmergencySpawnCheck()
    {
        while (true)
        {
            // 일정 시간마다 확인
            yield return new WaitForSeconds(emergencyCheckInterval);

            if (!enableEmergencySpawn)
                continue;

            // 리스트에서 이미 Destroy된 객체는 제거
            spawnedObjects.RemoveAll(obj => obj == null);

            // 현재 살아있는 오브젝트 수가 0이면 비상 스폰
            if (spawnedObjects.Count == 0)
            {
                if (emergencyPrefab != null && emergencySpawnPoint != null)
                {
                    GameObject obj = Instantiate(emergencyPrefab, emergencySpawnPoint.position, Quaternion.identity);
                    spawnedObjects.Add(obj);
                    //Debug.Log("[EmergencySpawn] 모든 스폰 오브젝트가 사라짐 → 비상 몬스터 1마리 생성");
                }
                else
                {
                    //Debug.LogWarning("[EmergencySpawn] emergencyPrefab 또는 emergencySpawnPoint가 비어있음");
                }
            }
        }
    }
}
