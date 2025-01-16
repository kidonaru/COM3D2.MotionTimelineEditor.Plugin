using UnityEngine;
using RenderHeads.Media.AVProVideo;
using System.Collections;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MoviePlayerImpl : MonoBehaviour
    {
        private MediaPlayer _mediaPlayer = null;
        private DisplayIMGUI _displayIMGUI = null;
        private MeshFilter _meshFilter = null;
        private MeshRenderer _meshRenderer = null;
        private ApplyToMaterial _applyToMaterial = null;

        private bool _isAnmPlaying = false;
        private bool _isStarted = false;
        private float _prevTime = 0f;
        private float _aspectRatio = 1f;
        private float _duration = 0f;
        private float _frameRate = 60f;
        private bool _metaUpdated = false;
        private Material _gridMaterial = null;

        public enum SeekState
        {
            None,
            Seeking,
            Adjusting,
        }

        public SeekState _seekState = SeekState.None;

        public bool isDisplayOnGUI
        {
            get => timeline.videoDisplayType == VideoDisplayType.GUI;
        }

        public bool isDisplayBackmost
        {
            get => timeline.videoDisplayType == VideoDisplayType.Backmost;
        }

        public bool isDisplayFrontmost
        {
            get => timeline.videoDisplayType == VideoDisplayType.Frontmost;
        }

        public float currentTime
        {
            get => timelineManager.currentTime;
        }

        public float duration => _duration;

        public float frameRate => _frameRate;

        public Camera targetCamera
        {
            get
            {
                if (isDisplayFrontmost)
                {
                    return cameraManager.frontCamera;
                }
                return PluginUtils.MainCamera;
            }
        }

        public int layerMask
        {
            get
            {
                if (isDisplayFrontmost)
                {
                    return LayerMask.NameToLayer("NGUI");
                }
                return LayerMask.NameToLayer("Default");
            }
        }

        public IMediaControl mediaControl
        {
            get => _mediaPlayer != null ? _mediaPlayer.Control : null;
        }

        public float targetSeekTimeMs
        {
            get => (currentTime + timeline.startOffsetTime + timeline.videoStartTime) * 1000f;
        }

        public float playingTimeMs
        {
            get
            {
                if (mediaControl != null)
                {
                    return mediaControl.GetCurrentTimeMs();
                }
                return 0f;
            }
        }

        public bool isSeeking => _seekState != SeekState.None;

        private static TimelineManager timelineManager => TimelineManager.instance;
        private static TimelineData timeline => timelineManager.timeline;
        private static ITimelineLayer currentLayer => timelineManager.currentLayer;
        private static StudioHackManager studioHackManager => StudioHackManager.instance;
        private static StudioHackBase studioHack => StudioHackManager.instance.studioHack;
        private static Config config => ConfigManager.instance.config;
        private static CameraManager cameraManager =>  CameraManager.instance;

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
                gameObject.layer = layerMask;
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();
                _meshFilter = gameObject.AddComponent<MeshFilter>();
                _meshFilter.mesh = CreateQuadMesh();

                Material material = new Material(Shader.Find(config.videoShaderName));
                _meshRenderer.material = material;

                _applyToMaterial = gameObject.AddComponent<ApplyToMaterial>();
                _applyToMaterial.Material = material;
                _applyToMaterial.Player = _mediaPlayer;
            }

            CreateGridMaterial();
        }

        private void CreateGridMaterial()
        {
            if (_gridMaterial == null)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _gridMaterial = new Material(shader);
                _gridMaterial.hideFlags = HideFlags.HideAndDontSave;
                _gridMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _gridMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _gridMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _gridMaterial.SetInt("_ZWrite", 0);
            }
        }

        public void OnDestroy()
        {
            _mediaPlayer = null;
            _displayIMGUI = null;
            _meshFilter = null;
            _meshRenderer = null;
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
            _seekState = SeekState.None;

            UpdateVolume();
            UpdateTransform();
            UpdateSpeed();
        }

        public void Update()
        {
            if (mediaControl == null)
            {
                return;
            }

            if (_metaUpdated)
            {
                UpdateTransform();
                _metaUpdated = false;
            }

            if (_seekState == SeekState.Adjusting)
            {
                var overTime = playingTimeMs - targetSeekTimeMs;
                //MTEUtils.LogDebug("MoviePlayerImpl: overTime={0}", overTime);
                if (overTime < 10f)
                {
                    _seekState = SeekState.None;
                    UpdateSpeed();
                }
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
                // カメラの位置に合わせて毎フレーム更新
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
                    var camera = targetCamera;
                    var distanceFromCamera = camera.farClipPlane - 10f;

                    // アスペクト比調整
                    var scale = Vector3.one;
                    scale.y = 2f * distanceFromCamera * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                    scale.x = scale.y * _aspectRatio;

                    // スケール調整
                    scale *= timeline.videoBackmostScale;
                    transform.localScale = scale;

                    // 位置調整
                    var position = camera.transform.position;
                    position += camera.transform.forward * distanceFromCamera;
                    transform.position = position;

                    transform.LookAt(camera.transform, camera.transform.up);
                }
                else if (isDisplayFrontmost)
                {
                    var transform = gameObject.transform;
                    var camera = targetCamera;
                    var distanceFromCamera = camera.nearClipPlane + 0.1f;

                    // アスペクト比調整
                    var scale = Vector3.one;
                    scale.y = camera.orthographicSize * 2f;
                    scale.x = scale.y * _aspectRatio;

                    // スケール調整
                    scale *= timeline.videoFrontmostScale;
                    transform.localScale = scale;

                    // 位置調整
                    var position = camera.transform.position;
                    position += camera.transform.forward * distanceFromCamera;
                    transform.position = position;
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
            if (mediaControl != null)
            {
                mediaControl.SetVolume(timeline.videoVolume);
                _mediaPlayer.m_Muted = timeline.videoVolume == 0f;
            }
        }

        public void UpdateSpeed()
        {
            if (mediaControl != null)
            {
                var playbackRate = (_isAnmPlaying && !isSeeking) ? timelineManager.anmSpeed : 0f;
                mediaControl.SetPlaybackRate(playbackRate);
            }
        }

        public void UpdateSeekTime()
        {
            if (!_isStarted)
            {
                return;
            }

            if (mediaControl != null)
            {
                var seekTimeMs = this.targetSeekTimeMs;
                var playingTimeMs = this.playingTimeMs;
                if (Mathf.Abs(seekTimeMs - playingTimeMs) > 10f)
                {
                    if (currentLayer.isAnmPlaying)
                    {
                        var halfPrebufferTimeMs = config.videoPrebufferTime * 500f;
                        mediaControl.SeekWithTolerance(
                            seekTimeMs + halfPrebufferTimeMs,
                            halfPrebufferTimeMs,
                            halfPrebufferTimeMs);
                    }
                    else
                    {
                        mediaControl.Seek(seekTimeMs);
                    }
                    _seekState = SeekState.Seeking;
                    UpdateSpeed();
                }

                _prevTime = currentTime;
            }
        }

        private IEnumerator UpdateSeekTimeAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            UpdateSeekTime();
        }

        private Color videoColor
        {
            get
            {
                var color = Color.white;
                if (isDisplayOnGUI)
                {
                    color.a = timeline.videoGUIAlpha;
                }
                else if (isDisplayBackmost)
                {
                    color.a = timeline.videoBackmostAlpha;
                }
                else if (isDisplayFrontmost)
                {
                    color.a = timeline.videoFrontmostAlpha;
                }
                else
                {
                    color.a = timeline.videoAlpha;
                }
                return color;
            }
        }

        public void UpdateColor()
        {
            if (isDisplayOnGUI)
            {
                if (_displayIMGUI != null)
                {
                    var color = videoColor;
                    _displayIMGUI._alphaBlend = color.a != 1f;
                    _displayIMGUI._color = color;
                }
            }
            else
            {
                if (_meshRenderer != null)
                {
                    var color = videoColor;
                    _meshRenderer.material.SetColor("_Color", color);
                    _meshRenderer.material.SetFloat("_ZWrite", (color.a == 1f) ? 1f : 0f);
                    _meshRenderer.sortingOrder = GetSortingOrder();
                }
            }
        }

        private int GetSortingOrder()
        {
            if (isDisplayBackmost)
            {
                return 0;
            }
            if (isDisplayFrontmost)
            {
                return 9001;
            }
            if (videoColor.a < 1f)
            {
                return 3000;
            }
            return 0;
        }

        private IEnumerator UpdateColorAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            UpdateColor();
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

        public void UpdateShader()
        {
            var shader = Shader.Find(config.videoShaderName);
            if (shader == null)
            {
                MTEUtils.LogError("MoviePlayerImpl：シェーダーが見つかりませんでした：" + name);
                return;
            }

            if (_meshRenderer == null || _applyToMaterial == null)
            {
                return;
            }

            Material videoMaterial = new Material(shader);
            _meshRenderer.material = videoMaterial;
            _applyToMaterial.Material = videoMaterial;
        }

        private bool IsGridVisible()
        {
            if (_gridMaterial == null || studioHack == null)
            {
                return false;
            }
            if (!config.isGridVisible || !config.isGridVisibleInVideo || isDisplayOnGUI)
            {
                return false;
            }
            if (config.isGridVisibleOnlyEdit && !studioHackManager.isPoseEditing)
            {
                return false;
            }

            return true;
        }

        public void OnRenderObject()
        {
            if (!IsGridVisible())
            {
                return;
            }

            if (Camera.current != targetCamera)
            {
                return;
            }

            _gridMaterial.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);

            GL.Begin(GL.LINES);

            Color gridColor = config.gridColorInVideo;
            gridColor.a = config.gridAlpha;
            GL.Color(gridColor);

            int gridCount = config.gridCount;
            float cellSize = 1f / gridCount;
            float half = 0.5f;

            var offset = new Vector3(0, 0, 0);
            if (isDisplayBackmost)
            {
                offset = new Vector3(-timeline.videoBackmostPosition.x, timeline.videoBackmostPosition.y, 0);
            }
            else if (isDisplayFrontmost)
            {
                offset = new Vector3(timeline.videoFrontmostPosition.x, timeline.videoFrontmostPosition.y, 0);
            }
            else
            {
                offset = new Vector3(0, 0.5f, 0);
            }

            System.Action<Vector3, Vector3> drawLine = (start, end) =>
            {
                start += offset;
                end += offset;

                GL.Vertex3(start.x, start.y, start.z);
                GL.Vertex3(end.x, end.y, end.z);
            }; 

            // 縦線を描画
            for (int i = 0; i <= gridCount; i++)
            {
                float x = i * cellSize - half;
                drawLine(new Vector3(x, -half, 0), new Vector3(x, half, 0));
            }

            // 横線を描画
            for (int j = 0; j <= gridCount; j++)
            {
                float y = j * cellSize - half;
                drawLine(new Vector3(-half, y, 0), new Vector3(half, y, 0));
            }

            GL.End();
            GL.PopMatrix();
        }

        private void OnVideoEvent(
            MediaPlayer mp,
            MediaPlayerEvent.EventType et,
            ErrorCode errorCode)
        {
            //MTEUtils.LogDebug("MoviePlayer：EventType：" + et.ToString());

            if (errorCode != ErrorCode.None)
            {
                MTEUtils.LogError("MoviePlayer：エラー EventType：" + et.ToString() + "  ErrorCode：" + errorCode.ToString());
                return;
            }

            if (et == MediaPlayerEvent.EventType.Started)
            {
                _isStarted = true;
                StartCoroutine(UpdateSeekTimeAfterDelay(0.5f));
                StartCoroutine(UpdateColorAfterDelay(0.5f));
                return;
            }

            if (et == MediaPlayerEvent.EventType.FinishedSeeking)
            {
                _seekState = SeekState.Adjusting;
                UpdateSpeed();
                return;
            }

            if (et == MediaPlayerEvent.EventType.MetaDataReady)
            {
                _aspectRatio = (float)_mediaPlayer.Info.GetVideoWidth() / _mediaPlayer.Info.GetVideoHeight();
                _duration = _mediaPlayer.Info.GetDurationMs() / 1000f;
                _frameRate = _mediaPlayer.Info.GetVideoFrameRate();
                _metaUpdated = true;
                return;
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
            else if (isDisplayFrontmost)
            {
                var vertices = new Vector3[] {
                    new Vector3(0.5f, -0.5f, 0),
                    new Vector3(-0.5f, -0.5f, 0),
                    new Vector3(0.5f, 0.5f, 0),
                    new Vector3(-0.5f, 0.5f, 0),
                };

                for (var i = 0; i < vertices.Length; i++)
                {
                    var vertex = vertices[i];
                    vertex.x += timeline.videoFrontmostPosition.x;
                    vertex.y += timeline.videoFrontmostPosition.y;
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