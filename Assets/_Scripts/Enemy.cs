using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour, IInteractable, IBubbleable, IDamageable
{
    [Header("Internal References")]
    [SerializeField] private Collider modelCollider;
    
    [Header("Movement Settings")]
    [SerializeField] private float waypointTolerance = 0.5f;
    
    [Header("Bubble Settings")]
    [SerializeField] private Bubble bubblePrefab;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Enemy Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float damageToShip = 10f;
    [SerializeField] private float damageToBubble = 20f;
    [SerializeField] private float attackSpeed = 1f;

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isInterrupted = false;
    private Transform[] waypoints;
    private Ship ship;
    private Bubble bubble;
    private int floatUpAnimId = -1;
    private int spinAnimId = -1;
    private Transform originalParent;
    private Player player;
    private float health;
    private Coroutine attackingCoroutine;
    private Vector3 rotationAxis;

    public event Action<Enemy> HealthChanged;
    public event Action<Enemy> Died;

    public void Spawn(Transform[] waypoints, Player player, Ship ship)
    {
        this.waypoints = waypoints;
        this.player = player;
        this.ship = ship;
        
        health = maxHealth;
    }
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        originalParent = transform.parent;
        StartCoroutine(RotationAxisSelection());
    }

    private IEnumerator RotationAxisSelection()
    {
        while (true)
        {
            rotationAxis = Random.onUnitSphere;
            yield return new WaitForSeconds(2.5f);
        }
    }

    private void Start()
    {
        if (waypoints.Length > 0)
        {
            MoveToWaypoint();
        }
        else
        {
            Debug.LogWarning("Waypoints are not set for the enemy!");
        }
    }

    private void Update()
    {
        if (bubble != null) modelCollider.transform.Rotate(rotationAxis, 60 * Time.deltaTime);
        
        // If interrupted, don't process movement
        if (isInterrupted) return;

        // Check if the agent has reached the current waypoint
        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            // Move to the next waypoint or the end goal
            MoveToNextWaypoint();
        }
    }

    private void MoveToWaypoint()
    {
        if (currentWaypointIndex == waypoints.Length)
        {
            BeginAttackingShip();
        }
        else if (currentWaypointIndex < waypoints.Length)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    private void MoveToNextWaypoint()
    {
        if (currentWaypointIndex < waypoints.Length)
        {
            currentWaypointIndex++;
            MoveToWaypoint();
        }
    }

    public void Interrupt()
    {
        isInterrupted = true;
        agent.isStopped = true;
        agent.enabled = false;
        modelCollider.enabled = false;
    }

    public void Resume()
    {
        isInterrupted = false;
        modelCollider.enabled = true;
        agent.enabled = true;
        agent.isStopped = false;
        MoveToWaypoint();
    }

    private void BeginAttackingShip()
    {
        CancelAttack();
        attackingCoroutine = StartCoroutine(Attack(ship, damageToShip));
    }

    private void BeginAttackingBubble()
    {
        CancelAttack();
        attackingCoroutine = StartCoroutine(Attack(bubble, damageToBubble));
    }

    private IEnumerator Attack(IDamageable damageable, float damage)
    {
        while (true)
        {
            damageable.TakeDamage(damage);
            yield return new WaitForSeconds(attackSpeed);
        }
    }

    private void CancelAttack()
    {
        if (attackingCoroutine != null)
        {
            StopCoroutine(attackingCoroutine);
            attackingCoroutine = null;
        }
    }

    private void FloatUpToBubble()
    {
        LeanTween.cancel(gameObject, floatUpAnimId);
        LeanTween.cancel(gameObject, spinAnimId);
        floatUpAnimId = -1;
        spinAnimId = -1;
        
        floatUpAnimId = LeanTween
            .moveLocal(gameObject, Vector3.zero, 0.5f)
            .setOnComplete(() => BeginAttackingBubble())
            .id;
    }

    private void DropToGround()
    {
        LeanTween.cancel(gameObject, floatUpAnimId);
        LeanTween.cancel(gameObject, spinAnimId);
        floatUpAnimId = -1;
        spinAnimId = -1;
        
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetPosition = hit.point;
            floatUpAnimId = LeanTween
                .move(gameObject, targetPosition, 0.5f).setEaseInExpo()
                .setOnComplete(() => Resume())
                .id;
            spinAnimId = LeanTween.rotateLocal(modelCollider.gameObject, Vector3.zero, 0.2f).id;
        }
        else
        {
            Resume();
            Debug.LogWarning("No ground detected below the PowerUp!");
        }
    }

    private void Die()
    {
        Destroy(gameObject);
        if (bubble != null) bubble.TakeDamage(bubble.Health);
        Died?.Invoke(this);
    }

    #region IInteractable
    public bool CanInteract => bubble == null;
    
    public void Interact()
    {
        var bubble = Instantiate(bubblePrefab, modelCollider.transform.position, Quaternion.identity);
        bubble.Spawn(this, player);
    }
    #endregion IInteractable

    #region IBubbleable
    public bool MoveToPlayer => false;
    
    public void Bubble(Bubble bubble)
    {
        // interrupt and float up into bubble, begin attack bubble
        this.bubble = bubble;
        Interrupt();
        
        transform.SetParent(bubble.BubbleTransform, true);
        
        FloatUpToBubble();
    }

    public void PopBubble()
    {
        CancelAttack();
        bubble = null;
        
        // drop to ground and resume

        transform.SetParent(originalParent, true);
        DropToGround();
    }

    public Vector3 GetBubbleWorldPosition()
    {
        return modelCollider.transform.position;
    }

    public void TakeDamage(float damage)
    {   
        var tmpHealth = health;
        health += -damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (tmpHealth != health)
        {
            HealthChanged?.Invoke(this);
            if (health == 0) Die();
        }
    }

    #endregion IBubbleable
}