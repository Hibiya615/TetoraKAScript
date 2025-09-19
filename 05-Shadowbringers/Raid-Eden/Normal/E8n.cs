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

namespace E8n;

[ScriptType(guid: "c4d533c8-8798-441d-b849-fc3cd5cf63d9", name: "E8N", territorys: [905],
    version: "0.0.0.4" , author: "Tetora", note: noteStr)]

public class E8n
{
    const string noteStr =
        """
        v0.0.0.3:
        LV80 伊甸希望乐园 共鸣之章4（构想希瓦）初版绘制
        """;
    
    
    #region P1
    
    [ScriptMethod(name: "换装_闪光护甲（背对）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:20303"])]
    public async void 闪光护甲(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(6200); 
        accessory.Method.TextInfo("背对BOSS", duration: 2500, true);
        accessory.Method.EdgeTTS("背对BOSS");
    }
    
    [ScriptMethod(name: "换装_冰霜护甲（冰面）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:20302"])]
    public void 冰霜护甲(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("停止移动", duration: 4700, true);
        accessory.Method.EdgeTTS("停止移动");
    }
    
    [ScriptMethod(name: "阔斧回旋踢（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19938"])]
    public void 阔斧回旋踢(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "阔斧回旋踢";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "镰形回旋踢（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19939"])]
    public void 镰形回旋踢(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "镰形回旋踢";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.InnerScale = new Vector2(4f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "钻石星尘_冰柱冲击", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19978"])]
    public void 冰柱冲击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冰柱冲击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 7700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "天降一击（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19928"])]
    public void 天降一击(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("击退", duration: 4700, true);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "天降一击";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("天降一击");
    }
    
    [ScriptMethod(name: "冰霜刺（扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19932"])]
    public void 冰霜刺(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冰霜刺";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "连锁反射：冰霜刺", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19967"])]
    public void 连锁反射_冰霜刺(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "连锁反射_冰霜刺";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "冰霜斩（270°顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19931"])]
    public void 冰霜斩(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冰霜斩";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 270f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "连锁反射：冰霜斩", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19966"])]
    public void 连锁反射_冰霜斩(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "连锁反射_冰霜斩";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.Radian = 270f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    #endregion
    
    #region P2 世界分断

    [ScriptMethod(name: "土元精 石肤提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19951"])]
    public void 石肤(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("打断 < 土元精 >", duration: 2500, true);
        accessory.Method.EdgeTTS("打断土元精");
    }
    
    /*
    [ScriptMethod(name: "光以太_蓄势冲撞（截线）", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0054"])]
    public void 蓄势冲撞(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "蓄势冲撞";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(4f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    */
    
    #endregion
    
    #region P3

    [ScriptMethod(name: "分裂神圣 靠近连线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19949"])]
    public void 分裂神圣(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("假核爆：靠近BOSS", duration: 7700, true);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "分裂神圣";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    uint TargetMe=0;
    public void Init(ScriptAccessory accessory) {
        TargetMe=0; 
    }
    
    [ScriptMethod(name: "光之波动 点名记录", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"],userControl: false)]
    public void 光之海啸记录(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        TargetMe=1; 
    }
    
        
    [ScriptMethod(name: "光之波动 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19929"])]
    public void 光之波动(Event @event, ScriptAccessory accessory)
    {
        if (TargetMe == 1)
        {
            accessory.Method.TextInfo("引导扇形，避开塔", duration: 5200, true);
        }
        else
        {
            accessory.Method.TextInfo("踩塔", duration: 6700, true);
        }
    }
    
    [ScriptMethod(name: "光之波动 分散扇形", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
    public void 光之海啸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var boss = accessory.Data.Objects.GetByDataId(11635).FirstOrDefault();
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;
        
        dp.Name = "光之海啸";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new Vector2(40);
        dp.Radian = 60f.DegToRad();
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
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