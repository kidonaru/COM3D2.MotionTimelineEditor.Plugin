using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class RimlightEffectModel : PostEffectModelBase
	{
		public static readonly int MAX_RIMLIGHT_COUNT = 4;

		[System.Serializable]
		public struct RimlightBuffer
		{
			public Color color1;
			public Color color2;
			public Vector3 direction;
			public float lightArea;
			public float fadeRange;
			public float fadeExp;
			public float depthMin;
			public float depthMax;
			public float depthFade;
			public float useNormal;
			public float useAdd;
			public float useMultiply;
			public float useOverlay;
			public float useSubstruct;
			public float edgeDepth;
			public Vector2 edgeRange;
			public float heightMin;
		}

		private RimlightBuffer[] _rimlightBuffers = new RimlightBuffer[MAX_RIMLIGHT_COUNT];
		private int _enabledCount = 0;
		private bool _enableEdge = false;
		private bool _enableHeight = false;
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

		public RimlightEffectSettings settings
		{
			get
			{
				return context.rimlightSettings;
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
				_computeBuffer = new ComputeBuffer(MAX_RIMLIGHT_COUNT, sizeof(float) * 22);
			}

			camera.depthTextureMode |= DepthTextureMode.DepthNormals;

			BuildRimlightBuffers();
		}

		public override void Prepare(Material material)
		{
			if (!active)
			{
				material.DisableKeyword("RIMLIGHT");
				return;
			}

			_computeBuffer.SetData(_rimlightBuffers);

			material.SetBuffer(Uniforms._RimlightBuffer, _computeBuffer);
			material.SetInt(Uniforms._RimlightCount, _enabledCount);
    		material.SetMatrix(Uniforms._CameraToWorldMatrix, camera.worldToCameraMatrix.inverse);

			material.EnableKeyword("RIMLIGHT");
			SetKeyword(material, "RIMLIGHT_EDGE", _enableEdge);
			SetKeyword(material, "RIMLIGHT_HEIGHT", _enableHeight);
		}

		private static class Uniforms
		{
			internal static readonly int _RimlightBuffer = Shader.PropertyToID("_RimlightBuffer");
			internal static readonly int _RimlightCount = Shader.PropertyToID("_RimlightCount");
			internal static readonly int _CameraToWorldMatrix = Shader.PropertyToID("_CameraToWorldMatrix");
		}

		private void BuildRimlightBuffers()
		{
			_enabledCount = 0;
			_enableEdge = false;
			_enableHeight = false;

			if (!settings.enabled)
			{
				return;
			}

			for (int i = 0; i < settings.dataList.Count; i++)
			{
				if (_enabledCount >= MAX_RIMLIGHT_COUNT)
				{
					Debug.LogError("Too many rimlight effects. Max count is " + MAX_RIMLIGHT_COUNT);
					break;
				}

				var data = settings.dataList[i];
				if (!data.enabled)
				{
					continue;
				}

				if (data.edgeDepth > 0.0f && data.edgeRange > 0.0f)
				{
					_enableEdge = true;
				}

				if (data.heightMin > 0.0f)
				{
					_enableHeight = true;
				}

				_rimlightBuffers[_enabledCount] = ConvertToBuffer(data);
				++_enabledCount;
			}
		}

		private RimlightBuffer ConvertToBuffer(RimlightData data)
		{
			float maxDistance = camera.farClipPlane;
			float depthMin = Mathf.Clamp01(data.depthMin / maxDistance);
			float depthMax = data.depthMax == 0f ? camera.farClipPlane : data.depthMax;
			depthMax = Mathf.Clamp01(depthMax / maxDistance);
			float depthFade = Mathf.Clamp01(data.depthFade / maxDistance);

			var rotation = Quaternion.Euler(data.rotation);
    		Vector3 direction = rotation * Vector3.forward;

			if (data.isWorldSpace)
			{
				direction = camera.worldToCameraMatrix.MultiplyVector(direction);
    			direction = direction.normalized;
			}

			float screenAspect = (float)Screen.width / Screen.height;
			float edgeRangeScale = 0.01f;
			var edgeRange = new Vector2(data.edgeRange / screenAspect, data.edgeRange) * edgeRangeScale;

			if (data.edgeDepth == 0f || data.edgeRange == 0f)
			{
				edgeRange = new Vector2(1f, 1f);
			}

			float fadeRange = data.fadeRange * 0.5f;

			return new RimlightBuffer
			{
				color1 = data.color1,
				color2 = data.color2,
				direction = direction,
				lightArea = data.lightArea,
				fadeRange = fadeRange,
				fadeExp = data.fadeExp,
				depthMin = depthMin,
				depthMax = depthMax,
				depthFade = depthFade,
				useNormal = data.useNormal,
				useAdd = data.useAdd,
				useMultiply = data.useMultiply,
				useOverlay = data.useOverlay,
				useSubstruct = data.useSubstruct,
				edgeDepth = data.edgeDepth,
				edgeRange = edgeRange,
				heightMin = data.heightMin,
			};
		}
	}
}