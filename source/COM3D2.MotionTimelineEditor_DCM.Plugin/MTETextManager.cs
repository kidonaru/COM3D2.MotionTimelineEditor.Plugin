using System.Collections.Generic;
using COM3D2.DanceCameraMotion.Plugin;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class MTETextManager : ManagerBase
    {
        public static readonly string DefaultFontName = "Yu Gothic Bold";

        private TextManager _textManager = null;
        private TextManager textManager
        {
            get
            {
                if (_textManager == null)
                {
                    _textManager = new TextManager(0);
                }
                return _textManager;
            }
        }

        public static List<string> fontNames = new List<string>
        {
        };

        private static MTETextManager _instance;
        public static MTETextManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MTETextManager();
                }

                return _instance;
            }
        }

        public FreeTextSet[] TextData => textManager.TextData;

        private MTETextManager()
        {
        }

        public override void Init()
        {
        }

        public override void OnLoad()
        {
            InitTextManager();
        }

        public override void OnPluginDisable()
        {
            ReleaseTextManager();
        }

        public override void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            ReleaseTextManager();
        }

        public Font GetFont(string fontName)
        {
            return textManager.GetFont(fontName);
        }

        public void ReleaseTextManager()
        {
            if (_textManager != null)
            {
                _textManager.ReleaseDanceText();
                _textManager = null;
            }
        }

        public void InitTextManager()
        {
            if (timeline == null)
            {
                return;
            }
            if (_textManager != null && _textManager.TextData.Length == timeline.textCount)
            {
                return;
            }

            ReleaseTextManager();

            _textManager = new TextManager(timeline.textCount);

            for (var i = 0; i < timeline.textCount; i++)
            {
                _textManager.InitializeText(i);

                var freeTextSet = GetFreeTextSet(i);
                var text = freeTextSet.text;
                var rect = freeTextSet.rect;

                text.font = _textManager.GetFont(DefaultFontName);
                text.fontSize = 50;
                text.lineSpacing = 50f;
                text.alignment = TextAnchor.MiddleCenter;
                rect.localPosition = Vector3.zero;
                rect.localScale = Vector3.one;
                rect.sizeDelta = new Vector2(1000, 1000);
            }

            if (fontNames.Count == 0)
            {
                _textManager.GetFontNames();
                fontNames = _textManager.FontNames;
            }
        }

        public bool IsValidIndex(int index)
        {
            return index >= 0 && index < textManager.TextData.Length;
        }

        public FreeTextSet GetFreeTextSet(int index)
        {
            if (!IsValidIndex(index))
            {
                return new FreeTextSet();
            }
            return textManager.TextData[index];
        }

        public void UpdateFreeTextSet(int index, FreeTextSet freeTextSet)
        {
            if (IsValidIndex(index))
            {
                textManager.TextData[index] = freeTextSet;
            }
        }
    }
}