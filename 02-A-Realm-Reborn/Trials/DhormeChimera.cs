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

namespace DhormeChimera;

[ScriptType(guid: "e24bb311-704f-4f4c-8188-87eaa8da8b29", name: "死化奇美拉讨伐战", territorys: [368],
    version: "0.0.0.4", author: "Tetora", note: noteStr)]

public class DhormeChimera
{
    const string noteStr =
        """
        v0.0.0.3:
        LV50 死化奇美拉讨伐战 初版绘制
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    uint StaticCharge=0; // 带电 StatusID 326
    uint GelidCharge=0; // 带冰 StatusID 327
    
    public void Init(ScriptAccessory accessory) {
        StaticCharge = 0;
        GelidCharge = 0;
    }
    
    [ScriptMethod(name:"带电记录", eventType:EventTypeEnum.ActionEffect, eventCondition: ["ActionId:1283"], userControl:false)]
    public void 带电记录(Event @event, ScriptAccessory accessory)
    {
        StaticCharge = 1;
        GelidCharge = 0;
        accessory.Method.RemoveDraw("寒冰吐息");
    }
    
    [ScriptMethod(name:"带冰记录", eventType:EventTypeEnum.ActionEffect, eventCondition: ["ActionId:1284"], userControl:false)]
    public void 带冰记录(Event @event, ScriptAccessory accessory)
    {
        StaticCharge = 0;
        GelidCharge = 1;
        accessory.Method.RemoveDraw("雷电吐息");
    }
    
    [ScriptMethod(name: "伤头&插言 打断销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void 打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("(寒冰|雷电)咆哮");
    }
    
    [ScriptMethod(name: "寒冰&雷电咆哮 打断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:regex:^(1104|1338|1442)$"], userControl: false)]
    public void 寒冰雷电咆哮销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("(寒冰|雷电)咆哮");
    }
    
    [ScriptMethod(name: "雷电吐息", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:1283"])]
    public void 雷电吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "雷电吐息";
        dp.Color = new Vector4(1f, 0.2f, 1f, 0.4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9.7f);
        dp.Radian = MathHelpers.DegToRad(120f);
        dp.Rotation = MathHelpers.DegToRad(45);
        dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "寒冰吐息", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:1284"])]
    public void 寒冰吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "寒冰吐息";
        dp.Color = new Vector4(0f, 1f, 1f, 0.4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9.7f);
        dp.Radian = MathHelpers.DegToRad(120f);
        dp.Rotation = MathHelpers.DegToRad(-45);
        dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "嘈杂的噪音 点名提示", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0001"])]
    public void 嘈杂的噪音(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("避开人群溜雷球", duration: 3500, true);
        if (isTTS)accessory.Method.TTS("雷球点名");
        if (isEdgeTTS)accessory.Method.EdgeTTS("雷球点名");
    }
    
    [ScriptMethod(name: "嘈杂的噪音（雷球）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:2222"])]
    public void 无序的和声(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "无序的和声";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Delay = 3500;
        dp.DestoryAt = 8500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "无序的和声销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:1108"],userControl: false)]
    public void 无序的和声销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("无序的和声");
    }
    
    [ScriptMethod(name: "寒冰咆哮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1104"])]
    public void 寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "寒冰咆哮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "强化寒冰咆哮 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1285"])]
    public void 强化寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("打断或眩晕BOSS", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("打断或眩晕BOSS");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断或眩晕BOSS");
    }
    
    [ScriptMethod(name: "雷电咆哮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(1338|1442)$"])]
    public void 雷电咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "雷电咆哮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(6.7f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = @event.ActionId() == 1338 ? 17000 : 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
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
public static class MathHelpers
{
    public static float DegToRad(float degrees)
    {
        return degrees * (float)(Math.PI / 180.0);
    }
    
    public static double DegToRad(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    
    public static float RadToDeg(float radians)
    {
        return radians * (float)(180.0 / Math.PI);
    }
    
    public static double RadToDeg(double radians)
    {
        return radians * 180.0 / Math.PI;
    }
}