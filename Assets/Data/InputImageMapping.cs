using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputImageMapping : ScriptableObject
{
    [Serializable]
    public struct Ps4InputImage
    {
        public string name;
        public InputDevice.Ps4Inputs input;
        public Sprite image;

        public Ps4InputImage(InputDevice.Ps4Inputs input, Sprite image)
        {
            this.name = input.ToString();
            this.input = input;
            this.image = image;
        }
    }
    
    [Serializable]
    public struct XBoxInputImage
    {
        public string name;
        public InputDevice.XBoxInputs input;
        public Sprite image;

        public XBoxInputImage(InputDevice.XBoxInputs input, Sprite image)
        {
            this.name = input.ToString();
            this.input = input;
            this.image = image;
        }
    }
    
    [Serializable]
    public struct KeyboardInputImage
    {
        public string name;
        public InputDevice.KeyboardInputs input;
        public Sprite image;

        public KeyboardInputImage(InputDevice.KeyboardInputs input, Sprite image)
        {
            this.name = input.ToString();
            this.input = input;
            this.image = image;
        }
    }

    [SerializeField] private List<Ps4InputImage> Ps4Mapping;
    [SerializeField] private List<XBoxInputImage> XBoxMapping;
    [SerializeField] private List<KeyboardInputImage> KeyboardMapping;
    
    public void Awake()
    {
        Sprite placeholder = Resources.Load<Sprite>("Input Images/PlaceHolder");

        if (Ps4Mapping == null)
        {
            Ps4Mapping = new List<Ps4InputImage>();
            
            InputDevice.Ps4Inputs[] ps4Enums = (InputDevice.Ps4Inputs[])Enum.GetValues(typeof(InputDevice.Ps4Inputs));

            foreach (InputDevice.Ps4Inputs ps4Enum in ps4Enums)
            {
                Ps4Mapping.Add(new Ps4InputImage(ps4Enum, placeholder));
            }
        }
        
        if (XBoxMapping == null)
        {
            XBoxMapping = new List<XBoxInputImage>();
            
            InputDevice.XBoxInputs[] xBoxEnums = (InputDevice.XBoxInputs[])Enum.GetValues(typeof(InputDevice.XBoxInputs));

            foreach (InputDevice.XBoxInputs xBoxEnum in xBoxEnums)
            {
                XBoxMapping.Add(new XBoxInputImage(xBoxEnum, placeholder));
            }
        }
        
        if (KeyboardMapping == null)
        {
            KeyboardMapping = new List<KeyboardInputImage>();
            
            InputDevice.KeyboardInputs[] keyboardEnums = (InputDevice.KeyboardInputs[])Enum.GetValues(typeof(InputDevice.KeyboardInputs));

            foreach (InputDevice.KeyboardInputs keyboardEnum in keyboardEnums)
            {
                KeyboardMapping.Add(new KeyboardInputImage(keyboardEnum, placeholder));
            }
        }
    }

    public Sprite GetInputImage(InputDevice.Ps4Inputs input)
    {
        return Ps4Mapping.First(i => i.input == input).image;
    }

    public Sprite GetInputImage(InputDevice.XBoxInputs input)
    {
        return XBoxMapping.First(i => i.input == input).image;
    }

    public Sprite GetInputImage(InputDevice.KeyboardInputs input)
    {
        return KeyboardMapping.First(i => i.input == input).image;
    }
}
