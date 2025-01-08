using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public enum VideoDisplayType
    {
        GUI,
        Mesh,
        Backmost,
        Frontmost,
    }

    public class MovieManager : ManagerBase
    {
        private MoviePlayerImpl _moviePlayerImpl = null;

        private VideoDisplayType _videoDisplayType = VideoDisplayType.GUI;
        private string _loadedVideoPath = "";

        private static MovieManager _instance;
        public static MovieManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MovieManager();
                }
                return _instance;
            }
        }

        private string videoPath
        {
            get => timeline != null ? timeline.videoPath : "";
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
            get => isValidPath && timeline.videoEnabled;
        }

        public float currentTime
        {
            get => _moviePlayerImpl != null ? _moviePlayerImpl.currentTime : 0f;
        }

        public float duration
        {
            get => _moviePlayerImpl != null ? _moviePlayerImpl.duration : 0f;
        }

        public float frameRate
        {
            get => _moviePlayerImpl != null ? _moviePlayerImpl.frameRate : 0f;
        }

        private MovieManager()
        {
        }

        public override void Init()
        {
            TimelineManager.onStop += UpdateSeekTime;
            TimelineManager.onAnmSpeedChanged += UpdateSpeed;
            TimelineManager.onSeekCurrentFrame += UpdateSeekTime;
        }

        private void SetupImpl()
        {
            if (_videoDisplayType != timeline.videoDisplayType)
            {
                UnloadMovie();
                _videoDisplayType = timeline.videoDisplayType;
            }

            if (!isEnabled)
            {
                return;
            }

            if (_moviePlayerImpl == null)
            {
                var guid = System.Guid.NewGuid().ToString();
                var gameObject = new GameObject("MoviePlayer_" + guid);
                _moviePlayerImpl = gameObject.AddComponent<MoviePlayerImpl>();
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

            SetupImpl();

            if (_moviePlayerImpl != null)
            {
                _moviePlayerImpl.LoadMovie(videoPath);
            }
        }

        public void UnloadMovie()
        {
            if (_moviePlayerImpl != null)
            {
                Object.Destroy(_moviePlayerImpl.gameObject);
                _moviePlayerImpl = null;
            }
            _loadedVideoPath = "";
        }

        public void ReloadMovie()
        {
            UnloadMovie();
            LoadMovie();
        }

        public void UpdateTransform()
        {
            if (_moviePlayerImpl != null)
            {
                _moviePlayerImpl.UpdateTransform();
            }
        }

        public void UpdateVolume()
        {
            if (_moviePlayerImpl != null)
            {
                _moviePlayerImpl.UpdateVolume();
            }
        }

        public void UpdateSpeed()
        {
            if (_moviePlayerImpl != null)
            {
                _moviePlayerImpl.UpdateSpeed();
            }
        }

        public void UpdateSeekTime()
        {
            if (_moviePlayerImpl != null)
            {
                _moviePlayerImpl.UpdateSeekTime();
            }
        }

        public void UpdateColor()
        {
            if (_moviePlayerImpl != null)
            {
                _moviePlayerImpl.UpdateColor();
            }
        }

        public void UpdateMesh()
        {
            if (_moviePlayerImpl != null)
            {
                _moviePlayerImpl.UpdateMesh();
            }
        }

        public void UpdateShader()
        {
            if (_moviePlayerImpl != null)
            {
                _moviePlayerImpl.UpdateShader();
            }
        }

        public override void OnLoad()
        {
            ReloadMovie();
        }

        public override void OnPluginDisable()
        {
            UnloadMovie();
        }
    }
}