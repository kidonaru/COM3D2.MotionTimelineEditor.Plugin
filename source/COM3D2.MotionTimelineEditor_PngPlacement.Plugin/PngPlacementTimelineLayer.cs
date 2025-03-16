using UnityEngine;
using COM3D2.MotionTimelineEditor.Plugin;
using System.Collections.Generic;
using System;
using System.Linq;
using COM3D2.MotionTimelineEditor;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    [TimelineLayerDesc("PNG配置", 35)]
    public class PngPlacementTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(PngPlacementTimelineLayer);
        public override string layerName => nameof(PngPlacementTimelineLayer);

        public override List<string> allBoneNames => pngPlacementManager.pngObjectNames;

        private static PngPlacementManager pngPlacementManager => PngPlacementManager.instance;

        private PngPlacementTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static PngPlacementTimelineLayer Create(int slotNo)
        {
            return new PngPlacementTimelineLayer(0);
        }

        public override void Init()
        {
            base.Init();

            PngPlacementManager.onObjectAdded += OnPngObjectAdded;
            PngPlacementManager.onObjectRemoved += OnPngObjectRemoved;
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            foreach (var pngObject in pngPlacementManager.pngObjects)
            {
                var menuItem = new BoneMenuItem(pngObject.displayName, pngObject.displayName);
                allMenuItems.Add(menuItem);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            PngPlacementManager.onObjectAdded -= OnPngObjectAdded;
            PngPlacementManager.onObjectRemoved -= OnPngObjectRemoved;
        }

        public override bool IsValidData()
        {
            errorMessage = "";
            return true;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (!studioHackManager.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            var pngObject = pngPlacementManager.GetPngObject(motion.name);
            if (pngObject == null || pngObject.transform == null)
            {
                return;
            }

            if (indexUpdated)
            {
                ApplyPngObjectInit(motion, t, pngObject);
            }

            ApplyPngObjectUpdate(motion, t, pngObject);
        }

        private void ApplyPngObjectInit(MotionData motion, float t, PngObjectDataWrapper pngObject)
        {
            var transform = pngObject.transform;
            var start = motion.start as TransformDataPngObject;

            pngObject.SetEnable(start.visible);

            pngObject.SetAPNGSpeed(start.apngspeed);
            pngObject.SetAPNGIsFixedSpeed(start.apngisfixedspeed);
            pngObject.SetScale(start.scalex);
            pngObject.SetScaleMag(start.scalemag);
            pngObject.SetScaleZ(start.scalez);
            pngObject.SetInversion(start.inversion);
            pngObject.SetBrightness(start.brightness);
            pngObject.SetColor(start.color);

            pngObject.SetFixedCamera(start.fixcamera);
            pngObject.SetFixedPos(start.fixedpos);

            pngObject.SetAttachPoint(start.attach, start.maid);
            pngObject.SetAttachRotation(start.attachrotation);

            pngObject.SetStopRotation(start.stoprotation);
            pngObject.SetStopRotationVector(start.stoprotationv);

            transform.localPosition = start.position;
            pngObject.SetRotation(start.eulerAngles);
        }

        private void ApplyPngObjectUpdate(MotionData motion, float t, PngObjectDataWrapper pngObject)
        {
            var transform = pngObject.transform;
            var start = motion.start as TransformDataPngObject;
            var end = motion.end as TransformDataPngObject;

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            if (start.position != end.position)
            {
                transform.localPosition = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.positionValues,
                    end.positionValues,
                    t);
            }

            if (start.eulerAngles != end.eulerAngles)
            {
                var rotation = PluginUtils.HermiteVector3(
                    t0,
                    t1,
                    start.eulerAnglesValues,
                    end.eulerAnglesValues,
                    t);
                pngObject.SetRotation(rotation);
            }

            if (start.scalex != end.scalex)
            {
                var scale = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.scalexValue,
                    end.scalexValue,
                    t);
                pngObject.SetScale(scale);
            }

            if (start.scalez != end.scalez)
            {
                var scaleZ = PluginUtils.HermiteValue(
                    t0,
                    t1,
                    start.scalezValue,
                    end.scalezValue,
                    t);
                pngObject.SetScaleZ(scaleZ);
            }

            if (start.color != end.color)
            {
                var color = Color.Lerp(start.color, end.color, t);
                pngObject.SetColor(color);
            }

            if (start.brightness != end.brightness)
            {
                var brightness = (byte) Mathf.Lerp(start.brightness, end.brightness, t);
                pngObject.SetBrightness(brightness);
            }
        }

        public void OnPngObjectAdded(PngObjectDataWrapper pngObject)
        {
            InitMenuItems();
            AddFirstBones(new List<string> { pngObject.displayName });
            ApplyCurrentFrame(true);
        }

        public void OnPngObjectRemoved(PngObjectDataWrapper pngObject)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { pngObject.displayName });
            ApplyCurrentFrame(true);
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var pngObject in pngPlacementManager.pngObjects)
            {
                var pngObjectName = pngObject.displayName;

                var trans = frame.GetOrCreateTransformData<TransformDataPngObject>(pngObjectName);
                trans.visible = pngObject.enable;
                trans.position = pngObject.transform.localPosition;
                trans.eulerAngles = pngObject.rotation;
                trans.color = pngObject.color;
                trans.inversion = pngObject.inversion;
                trans.stoprotation = pngObject.stopRotation;
                trans.stoprotationv = pngObject.stopRotationVector;
                trans.scalex = pngObject.scale;
                trans.scalemag = pngObject.scaleMag;
                trans.fixcamera = pngObject.fixedCamera;
                trans.fixedpos = pngObject.fixedPos;
                trans.attach = pngObject.attach;
                trans.attachrotation = pngObject.attachRotation;
                trans.brightness = pngObject.brightness;
                trans.scalez = pngObject.scaleZ;
                trans.primitivereferencex = pngObject.primitiveReferenceX;
                trans.squareuv = pngObject.squareUV;
                trans.maid = pngObject.maid;

                if (pngObject.apng != null)
                {
                    trans.apngspeed = pngObject.apngAnm.speed;
                    trans.apngisfixedspeed = pngObject.apngAnm.isFixedSpeed;
                }
            }
        }

        private GUIComboBox<PngObjectDataWrapper> _pngObjectComboBox = new GUIComboBox<PngObjectDataWrapper>
        {
            getName = (obj, index) => obj.displayName,
            labelWidth = 70,
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
            onSelected = (obj, index) =>
            {
                pngPlacementManager.pngPlacement.iCurrentObject = index;
            },
        };

        private ColorFieldCache _colorFieldValue = new ColorFieldCache("", true);

        private GUIComboBox<MaidCache> _maidComboBox = new GUIComboBox<MaidCache>
        {
            getName = (maidCache, _) => maidCache == null ? "未選択" : maidCache.fullName,
            onSelected = (maidCache, index) =>
            {
                maidManager.ChangeMaid(maidCache.maid);
            },
            buttonSize = new Vector2(100, 20),
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<PngAttachPoint> _attachPointComboBox = new GUIComboBox<PngAttachPoint>
        {
            items = Enum.GetValues(typeof(PngAttachPoint)).Cast<PngAttachPoint>().ToList(),
            getName = (type, index) => type.ToString(),
            buttonSize = new Vector2(100, 20),
            contentSize = new Vector2(100, 300),
        };

        public override void DrawWindow(GUIView view)
        {
            DrawPngObjectEdit(view);
            view.DrawComboBox();
        }
        
        public void DrawPngObjectEdit(GUIView view)
        {
            var pngObjects = pngPlacementManager.pngObjects;
            if (pngObjects.Count == 0)
            {
                view.DrawLabel("オブジェクトが存在しません", 200, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            _pngObjectComboBox.items = pngObjects;
            _pngObjectComboBox.currentIndex = pngPlacementManager.pngPlacement.iCurrentObject;
            _pngObjectComboBox.DrawButton("対象", view);

            var pngObject = _pngObjectComboBox.currentItem;
            if (pngObject == null || pngObject.transform == null)
            {
                view.DrawLabel("オブジェクトを選択してください", 200, 20);
                return;
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            var defaultTrans = TransformDataPngObject.defaultTrans;
            var updateTransform = false;

            updateTransform |= view.DrawToggle(pngObject.displayName, pngObject.enable, 200, 20, (visible) =>
            {
                pngObject.SetEnable(visible);
            });

            view.DrawLabel("移動", 200, 20);

            {
                var initialPosition = defaultTrans.initialPosition;
                var transformCache = view.GetTransformCache(null);
                transformCache.position = pngObject.transform.localPosition;

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    pngObject.transform.localPosition = transformCache.position;
                }
            }

            view.DrawLabel("回転", 200, 20);

            {
                var initialEulerAngles = defaultTrans.initialEulerAngles;
                var transformCache = view.GetTransformCache(null);
                transformCache.eulerAngles = pngObject.rotation;

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    pngObject.displayName,
                    initialEulerAngles);

                if (updateTransform)
                {
                    pngObject.SetRotation(transformCache.eulerAngles);
                }
            }

            view.DrawLabel("色", 200, 20);

            updateTransform |= view.DrawColor(
                _colorFieldValue,
                pngObject.color,
                defaultTrans.initialColor,
                c => pngObject.SetColor(c));

            updateTransform |= view.DrawCustomValueInt(
                defaultTrans.brightnessInfo,
                pngObject.brightness,
                y => pngObject.SetBrightness((byte) y));

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.scalexInfo,
                pngObject.scale,
                y => pngObject.SetScale(y));

            updateTransform |= view.DrawCustomValueInt(
                defaultTrans.scalemagInfo,
                pngObject.scaleMag,
                y => pngObject.SetScaleMag(y));

            updateTransform |= view.DrawCustomValueBool(
                defaultTrans.stoprotationInfo,
                pngObject.stopRotation,
                y => pngObject.SetStopRotation(y));

            view.DrawLabel("停止角度", 200, 20);

            {
                var initialEulerAngles = defaultTrans.initialEulerAngles;
                var transformCache = view.GetTransformCache(null);
                var prevEulerAngles = Vector3.zero;
                transformCache.eulerAngles = pngObject.stopRotationVector;

                updateTransform |= DrawEulerAngles(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    prevEulerAngles,
                    initialEulerAngles);

                if (updateTransform)
                {
                    pngObject.SetStopRotationVector(transformCache.eulerAngles);
                }
            }

            updateTransform |= view.DrawCustomValueBool(
                defaultTrans.fixcameraInfo,
                pngObject.fixedCamera,
                y => pngObject.SetFixedCamera(y));

            view.DrawLabel("固定位置", 200, 20);

            {
                var initialPosition = defaultTrans.initialPosition;
                var transformCache = view.GetTransformCache(null);
                transformCache.position = pngObject.fixedPos;

                updateTransform |= DrawPosition(
                    view,
                    transformCache,
                    TransformEditType.全て,
                    initialPosition);

                if (updateTransform)
                {
                    pngObject.SetFixedPos(transformCache.position);
                }
            }

            updateTransform |= view.DrawCustomValueBool(
                defaultTrans.inversionInfo,
                pngObject.inversion,
                y => pngObject.SetInversion(y));

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.scalezInfo,
                pngObject.scaleZ,
                y => pngObject.SetScaleZ(y));

            _attachPointComboBox.currentIndex = (int) pngObject.attach;
            _attachPointComboBox.onSelected = (attachPoint, _) =>
            {
                pngObject.SetAttachPoint(attachPoint, pngObject.maid);
                updateTransform = true;
            };
            _attachPointComboBox.DrawButton("アタッチ", view);

            _maidComboBox.items = maidManager.maidCaches;
            _maidComboBox.currentItem = maidManager.maidCache;
            _maidComboBox.onSelected = (maidCache, index) =>
            {
                pngObject.SetAttachPoint(pngObject.attach, index);
                updateTransform = true;
            };
            _maidComboBox.DrawButton("対象メイド", view);

            updateTransform |= view.DrawCustomValueBool(
                defaultTrans.attachrotationInfo,
                pngObject.attachRotation,
                y => pngObject.SetAttachRotation(y));

            if (pngObject.apng != null)
            {
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.apngspeedInfo,
                    pngObject.apngAnm.speed,
                    y => pngObject.SetAPNGSpeed(y));

                updateTransform |= view.DrawCustomValueBool(
                    defaultTrans.apngisfixedspeedInfo,
                    pngObject.apngAnm.isFixedSpeed,
                    y => pngObject.SetAPNGIsFixedSpeed(y));
            }

            if (updateTransform)
            {
                pngPlacementManager.LateUpdate(true);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.PngObject;
        }
    }
}