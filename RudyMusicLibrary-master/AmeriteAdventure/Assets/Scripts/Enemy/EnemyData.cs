using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyData : MonoBehaviour
{
    public RoomGeneration myIns;
    public HealthBar EnemyHB_Instance;
    public float health;
    public float dmg;
    public bool hasDmgAnimation;
    public int myX, myY;

    private void Start()
    {
        //base start stats
        health = 5f;
        dmg = 0.75f;

        //Stuff to do with my healthbar
        EnemyHB_Instance.Init(this.transform.position, this.transform);
        EnemyHB_Instance.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
    }

    private void FixedUpdate()
    {
        if (health < 0f)
        {
            myIns.EnemyDied(myX, myY);
            Destroy(EnemyHB_Instance.gameObject);
            Destroy(this.gameObject);
        }
    }

    public void Damage(float dmg)
    {
        health -= dmg;
        EnemyHB_Instance.Damage(dmg);
        if (hasDmgAnimation) { this.GetComponentInChildren<Animator>().SetBool("Damaged", true); }
    }
}
