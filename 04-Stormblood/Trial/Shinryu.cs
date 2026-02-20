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

namespace theRoyalMenagerie;

[ScriptType(guid: "da23fd13-2d1f-41d3-b2c9-91fd8d948a98", name: "神龙歼灭战", territorys: [679],
    version: "0.0.0.5", author: "Tetora", note: noteStr)]

public class Shinryu
{
    const string noteStr =
        """
        v0.0.0.5:
        LV70 神龙歼灭战 初版绘制
        缺少部分机制：如闪电分散.
        若有遇见可通过DC将ARR录像文件私发给我补充
        """;
    
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    
    [ScriptMethod(name: "巨浪 击退连线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8075"])]
    public void 巨浪连线(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("靠近水柱击退", duration: 9700, true);
        if (isTTS)accessory.Method.TTS($"靠近水柱击退");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"靠近水柱击退");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "巨浪连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 9700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "巨浪（击退预测）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8075"])]
    public void 巨浪击退预测(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "巨浪击退";
        dp.Scale = new(1f, 35f);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3f);
        dp.Owner = accessory.Data.Me;
        dp.Rotation = @event.SourceRotation();
        dp.FixRotation = true;
        dp.DestoryAt = 9700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "巨浪击退销毁", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(160|1209|2663)$"],userControl: false)]
    public void 巨浪防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($"巨浪.*");
    }
    
    [ScriptMethod(name: "超新星（分摊）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:003E"])]
    public void 超新星(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "超新星";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "地狱之火炎 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8107"])]
    public void 地狱之火炎(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE，进入水圈", duration: 9700, true);
        if (isTTS)accessory.Method.TTS($"AOE，进入水圈");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE，进入水圈");
    }
    
    [ScriptMethod(name: "制裁之雷 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8108"])]
    public void 制裁之雷(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE，离开水圈", duration: 9700, false);
        if (isTTS)accessory.Method.TTS($"AOE，离开水圈");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE，离开水圈");
        
        foreach (var item in accessory.Data.Objects.GetByDataId(2004237))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = new Vector4(1f, 0f, 0f, 4f);
            dp.Scale = new Vector2(5f);
            dp.DestoryAt = 9700;
            dp.Name = "水圈";
            dp.Owner = item.EntityId;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "右翼_闪电 分散", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8091"])]
    public void 闪电(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("分散，离开水圈", duration: 5700, true);
        if (isTTS)accessory.Method.TTS($"分散，离开水圈");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"分散，离开水圈");
    }
    
    [ScriptMethod(name: "陨石冲击", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9291"])]
    public void 陨石冲击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "陨石冲击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 4700;
        // dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "尾部猛击（砸地板）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8083"])]
    public void 尾部猛击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "尾部猛击";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(20f, 40f); 
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "攻击龙尾提示", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True","SourceName:regex:^(龙尾|tail|神龍の尾)$"])]
    public void 龙尾(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("攻击龙尾", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"攻击龙尾");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"攻击龙尾");
    }
    
    volatile int BurningChainsMarkingPlayers=0;
    
    [ScriptMethod(name:"烈焰链点名记录", eventType:EventTypeEnum.TargetIcon, eventCondition:["Id:0061"], userControl:false)]
    public void 烈焰链点名记录(Event @event, ScriptAccessory accessory) 
    {
        BurningChainsMarkingPlayers=1;
    }
    
    [ScriptMethod(name: "烈焰链 引导提示", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0061"])]
    public void 烈焰链(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return;
        if (isText) accessory.Method.TextInfo("靠近引导连线", duration: 3000, false);
        if (isTTS)accessory.Method.TTS($"靠近引导连线");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"靠近引导连线");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "引导";
        dp.Color = new Vector4(0f, 1f, 0f, 3f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(1f);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "烈焰链 扯线提示", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0009"])]
    public void 烈焰链2(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return;
        if (isText) accessory.Method.TextInfo("扯断连线", duration: 2500, true);
        if (isTTS)accessory.Method.TTS($"扯断连线");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"扯断连线");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "烈焰链扯线";
        dp.Color = new Vector4(1f, 0f, 0f, 3f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(1f);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "烈焰链扯断销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:769"],userControl: false)]
    public void 烈焰链扯断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("烈焰链扯线");
    }
    
    [ScriptMethod(name: "钻石星尘 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8109"])]
    public async void 钻石星尘(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(7700); 
        if (isText) accessory.Method.TextInfo("停止移动！", duration: 2000, true);
        if (isTTS)accessory.Method.TTS($"停止移动");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"停止移动");
    }
    
    [ScriptMethod(name: "大气爆发 击退", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8080"])]
    public void 大气爆发(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("连续中心击退", duration: 9700, true);
        if (isTTS)accessory.Method.TTS($"连续中心击退");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"连续中心击退");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "大气爆发";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "台风眼", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6935"])]
    public void 台风眼(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "台风眼";
        dp.Color = new Vector4(1f, 0f, 0f, 3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 10800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "台风眼销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:6935"],userControl: false)]
    public void 台风眼销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("台风眼");
    }
    
    [ScriptMethod(name: "左翼_吹雪（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8098"])]
    public void 吹雪(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 5700, false);
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
}

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

    public static uint StatusId(this Event @event)
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
public static class MathHelpers
{
    public static float DegToRad(float degrees)
    {
        return degrees * (float)(Math.PI / 180.0);
    }
    
    public static double DegToRad(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    
    public static float RadToDeg(float radians)
    {
        return radians * (float)(180.0 / Math.PI);
    }
    
    public static double RadToDeg(double radians)
    {
        return radians * 180.0 / Math.PI;
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
