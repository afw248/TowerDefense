using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractBow : MonoBehaviour
{
    [SerializeField]private float speed =13;
    [SerializeField]private LayerMask WhatEnemy;
    private CharacterController charController;
    public virtual void Awake()
    {
        charController = GetComponent<CharacterController>();
    }
    public virtual void Update()
    {
        charController.Move(transform.forward * speed * Time.deltaTime);          
    }
    public virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log("гого");
            Destroy(gameObject);
        }
    }

    public abstract void New();

    public abstract void Free();
}
