using UnityEngine;

/// <summary>
/// TargetShell - Wraps student project prefabs in a breakable target shell.
/// When hit by a dart (tagged "Dart"), triggers explosion effect and destroys the shell.
/// Attach this to a parent GameObject that contains the student project as a child.
/// </summary>
public class TargetShell : MonoBehaviour
{
    [Header("Explosion Settings")]
    [Tooltip("Particle effect to spawn on destruction")]
    public GameObject explosionPrefab;
    
    [Tooltip("Sound to play on explosion")]
    public AudioClip explosionSound;
    
    [Tooltip("Minimum impact force to trigger explosion")]
    public float minImpactForce = 0.5f;
    
    [Header("Optional")]
    [Tooltip("Spawn explosion at impact point instead of center")]
    public bool spawnAtImpactPoint = true;
    
    [Tooltip("Destroy shell after explosion")]
    public bool destroyOnExplosion = true;
    
    [Tooltip("Delay before destruction (seconds)")]
    public float destroyDelay = 0.5f;

    [Header("Spin Settings")]
    [Tooltip("Enable continuous spinning")]
    public bool enableSpin = true;

    [Tooltip("Rotation speed in degrees per second on each axis")]
    public Vector3 spinSpeed = new Vector3(45f, 90f, 30f);

    [Tooltip("Randomize spin speeds on Start")]
    public bool randomizeSpin = false;

    [Tooltip("Min random speed (degrees/sec) per axis")]
    public float randomSpeedMin = 20f;

    [Tooltip("Max random speed (degrees/sec) per axis")]
    public float randomSpeedMax = 120f;

    private AudioSource audioSource;
    private bool hasExploded = false;

    void Awake()
    {
        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();

        if (explosionSound != null && GetComponent<AudioSource>() == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f;
        }
    }

    void Start()
    {
        if (randomizeSpin)
        {
            spinSpeed = new Vector3(
                Random.Range(randomSpeedMin, randomSpeedMax),
                Random.Range(randomSpeedMin, randomSpeedMax),
                Random.Range(randomSpeedMin, randomSpeedMax)
            );
        }
    }

    void Update()
    {
        if (enableSpin && !hasExploded)
        {
            transform.Rotate(spinSpeed * Time.deltaTime, Space.Self);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        if (collision.gameObject.CompareTag("Dart"))
        {
            float impactForce = collision.relativeVelocity.magnitude;
            
            if (impactForce >= minImpactForce)
            {
                Vector3 explosionPoint = spawnAtImpactPoint && collision.contacts.Length > 0 
                    ? collision.contacts[0].point 
                    : transform.position;
                
                TriggerExplosion(explosionPoint);
            }
        }
    }

    public void TriggerExplosion(Vector3 position)
    {
        if (hasExploded) return;
        hasExploded = true;

        if (explosionPrefab != null)
        {
            GameObject particles = Instantiate(explosionPrefab, position, Quaternion.identity);
            ParticleSystem ps = particles.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(particles, ps.main.duration + ps.main.startLifetime.constant);
            else
                Destroy(particles, 3f);
        }

        if (explosionSound != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(explosionSound);
            else
                AudioSource.PlayClipAtPoint(explosionSound, position);
        }

        if (destroyOnExplosion)
            Destroy(gameObject, destroyDelay);
    }

    public void TriggerExplosion()
    {
        TriggerExplosion(transform.position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
    }
}