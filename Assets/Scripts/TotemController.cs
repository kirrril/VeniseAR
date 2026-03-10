using UnityEngine;

public class TotemController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    void Update()
    {
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);

        if (distance < 2.5f)
        {
            animator.SetBool("isTwisting", true);
        }
        else
        {
            animator.SetBool("isTwisting", false);
        }
    }
}