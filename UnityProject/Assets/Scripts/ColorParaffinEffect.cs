using System;
using System.Collections.Generic;
using UnityEngine;

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
			useNormal = data.useNormal;
			useAdd = data.useAdd;
			useMultiply = data.useMultiply;
			useOverlay = data.useOverlay;
			useSubstruct = data.useSubstruct;
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
        private bool _isDebug = false;
        public bool isDebug
        {
            get
            {
                return _isDebug;
            }
            set
            {
                if (_isDebug == value)
                {
                    return;
                }
                _isDebug = value;

                InitMaterial();
            }
        }

		public static readonly int MAX_PARAFFIN_COUNT = 4;

		[System.Serializable]
		private struct ParaffinBuffer
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
			public float padding0;
		}

		private Material material = null;
		private ParaffinBuffer[] paraffinBuffers = new ParaffinBuffer[MAX_PARAFFIN_COUNT];
		private int enabledCount = 0;
		private ComputeBuffer computeBuffer = null;

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
			computeBuffer = new ComputeBuffer(MAX_PARAFFIN_COUNT, sizeof(float) * 24);
            InitMaterial();
		}

		void OnDisable()
		{
			if (computeBuffer != null)
			{
				computeBuffer.Release();
				computeBuffer = null;
			}
			DeleteMaterial();
		}

        private void InitMaterial()
        {
            DeleteMaterial();
            var materialName = isDebug ? "ColorParaffinDebug" : "ColorParaffin";
#if COM3D2
			material = bundleManager.LoadMaterial(materialName);
#else
			material = new Material(Shader.Find("MTE/" + materialName));
#endif
			material.hideFlags = HideFlags.HideAndDontSave;
        }

        private void DeleteMaterial()
        {
            if (material != null)
            {
                DestroyImmediate(material);
                material = null;
            }
        }

		void OnValidate()
		{
            InitMaterial();
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			BuildParaffinBuffers();

			if (enabledCount == 0)
			{
				Graphics.Blit(source, destination);
				return;
			}

			computeBuffer.SetData(paraffinBuffers);
			material.SetBuffer("_ParaffinBuffer", computeBuffer);
			material.SetInt("_ParaffinCount", enabledCount);

			Graphics.Blit(source, destination, material);
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
			enabledCount = 0;
			for (int i = 0; i < paraffinDataList.Count; i++)
			{
				if (enabledCount >= MAX_PARAFFIN_COUNT)
				{
					Debug.LogError("Too many paraffin effects. Max count is " + MAX_PARAFFIN_COUNT);
					break;
				}

				var data = paraffinDataList[i];
				if (!data.enabled)
				{
					continue;
				}

				paraffinBuffers[enabledCount] = ConvertToBuffer(data);
				++enabledCount;
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
			buffer.useNormal = data.useNormal;
			buffer.useAdd = data.useAdd;
			buffer.useMultiply = data.useMultiply;
			buffer.useOverlay = data.useOverlay;
			buffer.useSubstruct = data.useSubstruct;

			return buffer;
		}
	}
}