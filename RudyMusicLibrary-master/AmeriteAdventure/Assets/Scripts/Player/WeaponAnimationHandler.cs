﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimationHandler : MonoBehaviour
{
    private static readonly WeaponAnimationHandler instance;
    public static WeaponAnimationHandler Instance() { return instance; }

    public bool Throwable, canUseKnowledge;

    public string tagString, grandParentString;
    public Animator WeaponAnimator;
    public int nextType;
    public Vector3 throwForce;

    [HideInInspector]
    public float Dmg;
    [HideInInspector]
    public AttackData[] weaponKnowledge;


    public void AnimationEnded()
    {
        if (grandParentString == "Player")
        {
            transform.parent.GetComponentInParent<PlayerMovement>().attacking = false; //might need to change back
            if (Throwable)
            {
                WeaponAnimator.SetBool("Attacking", false);
                WeaponAnimator.SetInteger("Type", nextType);
                transform.parent.GetComponentInParent<PlayerMovement>().ThrowWeapon();
                return;
            }
        }
        else if (grandParentString == "Enemy")
        {
            transform.parent.GetComponentInParent<EnemyMovement>().cooldown = 0.5f;
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
                //EnableWeaponKnowledge(other.gameObject);
                other.gameObject.GetComponentInParent<EnemyData>().Damage(2f);
            }
        }
        else if (grandParentString == "Enemy")
        {
            if (other.gameObject.tag == tagString)
            {
                other.gameObject.GetComponent<PlayerMovement>().hb_instance.Damage(Dmg);
            }
        }
    }

    public void EnableWeaponKnowledge(GameObject thingToAffect)
    {
        int technique = WeaponAnimator.GetInteger("Type");
        AttackType knowledge = weaponKnowledge[technique].knowledge;
        switch (knowledge)
        {
            case AttackType.Fire:
                //deal tick dmg
                break;
            case AttackType.Ice:
                //slow enemy
                break;
            case AttackType.Lighting:
                //stun enemy
                break;
            case AttackType.Nature:
                //summon bush
                break;
            case AttackType.None:
                break;
        }
    }

}
