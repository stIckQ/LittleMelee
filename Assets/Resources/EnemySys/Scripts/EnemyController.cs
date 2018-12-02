using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController :MonoBehaviour{

    public float lookRadius = 10f;
    public float moveSpeed = 1f;
    public float moveRadius = 5f;
    public float waitTime = 2f;
    public bool CanWander = true;
    public bool canAttack = true;
    public GameObject attackParticle;

    private Transform target;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private NavMeshHit navMeshHit;
    private Vector3 wanderPosition;
    private Coroutine coroutine;
    private int coroutineCount;
    private bool isGettingHit;
    private EnemyStates enemyStates;
    private GameObject gameManager;
    private GameController gameController;

    // Use this for initialization
    void Start () {
        navMeshAgent = GetComponent<NavMeshAgent>();
        target = PlayerManager.instance.player.transform;
        animator = GetComponent<Animator>();
        navMeshAgent.speed = moveSpeed;
        wanderPosition = RandomWanderPosition();
        coroutineCount = 0;
        isGettingHit = false;
        enemyStates = GetComponent<EnemyStates>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
        gameController = gameManager.GetComponent<GameController>();
	}
	
	// Update is called once per frame
	void Update () {
        if(gameController.isPlaying)
        {
            if (enemyStates.isAlive)
            {
                float distance = Vector3.Distance(target.position, transform.position);

                if (distance <= EnemyStates.attackRange)
                {
                    if (canAttack)
                    {
                        transform.LookAt(target.position);
                        animator.SetInteger("Condition", EnemyStates.animState_Attack);
                    }
                }
                else
                {
                    AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    if (!(animatorStateInfo.IsName("Attack") && animatorStateInfo.normalizedTime % 1f < 0.9f))
                    {
                        if (distance <= lookRadius)
                        {
                            TrackPlayer();
                        }
                        else
                        {
                            Wander();
                        }

                        if (navMeshAgent.velocity.magnitude > 0.2f)
                        {
                            if (!isGettingHit)
                            {
                                animator.SetInteger("Condition", EnemyStates.animState_Walk);
                            }
                        }
                        else
                        {
                            if (!isGettingHit)
                            {
                                animator.SetInteger("Condition", EnemyStates.animState_Idle);
                            }
                        }
                    }
                }
            }
            else
            {
                Die();
            }
        }
        else
        {
            animator.SetInteger("Condition", EnemyStates.animState_Idle);
        }
	}

    public void TrackPlayer()
    {
        if (!isGettingHit)
        {
            navMeshAgent.stoppingDistance = EnemyStates.attackRange;
            navMeshAgent.SetDestination(target.position);
        }
    }

    //wander
    public void Wander()
    {
        if(CanWander)
        {
            navMeshAgent.stoppingDistance = 0f;
            if (Vector3.Distance(transform.position, wanderPosition) < 0.1f)
            {
                StartCoroutine(WanderWait());
                wanderPosition = RandomWanderPosition();
            }
            else
            {
                navMeshAgent.SetDestination(wanderPosition);
                if(!navMeshAgent.hasPath)
                {
                    wanderPosition = RandomWanderPosition();
                }
            }
            //Debug.Log(transform.position.ToString()+wanderPosition.ToString()+ Vector3.Distance(transform.position, wanderPosition).ToString());
        }
    }

    //after wander wait
    private IEnumerator WanderWait()
    {
        CanWander = false;
        yield return new WaitForSeconds(waitTime);
        CanWander = true;

    }

    //get random wander positon
    public Vector3 RandomWanderPosition()
    {
        Vector3 randomPosition = Random.insideUnitSphere * moveRadius + transform.position;
        NavMesh.SamplePosition(randomPosition, out navMeshHit, moveRadius, -1);
        randomPosition.y = Terrain.activeTerrain.SampleHeight(randomPosition);

        return new Vector3(randomPosition.x, randomPosition.y, randomPosition.z);

    }

    //get hit
    public void GetHit(Vector3 attackPosition)
    {
        if(enemyStates.isAlive)
        {
            //Damage
            GetComponent<EnemyStates>().TakeDamage();
            animator.SetInteger("Condition", EnemyStates.animState_GetHit);

            //attack effect
            GameObject particle = Instantiate(attackParticle, attackPosition, Quaternion.identity)as GameObject;
            ParticleSystem particleSystem = particle.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main=particleSystem.main;
            main.startSize= 0.1f;
            Destroy(particle, 2);

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(GetHitRecover(coroutineCount++));
        }
    }

    //after get hit, need recover
    IEnumerator GetHitRecover(int i)
    {
        canAttack = false;
        isGettingHit = true;
        yield return new WaitForSeconds(EnemyStates.GetHitRecoverTime);
        isGettingHit = false;
        canAttack = true;
    }

    public void Die()
    {
        animator.SetInteger("Condition", EnemyStates.animState_Die);
    }

    //test range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
