using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ManagerBase : IManager
    {
        public virtual Maid maid => maidManager.maid;
        public virtual MaidCache maidCache => maidManager.maidCache;
        public virtual Config config => ConfigManager.instance.config;
        public virtual MainWindow mainWindow => windowManager.mainWindow;
        public virtual TimelineData timeline => timelineManager.timeline;
        public virtual ITimelineLayer currentLayer => timelineManager.currentLayer;
        public virtual ITimelineLayer defaultLayer => timeline.defaultLayer;
        public virtual IPartsEditHack partsEditHack => partsEditHackManager.partsEditHack;
        public virtual StudioHackBase studioHack => StudioHackManager.instance.studioHack;

        protected static MotionTimelineEditor mte => MotionTimelineEditor.instance;
        protected static MaidManager maidManager => MaidManager.instance;
        protected static StudioHackManager studioHackManager => StudioHackManager.instance;
        protected static ModelHackManager modelHackManager => ModelHackManager.instance;
        protected static PartsEditHackManager partsEditHackManager => PartsEditHackManager.instance;
        protected static LightHackManager lightHackManager => LightHackManager.instance;

        protected static TimelineManager timelineManager => TimelineManager.instance;
        protected static TimelineLoadManager timelineLoadManager => TimelineLoadManager.instance;
        protected static TimelineHistoryManager historyManager => TimelineHistoryManager.instance;

        protected static StudioModelManager modelManager => StudioModelManager.instance;
        protected static BGModelManager bgModelManager => BGModelManager.instance;
        protected static StudioLightManager lightManager => StudioLightManager.instance;
        protected static StageLaserManager stageLaserManager => StageLaserManager.instance;
        protected static StageLightManager stageLightManager => StageLightManager.instance;
        protected static PsylliumManager psylliumManager => PsylliumManager.instance;
        protected static BGMManager bgmManager => BGMManager.instance;
        protected static MovieManager movieManager => MovieManager.instance;
        protected static ConfigManager configManager => ConfigManager.instance;
        protected static CameraManager cameraManager => CameraManager.instance;
        protected static GridViewManager gridViewManager => GridViewManager.instance;
        protected static PostEffectManager postEffectManager => PostEffectManager.instance;

        protected static TimelineBundleManager bundleManager => TimelineBundleManager.instance;
        protected static WindowManager windowManager => WindowManager.instance;
        protected static BoneMenuManager boneMenuManager => BoneMenuManager.Instance;
        protected static PhotoBGManager photoBGManager => PhotoBGManager.instance;
        protected static BackgroundCustomManager backgroundCustomManager => BackgroundCustomManager.instance;

        public virtual void Init()
        {
        }

        public virtual void PreUpdate()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void LateUpdate()
        {
        }

        public virtual void OnLoad()
        {
        }

        public virtual void OnPluginDisable()
        {
        }

        public virtual void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
        }
    }
}