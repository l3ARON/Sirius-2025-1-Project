using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    public float smoothSpeed = 0.125f;

    public Vector3 offset;

    void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        Vector3 desiredPosition = player.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}