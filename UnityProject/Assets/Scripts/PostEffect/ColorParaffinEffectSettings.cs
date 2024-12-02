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

    [System.Serializable]
	public class ColorParaffinEffectSettings
	{
		public bool enabled = false;
		public List<ParaffinData> dataList = new List<ParaffinData>();
		public bool isDebug = false;

		public ParaffinData GetParaffinData(int index)
		{
			if (index < 0 || index >= dataList.Count)
			{
				return null;
			}
			return dataList[index];
		}

		public void SetParaffinData(int index, ParaffinData data)
		{
			if (index < 0 || index >= dataList.Count)
			{
				return;
			}
			dataList[index].CopyFrom(data);
		}

		public void AddParaffinData(ParaffinData data)
		{
			dataList.Add(data);
		}

		public void RemoveParaffinData(int index)
		{
			if (index < 0 || index >= dataList.Count)
			{
				return;
			}
			dataList.RemoveAt(index);
		}

		public void RemoveLastParaffinData()
		{
			if (dataList.Count > 0)
			{
				dataList.RemoveAt(dataList.Count - 1);
			}
		}

		public int GetParaffinCount()
		{
			return dataList.Count;
		}

		public void ClearParaffinData()
		{
			dataList.Clear();
		}
	}
}