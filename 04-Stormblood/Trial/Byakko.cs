using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using System.Threading.Tasks;

namespace Byakko;

[ScriptType(guid: "6a9e30e9-c58e-4f39-9f1e-ad26e5d49dc3", name: "白虎镇魂战", territorys: [746],
    version: "0.0.0.12", author: "Tetora", note: noteStr)]

public class Byakko
{
    const string noteStr =
        """
        v0.0.0.11:
        LV70 白虎镇魂战 初版绘制
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "天雷掌（死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10797"])]
    public void 天雷掌(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "天雷掌";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "乾坤一掷（分摊）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:003E"])]
    public void 乾坤一掷(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "乾坤一掷";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "荒弹（红球）Imgui高亮", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:8441"])]
    public void 荒弹红球(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"荒弹红球{@event.SourceId()}";
        dp.Color = new Vector4(1.0f, 0.0f, 0.0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(0.7f);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "荒弹红球销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:10824"],userControl: false)]
    public void 荒弹红球销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"荒弹红球{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "荒弹（空中弹幕）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:10817"])]
    public void 荒弹弹幕(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "荒弹弹幕";
        dp.Color = @accessory.Data.DefaultDangerColor;
        var pos = @event.EffectPosition();
        pos.Y = 0;
        dp.Position = pos;
        dp.Scale = new Vector2(2f);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "旋体脚（月环）判定动画", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10822"])]
    public void 旋体脚(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "旋体脚";
        dp.Color = new Vector4(0f, 1f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4800;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "百雷缭乱（地火）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10808"])]
    public void 百雷缭乱(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "百雷缭乱";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.Offset = new Vector3 (0 ,0, -5);
        dp.Delay = 4000;
        dp.DestoryAt = 4700;
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