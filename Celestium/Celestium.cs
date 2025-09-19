using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
// using Dalamud.Game.ClientState.Objects.Subkinds;
// using Dalamud.Game.ClientState.Objects.Types;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using System.Threading.Tasks;

namespace Celestium;


[ScriptType(guid: "7703f1a9-5698-4896-8908-bb8e415c1321", name: "天青斗场", territorys: [796],
    version: "0.0.0.6", author: "Tetora", note: noteStr)]

public class Celestium {
    const string noteStr =
        """
        v0.0.0.5:
        天青斗场绘制及机制提醒，随缘更新中
        若无法更新请删除后刷新重新下载
        目前支持层数：18 [爆破死斗]
        """;
    
    #region 基础控制
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    #endregion

    #region 03：最初的岩壁——希帕克纳
    
    [ScriptMethod(name: "—————— 03：最初的岩壁——希帕克纳 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第3层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "破魔震（打断）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14365"])]
    public void 破魔震(Event @event, ScriptAccessory accessory)
    {
        if(isText)accessory.Method.TextInfo("打断BOSS", duration: 3000, true);
        if(isTTS) accessory.Method.TTS("打断BOSS");
        if(isEdgeTTS) accessory.Method.EdgeTTS("打断BOSS");
    }
    
    #endregion
    
    #region 08：青之牙、红之牙
    
    [ScriptMethod(name: "—————— 08：青之牙、红之牙 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第8层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "大爆炸（打断）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14680"])]
    public void 大爆炸(Event @event, ScriptAccessory accessory)
    {
        if(isText)accessory.Method.TextInfo("打断BOSS", duration: 3000, true);
        if(isTTS) accessory.Method.TTS("打断BOSS");
        if(isEdgeTTS) accessory.Method.EdgeTTS("打断BOSS");
    }
    
    #endregion
    
    #region 18：爆破死斗

    [ScriptMethod(name: "—————— 18：爆破死斗 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第18层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "狂野冲锋（直线击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15055"])]
    public void 狂野冲锋(Event @event, ScriptAccessory accessory)
    {
        if(isTTS) accessory.Method.TTS("冲锋击退");
        if(isEdgeTTS) accessory.Method.EdgeTTS("冲锋击退");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"狂野冲锋{@event.SourceId()}";
        dp.Scale = new (8f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }

    [ScriptMethod(name: "狂野冲锋销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15055"],userControl: false)]
    public void 狂野冲锋销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"狂野冲锋{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "撕裂利爪（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15050"])]
    public void 撕裂利爪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"撕裂利爪{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "撕裂利爪销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15050"],userControl: false)]
    public void 撕裂利爪销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"撕裂利爪{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "尾部碎击（扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15052"])]
    public void 尾部碎击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"尾部碎击{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "尾部碎击销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15052"],userControl: false)]
    public void 尾部碎击销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"尾部碎击{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "火球（面前圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15051"])]
    public void 火球(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"火球{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "火球销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15051"],userControl: false)]
    public void 火球销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"火球{@event.SourceId()}");
    }
    
    #endregion
    
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



#region 计算函数

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

#endregion 计算函数