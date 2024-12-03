using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public abstract class PostEffectModelBase
	{
		public PostEffectContext context { get; private set; }

		public Camera camera
		{
			get
			{
				return context.camera;
			}
		}

		public abstract bool active { get; }

		public abstract CameraEvent cameraEvent { get; }

		public void Init(PostEffectContext context)
		{
			this.context = context;
		}

		public abstract void Dispose();

		public abstract void OnPreRender();

		public abstract void Prepare(Material material);

		protected void SetKeyword(Material material, string keyword, bool enable)
		{
			if (enable)
			{
				material.EnableKeyword(keyword);
			}
			else
			{
				material.DisableKeyword(keyword);
			}
		}
	}
}