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
using KodakkuAssist.Extensions;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.MathHelpers;
using System.Threading.Tasks;

namespace the_Voidcast_Dais_Extreme;

[ScriptType(guid: "ef5d2622-37eb-45ba-a40a-b93f494dd097", name: "高贝扎歼殛战", territorys: [1141],
    version: "0.0.0.3", author: "Tetora", note: noteStr)]

public class Golbez_Extreme
{
    const string noteStr =
        """
        v0.0.0.3:
        LV90 高贝扎歼殛战 初版绘制
        """;

    [ScriptMethod(name: "虚空陨石（死刑）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0158"])]
    public void 虚空陨石(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"虚空陨石{@event.TargetId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        dp.Delay = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "虚空陨石销毁", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:0"],userControl: false)]
    public void 虚空陨石销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"虚空陨石{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "迟缓地暴（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33894"])]
    public void 迟缓地暴(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "迟缓地暴";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16f);
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "迟缓电火花（暗天顶）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33898"])]
    public void 迟缓电火花(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "迟缓电火花";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "弦月连剑（半场刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(34523|34546)$"])]
    public void 弦月连剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "弦月连剑";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(22);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 4700; // 34546 为 8100
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "升龙烽火（组合钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33935"])]
    public void 升龙烽火(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "升龙烽火";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 6900;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "极寒突袭", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33888"])]
    public void 极寒突袭(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "极寒突袭";
        dp.Scale = new(15, 15f); 
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "风晶球", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3387[6-9])$"])]
    public void 风晶球(Event @event, ScriptAccessory accessory)
    {
        var dura = @event.DurationMilliseconds();
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "风晶球";
        dp.Scale = new (5, 30f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3700;
        dp.Delay = dura - 3500;
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