using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
	[System.Serializable]
	public class RimlightData : IPostEffectData
	{
		public bool enabled = false;
		public Color color1 = new Color(0.77f, 0.70f, 1f, 1f);
		public Color color2 = new Color(0.77f, 0.70f, 1f, 0f);

		public Vector3 rotation = new Vector3(10f, -40f, 0f);
		public float lightArea = 1f;
		public float fadeRange = 0.2f;
		public float fadeExp = 1f;
		public float depthMin = 0f;
		public float depthMax = 5f;
		public float depthFade = 1f;

		[Header("Blend Mode")]
		[Range(0f, 1f)]
		public float useNormal = 0f;
		[Range(0f, 1f)]
		public float useAdd = 0.8f;
		[Range(0f, 1f)]
		public float useMultiply = 0f;
		[Range(0f, 1f)]
		public float useOverlay = 0f;
		[Range(0f, 1f)]
		public float useSubstruct = 0f;

		public bool isWorldSpace = false;

		public float edgeDepth = 0f;
		[Range(0f, 10f)]
		public float edgeRange = 0f;

		public float heightMin = 0.01f;

		public void CopyFrom(IPostEffectData data)
		{
			var paraffinData = data as RimlightData;
			enabled = paraffinData.enabled;
			color1 = paraffinData.color1;
			color2 = paraffinData.color2;
			rotation = paraffinData.rotation;
			lightArea = paraffinData.lightArea;
			fadeRange = paraffinData.fadeRange;
			fadeExp = paraffinData.fadeExp;
			depthMin = paraffinData.depthMin;
			depthMax = paraffinData.depthMax;
			depthFade = paraffinData.depthFade;
			useNormal = paraffinData.useNormal;
			useAdd = paraffinData.useAdd;
			useMultiply = paraffinData.useMultiply;
			useOverlay = paraffinData.useOverlay;
			useSubstruct = paraffinData.useSubstruct;
			isWorldSpace = paraffinData.isWorldSpace;
			edgeDepth = paraffinData.edgeDepth;
			edgeRange = paraffinData.edgeRange;
			heightMin = paraffinData.heightMin;
		}

		public static RimlightData Lerp(
            RimlightData a,
            RimlightData b,
            float t)
        {
			return new RimlightData
			{
				enabled = a.enabled,
				color1 = Color.Lerp(a.color1, b.color1, t),
				color2 = Color.Lerp(a.color2, b.color2, t),
				rotation = Vector3.Lerp(a.rotation, b.rotation, t),
				lightArea = Mathf.Lerp(a.lightArea, b.lightArea, t),
				fadeRange = Mathf.Lerp(a.fadeRange, b.fadeRange, t),
				fadeExp = Mathf.Lerp(a.fadeExp, b.fadeExp, t),
				depthMin = Mathf.Lerp(a.depthMin, b.depthMin, t),
				depthMax = Mathf.Lerp(a.depthMax, b.depthMax, t),
				depthFade = Mathf.Lerp(a.depthFade, b.depthFade, t),
				useNormal = Mathf.Lerp(a.useNormal, b.useNormal, t),
				useAdd = Mathf.Lerp(a.useAdd, b.useAdd, t),
				useMultiply = Mathf.Lerp(a.useMultiply, b.useMultiply, t),
				useOverlay = Mathf.Lerp(a.useOverlay, b.useOverlay, t),
				useSubstruct = Mathf.Lerp(a.useSubstruct, b.useSubstruct, t),
				isWorldSpace = a.isWorldSpace,
				edgeDepth = Mathf.Lerp(a.edgeDepth, b.edgeDepth, t),
				edgeRange = Mathf.Lerp(a.edgeRange, b.edgeRange, t),
				heightMin = Mathf.Lerp(a.heightMin, b.heightMin, t),
			};
		}
	}

    [System.Serializable]
	public class RimlightEffectSettings : PostEffectSettingsBase<RimlightData>
	{
	}
}