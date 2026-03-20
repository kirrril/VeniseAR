using UnityEngine;

public class ShowerController : MonoBehaviour
{
    [SerializeField] private ParticleSystem drops;
    [SerializeField] private ParticleSystem bursts;
    [SerializeField] private ParticleSystem drying;
    bool alreadyTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (alreadyTriggered) return;

        alreadyTriggered = true;
        drops.Play();
        bursts.Play();
        drying.Play();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (alreadyTriggered) return;

        alreadyTriggered = true;
        drops.Play();
        bursts.Play();
        drying.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        alreadyTriggered = false;
        drops.Stop();
        bursts.Stop();
        drying.Stop();
    }
}