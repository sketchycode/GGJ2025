using UnityEngine;

public class TexturePanner : MonoBehaviour
{
    public float speed = 0.01f; // Adjust the panning speed
    public Vector2 direction = Vector2.right; // Direction of panning (e.g., Vector2.right, Vector2.up, Vector2.left)

    private Renderer rend;
    private Vector2 offset;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        // Calculate the offset based on speed, direction, and time
        offset += direction * speed * Time.deltaTime;

        // Apply the offset to the material's main texture
        rend.material.mainTextureOffset = offset;
    }
}