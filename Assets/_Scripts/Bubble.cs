using System;
using UnityEngine;

public class Bubble : MonoBehaviour, IInteractable
{
    [Header("Internal References")]
    [SerializeField] private Transform model;
    
    [Header("Bubble Settings")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float playerFollowDistance = 4f;
    
    private CharacterController controller;
    private IBubbleable bubbleable;
    private Player player;
    private float health;
    
    public float Health => health;
    public float MaxHealth => maxHealth;

    public Transform BubbleTransform => model.transform;
    
    public void Spawn(IBubbleable bubbleable, Player player)
    {
        transform.position = bubbleable.GetBubbleWorldPosition(); // plus some offset off the ground maybe?
        this.bubbleable = bubbleable;
        this.bubbleable.Bubble(this); 
        this.player = player;
        
        health = maxHealth;
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (bubbleable.MoveToPlayer)
        {
            var vectorToPlayer = player.transform.position - transform.position;
            if (vectorToPlayer.magnitude > playerFollowDistance)
            {
                controller.Move(vectorToPlayer.normalized.ToXZ() * (speed * Time.deltaTime));
            }
        }
    }

    public void Damage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            PopBubble();
        }
    }

    private void PopBubble()
    {
        bubbleable.PopBubble();
    }

    #region IInteractable
    bool IInteractable.CanInteract => true;
    
    void IInteractable.Interact()
    {
        PopBubble();
    }
    #endregion IInteractable
}