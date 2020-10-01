using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimationHandler : MonoBehaviour
{
    private static readonly WeaponAnimationHandler instance;
    public static WeaponAnimationHandler Instance() { return instance; }

    public bool Thrown;
    public float timeTillCanPickup, timeSinceThrow = 0f;

    public string tagString, grandParentString;
    public Animator WeaponAnimator;
    public int nextType;
    public Vector3 throwForce;

    public void FixedUpdate()
    {
        if (Thrown)
        {
            if (timeSinceThrow < timeTillCanPickup)
            {
                timeSinceThrow += Time.fixedDeltaTime;
            }
            else
            {
                timeSinceThrow = 0f;
                Thrown = false;
                GetComponentInParent<BoxCollider>().enabled = true;
            }
        }
    }

    public void AnimationEnded()
    {
        if (grandParentString == "Player")
        {
            GetComponentInParent<PlayerMovement>().attacking = false;
        }
        WeaponAnimator.SetBool("Attacking", false);
        WeaponAnimator.SetInteger("Type", nextType);
    }

    public void Throw()
    {
        transform.parent.GetComponentInParent<PlayerMovement>().RemoveWeapon();
        transform.parent.SetParent(null);
        transform.parent.position = new Vector3(transform.parent.position.x, 1f, transform.parent.position.z);
        Vector3 force = new Vector3(transform.parent.forward.x * throwForce.x, 0f, transform.parent.forward.z * throwForce.z);
        transform.parent.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
        Debug.Log("Thrown: " + force);
        Thrown = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "RoomTerrian" && transform.parent.root == null)
        {
            GetComponentInParent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
            timeSinceThrow = 0f;
            Thrown = false;
        }
        if (grandParentString == "Player")
        {
            if (other.gameObject.tag == tagString)
            {
                other.gameObject.GetComponentInParent<EnemyData>().Damage(2f);
            }
        }
        else if(grandParentString == "Enemy")
        {
            if (other.gameObject.tag == tagString)
            {
                other.gameObject.GetComponent<PlayerMovement>().hb_instance.Damage(0.25f);
            }
        }
    }
}
