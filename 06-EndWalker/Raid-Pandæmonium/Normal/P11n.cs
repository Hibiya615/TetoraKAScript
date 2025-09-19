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
using System.Threading.Tasks;

namespace P11n;

[ScriptType(guid: "07be068a-65ea-4fd9-a324-9ec5326bb04b", name: "P11N", territorys: [1151],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class P11n
{
    const string noteStr =
        """
        v0.0.0.1:
        LV90 万魔殿 荒天之狱1（特弥斯） 初版绘制
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name:"分歧判决-暗黑冲击 二段扩散直线",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^3320[78]$"])]
    public void 暗黑冲击 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "暗黑冲击";
        dp.Color = new Vector4(1f, 0f, 1f, 0.6f);
        dp.Owner = @event.SourceId;
        dp.Scale = new (16, 46f);
        if (@event.ActionId == 33207)
        {
            dp.Offset = new Vector3(-16, 0, 0);
        }
        else
        {
            dp.Offset = new Vector3(16, 0, 0);
        }
        dp.Delay = 5900;
        dp.DestoryAt = 2600;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Straight,dp);
    }
    
    [ScriptMethod(name:"分歧判决-光爆 二段预兆",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:33206"])]
    public void 光爆预览 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "光爆";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.25f);
        dp.Scale = new (26, 46f);
        dp.Owner = @event.SourceId;
        dp.DestoryAt = 8500;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Straight,dp);
    }
    
    [ScriptMethod(name:"分歧判决-光爆 二段扩大直线",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:33206"])]
    public void 光爆 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "光爆";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.3f);
        dp.Scale = new (26, 46f);
        dp.Owner = @event.SourceId;
        dp.Delay = 5900;
        dp.DestoryAt = 2600;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Straight,dp);
    }
    
    [ScriptMethod(name: "光暗之判纹：失调", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^355[56]$"])]
    public void 光暗之判纹(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (@event.StatusId == 3555 && isText) accessory.Method.TextInfo("站在暗区域", duration: 5000, true);
        if (@event.StatusId == 3556 && isText) accessory.Method.TextInfo("站在光区域", duration: 5000, false);

    }
    
    [ScriptMethod(name: "驳回判决 击退预测", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3327[45]$"])]
    public void 驳回判决(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "驳回判决";
        dp.Scale = new(1.2f, 11);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(8f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
    }
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("驳回判决");
    }
    
    [ScriptMethod(name: "驳回判决-外暗（击退月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34548"])]
    public void 外暗(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "外暗";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 9200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "驳回判决-内光（击退钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34547"])]
    public void 内光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "内光";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 9200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "维持判决-黑暗回环（跳跃月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34767"])]
    public void 黑暗回环(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "黑暗回环";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 6500;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "维持判决-分摊", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:013E"])]
    public void 维持判决(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "维持判决";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 6300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "维持判决-光爆破（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34766"])]
    public void 光爆破(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "光爆破";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.Delay = 6500;
        dp.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "仇恨（连续分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33243"])]
    public void 仇恨(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "仇恨";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 9500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name:"分身_分歧判决-暗黑冲击 二段扩散直线",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^3324[12]$"])]
    public void 分身_暗黑冲击 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "暗黑冲击";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId;
        dp.Scale = new (16, 46f);
        if (@event.ActionId == 33241)
        {
            dp.Offset = new Vector3(-16, 0, 0);
        }
        else
        {
            dp.Offset = new Vector3(16, 0, 0);
        }
        dp.Delay = 7900;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);
    }
    
    [ScriptMethod(name:"分身_分歧判决-光爆 二段预兆",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:33240"])]
    public void 分身_光爆预览 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "光爆";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.Scale = new (26, 46f);
        dp.Owner = @event.SourceId;
        dp.DestoryAt = 11100;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);
    }
    
    [ScriptMethod(name:"分身_分歧判决-光爆 二段扩大直线",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:33240"])]
    public void 分身_光爆 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "光爆";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (26, 46f);
        dp.Owner = @event.SourceId;
        dp.Delay = 7900;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);
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