using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

public class CSPMovement : NetworkBehaviour
{
    [SerializeField]
    private CharacterController _characterController;

    private bool _jumpQueued;

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpQueued = true;
        }
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (IsServer || IsClient)
        {
            TimeManager.OnTick += TimeManager_OnTick;
        }
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (TimeManager != null)
        {
            TimeManager.OnTick -= TimeManager_OnTick;
        }
    }

    private void TimeManager_OnTick()
    {
        if (IsOwner)
        {
            BuildActions(out MoveData md);
        }
    }

    private void BuildActions(out MoveData moveData)
    {
        moveData = default;
        moveData.Jump = _jumpQueued;

        //Unset queued values.
        _jumpQueued = false;
    }

    [Replicate]
    private void Move(MoveData moveData, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        //If jumping move the character up one unit.
        if (moveData.Jump && _characterController.isGrounded)
        {
            _characterController.Move(new Vector3(0f, 1f, 0f));
        }
    }

    [Reconcile]
    private void Reconcile(ReconcileData recData, bool asServer, Channel channel = Channel.Unreliable)
    {
        //Reset the client to the received position. It's okay to do this
        //even if there is no de-synchronization.
        transform.position = recData.Position;
    }
}

public struct MoveData : IReplicateData
{
    public bool Jump;

    /* Everything below this is required for
     * the interface. You do not need to implement
     * Dispose, it is there if you want to clean up anything
     * that may allocate when this structure is discarded. */
    private uint _tick;

    public void Dispose()
    {
    }

    public uint GetTick()
    {
        return _tick;
    }

    public void SetTick(uint value)
    {
        _tick = value;
    }
}

public struct ReconcileData : IReconcileData
{
    public Vector3 Position;

    /* Everything below this is required for
     * the interface. You do not need to implement
     * Dispose, it is there if you want to clean up anything
     * that may allocate when this structure is discarded. */
    private uint _tick;

    public void Dispose()
    {
    }

    public uint GetTick()
    {
        return _tick;
    }

    public void SetTick(uint value)
    {
        _tick = value;
    }
}