using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PowerUp : MonoBehaviour, IInteractable, IBubbleable
{
    [Header("Internal References")]
    [SerializeField] private Collider modelCollider;
    
    [Header("Bubble Settings")]
    [SerializeField] private Bubble bubblePrefab;
    [SerializeField] private LayerMask groundLayer;
    
    private int floatUpAnimId = -1;
    private int spinAnimId = -1;
    private bool isBubbled;
    private Vector3 rotationAxis;
    
    private Transform originalParent;
    private Player player;

    public void Spawn(Player player)
    {
        this.player = player;
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
            modelCollider.transform.Rotate(rotationAxis, 60 * Time.deltaTime);
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

    public void DropToGround()
    {
        LeanTween.cancel(gameObject, floatUpAnimId);
        LeanTween.cancel(gameObject, spinAnimId);
        floatUpAnimId = -1;
        spinAnimId = -1;
        
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetPosition = hit.point;
            floatUpAnimId = LeanTween.move(gameObject, targetPosition, 0.5f).setEaseInExpo().id;
            spinAnimId = LeanTween.rotateLocal(modelCollider.gameObject, Vector3.zero, 0.2f).id;
        }
        else
        {
            Debug.LogWarning("No ground detected below the PowerUp!");
        }
    }

    #region IInteractable
    public bool CanInteract => !isBubbled;

    public void Interact()
    {
        var bubble = Instantiate(bubblePrefab, modelCollider.transform.position, Quaternion.identity);
        bubble.Spawn(this, player);
    }
    #endregion IInteractable

    #region IBubbleable

    bool IBubbleable.MoveToPlayer => true;

    void IBubbleable.Bubble(Bubble bubble)
    {
        isBubbled = true;
        modelCollider.enabled = false;
        transform.SetParent(bubble.BubbleTransform, true);
        FloatUpToBubble();
    }

    void IBubbleable.PopBubble()
    {
        isBubbled = false;
        modelCollider.enabled = true;
        transform.SetParent(originalParent, true);
        DropToGround();
    }
    
    Vector3 IBubbleable.GetBubbleWorldPosition()
    {
        return modelCollider.transform.position;
    }

    public void TakeDamage(float damage)
    {
        // do nothing
    }

    #endregion IBubbleable
}