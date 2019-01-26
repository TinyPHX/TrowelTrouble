using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Remoting.Messaging;

[System.Serializable]
public class InputDevice
{
    [SerializeField] private static InputImageMapping inputImageMapping;
    
    public enum InputDeviceType
    {
        NONE,
        KEYBOARD,
        MOBILE,
        XBOX,
        PS4,
    }
    
    public enum ID { NONE, M1, K1, C1, C2, C3, C4, C5, C6, C7, C8, C9, C10, C11 };

    public static List<ID> CONTROLLERS = new List<ID> { ID.C1, ID.C2, ID.C3, ID.C4, ID.C5, ID.C6, ID.C7, ID.C8, ID.C9, ID.C10, ID.C11 };
    
    public enum RawInputs {
        NONE,
        AXIS_X, AXIS_Y, AXIS_Y_INVERT, AXIS_3, AXIS_4, AXIS_5, AXIS_5_INVERT, AXIS_6, AXIS_6_INVERT, AXIS_7, AXIS_8, AXIS_9, AXIS_10,
        BUTTON_0, BUTTON_1, BUTTON_2, BUTTON_3, BUTTON_4, BUTTON_5, BUTTON_6, BUTTON_7, BUTTON_8, BUTTON_9, BUTTON_10, BUTTON_11, BUTTON_12, BUTTON_13, 
    };

    public enum GenericInputs
    {
        NONE,
        AXIS_1_X,
        AXIS_1_Y,
        AXIS_2_X,
        AXIS_2_Y,
        AXIS_3_X,
        AXIS_3_Y,
        AXIS_ALT_1, 
        AXIS_ALT_2,
        OPTION_1,
        OPTION_2,
        ACTION_1,
        ACTION_2,
        ACTION_3,
        ACTION_4,
        ACTION_ALT_1,
        ACTION_ATL_2,
        INDEX_1,
        INDEX_2,
        INDEX_3,
        INDEX_4,
        INDEX_5,
        INDEX_6,
        INDEX_7,
        INDEX_8,
        INDEX_9,
        INDEX_0,
    }

    public enum XBoxInputs
    {
        NONE,
        TRIGGER_L,
        TRIGGER_R,
        BUMPER_L,
        BUMPER_R,
        JOY_L_X,
        JOY_L_Y,
        DPAD_L_X,
        DPAD_L_Y,
        JOY_R_X,
        JOY_R_Y,
        BUTTON_BACK,
        BUTTON_MENU,
        BUTTON_A,
        BUTTON_B,
        BUTTON_X,
        BUTTON_Y,
    }

    public enum Ps4Inputs
    {
        NONE,
        TRIGGER_L,
        TRIGGER_R,
        BUMPER_L,
        BUMPER_R,
        JOY_L_X,
        JOY_L_Y,
        DPAD_L_X,
        DPAD_L_Y,
        JOY_R_X,
        JOY_R_Y,
        BUTTON_SHARE,
        BUTTON_OPTIONS,
        BUTTON_X,
        BUTTON_CIRCLE,
        BUTTON_SQUARE,
        BUTTON_TRIANGLE,
    }

    public enum KeyboardInputs
    {
        NONE,
        WS,
        AD,
        UPDOWN,
        LEFTRIGHT,
        SCROLL_X,
        SCROLL_Y,
        BACKSPACE,
        ENTER,
        DELETE,
        ESCAPE,
        SPACE,
        SHIFT,
        Q,
        E,
        TAB,
        ALT,
        MOUSE_LEFT,
        MOUSE_MIDDLE,
        MOUSE_RIGHT,
        NUMBER_1,
        NUMBER_2,
        NUMBER_3,
        NUMBER_4,
        NUMBER_5,
        NUMBER_6,
        NUMBER_7,
        NUMBER_8,
        NUMBER_9,
        NUMBER_0,
    }

    //Maps
    public Dictionary<GenericInputs, XBoxInputs> genericToXBoxMap;
    public Dictionary<GenericInputs, Ps4Inputs> genericToPs4Map;
    public Dictionary<GenericInputs, KeyboardInputs> genericToKeyboardMap;
    public Dictionary<XBoxInputs, RawInputs> xBoxToRawMap;
    public Dictionary<Ps4Inputs, RawInputs> ps4ToRawMap;

    public Dictionary<GenericInputs, string> axisMap;
    [SerializeField] InputDeviceType type = InputDeviceType.NONE;
    [SerializeField] private ID id = ID.NONE;
    [SerializeField] private bool valid = false;
    public string name = "";
    private static List<InputDevice> allInputDevices;

