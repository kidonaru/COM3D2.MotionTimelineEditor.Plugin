using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
	[System.Serializable]
	public struct GTToneMapData
	{
		public bool enabled;
		[Range(1f, 100f)]
		public float maxBrightness;
		[Range(0f, 5f)]
		public float contrast;
		[Range(0f, 1f)]
		public float linearStart;
		[Range(0f, 1f)]
		public float linearLength;
		[Range(1f, 3f)]
		public float blackTightness;
		[Range(0f, 1f)]
		public float blackOffset;

		public static GTToneMapData Create()
		{
			return new GTToneMapData
			{
				enabled = false,
				maxBrightness = 1.0f,
				contrast = 1.0f,
				linearStart = 0.22f,
				linearLength = 0.4f,
				blackTightness = 1.33f,
				blackOffset = 0.0f
			};
		}

		public void CopyFrom(GTToneMapData other)
		{
			enabled = other.enabled;
			maxBrightness = other.maxBrightness;
			contrast = other.contrast;
			linearStart = other.linearStart;
			linearLength = other.linearLength;
			blackTightness = other.blackTightness;
			blackOffset = other.blackOffset;
		}

		public bool Equals(GTToneMapData other)
		{
			return enabled == other.enabled &&
			       maxBrightness.Equals(other.maxBrightness) &&
			       contrast.Equals(other.contrast) &&
			       linearStart.Equals(other.linearStart) &&
			       linearLength.Equals(other.linearLength) &&
			       blackTightness.Equals(other.blackTightness) &&
			       blackOffset.Equals(other.blackOffset);
		}

		public static GTToneMapData Lerp(GTToneMapData start, GTToneMapData end, float t)
		{
			GTToneMapData result = new GTToneMapData();
			result.enabled = start.enabled;
			result.maxBrightness = Mathf.Lerp(start.maxBrightness, end.maxBrightness, t);
			result.contrast = Mathf.Lerp(start.contrast, end.contrast, t);
			result.linearStart = Mathf.Lerp(start.linearStart, end.linearStart, t);
			result.linearLength = Mathf.Lerp(start.linearLength, end.linearLength, t);
			result.blackTightness = Mathf.Lerp(start.blackTightness, end.blackTightness, t);
			result.blackOffset = Mathf.Lerp(start.blackOffset, end.blackOffset, t);
			return result;
		}
	}

	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/GTToneMap")]
	[ImageEffectAllowedInSceneView]
	public class GTToneMapController : MonoBehaviour
	{
		public GTToneMapData data = GTToneMapData.Create();

		public Material material;

		private static class Uniforms
		{
			internal static readonly int _TempRT = Shader.PropertyToID("_TempRT");
			internal static readonly int _MaxBrightness = Shader.PropertyToID("_MaxBrightness");
			internal static readonly int _Contrast = Shader.PropertyToID("_Contrast");
			internal static readonly int _LinearStart = Shader.PropertyToID("_LinearStart");
			internal static readonly int _LinearLength = Shader.PropertyToID("_LinearLength");
			internal static readonly int _BlackTightness = Shader.PropertyToID("_BlackTightness");
			internal static readonly int _BlackOffset = Shader.PropertyToID("_BlackOffset");
		}

#if COM3D2
		private static TimelineBundleManager bundleManager => TimelineBundleManager.instance;
#endif

		void OnEnable()
		{
			InitMaterial();
		}

#if UNITY_EDITOR
		void Update()
		{
			OnUpdateData();
		}
#endif

		public void ApplyData(GTToneMapData newData)
		{
			if (data.Equals(newData))
			{
				return; // No change, no need to update
			}

			data.CopyFrom(newData);

			OnUpdateData();
		}

		public void SetEnable(bool ebabled)
		{
			if (data.enabled == enabled)
			{
				return; // Already set, no need to update
			}

			data.enabled = enabled;
			OnUpdateData();
		}

		public void InitMaterial()
		{
			if (material == null)
			{
				material = LoadMaterial("GTToneMap");
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

		public void OnUpdateData()
		{
			enabled = data.enabled;

			material.SetFloat(Uniforms._MaxBrightness, data.maxBrightness);
			material.SetFloat(Uniforms._Contrast, data.contrast);
			material.SetFloat(Uniforms._LinearStart, data.linearStart);
			material.SetFloat(Uniforms._LinearLength, data.linearLength);
			material.SetFloat(Uniforms._BlackTightness, data.blackTightness);
			material.SetFloat(Uniforms._BlackOffset, data.blackOffset);
		}

		[ImageEffectTransformsToLDR]
		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (material == null || !data.enabled)
			{
				Graphics.Blit(source, destination);
				return;
			}

			// HDR → LDR変換
			Graphics.Blit(source, destination, material);
		}
	}
}