using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyData : MonoBehaviour
{
    public float health;
    public float damage;
    public bool isNested, hasDmgAnimation, isBoss, isHealBaby;

    public GameObject nestedEnemyBaby, nestedEnemySoldier;

    [HideInInspector]
    public RoomGeneration myIns;
    [HideInInspector]
    public HealthBar EnemyHB_Instance;
    [HideInInspector]
    public int myX, myY;

    private void Start()
    {
        //Stuff to do with my healthbar
        EnemyHB_Instance.Init(Mathf.FloorToInt(health), this.transform.position, this.transform);
        EnemyHB_Instance.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        GetComponent<EnemyMovement>().Dmg = damage;
        if (!GetComponent<EnemyMovement>().hasNoWeapon) { GetComponentInChildren<WeaponAnimationHandler>().Dmg = damage; }
    }

    private void FixedUpdate()
    {
        if (health < 0f)
        {
            if (isNested && !isBoss)
            {
                myIns.EnemySpawned(myX, myY, nestedEnemyBaby, null);
                myIns.EnemySpawned(myX, myY, nestedEnemyBaby, null);
            }
            myIns.EnemyDied(myX, myY);
            Destroy(EnemyHB_Instance.gameObject);
            Destroy(this.gameObject);
        }
    }

    public void Damage(float dmg)
    {
        if (dmg <= 0f) { return; }
        health -= dmg;
        EnemyHB_Instance.Damage(dmg);
        
        if (hasDmgAnimation) { this.GetComponentInChildren<Animator>().SetBool("Damaged", true); }
        if (isBoss)
        {
            myIns.EnemySpawned(myX, myY, nestedEnemyBaby, gameObject);
            myIns.EnemySpawned(myX, myY, nestedEnemyBaby, gameObject);
        }
    }

    public void SpawnMerged()
    {
        Debug.Log("Spawning Merged");
        myIns.EnemyDied(myX, myY);
        myIns.EnemySpawned(myX, myY, nestedEnemySoldier, null);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "HealBaby(Clone)" && name != "HealBaby(Clone)")
        {
            health += 0.5f;
            EnemyHB_Instance.Heal(0.5f);
            myIns.EnemyDied(myX, myY);
            Destroy(other.GetComponent<EnemyData>().EnemyHB_Instance.gameObject);
            Destroy(other.gameObject);
        }
    }
}
