using UnityEngine;

public class SphereController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private ParticleSystem dust2;
    // [SerializeField] private GameObject canvas;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        animator.SetBool("isBouncing", true);
        dust.Play();
        dust2.Play();
        // canvas.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        animator.SetBool("isBouncing", false);
        dust.Stop();
        dust2.Stop();
        // canvas.SetActive(false);
    }
}
