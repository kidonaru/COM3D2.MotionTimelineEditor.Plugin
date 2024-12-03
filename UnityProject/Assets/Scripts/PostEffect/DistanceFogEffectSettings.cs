using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
	[System.Serializable]
	public class DistanceFogData : IPostEffectData
	{
		public bool enabled = false;
		public Color color1 = new Color(0.35f, 0f, 1f, 0f);
		public Color color2 = new Color(0.35f, 0f, 1f, 0.3f);

		public float fogStart = 0f;
		public float fogEnd = 40f;
		public float fogExp = 1f;

		public void CopyFrom(IPostEffectData data)
		{
			var paraffinData = data as DistanceFogData;
			enabled = paraffinData.enabled;
			color1 = paraffinData.color1;
			color2 = paraffinData.color2;
			fogStart = paraffinData.fogStart;
			fogEnd = paraffinData.fogEnd;
			fogExp = paraffinData.fogExp;
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
			};
		}
	}

    [System.Serializable]
	public class DistanceFogEffectSettings : PostEffectSettingsBase<DistanceFogData>
	{
		public bool isDebug = false;
	}
}