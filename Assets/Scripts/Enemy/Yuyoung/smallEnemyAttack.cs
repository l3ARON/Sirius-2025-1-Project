using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
범위 내에 플레이어가 감지되면 플레이어를 향해서 일직선으로 날아감
*/
public class smallEnemyAttack : MonoBehaviour
{
    public Transform player;        // 플레이어 트랜스폼 참조
    public float detectionRadius = 3f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

     void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRadius)
        {
            Debug.Log("Player detected within 3f radius!");
        }
    }

    // Optional: Scene 뷰에서 감지 범위를 시각적으로 확인
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
