using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ColorParaffinEffectModel : PostEffectModelBase
	{
		public static readonly int MAX_PARAFFIN_COUNT = 4;

		[System.Serializable]
		public struct ParaffinBuffer
		{
			public Color color1;
			public Color color2;
			public Vector2 centerPosition;
			public float radiusFar;
			public float radiusNear;
			public Vector2 radiusScale;
			public float useNormal;
			public float useAdd;
			public float useMultiply;
			public float useOverlay;
			public float useSubstruct;
			public float depthMin;
			public float depthMax;
			public float depthFade;
		}

		private ParaffinBuffer[] _paraffinBuffers = new ParaffinBuffer[MAX_PARAFFIN_COUNT];
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
				return CameraEvent.BeforeImageEffects;
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

		public ColorParaffinEffectSettings settings
		{
			get
			{
				return context.paraffinSettings;
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
				_computeBuffer = new ComputeBuffer(MAX_PARAFFIN_COUNT, sizeof(float) * 24);
			}

			camera.depthTextureMode |= DepthTextureMode.Depth;

			BuildParaffinBuffers();
		}

		public override void Prepare(Material material)
		{
			if (!active)
			{
				material.DisableKeyword("PARAFFIN");
				return;
			}

			_computeBuffer.SetData(_paraffinBuffers);

			material.SetBuffer(Uniforms._ParaffinBuffer, _computeBuffer);
			material.SetInt(Uniforms._ParaffinCount, _enabledCount);

			material.EnableKeyword("PARAFFIN");
		}

		private static class Uniforms
		{
			internal static readonly int _ParaffinBuffer = Shader.PropertyToID("_ParaffinBuffer");
			internal static readonly int _ParaffinCount = Shader.PropertyToID("_ParaffinCount");
		}

		private void BuildParaffinBuffers()
		{
			_enabledCount = 0;
			_enableExtraBlend = false;

			if (!settings.enabled)
			{
				return;
			}

			for (int i = 0; i < settings.dataList.Count; i++)
			{
				if (_enabledCount >= MAX_PARAFFIN_COUNT)
				{
					Debug.LogError("Too many paraffin effects. Max count is " + MAX_PARAFFIN_COUNT);
					break;
				}

				var data = settings.dataList[i];
				if (!data.enabled)
				{
					continue;
				}

				if (data.useNormal > 0f || data.useMultiply > 0f || data.useOverlay > 0f || data.useSubstruct > 0f)
				{
					_enableExtraBlend = true;
				}

				_paraffinBuffers[_enabledCount] = ConvertToBuffer(data);
				++_enabledCount;
			}
		}

        private Vector2 AdjustUV(Vector2 scale, Vector2 pos)
        {
            var centeredUV = pos - new Vector2(0.5f, 0.5f);
            centeredUV.x *= scale.x;
            centeredUV.y *= scale.y;
            return centeredUV + new Vector2(0.5f, 0.5f);
        }

		private ParaffinBuffer ConvertToBuffer(ColorParaffinData data)
		{
			var buffer = new ParaffinBuffer();

			float screenAspect = (float)Screen.width / Screen.height;
			var aspectScale = new Vector2(screenAspect, 1f);

			if (data.radiusScale.x > 0f)
			{
				aspectScale.x /= data.radiusScale.x;
			}
			else
			{
				aspectScale.x *= 10000f;
			}

			if (data.radiusScale.y > 0f)
			{
				aspectScale.y /= data.radiusScale.y;
			}
			else
			{
				aspectScale.y *= 10000f;
			}

			var depthMin = data.depthMin;
			var depthMax = data.depthMax == 0f ? camera.farClipPlane * 2f : data.depthMax;
			var depthFade = data.depthFade;

			buffer.color1 = data.color1;
			buffer.color2 = data.color2;
			buffer.centerPosition = AdjustUV(aspectScale, data.centerPosition);
			buffer.radiusFar = data.radiusFar;
			buffer.radiusNear = data.radiusNear;
			buffer.radiusScale = aspectScale;
			buffer.depthMin = depthMin;
			buffer.depthMax = depthMax;
			buffer.depthFade = depthFade;
			buffer.useNormal = data.useNormal;
			buffer.useAdd = data.useAdd;
			buffer.useMultiply = data.useMultiply;
			buffer.useOverlay = data.useOverlay;
			buffer.useSubstruct = data.useSubstruct;

			return buffer;
		}
	}
}