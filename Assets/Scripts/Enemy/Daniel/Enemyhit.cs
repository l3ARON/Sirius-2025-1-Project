using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemyhit : MonoBehaviour
{
    void Awake()
    {
        // Renderer renderer = GetComponent<Renderer>();
        // if (renderer != null)
        // {
        //     renderer.material.color = Color.red;
        // }

        // 2초 뒤 자동 파괴
        Destroy(gameObject, 0.5f);
    }
}
