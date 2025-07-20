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

namespace Celestium;


[ScriptType(guid: "7703f1a9-5698-4896-8908-bb8e415c1321", name: "天青斗场", territorys: [796],
    version: "0.0.0.5", author: "Tetora", note: noteStr)]

public class Celestium {
    const string noteStr =
        """
        v0.0.0.5:
        天青斗场绘制及机制提醒，随缘更新中
        若无法更新请删除后刷新重新下载
        目前支持层数：18 [爆破死斗]
        """;
    
    #region 基础控制
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    #endregion

    #region 03：最初的岩壁——希帕克纳
    
    [ScriptMethod(name: "—————— 03：最初的岩壁——希帕克纳 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第3层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "破魔震（打断）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14365"])]
    public void 破魔震(Event @event, ScriptAccessory accessory)
    {
        if(isText)accessory.Method.TextInfo("打断BOSS", duration: 3000, true);
        if(isTTS) accessory.Method.TTS("打断BOSS");
        if(isEdgeTTS) accessory.Method.EdgeTTS("打断BOSS");
    }
    
    #endregion
    
    #region 08：青之牙、红之牙
    
    [ScriptMethod(name: "—————— 08：青之牙、红之牙 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第8层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "大爆炸（打断）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14680"])]
    public void 大爆炸(Event @event, ScriptAccessory accessory)
    {
        if(isText)accessory.Method.TextInfo("打断BOSS", duration: 3000, true);
        if(isTTS) accessory.Method.TTS("打断BOSS");
        if(isEdgeTTS) accessory.Method.EdgeTTS("打断BOSS");
    }
    
    #endregion
    
    #region 18：爆破死斗

    [ScriptMethod(name: "—————— 18：爆破死斗 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第18层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "狂野冲锋（直线击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15055"])]
    public void 狂野冲锋(Event @event, ScriptAccessory accessory)
    {
        if(isTTS) accessory.Method.TTS("冲锋击退");
        if(isEdgeTTS) accessory.Method.EdgeTTS("冲锋击退");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"狂野冲锋{@event.SourceId()}";
        dp.Scale = new (8f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }

    [ScriptMethod(name: "狂野冲锋销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15055"],userControl: false)]
    public void 狂野冲锋销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"狂野冲锋{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "撕裂利爪（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15050"])]
    public void 撕裂利爪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"撕裂利爪{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "撕裂利爪销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15050"],userControl: false)]
    public void 撕裂利爪销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"撕裂利爪{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "尾部碎击（扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15052"])]
    public void 尾部碎击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"尾部碎击{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "尾部碎击销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15052"],userControl: false)]
    public void 尾部碎击销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"尾部碎击{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "火球（面前圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15051"])]
    public void 火球(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"火球{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "火球销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15051"],userControl: false)]
    public void 火球销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"火球{@event.SourceId()}");
    }
    
    #endregion
    
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

