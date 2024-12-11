using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
	[System.Serializable]
	public class DistanceFogData : IPostEffectData
	{
		public bool enabled = false;
		public Color color1 = new Color(1f, 1f, 1f, 1f);
		public Color color2 = new Color(1f, 1f, 1f, 0f);

		public float fogStart = 0f;
		public float fogEnd = 40f;
		public float fogExp = 1f;

		[Header("Blend Mode")]
		[Range(0f, 1f)]
		public float useNormal = 1f;
		[Range(0f, 1f)]
		public float useAdd = 0f;
		[Range(0f, 1f)]
		public float useMultiply = 0f;
		[Range(0f, 1f)]
		public float useOverlay = 0f;
		[Range(0f, 1f)]
		public float useSubstruct = 0f;

		public void CopyFrom(IPostEffectData data)
		{
			var paraffinData = data as DistanceFogData;
			enabled = paraffinData.enabled;
			color1 = paraffinData.color1;
			color2 = paraffinData.color2;
			fogStart = paraffinData.fogStart;
			fogEnd = paraffinData.fogEnd;
			fogExp = paraffinData.fogExp;
			useNormal = paraffinData.useNormal;
			useAdd = paraffinData.useAdd;
			useMultiply = paraffinData.useMultiply;
			useOverlay = paraffinData.useOverlay;
			useSubstruct = paraffinData.useSubstruct;
		}

		public static DistanceFogData Lerp(
            DistanceFogData a,
            DistanceFogData b,
            float t)
        {
			return new DistanceFogData
			{
				enabled = a.enabled,
				color1 = Color.Lerp(a.color1, b.color1, t),
				color2 = Color.Lerp(a.color2, b.color2, t),
				fogStart = Mathf.Lerp(a.fogStart, b.fogStart, t),
				fogEnd = Mathf.Lerp(a.fogEnd, b.fogEnd, t),
				fogExp = Mathf.Lerp(a.fogExp, b.fogExp, t),
				useNormal = Mathf.Lerp(a.useNormal, b.useNormal, t),
				useAdd = Mathf.Lerp(a.useAdd, b.useAdd, t),
				useMultiply = Mathf.Lerp(a.useMultiply, b.useMultiply, t),
				useOverlay = Mathf.Lerp(a.useOverlay, b.useOverlay, t),
				useSubstruct = Mathf.Lerp(a.useSubstruct, b.useSubstruct, t),
			};
		}
	}

    [System.Serializable]
	public class DistanceFogEffectSettings : PostEffectSettingsBase<DistanceFogData>
	{
	}
}