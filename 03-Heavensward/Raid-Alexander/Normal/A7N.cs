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

namespace A7N;

[ScriptType(guid: "48d652cd-16c4-413f-a08c-49a308ccb98f", name: "A7N", territorys: [522],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class A7N
{
    const string noteStr =
        """
        v0.0.0.1:
        LV60 亚历山大机神城 律动之章3（万事通 奎克辛克斯） 初版绘制
        """;
    
    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    // wiki: https://ff14.huijiwiki.com/wiki/%E4%BA%9A%E5%8E%86%E5%B1%B1%E5%A4%A7%E6%9C%BA%E7%A5%9E%E5%9F%8E_%E5%BE%8B%E5%8A%A8%E4%B9%8B%E7%AB%A03
    
    [ScriptMethod(name: "哥布式波动炮（点名直线）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0018"])]
    public void 哥布式波动炮(Event @event, ScriptAccessory accessory)
    {        
        if (@event.TargetId() != accessory.Data.Me) {
            if (isText)accessory.Method.TextInfo("远离点名玩家", duration: 5300, true);
            if (isTTS)accessory.Method.TTS("远离点名玩家");
            if (isEdgeTTS)accessory.Method.EdgeTTS("远离点名玩家");
        } 
        else {
            if (isText)accessory.Method.TextInfo("直线点名", duration: 5300, true);
            if (isTTS)accessory.Method.TTS("直线点名");
            if (isEdgeTTS)accessory.Method.EdgeTTS("直线点名");
        }
        
        var boss = accessory.Data.Objects.GetByDataId(5381).FirstOrDefault();
        if (boss == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "哥布式波动炮"; // ActionId: 5614
        dp.Scale = new (6f, 64f); // 60m长 + 4m目标圈
        dp.Owner = boss.GameObjectId;
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "震撼弹（抓人点名）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0029"])]
    public void 震撼弹 (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) {
            if (isText)accessory.Method.TextInfo("远离点名玩家", duration: 8500, true);
            if (isTTS)accessory.Method.TTS("远离点名玩家");
            if (isEdgeTTS)accessory.Method.EdgeTTS("远离点名玩家");
        } 
        else {
            if (isText)accessory.Method.TextInfo("抓人点名", duration: 8500, true);
            if (isTTS)accessory.Method.TTS("抓人点名");
            if (isEdgeTTS)accessory.Method.EdgeTTS("抓人点名");
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"震撼弹"; // ActionId: 5567
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 9200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "齐射 连续易伤死刑提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3821"])]
    public void 齐射(Event @event, ScriptAccessory accessory)
    {
        // var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        
        if (isText)accessory.Method.TextInfo("连续易伤死刑", duration: 7800, true);
        if (isTTS)accessory.Method.TTS("连续易伤死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS("连续易伤死刑");
    }
    
    [ScriptMethod(name: "夏诺雅 声援提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5612"])]
    public void 声援(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("准备集火真心", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("准备集火真心");
        if (isEdgeTTS)accessory.Method.EdgeTTS("准备集火真心");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "夏诺雅";
        dp.Owner = accessory.Data.Me;
        dp.Color = new Vector4(1f, 1f, 0f, 1f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "真心 击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:5384"])]
    public void 击杀真心提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("击杀真心", duration: 5000, true);
        if (isTTS)accessory.Method.TTS("击杀真心");
        if (isEdgeTTS)accessory.Method.EdgeTTS("击杀真心");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "真心";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "真心销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:5384"],userControl: false)]
    public void 真心销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("真心");
    }
    
    [ScriptMethod(name: "铁球高亮", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:5388"])]
    public void 铁球 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"铁球";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(4.8f);
        dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "铁球";
        dp1.Scale = new (1f, 9.6f); 
        dp1.Owner = @event.SourceId();
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp1.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp1);  
    }
    
    [ScriptMethod(name: "铁球销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:5388"],userControl: false)]
    public void 铁球销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("铁球");
    }
    
    [ScriptMethod(name: "哥布林电火花 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5615"])]
    public void 哥布林电火花(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("AOE", duration: 3000, false);
        if (isTTS)accessory.Method.TTS("AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "牢门的锁 击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:5389"])]
    public void 击杀牢锁提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("击杀牢锁", duration: 2500, true);
        if (isTTS)accessory.Method.TTS("击杀牢锁");
        if (isEdgeTTS)accessory.Method.EdgeTTS("击杀牢锁");
    }
    
    uint MyPrey=0;
    
    public void Init(ScriptAccessory accessory) {
        MyPrey=0;
    }
    
    [ScriptMethod(name: "点名记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1051"],userControl: false)]
    public void 点名记录(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        MyPrey=1;
    }
    
    [ScriptMethod(name: "热病提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1049"])]
    public void 热病提示(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("热病：停止行动", duration: 5000, true);
        if (isTTS)accessory.Method.TTS("停止行动");
        if (isEdgeTTS)accessory.Method.EdgeTTS("停止行动");
        MyPrey=0;
    }
    
    [ScriptMethod(name: "窒息提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:938"])]
    public void 窒息提示(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("击杀 <警报系统>", duration: 5000, false);
        if (isTTS)accessory.Method.TTS("击杀小怪");
        if (isEdgeTTS)accessory.Method.EdgeTTS("击杀小怪");
        MyPrey=0;
    }
    
    [ScriptMethod(name: "冻伤提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:285"])]
    public void 冻伤提示(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("击杀 <3号哥布林装甲L型>", duration: 5000, false);
        if (isTTS)accessory.Method.TTS("击杀小怪");
        if (isEdgeTTS)accessory.Method.EdgeTTS("击杀小怪");
        MyPrey=0;
    }
    
    [ScriptMethod(name: "剧毒提示", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:1010"])]
    public void 剧毒提示(Event @event, ScriptAccessory accessory)
    {
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (MyPrey != 1 || @event.TargetId() != accessory.Data.Me) return; 
        if (isHealer && isText)accessory.Method.TextInfo("踩住毒气喷发位置", duration: 6000, true);
        if (isHealer && isTTS)accessory.Method.TTS("踩住毒气喷发位置");
        if (isHealer && isEdgeTTS)accessory.Method.EdgeTTS("踩住毒气喷发位置");
        MyPrey=0;
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