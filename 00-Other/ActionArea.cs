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
using System.Threading.Tasks;

namespace TetoraKodakkuScript._00_Other;

[ScriptType(guid: "f85df3b8-baa4-42b7-8196-bc26c58fa251", name: "技能范围", territorys: [],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class ActionArea
{
    const string noteStr =
        """
        v0.0.0.1:
        技能范围绘制，全场景可用
        请先自行设置合适亮度并用任意技能测试
        """;
    
    #region 用户控制
    
    [UserSetting("常驻AOE技能绘制一键开关")]
    public bool isPersistentAoEs { get; set; } = true;
    
    [UserSetting("常驻AOE技能颜色")]
    public ScriptColor PersistentAoEsColor { get; set; } = new() { V4 = new(0f, 1f, 1f, 2f) };
    
    [UserSetting("常驻AOE描边亮度（推荐10以上）")]
    public float PersistentOutlineBrightness { get; set; } = 15;
    
    [UserSetting("常驻AOE填充亮度（推荐小于1）")]
    public double PersistentFillBrightness { get; set; } = 0.3;
    
    [UserSetting("其它AOE技能颜色")]
    public ScriptColor ActionAoEsColor { get; set; } = new() { V4 = new(0f, 1f, 0f, 1f) };
    
    [UserSetting("其它AOE描边亮度（推荐10以上）")]
    public float ActionOutlineBrightness { get; set; } = 15;
    
    [UserSetting("其它AOE填充亮度（推荐小于1）")]
    public float ActionFillBrightness { get; set; } = 0.3f;
    
    [UserSetting("选择描边绘制类型")]
    public BlendModeEnum BlendMode { get; set; } = BlendModeEnum.Default;
    
    private static List<string> _BlendMode = ["VFX", "Imgui"];
    
    public enum BlendModeEnum
    {
        Default = 0,
        VFX = 1,
        Imgui = 2,
    }
    
    #endregion
    
    [ScriptMethod(name: "[描边] 标准舞步", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1818"])]
    public void 标准舞步描边 (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "标准舞步描边";
        dp.Color = ActionAoEsColor.V4.WithW(ActionOutlineBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(14.97f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 15000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[填充] 标准舞步", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1818"])]
    public void 标准舞步填充 (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "标准舞步填充";
        dp.Color = ActionAoEsColor.V4.WithW(ActionFillBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 15000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "标准舞步销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:1818"],userControl: false)]
    public void 标准舞步销毁 (Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"标准舞步.*");
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