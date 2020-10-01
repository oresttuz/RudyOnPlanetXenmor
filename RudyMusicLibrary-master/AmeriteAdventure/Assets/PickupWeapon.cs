using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupWeapon : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player(Clone)")
        {
            GetComponent<BoxCollider>().enabled = false;
            WeaponAnimationHandler tempRef = GetComponentInChildren<WeaponAnimationHandler>();
            tempRef.tagString = "Enemy";
            tempRef.grandParentString = "Player";
            other.GetComponent<PlayerMovement>().AddWeapon(gameObject);
        }
    }
}
