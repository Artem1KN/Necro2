using UnityEngine;

public class WallRunHandler : MonoBehaviour
{
    [Header("Wall Checkers")]
    public CollisionDetectorRaycast leftWallDetector;
    public CollisionDetectorRaycast rightWallDetector;

    [Header("Wall Run Settings")]
    public float WallRunSpeed = 7f;
    public float WallJumpForce = 10f;

    private bool isWallRunning = false;
    private Vector3 wallNormal;
            
    private Vector3 wallRunDirection;

    public bool IsWallRunningState => isWallRunning;
    public Vector3 WallNormal => wallNormal;
    public Vector3 WallRunDirection => wallRunDirection;

    private CharacterController characterController;

    void Awake()
    {
        characterController = GetComponentInParent<CharacterController>();
    }

    public void UpdateWallRunLogic(Vector3 lastMoveDirection, ref float verticalVelocity)
        {
        if (characterController == null) return;

        bool hitLeft = leftWallDetector.IsColliding;
        bool hitRight = rightWallDetector.IsColliding;

        if ((hitLeft || hitRight) && !characterController.isGrounded)
        {
            bool wasWallRunning = isWallRunning;
            isWallRunning = true;
            wallNormal = hitLeft ? leftWallDetector.outHit.normal : rightWallDetector.outHit.normal;

            if (!wasWallRunning)
            {
                verticalVelocity = 0;
            }

            Vector3 wallTangent = Vector3.Cross(wallNormal, Vector3.up);
            if (Vector3.Dot(wallTangent, lastMoveDirection) < 0)
                wallRunDirection = -wallTangent;
            else
                wallRunDirection = wallTangent;
        }
        else
        {
            isWallRunning = false;
            wallRunDirection = Vector3.zero;
        }
    }
    
    public void SyncDetectorsToCamera(Transform cameraTransform)
        {
        if (cameraTransform == null) return;

        float targetYRotation = cameraTransform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0, targetYRotation, 0);
        
        if (leftWallDetector != null) leftWallDetector.transform.rotation = targetRotation;
        if (rightWallDetector != null) rightWallDetector.transform.rotation = targetRotation;
    }
}