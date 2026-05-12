using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 따라갈 플레이어

    void LateUpdate()
    {
        if (target == null)
            return;

        // 플레이어 위치 따라가기 (카메라 Z값 유지)
        Vector3 targetPosition = target.position;
        targetPosition.z = -10;

        transform.position = targetPosition;

    }
}