using UnityEngine;

public class PushMovement : MonoBehaviour
{
    public float CurrentSpeed => Vector3Helper.UpPlane(bodyrb.velocity).magnitude; //m/s
    public float CurrentForwardVelocity => transform.InverseTransformDirection(bodyrb.velocity).z;
    public float BrakeRate => brakePushRate;
    public float BrakeReleaseRate => brakeReleaseRate;
    public Transform FrontWheelTurner => frontWheelTurner;
    public Transform BackWheelTurner => backWheelTurner;
    private float processedThrottle => inputHandler.IsBraking ? (throttleForce*forceRounding) / 10 : (throttleForce * forceRounding);

    private const float forceRounding = 1000;

    #region Components
    [Header("Components")]
    [SerializeField] private Rigidbody bodyrb;
    [SerializeField] private VehicleMovementData movementData;
    [SerializeField] private Wheels wheels;
    private CarInputHandler inputHandler;
    #endregion

    #region acceleration & braking
    private float throttleForce => movementData.throttleForce;  
    private float brakePushRate => movementData.brakePushRate;
    private float brakeReleaseRate => movementData.brakeReleaseRate;    
    private Trajectory trajectory;
    #endregion

    #region Turning
    [SerializeField] private Transform frontWheelTurner;
    [SerializeField] private Transform backWheelTurner;
    private float wheelRotationSpeed => movementData.wheelRotationSpeed;
    private float wheelRotationMaxDegree => movementData.wheelRotationLimit;
    private float angularSpeed => CurrentSpeed > 20? CurrentSpeed * movementData.turnStrength : CurrentSpeed * (movementData.turnStrength * 2);// movementData.bodyRotationSpeed; //Move to ScriptableObject

    private ForwardDirection forwardDirection = ForwardDirection.neutral;

    private Quaternion currentFrontWheelRotation;
    private Quaternion currentBackWheelRotation;
    private Quaternion frontWheelTargetRotation;
    private Quaternion backWheelTargetRotation;
    private Quaternion slerpFrontRotation;
    private Quaternion slerpBackRotation;
    private Quaternion slerpBodyRotation;
    #endregion

    #region Stabilizer
    private Vector3 centerOfMass => movementData.centerOfMass;
    private float stabilizeValue => movementData.stabilizerTolerance;
    private float stabilizeSpeed => movementData.stabilizerReactionSpeed;
    private Vector3 StabilizeAxisReference => Quaternion.AngleAxis(bodyrb.angularVelocity.magnitude * Mathf.Rad2Deg * stabilizeValue / stabilizeSpeed, bodyrb.angularVelocity) * transform.up;
    private Vector3 stabilizerVector => Vector3.Cross(StabilizeAxisReference, Vector3.up);

    private float fallMod => movementData.fallMod;
    public float boostForce = 1000f;
    #endregion

    #region Jump variables
    public float jumpForce => movementData.jumpForce;
    private float currentJumpForce => inputHandler.IsJumping ? jumpForce : 0;
    #endregion

    
    #region Movement process
    private void SteerWheel()
    {
        currentFrontWheelRotation = frontWheelTurner.localRotation;
        frontWheelTargetRotation = Quaternion.Euler(Vector3.up * wheelRotationMaxDegree * inputHandler.Turning);
        slerpFrontRotation = Quaternion.Slerp(currentFrontWheelRotation, frontWheelTargetRotation, wheelRotationSpeed * Time.deltaTime);
        
        currentBackWheelRotation = backWheelTurner.localRotation;        
        backWheelTargetRotation = Quaternion.Euler(Vector3.up * wheelRotationMaxDegree * inputHandler.Turning * -1);
        slerpBackRotation = Quaternion.Slerp(currentBackWheelRotation, backWheelTargetRotation, wheelRotationSpeed * Time.deltaTime);
        
        frontWheelTurner.localRotation = slerpFrontRotation;
        backWheelTurner.localRotation = slerpBackRotation;
    }

