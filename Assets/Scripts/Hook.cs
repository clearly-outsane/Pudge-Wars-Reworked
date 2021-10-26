using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.Examples.Additive;
using UnityEngine;
using UnityTemplateProjects;
using PlayerController = Mirror.Examples.MultipleAdditiveScenes.PlayerController;

public class Hook : NetworkBehaviour
{
    public string[] tagstoCheck;
    public float speed, returnSpeed, range, stopRange;

    [SyncVar]
    public GameObject casterGameObj;
    
    [HideInInspector]
    public Transform caster, collidedWidth;
    private LineRenderer _lineRenderer;
    private bool hasCollided;
    
    
    void Start()
    {
        _lineRenderer = transform.Find("Line").GetComponent<LineRenderer>();
    }


    public void RetractHook()
    {
        transform.LookAt(casterGameObj.transform);
        
        var dist = Vector3.Distance(transform.position, casterGameObj.transform.position);
        if (dist < stopRange)
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        if (casterGameObj.transform)
        {
            _lineRenderer.SetPosition(0, casterGameObj.transform.position);
            _lineRenderer.SetPosition(1, transform.position);
            
            if (hasCollided) 
            {
                // this part is moving the hook
                RetractHook();
            }
            
            else
            {
                var dist = Vector3.Distance(transform.position, casterGameObj.transform.position);
                if (dist > range)
                {
                    Collision(null);
                }
            }
            
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            
            // this is moving the player
            // if (collidedWidth)
            // {
            //     collidedWidth.transform.position = transform.position; 
            // }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasCollided && tagstoCheck.Contains(other.tag))
        {
            if (other.tag.Equals("Player"))
            {            
                // GetComponent<NetworkIdentity>().RemoveClientAuthority();
                // GetComponent<NetworkIdentity>().AssignClientAuthority(other.GetComponent<NetworkIdentity>().connectionToClient);
                
                other.GetComponent<MovementController>().TriggerCollision(casterGameObj, returnSpeed);
            }

            Collision(other.transform);
        }
    }

    private void Collision(Transform col)
    {
        speed = returnSpeed;
        hasCollided = true;
    
        if (col)
        {
            transform.position = col.position;
            collidedWidth = col.transform;
        }
    }
}