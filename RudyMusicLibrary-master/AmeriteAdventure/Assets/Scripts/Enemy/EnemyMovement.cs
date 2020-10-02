using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    //Instances given in inspector
    public GameObject Weapon;
    
    [HideInInspector]
    public GameObject Player;
    [HideInInspector]
    public float Dmg, timeTilEnable = 0.5f, timeSinceSpawn, cooldown = 0f,
        timeSlowed, timeTilUnslow, timeStunned, timeTilUnstunned;

    //variables that can be fine tuned
    public LayerMask playerLayers, hurtsMe;
    public float timeBetweenDetect, radius;
    public bool hasNoWeapon, isHealBaby;

    //private variables
    private NavMeshAgent agent;
    private float timeSinceDetect;
    private Animator WeaponAnimator;

    // Start is called before the first frame update
    private void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError(this.name + " does not have a NavMeshAgent");
            return;
        }
        agent.enabled = false;
        timeSinceSpawn = 0f;
        if (hasNoWeapon) { return; }
        WeaponAnimator = this.transform.GetChild(0).GetComponentInChildren<Animator>();
        if (WeaponAnimator == null)
        {
            Debug.LogError(this.name + " does not have an Animator");
            return;
        }
    }

    private void Update()
    {
        if (!agent.enabled)
        {
            timeSinceSpawn += Time.deltaTime;
            if (timeSinceSpawn >= timeTilEnable) { agent.enabled = true; }
            else { return; }
        }
        if (Player == null) { return; }
        if (!hasNoWeapon)
        {
            if (!WeaponAnimator.GetBool("Attacking") && cooldown <= 0f)
            {
                Collider[] hitPlayers = Physics.OverlapSphere(Weapon.transform.position, radius, playerLayers);
                if (hitPlayers.Length > 0)
                {
                    agent.isStopped = true;
                    agent.velocity = new Vector3(0f, 0f, 0f);
                    WeaponAnimator.SetBool("Attacking", true);
                }
                else
                {
                    agent.SetDestination(Player.transform.position);
                    agent.isStopped = false;
                }
            }
            else
            {
                if (cooldown >= 0f) { cooldown -= Time.deltaTime; }
                agent.SetDestination(Player.transform.position);
                agent.isStopped = false;
            }
        }
        else
        {
            agent.SetDestination(Player.transform.position);
            agent.isStopped = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isHealBaby && other.name == "HealBaby(Clone)")
        {
            Debug.Log("Upgrade");
            GetComponent<EnemyData>().SpawnMerged();
            Destroy(GetComponent<EnemyData>().EnemyHB_Instance.gameObject);
            Destroy(gameObject);
            return;
        }
        if (hasNoWeapon)
        {
            if (other.name == "Player(Clone)")
            {
                other.gameObject.GetComponent<PlayerMovement>().hb_instance.Damage(Dmg);
            }
        }
    }
}
