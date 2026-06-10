using UnityEngine;

/// <summary>
/// Attached to each solution-path marker.
///
/// Inbound  phase: HMD enters radius → marker turns BLUE  (walked-past colour)
/// Outbound phase: HMD enters radius → marker turns WHITE (restored colour)
///
/// No prefab swapping. No SetActive trickery. Just a colour change on the renderer.
/// The GameObject is always active so Update() always runs.
/// </summary>
public class OldMarkerExtinguish : MonoBehaviour
{
    [Header("HMD trigger")]
    public Transform hmd;
    public float extinguishRadius = 0.8f;
    public float cooldownSeconds  = 0.25f;
    public bool  extinguishOnce   = true;

    [Header("Colours")]
    public Color inboundColor  = Color.white;
    public Color outboundColor = new Color(0.2f, 0.4f, 1f); // blue

    // ------------------------------------------------------------------ //
    //  Private state
    // ------------------------------------------------------------------ //

    private float            _nextCheckTime = 0f;
    private bool             _hasSwapped    = false;
    private MazeJourneyPhase _phase         = MazeJourneyPhase.Inbound;
    private Renderer[]       _renderers;

    // ------------------------------------------------------------------ //
    //  Unity lifecycle
    // ------------------------------------------------------------------ //

    void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        SetColour(inboundColor);
    }

    // ------------------------------------------------------------------ //
    //  Public API called by GridMazeHedgeBuilder
    // ------------------------------------------------------------------ //

    public void SetSwapPrefab(GameObject prefab)
    {
        // No longer used — kept so the builder compiles without changes.
    }

    /// <summary>Explicitly set white regardless of prefab's baked material state.</summary>
    public void ForceInboundColour()
    {
        _hasSwapped = false;
        _phase      = MazeJourneyPhase.Inbound;
        SetColour(inboundColor);
    }

    /// <summary>Called every Build() to set swap direction.</summary>
    public void SetPhase(MazeJourneyPhase phase)
    {
        _phase = phase;
        if (phase == MazeJourneyPhase.Outbound)
            _hasSwapped = false; // re-arm for outbound pass
    }

    /// <summary>
    /// Called by SpawnSolutionMarkersAsReturn() — marker starts blue (already walked inbound).
    /// Arm for outbound so walking past turns it white.
    /// </summary>
    public void InitAsAlreadySwapped(GameObject ignored, MazeJourneyPhase phase)
    {
        _phase      = phase;
        _hasSwapped = false;        // armed for outbound pass
        SetColour(outboundColor);   // start blue — player hasn't walked back past yet
    }

    /// <summary>Full reset at loop start — restore white.</summary>
    public void Relight()
    {
        _hasSwapped = false;
        _phase      = MazeJourneyPhase.Inbound;
        SetColour(inboundColor);
    }

    // ------------------------------------------------------------------ //
    //  Update
    // ------------------------------------------------------------------ //

    void Update()
    {
        if (_hasSwapped && extinguishOnce) return;
        if (hmd == null)
        {
            if (Camera.main != null) hmd = Camera.main.transform;
            return;
        }
        if (Time.time < _nextCheckTime) return;

        _nextCheckTime = Time.time + cooldownSeconds;

        if (Vector3.Distance(hmd.position, transform.position) > extinguishRadius) return;

        if (_phase == MazeJourneyPhase.Inbound)
        {
            _hasSwapped = true;
            SetColour(outboundColor); // turn blue
        }
        else
        {
            _hasSwapped = true;
            SetColour(inboundColor);  // turn white
        }
    }

    // ------------------------------------------------------------------ //
    //  Colour helper
    // ------------------------------------------------------------------ //

    void SetColour(Color c)
    {
        if (_renderers == null)
            _renderers = GetComponentsInChildren<Renderer>(true);

        foreach (var r in _renderers)
        {
            // Works for standard, URP, and HDRP lit shaders
            if (r.material.HasProperty("_Color"))
                r.material.color = c;
            else if (r.material.HasProperty("_BaseColor"))
                r.material.SetColor("_BaseColor", c);
        }
    }

    // ------------------------------------------------------------------ //
    //  Gizmos
    // ------------------------------------------------------------------ //

    void OnDrawGizmosSelected()
    {
        Gizmos.color = (_phase == MazeJourneyPhase.Inbound) ? Color.white : Color.blue;
        Gizmos.DrawWireSphere(transform.position, extinguishRadius);
    }
}