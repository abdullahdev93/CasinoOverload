using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float sensativity = 100;
    [SerializeField] bool flipX, flipY;

    [Header("References")]
    [SerializeField] PlayerInput playerInput;
    [SerializeField] CinemachineOrbitalFollow cinemachine;

    float horizontalValue = 0;
    float verticalValue = 0;


    private void Start() {

        transform.parent = null;
    }

    private void Update() {
        if (!playerInput) return;

        // Use PlayerInput instead of Input.mousePositionDelta
        horizontalValue += playerInput.LookInput.x * sensativity * Time.deltaTime * (flipX ? -1 : 1);
        verticalValue += playerInput.LookInput.y * sensativity * Time.deltaTime * (flipY ? -1 : 1);

        verticalValue = Mathf.Clamp(verticalValue, cinemachine.VerticalAxis.Range.x, cinemachine.VerticalAxis.Range.y);

        cinemachine.HorizontalAxis.Value = horizontalValue;
        cinemachine.VerticalAxis.Value = verticalValue;
    }
}
