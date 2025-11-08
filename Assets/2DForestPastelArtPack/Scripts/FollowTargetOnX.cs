using System;
using UnityEngine;


public class FollowTargetOnX : MonoBehaviour
{
    public Transform target;

    private void LateUpdate()
    {
        transform.position = new Vector3(target.position.x,transform.position.y, transform.position.z);
    }
}