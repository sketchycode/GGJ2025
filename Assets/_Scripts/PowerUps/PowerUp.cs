using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PowerUp : MonoBehaviour, IInteractable, IBubbleable
{
    [Header("Internal References")]
    [SerializeField] private new Collider collider;
    [SerializeField] private Transform model;
    
    [Header("Bubble Settings")]
    [SerializeField] private Bubble bubblePrefab;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("PowerUp Settings")]
    [SerializeField] private float towerDetectionRadius = 10f;
    [SerializeField] private LayerMask towerLayer;
    [SerializeField] private TowerShotModifier modifier;
    
    private int floatUpAnimId = -1;
    private int spinAnimId = -1;
    private bool isBubbled;
    private Vector3 rotationAxis;
    
    private Transform originalParent;
    private Player player;
    private Collider[] hitColliders;
    private PowerUpObjectPool powerUpPool;
    
    public TowerShotModifier Modifier => modifier;

    public void Spawn(Player player, Collider[] reusableColliders, PowerUpObjectPool powerUpPool)
    {
        this.player = player;
        hitColliders = reusableColliders;
        this.powerUpPool = powerUpPool;
    }

    private void Awake()
    {
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

    private void Update()
    {
        if (isBubbled)
        {
            model.transform.Rotate(rotationAxis, 60 * Time.deltaTime);
        }
    }

    private void FloatUpToBubble()
    {
        LeanTween.cancel(gameObject, floatUpAnimId);
        LeanTween.cancel(gameObject, spinAnimId);
        floatUpAnimId = -1;
        spinAnimId = -1;
        
        floatUpAnimId = LeanTween.moveLocal(gameObject, Vector3.zero, 0.5f).id;
    }

    public void DropToGround(bool notifyTower = true)
    {
        LeanTween.cancel(gameObject, floatUpAnimId);
        LeanTween.cancel(gameObject, spinAnimId);
        floatUpAnimId = -1;
        spinAnimId = -1;
        
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetPosition = hit.point;
            floatUpAnimId = LeanTween
                .move(gameObject, targetPosition, 0.5f)
                .setEaseInExpo()
                .setOnComplete(_ => { if (notifyTower) NotifyTower(); })
                .id;
            spinAnimId = LeanTween.rotateLocal(model.gameObject, Vector3.zero, 0.2f).id;
        }
        else
        {
            Debug.LogWarning("No ground detected below the PowerUp!");
        }
    }

    public void DragToTower(Tower tower, Action onDragComplete)
    {
        LeanTween.cancel(gameObject, floatUpAnimId);
        LeanTween.cancel(gameObject, spinAnimId);
        floatUpAnimId = -1;
        spinAnimId = -1;

        LeanTween
            .move(gameObject, tower.GunTransform, 1.2f)
            .setOnComplete(_ =>
            {
                onDragComplete?.Invoke();
                powerUpPool.Despawn(this);
            });
    }

    private Tower GetNearestTower()
    {
        Tower foundTower = null;
        var minDistance = float.MaxValue;
        var hitCount = Physics.OverlapSphereNonAlloc(transform.position, towerDetectionRadius, hitColliders, towerLayer);
        
        for (int i = 0; i < hitCount && i < hitColliders.Length; i++)
        {
            var hitCollider = hitColliders[i];

            var tower = hitCollider.GetComponentInParent<Tower>();
            
            var distance= (hitCollider.transform.position - transform.position).sqrMagnitude;
            
            if (distance < minDistance)
            {
                minDistance = distance;
                foundTower = tower;
            }
        }
        return foundTower;
    }

    private void NotifyTower()
    {
        var tower = GetNearestTower();
        if (tower == null) return;

        tower.CollectPowerUp(this);
    }

    #region IInteractable
    public bool CanInteract => !isBubbled;

    public void Interact()
    {
        var bubble = Instantiate(bubblePrefab, collider.transform.position, Quaternion.identity);
        bubble.Spawn(this, player);
    }
    #endregion IInteractable

    #region IBubbleable

    bool IBubbleable.MoveToPlayer => true;

    void IBubbleable.Bubble(Bubble bubble)
    {
        isBubbled = true;
        collider.enabled = false;
        transform.SetParent(bubble.BubbleTransform, true);
        FloatUpToBubble();
    }

    void IBubbleable.PopBubble()
    {
        isBubbled = false;
        collider.enabled = true;
        transform.SetParent(originalParent, true);
        DropToGround();
    }
    
    Vector3 IBubbleable.GetBubbleWorldPosition()
    {
        return collider.transform.position;
    }

    public void TakeDamage(float damage)
    {
        // do nothing
    }

    #endregion IBubbleable
}