    public InputDevice(ID id)
    {
        this.id = id;

        Refresh();

        BuildAxisMap();
    }

    public Sprite GetInputImage(GenericInputs input)
    {
        if (inputImageMapping == null)
        {
            inputImageMapping = Resources.Load<InputImageMapping>("Input Images/Default Input Image Mapping");
        }

        Sprite image = null;

        if (type == InputDeviceType.PS4)
        {
            image = inputImageMapping.GetInputImage(GenericToPs4Map[input]);
        }
        else if (type == InputDeviceType.XBOX)
        {
            image = inputImageMapping.GetInputImage(GenericToXBoxMap[input]);
        }
        else if (type == InputDeviceType.KEYBOARD)
        {
            image = inputImageMapping.GetInputImage(GenericToKeyboardMap[input]);
        }

        return image;
    }

    public void Refresh()
    {
        //PrintRawOutput(false);

        InputDeviceType previousType = type;

        valid = false;
        if (IsMobile())
        {
            type = InputDeviceType.MOBILE;
            valid = true;
        }
        else if (IsKeyboard())
        {
            type = InputDeviceType.KEYBOARD;
            valid = true;
        }
        else if (IsController())
        {
            string[] joystickNames = Input.GetJoystickNames();
            int controllerIndex = CONTROLLERS.IndexOf(Id);

            if (controllerIndex < joystickNames.Length)
            {
                name = joystickNames[controllerIndex];
                
                if (name.Contains("xbox") || name.Contains("Xbox"))
                {
                    type = InputDeviceType.XBOX;
                    valid = true;
                }
                else if (name == "Wireless Controller")
                {
                    type = InputDeviceType.PS4;
                    valid = true;
                }
                else if (name != "")
                {
                    type = InputDeviceType.XBOX;
                    Debug.LogWarning("This controller has not been set up yet. joystickName: " + name);
                    valid = true;
                }
            }
        }

        if (previousType != type)
        {
            BuildAxisMap();
        }
    }

    private void BuildAxisMap()
    {
        axisMap = new Dictionary<GenericInputs, string> { };

        if (type == InputDeviceType.XBOX)
        {
            foreach (var item in GenericToXBoxMap)
            {
                XBoxInputs xboxInput = GenericToXBoxMap[item.Key];
                RawInputs rawInput = XBoxToRawMap[xboxInput];
                string axis = string.Concat(Id.ToString(), "_", rawInput.ToString());
                if (!AxisMap.ContainsKey(item.Key))
                {
                    AxisMap.Add(item.Key, axis);
                }
            }
        }
        else if (type == InputDeviceType.PS4)
        {
            foreach (var item in GenericToPs4Map)
            {
                Ps4Inputs ps4Inputs = GenericToPs4Map[item.Key];
                RawInputs rawInput = Ps4ToRawMap[ps4Inputs];
                string axis = string.Concat(Id.ToString(), "_", rawInput.ToString());
                if (!AxisMap.ContainsKey(item.Key))
                {
                    AxisMap.Add(item.Key, axis);
                }
            }
        }
        else if (type == InputDeviceType.KEYBOARD)
        {
            foreach(var item in GenericToKeyboardMap)
            {
                KeyboardInputs keyboardInput = GenericToKeyboardMap[item.Key];
                string axis = string.Concat(Id.ToString(), "_", keyboardInput.ToString());
                if (!AxisMap.ContainsKey(item.Key))
                {
                    AxisMap.Add(item.Key, axis);
                }
            }
        }
    }

    //This is for debug only and is very CPU intensive.
    public void PrintRawOutput(bool showZeros)
    {
        if (id != ID.NONE && id != ID.K1)
        {
            foreach (RawInputs rawInput in Enum.GetValues(typeof(RawInputs)))
            {
                string axis = string.Concat(Id.ToString(), "_", rawInput.ToString());
                float value = Input.GetAxis(axis);
                if (showZeros || value != 0)
                {
                    Debug.Log(string.Concat(axis, ": ", value));
                }
            }
        }
    }

    public float GetAxis(GenericInputs axis)
    {
        string axisValue;
        if (AxisMap.TryGetValue(axis, out axisValue))
        {
            return Input.GetAxis(axisValue);
        }
        else
        {
            return 0;
        }
    }

    public float GetAxisRaw(GenericInputs axis)
    {
        string axisValue;
        if (AxisMap.TryGetValue(axis, out axisValue))
        {
            return Input.GetAxisRaw(axisValue);
        }
        else
        {
            return 0;
        }
    }

    public float GetAxis(GenericInputs axis, bool invert)
    {
        if (invert)
        {
            return -GetAxis(axis);
        }
        else
        {
            return GetAxis(axis);
        }
    }

