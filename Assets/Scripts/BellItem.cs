using UnityEngine;

/// <summary>
/// Handles the bell item that starts invisible and becomes collectible after Max reveals it.
/// Includes optional particle effects and sound for dramatic reveal.
/// </summary>
public class BellItem : MonoBehaviour, Interactable
{
    [SerializeField] private string itemName = "Bell";
    [SerializeField] private GameObject visualObject; // The visual representation (SpriteRenderer, etc.)
    
    [Header("WICHTIG: Setze dies auf TRUE")]
    [Tooltip("Wenn TRUE, wird die Glocke komplett deaktiviert und ist GARANTIERT unsichtbar")]
    [SerializeField] private bool hideCompleteGameObject = true;
    
    [Header("Reveal Effects (Optional)")]
    [SerializeField] private ParticleSystem revealParticles; // Particle effect when bell appears
    [SerializeField] private AudioClip revealSound; // Sound effect when bell appears
    [SerializeField] private float revealDuration = 2f; // How long the reveal animation takes
    
    private bool picked = false;
    private bool isRevealed = false;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && revealSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // CRITICAL: Force invisibility IMMEDIATELY in Awake (before Start)
        // This ensures the bell is invisible even if Unity Inspector has it enabled
        ForceInvisible();
    }

    private void Start()
    {
        // NUCLEAR OPTION: Just deactivate the entire GameObject if flag is set
        if (hideCompleteGameObject)
        {
            gameObject.SetActive(false);
            Debug.Log("🔒 Bell GameObject completely DEACTIVATED");
            return;
        }
        
        // Double-check invisibility in Start
        // This handles any race conditions or late initialization
        if (!isRevealed)
        {
            ForceInvisible();
        }
        
        // Check if already revealed from quest manager (shouldn't happen normally)
        if (QuestManager.Instance != null && QuestManager.Instance.bellRevealed)
        {
            Reveal();
        }
    }
    
    /// <summary>
    /// Forces the bell to be completely invisible and non-interactable.
    /// Call this in Awake AND Start to guarantee invisibility.
    /// </summary>
    private void ForceInvisible()
    {
        // Disable visual object if specified
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
        
        // Disable all sprite renderers on this object
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            
            // Also set alpha to 0 as extra safety
            Color c = spriteRenderer.color;
            c.a = 0f;
            spriteRenderer.color = c;
        }
        
        // Disable all sprite renderers on children
        var childRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var renderer in childRenderers)
        {
            renderer.enabled = false;
            Color c = renderer.color;
            c.a = 0f;
            renderer.color = c;
        }
        
        // Disable mesh renderers (3D objects)
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
        
        // Disable collider so player can't interact
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Disable all colliders on children
        var childColliders = GetComponentsInChildren<Collider2D>(true);
        foreach (var collider in childColliders)
        {
            collider.enabled = false;
        }
        
        Debug.Log("🔒 Bell is now FORCED INVISIBLE");
    }

    private void Update()
    {
        // Check if bell should be revealed
        if (!isRevealed && QuestManager.Instance != null && QuestManager.Instance.bellRevealed)
        {
            Reveal();
        }
    }

    public void Reveal()
    {
        if (isRevealed) return;
        
        isRevealed = true;
        
        // If GameObject was completely deactivated, activate it first
        if (hideCompleteGameObject && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.Log("🔓 Bell GameObject ACTIVATED");
        }
        
        // Start dramatic reveal animation
        StartCoroutine(RevealAnimation());
    }

    private System.Collections.IEnumerator RevealAnimation()
    {
        Debug.Log("🔔 Die Glocke wird enthüllt...");
        
        // If there's a ForceInvisibleOnStart component, tell it to make things visible
        var forceInvisible = GetComponent<ForceInvisibleOnStart>();
        
        // Play reveal sound
        if (audioSource != null && revealSound != null)
        {
            audioSource.PlayOneShot(revealSound);
        }
        
        // Play particles
        if (revealParticles != null)
        {
            revealParticles.Play();
        }
        
        // Gradually fade in the bell
        if (visualObject != null)
        {
            visualObject.SetActive(true);
            
            // Fade in effect (if has sprite renderer)
            var spriteRenderer = visualObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true; // Enable first
                Color c = spriteRenderer.color;
                c.a = 0f;
                spriteRenderer.color = c;
                
                float elapsed = 0f;
                while (elapsed < revealDuration)
                {
                    elapsed += Time.deltaTime;
                    c.a = Mathf.Lerp(0f, 1f, elapsed / revealDuration);
                    spriteRenderer.color = c;
                    
                    // Optional: scale effect
                    float scale = Mathf.Lerp(0.5f, 1f, elapsed / revealDuration);
                    visualObject.transform.localScale = Vector3.one * scale;
                    
                    yield return null;
                }
                
                c.a = 1f;
                spriteRenderer.color = c;
                visualObject.transform.localScale = Vector3.one;
            }
            else
            {
                // No sprite renderer on visual object, just enable it
                if (forceInvisible != null)
                {
                    forceInvisible.MakeVisible();
                }
            }
        }
        else
        {
            // No visual object, fade in this GameObject
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                
                // Fade in
                Color c = spriteRenderer.color;
                c.a = 0f;
                spriteRenderer.color = c;
                
                float elapsed = 0f;
                while (elapsed < revealDuration)
                {
                    elapsed += Time.deltaTime;
                    c.a = Mathf.Lerp(0f, 1f, elapsed / revealDuration);
                    spriteRenderer.color = c;
                    
                    // Scale effect
                    float scale = Mathf.Lerp(0.5f, 1f, elapsed / revealDuration);
                    transform.localScale = Vector3.one * scale;
                    
                    yield return null;
                }
                
                c.a = 1f;
                spriteRenderer.color = c;
                transform.localScale = Vector3.one;
            }
            else if (forceInvisible != null)
            {
                // Use ForceInvisibleOnStart to make visible
                forceInvisible.MakeVisible();
            }
        }
        
        // Enable collider so player can interact
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }
        
        Debug.Log("🔔 Die Glocke ist jetzt sichtbar und kann aufgehoben werden!");
    }

    private void SetVisibility(bool visible)
    {
        // Set visual object visibility
        if (visualObject != null)
        {
            visualObject.SetActive(visible);
        }
        else
        {
            // If no visual object specified, toggle this entire GameObject's renderer
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = visible;
            }
            
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = visible;
            }
        }
        
        // Also enable/disable collider so player can't interact when invisible
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = visible;
        }
    }

    public void Interact()
    {
        // Can only pick up if revealed
        if (!isRevealed)
        {
            Debug.Log("⚠️ Die Glocke ist noch nicht sichtbar!");
            return;
        }
        
        if (picked) return;
        
        picked = true;

        // Add to inventory
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddItem(itemName);
        }

        // Mark in quest manager
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CollectBell();
        }

        // Hide the bell
        SetVisibility(false);
        
        // Disable interaction
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Debug.Log("🔔 " + itemName + " ins Inventar gelegt! Quest abgeschlossen!");
    }
}
