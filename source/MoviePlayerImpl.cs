using UnityEngine;
using RenderHeads.Media.AVProVideo;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public class MoviePlayerImpl : MonoBehaviour
    {
        private MediaPlayer _mediaPlayer = null;
        private DisplayIMGUI _displayIMGUI = null;
        private bool _isAnmPlaying = false;
        private bool _isStarted = false;
        private float _prevTime = 0f;
        private float _aspectRatio = 1f;
        private float _duration = 0f;
        private float _frameRate = 60f;
        private bool _metaUpdated = false;

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        private static TimelineData timeline
        {
            get
            {
                return timelineManager.timeline;
            }
        }

        public bool isDisplayOnGUI
        {
            get
            {
                return timeline.videoDisplayOnGUI;
            }
        }

        public float currentTime
        {
            get
            {
                return timelineManager.currentTime;
            }
        }

        public float duration
        {
            get
            {
                return _duration;
            }
        }

        public float frameRate
        {
            get
            {
                return _frameRate;
            }
        }

        public void Awake()
        {
            _mediaPlayer = gameObject.AddComponent<MediaPlayer>();
            _mediaPlayer.Events.AddListener(OnVideoEvent);

            if (isDisplayOnGUI)
            {
                _displayIMGUI = gameObject.AddComponent<DisplayIMGUI>();
                _displayIMGUI._mediaPlayer = _mediaPlayer;
                _displayIMGUI._scaleMode = ScaleMode.ScaleToFit;
                _displayIMGUI._fullScreen = false;
            }
            else
            {
                MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.mesh = CreateQuadMesh();

                Material videoMaterial = new Material(Shader.Find("Unlit/Texture"));
                meshRenderer.material = videoMaterial;

                ApplyToMaterial applyToMaterial = gameObject.AddComponent<ApplyToMaterial>();
                applyToMaterial.Material = videoMaterial;
                applyToMaterial.Player = _mediaPlayer;
            }
        }

        public void OnDestroy()
        {
            _mediaPlayer = null;
            _displayIMGUI = null;
        }

        public void LoadMovie(string videoPath)
        {
            //_mediaPlayer.PlatformOptionsWindows.videoApi = Windows.VideoApi.MediaFoundation;
            _mediaPlayer.OpenVideoFromFile(
                MediaPlayer.FileLocation.AbsolutePathOrURL,
                videoPath,
                true);

            _mediaPlayer.m_Loop = true;
            _isStarted = false;

            UpdateVolume();
            UpdateTransform();
            UpdateColor();
            UpdateSpeed();
        }

        public void Update()
        {
            if (_mediaPlayer == null || _mediaPlayer.Control == null)
            {
                return;
            }

            if (_metaUpdated)
            {
                UpdateTransform();
                _metaUpdated = false;
            }

            if (currentTime < _prevTime)
            {
                UpdateSeekTime();
            }
            _prevTime = currentTime;

            var newIsAnmPlaying = timelineManager.isAnmPlaying;
            if (_isAnmPlaying != newIsAnmPlaying)
            {
                _isAnmPlaying = newIsAnmPlaying;
                UpdateSpeed();
            }
        }

        public void UpdateTransform()
        {
            if (_mediaPlayer != null && _mediaPlayer.Info != null)
            {
                if (isDisplayOnGUI)
                {
                    // 位置調整
                    _displayIMGUI._x = timeline.videoGUIPosition.x;
                    _displayIMGUI._y = timeline.videoGUIPosition.y;

                    // アスペクト比調整
                    if (_aspectRatio > 1f)
                    {
                        _displayIMGUI._width = timeline.videoGUIScale;
                        _displayIMGUI._height = timeline.videoGUIScale / _aspectRatio;
                    }
                    else
                    {
                        _displayIMGUI._width = timeline.videoGUIScale * _aspectRatio;
                        _displayIMGUI._height = timeline.videoGUIScale;
                    }
                }
                else
                {
                    // 位置調整
                    gameObject.transform.position = timeline.videoPosition;

                    // アスペクト比調整
                    var scale = Vector3.one * timeline.videoScale;
                    scale.x = scale.y * _aspectRatio;
                    gameObject.transform.localScale = scale;

                    // ビルボード補正
                    /*var cameraTransform = SH.mainCamera.transform;
                    gameObject.transform.LookAt(
                        gameObject.transform.position + cameraTransform.rotation * Vector3.forward,
                        cameraTransform.rotation * Vector3.up);*/

                    var rotation = timeline.videoRotation;
                    gameObject.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
                }
                
            }
        }

        public void UpdateVolume()
        {
            if (_mediaPlayer != null && _mediaPlayer.Control != null)
            {
                _mediaPlayer.Control.SetVolume(timeline.videoVolume);
                _mediaPlayer.m_Muted = timeline.videoVolume == 0f;
            }
        }

        public void UpdateSpeed()
        {
            if (_mediaPlayer != null && _mediaPlayer.Control != null)
            {
                var playbackRate = _isAnmPlaying ? timelineManager.anmSpeed : 0f;
                _mediaPlayer.Control.SetPlaybackRate(playbackRate);
            }
        }

        public void UpdateSeekTime()
        {
            if (!_isStarted)
            {
                return;
            }

            if (_mediaPlayer != null && _mediaPlayer.Control != null)
            {
                var seekTimeMs = (currentTime + timeline.videoStartTime) * 1000f;
                var playingTimeMs = _mediaPlayer.Control.GetCurrentTimeMs();
                if (Mathf.Abs(seekTimeMs - playingTimeMs) > 1)
                {
                    _mediaPlayer.Control.Seek(seekTimeMs);
                }

                _prevTime = currentTime;
            }
        }

        public void UpdateColor()
        {
            if (_displayIMGUI != null)
            {
                var color = Color.white;
                color.a = timeline.videoGUIAlpha;
                _displayIMGUI._alphaBlend = color.a != 1f;
                _displayIMGUI._color = color;
            }
        }

        private void OnVideoEvent(
            MediaPlayer mp,
            MediaPlayerEvent.EventType et,
            ErrorCode errorCode)
        {
            //PluginUtils.LogDebug("MoviePlayer：EventType：" + et.ToString());

            if (errorCode != ErrorCode.None)
            {
                PluginUtils.LogError("MoviePlayer：エラー EventType：" + et.ToString() + "  ErrorCode：" + errorCode.ToString());
                return;
            }

            if (et == MediaPlayerEvent.EventType.Started)
            {
                _isStarted = true;
                UpdateSeekTime();
            }

            if (et == MediaPlayerEvent.EventType.MetaDataReady)
            {
                _aspectRatio = (float)_mediaPlayer.Info.GetVideoWidth() / _mediaPlayer.Info.GetVideoHeight();
                _duration = _mediaPlayer.Info.GetDurationMs() / 1000f;
                _frameRate = _mediaPlayer.Info.GetVideoFrameRate();
                _metaUpdated = true;
            }
        }

        private Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[] {
                new Vector3(-0.5f, 0f, 0),
                new Vector3(0.5f, 0f, 0),
                new Vector3(-0.5f, 1f, 0),
                new Vector3(0.5f, 1f, 0),
            };
            mesh.uv = new Vector2[] {
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            mesh.triangles = new int[] {
                0, 1, 2,
                2, 1, 3,
                0, 2, 1,
                2, 3, 1,
            };
            return mesh;
        }
    }
}