using UnityEngine;

public class CharacterControllerPush : MonoBehaviour
{
    [SerializeField]
    private CharacterController _characterController;

    [SerializeField]
    private float _pushForce;

    [SerializeField]
    private float _upwardForce;

    private void OnCollisionEnter(Collision other)
    {
        if (other.rigidbody != null)
        {
            Vector3 velocity = _characterController.velocity;
            Vector3 impactVelocity = velocity; //Vector3.ProjectOnPlane(velocity, other.contacts[0].normal);

            other.rigidbody.WakeUp();
            Debug.Log($"Pushing {other.rigidbody.name} with force {impactVelocity * _pushForce}");
            other.rigidbody.AddForce(-other.contacts[0].normal * _pushForce + Vector3.up * _upwardForce,
                ForceMode.Impulse);
        }
    }
}