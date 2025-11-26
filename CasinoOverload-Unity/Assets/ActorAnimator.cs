using UnityEngine;


[RequireComponent(typeof(Animator))]
public class ActorAnimator : MonoBehaviour
{
    [SerializeField] PlayerInput input;

    private Animator animator;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    void LateUpdate()
    {
        if (!input) {

            animator.SetBool("IsWalking", false);

            return;
        }

        animator.SetBool("IsWalking", input.MoveInput.magnitude > .1f);
    }
}
