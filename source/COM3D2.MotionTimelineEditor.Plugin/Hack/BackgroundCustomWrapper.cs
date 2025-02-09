using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BackgroundCustomWrapper
    {
        private BackgroundCustomField _field = new BackgroundCustomField();
        private Component _backgroundCustom;

        public bool initialized => _backgroundCustom != null;

        public bool Init()
        {
            if (!_field.Init())
            {
                return false;
            }

            {
                GameObject gameObject = GameObject.Find("UnityInjector");
                _backgroundCustom = gameObject.GetComponent(_field.BackgroundCustomType);
                MTEUtils.AssertNull(_backgroundCustom != null, "backgroundCustom is null");
            }

            return true;
        }

        public bool IsValid()
        {
            return initialized;
        }

        public void CreateCategory()
        {
            if (!IsValid()) return;
            _field.createCategory.Invoke(_backgroundCustom, null);
        }

        public void CreateObjectCategory()
        {
            if (!IsValid()) return;
            _field.createObjectCategory.Invoke(_backgroundCustom, null);
        }
    }
}