using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MoviePlayer
    {
        private MoviePlayerImpl _moviePlayerImpl = null;

        private bool _isDisplayOnGUI = false;
        private string _loadedVideoPath = "";

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

        private MoviePlayer()
        {
            timelineManager.onRefresh += ReloadMovie;
            timelineManager.onAnmSpeedChanged += UpdateSpeed;
            timelineManager.onSeekCurrentFrame += UpdateSeekTime;
        }

        private void SetupImpl()
        {
            if (_isDisplayOnGUI != timeline.videoDisplayOnGUI)
            {
                UnloadMovie();
                _isDisplayOnGUI = timeline.videoDisplayOnGUI;
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

        public void Update()
        {
            if (!isEnabled)
            {
                return;
            }

            if (_moviePlayerImpl != null)
            {
                _moviePlayerImpl.Update();
            }
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
    }
}