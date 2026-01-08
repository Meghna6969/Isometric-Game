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
    [SerializeField] private LayerMask detectionMask;

    [Header("Detection Spheres")]
    [SerializeField] private float sleepDetectionRadius = 3f;
    [SerializeField] private float walkDetectionRadius = 8f;
    [SerializeField] private float chaseRadius = 10f;
    [SerializeField] private float barkNoiseRadius = 10f;
    [SerializeField] private float hearingRadius = 20f;

    [Header("Movement and Odds")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private Vector3 sleepSpotOffset = Vector3.zero;
    [SerializeField] private float minSleepDuration = 5f;
    [SerializeField] private float proximitySenseRadius = 2.5f;
    [SerializeField] private float maxSleepDuration = 15f;
    [SerializeField] private float sleepChance = 0.7f;

    [Header("Idle Behavior")]
    [SerializeField] private float idleChanceWhileWalking = 0.3f;
    [SerializeField] private float minIdleDuration = 3f;
    [SerializeField] private float maxIdleDuration = 10f;

    [Header("Chase Behavior")]
    [SerializeField] private float barkInterval = 5f;
    [SerializeField] private float barkNoiseIntensity = 70f;

    [Header("Investigation Behavior")]
    [SerializeField] private float investigationThreshold = 30f;
    [SerializeField] private float investigationSpeed = 3.5f;
    [SerializeField] private float investigationWaitTime = 4f;
    
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

    private float proximityCheckTimer;
    private Vector3 noiseInvestigationTarget;

    private enum DogState
    {
        Sleeping,
        Idle,
        Walking,
        Chasing,
        ReturningToSleep,
        Investigating
    }

    private void Start()
    {
        sleepSpot = transform.position + sleepSpotOffset;
        if(agent == null || animator == null)
        {
            Debug.LogError("DogEnemy: Animator or Agent not found");
        }
        if(detectionMask == 0)
        {
            detectionMask = Physics.DefaultRaycastLayers;
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
            case DogState.Investigating:
                UpdateInvestigating();
                break;
        }
        if(currentState != DogState.Chasing)
        {
            CheckForPlayer();
            // Gotta get the global noise value for this function so lets keep that in there
            // But Noise is one factor only, the dog can walk up even if the player is near it
            CheckForNoise();
        }

        

    }
    private void UpdateSleeping()
    {
        if(enableProximityWakeup)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if(distanceToPlayer <= proximityWakeupRadius && distanceToPlayer > sleepDetectionRadius)
            {
                proximityCheckTimer += Time.deltaTime;

                if(proximityCheckTimer >= 1f)
                {
                    proximityCheckTimer = 0f;
                    if(Random.value < proximityWakeupChance)
                    {
                        TransitionToState(DogState.Idle);
                        return;
                    }
                }
            }
        }

        // This for the random waking up during the night
        if(stateTimer >= targetStateDuration)
        {
            if(Random.value < 0.1f)
            {
                TransitionToState(DogState.Idle);
            }
            else
            {
                stateTimer = 0;
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
    private void UpdateInvestigating()
    {
        if(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!animator.GetBool("isIdle"))
            {
                animator.SetBool("isWalk", false);
                animator.SetBool("isRun", false);
                animator.SetBool("isIdle", true);
            }
            if(stateTimer >= investigationWaitTime)
            {
                TransitionToState(DogState.Walking);
            }
        }
    }
    private void TransitionToState(DogState newState)
    {
        if(currentState == DogState.Sleeping)
        {
            agent.isStopped = false;
        }
        currentState = newState;
        stateTimer = 0f;
        proximityCheckTimer = 0f;

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
            case DogState.Investigating:
                agent.speed = investigationSpeed;
                agent.isStopped = false;
                NavMeshHit hit;
                if(NavMesh.SamplePosition(noiseInvestigationTarget, out hit, 2f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                else
                {
                    agent.SetDestination(noiseInvestigationTarget);
                }
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

        float dist = Vector3.Distance(transform.position, player.position);
        if(dist < proximitySenseRadius)
        {
            TransitionToState(DogState.Chasing);
            return;
        }
        float detectionRadius = currentState == DogState.Sleeping ? sleepDetectionRadius : walkDetectionRadius;
        if(dist <= detectionRadius)
        {
            Vector3 targetPosition = player.position + Vector3.up;
            Vector3 origin = transform.position + Vector3.up;
            Vector3 directionToPlayer = (targetPosition - origin).normalized;

            RaycastHit hit;
            if(Physics.Raycast(origin, directionToPlayer, out hit, detectionRadius, detectionMask))
            {
                if(hit.transform == player || hit.transform.IsChildOf(player))
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
        float loudestHeardIntensity = 0f;
        NoiseManager.NoiseEvent bestNoise = null;

        foreach(var noise in activeNoises)
        {
            float distance = Vector3.Distance(transform.position, noise.position);

            if(distance > hearingRadius) continue;
            if(distance > noise.radius) continue;
            float hearingIntensity = noise.GetCurrentIntensity() * (1 - (distance / noise.radius));
            if(hearingIntensity > loudestHeardIntensity)
            {
                loudestHeardIntensity = hearingIntensity;
                bestNoise = noise;
            }
        }
        if(bestNoise != null)
        {
           if(currentState == DogState.Investigating && Vector3.Distance(noiseInvestigationTarget, bestNoise.position) < 1f)
            {
                return;
            } 
            noiseInvestigationTarget = bestNoise.position;
            TransitionToState(DogState.Investigating);
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
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);

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
