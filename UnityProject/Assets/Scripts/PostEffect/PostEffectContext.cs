using System;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace COM3D2.MotionTimelineEditor.Plugin
{
	[System.Serializable]
	public class PostEffectContext
	{
		public Camera camera;
		public ColorParaffinEffectSettings paraffinSettings = new ColorParaffinEffectSettings();
		public DistanceFogEffectSettings fogSettings = new DistanceFogEffectSettings();
	}
}