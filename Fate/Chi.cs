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

namespace Omicron_Recall_Killing_Order;

[ScriptType(guid: "b73d07ef-aa90-45a9-ab4b-fc3ccce8791b", name: "侵略兵器召回指令：破坏侵略兵器希", territorys: [960],
    version: "0.0.0.3", author: "Tetora", note: noteStr)]

public class Chi
{
    const string noteStr =
        """
        v0.0.0.2:
        LV90 特殊Fate 绘制
        侵略兵器召回指令：破坏侵略兵器希
        """;
    
    [ScriptMethod(name: "迷失连线", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^758[67]$"])]
    public void 迷失连线(Event @event, ScriptAccessory accessory)
    {
        // Data ID 7586: 迷失少女 ；7587：迷失者
        accessory.Method.TextInfo("迷失出现", duration: 5000, true);
        accessory.Method.TTS("迷失出现");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "迷失连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "迷失连线销毁", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:regex:^758[67]$"],userControl: false)]
    public void 迷失连线销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("迷失连线");
    }
    
    [ScriptMethod(name: "终端攻击（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25172|2595[356])$"])]
    public void 月环(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "月环";
        dp.Color = new Vector4(1f, 1f, 0f, 0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(16f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = @event.ActionId() == 25953 ? 4700 : 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "终端攻击（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25173|2595[478])$"])]
    public void 直线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "直线";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 1f, 0f, 0.4f);
        dp.Scale = new(32f, 120f); 
        dp.DestoryAt = @event.ActionId() == 25954 ? 4700 : 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "前方扫荡 & 后方扫射", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25959|2596[023]|2652[34])$"])]
    public void 半场扇形(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        dp.Color = new Vector4(1f, 1f, 0f, 0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(45);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 5700;
        
        switch (@event.ActionId())
        {
            case 26523:
            case 25959:
            case 25960: // 前方扫荡·零式后方扫射
                dp.Name = "前方扫荡";
                dp.Rotation = 0f.DegToRad(); 
                break;

            case 26524:
            case 25962:
            case 25963:  // 后方扫射·零式前方扫荡
                dp.Name = "后方扫射";
                dp.Rotation = 180f.DegToRad(); 
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "零式前方扫荡 & 后方扫射", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2595[5-8]|2596[03])$"])]
    public void 零式半场扇形(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        dp.Color = new Vector4(1f, 1f, 0f, 0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(45);
        dp.Radian = 180f.DegToRad();
        
        switch (@event.ActionId())
        {
            case 25955: //终端攻击·零式前方扫荡 直接显示
            case 25957:
                dp.Name = "前方扫荡";
                dp.Rotation = 0f.DegToRad(); 
                dp.DestoryAt = 12200;
                break;

            case 25956: //终端攻击·零式后方扫射 直接显示
            case 25958:
                dp.Name = "后方扫射";
                dp.Rotation = 180f.DegToRad(); 
                dp.DestoryAt = 12200;
                break;
            
            // 画第二刀 方向相反 ， 第一刀跟前面的普通画一起了
            case 25960: //前方扫荡·零式后方扫荡
                dp.Name = "后方扫射";
                dp.Rotation = 180f.DegToRad(); 
                dp.Delay = 5800;
                dp.DestoryAt = 4400;
                break;
            
            case 25963: //后方扫射·零式前方扫荡
                dp.Name = "前方扫荡";
                dp.Rotation = 0f.DegToRad(); 
                dp.Delay = 5800;
                dp.DestoryAt = 4400;
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "地下贯通爆弹（三穿一）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25101"])]
    public void 地下贯通爆弹1(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "地下贯通爆弹1";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
        dp.Scale = new(20f, 20f);
        dp.Delay = 7700;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "地下贯通爆弹（随机）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25976"])]
    public void 地下贯通爆弹2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "地下贯通爆弹2";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
        dp.Scale = new(20f, 20f);
        dp.Delay = 9800;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "地下贯通爆弹（随机）提前一炸显示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25976"])]
    public void 地下贯通爆弹3(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "地下贯通爆弹3";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 1f, 0f, 0.4f);
        dp.Scale = new(20f, 20f);
        dp.Delay = 8800;
        dp.DestoryAt = 1000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "电视死亡销毁", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:13515"],userControl: false)]
    public void 电视死亡销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
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