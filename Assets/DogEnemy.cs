using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;

public class DogEnemy : MonoBehaviour
{
    // Note: None of these numbers here have any meaning, its just so that the variables dont get null
    // All values changed in the editor later
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [Header("Detection Spheres")]
    [SerializeField] private float sleepDetectionRadius = 3f;
    [SerializeField] private float walkDetectionRadius = 8f;
    [SerializeField] private float chaseRadius = 10f;
    [SerializeField] private float barkNoiseRadius = 10f;

    [Header("Movement and Odds")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private Vector3 sleepSpotOffset = Vector3.zero;
    [SerializeField] private float minSleepDuration = 5f;
    [SerializeField] private float maxSleepDuration = 15f;
    [SerializeField] private float sleepChance = 0.7f;

    [Header("Idle Behavior")]
    [SerializeField] private float idleChanceWhileWalking = 0.3f;
    [SerializeField] private float minIdleDuration = 3f;
    [SerializeField] private float maxIdleDuration = 10f;

    [Header("Chase Behavior")]
    [SerializeField] private float barkInterval = 5f;
    [SerializeField] private float barkNoiseIntensity = 70f;
    
    // Increased chance of waking up when the player is near 
    // but not so near that is makes the dog wake up in the first place
    [Header("Some Extra sheet")]
    [SerializeField] private bool enableProximityWakeup = true;
    [SerializeField] private float proximityWakeupRadius = 6f;
    [SerializeField] private float proximityWakeupChance = 0.3f;

    private Vector3 sleepSpot;
   [SerializeField] private DogState currentState;
    private float lastBarkTime;
    private float stateTimer;
    private Vector3 currentWanderTarget;
    private float targetStateDuration;

    private enum DogState
    {
        Sleeping,
        Idle,
        Walking,
        Chasing,
        ReturningToSleep
    }

    private void Start()
    {
        sleepSpot = transform.position + sleepSpotOffset;
        if(agent == null || animator == null)
        {
            Debug.LogError("DogEnemy: Animator or Agent not found");
        }
        TransitionToState(DogState.Sleeping);
    }

    private void Update()
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case DogState.Sleeping:
                UpdateSleeping();
                break;
            case DogState.Idle:
                UpdateIdle();
                break;
            case DogState.Walking:
                UpdateWalking();
                break;
            case DogState.Chasing:
                UpdateChasing();
                break;
            case DogState.ReturningToSleep:
                UpdateReturningToSleep();
                break;
        }
        if(currentState != DogState.Chasing)
        {
            CheckForPlayer();
        }

