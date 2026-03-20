using UnityEngine;

public class SphereController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private ParticleSystem dust2;
    bool alreadyTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (alreadyTriggered) return;

        alreadyTriggered = true;
        animator.SetBool("isBouncing", true);
        dust.Play();
        dust2.Play();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (alreadyTriggered) return;

        alreadyTriggered = true;
        animator.SetBool("isBouncing", true);
        dust.Play();
        dust2.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        alreadyTriggered = false;
        animator.SetBool("isBouncing", false);
        dust.Stop();
        dust2.Stop();
    }
}
