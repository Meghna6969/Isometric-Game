using System.Numerics;
using UnityEngine;
using UnityEngine.AI;

public class DogEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [Header("Detection Spheres")]
    [SerializeField] private float sleepDetectionRadius = 3f;
    [SerializeField] private float walkDetectionRadius = 8f;
    [SerializeField] private float chaseRadius = 10f;

    [Header("Movement and Odds")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float wanderRadius = 10f;
    public Transform sleepSpot;
    [SerializeField] private float minSleepDuration = 5f;
    [SerializeField] private float maxSleepDuration = 15f;
    [SerializeField] private float sleepChance = 0.7f;
}
