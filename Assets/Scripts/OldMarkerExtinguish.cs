using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OldMarkerExtinguish : MonoBehaviour
{
 
    [Header("Extinguish")]
    [SerializeField] private float radius = 0.8f;
    [SerializeField] private bool extinguishOnce = true;
    [SerializeField] private float cooldownSeconds = 0.25f;

    [Header("Optional references")]
    public ParticleSystem[] flames;
    public Light[] lightsToToggle;

    private Transform hmd;
    private bool _extinguished;
    private float _nextAllowedTime;

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

