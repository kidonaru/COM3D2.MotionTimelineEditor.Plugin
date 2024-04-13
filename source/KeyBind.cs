using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class KeyBind
    {
        [XmlIgnore]
        public KeyCode keyCode = KeyCode.None;
        [XmlIgnore]
        public bool ctrl = false;
        [XmlIgnore]
        public bool shift = false;
        [XmlIgnore]
        public bool alt = false;

        public KeyBind(string data)
        {
            FromString(data);
        }

        public KeyBind()
        {
        }

        public void FromString(string data)
        {
            string[] parts = data.Split('+');
            foreach (string part in parts)
            {
                switch (part)
                {
                    case "Ctrl":
                        ctrl = true;
                        break;
                    case "Shift":
                        shift = true;
                        break;
                    case "Alt":
                        alt = true;
                        break;
                    case "Enter":
                        keyCode = KeyCode.Return;
                        break;
                    default:
                        keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), part);
                        break;
                }
            }
        }

        public override string ToString()
        {
            string result = "";
            if (ctrl)
            {
                result += "Ctrl+";
            }
            if (shift)
            {
                result += "Shift+";
            }
            if (alt)
            {
                result += "Alt+";
            }

            if (keyCode == KeyCode.Return)
            {
                result += "Enter";
            }
            else
            {
                result += keyCode.ToString();
            }
            return result;
        }

        private bool IsModifierKeyPressed()
        {
            if (keyCode == KeyCode.None && !ctrl && !shift && !alt)
            {
                return false;
            }

            bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

            if (ctrl != ctrlPressed || shift != shiftPressed || alt != altPressed)
            {
                return false;
            }

            return true;
        }

        public bool GetKey()
        {
            if (!IsModifierKeyPressed())
            {
                return false;
            }
            if (keyCode == KeyCode.None)
            {
                return true;
            }
            return Input.GetKey(keyCode);
        }

        public bool GetKeyDown()
        {
            if (!IsModifierKeyPressed())
            {
                return false;
            }
            if (keyCode == KeyCode.None)
            {
                return true;
            }
            return Input.GetKeyDown(keyCode);
        }

        public bool GetKeyUp()
        {
            if (!IsModifierKeyPressed())
            {
                return false;
            }
            if (keyCode == KeyCode.None)
            {
                return true;
            }
            return Input.GetKeyUp(keyCode);
        }

        public override int GetHashCode()
        {
            return keyCode.GetHashCode()
                ^ ctrl.GetHashCode()
                ^ shift.GetHashCode()
                ^ alt.GetHashCode();
        }
    }
}