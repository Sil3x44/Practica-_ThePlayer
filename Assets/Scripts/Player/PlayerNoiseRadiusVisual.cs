using UnityEngine;

public class PlayerNoiseRadiusVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerNoiseEmitter playerNoiseEmitter;
    [SerializeField] private Transform noiseRadiusRoot;
    [SerializeField] private ParticleSystem noiseParticleSystem;

    [Header("Scale")]
    [SerializeField] private float baseDiameterAtScaleOne;

    private void Start()
    {
        RefreshVisual();
    }

    private void Update()
    {
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        float noiseRadius = playerNoiseEmitter.GetCurrentNoiseRadius();
        bool shouldShow = playerMovement.GetIsMoving() && noiseRadius > 0f;

        if (!shouldShow)
        {
            if (noiseParticleSystem.isPlaying)
            {
                noiseParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            return;
        }

        float targetDiameter = noiseRadius * 2f;
        float scaleFactor = targetDiameter / baseDiameterAtScaleOne;

        noiseRadiusRoot.localScale = new Vector3(scaleFactor, 1f, scaleFactor);

        if (!noiseParticleSystem.isPlaying)
        {
            noiseParticleSystem.Play();
        }
    }
}
