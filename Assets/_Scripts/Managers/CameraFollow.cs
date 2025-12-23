using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float smoothSpeed = 0.125f;

    // ADDED: Required for Vector3.SmoothDamp
    private Vector3 velocity = Vector3.zero;

    // CHANGED: Switched to LateUpdate for camera movement.
    // This runs *after* all physics (FixedUpdate) and logic (Update)
    // for the frame, ensuring the camera follows the player's final position.
    private void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // CHANGED: Using SmoothDamp instead of Lerp.
        // SmoothDamp is generally better for cameras as it's not
        // frame-rate dependent and provides a critically-damped spring effect.
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothSpeed
        );

        transform.position = smoothedPosition;
    }
}