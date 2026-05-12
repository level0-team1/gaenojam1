using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 따라갈 플레이어
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null)
            return;

        // 플레이어 위치 따라가기 (카메라 Z값 유지)
        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        // 부드럽게 이동
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}