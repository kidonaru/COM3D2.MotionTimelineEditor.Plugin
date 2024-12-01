using System;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BGMManager
    {
        private static BGMManager _instance = null;

        public static BGMManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BGMManager();
                }
                return _instance;
            }
        }

        private static TimelineManager timelineManager => TimelineManager.instance;

        private static TimelineData timeline => timelineManager.timeline;

        private static ITimelineLayer defaultLayer => timeline.defaultLayer;

        private static SoundMgr soundMgr => GameMain.Instance.SoundMgr;

        private AudioSourceMgr _audioMgr;
        private AudioClip _audioClip;
        private string _loadedBgmPath = "";
        private float _prevMotionTime = 0f;

        public int volumeDance
        {
            get => soundMgr.GetVolumeDance();
            set
            {
                soundMgr.SetVolumeDance(value);
                soundMgr.Apply();
                UpdateVolume();
            }
        }

        private BGMManager()
        {
        }

        public void Init()
        {
            SceneManager.sceneLoaded += OnChangedSceneLevel;
            TimelineManager.onPlay += OnPlay;
            TimelineManager.onRefresh += OnRefresh;
            TimelineManager.onAnmSpeedChanged += OnAnmSpeedChanged;
            TimelineManager.onSeekCurrentFrame += OnSeekCurrentFrame;
        }

        public bool Load()
        {
            if (timeline == null)
            {
                return false;
            }

            var bgmPath = timeline.bgmPath;

            if (_audioMgr == null)
            {
                var components = GameMain.Instance.MainCamera.gameObject.GetComponentsInChildren<AudioSourceMgr>();
                _audioMgr = components.FirstOrDefault(a => a.SoundType == AudioSourceMgr.Type.Bgm);
            }
            if (_audioMgr == null)
            {
                PluginUtils.LogError("AudioSourceMgrが見つかりません。");
                return false;
            }

            // 読み込み済み
            if (IsLoaded() && _loadedBgmPath == bgmPath)
            {
                return true;
            }

            Stop();

            if (string.IsNullOrEmpty(bgmPath))
            {
                return true;
            }

            string extension = Path.GetExtension(bgmPath);
            if (!File.Exists(bgmPath) || (extension != ".ogg" && extension != ".wav"))
            {
                PluginUtils.LogError(string.Format("{0}または{1}ファイルを指定してください。{2}", ".ogg", ".wav", bgmPath));
                return false;
            }

            using (var www = new WWW("file:///" + bgmPath))
            {
                int num = 0;
                while (!www.isDone)
                {
                    Thread.Sleep(100);
                    num += 100;
                    if (10000 < num)
                    {
                        PluginUtils.LogError("音声読込タイムアウトのため処理を中止します。");
                        return false;
                    }
                }

                var audioClip = www.GetAudioClip();
                if (audioClip.loadState == AudioDataLoadState.Loaded)
                {
                    _audioMgr.audiosource.clip = audioClip;
                    _audioMgr.audiosource.loop = false;
                    _audioMgr.audiosource.pitch = timelineManager.anmSpeed;
                    _audioClip = audioClip;
                    _loadedBgmPath = bgmPath;
                    PluginUtils.LogDebug("{0}を読み込みました。", Path.GetFileName(_loadedBgmPath));
                    return true;
                }
            }

            return false;
        }

        public void Reload()
        {
            Stop();
            Load();
        }

        public void Update()
        {
            if (!IsLoaded())
            {
                return;
            }

            var isMotionPlaying = defaultLayer.isMotionPlaying;
            var isAudioPlaying = _audioMgr.audiosource.isPlaying;
            if (isMotionPlaying && !isAudioPlaying)
            {
                Play();
            }
            else if (!isMotionPlaying && isAudioPlaying)
            {
                Pause();
            }

            var motionTime = defaultLayer.playingTime;
            if (motionTime < _prevMotionTime)
            {
                SeekPlayingTime();
            }
            _prevMotionTime = motionTime;
        }

        public void UpdateVolume()
        {
            if (IsLoaded())
            {
                _audioMgr.audiosource.outputAudioMixerGroup = soundMgr.mix_mgr[AudioMixerMgr.Group.Dance];
                _audioMgr.audiosource.mute = false;
                _audioMgr.audiosource.volume = volumeDance / 100f;
            }
        }

        public bool IsLoaded()
        {
            return _audioMgr != null && _audioClip != null && _audioMgr.audiosource.clip == _audioClip;
        }

        public void SeekPlayingTime()
        {
            if (IsLoaded())
            {
                var motionTime = defaultLayer.playingTime;
                _audioMgr.audiosource.time = motionTime + timeline.startOffsetTime;
                _prevMotionTime = motionTime;
            }
        }

        public float GetPlayingTime()
        {
            if (IsLoaded())
            {
                return _audioMgr.audiosource.time;
            }
            return 0f;
        }

        public void Play()
        {
            if (IsLoaded())
            {
                PluginUtils.LogDebug("{0}を再生します。", Path.GetFileName(_loadedBgmPath));
                GameMain.Instance.SoundMgr.StopBGM(0f);
                _audioMgr.audiosource.Play();
                SeekPlayingTime();
                UpdateVolume();
            }
        }

        public void Stop()
        {
            if (IsLoaded())
            {
                _audioMgr.audiosource.Stop();
                _audioMgr.audiosource.clip = null;
            }

            if (_audioClip != null)
            {
                UnityEngine.Object.Destroy(_audioClip);
                _audioClip = null;
            }

            _loadedBgmPath = "";
        }

        public void Pause()
        {
            if (IsLoaded())
            {
                _audioMgr.audiosource.Pause();
            }
        }

        public void Resume()
        {
            if (IsLoaded())
            {
                _audioMgr.audiosource.UnPause();
            }
        }

        private void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            Stop();
        }

        private void OnPlay()
        {
            Load();
        }

        private void OnRefresh()
        {
            Load();
        }

        private void OnAnmSpeedChanged()
        {
            if (IsLoaded())
            {
                _audioMgr.audiosource.pitch = timelineManager.anmSpeed;
            }
        }

        private void OnSeekCurrentFrame()
        {
            SeekPlayingTime();
        }
    }
}