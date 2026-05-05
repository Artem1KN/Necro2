using UnityEngine;

public class WallRunHandler : MonoBehaviour
{
    [Header("Wall Checkers")]
    public CollisionDetectorRaycast leftWallDetector;
    public CollisionDetectorRaycast rightWallDetector;

    [Header("Wall Run Settings")]
    public float WallRunSpeed = 7f;
    public float WallJumpForce = 10f;

    private bool _isWallRunning = false;
    private Vector3 _wallNormal;
            
    private Vector3 _wallRunDirection;

    // Properties to allow PlayerMotor to access state
    public bool IsWallRunningState => _isWallRunning;
    public Vector3 WallNormal => _wallNormal;
    public Vector3 WallRunDirection => _wallRunDirection;

    private CharacterController _characterController;

    void Awake()
    {
        _characterController = GetComponentInParent<CharacterController>();
    }

    public void UpdateWallRunLogic(Vector3 lastMoveDirection, ref float verticalVelocity)
        {
        if (_characterController == null) return;

        bool hitLeft = leftWallDetector.IsColliding;
        bool hitRight = rightWallDetector.IsColliding;

        if ((hitLeft || hitRight) && !_characterController.isGrounded)
        {
            bool wasWallRunning = _isWallRunning;
            _isWallRunning = true;
            _wallNormal = hitLeft ? leftWallDetector.outHit.normal : rightWallDetector.outHit.normal;

            if (!wasWallRunning)
            {
                verticalVelocity = 0;
            }

            Vector3 wallTangent = Vector3.Cross(_wallNormal, Vector3.up);
            if (Vector3.Dot(wallTangent, lastMoveDirection) < 0)
                _wallRunDirection = -wallTangent;
            else
                _wallRunDirection = wallTangent;
        }
        else
        {
            _isWallRunning = false;
            _wallRunDirection = Vector3.zero;
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