using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StudentProjectSpawner - Spawns student project prefabs wrapped in a target shell
/// at scene load, each with a torch above it. Cleans up on scene unload.
/// Drag student project prefabs directly into the studentProjects array in the Inspector.
/// </summary>
public class StudentProjectSpawner : MonoBehaviour
{
    [Header("Asset References")]
    [Tooltip("The Target prefab found at Assets/Student Projects/")]
    public GameObject targetPrefab;

    [Tooltip("The Torch prefab found at Assets/Student Projects/")]
    public GameObject torchPrefab;

    [Tooltip("Drag student project prefabs here from Assets/Student Projects/projects")]
    public GameObject[] studentProjects;

    [Header("Project Scale")]
    [Tooltip("Uniform scale applied to each spawned student project")]
    public float projectScale = 1f;

    [Tooltip("Override uniform scale with per-axis control")]
    public bool useNonUniformScale = false;

    [Tooltip("Per-axis scale applied when Use Non Uniform Scale is enabled")]
    public Vector3 projectScaleAxes = Vector3.one;

    [Header("Spawn Settings")]
    [Tooltip("Center point of the spawn area")]
    public Vector3 spawnAreaCenter = Vector3.zero;

    [Tooltip("Half-extents of the random spawn area (X and Z)")]
    public Vector2 spawnAreaSize = new Vector2(20f, 20f);

    [Tooltip("Y position to place targets at")]
    public float spawnHeight = 0f;

    [Tooltip("Height above the target to place the torch")]
    public float torchHeightOffset = 2.5f;

    [Tooltip("Minimum distance between spawned targets")]
    public float minSpacing = 3f;

    [Tooltip("Maximum attempts to find a valid position before giving up")]
    public int maxPlacementAttempts = 30;

    // Tracked separately so torches are NOT children of spinning targets
    private readonly List<GameObject> spawnedTargets = new List<GameObject>();
    private readonly List<GameObject> spawnedTorches = new List<GameObject>();

    void Awake()
    {
        SpawnTargets();
    }

    void OnDestroy()
    {
        ClearTargets();
    }

    private void SpawnTargets()
    {
        if (targetPrefab == null)
        {
            Debug.LogError("[StudentProjectSpawner] Target prefab is not assigned.");
            return;
        }
        if (torchPrefab == null)
        {
            Debug.LogError("[StudentProjectSpawner] Torch prefab is not assigned.");
            return;
        }
        if (studentProjects == null || studentProjects.Length == 0)
        {
            Debug.LogError("[StudentProjectSpawner] No student projects assigned. " +
                           "Drag prefabs into the Student Projects array in the Inspector.");
            return;
        }

        Debug.Log($"[StudentProjectSpawner] Found {studentProjects.Length} student project(s) to spawn.");

        List<Vector3> usedPositions = new List<Vector3>();

        foreach (GameObject projectPrefab in studentProjects)
        {
            if (projectPrefab == null)
            {
                Debug.LogWarning("[StudentProjectSpawner] Null entry in studentProjects array, skipping.");
                continue;
            }

            Vector3 spawnPos = FindValidPosition(usedPositions);
            usedPositions.Add(spawnPos);

            // Instantiate the target shell
            GameObject target = Instantiate(targetPrefab, spawnPos,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            target.name = $"Target_{projectPrefab.name}";
            spawnedTargets.Add(target);

            // Attach the student project as a child of the target
            GameObject project = Instantiate(projectPrefab);
            project.transform.SetParent(target.transform);
            project.transform.localPosition = Vector3.zero;
            project.transform.localRotation = Quaternion.identity;

            // Apply scale
            project.transform.localScale = useNonUniformScale
                ? projectScaleAxes
                : Vector3.one * projectScale;

            project.name = projectPrefab.name;

            // Torch sits at scene root so it does not spin with the target
            GameObject torch = Instantiate(torchPrefab,
                spawnPos + Vector3.up * torchHeightOffset, Quaternion.identity);
            torch.name = $"Torch_{projectPrefab.name}";
            spawnedTorches.Add(torch);
        }

        Debug.Log($"[StudentProjectSpawner] Spawned {spawnedTargets.Count} project target(s).");
    }

    private void ClearTargets()
    {
        foreach (GameObject target in spawnedTargets)
            if (target != null) Destroy(target);

        foreach (GameObject torch in spawnedTorches)
            if (torch != null) Destroy(torch);

        spawnedTargets.Clear();
        spawnedTorches.Clear();
        Debug.Log("[StudentProjectSpawner] Cleared all spawned targets and torches.");
    }

    private Vector3 FindValidPosition(List<Vector3> usedPositions)
    {
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            float x = spawnAreaCenter.x + Random.Range(-spawnAreaSize.x, spawnAreaSize.x);
            float z = spawnAreaCenter.z + Random.Range(-spawnAreaSize.y, spawnAreaSize.y);
            Vector3 candidate = new Vector3(x, spawnHeight, z);

            bool tooClose = false;
            foreach (Vector3 used in usedPositions)
            {
                if (Vector3.Distance(candidate, used) < minSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose) return candidate;
        }

        float fx = spawnAreaCenter.x + Random.Range(-spawnAreaSize.x, spawnAreaSize.x);
        float fz = spawnAreaCenter.z + Random.Range(-spawnAreaSize.y, spawnAreaSize.y);
        return new Vector3(fx, spawnHeight, fz);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Gizmos.DrawCube(
            new Vector3(spawnAreaCenter.x, spawnHeight, spawnAreaCenter.z),
            new Vector3(spawnAreaSize.x * 2f, 0.1f, spawnAreaSize.y * 2f)
        );
        Gizmos.color = new Color(0f, 1f, 0.5f, 1f);
        Gizmos.DrawWireCube(
            new Vector3(spawnAreaCenter.x, spawnHeight, spawnAreaCenter.z),
            new Vector3(spawnAreaSize.x * 2f, 0.1f, spawnAreaSize.y * 2f)
        );
    }
}