    public string GetAxisString(GenericInputs axis)
    {
        return AxisMap[axis];
    }

    public ID Id
    {
        get
        {
            return id;
        }
    }

    public bool Valid
    {
        get { return valid; }
    }

    public InputDeviceType Type
    {
        get
        {
            return type;
        }
    }

    public bool IsMobile()
    {
        return id == ID.M1;
    }

    public bool IsKeyboard()
    {
        return id == ID.K1;
    }

    public bool IsController()
    {
        return CONTROLLERS.IndexOf(Id) != -1;
    }

    public static List<InputDevice> AllInputDevices
    {
        get
        {
            if (allInputDevices == null)
            {
                allInputDevices = new List<InputDevice> { };
                allInputDevices.Add(new InputDevice(InputDevice.ID.K1));
                //inputDevices.Add(new InputDevice(InputDevice.ID.M1));

                int controllerCount = InputDevice.CONTROLLERS.Count;
                string[] joyNames = Input.GetJoystickNames();
                for (int i = 0; i < controllerCount; i++)
                {
                    InputDevice.ID inputDeviceID = InputDevice.CONTROLLERS[i];
                    InputDevice inputDevice = new InputDevice(inputDeviceID);
                    
                    allInputDevices.Add(inputDevice);
                }
            }

            return allInputDevices;
        }
    }

    public Dictionary<GenericInputs, string> AxisMap
    {
        get
        {
            if (axisMap == null)
            {
                BuildAxisMap();
            }
            
            return axisMap;
        }
    }

    public Dictionary<GenericInputs, XBoxInputs> GenericToXBoxMap
    {
        get
        {
            if (genericToXBoxMap == null)
            {
                genericToXBoxMap = new Dictionary<GenericInputs, XBoxInputs>()
                {
                    { GenericInputs.NONE,     XBoxInputs.NONE },
                    { GenericInputs.AXIS_1_X, XBoxInputs.JOY_L_X }, 
                    { GenericInputs.AXIS_1_Y, XBoxInputs.JOY_L_Y }, 
                    { GenericInputs.AXIS_2_X, XBoxInputs.JOY_R_X }, 
                    { GenericInputs.AXIS_2_Y, XBoxInputs.JOY_R_Y }, 
                    { GenericInputs.AXIS_3_X, XBoxInputs.DPAD_L_X }, 
                    { GenericInputs.AXIS_3_Y, XBoxInputs.DPAD_L_Y }, 
                    { GenericInputs.AXIS_ALT_1, XBoxInputs.TRIGGER_L }, 
                    { GenericInputs.AXIS_ALT_2, XBoxInputs.TRIGGER_R }, 
                    { GenericInputs.OPTION_1, XBoxInputs.BUTTON_BACK }, 
                    { GenericInputs.OPTION_2, XBoxInputs.BUTTON_MENU }, 
                    { GenericInputs.ACTION_1, XBoxInputs.BUTTON_A }, 
                    { GenericInputs.ACTION_2, XBoxInputs.BUTTON_B }, 
                    { GenericInputs.ACTION_3, XBoxInputs.BUTTON_X }, 
                    { GenericInputs.ACTION_4, XBoxInputs.BUTTON_Y }, 
                    { GenericInputs.ACTION_ALT_1, XBoxInputs.BUMPER_L }, 
                    { GenericInputs.ACTION_ATL_2, XBoxInputs.BUMPER_R }, 
                    { GenericInputs.INDEX_1, XBoxInputs.NONE },
                    { GenericInputs.INDEX_2, XBoxInputs.NONE },
                    { GenericInputs.INDEX_3, XBoxInputs.NONE },
                    { GenericInputs.INDEX_4, XBoxInputs.NONE },
                    { GenericInputs.INDEX_5, XBoxInputs.NONE },
                    { GenericInputs.INDEX_6, XBoxInputs.NONE },
                    { GenericInputs.INDEX_7, XBoxInputs.NONE },
                    { GenericInputs.INDEX_8, XBoxInputs.NONE },
                    { GenericInputs.INDEX_9, XBoxInputs.NONE },
                    { GenericInputs.INDEX_0, XBoxInputs.NONE },
                };
            }
            
            return genericToXBoxMap;
        }
    }

