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

namespace The_Navel___EX._04_Omega_Quests.Normal;

[ScriptType(guid: "2232ae84-c1e7-4382-88b4-d691887f27cf", name: "O11N", territorys: [800],
    version: "0.0.0.2" , author: "Tetora", note: noteStr)]

public class O11n
{
    const string noteStr =
        """
        v0.0.0.2:
        LV70 欧米茄时空狭缝 阿尔法幻境3（欧米茄）初版绘制
        """;
    
    [ScriptMethod(name: "芥末爆弹（死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12935"])]
    public void 芥末爆弹死刑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "芥末爆弹死刑";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "左/右舷齐射·波动炮", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(12929|1293[012])$"])]
    public void 齐射波动炮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "齐射波动炮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60);
        dp.Radian = 210f.DegToRad();
        
        switch (@event.ActionId())
        {
            case 12929:
                dp.Rotation = 270f.DegToRad();
                dp.DestoryAt = 4700;
                break;
            case 12930:
                dp.Rotation = 270f.DegToRad();
                dp.DestoryAt = 2700;
                break;
            case 12931:
                dp.Rotation = 90f.DegToRad();
                dp.DestoryAt = 4700;
                break;
            case 12932:
                dp.Rotation = 90f.DegToRad();
                dp.DestoryAt = 2700;
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "生成外设-火箭飞拳（直线）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:9622"])]
    public void 突进(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "突进";
        dp.Scale = new (15f, 49f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 12500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "突进销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:13724"],userControl: false)]
    public void 突进销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("突进");
    }
    
    /*
    [ScriptMethod(name: "雷力投射点 开始", eventType: EventTypeEnum.Chat, eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^从欧米茄那里感知到了强烈的能量反应.*"])]
    public void 雷力投射点Start(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "雷力投射点 结束", eventType: EventTypeEnum.Chat, eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^确认到防御力场生成完成.*"])]
    public void 雷力投射点End(Event @event, ScriptAccessory accessory)
    {
    }
    */
    

    [ScriptMethod(name: "芥末爆弹（连线）", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0054"])]
    public async void 冲击波(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("冲击波");
            
        await Task.Delay(200);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冲击波";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 8400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "冲击波销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:12928"],userControl: false)]
    public void 冲击波销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("冲击波");
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
