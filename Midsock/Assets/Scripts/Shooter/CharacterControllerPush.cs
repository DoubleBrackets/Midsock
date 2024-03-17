using UnityEngine;

public class CharacterControllerPush : MonoBehaviour
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
        if (other.rigidbody != null)
        {
            Vector3 velocity = _characterController.velocity;
            Vector3 impactVelocity = Vector3.ProjectOnPlane(velocity, other.contacts[0].normal);

            other.rigidbody.WakeUp();
            float force = _pushForce; // * velocity.magnitude;
            Debug.Log($"Pushing {other.rigidbody.name} with force {force}");

            if (force > _maxForce)
            {
                force = _maxForce;
            }

            other.rigidbody.AddForce(-other.contacts[0].normal * force + Vector3.up * _upwardForce,
                ForceMode.Impulse);
        }
    }
}