using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class DistanceFogEffectModel : PostEffectModelBase
	{
		public static readonly int MAX_FOG_COUNT = 1;

		[System.Serializable]
		public struct DistanceFogBuffer
		{
			public Color color1;
			public Color color2;
			public float fogStart;
			public float fogEnd;
			public float fogExp;
			public float useNormal;
			public float useAdd;
			public float useMultiply;
			public float useOverlay;
			public float useSubstruct;
		}

		private DistanceFogBuffer[] _fogBuffers = new DistanceFogBuffer[MAX_FOG_COUNT];
		private int _enabledCount = 0;
		private bool _enableExtraBlend = false;
		private ComputeBuffer _computeBuffer = null;

		public override bool active
		{
			get
			{
				return settings.enabled && _enabledCount > 0;
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

		public override bool isExtraBlend
		{
			get
			{
				return _enableExtraBlend;
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
			if (_computeBuffer != null)
			{
				_computeBuffer.Release();
				_computeBuffer = null;
			}
		}

		public override void OnPreRender()
		{
			if (_computeBuffer == null)
			{
				_computeBuffer = new ComputeBuffer(MAX_FOG_COUNT, sizeof(float) * 16);
			}

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

			_computeBuffer.SetData(_fogBuffers);

			material.SetBuffer(Uniforms._DistanceFogBuffer, _computeBuffer);

			material.EnableKeyword("DISTANCE_FOG");
		}

		private static class Uniforms
		{
			internal static readonly int _DistanceFogBuffer = Shader.PropertyToID("_DistanceFogBuffer");
		}

		private void BuildDistanceFogBuffers()
		{
			_enabledCount = 0;
			_enableExtraBlend = false;

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

				if (_enabledCount >= MAX_FOG_COUNT)
				{
					Debug.LogError("Too many fog effects.");
					break;
				}

				if (data.useAdd > 0f || data.useMultiply > 0f || data.useOverlay > 0f || data.useSubstruct > 0f)
				{
					_enableExtraBlend = true;
				}

				_fogBuffers[_enabledCount] = ConvertToBuffer(data);
				++_enabledCount;
			}
		}

		private DistanceFogBuffer ConvertToBuffer(DistanceFogData data)
		{
			var fogStart = data.fogStart;
			var fogEnd = data.fogEnd;

			// 0割り対策
			if (fogStart == fogEnd)
			{
				fogEnd += 0.001f;
			}

			return new DistanceFogBuffer
			{
				color1 = data.color1,
				color2 = data.color2,
				fogStart = fogStart,
				fogEnd = fogEnd,
				fogExp = data.fogExp,
				useNormal = data.useNormal,
				useAdd = data.useAdd,
				useMultiply = data.useMultiply,
				useOverlay = data.useOverlay,
				useSubstruct = data.useSubstruct,
			};
		}
	}
}