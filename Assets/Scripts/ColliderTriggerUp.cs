using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTriggerUp : MonoBehaviour, MultiBoxCollider
{
    /* Semi-recursive, ends when there is no longer a parent */
    public void colliderEnter(Collider2D collision, MultiBoxCollider childScript)
    {
        this.OnTriggerEnter2D(collision);
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject parent = this.transform.parent.gameObject;
        if (parent == null)
        {
            return;
        }
        
        MultiBoxCollider parentCollider = parent.GetComponent<MultiBoxCollider>();
        if (parentCollider == null)
        { 
            return;
        }

        parentCollider.colliderEnter(collision, this);
    }
}
