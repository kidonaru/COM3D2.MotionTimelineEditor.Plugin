using UnityEngine;
using RenderHeads.Media.AVProVideo;
using UnityEngine.AI;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using SH = StudioHack;
    using MTE = MotionTimelineEditor;

    public class MoviePlayer
    {
        private GameObject _gameObject = null;
        private MediaPlayer _mediaPlayer = null;
        private DisplayIMGUI _displayIMGUI = null;
        private string _loadedVideoPath = "";
        private bool _isAnmPlaying = false;
        private float _prevTime = 0f;
        private float _aspectRatio = 1f;
        private float _duration = 0f;
        private float _frameRate = 60f;
        private bool _isDisplayOnGUI = false;
        private bool _metaUpdated = false;

        private static MoviePlayer _instance;
        public static MoviePlayer instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MoviePlayer();
                }
                return _instance;
            }
        }

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

        private static Config config
        {
            get
            {
                return MTE.config;
            }
        }

        private string videoPath
        {
            get
            {
                return timeline != null ? timeline.videoPath : "";
            }
        }

        public bool isValidPath
        {
            get
            {
                if (videoPath.Length == 0)
                {
                    return false;
                }

                return System.IO.File.Exists(videoPath);
            }
        }

        public bool isEnabled
        {
            get
            {
                return isValidPath && timeline.videoEnabled;
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

        private MoviePlayer()
        {
        }

        private void Setup()
        {
            if (_isDisplayOnGUI != timeline.videoDisplayOnGUI)
            {
                if (_gameObject != null)
                {
                    Object.Destroy(_gameObject);
                    _gameObject = null;
                    _mediaPlayer = null;
                    _displayIMGUI = null;
                }
                _isDisplayOnGUI = timeline.videoDisplayOnGUI;
            }

            if (_gameObject != null)
            {
                return;
            }

            _gameObject = new GameObject("MoviePlayer");
            _mediaPlayer = _gameObject.AddComponent<MediaPlayer>();
            _mediaPlayer.Events.AddListener(OnVideoEvent);

            if (_isDisplayOnGUI)
            {
                _displayIMGUI = _gameObject.AddComponent<DisplayIMGUI>();
                _displayIMGUI._mediaPlayer = _mediaPlayer;
                _displayIMGUI._scaleMode = ScaleMode.ScaleToFit;
                _displayIMGUI._fullScreen = false;
            }
            else
            {
                MeshRenderer meshRenderer = this._gameObject.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = this._gameObject.AddComponent<MeshFilter>();
                meshFilter.mesh = CreateQuadMesh();

                Material videoMaterial = new Material(Shader.Find("Unlit/Texture"));
                meshRenderer.material = videoMaterial;

                ApplyToMaterial applyToMaterial = _gameObject.AddComponent<ApplyToMaterial>();
                applyToMaterial.Material = videoMaterial;
                applyToMaterial.Player = _mediaPlayer;
            }
        }

        public void Update()
        {
            if (!isEnabled)
            {
                return;
            }

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
            if (_gameObject != null && _mediaPlayer != null && _mediaPlayer.Info != null)
            {
                if (_isDisplayOnGUI)
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
                    _gameObject.transform.position = timeline.videoPosition;

                    // アスペクト比調整
                    var scale = Vector3.one * timeline.videoScale;
                    scale.x = scale.y * _aspectRatio;
                    _gameObject.transform.localScale = scale;

                    // ビルボード補正
                    /*var cameraTransform = SH.mainCamera.transform;
                    _gameObject.transform.LookAt(
                        _gameObject.transform.position + cameraTransform.rotation * Vector3.forward,
                        cameraTransform.rotation * Vector3.up);*/

                    var rotation = timeline.videoRotation;
                    _gameObject.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
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
            if (_mediaPlayer != null && _mediaPlayer.Control != null)
            {
                var seekTime = currentTime + timeline.videoStartTime;
                _mediaPlayer.Control.Seek(seekTime * 1000f);

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

        public void LoadMovie()
        {
            if (!isEnabled)
            {
                return;
            }

            if (_loadedVideoPath == videoPath)
            {
                return;
            }
            _loadedVideoPath = videoPath;

            Setup();

            //_mediaPlayer.PlatformOptionsWindows.videoApi = Windows.VideoApi.MediaFoundation;
            _mediaPlayer.OpenVideoFromFile(
                MediaPlayer.FileLocation.AbsolutePathOrURL,
                videoPath,
                true);

            _mediaPlayer.m_Loop = true;

            UpdateSeekTime();
            UpdateVolume();
            UpdateTransform();
            UpdateColor();
            UpdateSpeed();
        }

        public void UnloadMovie()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.CloseVideo();
                _loadedVideoPath = "";
                _isAnmPlaying = false;
            }
        }

        public void ReloadMovie()
        {
            UnloadMovie();
            LoadMovie();
        }

        private void OnVideoEvent(
            MediaPlayer mp,
            MediaPlayerEvent.EventType et,
            ErrorCode errorCode)
        {
            Extensions.LogDebug("MoviePlayer：EventType：" + et.ToString() + "  ErrorCode：" + errorCode.ToString());

            if (errorCode != ErrorCode.None)
            {
                Extensions.LogError("MoviePlayer：エラー EventType：" + et.ToString() + "  ErrorCode：" + errorCode.ToString());
                return;
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
                new Vector3(-0.5f, 0.0f, 0),
                new Vector3(0.5f, 0.0f, 0),
                new Vector3(-0.5f, 1.0f, 0),
                new Vector3(0.5f, 1.0f, 0),
            };
            mesh.uv = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
            };
            mesh.triangles = new int[] { 0, 1, 2, 2, 1, 3 };
            return mesh;
        }
    }
}