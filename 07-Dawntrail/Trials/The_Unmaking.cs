using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace The_Unmaking;

[ScriptType(guid: "1d876fc3-e0b4-405f-93f7-6892cc1f22e5", name: "恩欧歼灭战", territorys: [1361],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class The_Unmaking
{
    const string noteStr =
        """
        v0.0.0.1:
        恩欧歼灭战 初版绘制
        """;
    
    #region 用户控制

    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("启用小世界直线随时间绘制")]
    public bool isLineByTime { get; set; } = false;
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;

    #endregion
    
    uint TargetMe=0;
    
    public void Init(ScriptAccessory accessory) {
        TargetMe=0;
    }
    
    #region TTS部分
    
    [ScriptMethod(name: "流星雨 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49971$"])]
    public void 流星雨(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"AOE", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "深度冻结 核爆", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49966$"])]
    public async void 深度冻结(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me) TargetMe = 1;
        
        await Task.Delay(500);
        if (TargetMe != 1)
        {
            // if (isText)accessory.Method.TextInfo($"远离核爆", duration: 5000, false);
            if (isTTS)accessory.Method.TTS($"远离核爆");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"远离核爆");
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 远离核爆");
        }
        else
        {
            // if (isText)accessory.Method.TextInfo($"核爆点名", duration: 5000, true);
            if (isTTS)accessory.Method.TTS($"核爆点名");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"核爆点名");
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 核爆点名");
        }
    }
    
    [ScriptMethod(name: "无之追踪_追尾波动", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^019[45]$"],suppress: 8000)]
    public async void 追尾波动(Event @event, ScriptAccessory accessory)
    {
        // 无之追踪 49939 ; 无之再追踪 49941 ; 追尾波动 48474
        if (@event.TargetId() == accessory.Data.Me) TargetMe = 1;
        
        await Task.Delay(500);
        if (TargetMe != 1)
        {
            // if (isText)accessory.Method.TextInfo($"远离连线玩家", duration: 5000, false);
            if (isTTS)accessory.Method.TTS($"远离连线玩家");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"远离连线玩家");
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 远离连线玩家");
        }
        else
        {
            if (isText)accessory.Method.TextInfo($"追踪AOE点名", duration: 5000, true);
            if (isTTS)accessory.Method.TTS($"追踪AOE点名");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"追踪AOE点名");
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 追踪AOE点名");
        }
    }
    
    [ScriptMethod(name: "核心熔毁 引导+分散", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49962$"])]
    public void 核心熔毁(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"引导黄圈+分散", duration: 3000, true);
        if (isTTS)accessory.Method.TTS($"引导黄圈+分散");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"引导黄圈+分散");
    }
    
    [ScriptMethod(name: "至高无上 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49928$"])]
    public void 至高无上(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"AOE", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    #endregion
    
    #region 绘制部分
    
    [ScriptMethod(name: "无之膨胀（无之巨漩涡）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49933$"])]
    public void 无之巨漩涡_无之膨胀(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"无之巨漩涡_无之膨胀{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "无之膨胀（BOSS本体）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49934$"])]
    public void 无之膨胀(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"无之膨胀";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 6700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"无之膨胀描边";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(12f);
        dp1.InnerScale = new Vector2(11.96f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "混沌激流（大风车）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4995[123]$"])]
    public void 混沌激流 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"混沌激流{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 45f.DegToRad();
        dp.Delay = @event.ActionId() == 49953 ? 4200 : 0;
        dp.DestoryAt = @event.ActionId() == 49953 ? 3500 : 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "暗影神圣（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49968$"])]
    public void 暗影神圣(Event @event, ScriptAccessory accessory)
    {
        string targetName = @event["TargetName"]?.ToString();
        if (!string.IsNullOrEmpty(targetName))
        {
            if (isText)accessory.Method.TextInfo($"靠近{targetName}集合", duration: 4700, true);
            if (isTTS) accessory.Method.TTS($"靠近{targetName}集合");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"靠近{targetName}集合");
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"暗影神圣";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "无之漩涡_奔流（末端钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4994[345]$"])]
    public void 奔流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"奔流{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.75f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(7f);
        dp.Delay = 1000;
        dp.DestoryAt = 5000;
        // dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    /* 范围待确定
    [ScriptMethod(name: "虚无大冲击（转场核爆）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49981$"])]
    public void 虚无大冲击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"虚无大冲击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.DestoryAt = 6700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    */
    
    [ScriptMethod(name: "虚无冲击（黄圈动画）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50667$"])]
    public void 虚无冲击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"虚无冲击{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 7700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "无之波动（直线动画）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49958$"])]
    public void 无之波动(Event @event, ScriptAccessory accessory)
    {
        if (!isLineByTime) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"无之波动{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 100f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    private ulong _StackId = 0; 
    
    [ScriptMethod(name: "零次元（直线分摊点名）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^02CF$"])]
    public void 零次元点名(Event @event, ScriptAccessory accessory)
    {
        _StackId = @event.TargetId();
    }
    
    [ScriptMethod(name: "零次元（直线分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49969$"])]
    public void 零次元(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"零次元";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.SourceId();
        dp.TargetObject = _StackId;
        dp.Scale = new (8f, 60f);
        dp.DestoryAt = 8100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    
    #endregion

    #region  清除
    
    [ScriptMethod(name: "选中目标清除", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(49966|49939|49941|48474|49969|49970)$"], userControl: false)]
    public void 选中目标清除(Event @event, ScriptAccessory accessory)
    {
        TargetMe = 0;
    }
    
    [ScriptMethod(name: "小怪咏唱中断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: [], userControl: false)]
    public void 咏唱中断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "小怪死亡销毁", eventType: EventTypeEnum.Death, eventCondition: [], userControl: false)]
    public void 死亡销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }
    
    #endregion
    
}

#region EventExtensions

public static class EventExtensions
{
    private static bool ParseHexId(string? idStr, out uint id)
    {
        id = 0;
        if (string.IsNullOrEmpty(idStr)) return false;
        try
        {
            var idStr2 = idStr.Replace("0x", "");
            id = uint.Parse(idStr2, System.Globalization.NumberStyles.HexNumber);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static uint ActionId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
    }

    public static uint SourceId(this Event @event)
    {
        return ParseHexId(@event["SourceId"], out var id) ? id : 0;
    }

    public static uint SourceDataId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["SourceDataId"]);
    }

    public static uint Command(this Event @event)
    {
        return ParseHexId(@event["Command"], out var cid) ? cid : 0;
    }
    
    public static uint DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DurationMilliseconds"]);
    }

    public static float SourceRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
    }

    public static float TargetRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["TargetRotation"]);
    }

    public static byte Index(this Event @event)
    {
        return (byte)(ParseHexId(@event["Index"], out var index) ? index : 0);
    }

    public static uint State(this Event @event)
    {
        return ParseHexId(@event["State"], out var state) ? state : 0;
    }

    public static string SourceName(this Event @event)
    {
        return @event["SourceName"];
    }

    public static string TargetName(this Event @event)
    {
        return @event["TargetName"];
    }

    public static uint TargetId(this Event @event)
    {
        return ParseHexId(@event["TargetId"], out var id) ? id : 0;
    }

    public static Vector3 SourcePosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
    }

    public static Vector3 TargetPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
    }

    public static Vector3 EffectPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
    }

    public static uint DirectorId(this Event @event)
    {
        return ParseHexId(@event["DirectorId"], out var id) ? id : 0;
    }

    public static uint StatusID(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StatusId"]);
    }

    public static uint StackCount(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StackCount"]);
    }

    public static uint Param(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["Param"]);
    }
}

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;
        return attribute?.Description ?? value.ToString();
    }
}

