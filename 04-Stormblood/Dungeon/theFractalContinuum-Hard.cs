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


namespace the_Fractal_Continuum_Hard;

[ScriptType(guid: "c76136e1-1b5b-4cfb-a677-4cc0917fa050", name: "疯狂战舰无限回廊", territorys: [743],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class the_Fractal_Continuum_Hard
{
    const string noteStr =
        """
        v0.0.0.2:
        疯狂战舰无限回廊 副本绘制
        注意：BOSS2三斗神与尾王光柱地火未经实战测试，若有误请带ARR反馈
        """;
    
    #region 小怪
    [ScriptMethod(name: "小怪-弥诺陶洛斯 XX吨横扫（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^10(981|658)$"])]
    public void XX吨横扫(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"XX吨横扫{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
 
    [ScriptMethod(name: "小怪-奇美拉 寒冰咆哮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2144"])]
    public void 寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"寒冰咆哮{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.4f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "小怪-奇美拉 雷电咆哮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2145"])]
    public void 雷电咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"雷电咆哮{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(7.4f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    #endregion
    
    #region BOSS1_浮游炮主板
    [ScriptMethod(name: "BOSS1_浮游炮主板 攻城炮（面前直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10149"])]
    public void 攻城炮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "攻城炮";
        dp.Scale = new (8, 20f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS1_原型浮游炮 扩散射线（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10153"])]
    public void 扩散射线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "扩散射线";
        dp.Scale = new (4, 45f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS1_浮游炮主板 高压雷流（横线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10149"])]
    public void 高圧雷流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "高圧雷流";
        dp.Scale = new (40, 4f);
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
    }
    #endregion
    
    #region BOSS2_究极战士
    [ScriptMethod(name: "BOSS2_究极战士 攻城炮（面前直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10130"])]
    public void 攻城炮2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "攻城炮2";
        dp.Scale = new (6, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS2_究极战士 无限（扩大圆圈）", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2009284", "Operate:Add", "Kind:EventObj"])]
    public void 无限(Event @event, ScriptAccessory accessory)
    {
        // 10135 点名分散 意志 ； 10136 无限（EventObj)
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "无限";
        dp.Color = new Vector4(1f, 0f, 0f, 0.5f);
        dp.Position = @event.SourcePosition();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 22300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS2_究极战士 冰炎刻印 提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1143"])]
    public void 冰炎刻印提示(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        // 1143 炎之刻印 ； 1144 冰之刻印
        switch (@event.StatusId())
        {
            case 1143:
                accessory.Method.TextInfo("踩火塔", duration: 3000, true);
                break;
            case 1144:
                accessory.Method.TextInfo("踩冰塔", duration: 3000, true);
                break;
        }
    }
    #endregion
    
    #region BOSS3_究极神兽
    [ScriptMethod(name: "BOSS3_究极神兽 死亡回旋（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1015[78]$"])]
    public void 死亡回旋(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "死亡回旋";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 5700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_究极神兽 以太曲折（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^101(59|60)$"])]
    public void 以太曲折(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "以太曲折";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "BOSS3_究极神兽 光柱（冰花地火）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10171"])]
    public void 光柱(Event @event, ScriptAccessory accessory)
    {
        // 冰花初始位置 10170 光柱
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "光柱";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.Delay = 1900;
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    #endregion
    
    #region 死亡销毁
    [ScriptMethod(name: "弥诺陶洛斯死亡销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:regex:^10(981|658)$"],userControl: false)]
    public void 弥诺陶洛斯死亡销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"XX吨横扫{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "奇美拉死亡销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:regex:^214(4|5)$"],userControl: false)]
    public void 奇美拉死亡销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"寒冰咆哮{@event.SourceId()}");
        accessory.Method.RemoveDraw($"雷电咆哮{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "BOSS死亡销毁", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:regex:^(8390|2|3)$"],userControl: false)]
    public void BOSS死亡销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
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
