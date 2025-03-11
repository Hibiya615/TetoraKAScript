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

namespace Emanation;

[ScriptType(guid: "214f8fbd-ad04-430f-8bba-fd7319581780", name: "吉祥天女歼灭战", territorys: [719],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class Emanation
{
    const string noteStr =
        """
        v0.0.0.2:
        LV70 吉祥天女歼灭战 初版绘制
        """;
    
    public static class IbcHelper
    {
        public static IBattleChara? GetById(uint id)
        {
            return (IBattleChara?)Svc.Objects.SearchByEntityId(id);
        }
    
        public static IBattleChara? GetMe()
        {
            return Svc.ClientState.LocalPlayer;
        }
    
        public static IGameObject? GetFirstByDataId(uint dataId)
        {
            return Svc.Objects.Where(x => x.DataId == dataId).FirstOrDefault();
        }
    
        public static IEnumerable<IGameObject?> GetByDataId(uint dataId)
        {
            return Svc.Objects.Where(x => x.DataId == dataId);
        }
    }
    
    [ScriptMethod(name: "元气刷新提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^93(47|74)$"])]
    public void 元气刷新(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("即将刷新元气", duration: 5000, false);
    }
    
    [ScriptMethod(name: "至上天母", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:9026"])]
    public void 至上天母(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("使用元气", duration: 5000, true);
    }
    
    [ScriptMethod(name: "完全拒绝", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9349"])]
    public void 完全拒绝(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("使用元气 + 击退", duration: 7700, true);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "完全拒绝";
        dp.Scale = new(1f, 5);
        dp.Color = new Vector4(0f, 1f, 1f, 2f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "光之波动（二仇顺劈）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:000E"])]
    public void 光之波动(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        var boss = IbcHelper.GetFirstByDataId(7712);
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;
        
        dp.Name = "光之波动";
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(40);
        dp.Radian = 125f.DegToRad();
        dp.DestoryAt = 5400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    
    [ScriptMethod(name: "光之瀑布（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9361"])]
    public void 光之瀑布(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "光之瀑布";
        dp.Owner = @event.TargetId();
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new Vector2(7);
        dp.DestoryAt = 5000;
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
