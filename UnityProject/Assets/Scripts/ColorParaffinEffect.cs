using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace COM3D2.MotionTimelineEditor.Plugin
{
	[System.Serializable]
	public class ParaffinData
	{
		public bool enabled = false;
		public Color color1 = new Color(0.68f, 0.34f, 0f, 1f);
		public Color color2 = new Color(0.68f, 0.34f, 0f, 0f);

		public Vector2 centerPosition = new Vector2(0.5f, 1.0f);
		[Range(0f, 1f)]
		public float radiusFar = 1f;
		[Range(0f, 1f)]
		public float radiusNear = 0f;
		public Vector2 radiusScale = new Vector2(1f, 1f);
		public float depthMin = 0f;
		public float depthMax = 0f;
		public float depthFade = 0f;

		[Header("Blend Mode")]
		public float useNormal = 0f;
		public float useAdd = 1f;
		public float useMultiply = 0f;
		public float useOverlay = 0f;
		public float useSubstruct = 0f;

		public void CopyFrom(ParaffinData data)
		{
			enabled = data.enabled;
			color1 = data.color1;
			color2 = data.color2;
			centerPosition = data.centerPosition;
			radiusFar = data.radiusFar;
			radiusNear = data.radiusNear;
			radiusScale = data.radiusScale;
			depthMin = data.depthMin;
			depthMax = data.depthMax;
			depthFade = data.depthFade;
			useNormal = data.useNormal;
			useAdd = data.useAdd;
			useMultiply = data.useMultiply;
			useOverlay = data.useOverlay;
			useSubstruct = data.useSubstruct;
		}

		public static ParaffinData Lerp(
            ParaffinData a,
            ParaffinData b,
            float t)
        {
			return new ParaffinData
			{
				enabled = a.enabled,
				color1 = Color.Lerp(a.color1, b.color1, t),
				color2 = Color.Lerp(a.color2, b.color2, t),
				centerPosition = Vector2.Lerp(a.centerPosition, b.centerPosition, t),
				radiusFar = Mathf.Lerp(a.radiusFar, b.radiusFar, t),
				radiusNear = Mathf.Lerp(a.radiusNear, b.radiusNear, t),
				radiusScale = Vector2.Lerp(a.radiusScale, b.radiusScale, t),
				depthMin = Mathf.Lerp(a.depthMin, b.depthMin, t),
				depthMax = Mathf.Lerp(a.depthMax, b.depthMax, t),
				depthFade = Mathf.Lerp(a.depthFade, b.depthFade, t),
				useNormal = Mathf.Lerp(a.useNormal, b.useNormal, t),
				useAdd = Mathf.Lerp(a.useAdd, b.useAdd, t),
				useMultiply = Mathf.Lerp(a.useMultiply, b.useMultiply, t),
				useOverlay = Mathf.Lerp(a.useOverlay, b.useOverlay, t),
				useSubstruct = Mathf.Lerp(a.useSubstruct, b.useSubstruct, t),
			};
		}
	}

	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class ColorParaffinEffect : MonoBehaviour
	{
		[SerializeField]
		private List<ParaffinData> paraffinDataList = new List<ParaffinData>();

        [Header("Aspect Ratio")]
        public float targetAspect = 0f;

        [Header("Debug")]
        [SerializeField]
        public bool isDebug = false;

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

		private Material _material = null;
		private Material _materialDepth = null;
		private Material _materialDebug = null;
		private ParaffinBuffer[] _paraffinBuffers = new ParaffinBuffer[MAX_PARAFFIN_COUNT];
		private int _enabledCount = 0;
		private bool _enableDepth = false;
		private ComputeBuffer _computeBuffer = null;
		private Camera _camera = null;

		private Material activeMaterial
		{
			get
			{
				if (isDebug)
				{
					return _materialDebug;
				}
				if (_enableDepth)
				{
					return _materialDepth;
				}
				return _material;
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

		void OnEnable()
		{
			_computeBuffer = new ComputeBuffer(MAX_PARAFFIN_COUNT, sizeof(float) * 24);
			_camera = GetComponent<Camera>();
			_camera.depthTextureMode |= DepthTextureMode.Depth;
			InitMaterial();
		}

		void OnDisable()
		{
			if (_computeBuffer != null)
			{
				_computeBuffer.Release();
				_computeBuffer = null;
			}
			DeleteMaterial();
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

			_material = LoadMaterial("ColorParaffin");
			_materialDepth = LoadMaterial("ColorParaffinDepth");
			_materialDebug = LoadMaterial("ColorParaffinDebug");
		}

        private void DeleteMaterial()
        {
            if (_material != null)
            {
                DestroyImmediate(_material);
                _material = null;
            }
			if (_materialDepth != null)
			{
				DestroyImmediate(_materialDepth);
				_materialDepth = null;
			}
			if (_materialDebug != null)
			{
				DestroyImmediate(_materialDebug);
				_materialDebug = null;
			}
        }

		public void LateUpdate()
		{
			BuildParaffinBuffers();
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (_enabledCount == 0)
			{
				Graphics.Blit(source, destination);
				return;
			}

			_computeBuffer.SetData(_paraffinBuffers);

			var material = activeMaterial;
			material.SetBuffer(Uniforms._ParaffinBuffer, _computeBuffer);
			material.SetInt(Uniforms._ParaffinCount, _enabledCount);

			Graphics.Blit(source, destination, material);
		}

		private static class Uniforms
		{
			internal static readonly int _ParaffinBuffer = Shader.PropertyToID("_ParaffinBuffer");
			internal static readonly int _ParaffinCount = Shader.PropertyToID("_ParaffinCount");
		}

		public ParaffinData GetParaffinData(int index)
		{
			if (index < 0 || index >= paraffinDataList.Count)
			{
				return null;
			}
			return paraffinDataList[index];
		}

		public void SetParaffinData(int index, ParaffinData data)
		{
			if (index < 0 || index >= paraffinDataList.Count)
			{
				return;
			}
			paraffinDataList[index].CopyFrom(data);
		}

		public void AddParaffinData(ParaffinData data)
		{
			paraffinDataList.Add(data);
		}

		public void RemoveParaffinData(int index)
		{
			if (index < 0 || index >= paraffinDataList.Count)
			{
				return;
			}
			paraffinDataList.RemoveAt(index);
		}

		public void RemoveLastParaffinData()
		{
			if (paraffinDataList.Count > 0)
			{
				paraffinDataList.RemoveAt(paraffinDataList.Count - 1);
			}
		}

		public int GetParaffinCount()
		{
			return paraffinDataList.Count;
		}

		public void ClearParaffinData()
		{
			paraffinDataList.Clear();
		}

		private void BuildParaffinBuffers()
		{
			_enabledCount = 0;
			_enableDepth = false;
			for (int i = 0; i < paraffinDataList.Count; i++)
			{
				if (_enabledCount >= MAX_PARAFFIN_COUNT)
				{
					Debug.LogError("Too many paraffin effects. Max count is " + MAX_PARAFFIN_COUNT);
					break;
				}

				var data = paraffinDataList[i];
				if (!data.enabled)
				{
					continue;
				}

				if (data.depthMin != 0f || data.depthMax != 0f)
				{
					_enableDepth = true;
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

		private ParaffinBuffer ConvertToBuffer(ParaffinData data)
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

			buffer.color1 = data.color1;
			buffer.color2 = data.color2;
			buffer.centerPosition = AdjustUV(aspectScale, data.centerPosition);
			buffer.radiusFar = data.radiusFar;
			buffer.radiusNear = data.radiusNear;
			buffer.radiusScale = aspectScale;
			buffer.depthMin = data.depthMin == 0f ? _camera.nearClipPlane : data.depthMin;
			buffer.depthMax = data.depthMax == 0f ? _camera.farClipPlane : data.depthMax;
			buffer.depthFade = data.depthFade;
			buffer.useNormal = data.useNormal;
			buffer.useAdd = data.useAdd;
			buffer.useMultiply = data.useMultiply;
			buffer.useOverlay = data.useOverlay;
			buffer.useSubstruct = data.useSubstruct;

			return buffer;
		}
	}
}