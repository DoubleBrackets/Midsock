using FishNet.Object;
using UnityEngine;

public class CharacterControllerPush : NetworkBehaviour
{
    [SerializeField]
    private CharacterController _characterController;

    [SerializeField]
    private float _pushForce;

    [SerializeField]
    private float _upwardForce;

    [SerializeField]
    private float _maxForce;

    private void OnCollisionEnter(Collision other)
    {
        /*if (!IsOwner)
        {
            Debug.Log("Not owner");
            return;
        }*/

        var nob = other.gameObject.GetComponent<NetworkObject>();

        Debug.Log(nob);

        if (nob == null)
        {
            return;
        }

        Vector3 velocity = _characterController.velocity;
        Vector3 impactVelocity = Vector3.ProjectOnPlane(velocity, other.contacts[0].normal);

        float force = _pushForce;

        if (force > _maxForce)
        {
            force = _maxForce;
        }

        Debug.Log(other.rigidbody);

        if (other.rigidbody != null)
        {
            Vector3 forceVector = -other.contacts[0].normal * force + Vector3.up * _upwardForce;
            PushRPC(nob, forceVector);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PushRPC(NetworkObject nob, Vector3 force)
    {
        Debug.Log("Push");
        var rb = nob.GetComponent<Rigidbody>();
        rb.AddForce(force, ForceMode.Impulse);
    }
}