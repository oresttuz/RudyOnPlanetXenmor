using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickupThrowable : MonoBehaviour
{
    public float timeTillPickup;

    private bool canPickup = false;
    private float timeSinceThrow = 0f;

    private void FixedUpdate()
    {
        if (!canPickup)
        {
            if (timeSinceThrow >= timeTillPickup) { canPickup = true; }
            else { timeSinceThrow += Time.fixedDeltaTime; }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "RoomTerrian")
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<CapsuleCollider>().isTrigger = false;
            GetComponent<BoxCollider>().enabled = true;
        }
        if (other.tag == "Enemy")
        {
            other.gameObject.GetComponentInParent<EnemyData>().Damage(2f);
        }

        if (canPickup)
        {
            if (other.name == "Player(Clone)")
            {
                other.GetComponent<PlayerMovement>().ReturnWeapon();
                Destroy(gameObject);
            }
        }
    }
}
