using FishNet.Object;
using UnityEngine;

public class PlayerShootScript : NetworkBehaviour
{
    [SerializeField]
    private int _damage;

    [SerializeField]
    public float _cooldownTime;

    private float _nextFireTime;

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (!IsClient)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time > _nextFireTime)
        {
            _nextFireTime = Time.time + _cooldownTime;
            Debug.Log("Shoot Client Side");
            ShootServerRpc(_damage, Camera.main.transform.position, Camera.main.transform.forward);
        }
    }

    [ServerRpc]
    private void ShootServerRpc(int damage, Vector3 pos, Vector3 dir)
    {
        Debug.Log("Shoot Server Side");
        RaycastHit hit;
        if (Physics.Raycast(pos, dir, out hit, Mathf.Infinity))
        {
            Debug.Log($"Something was hit {hit.collider.gameObject}");
            if (hit.transform.CompareTag("Player"))
            {
                Debug.Log("Something was damaged");
                var target = hit.transform.GetComponent<PlayerHealth>();
                target.ReceiveDamageServer(damage);
            }
        }
    }
}