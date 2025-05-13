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
using KodakkuAssist.Extensions;

namespace Battle_in_the_Big_Keep;

[ScriptType(guid: "90783a42-7947-4392-9c3e-f3162d52c248", name: "无限城的死斗", territorys: [396],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Battle_in_the_Big_Keep
{
    const string noteStr =
        """
        v0.0.0.1:
        LV50 无限城的死斗 初版绘制
        TTS请在“用户设置”中二选一启用，请勿同时开启
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    
    [ScriptMethod(name: "开场提示", eventType: EventTypeEnum.Chat, eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^(一起上吧.*|To my side.*|行くぞ.*)$"])]
    public void 开场(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("只需攻击 <恩奇都>", duration: 5000, true);
        if (isTTS)accessory.Method.TTS("攻击恩奇都");
        if (isEdgeTTS)accessory.Method.EdgeTTS("攻击恩奇都");
    }
    
    [ScriptMethod(name: "烈风（点名圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3316"])]
    public void 烈风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "烈风";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(4f);
        dp.DestoryAt = 700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "缩小旋律 连线提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:438"])]
    public void 缩小旋律(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("靠近连线队友", duration: 5000, true);
        if (isTTS)accessory.Method.TTS("靠近连线队友");
        if (isEdgeTTS)accessory.Method.EdgeTTS("靠近连线队友");
    }
    
    [ScriptMethod(name: "加重&减速 驱散提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:9"])]
    public void 驱散debuff提示(Event @event, ScriptAccessory accessory)
    {
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (isHealer && isText)accessory.Method.TextInfo("驱散队友debuff", duration: 5000, false);
        if (isHealer && isTTS)accessory.Method.TTS("驱散队友debuff");
        if (isHealer && isEdgeTTS)accessory.Method.EdgeTTS("驱散队友debuff");
    }
    
    uint Chicken = 0;
    
    public void Init(ScriptAccessory accessory) {
        Chicken = 0; 
    }
    
    [ScriptMethod(name: "鸡之诗_变鸡记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:644"] ,userControl:false)]
    public void 鸡之诗(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        Chicken = 1; 
    }
    
    [ScriptMethod(name: "变鸡销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:644"] ,userControl:false)]
    public void 小鸡销毁(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        Chicken = 0; 
    }
    
    [ScriptMethod(name: "踩风圈提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3623"])]
    public void 踩风圈提示(Event @event, ScriptAccessory accessory)
    {
        if (Chicken != 1) return; 
        if (isText)accessory.Method.TextInfo("踩风圈", duration: 2500, false);
        if (isTTS)accessory.Method.TTS("踩风圈");
        if (isEdgeTTS)accessory.Method.EdgeTTS("踩风圈");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "突风";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "突风销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3318"],userControl: false)]
    public void 突风销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("突风");
    }
    
    [ScriptMethod(name: "导弹（点名圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3320"])]
    public void 导弹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "导弹";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "捕获 连线提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3335"])]
    public void 捕获(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("远离扯线", duration: 3000, true);
        if (isTTS)accessory.Method.TTS("远离扯线");
        if (isEdgeTTS)accessory.Method.EdgeTTS("远离扯线");
    }
    
    [ScriptMethod(name: "剑舞（连续顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3333"])]
    public void 剑舞(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "剑舞";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "正宗（直线冲锋）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3325"])]
    public void 正宗(Event @event, ScriptAccessory accessory)
    {
        // NpcYell  Id:2436  ; ActionId:3325 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正宗";
        dp.Scale = new (8f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "攻击龙头提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3626"])]
    public void 攻击龙头提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("踩风圈", duration: 2500, false);
        if (isTTS)accessory.Method.TTS("踩风圈");
        if (isEdgeTTS)accessory.Method.EdgeTTS("踩风圈");
    }
    
    [ScriptMethod(name: "电击_放电（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3650"])]
    public void 放电(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "放电";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 4600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
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