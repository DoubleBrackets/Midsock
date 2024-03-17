using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementConfig", menuName = "MovementConfig")]
public class MovementConfig : ScriptableObject
{
    [field: SerializeField]
    [field: Header("Grounded movement")]
    public float MaxGroundSpeed { get; private set; }

    [field: SerializeField] public float GroundAcceleration { get; private set; }

    [field: SerializeField] public float GroundFriction { get; private set; }


    [field: SerializeField]
    [field: Header("Air Movement")]
    public float MaxAirSpeed { get; private set; }

    [field: SerializeField] public float AirAcceleration { get; private set; }

    [field: SerializeField] public float AirFriction { get; private set; }

    [field: SerializeField]
    [field: Header("Jump")]
    public float JumpVelocity { get; private set; }

    [field: SerializeField] public float DefaultGravity { get; private set; }

    [field: SerializeField] public float JumpGravity { get; private set; }
    [field: SerializeField] public float BunnyHopInterval { get; private set; }
    [field: SerializeField] public float CoyoteTime { get; private set; }

    [field: Header("Grounding checks")]
    [field: SerializeField]
    public float MaxStableGroundedAngle { get; private set; }

    [field: SerializeField] public float GroundedDistance { get; private set; }

    [field: SerializeField] public LayerMask GroundedLayerMask { get; private set; }

    [field: SerializeField] public float SpherecastRadius { get; private set; }

    [field: SerializeField] public float SpherecastDistance { get; private set; }

    [field: SerializeField] public float SpherecastOffset { get; private set; }
}