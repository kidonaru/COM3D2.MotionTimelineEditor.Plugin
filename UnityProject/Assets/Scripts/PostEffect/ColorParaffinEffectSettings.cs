using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [System.Serializable]
	public class ColorParaffinData : IPostEffectData
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

		public void CopyFrom(IPostEffectData data)
		{
			var paraffinData = data as ColorParaffinData;
			enabled = paraffinData.enabled;
			color1 = paraffinData.color1;
			color2 = paraffinData.color2;
			centerPosition = paraffinData.centerPosition;
			radiusFar = paraffinData.radiusFar;
			radiusNear = paraffinData.radiusNear;
			radiusScale = paraffinData.radiusScale;
			depthMin = paraffinData.depthMin;
			depthMax = paraffinData.depthMax;
			depthFade = paraffinData.depthFade;
			useNormal = paraffinData.useNormal;
			useAdd = paraffinData.useAdd;
			useMultiply = paraffinData.useMultiply;
			useOverlay = paraffinData.useOverlay;
			useSubstruct = paraffinData.useSubstruct;
		}

		public static ColorParaffinData Lerp(
            ColorParaffinData a,
            ColorParaffinData b,
            float t)
        {
			return new ColorParaffinData
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

    [System.Serializable]
	public class ColorParaffinEffectSettings : PostEffectSettingsBase<ColorParaffinData>
	{
		public bool isDebug = false;
	}
}