using UnityEngine;

public class FireBossController : MonoBehaviour
{
    Animator Anim;
    void Start()
    {
Anim = GetComponent<Animator>();
    }

    void Update()
    {
if (Input.GetKeyDown(KeyCode.Space))
{
    Anim.SetInteger("Status", 1);
}
    }
}
