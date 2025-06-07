using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public Camera mainCamera;
    Vector3 cameraPos;
    [SerializeField][Range(0.01f, 0.1f)] float shakeRange = 0.05f;

    public void Shake(float duration, float inputRange)
    {
        shakeRange = inputRange;
        cameraPos = mainCamera.transform.localPosition;
        InvokeRepeating("StartShake", 0f, 0.005f);
        Invoke("StopShake", duration);
    }

    void StartShake()
    {
        //Debug.Log("흔들기 작동");
        float cameraPosX = Random.value * shakeRange * 2 - shakeRange;
        float cameraPosY = Random.value * shakeRange * 2 - shakeRange;
        Vector3 StartcameraPos = mainCamera.transform.localPosition;
        StartcameraPos.x += cameraPosX;
        StartcameraPos.y += cameraPosY;
        mainCamera.transform.localPosition = StartcameraPos;
    }

    void StopShake()
    {
        CancelInvoke("StartShake");
        mainCamera.transform.localPosition = cameraPos;
    }

}
