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

namespace the_Palace_of_the_Dead;

[ScriptType(guid: "4210c323-eba4-4d67-a7e7-b90799494729", name: "死者宫殿", author: "Tetora", 
    //territorys: uint [Regex:(56[1-5]|59[3-9]|60[0-7])],
    territorys: [561,562,563,564,565,593,594,595,596,597,598,599,600,601,602,603,604,605,606,607],
    version: "0.0.0.1",note: noteStr)]

public class the_Palace_of_the_Dead
{
    const string noteStr =
        """
        v0.0.0.1:
        死者宫殿绘制
        注：方法设置中的层数仅做分割线效果，并不是批量开关
        现支持层数：171~200
        严重错误：暂未支持【形态变化】【眩晕】【催眠】等限制
        """;
    
    //眩晕、催眠、无法发动技能1113等状态都需要销毁绘图
    
    // 通用内容
    [ScriptMethod(name: "拟态怪_怨念提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6397"])]
    public void 拟态怪_怨念(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("打断拟态怪", duration: 2700, true);
        accessory.Method.TTS("打断拟态怪");
    }
    
    // 21~30层 小怪
    [ScriptMethod(name: "\ue05b 地宫斯卡尼特 唧唧咋咋（睡眠钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6365"])]
    public void 地宫斯卡尼特_唧唧咋咋(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "地宫斯卡尼特_唧唧咋咋";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f); //还需要加目标圈
        dp.DestoryAt = 2200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    // 101~110层 小怪
    [ScriptMethod(name: "—————— \ue061\ue060\ue061 ~ \ue061\ue061\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第101层(Event @event, ScriptAccessory accessory) { }
    
    // 111~120层 小怪
    [ScriptMethod(name: "—————— \ue061\ue061\ue061 ~ \ue061\ue062\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第111层(Event @event, ScriptAccessory accessory) { }
    
    // 121~130层 小怪
    [ScriptMethod(name: "—————— \ue061\ue062\ue061 ~ \ue061\ue063\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第121层(Event @event, ScriptAccessory accessory) { }
    
    // 131~140层 小怪
    [ScriptMethod(name: "—————— \ue061\ue063\ue061 ~ \ue061\ue064\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第131层(Event @event, ScriptAccessory accessory) { }
    
    // 141~150层 小怪
    [ScriptMethod(name: "—————— \ue061\ue064\ue061 ~ \ue061\ue065\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第141层(Event @event, ScriptAccessory accessory) { }
    
    // 151~160层 小怪
    [ScriptMethod(name: "—————— \ue061\ue065\ue061 ~ \ue061\ue066\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第151层(Event @event, ScriptAccessory accessory) { }
    
    // 161~170层 小怪
    [ScriptMethod(name: "—————— \ue061\ue066\ue061 ~ \ue061\ue067\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第161层(Event @event, ScriptAccessory accessory) { }
    
    // 171~180层 小怪
    [ScriptMethod(name: "—————— \ue061\ue067\ue061 ~ \ue061\ue068\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第171层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫独眼雪巨人 怒视（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7061"])]
    public void 深宫独眼雪巨人_怒视(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深宫独眼雪巨人_怒视";
        dp.Scale = new (7, 21f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "深宫独眼雪巨人 百吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6971"])]
    public void 深宫独眼雪巨人_百吨回转(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深宫独眼雪巨人_百吨回转";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫大脚巨猿 捶胸（脱战钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6973"])]
    public void 深宫大脚巨猿_捶胸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫大脚巨猿_捶胸{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0.8f, 0f, 0.3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(53.6f);
        dp.Delay = 1700;
        dp.DestoryAt = 15300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫大脚巨猿 捶胸销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:6499"], userControl: false)]
    public void 深宫大脚巨猿_捶胸销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"深宫大脚巨猿_捶胸{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "\ue061\ue068\ue060 丹代恩索涅 吸引（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6384"])]
    public void 丹代恩索涅_吸引(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"丹代恩索涅_吸引{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 44000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "丹代恩索涅 吸引销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:6384"], userControl: false)]
    public void 深宫大脚巨猿_吸引销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"丹代恩索涅_吸引{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "\ue061\ue068\ue060 丹代恩索涅 黄道陨石", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7166"])]
    public void 丹代恩索涅_黄道陨石(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("80%真伤", duration: 5, true);
        accessory.Method.TTS("80%真伤");
    }
    
    // 181~190层 小怪
    [ScriptMethod(name: "—————— \ue061\ue068\ue061 ~ \ue061\ue069\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第181层(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "\ue05e 深宫加姆 寒冰咆哮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7078"])]
    public void 深宫加姆_寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深宫加姆_寒冰咆哮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.4f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "\ue05e 深宫加姆 雷电咆哮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7079"])]
    public void 深宫加姆_雷电咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深宫加姆_雷电咆哮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(7f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    // 190层 BOSS 爆弹怪教父
    [ScriptMethod(name: "\ue061\ue069\ue060 眩晕爆弹怪 冰碎（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6387"])]
    public void 眩晕爆弹怪_冰碎(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "眩晕爆弹怪_冰碎";
        dp.Color = new Vector4(1f, 0.5f, 0f, 1.5f);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6.6f);
        dp.DestoryAt = 8400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "\ue061\ue069\ue060 治疗爆弹怪 击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6385"])]
    public void 治疗爆弹怪_击杀提示(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("击杀治疗爆弹怪", duration: 5, true);
        accessory.Method.TTS("击杀治疗爆弹怪");
    }
    
    [ScriptMethod(name: "\ue061\ue069\ue060 熔岩爆弹怪 震撼弹提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6386"])]
    public void 熔岩爆弹怪_震撼弹(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("将熔岩爆弹怪推至BOSS脚下", duration: 5, true);
        accessory.Method.TTS("将熔岩爆弹怪推至BOSS脚下");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "熔岩爆弹怪_震撼弹";
        dp.Color = new Vector4(1f, 0f, 0f, 3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.2f);
        dp.DestoryAt = 24700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "熔岩爆弹怪 震撼弹销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:7170"], userControl: false)]
    public void 熔岩爆弹怪_震撼弹销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"熔岩爆弹怪_震撼弹");
    }
    
   /*
    [ScriptMethod(name: "\ue061\ue069\ue060 爆弹怪教父 特大爆炸提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7103"])]
    public void 爆弹怪教父_特大爆炸提示(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("99.9%真伤，注意瞬回", duration: 10, true);
        accessory.Method.TTS("99.9%真伤，注意瞬回");
    }
    */
    
    [ScriptMethod(name: "特大爆炸打断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:7103"], userControl: false)]
    public void 特大爆炸打断销毁(Event @event, ScriptAccessory accessory)
    {
        /* 判断读条时间：
         if 7103 读条至14.8s时 ：触发方法
         else：取消对应方法触发
         */
    }
    
    
    // 191~200层 小怪
    [ScriptMethod(name: "—————— \ue061\ue069\ue061 ~ \ue062\ue060\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第191层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "奥尼克斯龙 邪视（背对）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7043"])]
    public void 奥尼克斯龙_邪视(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "奥尼克斯龙_邪视";
        dp.Color = new Vector4(1f, 0f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f); 
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "\ue05e 深宫幽鬼之眼 5级即死 （扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7084"])]
    public void 深宫幽鬼之眼_5级即死(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "深宫幽鬼之眼_5级即死";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.1f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
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
