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

namespace Susano;

[ScriptType(guid: "2c4b6205-6a77-4c89-b641-2c2359a43f25", name: "须佐之男歼灭战", territorys: [674],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Susano
{
    const string noteStr =
        """
        v0.0.0.1:
        LV63 须佐之男歼灭战 初版绘制
        支持DR 自动QTE
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("DR 自动QTE")]
    public bool isDRHelper { get; set; } = true;
    
    [ScriptMethod(name: "晴岚（分摊）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:003E"])]
    public void 晴岚(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "晴岚";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "拼刀 自动QTE", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1271"])]
    public void 拼刀(Event @event, ScriptAccessory accessory)
    {
        if (isDRHelper && @event.TargetId() == accessory.Data.Me) accessory.Method.SendChat("/pdr load AutoQTE");
        if (isText && @event.TargetId() != accessory.Data.Me) accessory.Method.TextInfo("攻击 <天之丛云>", duration: 2500, true);
    }
    
    [ScriptMethod(name: "破浪斩（直线死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8227"])]
    public void 破浪斩(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "破浪斩";
        dp.Scale = new (8f, 23.5f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "天之岩户 石牢连线", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0070"])]
    public void 天之岩户(Event @event, ScriptAccessory accessory)
    {
        // ActionId:8333 岩户返 其中一个tid为被抓对象  
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "天之岩户";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1);
        dp.DestoryAt = 20500;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "岩户闭合 石牢", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:292"])]
    public void 岩户闭合(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "岩户闭合";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(4.8f);
        dp.DestoryAt = 25200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "天之岩户销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:8229"],userControl: false)]
    public void 天之岩户销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("(天之岩户|岩户闭合)");
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