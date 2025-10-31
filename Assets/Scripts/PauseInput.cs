using UnityEngine;
using UnityEngine.InputSystem;

public class PauseInput : MonoBehaviour
{
    private InputSystem_Actions input;
    private PopupManager popupManager;

    private void Awake()
    {
        input = new InputSystem_Actions();
    }

    private void Start()
    {
        popupManager = FindFirstObjectByType<PopupManager>();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Pause.performed += OnPausePressed;
    }

    private void OnDisable()
    {
        input.Player.Pause.performed -= OnPausePressed;
        input.Disable();
    }

    private void OnPausePressed(InputAction.CallbackContext context)
    {
        if (popupManager != null)
        {
            popupManager.TogglePause();
        }
    }
}