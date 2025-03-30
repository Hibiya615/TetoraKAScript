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

namespace theStrikingTree_Hard;

[ScriptType(guid: "de6d6f10-775d-4c45-91ec-2bd4ed6762c7", name: "拉姆歼灭战", territorys: [374],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class theStrikingTree_Hard_
{
    const string noteStr =
        """
        v0.0.0.2:
        LV50 拉姆歼灭战 初版绘制
        """;
    
    [ScriptMethod(name: "混乱冲击 点名提示", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0001"])]
    public void 混乱冲击(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.TextInfo("恐慌点名，站BOSS背后", duration: 2700, true);
        accessory.Method.TTS("站BOSS背后");
    }
    
    [ScriptMethod(name: "混乱冲击 恐怖debuff高亮", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:66"])]
    public void 恐怖(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"恐怖{@event.TargetId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 2f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 39000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "混乱冲击 恐怖debuff解除", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:66"],userControl: false)]
    public void 恐怖销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"恐怖{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "雷暴 点名提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2278"])]
    public void 雷暴(Event @event, ScriptAccessory accessory)
    {
        // 也有 TargetIcon Id:0015
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.TextInfo("去炸恐慌玩家", duration: 3700, false);
        accessory.Method.TTS("去炸恐慌玩家");
        
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "雷暴描边";
            dp.Color = new Vector4(0f, 1f, 1f, 4f);
            dp.Owner = @event.TargetId();
            dp.Scale = new Vector2(5f);
            dp.InnerScale = new Vector2(4.95f);
            dp.Radian = float.Pi * 2;
            dp.DestoryAt = 3700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
    }
    
    [ScriptMethod(name: "仲裁者之影_霹雳（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2282"])]
    public void 霹雳(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name =  $"霹雳{@event.SourceId()}";
        dp.Scale = new (4, 50f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "仲裁者之影_霹雳销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:2282"],userControl: false)]
    public void 霹雳销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"霹雳{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "因果 点名提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:514"])]
    public void 因果(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.TextInfo("停手，吃3个球", duration: 5000, true);
        accessory.Method.TTS("停手，吃3个球");
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