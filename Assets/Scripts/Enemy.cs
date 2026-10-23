using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Animator animator;
    public Health health;
void Start()
    {
        // Optional: Get the Animator automatically if not assigned in Inspector
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // Detect Space key press
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage();
            health.TakeDamage(10);
        }
        if (health.currentHealth <= 0)
        {
            Death();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Death();
        }
    }

    public void TakeDamage()
    {
        // Sets the Animator parameter named "Status" to 1
        animator.SetInteger("Status", 1);
        StartCoroutine(ResetStatusRoutine());
    }
    public void Death()
    {
        // Sets the Animator parameter named "Status" to 2
        animator.SetInteger("Status", 2);
    }
    private IEnumerator ResetStatusRoutine()
    {
        // Wait a frame or short moment so the Animator registers the parameter change
        yield return null; 
        animator.SetInteger("Status", 0);
    }
}