    public Dictionary<GenericInputs, Ps4Inputs> GenericToPs4Map
    {
        get
        {
            if (genericToPs4Map == null)
            {
                genericToPs4Map = new Dictionary<GenericInputs, Ps4Inputs>()
                {
                    { GenericInputs.NONE,           Ps4Inputs.NONE },
                    { GenericInputs.AXIS_1_X,       Ps4Inputs.JOY_L_X }, 
                    { GenericInputs.AXIS_1_Y,       Ps4Inputs.JOY_L_Y }, 
                    { GenericInputs.AXIS_2_X,       Ps4Inputs.JOY_R_X }, 
                    { GenericInputs.AXIS_2_Y,       Ps4Inputs.JOY_R_Y }, 
                    { GenericInputs.AXIS_3_X,       Ps4Inputs.DPAD_L_X }, 
                    { GenericInputs.AXIS_3_Y,       Ps4Inputs.DPAD_L_Y }, 
                    { GenericInputs.AXIS_ALT_1,     Ps4Inputs.TRIGGER_L }, 
                    { GenericInputs.AXIS_ALT_2,     Ps4Inputs.TRIGGER_R }, 
                    { GenericInputs.OPTION_1,       Ps4Inputs.BUTTON_SHARE }, 
                    { GenericInputs.OPTION_2,       Ps4Inputs.BUTTON_OPTIONS }, 
                    { GenericInputs.ACTION_1,       Ps4Inputs.BUTTON_X }, 
                    { GenericInputs.ACTION_2,       Ps4Inputs.BUTTON_CIRCLE }, 
                    { GenericInputs.ACTION_3,       Ps4Inputs.BUTTON_SQUARE }, 
                    { GenericInputs.ACTION_4,       Ps4Inputs.BUTTON_TRIANGLE }, 
                    { GenericInputs.ACTION_ALT_1,   Ps4Inputs.BUMPER_L }, 
                    { GenericInputs.ACTION_ATL_2,   Ps4Inputs.BUMPER_R }, 
                    { GenericInputs.INDEX_1,        Ps4Inputs.NONE },
                    { GenericInputs.INDEX_2,        Ps4Inputs.NONE },
                    { GenericInputs.INDEX_3,        Ps4Inputs.NONE },
                    { GenericInputs.INDEX_4,        Ps4Inputs.NONE },
                    { GenericInputs.INDEX_5,        Ps4Inputs.NONE },
                    { GenericInputs.INDEX_6,        Ps4Inputs.NONE },
                    { GenericInputs.INDEX_7,        Ps4Inputs.NONE },
                    { GenericInputs.INDEX_8,        Ps4Inputs.NONE },
                    { GenericInputs.INDEX_9,        Ps4Inputs.NONE },
                    { GenericInputs.INDEX_0,        Ps4Inputs.NONE },
                };
            }
            
            return genericToPs4Map;
        }
    }

    public Dictionary<GenericInputs, KeyboardInputs> GenericToKeyboardMap
    {
        get
        {
            if (genericToKeyboardMap == null)
            {
                genericToKeyboardMap = new Dictionary<GenericInputs, KeyboardInputs>()
                {
                    { GenericInputs.NONE,     KeyboardInputs.NONE },
                    { GenericInputs.AXIS_1_X, KeyboardInputs.AD }, 
                    { GenericInputs.AXIS_1_Y, KeyboardInputs.WS }, 
                    { GenericInputs.AXIS_2_X, KeyboardInputs.LEFTRIGHT },
                    { GenericInputs.AXIS_2_Y, KeyboardInputs.UPDOWN },
                    { GenericInputs.AXIS_3_X, KeyboardInputs.NONE },
                    { GenericInputs.AXIS_3_Y, KeyboardInputs.NONE }, 
                    { GenericInputs.AXIS_ALT_1, KeyboardInputs.MOUSE_RIGHT },
                    { GenericInputs.AXIS_ALT_2, KeyboardInputs.MOUSE_LEFT },
                    { GenericInputs.OPTION_1, KeyboardInputs.DELETE },
                    { GenericInputs.OPTION_2, KeyboardInputs.ESCAPE },
                    { GenericInputs.ACTION_1, KeyboardInputs.SPACE },
                    { GenericInputs.ACTION_2, KeyboardInputs.BACKSPACE },
                    { GenericInputs.ACTION_3, KeyboardInputs.E },
                    { GenericInputs.ACTION_4, KeyboardInputs.TAB },
                    { GenericInputs.ACTION_ALT_1, KeyboardInputs.SCROLL_X },
                    { GenericInputs.ACTION_ATL_2, KeyboardInputs.SCROLL_Y },
                    { GenericInputs.INDEX_1, KeyboardInputs.NUMBER_1 },
                    { GenericInputs.INDEX_2, KeyboardInputs.NUMBER_2 },
                    { GenericInputs.INDEX_3, KeyboardInputs.NUMBER_3 },
                    { GenericInputs.INDEX_4, KeyboardInputs.NUMBER_4 },
                    { GenericInputs.INDEX_5, KeyboardInputs.NUMBER_5 },
                    { GenericInputs.INDEX_6, KeyboardInputs.NUMBER_6 },
                    { GenericInputs.INDEX_7, KeyboardInputs.NUMBER_7 },
                    { GenericInputs.INDEX_8, KeyboardInputs.NUMBER_8 },
                    { GenericInputs.INDEX_9, KeyboardInputs.NUMBER_9 },
                    { GenericInputs.INDEX_0, KeyboardInputs.NUMBER_0 },
                };
            }
            
            return genericToKeyboardMap;
        }
    }

