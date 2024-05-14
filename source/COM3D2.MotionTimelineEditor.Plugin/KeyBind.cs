using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public enum KeyBindType
    {
        PluginToggle,
        Visible,
        AddKeyFrame,
        RemoveKeyFrame,
        Play,
        EditMode,
        Copy,
        Paste,
        FlipPaste,
        PoseCopy,
        PosePaste,
        PrevFrame,
        NextFrame,
        PrevKeyFrame,
        NextKeyFrame,
        MultiSelect,
        Undo,
        Redo,
    }

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
            keyCode = KeyCode.None;
            ctrl = false;
            shift = false;
            alt = false;

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
                    case "":
                        keyCode = KeyCode.None;
                        break;
                    default:
                        keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), part);
                        break;
                }
            }
        }

        public override string ToString()
        {
            var parts = new List<string>();

            if (ctrl) parts.Add("Ctrl");
            if (shift) parts.Add("Shift");
            if (alt) parts.Add("Alt");

            switch (keyCode)
            {
                case KeyCode.Return:
                    parts.Add("Enter");
                    break;
                case KeyCode.None:
                    break;
                default:
                    parts.Add(keyCode.ToString());
                    break;
            }

            return string.Join("+", parts.ToArray());
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

        private float _keyDownTime = 0f;
        private bool _keyDownFirst = false;

        public bool GetKeyDownRepeat(float repeatTimeFirst, float repeatTime)
        {
            if (GetKeyDown())
            {
                _keyDownTime = Time.realtimeSinceStartup;
                _keyDownFirst = true;
                return true;
            }

            if (GetKey())
            {
                var diffTime = Time.realtimeSinceStartup - _keyDownTime;
                if (diffTime > (_keyDownFirst ? repeatTimeFirst : repeatTime))
                {
                    _keyDownTime = Time.realtimeSinceStartup;
                    _keyDownFirst = false;
                    return true;
                }
            }

            return false;
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