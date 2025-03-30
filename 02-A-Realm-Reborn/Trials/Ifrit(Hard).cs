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

namespace theBowlofEmbers_Hard;

[ScriptType(guid: "d3d532f1-0707-427f-ac04-871a22022c11", name: "伊弗利特歼灭战", territorys: [292],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class theBowlofEmbers_Hard
{
    const string noteStr =
        """
        v0.0.0.2:
        LV50 伊弗利特歼灭战 初版绘制
        """;
    
    [UserSetting("顺劈死刑预测")]
    public static bool 烈焰焚烧 { get; set; } = true;
    
    [ScriptMethod(name: "烈焰焚烧（仅开场）", eventType: EventTypeEnum.Chat, userControl:false, eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^勇猛无比.*", "Sender:伊弗利特"])]
    public void 烈焰焚烧绘制(Event @event, ScriptAccessory accessory)
    {
        if (!烈焰焚烧) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        var ifrits = Svc.Objects.Where(x => x.DataId == 209);
        foreach (var ifrit in ifrits)
        {
            if (ifrit.IsTargetable)
            {
                dp.Owner = ifrit.EntityId;
            }
        }
        dp.Name = "烈焰焚烧";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(15);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 2600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [UserSetting("击退预测")]
    public static bool 火神爆裂 { get; set; } = false;
    
    [ScriptMethod(name: "火神爆裂（仅开场）", eventType: EventTypeEnum.Chat, userControl:false, eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^勇猛无比.*", "Sender:伊弗利特"])]
    public void 火神爆裂绘制(Event @event, ScriptAccessory accessory)
    {
        if (!火神爆裂) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        var ifrits = Svc.Objects.Where(x => x.DataId == 209);
        foreach (var ifrit in ifrits)
        {
            if (ifrit.IsTargetable)
            {
                dp.Owner = ifrit.EntityId;
            }
        }
        dp.Name = "火神爆裂";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(21f);
        dp.DestoryAt = 2500;
        dp.Delay = 7000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "地火喷发", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1358"])]
    public void 地火喷发(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "地火喷发";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深红旋风（火神冲）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:457"])]
    public void 深红旋风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深红旋风";
        dp.Scale = new (12, 43f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
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