using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public static class BinaryExtensions
	{
		public static Vector2 ReadVector2(this BinaryReader binaryReader)
		{
			return new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
		}

		public static Vector3 ReadVector3(this BinaryReader binaryReader)
		{
			return new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		}

        public static void Write(this BinaryWriter binaryWriter, Vector3 vector3)
		{
			binaryWriter.Write(vector3.x);
			binaryWriter.Write(vector3.y);
			binaryWriter.Write(vector3.z);
		}

		public static Vector4 ReadVector4(this BinaryReader binaryReader)
		{
			return new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		}

		public static Quaternion ReadQuaternion(this BinaryReader binaryReader)
		{
			return new Quaternion(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
		}

		public static Matrix4x4 ReadMatrix4x4(this BinaryReader binaryReader)
		{
			Matrix4x4 result = default(Matrix4x4);
			for (int i = 0; i < 16; i++)
			{
				result[i] = binaryReader.ReadSingle();
			}
			return result;
		}
	}
}