public static class MathTools
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;

    /// <summary>
    /// 获得任意点与中心点的弧度值，以(0, 0, 1)方向为0，以(1, 0, 0)方向为pi/2。
    /// 即，逆时针方向增加。
    /// </summary>
    /// <param name="point">任意点</param>
    /// <param name="center">中心点</param>
    /// <returns></returns>
    public static float GetRadian(this Vector3 point, Vector3 center)
        => MathF.Atan2(point.X - center.X, point.Z - center.Z);

    /// <summary>
    /// 获得任意点与中心点的长度。
    /// </summary>
    /// <param name="point">任意点</param>
    /// <param name="center">中心点</param>
    /// <returns></returns>
    public static float GetLength(this Vector3 point, Vector3 center)
        => new Vector2(point.X - center.X, point.Z - center.Z).Length();

    /// <summary>
    /// 将任意点以中心点为圆心，逆时针旋转并延长。
    /// </summary>
    /// <param name="point">任意点</param>
    /// <param name="center">中心点</param>
    /// <param name="radian">旋转弧度</param>
    /// <param name="length">基于该点延伸长度</param>
    /// <returns></returns>
    public static Vector3 RotateAndExtend(this Vector3 point, Vector3 center, float radian, float length)
    {
        var baseRad = point.GetRadian(center);
        var baseLength = point.GetLength(center);
        var rotRad = baseRad + radian;
        return new Vector3(
            center.X + MathF.Sin(rotRad) * (length + baseLength),
            center.Y,
            center.Z + MathF.Cos(rotRad) * (length + baseLength)
        );
    }

    /// <summary>
    /// 获得某角度所在划分区域
    /// </summary>
    /// <param name="radian">输入弧度</param>
    /// <param name="regionNum">区域划分数量</param>
    /// <param name="baseRegionIdx">0度所在区域的初始Idx</param>>
    /// <param name="isDiagDiv">是否为斜分割，默认为false</param>
    /// <param name="isCw">是否顺时针增加，默认为false</param>
    /// <returns></returns>
    public static int RadianToRegion(this float radian, int regionNum, int baseRegionIdx = 0, bool isDiagDiv = false, bool isCw = false)
    {
        var sepRad = float.Pi * 2 / regionNum;
        var inputAngle = radian * (isCw ? -1 : 1) + (isDiagDiv ? sepRad / 2 : 0);
        var rad = (inputAngle + 4 * float.Pi) % (2 * float.Pi);
        return ((int)Math.Floor(rad / sepRad) + baseRegionIdx + regionNum) % regionNum;
    }

    /// <summary>
    /// 将输入点左右折叠
    /// </summary>
    /// <param name="point">待折叠点</param>
    /// <param name="centerX">中心折线坐标点</param>
    /// <returns></returns>
    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
        => point with { X = 2 * centerX - point.X };

    /// <summary>
    /// 将输入点上下折叠
    /// </summary>
    /// <param name="point">待折叠点</param>
    /// <param name="centerZ">中心折线坐标点</param>
    /// <returns></returns>
    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
        => point with { Z = 2 * centerZ - point.Z };

    /// <summary>
    /// 将输入点中心对称
    /// </summary>
    /// <param name="point">输入点</param>
    /// <param name="center">中心点</param>
    /// <returns></returns>
    public static Vector3 PointCenterSymmetry(this Vector3 point, Vector3 center)
        => point.RotateAndExtend(center, float.Pi, 0);

    /// <summary>
    /// 获取给定数的指定位数
    /// </summary>
    /// <param name="val">给定数值</param>
    /// <param name="x">对应位数，个位为1</param>
    /// <returns></returns>
    public static int GetDecimalDigit(this int val, int x)
    {
        var valStr = val.ToString();
        var length = valStr.Length;
        if (x < 1 || x > length) return -1;
        var digitChar = valStr[length - x]; // 从右往左取第x位
        return int.Parse(digitChar.ToString());
    }
}

