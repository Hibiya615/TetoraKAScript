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

namespace the_Aery;

[ScriptType(guid: "ac6caf4e-5eee-406c-8621-a5f3c5c7e9b0", name: "邪龙王座龙巢神殿", territorys: [1065],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class the_Aery
{
    const string noteStr =
        """
        v0.0.0.1:
        LV55 邪龙王座龙巢神殿 初版绘制
        TTS请在“用户设置”中二选一启用，请勿同时开启
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    
    [ScriptMethod(name: "BOSS1_琅妲巫龙 雷云放射（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3889"])]
    public void 雷云放射(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "雷云放射";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(64.9f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }

    [ScriptMethod(name: "BOSS1_琅妲巫龙 引雷 提示", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0006"])]
    public void 引雷(Event @event, ScriptAccessory accessory)
    {
        // StatusID:2574  引雷
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("将连线传给场边柱子", duration: 9600, true);
        if (isTTS)accessory.Method.TTS("将连线传给场边柱子");
        if (isEdgeTTS)accessory.Method.EdgeTTS("将连线传给场边柱子");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "避雷";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(2f);
        dp.DestoryAt = 9600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "避雷销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:2574"],userControl: false)]
    public void 避雷销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("避雷");
    }
    
    [ScriptMethod(name: "BOSS1_琅妲巫龙 感电 击退提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3890"])]
    public void 感电击退(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("击退", duration: 4000, false);
        if (isTTS)accessory.Method.TTS("击退");
        if (isEdgeTTS)accessory.Method.EdgeTTS("击退");
        
        /*
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "感电击退";
        dp.Scale = new(1.5f, 15);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        */
    }
    
    [ScriptMethod(name: "BOSS1_琅妲巫龙 感电 直线AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3890"])]
    public void 感电(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "感电";
        dp.Scale = new (5f, 64.9f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("感电击退");
    }
    
    [ScriptMethod(name: "BOSS2_格亚斯库图斯 可燃性气体（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30181"],suppress:600000)]
    public void 可燃性气体(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "可燃性气体";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(26f);
        dp.InnerScale = new Vector2(20f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "BOSS2_格亚斯库图斯 爆炸（可燃性气体）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30184"])]
    public void 爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "爆炸";
        dp.Color = new Vector4(1f, 1f, 1f, 2.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 9700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS2_格亚斯库图斯 龙啸（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31233"])]
    public void 龙啸2(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("AOE", duration: 3000, false);
        if (isTTS)accessory.Method.TTS("AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "BOSS2_格亚斯库图斯 圆焰（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30191"])]
    public void 圆焰(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "圆焰";
        dp.Color = new Vector4(1f, 1f, 0f, 1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS2_格亚斯库图斯 环火（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30190"])]
    public void 环火(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "环火";
        dp.Color = new Vector4(1f, 1f, 0f, 1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.InnerScale = new Vector2(11f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "小怪_龙巢变种龙 撕裂利爪（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5137"])]
    public void 撕裂利爪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "撕裂利爪";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "撕裂利爪销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:5137"],userControl: false)]
    public void 撕裂利爪销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("撕裂利爪");
    }
    
    [ScriptMethod(name: "BOSS3_尼德霍格 龙啸（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30206"])]
    public void 龙啸3(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("AOE", duration: 4000, false);
        if (isTTS)accessory.Method.TTS("AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "BOSS3_尼德霍格 恐惧咆哮（脚底圆形-Imgui）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30200"])]
    public void 恐惧咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "恐惧咆哮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
    }


    [ScriptMethod(name: "BOSS3_尼德霍格 邪炎俯冲 提前显示", eventType: EventTypeEnum.SetObjPos, eventCondition: ["Id:0197", "SourceDataId:14794", 
        "SourcePosition:regex:^({\"X\":35.00,\"Y\":149.24,\"Z\":-304.00}|{\"X\":13.00,\"Y\":147.97,\"Z\":-304.00}|{\"X\":57.00,\"Y\":147.97,\"Z\":-304.00})$"])]
    public void 邪炎俯冲(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "邪炎俯冲";
        dp.Scale = new (22f, 80f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }

    
    [ScriptMethod(name: "BOSS3_漆黑之珠 漆黑火焰 集火提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:14795"])]
    public void 漆黑之珠(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("集火 <漆黑之珠>", duration: 5000, true);
        if (isTTS)accessory.Method.TTS("集火漆黑之珠");
        if (isEdgeTTS)accessory.Method.EdgeTTS("集火漆黑之珠");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "漆黑之珠连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "漆黑火焰销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:30204"],userControl: false)]
    public void 漆黑火焰销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("漆黑之珠连线");
    }
    
    [ScriptMethod(name: "BOSS3_尼德霍格 真红之珠（死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30205"])]
    public void 真红之珠(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("坦克死刑", duration: 4000, false);
        if (isTTS)accessory.Method.TTS("坦克死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS("坦克死刑");
    }
    
    [ScriptMethod(name: "BOSS3_尼德霍格 屠戮（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30207"])]
    public void 屠戮(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("AOE", duration: 5000, false);
        if (isTTS)accessory.Method.TTS("AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS("AOE");
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