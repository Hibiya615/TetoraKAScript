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

namespace Tsukuyomi;

[ScriptType(guid: "97415dc8-cd16-4c9b-87be-026a297c3451", name: "月读歼灭战", territorys: [778],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Tsukuyomi
{
    const string noteStr =
        """
        v0.0.0.1:
        LV70 月读歼灭战 初版绘制
        """;
    
    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    uint Maiogi=0;
    
    public void Init(ScriptAccessory accessory) {
        Maiogi=0; 
    }
    
    [ScriptMethod(name: "折磨（顺劈死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:11235"])]
    public void 折磨(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "折磨";
        dp.Color = accessory.Data.DefaultDangerColor;
        var boss = accessory.Data.Objects.GetByDataId(8720).FirstOrDefault();
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;
        dp.Scale = new Vector2(18.2f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "宴会游乐（九连环预备）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:8769"])]
    public void 宴会游乐(Event @event, ScriptAccessory accessory)
    {
        Maiogi++;  // 出现间隔约400ms
        
        if(Maiogi <=7){        
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "宴会游乐";
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(10f);
            dp.DestoryAt = 5000 - 400*Maiogi;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "月下舞扇（九连环进行）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(11206|11245)$"])]
    public void 月下舞扇(Event @event, ScriptAccessory accessory)
    {
        Maiogi = 0;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "月下舞扇";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "黄泉之弹（直线分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:11238"])]
    public void 黄泉之弹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "黄泉之弹";
        dp.Scale = new (8, 43.2f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.8f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "月读（鸳鸯锅）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^153[89]$", "StackCount:regex:^[34]$"])]
    public void 月读(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        string statusID = @event.StatusId.ToString();
        if (System.Text.RegularExpressions.Regex.IsMatch(statusID, "^153[89]$"))
        {
            string color = statusID == "1538" ? "黑色" : "白色";
            if (isText) accessory.Method.TextInfo($"吃{color}", duration: 2000, true);
            if (isTTS) accessory.Method.EdgeTTS($"吃{color}");
        }
    }
    
    [ScriptMethod(name: "月下缭乱（连续分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:11259"])]
    public void 月下缭乱(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "月下缭乱";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 8200;
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
