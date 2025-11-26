using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] float speed;
    [SerializeField] float accelration;
    [SerializeField] float rotateSpeed = 10f;   // adjust in inspector

    [Header("References")]
    [SerializeField] PlayerInput playerInput;
    [SerializeField] Transform povit;

    private CharacterController cc;

    private Vector3 velocity;

    private void Awake() {
        cc = GetComponent<CharacterController>();

        if (!povit)
            povit = transform;
    }
    public override void FixedUpdateNetwork() {
        HandleMovement();
        ApplyVelocity();

        RotateToPovit();
    }

    void RotateToPovit() {
        if (!playerInput || playerInput.MoveInput.magnitude < .1f) return;

        Vector2 inputVector = playerInput.MoveInput;

        Vector3 inputDirection = Vector3.zero;
        inputDirection += inputVector.x * povit.right;
        inputDirection += inputVector.y * povit.forward;

        inputDirection.y = 0;
        inputDirection.Normalize();

        // compute target rotation
        Quaternion targetRot = Quaternion.LookRotation(inputDirection);

        // smooth rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotateSpeed
        );
    }

    void HandleMovement() {
        if (!playerInput) return;

        Vector2 inputVector = playerInput.MoveInput;

        Vector3 inputDirection = Vector3.zero;
        inputDirection += inputVector.x * povit.right;
        inputDirection += inputVector.y * povit.forward;

        inputDirection.y = 0;

        Vector3 targetVelocity = speed * inputDirection.normalized;
        Vector3 diffVelocity = targetVelocity - velocity;

        velocity += diffVelocity * Time.fixedDeltaTime * accelration;
    }

    void ApplyVelocity() {
        cc.SimpleMove(velocity);
    }
}
