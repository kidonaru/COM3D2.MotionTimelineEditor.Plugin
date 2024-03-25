using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using SH = StudioHack;

    public class TimelineData
    {
        [XmlElement("Frame")]
        public List<FrameData> keyFrames = new List<FrameData>();

        [XmlIgnore]
        public int _maxFrameNo = 30;
        [XmlElement("MaxFrameNo")]
        public int maxFrameNo
        {
            get
            {
                return _maxFrameNo;
            }
            set
            {
                if (_maxFrameNo == value)
                {
                    return;
                }

                _maxFrameNo = Mathf.Max(value, lastFrameNo, 1);
            }
        }

        [XmlIgnore]
        public float _frameRate = 30f;
        [XmlElement("FrameRate")]
        public float frameRate
        {
            get
            {
                return _frameRate;
            }
            set
            {
                if (_frameRate == value)
                {
                    return;
                }
                _frameRate = Mathf.Max(1f, value);
            }
        }

        [XmlElement("AnmName")]
        public string anmName;

        [XmlElement("IsHold")]
        public bool[] isHoldList = new bool[(int) IKHoldType.Max]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
        };

        [XmlElement("UseMuneKeyL")]
        public bool useMuneKeyL
        {
            get
            {
                return SH.useMuneKeyL;
            }
            set
            {
                SH.useMuneKeyL = value;
            }
        }

        [XmlElement("UseMuneKeyR")]
        public bool useMuneKeyR
        {
            get
            {
                return SH.useMuneKeyR;
            }
            set
            {
                SH.useMuneKeyR = value;
            }
        }

        public int lastFrameNo
        {
            get
            {
                if (keyFrames.Count == 0)
                {
                    return 0;
                }
                return keyFrames[keyFrames.Count - 1].frameNo;
            }
        }

        [XmlElement("IsLoopAnm")]
        public bool isLoopAnm = true;

        public int maxFrameCount
        {
            get
            {
                return maxFrameNo + 1;
            }
        }

        public String anmFileName
        {
            get
            {
                return anmName + ".anm";
            }
        }

        public String anmPath
        {
            get
            {
                var mode_folder_path = PhotoModePoseSave.folder_path;
                var path = mode_folder_path + "\\" + anmFileName;
                return path;
            }
        }

        public FrameData firstFrame
        {
            get
            {
                return keyFrames.Count > 0 ? keyFrames[0] : null;
            }
        }

        public bool isHoldActive
        {
            get
            {
                return isHoldList.Any(b => b);
            }
        }

        public FrameData GetFrame(int frameNo)
        {
            foreach (var frame in keyFrames)
            {
                if (frame.frameNo == frameNo)
                {
                    return frame;
                }
            }
            return null;
        }

        public void RemoveFrame(int frameNo)
        {
            var frame = GetFrame(frameNo);
            if (frame != null)
            {
                keyFrames.Remove(frame);
            }
        }

        public void UpdateFrame(int frameNo, CacheBoneDataArray boneDataArray)
        {
            var frame = GetFrame(frameNo);
            if (frame != null)
            {
                frame.SetCacheBoneDataArray(boneDataArray);
                return;
            }

            frame = new FrameData(frameNo);
            frame.SetCacheBoneDataArray(boneDataArray);
            keyFrames.Add(frame);
            keyFrames.Sort((a, b) => a.frameNo - b.frameNo);
        }

        public void UpdateBones(int frameNo, IEnumerable<BoneData> bones)
        {
            var frame = GetFrame(frameNo);
            if (frame != null)
            {
                frame.UpdateBones(bones);
                return;
            }

            frame = new FrameData(frameNo);
            frame.UpdateBones(bones);
            keyFrames.Add(frame);
            keyFrames.Sort((a, b) => a.frameNo - b.frameNo);
        }

        public void CleanFrames()
        {
            var removeFrames = new List<FrameData>();

            foreach (var key in keyFrames)
            {
                if (!key.HasBones())
                {
                    removeFrames.Add(key);
                }
            }

            foreach (var key in removeFrames)
            {
                keyFrames.Remove(key);
            }
        }

        public int GetFrameTimeMs(int frameNo)
        {
            return (int)((float) frameNo * 1000 / frameRate);
        }

        public float GetFrameTimeSecond(int frameNo)
        {
            return frameNo / frameRate;
        }

        public FrameData GetPrevFrame(int frameNo)
        {
            return keyFrames.LastOrDefault(f => f.frameNo < frameNo);
        }

        public FrameData GetNextFrame(int frameNo)
        {
            return keyFrames.First(f => f.frameNo > frameNo);
        }

        public BoneData GetPrevBone(int frameNo, string path)
        {
            BoneData prevBone = null;
            foreach (var frame in keyFrames)
            {
                if (frame.frameNo >= frameNo)
                {
                    break;
                }

                var bone = frame.GetBone(path);
                if (bone != null)
                {
                    prevBone = bone;
                }
            }

            return prevBone;
        }

        public FrameData GetActiveFrame(float frameNo)
        {
            return keyFrames.LastOrDefault(f => f.frameNo <= frameNo);
        }

        public bool IsValidData(out string message)
        {
            message = "";

            if (SH.maid == null)
            {
                message = "メイドを配置してください";
                return false;
            }

            if (anmName.Length == 0)
            {
                message = "アニメ名を入力してください";
                return false;
            }

            var firstFrame = this.firstFrame;
            if (firstFrame == null || firstFrame.frameNo != 0)
            {
                message = "0フレーム目にキーフレームが必要です";
                return false;
            }

            return true;
        }

        public void FixRotation()
        {
            bool isFirst = true;
            foreach (var frame in keyFrames)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }

                foreach (var bone in frame.bones)
                {
                    var prevBone = GetPrevBone(frame.frameNo, bone.bonePath);
                    bone.FixRotation(prevBone);
                }
            }
        }

        public byte[] GetAnmBinary(out string message)
        {
            if (!IsValidData(out message))
            {
                return null;
            }

            var maxSecond = GetFrameTimeSecond(maxFrameNo);

            Action<BinaryWriter, BoneData> write_bone_data = delegate (BinaryWriter w, BoneData first_bone_data)
            {
                w.Write((byte)1);
                w.Write(first_bone_data.bonePath);

                var times = new List<float>(keyFrames.Count);
                var valuesList = new List<float[]>(keyFrames.Count);
                bool hasLastKey = false;
                foreach (var frame in keyFrames)
                {
                    var bone = frame.GetBone(first_bone_data.bonePath);
                    if (bone != null)
                    {
                        times.Add(GetFrameTimeSecond(frame.frameNo));
                        valuesList.Add(bone.transform.values);
                        hasLastKey = frame.frameNo == maxFrameNo;
                    }
                }

                if (!hasLastKey)
                {
                    if (isLoopAnm)
                    {
                        // ループアニメーションの場合は最終フレームに最初のフレームを追加
                        times.Add(maxSecond);
                        valuesList.Add(valuesList[0]);
                    }
                    else
                    {
                        // ループアニメーションでない場合は最終フレームに最後のフレームを追加
                        times.Add(maxSecond);
                        valuesList.Add(valuesList.Last());
                    }
                }

                for (int i = 0; i < first_bone_data.transform.valueCount; i++)
                {
                    w.Write((byte)(100 + i));
                    w.Write(times.Count);
                    for (int j = 0; j < times.Count; j++)
                    {
                        w.Write(times[j]);
                        w.Write(valuesList[j][i]);
                        w.Write(0);
                        w.Write(0);
                    }
                }
            };
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write("CM3D2_ANIM");
            binaryWriter.Write(1001);
            foreach (var path in SH.saveBonePaths)
            {
                var bone = firstFrame.GetBone(path);
                if (bone == null)
                {
                    Extensions.LogError("ボーンがないのでスキップしました：" + path);
                    continue;
                }
                write_bone_data(binaryWriter, bone);
            }
            binaryWriter.Write((byte)0);
            binaryWriter.Write((byte)(useMuneKeyL ? 1u : 0u));
            binaryWriter.Write((byte)(useMuneKeyR ? 1u : 0u));
            binaryWriter.Close();
            memoryStream.Close();
            byte[] result = memoryStream.ToArray();
            memoryStream.Dispose();
            return result;
        }

        public Texture2D CreateBGTexture(
            int frameWidth,
            int frameHeight,
            int height,
            Color bgColor1,
            Color bgColor2,
            Color frameLineColor1,
            Color frameLineColor2,
            int frameNoInterval)
        {
            var width = maxFrameCount * frameWidth;
            var tex = new Texture2D(width, height);
            var pixels = new Color[width * height];

            for (int x = 0; x < width; x++)
            {
                var frameNo = x / frameWidth;
                var framePos = x - frameNo * frameWidth;
                bool isSecondColorLine = frameNo % frameNoInterval == 0;
                bool isCenterLine = framePos == frameWidth / 2 ||
                        (isSecondColorLine && framePos == frameWidth / 2 + 1);

                for (int y = 0; y < height; y++)
                {
                    if (isCenterLine)
                    {
                        pixels[y * width + x] = isSecondColorLine ? frameLineColor2 : frameLineColor1;
                    }
                    else
                    {
                        bool isSecondColorBg = (y / frameHeight) % 2 == 1;
                        pixels[y * width + x] = isSecondColorBg ? bgColor2 : bgColor1;
                    }
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        // ひし形のテクスチャを作成
        public static Texture2D CreateKeyFrameTexture(int size, Color color)
        {
            var tex = new Texture2D(size, size);
            var pixels = new Color[size * size];
            var bgColor = new Color(0, 0, 0, 0);
            int halfSize = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int distanceX = Math.Abs(x - halfSize);
                    int distanceY = Math.Abs(y - halfSize);
                    if (distanceX + distanceY <= halfSize)
                    {
                        pixels[y * size + x] = color;
                    }
                    else
                    {
                        pixels[y * size + x] = bgColor;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}