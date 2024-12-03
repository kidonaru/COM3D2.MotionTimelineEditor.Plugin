using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
	public interface IPostEffectData
	{
		void CopyFrom(IPostEffectData data);
	}

    [System.Serializable]
	public class PostEffectSettingsBase<T>
		where T : class, IPostEffectData
	{
		public bool enabled = false;
		public List<T> dataList = new List<T>();

		public T GetData(int index)
		{
			if (index < 0 || index >= dataList.Count)
			{
				return null;
			}
			return dataList[index];
		}

		public void SetData(int index, T data)
		{
			if (index < 0 || index >= dataList.Count)
			{
				return;
			}
			dataList[index].CopyFrom(data);
		}

		public void AddData(T data)
		{
			dataList.Add(data);
		}

		public void RemoveData(int index)
		{
			if (index < 0 || index >= dataList.Count)
			{
				return;
			}
			dataList.RemoveAt(index);
		}

		public void RemoveDataLast()
		{
			if (dataList.Count > 0)
			{
				dataList.RemoveAt(dataList.Count - 1);
			}
		}

		public int GetDataCount()
		{
			return dataList.Count;
		}

		public void ClearDataAll()
		{
			dataList.Clear();
		}
	}
}