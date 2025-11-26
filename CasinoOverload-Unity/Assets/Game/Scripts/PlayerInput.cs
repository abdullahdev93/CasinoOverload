using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Input Settings")]
    public bool AllowInput = true;

    public Vector2 MoveInput { get => _moveInput; }     // Horizontal + Vertical movement
    public Vector2 LookInput { get => _lookInput; }     // Mouse delta


    Vector2 _moveInput;
    Vector2 _lookInput;

    private void Start() {
        //Cursor.visible = false;
    }

    void Update() {
        if (!AllowInput) {
            _moveInput = Vector2.zero;
            _lookInput = Vector2.zero;
            return;
        }

        // MOVEMENT
        _moveInput.x = ControlFreak2.CF2Input.GetAxis("Horizontal");
        _moveInput.y = ControlFreak2.CF2Input.GetAxis("Vertical");

        // MOUSE DELTA (works even with locked cursor!)
        _lookInput.x = ControlFreak2.CF2Input.GetAxisRaw("Mouse X");
        _lookInput.y = ControlFreak2.CF2Input.GetAxisRaw("Mouse Y");
    }
}
