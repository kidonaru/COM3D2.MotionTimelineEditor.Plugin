using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface ITimelineLayer
    {
        string className { get; }
        int slotNo { get; }
        bool hasSlotNo { get; }
        bool isCameraLayer { get; }
        bool isPostEffectLayer { get; }
        List<FrameData> keyFrames { get; }

        Maid maid { get; }
        MaidCache maidCache { get; }
        int playingFrameNo { get; }
        float playingFrameNoFloat { get; }
        float playingTime { get; }
        bool isMotionPlaying { get; set; }
        float anmSpeed { get; }
        long anmId { get; }
        List<string> allBoneNames { get; }
        string errorMessage { get; }
        bool isCurrent { get; }
        int maxExistFrameNo { get; }
        FrameData firstFrame { get; }
        bool isAnmSyncing { get; }
        bool isAnmPlaying { get; }
        bool isDragging { get; }

        List<IBoneMenuItem> allMenuItems { get; }

        void Init();
        void Dispose();
        void Update();
        void LateUpdate();
        bool IsValidData();
        void OnEndPoseEdit();
        void OnPluginDisable();
        void OnMaidChanged(Maid maid);
        void OnCopyModel(StudioModelStat sourceModel, StudioModelStat newModel);
        void OnCopyLight(StudioLightStat sourceLight, StudioLightStat newLight);
        void OnShapeKeyAdded(string shapeKey);
        void OnShapeKeyRemoved(string shapeKey);
        void OnBoneNameAdded(string extendBoneName);
        void OnBoneNameRemoved(string extendBoneName);
        void UpdateFrame(FrameData frame);
        byte[] GetAnmBinary(bool forOutput);
        void ApplyAnm(long id, byte[] anmData);
        void CreateAndApplyAnm();
        void ApplyCurrentFrame(bool motionUpdate);
        void OutputAnm();
        void OutputDCM(XElement songElement);
        float CalcEasingValue(float t, int easing);
        void ResetDraw(GUIView view);
        void DrawWindow(GUIView view);

        void AddKeyFrameAll();
        void AddKeyFrameDiff();
        void AddKeyFrames(IEnumerable<string> boneNames);
        void RemoveKeyFrames(IEnumerable<string> boneNames);

        TransformType GetTransformType(string name);
        ITransformData CreateTransformData(ITransformData transform);
        ITransformData CreateTransformData(TransformXml xml);

        FrameData CreateFrame(int frameNo);
        FrameData CreateFrame(FrameXml xml);
        FrameData GetFrame(int frameNo);
        FrameData GetOrCreateFrame(int frameNo);
        void SetBone(int frameNo, BoneData bone);
        void SetBones(int frameNo, IEnumerable<BoneData> bones);
        void UpdateBone(int frameNo, BoneData bone);
        void UpdateBones(int frameNo, IEnumerable<BoneData> bones);
        void CleanFrames();
        FrameData GetPrevFrame(int frameNo);
        FrameData GetNextFrame(int frameNo);
        BoneData GetPrevBone(int frameNo, string path, out int prevFrameNo, bool loopSearch);
        BoneData GetPrevBone(int frameNo, string path, out int prevFrameNo);
        BoneData GetPrevBone(int frameNo, string path, bool loopSearch);
        BoneData GetPrevBone(int frameNo, string path);
        BoneData GetPrevBone(BoneData bone);
        List<BoneData> GetPrevBones(IEnumerable<BoneData> bones);
        BoneData GetNextBone(int frameNo, string path, out int nextFrameNo, bool loopSearch);
        BoneData GetNextBone(int frameNo, string path, out int nextFrameNo);
        BoneData GetNextBone(int frameNo, string path, bool loopSearch);
        BoneData GetNextBone(int frameNo, string path);
        FrameData GetActiveFrame(float frameNo);
        int GetStartFrameNo(int frameNo);
        int GetEndFrameNo(int frameNo);
        void InsertFrames(int startFrameNo, int endFrameNo);
        void DuplicateFrames(int startFrameNo, int endFrameNo);
        void DeleteFrames(int startFrameNo, int endFrameNo);
        void InitTangent();

        void FromXml(TimelineLayerXml xml);
        TimelineLayerXml ToXml();
    }
}