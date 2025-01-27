using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour, IInteractable, IBubbleable, IDamageable
{
    [Header("Internal References")]
    [SerializeField] private Collider _collider;
    [SerializeField] private Transform model;
    
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
    [SerializeField] private TowerShotConfig shotConfig;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathClip;

    [SerializeField] private string description = "Enemy";

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isInterrupted = false;
    private Transform[] waypoints;
    private Ship ship;
    private PowerUpObjectPool powerUpPool;
    private EnemyObjectPool enemyPool;
    private Bubble bubble;
    private int floatUpAnimId = -1;
    private int spinAnimId = -1;
    private Transform originalParent;
    private float health;
    private Coroutine attackingCoroutine;
    private Vector3 rotationAxis;
    
    private int isRunningAnimId = -1;
    private int attackAnimId = -1;

    public event Action<Enemy> HealthChanged;
    public event Action<Enemy> Died;
    
    public bool IsBubbled => bubble != null;
    
    public string Description => description;

    public void Spawn(Ship ship, Transform endGoal, PowerUpObjectPool powerUpPool, EnemyObjectPool enemyPool)
    {
        waypoints = new [] { endGoal };
        this.ship = ship;
        this.powerUpPool = powerUpPool;
        this.enemyPool = enemyPool;
        
        health = maxHealth;
        animator.SetBool(isRunningAnimId, true);
        currentWaypointIndex = 0;
        isInterrupted = false;
    }
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        originalParent = transform.parent;
        StartCoroutine(RotationAxisSelection());

        isRunningAnimId = Animator.StringToHash("isRunning");
        attackAnimId = Animator.StringToHash("attack");
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
        if (bubble != null) model.transform.Rotate(rotationAxis, 60 * Time.deltaTime);
        
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
        
        animator.SetBool(isRunningAnimId, false);
    }

    public void Resume()
    {
        if (!gameObject.activeInHierarchy) return;
        
        isInterrupted = false;
        agent.enabled = true;
        agent.isStopped = false;
        MoveToWaypoint();
        
        animator.SetBool(isRunningAnimId, true);
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
        animator.SetBool(isRunningAnimId, false);
        while (true)
        {
            animator.SetTrigger(attackAnimId);
            animator.SetTrigger(attackAnimId);
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
            spinAnimId = LeanTween.rotateLocal(model.gameObject, Vector3.zero, 0.2f).id;
        }
        else
        {
            Resume();
            Debug.LogWarning("No ground detected below the PowerUp!");
        }
    }

    private void Die()
    {
        LeanTween.cancel(gameObject, floatUpAnimId);
        LeanTween.cancel(gameObject, spinAnimId);
        floatUpAnimId = -1;
        spinAnimId = -1;

        audioSource.PlayOneShot(deathClip);
        agent.enabled = false;
        LeanTween.delayedCall(gameObject, 0.3f, () => Destroy(gameObject));
        if (bubble != null) bubble.TakeDamage(bubble.Health);

        powerUpPool.Spawn(transform);
        Died?.Invoke(this);
    }

    #region IInteractable
    public bool CanInteract => bubble == null;
    
    public void Interact()
    {
        var bubble = Instantiate(bubblePrefab, _collider.transform.position, Quaternion.identity);
        bubble.Spawn(this, null);
    }
    #endregion IInteractable

    #region IBubbleable
    public bool MoveToPlayer => false;
    
    public void Bubble(Bubble bubble)
    {
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
        return _collider.transform.position;
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