    private void Rotation()
    {       

        if(GroundCheck())
        {
            //if(CurrentSpeed > 0.1f)
            {
                if (forwardDirection == ForwardDirection.forward || forwardDirection == ForwardDirection.neutral)
                {
                    slerpBodyRotation = Quaternion.Slerp(bodyrb.rotation, FrontWheelTurner.rotation,
                        angularSpeed * Mathf.Deg2Rad * Time.deltaTime);
                }
                else if(forwardDirection == ForwardDirection.backward)
                {
                    slerpBodyRotation = Quaternion.Slerp(bodyrb.rotation, BackWheelTurner.rotation,
                        angularSpeed * Mathf.Deg2Rad * Time.deltaTime);
                }            
            }
        }
    }

    private void UpdateRotation()
    {     
        if(transform.rotation != slerpBodyRotation)
            bodyrb.MoveRotation(slerpBodyRotation.normalized);
    }

    private Vector3 ForwardVelocity() => Vector3Helper.UpPlane(transform.forward) * inputHandler.Acceleration;

    private Vector3 TurningVelocity() => Vector3Helper.UpPlane(frontWheelTurner.right) * inputHandler.Turning;

    private void Throttle()
    {
        if (inputHandler.IsBraking && CurrentSpeed < 2f)
            return;

        trajectory.forward = ForwardVelocity() * processedThrottle;
    }

    private void Boost(CarInputHandler inputHandler)
    {
        bodyrb.AddForce(transform.forward * boostForce * forceRounding, ForceMode.Impulse);
    }
    private void UpdateMove() => bodyrb.AddForce(trajectory.sum);
  
    private void Jump() => bodyrb.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
    
    private void Fall() => bodyrb.AddForce(Vector3.up * fallMod * forceRounding * -1);

    private void Stabilizer() => bodyrb.AddTorque(stabilizerVector * stabilizeSpeed * stabilizeSpeed);

    private void UpdateRigidbodyConstraints()
    {
        if(GroundCheck())
        {
            if (inputHandler.IsBraking)
            {
                if (CurrentSpeed < 0.5f)
                    bodyrb.constraints |= RigidbodyConstraints.FreezeRotationY |
                        RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            }
            else
            {
                if (bodyrb.constraints.HasFlag(RigidbodyConstraints.FreezePositionZ))
                {
                    bodyrb.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotationY; ;
                }
            }
        }        
    }

    public void DirectionSwitch()
    {
        switch (forwardDirection)
        {
            case ForwardDirection.forward:
                if (CurrentForwardVelocity < 0.1f)
                    forwardDirection = ForwardDirection.neutral;                                  
                break;
            case ForwardDirection.backward:
                if (CurrentForwardVelocity > -0.1f)
                    forwardDirection = ForwardDirection.neutral;                
                break;
            case ForwardDirection.neutral:
                if (CurrentForwardVelocity > 0.1f)
                    forwardDirection = ForwardDirection.forward;
                else if (CurrentForwardVelocity < -0.1f)
                    forwardDirection = ForwardDirection.backward;
                break;
        }
    }
    #endregion

    #region Mono Behaviour Callback
    void Start()
    {
        inputHandler = GetComponent<CarInputHandler>();
        inputHandler.OnBoost += Boost;
    }

    private void OnDisable()
    {
        inputHandler.OnBoost -= Boost;
    }

    void Update()
    {
        bodyrb.centerOfMass = centerOfMass;
        UpdateRigidbodyConstraints();

        DirectionSwitch();         

        SteerWheel();

        Rotation();

        if(GroundCheck())
            Throttle();       
    }

    private void FixedUpdate()
    {
        UpdateMove();

        UpdateRotation();

        Jump();

        if(!GroundCheck())
        {
            Fall();
            Stabilizer();
        }
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position + transform.rotation * movementData.centerOfMass, 0.5f);
    }
#endif

#region utilities
    private bool GroundCheck() => wheels.IsGrounded;

    public struct Trajectory
    {
        public Vector3 forward;
        public Vector3 right;
        public Vector3 sum => forward + right;
    }

    [System.Serializable]
    public struct Wheels
    {
        public WheelPushControl FL;
        public WheelPushControl FR;
        public WheelPushControl BL;
        public WheelPushControl BR;

        public bool IsGrounded => FL.IsGrounded || FR.IsGrounded || BL.IsGrounded || BR.IsGrounded;
    }

    public enum ForwardDirection
    {
        forward,
        backward,
        neutral
    }
#endregion
}

public static class Vector3Helper
{
    public static Vector3 UpPlane (Vector3 currentAngle)
    {       
        currentAngle.y = 0;
        return currentAngle; 
    }
}


