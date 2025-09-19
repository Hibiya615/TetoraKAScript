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

namespace E10n;

[ScriptType(guid: "038e00e8-d378-4f43-89ab-e27df5561d5a", name: "E10N", territorys: [943],
    version: "0.0.0.3", author: "Tetora", note: noteStr)]

public class E10n
{
    const string noteStr =
        """
        v0.0.0.2:
        LV80 伊甸希望乐园 再生之章2（影之王）初版绘制
        """;
    
    #region 向心聚爆 & 十亿斩击
    public enum ImplosionType {
        FRONT,          // 22196
        BACK,           // 22199
        LEFT,           // 22193
        RIGHT,          // 22190
        SHADOWY_FRONT,  // 22197
        SHADOWY_BACK,   // 22200
        SHADOWY_LEFT,   // 22194
        SHADOWY_RIGHT   // 22191

    }
    
    [ScriptMethod(name: "向心聚爆 & 十亿斩击", eventType: EventTypeEnum.StartCasting,
        eventCondition:["ActionId:regex:^(2219[0134679]|22200)$"])]
    public void 半场刀判断(Event @event, ScriptAccessory accessory) {
        uint decimalActionID = Convert.ToUInt32(@event["ActionId"], 10);

        ImplosionType implosionType = ImplosionType.FRONT;
        bool actionIDConfirmed = false;

        switch (decimalActionID)
        {
            case 22196:
            {
                implosionType = ImplosionType.FRONT;
                actionIDConfirmed = true;
                break;
            }

            case 22199:
            {
                implosionType = ImplosionType.BACK;
                actionIDConfirmed = true;
                break;
            }

            case 22193:
            {
                implosionType = ImplosionType.LEFT;
                actionIDConfirmed = true;
                break;
            }

            case 22190:
            {
                implosionType = ImplosionType.RIGHT;
                actionIDConfirmed = true;
                break;
            }

            case 22197:
            {
                implosionType = ImplosionType.SHADOWY_FRONT;
                actionIDConfirmed = true;
                break;
            }

            case 22200:
            {
                implosionType = ImplosionType.SHADOWY_BACK;
                actionIDConfirmed = true;
                break;
            }

            case 22194:
            {
                implosionType = ImplosionType.SHADOWY_LEFT;
                actionIDConfirmed = true;
                break;
            }

            case 22191:
            {
                implosionType = ImplosionType.SHADOWY_RIGHT;
                actionIDConfirmed = true;
                break;
            }

            default:
            {
                accessory.Method.SendChat("/e 技能ID解析失败！");
                break;
            }
        }

            if (actionIDConfirmed) {
                    向心聚爆_十亿斩击(accessory, @event.SourceId(), implosionType);
            }
    }
    public void 向心聚爆_十亿斩击(ScriptAccessory accessory,uint entityId,ImplosionType implosionType)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        bool directionConfirmed = false;
        
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = entityId;
        dp.Scale = new Vector2(70);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 5700;
        
        switch(implosionType) {

            case ImplosionType.FRONT: {
                dp.Name = "前向心聚爆";
                directionConfirmed = true;
                break;
            }
            
            case ImplosionType.SHADOWY_FRONT: {
                dp.Name = "影·前向心聚爆";
                directionConfirmed = true;
                break;
            }

            case ImplosionType.BACK: {
                dp.Name = "后向心聚爆";
                dp.Rotation = 180f.DegToRad();
                directionConfirmed = true;
                break;
            }
            
            case ImplosionType.SHADOWY_BACK: {
                dp.Name = "影·后向心聚爆";
                dp.Rotation = 180f.DegToRad();
                directionConfirmed = true;
                break;
            }
            
            case ImplosionType.LEFT: {
                dp.Name = "左十亿斩击";
                dp.Rotation = 90f.DegToRad();
                directionConfirmed = true;
                break;
            }
            
            case ImplosionType.SHADOWY_LEFT: {
                dp.Name = "影·左十亿斩击";
                dp.Rotation = 90f.DegToRad();
                directionConfirmed = true;
                break;
            }
            
            case ImplosionType.RIGHT: {
                dp.Name = "右十亿斩击";
                dp.Rotation = -90f.DegToRad();
                directionConfirmed = true;
                break;
            }
            
            case ImplosionType.SHADOWY_RIGHT: {
                dp.Name = "影·右十亿斩击";
                dp.Rotation = -90f.DegToRad();
                directionConfirmed = true;
                break;
            }

            default: {
                accessory.Method.SendChat("/e 画图范围确定失败！");
                break;
            }

        }
        if (directionConfirmed) {
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }
    #endregion
    
    [ScriptMethod(name: "痛苦钩刺(连线狗狗半场)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22233"])]
    public void 痛苦钩刺(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "痛苦钩刺";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40); //实际范围应为70m，此处为了方便看时间填充故设为场边半径
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "潜影（钻地钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:23313"])]
    public void 影之披风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "影之披风";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = 7200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "影之王权（起身钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22215"])]
    public void 影之王权(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "影之王权";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16f);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "影之王权描边";
        dp1.Color = new Vector4(1f, 1f, 0f, 8f);
        dp1.Scale = new(16.08f);
        dp1.InnerScale = new(16);
        dp1.Radian = float.Pi * 2;
        dp1.Owner = @event.SourceId();
        dp1.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "影之王权填充";
        dp2.Color = new Vector4(1f, 1f, 0.2f, .8f);
        dp2.Owner = @event.SourceId();
        dp2.Scale = new Vector2(16f);
        dp2.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);
    }
    
    [ScriptMethod(name: "影之斩击（直线死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:23307"])]
    public void 影之斩击(Event @event, ScriptAccessory accessory)
    {
        var pos = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "影之斩击";
        dp.Scale = new (8, 50f); 
        dp.Owner = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "影之喷发（脚下黄圈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22241"])]
    public void 影之喷发(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "影之喷发";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
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

    public static string DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<string>(@event["DurationMilliseconds"]) ?? string.Empty;
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


public static class Extensions
{
    public static void TTS(this ScriptAccessory accessory, string text, bool isTTS, bool isDRTTS)
    {
        if (isDRTTS)
        {
            accessory.Method.SendChat($"/pdr tts {text}");
        }
        else if (isTTS)
        {
            accessory.Method.TTS(text);
        }
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