using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using SH = StudioHack;
    using MTE = MotionTimelineEditor;

    public class TimelineData
    {
        public static readonly int CurrentVersion = 2;
        public static readonly TimelineData DefaultTimeline = new TimelineData();

        [XmlAttribute("version")]
        public int version = 0;

        [XmlElement("Frame")]
        public List<FrameData> keyFrames = new List<FrameData>();

        [XmlIgnore]
        private int _maxFrameNo = 30;
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

                _maxFrameNo = Mathf.Max(value, maxExistFrameNo, 1);
            }
        }

        [XmlIgnore]
        private float _frameRate = 30f;
        [XmlIgnore]
        private float _frameDuration = 1f / 30f;

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
                _frameDuration = 1f / _frameRate;
            }
        }

        public float frameDuration
        {
            get
            {
                return _frameDuration;
            }
        }

        [XmlElement("AnmName")]
        public string anmName = "";

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

        private bool _useMuneKeyL = false;

        [XmlElement("UseMuneKeyL")]
        public bool useMuneKeyL
        {
            get
            {
                return _useMuneKeyL;
            }
            set
            {
                if (_useMuneKeyL == value)
                {
                    return;
                }

                _useMuneKeyL = value;
                maidHack.useMuneKeyL = value;
            }
        }

        private bool _useMuneKeyR = false;

        [XmlElement("UseMuneKeyR")]
        public bool useMuneKeyR
        {
            get
            {
                return _useMuneKeyR;
            }
            set
            {
                if (_useMuneKeyR == value)
                {
                    return;
                }

                _useMuneKeyR = value;
                maidHack.useMuneKeyR = value;
            }
        }

        public int maxExistFrameNo
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

        [XmlElement("MinTangent")]
        public float minTangent = -5f;

        [XmlElement("MaxTangent")]
        public float maxTangent = 5f;

        [XmlElement("VideoEnabled")]
        public bool videoEnabled = true;

        [XmlElement("VideoDisplayOnGUI")]
        public bool videoDisplayOnGUI = true;

        [XmlElement("VideoPath")]
        public string videoPath = "";

        [XmlElement("VideoPosition")]
        public Vector3 videoPosition = new Vector3(0, 0, 0);

        [XmlElement("VideoRotation")]
        public Vector3 videoRotation = new Vector3(0, 0, 0);

        [XmlElement("VideoScale")]
        public float videoScale = 1f;

        [XmlElement("VideoStartTime")]
        public float videoStartTime = 0f;

        [XmlElement("VideoVolume")]
        public float videoVolume = 0.5f;

        [XmlElement("VideoGUIPosition")]
        public Vector2 videoGUIPosition = new Vector2(0, 0);

        [XmlElement("VideoGUIScale")]
        public float videoGUIScale = 1f;

        [XmlElement("VideoGUIAlpha")]
        public float videoGUIAlpha = 1f;

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
                return maidHack.outputAnmPath + "\\" + anmFileName;
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

        private static MaidHackBase maidHack
        {
            get
            {
                return MTE.maidHack;
            }
        }

        // ループ補正用の最終フレーム
        private FrameData _dummyLastFrame = new FrameData(0);

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

        public FrameData GetOrCreateFrame(int frameNo)
        {
            var frame = GetFrame(frameNo);
            if (frame != null)
            {
                return frame;
            }

            frame = new FrameData(frameNo);
            keyFrames.Add(frame);
            keyFrames.Sort((a, b) => a.frameNo - b.frameNo);
            return frame;
        }

        public void RemoveFrame(int frameNo)
        {
            var frame = GetFrame(frameNo);
            if (frame != null)
            {
                keyFrames.Remove(frame);
            }
        }

        public void UpdateFrame(int frameNo, CacheBoneDataArray cacheBoneData)
        {
            var frame = GetOrCreateFrame(frameNo);
            frame.SetCacheBoneDataArray(cacheBoneData);
        }

        public void SetBone(int frameNo, BoneData bone)
        {
            var frame = GetOrCreateFrame(frameNo);
            frame.SetBone(bone);
        }

        public void SetBones(int frameNo, IEnumerable<BoneData> bones)
        {
            var frame = GetOrCreateFrame(frameNo);
            frame.SetBones(bones);
        }
        
        public void UpdateBone(int frameNo, BoneData bone)
        {
            var frame = GetOrCreateFrame(frameNo);
            frame.UpdateBone(bone);
        }

        public void UpdateBones(int frameNo, IEnumerable<BoneData> bones)
        {
            var frame = GetOrCreateFrame(frameNo);
            frame.UpdateBones(bones);
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

        public float GetFrameTimeSeconds(int frameNo)
        {
            return frameNo * _frameDuration;
        }

        public FrameData GetPrevFrame(int frameNo)
        {
            return keyFrames.LastOrDefault(f => f.frameNo < frameNo);
        }

        public FrameData GetNextFrame(int frameNo)
        {
            return keyFrames.First(f => f.frameNo > frameNo);
        }

        public BoneData GetPrevBone(
            int frameNo,
            string path,
            out int prevFrameNo,
            bool loopSearch)
        {
            BoneData prevBone = null;
            prevFrameNo = -1;
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
                    prevFrameNo = frame.frameNo;
                }
            }

            if (prevBone == null && loopSearch)
            {
                frameNo = (frameNo == 0) ? maxFrameNo : maxFrameNo + 1; // 0Fの場合は最終フレームを除外
                prevBone = GetPrevBone(frameNo, path, out prevFrameNo, false);
                prevFrameNo -= maxFrameNo;
            }

            return prevBone;
        }

        public BoneData GetPrevBone(int frameNo, string path, out int prevFrameNo)
        {
            return GetPrevBone(frameNo, path, out prevFrameNo, true);
        }

        public BoneData GetPrevBone(int frameNo, string path)
        {
            int prevFrameNo;
            return GetPrevBone(frameNo, path, out prevFrameNo);
        }

        public BoneData GetPrevBone(BoneData bone)
        {
            return GetPrevBone(bone.frameNo, bone.bonePath);
        }

        public List<BoneData> GetPrevBones(IEnumerable<BoneData> bones)
        {
            var prevBones = new List<BoneData>();
            foreach (var bone in bones)
            {
                var prevBone = GetPrevBone(bone);
                if (prevBone != null)
                {
                    prevBones.Add(prevBone);
                }
            }
            return prevBones;
        }

        public BoneData GetNextBone(
            int frameNo,
            string path,
            out int nextFrameNo,
            bool loopSearch)
        {
            BoneData nextBone = null;
            nextFrameNo = -1;
            foreach (var frame in keyFrames)
            {
                if (frame.frameNo <= frameNo)
                {
                    continue;
                }

                var bone = frame.GetBone(path);
                if (bone != null)
                {
                    nextBone = bone;
                    nextFrameNo = frame.frameNo;
                    break;
                }
            }

            if (nextBone == null && loopSearch)
            {
                frameNo = (frameNo == maxFrameNo) ? 0 : -1; // 最終フレームの場合は0Fを除外
                nextBone = GetNextBone(frameNo, path, out nextFrameNo, false);
                nextFrameNo += maxFrameNo;
            }

            return nextBone;
        }

        public BoneData GetNextBone(int frameNo, string path, out int nextFrameNo)
        {
            return GetNextBone(frameNo, path, out nextFrameNo, true);
        }

        public FrameData GetActiveFrame(float frameNo)
        {
            return keyFrames.LastOrDefault(f => f.frameNo <= frameNo);
        }

        public bool IsValidData(out string message)
        {
            message = "";

            if (maidHack.maid == null)
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
            foreach (var frame in keyFrames)
            {
                if (frame.frameNo == 0)
                {
                    continue;
                }

                foreach (var bone in frame.bones)
                {
                    var prevBone = GetPrevBone(frame.frameNo, bone.bonePath);
                    bone.FixRotation(prevBone);
                }
            }
        }

        public void UpdateTangent()
        {
            foreach (var frame in keyFrames)
            {
                var currentTime = GetFrameTimeSeconds(frame.frameNo);
                foreach (var bone in frame.bones)
                {
                    int prevFrameNo;
                    var prevBone = GetPrevBone(frame.frameNo, bone.bonePath, out prevFrameNo);

                    int nextFrameNo;
                    var nextBone = GetNextBone(frame.frameNo, bone.bonePath, out nextFrameNo);

                    if (prevBone == null || nextBone == null)
                    {
                        PluginUtils.LogError("前後のキーフレームがないので補間処理をスキップしました：" + bone.bonePath);
                        continue;
                    }

                    var prevTrans = prevBone.transform;
                    var nextTrans = nextBone.transform;

                    // 別ループのキーフレームは回転補正を行う
                    if (prevFrameNo != prevBone.frameNo)
                    {
                        prevTrans = new TransformData(prevTrans);
                        prevTrans.FixRotation(bone.transform);
                    }
                    if (nextFrameNo != nextBone.frameNo)
                    {
                        nextTrans = new TransformData(nextTrans);
                        nextTrans.FixRotation(bone.transform);
                    }

                    var prevTime = GetFrameTimeSeconds(prevFrameNo);
                    var nextTime = GetFrameTimeSeconds(nextFrameNo);

                    bone.transform.UpdateTangent(
                        prevTrans,
                        nextTrans,
                        prevTime,
                        currentTime,
                        nextTime);
                }
            }
        }

        public void UpdateDummyLastFrame()
        {
            _dummyLastFrame.frameNo = maxFrameNo;

            var firstFrame = this.firstFrame;
            var lastFrame = GetFrame(maxFrameNo);
            var currentTime = GetFrameTimeSeconds(maxFrameNo);

            foreach (var firstBone in firstFrame.bones)
            {
                var path = firstBone.bonePath;
                var lastBone = lastFrame != null ? lastFrame.GetBone(path) : null;
                if (lastBone == null)
                {
                    _dummyLastFrame.UpdateBone(firstBone);

                    var dummyBone = _dummyLastFrame.GetBone(path);
                    var prevBone = GetPrevBone(maxFrameNo, path);
                    var prevTime = GetFrameTimeSeconds(prevBone.frameNo);

                    dummyBone.FixRotation(prevBone);
                    dummyBone.transform.UpdateInTangent(
                        prevBone.transform,
                        prevTime,
                        currentTime);
                }
            }
        }

        public byte[] GetAnmBinary(out string message)
        {
            if (!IsValidData(out message))
            {
                return null;
            }

            var maxSecond = GetFrameTimeSeconds(maxFrameNo);

            var times = new List<float>(keyFrames.Count);
            var valuesList = new List<float[]>(keyFrames.Count);
            var inTangentsList = new List<float[]>(keyFrames.Count);
            var outTangentsList = new List<float[]>(keyFrames.Count);

            Action<BinaryWriter, BoneData> write_bone_data = delegate (BinaryWriter w, BoneData first_bone_data)
            {
                var path = first_bone_data.bonePath;
                w.Write((byte)1);
                w.Write(path);

                times.Clear();
                valuesList.Clear();
                inTangentsList.Clear();
                outTangentsList.Clear();

                bool hasLastKey = false;
                foreach (var frame in keyFrames)
                {
                    var bone = frame.GetBone(path);
                    if (bone != null)
                    {
                        times.Add(GetFrameTimeSeconds(frame.frameNo));
                        valuesList.Add(bone.transform.values);
                        inTangentsList.Add(bone.transform.inTangents);
                        outTangentsList.Add(bone.transform.outTangents);
                        hasLastKey = frame.frameNo == maxFrameNo;
                    }
                }

                if (!hasLastKey)
                {
                    if (isLoopAnm)
                    {
                        // ループアニメーションの場合はダミーフレームを参照
                        var bone = _dummyLastFrame.GetBone(path);
                        if (bone != null)
                        {
                            times.Add(maxSecond);
                            valuesList.Add(bone.transform.values);
                            inTangentsList.Add(bone.transform.inTangents);
                            outTangentsList.Add(bone.transform.outTangents);
                        }
                    }
                    else
                    {
                        // ループアニメーションでない場合は最終フレームに最後のフレームを追加
                        times.Add(maxSecond);
                        valuesList.Add(valuesList.Last());
                        inTangentsList.Add(inTangentsList.Last());
                        outTangentsList.Add(outTangentsList.Last());
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
                        w.Write(inTangentsList[j][i]);
                        w.Write(outTangentsList[j][i]);
                    }
                }
            };
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write("CM3D2_ANIM");
            binaryWriter.Write(1001);
            foreach (var path in PluginUtils.saveBonePaths)
            {
                var bone = firstFrame.GetBone(path);
                if (bone == null)
                {
                    PluginUtils.LogError("ボーンがないのでスキップしました：" + path);
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

        public static void ClearTexture(Texture2D texture, Color color)
        {
            var pixels = new Color[texture.width * texture.height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
        }

        // ヘルミート曲線の計算
        private static float Hermite(
            float t,
            float outTangent,
            float inTangent)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return (2 * t3 - 3 * t2 + 1) * 0 + (t3 - 2 * t2 + t) * outTangent + (-2 * t3 + 3 * t2) * 1 + (t3 - t2) * inTangent;
        }

        public static void UpdateCurveTexture(
            Texture2D texture,
            float outTangent,
            float inTangent,
            Color lineColor,
            int lineWidth)
        {
            var width = texture.width;
            var height = texture.height;
            var halfLineWidth = lineWidth / 2;

            for (int x = 0; x < width; x++)
            {
                float t = x / (float)width;
                int y = (int)(Hermite(t, outTangent, inTangent) * height);

                y -= halfLineWidth;
                for (int i = 0; i < lineWidth; i++)
                {
                    var yy = Mathf.Clamp(y + i, 0, height - 1);
                    texture.SetPixel(x, yy, lineColor);
                }
            }

            texture.Apply();
        }

        public void ConvertVersion()
        {
            if (version < 2)
            {
                // Smoothを無効にする
                /*foreach (var frame in keyFrames)
                {
                    foreach (var bone in frame.bones)
                    {
                        bone.transform.inSmoothBit = 0;
                        bone.transform.outSmoothBit = 0;
                    }
                }*/
            }

            version = CurrentVersion;
        }

        public float ClampTangent(float tangent)
        {
            return Mathf.Clamp(tangent, minTangent, maxTangent);
        }

        public void ResetSettings()
        {
            //maxFrameNo = DefaultTimeline.maxFrameNo;
            frameRate = DefaultTimeline.frameRate;
            isHoldList = DefaultTimeline.isHoldList.ToArray();
            useMuneKeyL = DefaultTimeline.useMuneKeyL;
            useMuneKeyR = DefaultTimeline.useMuneKeyR;
            isLoopAnm = DefaultTimeline.isLoopAnm;
            minTangent = DefaultTimeline.minTangent;
            maxTangent = DefaultTimeline.maxTangent;
        }
    }
}