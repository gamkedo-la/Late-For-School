using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInputManager : MonoBehaviour
{
    [SerializeField] GameObject mobileController = null;
    [SerializeField] bool useMobileController = false;

    private Player player;

    private bool isJumpAndDashPressed = false;
    private bool isGrabWallPressed = false;

    private void Start() 
    {
        player = FindObjectOfType<Player>();
        SetInputToMobileOrNot();
    }

    private void SetInputToMobileOrNot()
    {
        if (useMobileController)
        {
            player.SetUseMobileInput(true);
            mobileController.SetActive(true);
        }
        else
        {
            player.SetUseMobileInput(false);
            mobileController.SetActive(false);
        }
    }

    public void SetVerticalAxisInput(float value) 
    {
        player.SetVerticalAxisInput(value);
    }

    public void SetHorizontalAxisInput(float value) 
    {
        player.SetHorizontalAxisInput(value);
    }
    public void PressJumpAndDashButton()
    {
        if (!isJumpAndDashPressed)
        {
            player.SetJumpAndDashInput(true);
            isJumpAndDashPressed = true;
        }
    }

    public void StopPressingJumpAndDashButton()
    {
        isJumpAndDashPressed = false;
    }

    public void PressGrabWallButton()
    {
        if (!isGrabWallPressed)
        {
            player.SetGrabWallInput(true);
            isGrabWallPressed = true;
        }
    }

    public void StopPressingGrabWallButton()
    {
        isGrabWallPressed = false;
    }

}
