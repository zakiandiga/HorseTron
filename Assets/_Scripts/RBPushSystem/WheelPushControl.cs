using UnityEngine;

public class WheelPushControl : MonoBehaviour
{
    [SerializeField] private Transform wheelMesh;
    private Rigidbody rb;
    private CarInputHandler inputHandler;
    private PushMovement body;

    [SerializeField] private WheelSide wheelSide = WheelSide.neutral;
    private Vector3 wheelMeshPosition;

    public struct PositionConstraint
    {
        public float x;
        public float y;
        public float z;

        public Vector3 sum;
    }

    private float currentDragValue;
    private float dragValueTarget = 8000f;
    private float defaultDragValue = 0.05f;

    private float smoothDragRef;
    private float dragDelta => body.BrakeRate;
    private float releaseDelta => body.BrakeReleaseRate;

    private Vector3 steeringAxis;
    private float steeringAngle;

    public bool IsGrounded { get { return isGrounded; } private set { } }
    private bool isGrounded;

    private float downForce = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        body = GetComponentInParent<PushMovement>();
        inputHandler = GetComponentInParent<CarInputHandler>();

        wheelMeshPosition = transform.localPosition;
    }

    private void Braking()
    {
        currentDragValue = Mathf.SmoothDamp(currentDragValue, dragValueTarget, ref smoothDragRef, dragDelta * Time.deltaTime);
        rb.drag = currentDragValue;
    }

    private void BrakeRelease()
    {
        if(currentDragValue != defaultDragValue)
        {
            currentDragValue = Mathf.SmoothDamp(currentDragValue, defaultDragValue, ref smoothDragRef, releaseDelta * Time.deltaTime);
            rb.drag = currentDragValue;
        }        
    }

    private void UpdateRotation()
    {
        wheelMeshPosition.y = transform.localPosition.y;
        wheelMesh.localPosition = wheelMeshPosition;
        
        wheelMesh.localRotation = transform.localRotation;
        //wheelMesh.transform.RotateAround(wheelMesh.transform.position, Vector3.right, body.CurrentForwardVelocity * Mathf.Rad2Deg * Time.deltaTime);
        switch (wheelSide)
        {
            case WheelSide.front:
                body.FrontWheelTurner.localRotation.ToAngleAxis(out steeringAngle, out steeringAxis);
                wheelMesh.transform.RotateAround(wheelMesh.transform.position, Vector3.up, steeringAngle * inputHandler.Turning);
                break;
            case WheelSide.back:
                body.BackWheelTurner.localRotation.ToAngleAxis(out steeringAngle, out steeringAxis);
                wheelMesh.transform.RotateAround(wheelMesh.transform.position, Vector3.up, steeringAngle * inputHandler.Turning);
                break;
            case WheelSide.neutral:
                break;
        }
    }
    
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer == 10)
        {
            if(!isGrounded)
                isGrounded = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.layer == 10)
        {
            if (!isGrounded)
                isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision col)
    {
        if (col.gameObject.layer == 10)
        { 
            if(isGrounded)
                isGrounded = false;
        }
    }

    private void Update()
    {
        if (inputHandler.IsBraking && isGrounded)
            Braking();
        else
            BrakeRelease();

        UpdateRotation();

        
    }

    private void FixedUpdate()
    {
        if (!IsGrounded)
            rb.velocity += Vector3.up * -downForce;
    }


    public enum WheelSide
    {
        front,
        back,
        neutral
    }
}
