using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;


namespace A_Finale_Most_Formidable;

[ScriptType(guid: "5f55a121-1fcc-48ce-a0e8-b6fbd4ce8489", name: "激斗畏惧装甲之秘密武器", territorys: [814],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Formidable
{
    const string noteStr =
        """
        v0.0.0.1:
        LV80 特殊Fate 绘制
        激斗畏惧装甲之秘密武器
        """;
    
    public static string GetBNpcName(uint key)
    {
        var sheet = Svc.Data.GetExcelSheet<BNpcName>();
        if (sheet == null) return $"Invalid sheet: ({key})";
        var row = sheet.GetRow(key);

        return row.Singular.ToString() ?? $"Invalid Rowid: ({key})";
    }
    
    [ScriptMethod(name: "迷失连线", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^758[67]$"])]
    public void 迷失连线(Event @event, ScriptAccessory accessory)
    {
        // Data ID 7586: 迷失少女 ；7587：迷失者
        accessory.Method.TextInfo("迷失出现", duration: 5000, true);
        accessory.Method.TTS("迷失出现");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "迷失连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "迷失连线销毁", eventType: EventTypeEnum.Death, userControl: false)]
    public void 迷失连线销毁(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetName() == GetBNpcName(6737) || @event.TargetName() == GetBNpcName(6738))
        {
            accessory.Method.RemoveDraw("迷失连线");
        }
    }
    
    [ScriptMethod(name: "护卫自走人偶 击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:10868"])]
    public void 护卫自走人偶(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("击杀 <护卫自走人偶>", duration: 8000, true);
        accessory.Method.TTS("击杀小怪");
    }
    
    
    [ScriptMethod(name: "燃烧弹（地面黄圈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17397"])]
    public void 燃烧弹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "燃烧弹";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "格鲁格之火 吸引范围", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17395"])]
    public void 格鲁格之火Inhale(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("靠近BOSS脚下，不要出圈", duration: 5000, true);
        accessory.Method.TTS("靠近BOSS脚下");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "格鲁格之火Inhale";
        dp.Color = new Vector4(1f, 0f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(10f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 22800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    /*
    [ScriptMethod(name: "格鲁格之火 月环范围", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17395"])]
    public void 格鲁格之火Donut(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "格鲁格之火Donut";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.InnerScale = new Vector2(10f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 22800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    */
    
    [ScriptMethod(name: "大型炸弹 爆炸（扩大钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17411"])]
    public void 爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "爆炸";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 11700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    /*
    [ScriptMethod(name: "矮人式导弹 高亮", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:11221"])]
    public void 矮人式导弹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"矮人式导弹{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 1.6f);
        dp.Scale = new(2f, 6f); 
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    */
    
    [ScriptMethod(name: "矮人式导弹 爆炸范围预测", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:11221"])]
    public void 矮人式导弹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"矮人式导弹{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "矮人式导弹 爆炸销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:18003"], userControl: false)]
    public void 矮人式导弹爆炸销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"矮人式导弹{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "矮人式导弹 移除销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:11221"], userControl: false)]
    public void 矮人式导弹移除销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"矮人式导弹{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "矮人雷击弹（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:11228"])]
    public void 矮人雷击弹钢铁(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "矮人雷击弹钢铁";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "矮人雷击弹（月环）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:10908"])]
    public void 矮人雷击弹月环(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("靠近紫球月环", duration: 5000, true);
        accessory.Method.TTS("靠近紫球月环");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "矮人雷击弹月环";
        dp.Color = new Vector4(1f, 0f, 1f, 0.4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(8.5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "月环连线";
        dp1.Owner = accessory.Data.Me;
        dp1.Color = accessory.Data.DefaultSafeColor;
        dp1.ScaleMode |= ScaleMode.YByDistance;
        dp1.TargetObject = @event.SourceId();
        dp1.Scale = new(1);
        dp1.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
        
    }
    
    [ScriptMethod(name: "蒸汽喷发 击退", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17394"])]
    public void 蒸汽喷发(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TTS("击退");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"蒸汽喷发";
        dp.Scale = new(1.5f, 15);
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = @event.TargetPosition();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    

    [ScriptMethod(name: "运动体探知干扰器 提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1269"])]
    public async void 运动体探知干扰器(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return;
        await Task.Delay(7500);

        accessory.Method.TextInfo("停止行动", duration: 2000, true);
        accessory.Method.TTS("停止行动");
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
