using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimationHandler : MonoBehaviour
{
    private static readonly WeaponAnimationHandler instance;
    public static WeaponAnimationHandler Instance() { return instance; }

    public string tagString, grandParentString;
    public Animator WeaponAnimator;
    public int nextType;

    public void AnimationEnded()
    {
        if (grandParentString == "Player")
        {
            GetComponentInParent<PlayerMovement>().attacking = false;
        }
        WeaponAnimator.SetBool("Attacking", false);
        WeaponAnimator.SetInteger("Type", nextType);
    }

    private void OnTriggerEnter(Collider other)
    {
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
