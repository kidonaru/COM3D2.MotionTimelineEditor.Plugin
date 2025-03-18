
namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class NPRShaderHackManager : ManagerBase
    {
        private INPRShaderHack _nprShaderHack = null;
        public INPRShaderHack nprShaderHack => _nprShaderHack;

        private static NPRShaderHackManager _instance;
        public static NPRShaderHackManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NPRShaderHackManager();
                }

                return _instance;
            }
        }

        private NPRShaderHackManager()
        {
            ModelHackManager.onCreateModel += OnCreateModel;
        }

        public void Register(INPRShaderHack nprShaderHack)
        {
            if (nprShaderHack == null || !nprShaderHack.Init())
            {
                return;
            }

            _nprShaderHack = nprShaderHack;
        }

        private void OnCreateModel(StudioModelStat model)
        {
            if (nprShaderHack == null)
            {
                return;
            }

            if (model == null || model.transform == null)
            {
                return;
            }

            nprShaderHack.UpdateMaterial(model.transform.gameObject);
        }
    }
}