using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileInputManager : MonoBehaviour
{
    [SerializeField] GameObject mobileController = null;
    [SerializeField] GameObject joystickCenter = null;
    [SerializeField] RectTransform mobileJoystick = null;
    [SerializeField] bool useMobileController = false;
    [Range(0.0f, 1.0f)][SerializeField] float minJoystickPush = 0.5f;

    private Player player;

    private bool isJumpAndDashPressed = false;
    private bool isGrabWallPressed = false;

    private float joystickSize = 1.0f;

    private void Start() 
    {
        player = FindObjectOfType<Player>();
        SetInputToMobileOrNot();

        Vector3[] v = new Vector3[4];
        mobileJoystick.GetWorldCorners(v);
        joystickSize = Mathf.Abs(v[0].x - mobileJoystick.position.x);
    }

    private void Update()
    {
        if (!useMobileController)
        {
            return;
        }

        Touch[] touches = Input.touches;
        foreach (Touch touch in touches)
        {
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
            Vector2 joystickDirection = touchPosition - mobileJoystick.position;

            if(joystickDirection.magnitude < joystickSize &&
                joystickDirection.magnitude > 0.1 * joystickSize)
            {
                joystickCenter.transform.position = touchPosition;
                float joystickAngle = Vector2.Angle(joystickDirection, Vector2.right);

                if (joystickDirection.y >= 0)
                {
                    if (joystickAngle < 22.5f)
                    {
                        Debug.Log("Right");
                        SetVerticalAxisInput(0.0f);
                        SetHorizontalAxisInput(1.0f);
                    }      
                    else if (joystickAngle < 45f + 22.5f)
                    {
                        Debug.Log("Up Right");
                        SetVerticalAxisInput(1.0f);
                        SetHorizontalAxisInput(1.0f);
                    }  
                    else if (joystickAngle < 2*45f + 22.5f)
                    {
                        Debug.Log("Up");
                        SetVerticalAxisInput(1.0f);
                        SetHorizontalAxisInput(0.0f);
                    } 
                    else if (joystickAngle < 3*45f + 22.5f)
                    {
                        Debug.Log("Up Left");
                        SetVerticalAxisInput(1.0f);
                        SetHorizontalAxisInput(-1.0f);
                    }    
                    else if (joystickAngle < 180f)     
                    {
                        Debug.Log("Left");
                        SetVerticalAxisInput(0.0f);
                        SetHorizontalAxisInput(-1.0f);
                    }     
                }
                else 
                {
                    if (joystickAngle < 22.5f)
                    {
                        Debug.Log("Right");
                        SetVerticalAxisInput(0.0f);
                        SetHorizontalAxisInput(1.0f);
                    }      
                    else if (joystickAngle < 45f + 22.5f)
                    {
                        Debug.Log("Down Right");
                        SetVerticalAxisInput(-1.0f);
                        SetHorizontalAxisInput(1.0f);
                    }  
                    else if (joystickAngle < 2*45f + 22.5f)
                    {
                        Debug.Log("Down");
                        SetVerticalAxisInput(-1.0f);
                        SetHorizontalAxisInput(0.0f);
                    } 
                    else if (joystickAngle < 3*45f + 22.5f)
                    {
                        Debug.Log("Down Left");
                        SetVerticalAxisInput(-1.0f);
                        SetHorizontalAxisInput(-1.0f);
                    }    
                    else if (joystickAngle <= 180f)     
                    {
                        Debug.Log("Left");
                        SetVerticalAxisInput(0.0f);
                        SetHorizontalAxisInput(-1.0f);
                    }
                }

                return;
            }
        }

        joystickCenter.transform.position = mobileJoystick.position;
        SetVerticalAxisInput(0.0f);
        SetHorizontalAxisInput(0.0f);
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
        player.SetJumpAndDashInput(false);
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
