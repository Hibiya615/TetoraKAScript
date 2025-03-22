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

namespace the_Whorleater_Extreme;

[ScriptType(guid: "07f20e0e-9463-4a10-9dd1-956fde6a9c46", name: "利维亚桑歼殛战", territorys: [359],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class the_Whorleater_Extreme
{
    const string noteStr =
        """
        v0.0.0.1:
        LV50 利维亚桑歼殛战 初版绘制
        """;
    
    
    [ScriptMethod(name: "猛撞销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2090"],userControl: false)]
    public void 猛撞销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("猛撞");  // 销毁水柱击退
    }
    
    [ScriptMethod(name: "水神的面纱 提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2165"])]
    public void 水神的面纱(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("法系打头部，物理打尾巴\n龙骑舞者赤魔DK额外注意自己的技能！\n尾巴有身位！", duration: 10000, true);
    }
    
    [ScriptMethod(name: "波齿鱼人 击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:2807"])]
    public void 波齿鱼人提示(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("优先击杀 < 波齿鱼人 >", duration: 2500, true);
    }
    
    [ScriptMethod(name: "波齿鱼人_恐慌洗礼", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1866"])]
    public void 恐慌洗礼(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("眩晕 < 波齿鱼人 >", duration: 3200, true);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "恐慌洗礼";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "恐慌洗礼销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:1866"],userControl: false)]
    public void 恐慌洗礼销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("恐慌洗礼");
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