public enum MarkType
{
    None = -1,
    Attack1 = 0,
    Attack2 = 1,
    Attack3 = 2,
    Attack4 = 3,
    Attack5 = 4,
    Bind1 = 5,
    Bind2 = 6,
    Bind3 = 7,
    Ignore1 = 8,
    Ignore2 = 9,
    Square = 10,
    Circle = 11,
    Cross = 12,
    Triangle = 13,
    Attack6 = 14,
    Attack7 = 15,
    Attack8 = 16,
    Count = 17
}

public static class IbcHelper
{
    public static IGameObject? GetById(this ScriptAccessory sa, ulong gameObjectId)
    {
        return sa.Data.Objects.SearchById(gameObjectId);
    }

    public static IGameObject? GetMe(this ScriptAccessory sa)
    {
        return sa.Data.Objects.LocalPlayer;
    }

    public static IEnumerable<IGameObject?> GetByDataId(this ScriptAccessory sa, uint dataId)
    {
        return sa.Data.Objects.Where(x => x.DataId == dataId);
    }

    public static string GetPlayerJob(this ScriptAccessory sa, IPlayerCharacter? playerObject, bool fullName = false)
    {
        if (playerObject == null) return "None";
        return fullName ? playerObject.ClassJob.Value.Name.ToString() : playerObject.ClassJob.Value.Abbreviation.ToString();
    }

