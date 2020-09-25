using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    //Instances given in inspector
    public GameObject Player, Weapon;
    //public Transform detectPoint;

    //variables that can be fine tuned
    public LayerMask playerLayers, hurtsMe;
    public float timeBetweenDetect, radius, hp;
    public Vector3 BoxVec;

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
        WeaponAnimator = this.transform.GetChild(0).GetComponentInChildren<Animator>();
        if (WeaponAnimator == null)
        {
            Debug.LogError(this.name + " does not have an Animator");
            return;
        }
    }

    private void FixedUpdate()
    {
        if (timeSinceDetect >= timeBetweenDetect)
        {
            agent.SetDestination(Player.transform.position);
            agent.isStopped = false;
            timeSinceDetect = 0f;
        }
        else { timeSinceDetect += Time.fixedDeltaTime; }

        if (!WeaponAnimator.GetBool("Attacking"))
        {
            Collider[] hitPlayers = Physics.OverlapSphere(Weapon.transform.position, radius, playerLayers);
            if (hitPlayers.Length > 0)
            {
                agent.isStopped = true;
                agent.velocity = new Vector3(0f, 0f, 0f);
                WeaponAnimator.SetBool("Attacking", true);
            }
        }
    }
}
