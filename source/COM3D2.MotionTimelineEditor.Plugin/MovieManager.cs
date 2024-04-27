using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public enum VideoDisplayType
    {
        GUI,
        Mesh,
        Backmost,
    }

    public class MovieManager
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
                return _moviePlayerImpl != null ? _moviePlayerImpl.currentTime : 0f;
            }
        }

        public float duration
        {
            get
            {
                return _moviePlayerImpl != null ? _moviePlayerImpl.duration : 0f;
            }
        }

        public float frameRate
        {
            get
            {
                return _moviePlayerImpl != null ? _moviePlayerImpl.frameRate : 0f;
            }
        }

        private MovieManager()
        {
        }

        public void Init()
        {
            TimelineManager.onRefresh += ReloadMovie;
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
    }
}