using System.Collections;
using UnityEngine;

/// <summary>
/// Apply forward force to instantiated prefab
/// </summary>
public class LaunchProjectile : MonoBehaviour
{
    [Tooltip("The projectile that's created")]
    public GameObject projectilePrefab = null;
    [Tooltip("The point that the project is created")]
    public Transform startPoint = null;
    [Tooltip("The speed at which the projectile is launched")]
    public float launchSpeed = 1.0f;
    [Tooltip("Time between each auto fire shot")]
    public float fireRate = 0.5f;

    private Coroutine autoFireCoroutine;

    public void Fire()
    {
        GameObject newObject = Instantiate(projectilePrefab, startPoint.position, startPoint.rotation);
        if (newObject.TryGetComponent(out Rigidbody rigidBody))
            ApplyForce(rigidBody);
    }

    public void StartAutoFire()
    {
        autoFireCoroutine = StartCoroutine(AutoFire());
    }

    public void StopAutoFire()
    {
        if (autoFireCoroutine != null)
            StopCoroutine(autoFireCoroutine);
    }

    private IEnumerator AutoFire()
    {
        while (true)
        {
            Fire();
            yield return new WaitForSeconds(fireRate);
        }
    }

    private void ApplyForce(Rigidbody rigidBody)
    {
        Vector3 force = startPoint.forward * launchSpeed;
        rigidBody.AddForce(force);
    }
}