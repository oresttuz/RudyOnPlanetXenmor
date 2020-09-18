using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public HealthBar phb_instance;
    public LayerMask hurtsMe;
    public List<int> weaponOneAttacks, weaponTwoAttacks;

    private void Start()
    {
        weaponOneAttacks = new List<int>();
        weaponTwoAttacks = new List<int>();
        //Default Settings

    }

    private void OnTriggerEnter(Collider objectCollider)
    {
        if (objectCollider.gameObject.layer == hurtsMe)
        {
            if (objectCollider.gameObject.tag == "Enemy")
            {
                phb_instance.Damage(objectCollider.gameObject.GetComponentInParent<EnemyData>().dmg);
                if (phb_instance.currHealth < 0f)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}