    public static float GetStatusRemainingTime(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return 0;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return charaStruct->GetStatusManager()->GetRemainingTime(statusIdx);
        }
    }

    public static bool HasStatus(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return false;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return statusIdx != -1;
        }
    }

    /// <summary>
    /// 获取指定标记索引的对象EntityId
    /// </summary>
    public static unsafe ulong GetMarkerEntityId(uint markerIndex)
    {
        var markingController = MarkingController.Instance();
        if (markingController == null) return 0;
        if (markerIndex >= 17) return 0;

        return markingController->Markers[(int)markerIndex];
    }

    /// <summary>
    /// 获取对象身上的标记
    /// </summary>
    /// <returns>MarkType</returns>
    public static MarkType GetObjectMarker(IGameObject? obj)
    {
        if (obj == null || !obj.IsValid()) return MarkType.None;

        ulong targetEntityId = obj.EntityId;
            
        for (uint i = 0; i < 17; i++)
        {
            var markerEntityId = GetMarkerEntityId(i);
            if (markerEntityId == targetEntityId)
            {
                return (MarkType)i;
            }
        }

        return MarkType.None;
    }

    /// <summary>
    /// 检查对象是否有指定的标记
    /// </summary>
    public static bool HasMarker(IGameObject? obj, MarkType markType)
    {
        return GetObjectMarker(obj) == markType;
    }

    /// <summary>
    /// 检查对象是否有任何标记
    /// </summary>
    public static bool HasAnyMarker(IGameObject? obj)
    {
        return GetObjectMarker(obj) != MarkType.None;
    }

    private static ulong GetMarkerForObject(IGameObject? obj)
    {
        if (obj == null) return 0;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return markerEntityId;
                }
            }
        }
        return 0;
    }

    private static MarkType GetMarkerTypeForObject(IGameObject? obj)
    {
        if (obj == null) return MarkType.None;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return (MarkType)i;
                }
            }
        }
        return MarkType.None;
    }

    /// <summary>
    /// 获取标记的名称
    /// </summary>
    public static string GetMarkerName(MarkType markType)
    {
        return markType switch
        {
            MarkType.Attack1 => "攻击1",
            MarkType.Attack2 => "攻击2",
            MarkType.Attack3 => "攻击3",
            MarkType.Attack4 => "攻击4",
            MarkType.Attack5 => "攻击5",
            MarkType.Bind1 => "止步1",
            MarkType.Bind2 => "止步2",
            MarkType.Bind3 => "止步3",
            MarkType.Ignore1 => "禁止1",
            MarkType.Ignore2 => "禁止2",
            MarkType.Square => "方块",
            MarkType.Circle => "圆圈",
            MarkType.Cross => "十字",
            MarkType.Triangle => "三角",
            MarkType.Attack6 => "攻击6",
            MarkType.Attack7 => "攻击7",
            MarkType.Attack8 => "攻击8",
            _ => "无标记"
        };
    }
    
    public static float GetHitboxRadius(IGameObject obj)
    {
        if (obj == null || !obj.IsValid()) return -1;
        return obj.HitboxRadius;
    }

}

public static class HelperExtensions
{
    public static unsafe uint GetCurrentTerritoryId()
    {
        return AgentMap.Instance()->CurrentTerritoryId; // 额外进行地图ID判断
    }
}

public unsafe static class ExtensionVisibleMethod
{
    public static bool IsCharacterVisible(this ICharacter chr)
    {
        var v = (IntPtr)(((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)chr.Address)->GameObject.DrawObject);
        if (v == IntPtr.Zero) return false;
        return Bitmask.IsBitSet(*(byte*)(v + 136), 0);
    }

    public static class Bitmask
    {
        public static bool IsBitSet(ulong b, int pos)
        {
            return (b & (1UL << pos)) != 0;
        }

        public static void SetBit(ref ulong b, int pos)
        {
            b |= 1UL << pos;
        }

        public static void ResetBit(ref ulong b, int pos)
        {
            b &= ~(1UL << pos);
        }

        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static bool IsBitSet(short b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
#endregion EventExtensions
