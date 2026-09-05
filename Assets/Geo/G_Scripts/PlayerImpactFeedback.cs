using UnityEngine;
using Unity.Cinemachine;

public class PlayerImpactFeedback : MonoBehaviour
{
    public enum ImpactType
    {
        Wall,
        Ground,
        Object
    }

    [Header("Cinemachine")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Header("Impact Detection")]
    [SerializeField] private float minimumImpactSpeed = 1f;
    [SerializeField] private float maximumImpactSpeed = 20f;

    [Header("Wall Shake")]
    [SerializeField] private float wallShakeMultiplier = 0.7f;

    [Header("Ground Shake")]
    [SerializeField] private float groundShakeMultiplier = 1f;

    [Header("Object Shake")]
    [SerializeField] private float objectShakeMultiplier = 1.25f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem wallImpactParticles;
    [SerializeField] private ParticleSystem groundImpactParticles;
    [SerializeField] private ParticleSystem objectImpactParticles;

    [Header("Particle Scaling")]
    [SerializeField] private float minimumParticleScale = 0.5f;
    [SerializeField] private float maximumParticleScale = 2f;

    [Header("Tags")]
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private string objectTag = "Bouncy";

    public void PlayImpact(
        Collision collision,
        Vector3 contactPoint,
        Vector3 contactNormal,
        float impactSpeed)
    {
        Debug.Log(
        $"IMPACT! Object: {collision.gameObject.name} | " +
        $"Tag: {collision.gameObject.tag} | " +
        $"Speed: {impactSpeed}"
    );
        ImpactType impactType = DetermineImpactType(
            collision,
            contactNormal
        );

        float intensity = Mathf.InverseLerp(
            minimumImpactSpeed,
            maximumImpactSpeed,
            impactSpeed
        );

        // Camera
        PlayCameraImpulse(
            impactType,
            intensity,
            contactNormal
        );

        // Particles
        PlayParticles(
            impactType,
            contactPoint,
            contactNormal,
            intensity
        );
    }

    // =========================================================
    // DETERMINE IMPACT TYPE
    // =========================================================

    private ImpactType DetermineImpactType(
        Collision collision,
        Vector3 contactNormal)
    {
        // Explicit bouncy/special objects first
        if (collision.collider.CompareTag(objectTag))
        {
            return ImpactType.Object;
        }

        // Ground detection
        //
        // A normal pointing mostly upward means we landed on
        // something rather than hitting a wall.
        if (contactNormal.y > 0.5f)
        {
            return ImpactType.Ground;
        }

        // Everything else is treated as a wall.
        return ImpactType.Wall;
    }

    // =========================================================
    // CINEMACHINE
    // =========================================================

    private void PlayCameraImpulse(ImpactType impactType, float intensity, Vector3 contactNormal)
    {
        if (impulseSource == null)
            return;

        float multiplier = GetShakeMultiplier(
            impactType
        );

        float strength = intensity * multiplier;

        if (strength <= 0f)
            return;
        // Push the camera in the opposite direction of the
        // collision surface.
        Vector3 impulseDirection = -contactNormal;

        impulseSource.GenerateImpulse(
            impulseDirection * strength
        );
    }

    private float GetShakeMultiplier(
        ImpactType impactType)
    {
        switch (impactType)
        {
            case ImpactType.Wall:
                return wallShakeMultiplier;

            case ImpactType.Ground:
                return groundShakeMultiplier;

            case ImpactType.Object:
                return objectShakeMultiplier;
        }

        return 1f;
    }

    // =========================================================
    // PARTICLES
    // =========================================================

    private void PlayParticles(
        ImpactType impactType,
        Vector3 contactPoint,
        Vector3 contactNormal,
        float intensity)
    {
        ParticleSystem particles = GetParticleSystem(
            impactType
        );

        if (particles == null)
            return;

        particles.transform.position = contactPoint;

        // Rotate particles so their emission faces away
        // from the surface.
        particles.transform.rotation =
            Quaternion.LookRotation(contactNormal);

        float particleScale = Mathf.Lerp(
            minimumParticleScale,
            maximumParticleScale,
            intensity
        );

        particles.transform.localScale =
            Vector3.one * particleScale;

        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particles.Play();
    }

    private ParticleSystem GetParticleSystem(
        ImpactType impactType)
    {
        switch (impactType)
        {
            case ImpactType.Wall:
                return wallImpactParticles;

            case ImpactType.Ground:
                return groundImpactParticles;

            case ImpactType.Object:
                return objectImpactParticles;
        }
        return null;
    }
}