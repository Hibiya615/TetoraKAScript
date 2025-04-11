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
using KodakkuAssist.Extensions;

namespace Ifrit_Extreme;

[ScriptType(guid: "dbb7983a-d2c4-4621-9734-76772e3f206a", name: "伊弗利特歼殛战", territorys: [295],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Ifrit_Extreme
{
    const string noteStr =
        """
        v0.0.0.1:
        LV50 伊弗利特歼殛战 初版绘制
        TTS请在“用户设置”中二选一启用，请勿同时开启
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("【开发用】Debug模式")]
    public bool isDebug { get; set; } = false;

    public static bool isTank { get; set; }
    public static bool isDps { get; set; }
    public static bool isHealer { get; set; }
    public void Init(ScriptAccessory accessory)
    {
         var player = accessory.Data.MyObject;
         isTank = player?.IsTank() ?? false;
         isDps = player?.IsDps() ?? false;
         isHealer = player?.IsHealer() ?? false;
    }
    
    [ScriptMethod(name: "开场提示", eventType: EventTypeEnum.Countdown, eventCondition: ["Type:Stop","SourceId:E0000000"])]
    public async void 开场提示(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(3000); 
        if (isTank && isText)accessory.Method.TextInfo("难度：★\nT：3层debuff换T，炸柱子时注意减伤", duration: 5000, true);
        if (isDebug && isTank && isText)accessory.Method.SendChat("/e [DEBUG]: 检测到自身职能为：T");
        if (isDps && isText)accessory.Method.TextInfo("难度：★\nD：优先转火柱子，锁链靠近，远离热风奶", duration: 5000, true);
        if (isDebug && isDps && isText)accessory.Method.SendChat("/e [DEBUG]: 检测到自身职能为：D");
        if (isHealer && isText)accessory.Method.TextInfo("难度：★\nH：热风远离人群", duration: 5000, true);
        if (isDebug && isHealer && isText)accessory.Method.SendChat("/e [DEBUG]: 检测到自身职能为：H");

        // if (isText)accessory.Method.TextInfo("难度：★\nT：3层debuff换T，炸柱子时注意减伤\nD：优先转火柱子、锁链靠近 \nH：热风远离人群" , duration: 8000, true);
        accessory.Method.SendChat("/e ————小抄————\nT：3层debuff换T，炸柱子时注意减伤\nD：优先转火柱子、锁链靠近 \nH：热风远离人群 \n出柱子一定先打柱子，否则上天直接狂暴");
    }

    [ScriptMethod(name: "灼伤 换T提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:375","StackCount:regex:^[345]$"])]
    public void 灼伤(Event @event, ScriptAccessory accessory)
    {
        // var player = accessory.Data.MyObject;
        // var isTank = player?.IsTank() ?? false;
        if (isTank && @event.TargetId() != accessory.Data.Me && isText) accessory.Method.TextInfo("挑衅", duration: 3000, true);
        if (isTank && @event.TargetId() == accessory.Data.Me && isText) accessory.Method.TextInfo("退避", duration: 3000, true); 
        if (isTank && isTTS) accessory.Method.TTS("换T！"); 
        if (isTank && isEdgeTTS) accessory.Method.EdgeTTS("换T！");
    }
    
    /*
    [ScriptMethod(name: "火神爆裂（时间轴不确定，暂时废弃）", 
    eventType: EventTypeEnum.Chat, eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^(勇猛无比.*|勇猛無比.*|My flames shall consume thy flesh and soul both.*)", "Sender:伊弗利特"])]
    public void 火神爆裂(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("防击退", duration: 4200, false);
        if (isTTS) accessory.Method.TTS("防击退");
        if (isEdgeTTS) accessory.Method.EdgeTTS("防击退");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        var ifrits = Svc.Objects.Where(x => x.DataId == 211);
        foreach (var ifrit in ifrits)
        {
            if (ifrit.IsTargetable)
            {
                dp.Owner = ifrit.EntityId;
                dp2.Owner = ifrit.EntityId;
            }
        }
        dp.Name = "火神爆裂";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.2f);
        dp.Scale = new Vector2(21f);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        dp2.Name = "火神爆裂描边";
        dp2.Color = accessory.Data.DefaultDangerColor.WithW(8f);
        dp2.Scale = new Vector2(21.08f);
        dp2.InnerScale = new Vector2(21f);
        dp2.Radian = float.Pi * 2;
        dp2.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
    }
    */
    
    [ScriptMethod(name: "火神爆裂销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:1531"],userControl: false)]
    public void 火神爆裂销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("火神爆裂.*");
    }
    
    [ScriptMethod(name: "灼热咆哮", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1529"])]
    public void 灼热咆哮(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me && isText) accessory.Method.TextInfo("热风点名，远离人群", duration: 4500, true);
        if (@event.TargetId() == accessory.Data.Me && isTTS) accessory.Method.TTS("热风点名");
        if (@event.TargetId() == accessory.Data.Me && isEdgeTTS) accessory.Method.EdgeTTS("热风点名");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"热风{@event.TargetId()}";;
        dp.Owner = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.Scale = new(14f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = $"热风描边{@event.TargetId()}";;
        dp2.Color = accessory.Data.DefaultDangerColor.WithW(8f);
        dp2.Owner = @event.TargetId();
        dp2.Scale = new Vector2(14.06f);
        dp2.InnerScale = new Vector2(14f);
        dp2.Radian = float.Pi * 2;
        dp2.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
    }
    
    [ScriptMethod(name: "热风", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:376"])]
    public void 灼热(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"热风{@event.TargetId()}";
        dp.Owner = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.Scale = new(14f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = $"热风描边{@event.TargetId()}";;
        dp2.Color = accessory.Data.DefaultDangerColor.WithW(8f);
        dp2.Owner = @event.TargetId();
        dp2.Scale = new Vector2(14.06f);
        dp2.InnerScale = new Vector2(14f);
        dp2.Radian = float.Pi * 2;
        dp2.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
    }
    
    [ScriptMethod(name: "热风销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:376"],userControl: false)]
    public void 热风销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"热风.*{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "火狱之楔 击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:212"])]
    public void 火狱之楔(Event @event, ScriptAccessory accessory)
    {
        // if (isText) accessory.Method.TextInfo("击杀 <火狱之楔>", duration: 2500, true);
        if (isTTS) accessory.Method.TTS("击杀火狱之楔");
        if (isEdgeTTS) accessory.Method.EdgeTTS("击杀火狱之楔");
    }
    
    [ScriptMethod(name: "地火喷发", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1677"])]
    public void 地火喷发(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "地火喷发";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "火狱之锁 连线提示", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0009"])]
    //  EventTypeEnum.StatusAdd, eventCondition: ["StatusID:377"],suppress:25000
    public void 火狱之锁(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me || @event.SourceId() != accessory.Data.Me ) return; 
        if (isText) accessory.Method.TextInfo("锁链靠近", duration: 3000, true);
        if (isTTS) accessory.Method.TTS("锁链靠近");
        if (isEdgeTTS) accessory.Method.EdgeTTS("锁链靠近");
        
        /*  懒得拿时间了 不画了x
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "火狱之锁";
        dp.Owner = @event.TargetId();
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new(0.5f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        */
    }
    
    [ScriptMethod(name: "火狱之锁销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:377"],userControl: false)]
    public void 火狱之锁销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("火狱之锁.*");
    }
    
    [ScriptMethod(name: "深红旋风（火神冲直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1532"])]
    public void 深红旋风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深红旋风";
        dp.Scale = new (18, 49f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "团灭销毁", eventType: EventTypeEnum.CombatChanged, eventCondition: ["Type:ResetCombat","InCombat:False"],userControl: false)]
    public void 团灭销毁(Event @event, ScriptAccessory accessory)
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