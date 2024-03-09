using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerShootScript : NetworkBehaviour
{
    [SerializeField]
    private int Damage;

    [SerializeField]
    public float cooldownTime;
    
    private float nextFireTime;

    private void Update()
    {
        if(!base.IsOwner)
        {
            return;
        }

        if (!base.IsClient)
        {
            return;
        }
        
        if(Input.GetMouseButtonDown(0) && Time.time > nextFireTime)
        {
            nextFireTime = Time.time + cooldownTime;
            Debug.Log("Shoot Client Side");
            ShootServerRpc(Damage, Camera.main.transform.position, Camera.main.transform.forward);
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
                PlayerHealth target = hit.transform.GetComponent<PlayerHealth>();
                target.ReceiveDamageServer(damage);
            }
        }
    }
}
