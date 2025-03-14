using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
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

namespace BattleOnTheBigBridgen;

[ScriptType(guid: "3e4102cb-9410-44fd-85e8-d43a3bc25737", name: "大桥上的决斗", territorys: [366],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class BattleOnTheBigBridge
{
    const string noteStr =
        """
        v0.0.0.1:
        LV50 大桥上的决斗 初版绘制
        """;
    
    
    public static class IbcHelper
    {
        public static IBattleChara? GetById(uint id)
        {
            return (IBattleChara?)Svc.Objects.SearchByEntityId(id);
        }
        
        public static IEnumerable<IGameObject?> GetByDataId(uint dataId)
        {
            return Svc.Objects.Where(x => x.DataId == dataId);
        }
    }
    
    [ScriptMethod(name: "蛙变之歌", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:439"])]
    public void 蛙变之歌(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return;

        accessory.Method.TextInfo("躲避绿鸡", duration: 5000, true);
        accessory.Method.EdgeTTS("躲避绿鸡");
        
        foreach (var item in IbcHelper.GetByDataId(2824))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "恩奇都";
            dp.Owner = item.EntityId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new Vector2(2.5f);
            dp.DestoryAt = 20000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp); 
        }
    }
    
    [ScriptMethod(name: "蛙变解除销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:439"],userControl: false)]
    public void 蛙变解除销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("恩奇都");
    }
    
    [ScriptMethod(name: "混乱 奶满提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:11"])]
    public void 混乱(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("奶满混乱队友", duration: 5000, false);
        accessory.Method.EdgeTTS("奶满混乱队友");
        
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"混乱{@event.SourceId()}";
            dp.Owner = @event.TargetId();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new Vector2(1.5f);
            dp.DestoryAt = 20000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp); 
    }
    
    [ScriptMethod(name: "混乱解除销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:11"],userControl: false)]
    public void 混乱解除销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw( $"混乱{@event.SourceId()}");
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

    public static uint StatusID(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StatusID"]);
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
