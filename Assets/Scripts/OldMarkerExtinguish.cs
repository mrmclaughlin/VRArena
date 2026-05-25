using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OldMarkerExtinguish : MonoBehaviour
{
 
    [Header("Extinguish")]
    [SerializeField] private float radius = 0.8f;
    [SerializeField] private bool extinguishOnce = true;
    [SerializeField] private float cooldownSeconds = 0.25f;

    [Header("Prefab Swap (instead of extinguish)")]
    [Tooltip("If assigned, swap to this prefab when player approaches instead of extinguishing.")]
    public GameObject swapPrefab;
    
    [Tooltip("Destroy the original object after swapping.")]
    public bool destroyOriginalOnSwap = true;

    [Header("Optional references")]
    public ParticleSystem[] flames;
    public Light[] lightsToToggle;

    private Transform hmd;
    private bool _extinguished;
    private float _nextAllowedTime;
    private GameObject _swappedObject;

    void Start()
    {
        // Auto-find camera at runtime
        if (Camera.main != null)
            hmd = Camera.main.transform;
        else
            Debug.LogWarning("HmdOldMarkerExtinguish: No Camera.main found.");
    }

    void Awake()
    {
        if (flames == null || flames.Length == 0)
            flames = GetComponentsInChildren<ParticleSystem>(true);
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        if (hmd == null) return;
        if (extinguishOnce && _extinguished) return;
        if (Time.time < _nextAllowedTime) return;

        float d = Vector3.Distance(hmd.position, transform.position);
        if (d <= radius)
        {
            Extinguish();
            _nextAllowedTime = Time.time + cooldownSeconds;
        }
    }

    public void Extinguish()
    {
        if (extinguishOnce && _extinguished) return;
        _extinguished = true;

        // If swap prefab is assigned, use that instead of extinguishing
        if (swapPrefab != null)
        {
            SwapToPrefab();
            return;
        }

        // Original extinguish behavior
        foreach (var ps in flames)
        {
            if (!ps) continue;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            ps.Clear(true);
        }

        foreach (var l in lightsToToggle)
        {
            if (l) l.enabled = false;
        }
    }

    private void SwapToPrefab()
    {
        if (swapPrefab == null) return;

        Transform parent = transform.parent;
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        Vector3 scale = transform.localScale;

        // Instantiate the swap prefab
        _swappedObject = Instantiate(swapPrefab, pos, rot, parent);
        _swappedObject.transform.localScale = scale;
        _swappedObject.name = transform.name.Replace("Lit", "Unlit").Replace("Candle", "Marker");

        // Destroy original if requested
        if (destroyOriginalOnSwap)
        {
            Destroy(gameObject);
        }
        else
        {
            // Just hide/disable the original
            gameObject.SetActive(false);
        }

        Debug.Log($"Swapped marker prefab at {pos}");
    }

    public void Relight()
    {
        _extinguished = false;

        foreach (var ps in flames)
        {
            if (!ps) continue;
            ps.Clear(true);
            ps.Play(true);
        }

        foreach (var l in lightsToToggle)
        {
            if (l) l.enabled = true;
        }
    }
}

