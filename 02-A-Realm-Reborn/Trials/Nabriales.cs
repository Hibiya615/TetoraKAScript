using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Objects.Types;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.MathHelpers;

namespace Nabriales;

[ScriptType(guid: "64206b9e-cd0a-47ec-960d-15f39a888f9e", name: "那布里亚勒斯讨伐战", territorys: [426],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Nabriales
{
    const string noteStr =
        """
        v0.0.0.1:
        LV50 那布里亚勒斯讨伐战 初版绘制
        """;
    
    
    [ScriptMethod(name: "双重", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3443"])]
    public void 双重(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("二连死刑");
    }
    
    [ScriptMethod(name: "三重", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3444"])]
    public void 三重(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("三连死刑");
    }
    
    [ScriptMethod(name: "爆震（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3437"])]
    public void 爆震(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "撞红球提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3421"])]
    public void 撞红球提示(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.EdgeTTS("撞红球");
    }
    
    [ScriptMethod(name: "暗以太·壹", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3651"])]
    public void 暗以太壹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"暗以太壹{@event.SourceId()}";
        dp.Color = new Vector4(0f, 1f, 1f, 2);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "暗以太爆发·壹", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3651"])]
    public void 暗以太爆发壹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"暗以太爆发壹{@event.SourceId()}";
        dp.Color = new Vector4(0f, 1f, 1f, 0.3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "暗以太壹销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3421"],userControl: false)]
    public void 暗以太壹销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"暗以太壹{@event.SourceId()}");
        accessory.Method.RemoveDraw($"暗以太爆发壹{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "暗以太·贰", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3652"])]
    public void 暗以太贰(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"暗以太贰{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 2);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "暗以太爆发·贰", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3652"])]
    public void 暗以太爆发贰(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"暗以太爆发贰{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 0.3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(11f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "暗以太贰销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3422"],userControl: false)]
    public void 暗以太贰销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"暗以太贰{@event.SourceId()}");
        accessory.Method.RemoveDraw($"暗以太爆发贰{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "撞蓝球提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3423"])]
    public void 撞蓝球提示(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.EdgeTTS("撞蓝球");
    }
    
    [ScriptMethod(name: "暗以太·叁", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3653"])]
    public void 暗以太叁(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"暗以太叁{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "暗以太爆发·叁", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3653"])]
    public void 暗以太爆发叁(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"暗以太爆发叁{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 0.3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "暗以太·叁销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3423"],userControl: false)]
    public void 暗以太叁销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"暗以太叁{@event.SourceId()}");
        accessory.Method.RemoveDraw($"暗以太爆发叁{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "暗影元精 击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3654"])]
    public void 暗影元精(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("击杀暗影元精", duration: 2000, true);
        // accessory.Method.EdgeTTS("击杀暗影元精");
    }
    
    uint Extend=0;
    public void Init(ScriptAccessory accessory) {
        Extend=0;
    }
    
    [ScriptMethod(name: "延时（吸引）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3425"])]
    public void 延时(Event @event, ScriptAccessory accessory)
    {
        Extend=1;
        accessory.Method.TextInfo("吸引", duration: 6500, true);
    }

    [ScriptMethod(name: "延迟之门", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3655"])]
    public void 延迟之门(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "延迟之门";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 15800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "延迟之门 吸引预测", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3655"])]
    public void 吸引(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "吸引";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new(1, 6);
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = @event.SourcePosition();
        dp.DestoryAt = 15800;
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "延时之门销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:3655"],userControl: false)]
    public void 延时之门销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("延时之门");
        accessory.Method.RemoveDraw("吸引");
    }
    
    [ScriptMethod(name: "彗星 踩塔连线", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2005159", "Operate:Add", "Kind:EventObj"])]
    public void 彗星(Event @event, ScriptAccessory accessory)
    {
        if (Extend != 1) return;  
        accessory.Method.TextInfo("T踩塔，准备近战LB", duration: 12000, true);
        accessory.Method.EdgeTTS("坦克踩塔，准备近战LB");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "一塔描边";
        dp.Color = new Vector4(1f, 1f, 0f, 8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3.1f);
        dp.InnerScale = new Vector2(3f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 12000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        /*
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "一塔连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 12000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        */
        
        Extend=0;
    }
    
    [ScriptMethod(name: "次元裂缝 击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3656"])]
    public void 次元裂缝(Event @event, ScriptAccessory accessory)
    {
        // accessory.Method.TextInfo("击杀 <次元裂缝>", duration: 5000, true);
        accessory.Method.EdgeTTS("击杀次元裂缝");
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