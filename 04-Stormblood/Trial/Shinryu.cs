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

namespace theRoyalMenagerie;

[ScriptType(guid: "da23fd13-2d1f-41d3-b2c9-91fd8d948a98", name: "神龙歼灭战", territorys: [679],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class Shinryu
{
    const string noteStr =
        """
        v0.0.0.2:
        LV70 神龙歼灭战 初版绘制
        缺少部分机制：如闪电分散、钻石星尘等
        若有遇见可通过DC将ARR录像文件私发给我补充
        """;
    
    
    [ScriptMethod(name: "巨浪 击退连线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8075"])]
    public void 巨浪连线(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("靠近水柱击退", duration: 9700, true);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "巨浪连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 9700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "超新星（分摊）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:003E"])]
    public void 超新星(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "超新星";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "地狱之火炎 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8107"])]
    public void 地狱之火炎(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("AOE，进入水圈", duration: 9700, true);
    }
    
    [ScriptMethod(name: "制裁之雷 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8108"])]
    public void 制裁之雷(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("AOE，离开水圈", duration: 9700, false);
        
        foreach (var item in accessory.Data.Objects.GetByDataId(2004237))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = new Vector4(1f, 0f, 0f, 4f);
            dp.Scale = new Vector2(5f);
            dp.DestoryAt = 9700;
            dp.Name = "水圈";
            dp.Owner = item.EntityId;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "右翼_闪电 分散", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8091"])]
    public void 闪电(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("分散，离开水圈", duration: 5700, true);
    }
    
    [ScriptMethod(name: "陨石冲击", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9291"])]
    public void 陨石冲击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "陨石冲击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "尾部猛击（砸地板）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8083"])]
    public void 尾部猛击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "尾部猛击";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(20f, 40f); 
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "攻击龙尾提示", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True","SourceName:regex:^(龙尾|tail|神龍の尾)$"])]
    public void 龙尾(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("攻击龙尾", duration: 4000, true);
    }
    
    volatile int BurningChainsMarkingPlayers=0;
    [ScriptMethod(name:"烈焰链点名记录", eventType:EventTypeEnum.TargetIcon, eventCondition:["Id:0061"], userControl:false)]
    public void 烈焰链点名记录(Event @event, ScriptAccessory accessory) 
    {
        BurningChainsMarkingPlayers=1;
    }
    
    [ScriptMethod(name: "烈焰链 引导提示", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0061"])]
    public void 烈焰链(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return;
        accessory.Method.TextInfo("靠近引导连线", duration: 3000, false);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "引导";
        dp.Color = new Vector4(0f, 1f, 0f, 3f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(1f);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "烈焰链 扯线提示", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0009"])]
    public void 烈焰链2(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return;
        accessory.Method.TextInfo("扯断连线", duration: 2500, true);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "烈焰链扯线";
        dp.Color = new Vector4(1f, 0f, 0f, 3f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(1f);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "烈焰链扯断销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:769"],userControl: false)]
    public void 烈焰链扯断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("烈焰链扯线");
    }
    
    [ScriptMethod(name: "钻石星尘 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8109"])]
    public async void 钻石星尘(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(7700); 
        accessory.Method.TextInfo("停止移动！", duration: 2000, true);
    }
    
    [ScriptMethod(name: "大气爆发 击退", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8080"])]
    public void 大气爆发(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("连续中心击退", duration: 9700, true);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "大气爆发";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "台风眼", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6935"])]
    public void 台风眼(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "台风眼";
        dp.Color = new Vector4(1f, 0f, 0f, 3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 10800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "台风眼销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:6935"],userControl: false)]
    public void 台风眼销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("台风眼");
    }
    
    [ScriptMethod(name: "左翼_吹雪（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8098"])]
    public void 吹雪(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("AOE", duration: 5700, false);
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