    public Dictionary<XBoxInputs, RawInputs> XBoxToRawMap
    {
        get
        {
            if (xBoxToRawMap == null)
            {
                xBoxToRawMap = new Dictionary<XBoxInputs, RawInputs>()
                {
                    { XBoxInputs.NONE, RawInputs.NONE }, 
                    { XBoxInputs.TRIGGER_L, RawInputs.AXIS_9 }, 
                    { XBoxInputs.TRIGGER_R, RawInputs.AXIS_10 }, 
                    { XBoxInputs.BUMPER_L, RawInputs.BUTTON_4 }, 
                    { XBoxInputs.BUMPER_R, RawInputs.BUTTON_5 }, 
                    { XBoxInputs.JOY_L_X, RawInputs.AXIS_X }, 
                    { XBoxInputs.JOY_L_Y, RawInputs.AXIS_Y_INVERT }, 
                    { XBoxInputs.DPAD_L_X, RawInputs.AXIS_6 }, 
                    { XBoxInputs.DPAD_L_Y, RawInputs.AXIS_7 }, 
                    { XBoxInputs.JOY_R_X, RawInputs.AXIS_4 }, 
                    { XBoxInputs.JOY_R_Y, RawInputs.AXIS_5_INVERT }, 
                    { XBoxInputs.BUTTON_BACK, RawInputs.BUTTON_6 }, 
                    { XBoxInputs.BUTTON_MENU, RawInputs.BUTTON_7 }, 
                    { XBoxInputs.BUTTON_A, RawInputs.BUTTON_0 }, 
                    { XBoxInputs.BUTTON_B, RawInputs.BUTTON_1 }, 
                    { XBoxInputs.BUTTON_X, RawInputs.BUTTON_2 }, 
                    { XBoxInputs.BUTTON_Y, RawInputs.BUTTON_3 }, 
                };
            }
            
            return xBoxToRawMap;
        }
    }

    public Dictionary<Ps4Inputs, RawInputs> Ps4ToRawMap
    {
        get
        {
            if (ps4ToRawMap == null)
            {
                ps4ToRawMap = new Dictionary<Ps4Inputs, RawInputs>()
                {
                    { Ps4Inputs.NONE,               RawInputs.NONE }, 
                    { Ps4Inputs.TRIGGER_L,          RawInputs.AXIS_4 }, 
                    { Ps4Inputs.TRIGGER_R,          RawInputs.AXIS_5 }, 
                    { Ps4Inputs.BUMPER_L,           RawInputs.BUTTON_4 }, 
                    { Ps4Inputs.BUMPER_R,           RawInputs.BUTTON_5 }, 
                    { Ps4Inputs.JOY_L_X,            RawInputs.AXIS_X }, 
                    { Ps4Inputs.JOY_L_Y,            RawInputs.AXIS_Y_INVERT }, 
                    { Ps4Inputs.DPAD_L_X,           RawInputs.AXIS_7 }, 
                    { Ps4Inputs.DPAD_L_Y,           RawInputs.AXIS_8 }, 
                    { Ps4Inputs.JOY_R_X,            RawInputs.AXIS_3 }, 
                    { Ps4Inputs.JOY_R_Y,            RawInputs.AXIS_6_INVERT }, 
                    { Ps4Inputs.BUTTON_SHARE,       RawInputs.BUTTON_8 }, 
                    { Ps4Inputs.BUTTON_OPTIONS,     RawInputs.BUTTON_9 }, 
                    { Ps4Inputs.BUTTON_X,           RawInputs.BUTTON_1 }, 
                    { Ps4Inputs.BUTTON_CIRCLE,      RawInputs.BUTTON_2 }, 
                    { Ps4Inputs.BUTTON_SQUARE,      RawInputs.BUTTON_0 }, 
                    { Ps4Inputs.BUTTON_TRIANGLE,    RawInputs.BUTTON_3 }, 
                };
            }
            
            return ps4ToRawMap;
        }
    }
}
