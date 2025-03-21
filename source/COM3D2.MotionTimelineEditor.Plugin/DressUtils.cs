using System;
using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public enum DressSlotID
    {
        wear,
        onepiece,
        mizugi,
        skirt,
        panz,
        bra,
        stkg,
        shoes,
        glove,

        headset,
        accHat,
        accKamiSubL,
        accKamiSubR,
        accKami_1_,
        accKami_2_,
        accKami_3_,

        accHead,
        accHana,
        accMiMiL,
        accMiMiR,
        accNipL,
        accNipR,
        accKubi,
        accKubiwa,
        accHa,
        accHeso,
        accUde,
        accAshi,
        accSenaka,
        accShippo,
        accAnl,
        accVag,
        accXXX,
        hairAho,
        kubiwa,
        megane,
        HandItemL,
        HandItemR,
        kousoku_upper,
        kousoku_lower,

        undressfront,
        undressback,
        undressshift,
    }

    public static class DressUtils
    {
        private static StudioHackBase studioHack => StudioHackManager.instance.studioHack;

        public static readonly List<DressSlotID> ClothingSlotIds = new List<DressSlotID>
        {
            DressSlotID.wear,
            DressSlotID.onepiece,
            DressSlotID.mizugi,
            DressSlotID.skirt,
            DressSlotID.panz,
            DressSlotID.bra,
            DressSlotID.stkg,
            DressSlotID.shoes,
            DressSlotID.glove,
        };

        public static readonly List<DressSlotID> HeadwearSlotIds = new List<DressSlotID>
        {
            DressSlotID.headset,
            DressSlotID.accHat,
            DressSlotID.accKamiSubL,
            DressSlotID.accKamiSubR,
            DressSlotID.accKami_1_,
            DressSlotID.accKami_2_,
            DressSlotID.accKami_3_,
        };

        public static readonly List<DressSlotID> AccessorySlotIds = new List<DressSlotID>
        {
            DressSlotID.accHead,
            DressSlotID.accHana,
            DressSlotID.accMiMiL,
            DressSlotID.accMiMiR,
            DressSlotID.accNipL,
            DressSlotID.accNipR,
            DressSlotID.accKubi,
            DressSlotID.accKubiwa,
            DressSlotID.accHa,
            DressSlotID.accHeso,
            DressSlotID.accUde,
            DressSlotID.accAshi,
            DressSlotID.accSenaka,
            DressSlotID.accShippo,
            DressSlotID.accAnl,
            DressSlotID.accVag,
            DressSlotID.accXXX,
            DressSlotID.hairAho,
            DressSlotID.kubiwa,
            DressSlotID.megane,
            DressSlotID.HandItemL,
            DressSlotID.HandItemR,
            DressSlotID.kousoku_upper,
            DressSlotID.kousoku_lower,
        };

        public static readonly List<DressSlotID> MekureSlotIds = new List<DressSlotID>
        {
            DressSlotID.undressfront,
            DressSlotID.undressback,
            DressSlotID.undressshift,
        };

        private static List<string> _dressSlotNames = null;
        public static List<string> DressSlotNames
        {
            get
            {
                if (_dressSlotNames == null)
                {
                    _dressSlotNames = new List<string>(Enum.GetNames(typeof(DressSlotID)));
                }
                return _dressSlotNames;
            }
        }

        public static string ToName(this DressSlotID slotId)
        {
            return DressSlotNames.GetOrDefault((int)slotId);
        }

        public static readonly Dictionary<DressSlotID, string> DressSlotJpNameMap = new Dictionary<DressSlotID, string>()
        {
            { DressSlotID.wear, "トップス" },
            { DressSlotID.onepiece, "ワンピース" },
            { DressSlotID.mizugi, "水着" },
            { DressSlotID.skirt, "ボトムス" },
            { DressSlotID.panz, "パンツ" },
            { DressSlotID.bra, "ブラ" },
            { DressSlotID.stkg, "靴下" },
            { DressSlotID.shoes, "靴" },
            { DressSlotID.glove, "手袋" },

            { DressSlotID.headset, "ヘッドドレス" },
            { DressSlotID.accHat, "帽子" },
            { DressSlotID.accKamiSubL, "リボン L" },
            { DressSlotID.accKamiSubR, "リボン R" },
            { DressSlotID.accKami_1_, "前髪 1" },
            { DressSlotID.accKami_2_, "前髪 2" },
            { DressSlotID.accKami_3_, "前髪 3" },

            { DressSlotID.accHead, "アイマスク" },
            { DressSlotID.accHana, "鼻" },
            { DressSlotID.accMiMiL, "耳 L" },
            { DressSlotID.accMiMiR, "耳 R" },
            { DressSlotID.accNipL, "乳首 L" },
            { DressSlotID.accNipR, "乳首 R" },
            { DressSlotID.accKubi, "ネックレス" },
            { DressSlotID.accKubiwa, "チョーカー" },
            { DressSlotID.accHa, "歯" },
            { DressSlotID.accHeso, "へそ" },
            { DressSlotID.accUde, "腕" },
            { DressSlotID.accAshi, "足首" },
            { DressSlotID.accSenaka, "背中" },
            { DressSlotID.accShippo, "尻尾" },
            { DressSlotID.accAnl, "アナル" },
            { DressSlotID.accVag, "膣" },
            { DressSlotID.accXXX, "前穴" },
            { DressSlotID.hairAho, "アホ毛" },
            { DressSlotID.kubiwa, "首輪" },
            { DressSlotID.megane, "メガネ" },
            { DressSlotID.HandItemL, "左手アイテム" },
            { DressSlotID.HandItemR, "右手アイテム" },
            { DressSlotID.kousoku_upper, "拘束 上" },
            { DressSlotID.kousoku_lower, "拘束 下" },

            { DressSlotID.undressfront, "めくれ前" },
            { DressSlotID.undressback, "めくれ後ろ" },
            { DressSlotID.undressshift, "ずらし" },
        };

        public static string GetDressSlotJpName(DressSlotID slotId)
        {
            if (DressSlotJpNameMap.ContainsKey(slotId))
            {
                return DressSlotJpNameMap[slotId];
            }
            return slotId.ToString();
        }

        public static DressSlotID GetDressSlotId(string slotName)
        {
            try
            {
                return (DressSlotID)Enum.Parse(typeof(DressSlotID), slotName);
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                return DressSlotID.wear;
            }
        }

        public static string GetDressSlotJpName(string slotName)
        {
            return GetDressSlotJpName(GetDressSlotId(slotName));
        }

        public static bool IsShiftSlotId(this DressSlotID slotId)
        {
            return slotId == DressSlotID.undressfront
                || slotId == DressSlotID.undressback
                || slotId == DressSlotID.undressshift;
        }

        private static Dictionary<DressSlotID, TBody.SlotID> _bodySlotIdMap = null;
        public static Dictionary<DressSlotID, TBody.SlotID> BodySlotIdMap
        {
            get
            {
                if (_bodySlotIdMap == null)
                {
                    _bodySlotIdMap = new Dictionary<DressSlotID, TBody.SlotID>(DressSlotNames.Count);

                    for (var i = 0; i < DressSlotNames.Count; i++)
                    {
                        var slotId = (DressSlotID)i;
                        if (slotId.IsShiftSlotId())
                        {
                            continue;
                        }

                        var slotName = DressSlotNames[i];
                        try
                        {
                            var bodySlotId = (TBody.SlotID)Enum.Parse(typeof(TBody.SlotID), slotName);
                            _bodySlotIdMap[slotId] = bodySlotId;
                        }
                        catch (Exception e)
                        {
                            MTEUtils.LogException(e);
                            _bodySlotIdMap[slotId] = TBody.SlotID.body;
                        }
                    }
                }
                return _bodySlotIdMap;
            }
        }

        public static TBody.SlotID GetBodySlotId(DressSlotID slotId)
        {
            return BodySlotIdMap.GetOrDefault(slotId, TBody.SlotID.body);
        }

        public static void SetMask(this MaidCache maidCache, TBody.SlotID slotId, bool visible)
        {
            var body = maidCache.maid.body0;
            if (body == null || body.GetMask(slotId) == visible)
            {
                return;
            }
            body.SetMask(slotId, visible);
        }

        public static bool GetMask(this MaidCache maidCache, TBody.SlotID slotId)
        {
            var body = maidCache.maid.body0;
            if (body == null)
            {
                return false;
            }
            return body.GetMask(slotId);
        }

        public static bool GetSlotLoaded(this MaidCache maidCache, TBody.SlotID slotId)
        {
            var body = maidCache.maid.body0;
            if (body == null)
            {
                return false;
            }

            return body.GetSlotLoaded(slotId);
        }

        public static bool IsSlotLoaded(this MaidCache maidCache, DressSlotID slotId)
        {
            if (IsShiftSlotId(slotId))
            {
                return true;
            }
            else
            {
                var bodySlotId = GetBodySlotId(slotId);
                return maidCache.GetSlotLoaded(bodySlotId);
            }
        }

        public static bool IsSlotVisible(this MaidCache maidCache, DressSlotID slotId)
        {
            if (IsShiftSlotId(slotId))
            {
                return maidCache.IsShifted(slotId);
            }
            else
            {
                var bodySlotId = GetBodySlotId(slotId);
                return maidCache.GetMask(bodySlotId);
            }
        }

        public static void SetSlotVisible(this MaidCache maidCache, DressSlotID slotId, bool isVisible)
        {
            if (IsShiftSlotId(slotId))
            {
                if (maidCache.IsShifted(slotId) != isVisible)
                {
                    maidCache.ShiftDeress(slotId, isVisible);
                    studioHack.UpdateUndress(maidCache.maid, slotId, isVisible);
                }
            }
            else
            {
                var bodySlotId = GetBodySlotId(slotId);
                if (maidCache.GetMask(bodySlotId) != isVisible)
                {
                    maidCache.SetMask(bodySlotId, isVisible);
                    studioHack.UpdateUndress(maidCache.maid, slotId, isVisible);
                }
            }
        }

        public static bool IsShifted(this MaidCache maidCache, DressSlotID slotId)
        {
            var maid = maidCache.maid;
            if (maid == null)
            {
                return false;
            }

            switch (slotId)
			{
                case DressSlotID.undressfront:
                    return maid.IsItemChange("skirt", "めくれスカート") || maid.IsItemChange("onepiece", "めくれスカート");
                case DressSlotID.undressback:
                    return maid.IsItemChange("skirt", "めくれスカート後ろ") || maid.IsItemChange("onepiece", "めくれスカート後ろ");
                case DressSlotID.undressshift:
                    return maid.IsItemChange("panz", "パンツずらし") || maid.IsItemChange("mizugi", "パンツずらし");
			}

            return false;
        }

        public static void ShiftDeress(this MaidCache maidCache, DressSlotID slotId, bool isShifted)
		{
            var maid = maidCache.maid;
            if (maid == null)
            {
                return;
            }

			switch (slotId)
			{
                case DressSlotID.undressfront:
                    if (isShifted)
                    {
                        maid.ItemChangeTemp("skirt", "めくれスカート");
                        maid.ItemChangeTemp("onepiece", "めくれスカート");
                    }
                    else
                    {
                        maid.ResetProp("skirt", false);
                        maid.ResetProp("onepiece", false);
                    }
                    break;
                case DressSlotID.undressback:
                    if (isShifted)
                    {
                        maid.ItemChangeTemp("skirt", "めくれスカート後ろ");
                        maid.ItemChangeTemp("onepiece", "めくれスカート後ろ");
                    }
                    else
                    {
                        maid.ResetProp("skirt", false);
                        maid.ResetProp("onepiece", false);
                    }
                    break;
                case DressSlotID.undressshift:
                    if (isShifted)
                    {
                        maid.ItemChangeTemp("panz", "パンツずらし");
                        maid.ItemChangeTemp("mizugi", "パンツずらし");
                    }
                    else
                    {
                        maid.ResetProp("panz", false);
                        maid.ResetProp("mizugi", false);
                    }
                    break;
			}

            maid.AllProcProp();
		}
    }
}