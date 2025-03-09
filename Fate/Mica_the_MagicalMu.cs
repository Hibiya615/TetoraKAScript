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


namespace Mascot_Murder;

[ScriptType(guid: "22134617-0ca4-463e-a40d-675ef1c20cf2", name: "亩鼠米卡：盛装巡游皆大欢喜", territorys: [1192],
    version: "0.0.0.12", author: "Tetora", note: noteStr)]

public class Mica_the_Magical_Mu
{
    const string noteStr =
        """
        v0.0.0.12:
        LV100 特殊Fate 绘制
        亩鼠米卡：盛装巡游皆大欢喜
        """;
    
    #region 迷失相关

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
    
    [ScriptMethod(name: "迷失连线销毁", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:regex:^758[67]$"],userControl: false)]
    public void 迷失连线销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("迷失连线");
    }
    #endregion
    
    #region 卡牌机制

    [ScriptMethod(name: "卡牌戏法_抽卡安全区", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39156"])]
    public void 卡牌戏法Safe(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "卡牌戏法Safe";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultSafeColor; 
        dp.Scale = new(14f, 20f); 
        dp.DestoryAt = 1200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "卡牌戏法_抽卡危险区", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38679"])]
    public void 卡牌戏法Danger(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "卡牌戏法Danger";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 0.5f);
        dp.Scale = new(14f, 20f); 
        dp.DestoryAt = 1200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    #endregion
    
    #region 卡牌机制 Debug
    
    #endregion

    #region 基础机制
    
    [ScriptMethod(name: "骑滚球（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3899[6-9]$"])]
    public void 骑滚球(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "骑滚球";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = 1700;
        switch (@event.ActionId())
        {
            case 38996:
                dp.DestoryAt = 10200;
                dp.Scale = new(10, 20f);
                break;

            case 38997: 
            case 38998: 
            case 38999: 
                dp.DestoryAt = 8500;
                dp.Scale = new(10, 52f);
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "喝彩回应-魔术爆发（二段钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38697"])]
    public void 魔术爆发(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "魔术爆发";
        dp.Color = new Vector4(1f, 1f, 0f, 1.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Delay = 4600;
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "盛装致敬-魔术环（二段月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38700"])]
    public void 魔术环(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "魔术环";
        dp.Color = new Vector4(1f, 1f, 0f, 1.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(10f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 4600;
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "双重魔术雷（二段扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38694"])]
    public void 双重魔术雷(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "双重魔术雷";
        dp.Color = new Vector4(1f, 1f, 0f, 1.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 60f.DegToRad();
        dp.Delay = 4600;
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "倾泻的闪烁（魔术环直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38692"])]
    public void 倾泻的闪烁(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "倾泻的闪烁";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(5f, 42f); 
        dp.Delay = 800;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    #endregion
    
    [ScriptMethod(name: "鼠鼠死亡销毁", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:17387"],userControl: false)]
    public void 鼠鼠死亡销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
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
