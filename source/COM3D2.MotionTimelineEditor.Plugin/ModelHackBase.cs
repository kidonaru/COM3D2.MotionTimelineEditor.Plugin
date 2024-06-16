using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface IModelHack
    {
        string pluginName { get; }
        List<StudioModelStat> modelList { get; }
        string errorMessage { get; }

        bool Init();
        void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode);
        bool IsValid();

        void DeleteAllModels();
        void DeleteModel(StudioModelStat model);
        void CreateModel(StudioModelStat model);
        void UpdateAttachPoint(StudioModelStat model);
        void SetModelVisible(StudioModelStat model, bool visible);
    }

    public abstract class ModelHackBase : IModelHack
    {
        public abstract string pluginName { get; }

        public abstract List<StudioModelStat> modelList { get; }

        protected string _errorMessage = "";
        public string errorMessage
        {
            get
            {
                return _errorMessage;
            }
        }

        protected static MaidManager maidManager
        {
            get
            {
                return MaidManager.instance;
            }
        }

        protected static CacheBoneDataArray cacheBoneData
        {
            get
            {
                return maidManager.cacheBoneData;
            }
        }

        protected static Animation animation
        {
            get
            {
                return maidManager.animation;
            }
        }

        protected static AnimationState animationState
        {
            get
            {
                return maidManager.animationState;
            }
        }

        protected static StudioModelManager modelManager
        {
            get
            {
                return StudioModelManager.instance;
            }
        }

        protected ModelHackBase()
        {
        }

        public virtual bool Init()
        {
            return true;
        }

        public virtual void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            // do nothing
        }

        public virtual bool IsValid()
        {
            _errorMessage = "";
            return true;
        }

        public virtual void DeleteAllModels()
        {
            // do nothing
        }

        public virtual void DeleteModel(StudioModelStat model)
        {
            // do nothing
        }

        public virtual void CreateModel(StudioModelStat model)
        {
            // do nothing
        }

        public virtual void UpdateAttachPoint(StudioModelStat model)
        {
            // do nothing
        }

        public virtual void SetModelVisible(StudioModelStat model, bool visible)
        {
            if (model.transform != null)
            {
                 model.transform.gameObject.SetActive(visible);
            }
        }
    }
}