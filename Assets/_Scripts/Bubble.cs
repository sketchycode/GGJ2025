using System;
using UnityEngine;

public class Bubble : MonoBehaviour, IInteractable, IDamageable
{
    [Header("Internal References")]
    [SerializeField] private Transform model;
    [SerializeField] private Transform captureAttachTransform;
    
    [Header("Bubble Settings")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float playerFollowDistance = 4f;
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip popBubbleClip;
    
    private CharacterController controller;
    private IBubbleable bubbleable;
    private Player player;
    private float health;
    
    public float Health => health;
    public float MaxHealth => maxHealth;

    public Transform BubbleTransform => captureAttachTransform;
    
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
        if (player != null)
        {
            var vectorToPlayer = (player.transform.position - transform.position).ToXZ();
            var distanceToPlayer = vectorToPlayer.magnitude;
            if (distanceToPlayer > playerFollowDistance)
            {
                var moveMagnitude = speed * Time.deltaTime;
                var moveNormal = vectorToPlayer.normalized;
                if (distanceToPlayer - moveMagnitude < playerFollowDistance)
                {
                    controller.Move(moveNormal * (distanceToPlayer - playerFollowDistance));
                }
                else
                {
                    controller.Move(moveNormal * moveMagnitude);
                }
            }
        }
        bubbleable.TakeDamage(damagePerSecond * Time.deltaTime);
    }

    private void PopBubble()
    {
        model.GetComponent<Collider>().enabled = false;
        bubbleable.PopBubble();
        audioSource.PlayOneShot(popBubbleClip);
        Destroy(gameObject);
    }

    #region IInteractable
    bool IInteractable.CanInteract => true;
    
    void IInteractable.Interact()
    {
        PopBubble();
    }
    #endregion IInteractable

    #region IDamageable
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            PopBubble();
        }
    }
    #endregion IDamageable
}