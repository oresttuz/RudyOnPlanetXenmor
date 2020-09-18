using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SlimeMovement : MonoBehaviour
{
    public Vector3 BoxVec;
    public Rigidbody rb;
    public Transform enemyCenter, detectPoint;
    public LayerMask playerLayers;
    public GameObject spriteObject;
    public bool chargeDone, idle;

    private Animator SpriteAnimator;
    private NavMeshAgent agent;
    private float defSpeed;
    private bool playerDetected = false;
    private Transform playerTransform;
    

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        SpriteAnimator = spriteObject.GetComponentInChildren<Animator>();
        if (agent == null)
        {
            Debug.LogError(this + " does not have a NavMeshAgent Component");
            return;
        }
        if (SpriteAnimator == null)
        {
            Debug.LogError(spriteObject + " does not have an Animtor Component");
            return;
        }
        defSpeed = agent.speed;
        SpriteAnimator.SetBool("Moving", false);
        chargeDone = false;
        idle = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (chargeDone && (!idle))
        {
            agent.isStopped = false;
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = new Vector3(0f, 0f, 0f);
        }
        spriteObject.transform.position = new Vector3(this.transform.position.x, 0f, this.transform.position.z);
    }

    private void FixedUpdate()
    {
        if (playerDetected)
        {
            if (playerTransform != null)
            {
                if (chargeDone)
                {
                    agent.SamplePathPosition(NavMesh.AllAreas, 10.0f, out NavMeshHit hit);
                    if (1.0f < hit.distance && hit.distance < 10.0f)
                    {
                        agent.speed = 3f;
                    }
                    else
                    {
                        agent.speed = defSpeed;
                    }
                    agent.SetDestination(playerTransform.position); 
                }
            }
        }

        Collider[] hitPlayers = Physics.OverlapBox(detectPoint.position, BoxVec / 2f, Quaternion.identity, playerLayers);
        if (hitPlayers.Length == 0)
        {
            playerDetected = false;
            idle = true;
            SpriteAnimator.SetBool("Moving", false);
        }
        else
        {
            if (!playerDetected)
            {
                foreach (Collider player in hitPlayers)
                {
                    playerDetected = true;
                    playerTransform = player.transform;
                    SpriteAnimator.SetBool("Moving", true);
                    idle = false;
                }
            }
        }
    }

    public void GotDamaged()
    {
        chargeDone = false;
        agent.isStopped = true;
        agent.velocity = new Vector3(0f, 0f, 0f);
        SpriteAnimator.SetBool("Damaged", false);
    }

    private void OnTriggerEnter(Collider objectCollider)
    {
        if (objectCollider.gameObject.tag == "Player")
        {
            objectCollider.gameObject.GetComponent<PlayerMovement>().hb_instance.Damage(0.25f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (detectPoint == null)
        {
            return;
        }
        Gizmos.DrawWireCube(detectPoint.position, BoxVec);
    }
}
