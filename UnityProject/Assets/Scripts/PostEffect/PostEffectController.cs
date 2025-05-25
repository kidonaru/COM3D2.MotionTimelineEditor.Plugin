using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class PostEffectController : MonoBehaviour
	{
		[SerializeField]
		public PostEffectContext context = new PostEffectContext();

		private List<PostEffectModelBase> _models = new List<PostEffectModelBase>();

		private Dictionary<CameraEvent, Material> _materials = new Dictionary<CameraEvent, Material>();
		private Dictionary<CameraEvent, CommandBuffer> _commandBuffers = new Dictionary<CameraEvent, CommandBuffer>();

#if COM3D2
		private static TimelineBundleManager bundleManager => TimelineBundleManager.instance;
#endif

		void Awake()
		{
			_models.Add(new ColorParaffinEffectModel());
			_models.Add(new DistanceFogEffectModel());
			_models.Add(new RimlightEffectModel());
		}

		void OnEnable()
		{
			context.camera = GetComponent<Camera>();

			InitMaterial();
			InitCommandBuffer();
		}

		void OnDisable()
		{
			DeleteMaterial();
			DeleteCommandBuffer();

			foreach (var model in _models)
			{
				model.Dispose();
			}
		}

		public static Material LoadMaterial(string materialName)
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

			foreach (var model in _models)
			{
				var cameraEvent = model.cameraEvent;
				if (!_materials.ContainsKey(cameraEvent))
				{
					_materials[cameraEvent] = LoadMaterial("PostEffect");
				}
			}
		}

        private void DeleteMaterial()
        {
            if (_materials != null)
			{
				foreach (var material in _materials.Values)
				{
					if (material != null)
					{
						DestroyImmediate(material);
					}
				}
				_materials.Clear();
			}
        }

		private void InitCommandBuffer()
		{
			DeleteCommandBuffer();

			foreach (var model in _models)
			{
				var cameraEvent = model.cameraEvent;
				if (!_commandBuffers.ContainsKey(cameraEvent))
				{
					var buffer = new CommandBuffer();
					buffer.name = "PostEffect_" + cameraEvent;
					context.camera.AddCommandBuffer(cameraEvent, buffer);
					_commandBuffers.Add(cameraEvent, buffer);
				}
			}
		}

		private void DeleteCommandBuffer()
		{
			if (_commandBuffers != null)
			{
				foreach (var pair in _commandBuffers)
				{
					var cameraEvent = pair.Key;
					var buffer = pair.Value;
					if (context.camera != null)
					{
						context.camera.RemoveCommandBuffer(cameraEvent, buffer);
					}
					buffer.Release();
				}
				_commandBuffers.Clear();
			}
		}

		public int GetActiveModelCount(CameraEvent cameraEvent)
		{
			var count = 0;

			foreach (var model in _models)
			{
				if (model.cameraEvent == cameraEvent && model.active)
				{
					++count;
				}
			}

			return count;
		}

		void OnPreRender()
		{
			foreach (var buffer in _commandBuffers.Values)
			{
				buffer.Clear();
			}

			foreach (var model in _models)
			{
				model.Init(context);
				model.OnPreRender();
			}

			foreach (var pair in _commandBuffers)
			{
				var cameraEvent = pair.Key;
				var buffer = pair.Value;
				var activeModelCount = GetActiveModelCount(cameraEvent);
				Material material;

				if (activeModelCount > 0 && _materials.TryGetValue(cameraEvent, out material))
				{
					bool isDebugView = false;
					bool isExtraBlend = false;

					foreach (var model in _models)
					{
						if (model.cameraEvent == cameraEvent)
						{
							model.Prepare(material);
							isDebugView |= model.isDebugView;
							isExtraBlend |= model.isExtraBlend;
						}
					}

					PostEffectModelBase.SetKeyword(material, "DEBUG_VIEW", isDebugView);
					PostEffectModelBase.SetKeyword(material, "EXTRA_BLEND", isExtraBlend);

					buffer.GetTemporaryRT(Uniforms._TempRT, -1, -1, 24, FilterMode.Bilinear);
					buffer.Blit(BuiltinRenderTextureType.CameraTarget, Uniforms._TempRT);
					buffer.Blit(Uniforms._TempRT, BuiltinRenderTextureType.CameraTarget, material);
					buffer.ReleaseTemporaryRT(Uniforms._TempRT);
				}
			}
		}

		private static class Uniforms
		{
			internal static readonly int _TempRT = Shader.PropertyToID("_TempRT");
		}

		/*void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (activeModelCount == 0)
			{
				Graphics.Blit(source, destination);
				return;
			}

			foreach (var model in _commonModels)
			{
				model.Prepare(_material);
			}

			Graphics.Blit(source, destination, _material);
		}*/
	}
}