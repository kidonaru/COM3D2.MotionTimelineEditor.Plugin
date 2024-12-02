using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class PostEffectController : MonoBehaviour
	{
		[SerializeField]
		public PostEffectContext context = new PostEffectContext();

		private List<PostEffectModelBase> _models = new List<PostEffectModelBase>();

		private Material _material = null;

		public int activeModelCount
		{
			get
			{
				var count = 0;

				foreach (var model in _models)
				{
					if (model.active)
					{
						++count;
					}
				}

				return count;
			}
		}

#if COM3D2
		private static TimelineBundleManager bundleManager
		{
			get
			{
				return TimelineBundleManager.instance;
			}
		}
#endif

		void Awake()
		{
			_models.Add(new ColorParaffinEffectModel());
		}

		void OnEnable()
		{
			InitMaterial();
		}

		void OnDisable()
		{
			DeleteMaterial();

			foreach (var model in _models)
			{
				model.Dispose();
			}
		}

		private Material LoadMaterial(string materialName)
		{
#if COM3D2
			var material = bundleManager.LoadMaterial(materialName);
#else
			var material = new Material(Shader.Find("MTE/" + materialName));
#endif
			material.hideFlags = HideFlags.HideAndDontSave;
			return material;
		}

        private void InitMaterial()
        {
            DeleteMaterial();

			_material = LoadMaterial("PostEffect");
		}

        private void DeleteMaterial()
        {
            if (_material != null)
            {
                DestroyImmediate(_material);
                _material = null;
            }
        }

		void OnPreCull()
		{
			context.camera = GetComponent<Camera>();

			foreach (var model in _models)
			{
				model.Init(context);
				model.OnPreCull();
			}
		}

		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (activeModelCount == 0)
			{
				Graphics.Blit(source, destination);
				return;
			}

			foreach (var model in _models)
			{
				model.Prepare(_material);
			}

			Graphics.Blit(source, destination, _material);
		}
	}
}