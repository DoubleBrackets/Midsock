using UnityEngine;

public class MovementController : MonoBehaviour
{
    public struct MovementContext
    {
        public bool isGrounded;
        public bool isStableGrounded;
        public Vector3 interpolatedNormal;
        public Vector3 normal;

        public float groundedTime;
        public float airborneTime;
    }

    [SerializeField]
    private MovementConfig config;

    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private Collider physicsCollider;

    [field: SerializeField]
    public MovementContext CurrentContext { get; private set; }

    private float ungroundTime;

    private void Awake()
    {
        CurrentContext = new MovementContext();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 spherecastOrigin = physicsCollider.bounds.center + Vector3.up * config.SpherecastOffset;
        Gizmos.DrawWireSphere(spherecastOrigin, config.SpherecastRadius);

        Gizmos.DrawWireSphere(spherecastOrigin + Vector3.down * config.SpherecastDistance, config.SpherecastRadius);

        Gizmos.DrawLine(spherecastOrigin, spherecastOrigin + Vector3.down * config.SpherecastDistance);
    }

    public void UpdateMovement(Vector2 input, float deltaTime, Transform cameraTransform)
    {
        ungroundTime -= Time.deltaTime;

        input.Normalize();

        var context = new MovementContext();
        context.groundedTime = CurrentContext.groundedTime + deltaTime;
        context.airborneTime = CurrentContext.airborneTime + deltaTime;

        // Grounded check
        GroundedCheck(ref context);

        // Snap to ground
        if (ungroundTime <= 0f)
        {
            SnapToGround(ref context);
        }

        // Movement
        CalculateMovement(input, deltaTime, cameraTransform, context);

        CurrentContext = context;
    }

    public void TryStartJump()
    {
        if (ungroundTime > 0f)
        {
            return;
        }

        if (CurrentContext.isStableGrounded || CurrentContext.airborneTime < config.CoyoteTime)
        {
            rb.velocity = new Vector3(rb.velocity.x, config.JumpVelocity, rb.velocity.z);
            ungroundTime = 0.15f;
        }
    }

    private void SnapToGround(ref MovementContext context)
    {
        rb.AddForce(-context.normal * config.DefaultGravity);
        bool isMovingAwayFromGround = Vector3.Dot(rb.velocity, context.normal) > 0f;
        if (context.isGrounded && isMovingAwayFromGround)
        {
            // Snap to ground
            Vector3 snapToGround =
                Vector3.ProjectOnPlane(rb.velocity, context.normal).normalized * rb.velocity.magnitude;
            // rb.velocity = snapToGround;
        }
    }

    private void CalculateMovement(Vector2 input, float deltaTime, Transform cameraTransform, MovementContext context)
    {
        // project input against normal
        Vector3 moveDir = Vector3.ProjectOnPlane(
            cameraTransform.forward * input.y + cameraTransform.right * input.x,
            context.normal);

        // calculate acceleration
        float acceleration = context.isGrounded
            ? config.GroundAcceleration
            : config.AirAcceleration;

        // calculate friction
        bool isBunnyHopInterval = context.groundedTime < config.BunnyHopInterval;
        float friction = context.isGrounded && !isBunnyHopInterval
            ? config.GroundFriction
            : config.AirFriction;

        // calculate max speed
        float maxSpeed = context.isGrounded
            ? config.MaxGroundSpeed
            : config.MaxAirSpeed;

        // calculate velocity
        Vector3 currentVel = Vector3.ProjectOnPlane(rb.velocity, context.normal);
        Vector3 targetVel = Vector3.ProjectOnPlane(moveDir, context.normal).normalized * maxSpeed;

        Vector3 newVel = currentVel;

        // Air strafing
        if (!context.isGrounded && targetVel != Vector3.zero)
        {
            currentVel = targetVel.normalized * currentVel.magnitude;
            rb.velocity = new Vector3(currentVel.x, rb.velocity.y, currentVel.z);
        }

        // Increasing acceleration
        if (Vector3.Dot(targetVel, currentVel) >= 0)
        {
            newVel = Vector3.MoveTowards(currentVel, targetVel,
                Mathf.MoveTowards(acceleration, 0, currentVel.magnitude > maxSpeed ? friction : 0) * deltaTime);
        }
        // Decreasing
        else
        {
            newVel = Vector3.MoveTowards(currentVel, targetVel, (friction + acceleration) * deltaTime);
        }

        Vector3 step = newVel - currentVel;

        // apply friction

        // Movement velocity
        newVel = rb.velocity + step;

        rb.velocity = newVel;
    }

    private void GroundedCheck(ref MovementContext context)
    {
        // spherecast
        Vector3 spherecastOrigin = physicsCollider.bounds.center + Vector3.up * config.SpherecastOffset;
        Vector3 spherecastDirection = Vector3.down;
        float spherecastDistance = config.SpherecastDistance;
        float spherecastRadius = config.SpherecastRadius;
        LayerMask spherecastLayerMask = config.GroundedLayerMask;
        bool didHit = Physics.SphereCast(
            spherecastOrigin,
            spherecastRadius,
            spherecastDirection,
            out RaycastHit hitInfo,
            spherecastDistance,
            spherecastLayerMask,
            QueryTriggerInteraction.Ignore);

        if (didHit)
        {
            context.isGrounded = true;
            context.interpolatedNormal = hitInfo.normal;
            context.normal = GetRawNormal(hitInfo.point, hitInfo.distance * Vector3.down + spherecastOrigin);

            // check angle
            float angle = Vector3.Angle(Vector3.up, context.interpolatedNormal);
            context.isStableGrounded = angle <= config.MaxStableGroundedAngle;
        }
        else
        {
            context.isGrounded = false;
            context.isStableGrounded = false;
            context.interpolatedNormal = Vector3.up;
            context.normal = Vector3.up;
        }

        if (context.isGrounded)
        {
            Debug.DrawLine(hitInfo.point, hitInfo.point + context.normal, Color.blue, 5f);
            Debug.DrawLine(hitInfo.point, hitInfo.point + context.interpolatedNormal, Color.red, 5f);
        }

        if (!context.isGrounded)
        {
            context.groundedTime = 0;
        }
        else
        {
            context.airborneTime = 0;
        }
    }

    private Vector2 GetRawNormal(Vector3 hitPos, Vector3 collHitCenter)
    {
        Vector3 vecToHitPos = hitPos - collHitCenter;
        bool didHit = Physics.Raycast(collHitCenter - vecToHitPos.normalized * 0.5f,
            vecToHitPos.normalized,
            out RaycastHit hitInfo,
            vecToHitPos.magnitude + 0.6f,
            config.GroundedLayerMask,
            QueryTriggerInteraction.Ignore);

        if (didHit)
        {
            Debug.DrawLine(collHitCenter, collHitCenter + vecToHitPos.normalized * (vecToHitPos.magnitude + 0.5f),
                Color.magenta,
                5f);
            return hitInfo.normal;
        }

        Debug.DrawLine(collHitCenter, collHitCenter + vecToHitPos.normalized * (vecToHitPos.magnitude + 0.5f),
            Color.green,
            5f);
        return Vector3.up;
    }
}