using UnityEngine;
using RenderHeads.Media.AVProVideo;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MoviePlayerImpl : MonoBehaviour
    {
        private MediaPlayer _mediaPlayer = null;
        private DisplayIMGUI _displayIMGUI = null;
        private MeshFilter _meshFilter = null;
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

        private static ITimelineLayer currentLayer
        {
            get
            {
                return timelineManager.currentLayer;
            }
        }

        public bool isDisplayOnGUI
        {
            get
            {
                return timeline.videoDisplayType == VideoDisplayType.GUI;
            }
        }

        public bool isDisplayBackmost
        {
            get
            {
                return timeline.videoDisplayType == VideoDisplayType.Backmost;
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
                _meshFilter = gameObject.AddComponent<MeshFilter>();
                _meshFilter.mesh = CreateQuadMesh();

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
            _meshFilter = null;
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

            var newIsAnmPlaying = currentLayer.isAnmPlaying;
            if (_isAnmPlaying != newIsAnmPlaying)
            {
                _isAnmPlaying = newIsAnmPlaying;
                UpdateSpeed();
            }
        }

        public void LateUpdate()
        {
            if (isDisplayBackmost)
            {
                UpdateTransform();
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
                else if (isDisplayBackmost)
                {
                    var transform = gameObject.transform;
                    var camera = Camera.main;
                    var distanceFromCamera = camera.farClipPlane - 10f;

                    // アスペクト比調整
                    var scale = Vector3.one;
                    scale.y = 2f * distanceFromCamera *
                            Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                    scale.x = scale.y * _aspectRatio;
                    transform.localScale = scale;

                    // 位置調整
                    var position = camera.transform.position;
                    position += camera.transform.forward * distanceFromCamera;
                    transform.position = position;

                    // ビルボード補正
                    transform.LookAt(camera.transform);
                }
                else
                {
                    var transform = gameObject.transform;

                    // 位置調整
                    transform.position = timeline.videoPosition;

                    // アスペクト比調整
                    var scale = Vector3.one * timeline.videoScale;
                    scale.x = scale.y * _aspectRatio;
                    transform.localScale = scale;

                    var rotation = timeline.videoRotation;
                    transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
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
                var seekTimeMs = (currentTime + timeline.startOffsetTime + timeline.videoStartTime) * 1000f;
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

        public void UpdateMesh()
        {
            if (_meshFilter != null)
            {
                if (_meshFilter.mesh != null)
                {
                    Object.Destroy(_meshFilter.mesh);
                    _meshFilter.mesh = null;
                }
                _meshFilter.mesh = CreateQuadMesh();
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

            if (isDisplayBackmost)
            {
                var vertices = new Vector3[] {
                    new Vector3(-0.5f, -0.5f, 0),
                    new Vector3(0.5f, -0.5f, 0),
                    new Vector3(-0.5f, 0.5f, 0),
                    new Vector3(0.5f, 0.5f, 0),
                };

                for (var i = 0; i < vertices.Length; i++)
                {
                    var vertex = vertices[i];
                    vertex.x -= timeline.videoBackmostPosition.x;
                    vertex.y += timeline.videoBackmostPosition.y;
                    vertices[i] = vertex;
                }

                mesh.vertices = vertices;
            }
            else
            {
                mesh.vertices = new Vector3[] {
                    new Vector3(-0.5f, 0f, 0),
                    new Vector3(0.5f, 0f, 0),
                    new Vector3(-0.5f, 1f, 0),
                    new Vector3(0.5f, 1f, 0),
                };
            }
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