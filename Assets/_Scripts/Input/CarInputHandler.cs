using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionReference forward;
    [SerializeField] private InputActionReference turn;
    [SerializeField] private InputActionReference brake;
    [SerializeField] private InputActionReference jump;
    [SerializeField] private InputActionReference boost;

    public event Action<CarInputHandler> OnBrake;
    public event Action<CarInputHandler> OnBoost;
    public float Acceleration => forward.action.ReadValue<float>();
    public float Turning => turn.action.ReadValue<float>();
    public bool IsBraking => brake.action.ReadValue<float>() > 0;
    public bool IsJumping => jump.action.ReadValue<float>() > 0;
    

    private void OnEnable()
    {
        InputActionSwitch(true);
    }
    private void OnDisable()
    {
        InputActionSwitch(false);
    }

    public void InputActionSwitch(bool enabling)
    {
        if (enabling)
        {
            forward.action.Enable();
            turn.action.Enable();
            brake.action.Enable();
            jump.action.Enable();
            boost.action.Enable();

            boost.action.started += Boost;
        }
        else if (!enabling)
        {
            forward.action.Disable();
            turn.action.Disable();
            brake.action.Disable();
            jump.action.Disable();
            boost.action.Disable();

            brake.action.started -= Boost;
        }
    }

    private void Boost(InputAction.CallbackContext context)
    {
        OnBoost?.Invoke(this);
    }

}
