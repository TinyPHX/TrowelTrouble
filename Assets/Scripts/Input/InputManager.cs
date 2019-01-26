using UnityEngine;
using System.Collections;

/// <summary>
/// Manages input for all device types.
/// </summary>
public class InputManager : MonoBehaviour
{
    public Vector3 moveDir = Vector3.zero;
    public Vector3 shootDir = Vector3.zero;
    public float jump = 0;
    public float specialOne = 0;
    public InputDevice lastInputDevice = 0;
    public WeaponSelection currentWeapon = 0;
    public float horizontalSwipeDirection = 0;
    public float verticalSwipeDirection = 0;

    //Variables for registering swipes
    private float swipeStartTime;
    private Vector2 swipeStartPos;
    private bool couldBeSwipe;
    [SerializeField] private float swipeComfortZone;
    [SerializeField] private float minSwipeDist;
    [SerializeField] private float maxSwipeTime;

    public enum WeaponSelection
    {
        up,
        down,
        left,
        right
    }

    public enum InputDevice
    {
        touch,
        keyboard,
        xbox360,
        ps3,
        wii,
        xboxOne,
        ps4,
        wiiU,
        steam
    };

    // Update is called once per frame
    void Update()
    {
        UpdateMovementInput();
        UpdateShootInput();
        UpdateJumpInput();
        UpdateSpecialOne();
        UpdateSwipeInput();
        UpdateWeaponSelection();
        UpdateLastInput();
    }

    /// <summary>
    /// Gets the movement input for the current frame.
    /// </summary>
    private void UpdateMovementInput()
    {
        moveDir = Vector2.zero;
        Vector2 axisChange = new Vector2(Input.GetAxis("Alt Horizontal"), Input.GetAxis("Alt Vertical"));
        
        if (axisChange != Vector2.zero)
        {
            lastInputDevice = InputDevice.keyboard;
            moveDir = axisChange;
        }
    }

    /// <summary>
    /// Gets the shooting inputs for the current frame.
    /// </summary>
    private void UpdateShootInput()
    {
        shootDir = Vector2.zero;
        Vector2 axisChange = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (axisChange != Vector2.zero)
        {
            lastInputDevice = InputDevice.keyboard;
            shootDir = axisChange;
        }
    }

    /// <summary>
    /// Gets the jump value for the current frame.
    /// </summary>
    private void UpdateJumpInput()
    {
        jump = Input.GetAxis("Jump");
    }

    /// <summary>
    /// Gets the input on the Fire3 axis for the current frame.
    /// </summary>
    private void UpdateSpecialOne()
    {
        specialOne = Input.GetAxis("Fire3");
    }

    /// <summary>
    /// Determines which weapon is selected and updates the current weapon if needed.
    /// </summary>
    private void UpdateWeaponSelection()
    {


        float horizontalWeapon = Input.GetAxis("Horizontal Weapon Selection");
        if (horizontalWeapon > 0)
        {
            lastInputDevice = InputDevice.keyboard;
            currentWeapon = WeaponSelection.left;
        }
        else if (horizontalWeapon < 0)
        {
            lastInputDevice = InputDevice.keyboard;
            currentWeapon = WeaponSelection.right;
        }

        float verticalWeapon = Input.GetAxis("Vertical Weapon Selection");
        if (verticalWeapon > 0)
        {
            lastInputDevice = InputDevice.keyboard;
            currentWeapon = WeaponSelection.up;
        }
        else if (verticalWeapon < 0)
        {
            lastInputDevice = InputDevice.keyboard;
            currentWeapon = WeaponSelection.down;
        }
    }

    /// <summary>
    /// Determines the input updates based off of swipe actions.
    /// </summary>
    private void UpdateSwipeInput()
    {
        // Handle Editor inputs and convert mouse movement to swipe
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            couldBeSwipe = true;
            swipeStartPos = Input.mousePosition;
            swipeStartTime = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            float swipeTime = Time.time - swipeStartTime;
            float swipeDist = (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - swipeStartPos).magnitude;

            if (couldBeSwipe && (swipeTime < maxSwipeTime) && (swipeDist > minSwipeDist))
            {
                verticalSwipeDirection = Mathf.Sign(Input.mousePosition.y - swipeStartPos.y);

                if (verticalSwipeDirection > 0)
                {
                    currentWeapon = WeaponSelection.up;
                }
                else if (verticalSwipeDirection < 0)
                {
                    currentWeapon = WeaponSelection.down;
                }
            }
            couldBeSwipe = false;
        }
#endif

        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    couldBeSwipe = true;
                    swipeStartPos = touch.position;
                    swipeStartTime = Time.time;
                    break;

                case TouchPhase.Moved:
                    if (Mathf.Abs(touch.position.y - swipeStartPos.y) > swipeComfortZone)
                    {
                        couldBeSwipe = false;
                    }
                    break;

                case TouchPhase.Stationary:
                    couldBeSwipe = false;
                    break;

                case TouchPhase.Ended:
                    float swipeTime = Time.time - swipeStartTime;
                    float swipeDist = (touch.position - swipeStartPos).magnitude;

                    if (couldBeSwipe && (swipeTime < maxSwipeTime) && (swipeDist > minSwipeDist))
                    {
                        verticalSwipeDirection = Mathf.Sign(touch.position.y - swipeStartPos.y);

                        if (verticalSwipeDirection > 0)
                        {
                            currentWeapon = WeaponSelection.up;
                        }
                        else if (verticalSwipeDirection < 0)
                        {
                            currentWeapon = WeaponSelection.down;
                        }
                    }

                    couldBeSwipe = false;
                    break;
                default:
                    // Intentionally empty
                    break;
            }
        }

    }

    /// <summary>
    /// Updates the last input device for this frame.
    /// </summary>
    private void UpdateLastInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastInputDevice = InputDevice.touch;
        }
    }
}
