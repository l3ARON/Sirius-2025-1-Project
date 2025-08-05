using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiddleEnemyHP : MonoBehaviour
{
    [SerializeField] int maxHP = 200;
    private int currentHP;
    public GameObject hitPrefab;
    public GameObject diePrefab;

    void Awake()
    {
        currentHP = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {      
        currentHP -= damage;
        Debug.Log("강아지 몬스터 피해! 현재 HP: " + currentHP);
        Instantiate(hitPrefab, transform.position, transform.rotation);
        if (currentHP <= 0)
        {
            Debug.Log("강아지 몬스터 사망");
            Die(); // 사망 처리 함수로 분리
        }
    }

    void Die()
    {
        Instantiate(diePrefab, transform.position, transform.rotation);
    }    
}
