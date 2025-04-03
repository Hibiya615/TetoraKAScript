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
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.MathHelpers;
using System.Threading.Tasks;
using KodakkuAssist.Extensions;

namespace Sephirot;

[ScriptType(guid: "6d0824b3-6d67-4450-8b4e-46857257579e", name: "萨菲洛特歼灭战", territorys: [517],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Sephirot
{
    const string noteStr =
        """
        v0.0.0.1:
        LV60 萨菲洛特歼灭战 初版绘制
        """;
    
    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "三重强击（顺劈）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:5851"])]
    public void 三重强击(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("坦克顺劈");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "三重强击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(18.5f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

    }
    
    [ScriptMethod(name: "魔神之怒（分摊）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0048"])]
    public void 魔神之怒(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "魔神之怒";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "虚空（半场刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5853"])]
    public void 虚空(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "虚空";
        dp.Scale = new (45, 45f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "慈悲之柱（蓝圈击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5579"])]
    public void 慈悲之柱(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "慈悲之柱";
        dp.Scale = new(1.5f, 10);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "大地摇动", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0028"])]
    public void 大地摇动(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        var boss = accessory.Data.Objects.GetByDataId(5556).FirstOrDefault();
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;
        dp.TargetObject = @event.TargetId();
        
        dp.Name = "大地摇动";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(75);
        dp.Radian = 30f.DegToRad();
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "王国（面前击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5867"])]
    public void 王国(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("面前击退", duration: 5500, true);
        if (isTTS)accessory.Method.EdgeTTS("面前击退");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "王国";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "荣光撞击（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5869"])]
    public void 荣光撞击(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("左侧击退", duration: 1100, true);
        if (isTTS)accessory.Method.EdgeTTS("左侧击退");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "荣光撞击";
        dp.Scale = new(1.5f, 5);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3f);
        dp.Owner = accessory.Data.Me; 
        dp.Rotation = 1.57f;
        dp.FixRotation = true;
        dp.DestoryAt = 1600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("(慈悲之柱|王国|荣光撞击)");
    }
    
    [ScriptMethod(name: "上升气流", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5869"])]
    public void 上升气流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "上升气流";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "上升气流指路";
        dp2.Owner = accessory.Data.Me;
        dp2.Color = accessory.Data.DefaultSafeColor;
        dp2.ScaleMode |= ScaleMode.YByDistance;
        dp2.TargetObject = @event.SourceId();
        dp2.Scale = new(1);
        dp2.Delay = 1600;
        dp2.DestoryAt = 4400;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
    }
    
    [ScriptMethod(name: "知识（分散）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5857"])]
    public void 知识(Event @event, ScriptAccessory accessory)
    {
        // 伤害ID：5858 ，范围5m
        if (isText)accessory.Method.TextInfo("保持分散", duration: 5500, false);
        if (isTTS)accessory.Method.EdgeTTS("保持分散");
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
