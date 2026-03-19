using UnityEngine;

public class ShowerController : MonoBehaviour
{
    [SerializeField] private ParticleSystem drops;
    [SerializeField] private ParticleSystem bursts;
    [SerializeField] private ParticleSystem drying;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        drops.Play();
        bursts.Play();
        drying.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        drops.Stop();
        bursts.Stop();
        drying.Stop();
    }
}