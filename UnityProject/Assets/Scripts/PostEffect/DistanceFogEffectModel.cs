using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class DistanceFogEffectModel : PostEffectModelBase
	{
		[System.Serializable]
		public struct DistanceFogBuffer
		{
			public Color color1;
			public Color color2;
			public float fogStart;
			public float fogEnd;
			public float fogExp;
		}

		private DistanceFogBuffer _fogBuffer = new DistanceFogBuffer();
		private bool _enabled = false;

		public override bool active
		{
			get
			{
				return settings.enabled && _enabled;
			}
		}

		public override CameraEvent cameraEvent
		{
			get
			{
				return CameraEvent.BeforeImageEffectsOpaque;
			}
		}

		public override bool isDebugView
		{
			get
			{
				return settings.isDebugView;
			}
		}

		public DistanceFogEffectSettings settings
		{
			get
			{
				return context.fogSettings;
			}
		}

		public override void Dispose()
		{
		}

		public override void OnPreRender()
		{
			camera.depthTextureMode |= DepthTextureMode.Depth;

			BuildDistanceFogBuffers();
		}

		public override void Prepare(Material material)
		{
			if (!active)
			{
				material.DisableKeyword("DISTANCE_FOG");
				return;
			}

			material.SetColor(Uniforms._DistanceFogColor1, _fogBuffer.color1);
			material.SetColor(Uniforms._DistanceFogColor2, _fogBuffer.color2);
			material.SetFloat(Uniforms._DistanceFogStart, _fogBuffer.fogStart);
			material.SetFloat(Uniforms._DistanceFogEnd, _fogBuffer.fogEnd);
			material.SetFloat(Uniforms._DistanceFogExp, _fogBuffer.fogExp);

			material.EnableKeyword("DISTANCE_FOG");
		}

		private static class Uniforms
		{
			internal static readonly int _DistanceFogColor1 = Shader.PropertyToID("_DistanceFogColor1");
			internal static readonly int _DistanceFogColor2 = Shader.PropertyToID("_DistanceFogColor2");
			internal static readonly int _DistanceFogStart = Shader.PropertyToID("_DistanceFogStart");
			internal static readonly int _DistanceFogEnd = Shader.PropertyToID("_DistanceFogEnd");
			internal static readonly int _DistanceFogExp = Shader.PropertyToID("_DistanceFogExp");
		}

		private void BuildDistanceFogBuffers()
		{
			_enabled = false;

			if (!settings.enabled)
			{
				return;
			}

			for (int i = 0; i < settings.dataList.Count; i++)
			{
				var data = settings.dataList[i];
				if (!data.enabled)
				{
					continue;
				}

				if (_enabled)
				{
					Debug.LogError("Too many fog effects.");
					break;
				}

				_fogBuffer = ConvertToBuffer(data);
				_enabled = true;
			}
		}

		private DistanceFogBuffer ConvertToBuffer(DistanceFogData data)
		{
			var buffer = new DistanceFogBuffer();

			var fogStart = data.fogStart;
			var fogEnd = data.fogEnd;

			// 0割り対策
			if (fogStart == fogEnd)
			{
				fogEnd += 0.001f;
			}

			buffer.color1 = data.color1;
			buffer.color2 = data.color2;
			buffer.fogStart = fogStart;
			buffer.fogEnd = fogEnd;
			buffer.fogExp = data.fogExp;

			return buffer;
		}
	}
}