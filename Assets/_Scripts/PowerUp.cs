using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PowerUp : MonoBehaviour, IInteractable, IBubbleable
{
    [Header("Internal References")]
    [SerializeField] private Collider modelCollider;
    
    [Header("Bubble Settings")]
    [SerializeField] private Bubble bubblePrefab;
    
    private int floatUpAnimId = -1;
    private int spinAnimId = -1;
    private bool isBubbled;
    private Vector3 rotationAxis;
    
    private Transform originalParent;
    private Player player;

    public bool CanInteract => true;

    public void Spawn(Player player)
    {
        this.player = player;
    }

    public void Interact()
    {
        var bubble = Instantiate(bubblePrefab, modelCollider.transform.position, Quaternion.identity);
        bubble.Spawn(this, player);
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

    private void DropToGround()
    {
        LeanTween.cancel(gameObject, floatUpAnimId);
        LeanTween.cancel(gameObject, spinAnimId);
        floatUpAnimId = -1;
        spinAnimId = -1;
        
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            Vector3 targetPosition = hit.point + Vector3.up * 0.5f; // Add a Y-offset of 0.5f (adjustable)

            floatUpAnimId = LeanTween.move(gameObject, targetPosition, 0.5f).setEaseInExpo().id;
            spinAnimId = LeanTween.rotateLocal(modelCollider.gameObject, Vector3.zero, 0.2f).id;
        }
        else
        {
            Debug.LogWarning("No ground detected below the PowerUp!");
        }
    }

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
        return transform.position;
    }
    #endregion IBubbleable
}