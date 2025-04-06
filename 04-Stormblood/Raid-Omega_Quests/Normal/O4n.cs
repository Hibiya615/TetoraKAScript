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

namespace O4n;

[ScriptType(guid: "f24f9cef-717e-4a2a-9616-d470443d6d8a", name: "O4N", territorys: [694],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class O4n
{
    const string noteStr =
        """
        v0.0.0.1:
        LV70 欧米茄时空狭缝 德尔塔幻境4（艾克斯迪司）绘制
        基于贾XX的绘制上更改与补充
        """;
    
    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "死亡宣告", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:910"])]
    public void 死亡宣告(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("奶妈康复死宣", 2500);
    }

    [ScriptMethod(name: "暴雷（范围死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9405"])]
    public void 暴雷(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "暴雷";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId;
        dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.CentreOrderIndex = 1;
        dp.Scale = new Vector2(5);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "暴雷（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9409"])]
    public void 暴雷钢铁(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "暴雷钢铁";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(14.8f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "冰封（冻结）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9408"])]
    public void 冰封(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("保持移动", duration: 4000, false);
    }
    
    [ScriptMethod(name: "爆炎（火分散）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0019"])]
    public void 爆炎分散(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.TargetId();
        dp.Name = "爆炎分散";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Scale = new Vector2(4);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "爆炎（热病）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9407"])]
    public void 热病(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("保持静止", duration: 7400, true);
    }
    
    [ScriptMethod(name: "死亡吐息", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9419"])]
    public void 死亡吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(20);
        dp.Radian = float.Pi * 2 / 3;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "神圣（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9413"])]
    public void 神圣(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.TargetId();
        dp.Name = "神圣";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "黑洞", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:7802"])]
    public void 黑洞(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Name = "黑洞";
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(2);
        dp.DestoryAt = 16500;
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "黑洞描边";
        dp2.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp2.Owner = @event.SourceId();
        dp2.Scale = new Vector2(2.08f);
        dp2.InnerScale = new Vector2(2f);
        dp2.Radian = float.Pi * 2;
        dp2.DestoryAt = 16500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
    }

    [ScriptMethod(name: "黑洞销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:7802"],userControl: false)]
    public void 黑洞销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("黑洞.*");
    }
    
    [ScriptMethod(name: "真空波（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9400"])]
    public void 真空波(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "真空波";
        dp.Scale = new(1.5f, 11f);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("真空波");
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