        // Gotta get the global noise value for this function so lets keep that in there
        // But Noise is one factor only, the dog can walk up even if the player is near it
        CheckForNoise();

    }
    private void UpdateSleeping()
    {
        if(enableProximityWakeup && player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if(distanceToPlayer <= proximityWakeupRadius && distanceToPlayer > sleepDetectionRadius)
            {
                if(Random.value < proximityWakeupChance * Time.deltaTime)
                {
                    TransitionToState(DogState.Idle);
                    return;
                }
            }
        }

        // This for the random waking up during the night
        if(stateTimer >= targetStateDuration)
        {
            if(Random.value < 0.1f * Time.deltaTime)
            {
                TransitionToState(DogState.Idle);
            }
        }
    }
    private void UpdateIdle()
    {

        if(stateTimer >= targetStateDuration)
        {
            if(Random.value < sleepChance)
            {
                TransitionToState(DogState.ReturningToSleep);
            }
            else
            {
                TransitionToState(DogState.Walking);
            }

        }
    }
    private void UpdateWalking()
    {
        if(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if(Random.value < idleChanceWhileWalking)
            {
                TransitionToState(DogState.Idle);
            }
            else if(Random.value < sleepChance)
            {
                TransitionToState(DogState.ReturningToSleep);
            }
            else
            {
                SetRandomWanderDestination();
            }
        }
    }
    private void UpdateChasing()
    {
        if(player == null)
        {
            Debug.LogError("DogEnemy: THERE IS NO PLAYER");
        }
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if(distanceToPlayer > chaseRadius)
        {
            TransitionToState(DogState.ReturningToSleep);
            return;
        }
        agent.SetDestination(player.position);

        if(Time.time - lastBarkTime >= barkInterval)
        {
            Bark();
            lastBarkTime = Time.time;
        }
    }
    private void UpdateReturningToSleep()
    {
        if(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            TransitionToState(DogState.Sleeping);
        }
    }
    private void TransitionToState(DogState newState)
    {
        switch (currentState)
        {
            case DogState.Sleeping:
                agent.isStopped = false;
                break;
        }

        currentState = newState;
        stateTimer = 0f;

        switch (newState)
        {
            case DogState.Sleeping:
                agent.isStopped = true;
                SetAnimation("isSleep", true);
                targetStateDuration = Random.Range(minSleepDuration, maxSleepDuration);
                break;
            case DogState.Idle:
                agent.isStopped = true;
                SetAnimation("isIdle", true);
                targetStateDuration = Random.Range(minIdleDuration, maxIdleDuration);
                break;

            case DogState.Walking:  
                agent.speed = walkSpeed;
                agent.isStopped = false;
                SetRandomWanderDestination();
                SetAnimation("isWalk", true);
                break;
            case DogState.Chasing:
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                SetAnimation("isRun", true);
                Bark();
                lastBarkTime = Time.time;
                break;
            case DogState.ReturningToSleep:
                agent.speed = walkSpeed;
                agent.isStopped = false;
                agent.SetDestination(sleepSpot);
                SetAnimation("isWalk", true);
                break;

        }
    }
    private void CheckForPlayer()
    {
        if(player == null)
        {
            Debug.LogError("DogEnemy: THERE IS NO PLAYER");
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float detectionRadius = currentState == DogState.Sleeping ? sleepDetectionRadius : walkDetectionRadius;

        if(distanceToPlayer <= detectionRadius)
        {
            RaycastHit hit;
            Vector3 directionToPlayer = (player.position - transform.position).normalized;

            if(Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRadius))
            {
                if(hit.transform == player)
                {
                    TransitionToState(DogState.Chasing);
                }
            }
        }
    }
    private void CheckForNoise()
    {
        if(NoiseManager.Instance == null) Debug.LogError("DogEnemy: No NoiseManager");
        if(currentState == DogState.Chasing)
        {
            return; // DOnt have to do anything here since already chasing more sound doesnot make it unchase lol
        }

        var activeNoises = NoiseManager.Instance.GetActiveNoises();

        foreach(var noise in activeNoises)
        {
            float distance = Vector3.Distance(transform.position, noise.position);

            if(distance <= noise.radius)
            {
                float hearingIntensity = noise.intensity * (1 - (distance / noise.radius));

                // Gotta add some variables in the place of 40 and 30 but for now just leave it
                float wakeThreshold = currentState == DogState.Sleeping ? 40f : 30f;

                if(hearingIntensity >= wakeThreshold)
                {
                    if(Vector3.Distance(transform.position, player.position) <= chaseRadius)
                    {
                        TransitionToState(DogState.Chasing);
                    }
                    else if(currentState == DogState.Sleeping)
                    {
                        TransitionToState(DogState.Idle);
                    }
                }
            }
        }

    }
    private void SetRandomWanderDestination()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 randomPoint = sleepSpot + new Vector3(randomCircle.x, 0, randomCircle.y);

        NavMeshHit hit;
        if(NavMesh.SamplePosition(randomPoint, out hit, wanderRadius, NavMesh.AllAreas))
        {
            currentWanderTarget = hit.position;
            agent.SetDestination(currentWanderTarget);
        }
    }
    private void Bark()
    {
        if(NoiseManager.Instance != null)
        {
            NoiseManager.Instance.MakeNoise(transform.position, barkNoiseIntensity, barkNoiseRadius, 2f);
        }
    }
    private void SetAnimation(string paramName, bool value)
    {
        if(animator == null)
        {
            Debug.LogError("DogEnemy: THERE IS NO ANIMATOR YOU DUMMY");
        }
        animator.SetBool("isSleep", false);
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalk", false);
        animator.SetBool("isRun", false);

        animator.SetBool(paramName, value);
    }
    private void OnDrawGizmos()
    {
        // Sleep spot
        Gizmos.color = Color.blue;
        Vector3 sleepPos = Application.isPlaying ? sleepSpot : transform.position + sleepSpotOffset;
        Gizmos.DrawWireSphere(sleepPos, 2f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(sleepPos, wanderRadius);

        Gizmos.color = currentState == DogState.Sleeping ? Color.yellow : Color.red;
        float detectionRadius = currentState == DogState.Sleeping ? sleepDetectionRadius : walkDetectionRadius;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if(currentState == DogState.Chasing)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);
        }
        if(enableProximityWakeup && currentState == DogState.Sleeping)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, proximityWakeupRadius);
        }

    }

}
