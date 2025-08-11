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

namespace the_Voidcast_Dais_Extreme;

[ScriptType(guid: "ef5d2622-37eb-45ba-a40a-b93f494dd097", name: "高贝扎歼殛战", territorys: [1141],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Golbez_Extreme
{
    const string noteStr =
        """
        v0.0.0.1:
        LV90 高贝扎歼殛战 初版绘制
        """;
    
    #region 基础控制
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion
    
    #region 绘制部分
    
    [ScriptMethod(name: "迟缓地暴（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33894"])]
    public void 迟缓地暴(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "迟缓地暴";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16f);
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "迟缓电火花（暗天顶）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33898"])]
    public void 迟缓电火花(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "迟缓电火花";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "弦月连剑（半场刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^345(23|35)$"])]
    public void 弦月连剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "弦月连剑";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(22);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "弦月连剑（反向刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3454[36]$"])]
    public void 弦月连剑2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "弦月连剑";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(22);
        dp.Radian = 180f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.Delay = 6000;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "极寒突袭", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33889"])]
    public void 极寒突袭(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "极寒突袭";
        dp.Scale = new(15, 15f); 
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.4f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "风晶球", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3388[0-3])$"])]
    public void 风晶球(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "风晶球";
        dp.Scale = new (5, 30f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = @event.DurationMilliseconds() - 3500;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "虚空陨石（死刑）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0158"])]
    public void 虚空陨石(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"虚空陨石{@event.TargetId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        dp.Delay = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "虚空陨石销毁", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:0"],userControl: false)]
    public void 虚空陨石销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"虚空陨石{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "黑龙炎（3连直线死刑）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^3391[23]$"])]
    public void 黑龙炎(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "黑龙炎";
        dp.Scale = new(6, 50f); 
        dp.Owner = @event.SourceId();
        dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.TargetOrderIndex = 1;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 11500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "黑龙炎（换T提示）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:33915"], suppress:9000)]
    public void 黑龙炎换T(Event @event, ScriptAccessory accessory)
    {        
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (!isTank) return;
        if (isText)accessory.Method.TextInfo("换T！", duration: 5600, true);
        if (isTTS)accessory.Method.TTS("换T！");
        if (isEdgeTTS)accessory.Method.EdgeTTS("换T！");
    }
    
    [ScriptMethod(name: "升龙烽火（钢铁预备显示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34540"])]
    public void 升龙烽火预备(Event @event, ScriptAccessory accessory)
    {
        // 33912 黑龙剑阿珠达雅 存储钢铁
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "升龙烽火预备";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 7400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "升龙烽火（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34540"])]
    public void 升龙烽火(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "升龙烽火";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Delay = 7400;
        dp.DestoryAt = 2300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "重黑炎（双奶圆形分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33942"])]
    public void 重黑炎(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "重黑炎";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "升龙环火（月环预备）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34541"])]
    public void 升龙环火预备(Event @event, ScriptAccessory accessory)
    {
        // 33913 黑龙剑阿珠达雅 存储月环
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "升龙环火预备";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(22f);
        dp.InnerScale = new Vector2(6f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 7400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "升龙环火（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34541"])]
    public void 升龙环火(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "升龙环火";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(22f);
        dp.InnerScale = new Vector2(6f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 7400;
        dp.DestoryAt = 2300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    /* 实体会变化 得另找方式写
    [ScriptMethod(name: "虚空彗星雨（地火）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33960"])]
    public void 虚空彗星雨(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "虚空彗星雨";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6);
        dp.Delay = 2500;
        dp.DestoryAt = 1200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    */
    
    uint Flare = 0;
    uint Knockback = 0;
    
    public void Init(ScriptAccessory accessory) {
        Flare = 0; 
        Knockback = 0;
    }
    
    [ScriptMethod(name: "双重陨石 核爆点名记录", userControl:false, eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:01D9"])]
    public void 双重陨石核爆记录(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)  Flare = 1;
    }
    
    [ScriptMethod(name: "双重陨石 击退点名记录", userControl:false, eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:01DA"])]
    public void 双重陨石击退记录(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)  Knockback = 1;
    }
    
    [ScriptMethod(name: "双重陨石 点名提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33973"])]
    public async void 双重陨石(Event @event, ScriptAccessory accessory)
    {
        var isDPS = accessory.Data.MyObject?.IsDps() ?? false;
        
        await Task.Delay (500);

        if (Flare == 0 && Knockback == 0)
        {
            if (isDPS)
            {
                if (isText)accessory.Method.TextInfo("击退至北侧三人塔", duration: 9000, true);
                if (isTTS)accessory.Method.TTS("击退至北侧三人塔");
                if (isEdgeTTS)accessory.Method.EdgeTTS("击退至北侧三人塔");
            }
            else
            {
                if (isText)accessory.Method.TextInfo("击退至南侧二人塔", duration: 9000, true);
                if (isTTS)accessory.Method.TTS("击退至南侧二人塔");
                if (isEdgeTTS)accessory.Method.EdgeTTS("击退至南侧二人塔");
            }

        }
        
        if (Flare == 1)
        {
            if (isDPS)
            {
                if (isText)accessory.Method.TextInfo("击退至北侧空地放置核爆", duration: 9000, true);
                if (isTTS)accessory.Method.TTS("击退至北侧空地放置核爆");
                if (isEdgeTTS)accessory.Method.EdgeTTS("击退至北侧空地放置核爆");
            }
            else
            {
                if (isText)accessory.Method.TextInfo("击退至南侧空地放置核爆", duration: 9000, true);
                if (isTTS)accessory.Method.TTS("击退至南侧空地放置核爆");
                if (isEdgeTTS)accessory.Method.EdgeTTS("击退至南侧空地放置核爆");
            }

        }

        if (Knockback == 1)
        {
            if (isText)accessory.Method.TextInfo("场中放置击退 + 南侧引导直线", duration: 9800, true);
            if (isTTS)accessory.Method.TTS("场中放置击退，然后南侧引导直线");
            if (isEdgeTTS)accessory.Method.EdgeTTS("场中放置击退，然后南侧引导直线");
        }
        
    }
    
    [ScriptMethod(name: "黑炎俯冲（引导直线）", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0011"])]
    public void 黑炎俯冲(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "黑炎俯冲";
        dp.Scale = new(12f, 50f); 
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isTTS)accessory.Method.TTS("南侧引导直线");
            if (isEdgeTTS)accessory.Method.EdgeTTS("南侧引导直线");
        }
            
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