using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Input Settings")]
    public bool AllowInput = true;

    public Vector2 MoveInput { get => _moveInput; }     // Horizontal + Vertical movement
    public Vector2 LookInput { get => _lookInput; }     // Mouse delta


    Vector2 _moveInput;
    Vector2 _lookInput;


    void Update() {
        if (!AllowInput) {
            _moveInput = Vector2.zero;
            _lookInput = Vector2.zero;
            return;
        }

        // Movement (WASD)
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        // Mouse Delta (Unity 2023+)
        _lookInput.x = Input.mousePositionDelta.x;
        _lookInput.y = Input.mousePositionDelta.y;
    }
}
