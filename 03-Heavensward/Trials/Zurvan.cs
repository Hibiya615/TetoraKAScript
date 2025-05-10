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
using KodakkuAssist.Extensions;
using KodakkuAssist.Data;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.MathHelpers;
using System.Threading.Tasks;

namespace Zurvan;

[ScriptType(guid: "214f8fbd-ad04-430f-8bba-fd7319581780", name: "祖尔宛歼灭战", territorys: [637],
    version: "0.0.0.4", author: "Tetora", note: noteStr)]

public class Zurvan
{
    const string noteStr =
        """
        v0.0.0.4:
        LV60 祖尔宛歼灭战 初版绘制
        """;
        
    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "波动炮", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7741"])]
    public void 波动炮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "波动炮";
        dp.Scale = new (10, 55.3f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "飞翔_多重灵身", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7714"])]
    public void 多重灵身(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "多重灵身";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0.4f, 0f, 0.4f);
        dp.Scale = new(10f, 88f); 
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "鬼神冲（分摊）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:003E"])]
    public void 鬼神冲(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "鬼神冲";
        dp.Owner = @event.TargetId();
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new Vector2(7);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp); 
    }
    
    [ScriptMethod(name: "冰与火 冰圈", eventType: EventTypeEnum.Chat, 
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^(我的叹息将化为寒冰.*|By sorrow's chill doth all become ice.*|我が嘆きは、氷となって凍て付き.*)$"])]
    public void 冰与火(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("别站中间", duration: 4000, true);
        if(isTTS) accessory.Method.TTS("别站中间");
        if(isEdgeTTS) accessory.Method.EdgeTTS("别站中间");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冰与火";
        dp.Color = new Vector4(1f, 1f, 0f, 1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5.3f);
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "小怪刷新位置", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:7726"])]
    public void 小怪刷新位置(Event @event, ScriptAccessory accessory)
    {
        var player = accessory.Data.MyObject;
        if (isText)accessory.Method.TextInfo("小怪即将刷新在正北", duration: 3700, false);
        if (isTTS)accessory.Method.TTS("小怪即将刷新在正北");
        if (isEdgeTTS)accessory.Method.EdgeTTS("小怪即将刷新在正北");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "小怪刷新位置";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Position = new Vector3(0f, 0f, -11f);
        dp.Scale = new Vector2(2.1f);
        dp.DestoryAt = 4400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "拉怪引导线";
        dp2.Owner = accessory.Data.Me;
        dp2.Color = accessory.Data.DefaultSafeColor;
        dp2.ScaleMode |= ScaleMode.YByDistance;
        dp2.TargetPosition = new Vector3(0f, 0f, -11f);
        dp2.Scale = new(1);
        dp2.DestoryAt = 4400;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);

    }
    
    [ScriptMethod(name: "智慧信徒攻击提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6554"])]
    public void 智慧信徒(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank) return; 
        if(isText)accessory.Method.TextInfo("攻击智慧信徒", duration: 5000, true);
        if(isTTS) accessory.Method.TTS("攻击智慧信徒");
        if(isEdgeTTS) accessory.Method.EdgeTTS("攻击智慧信徒");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "智慧信徒连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 20500;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "智慧信徒连线销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:7731"],userControl: false)]
    public void 智慧信徒连线销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("智慧信徒连线");
    }
    
    uint MyFire=0;
    uint MyIce=0;
    
    public void Init(ScriptAccessory accessory) {
        MyFire=0;
        MyIce=0; 
    }
    
    [ScriptMethod(name:"冰炎之纹buff记录", eventType:EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^114[34]$"], userControl:false)]
    public void 冰炎之纹记录(Event @event, ScriptAccessory accessory) 
    {
        if ( @event.TargetId() != accessory.Data.Me) return;  
        switch (@event.StatusID())
        {
            case 1143:  //炎之刻印
                MyFire = 1;
                break;
            case 1144:  // 冰之刻印
                MyIce  = 1;
                break;
        }
    }
    
    [ScriptMethod(name: "冰炎之纹 踩塔提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7776"])]
    public void 冰炎之纹(Event @event, ScriptAccessory accessory)
    {
        if (MyFire == 1)
        {
            if(isText) accessory.Method.TextInfo("踩火塔", duration: 14000, true);
            if(isTTS) accessory.Method.TTS("踩火塔");
            if(isEdgeTTS) accessory.Method.EdgeTTS("踩火塔");
            
        }

        if (MyIce == 1)
        {
            if(isText) accessory.Method.TextInfo("踩冰塔", duration: 14000, false);
            if(isTTS) accessory.Method.TTS("踩冰塔");
            if(isEdgeTTS) accessory.Method.EdgeTTS("踩冰塔");
        }
        
    }
    
    [ScriptMethod(name: "冰炎之纹 buff销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^777[235]$"], userControl:false)]
    public void 冰炎之纹销毁(Event @event, ScriptAccessory accessory)
    {
        MyFire = 0;
        MyIce  = 0;
    }
    
    [ScriptMethod(name: "冰炎之纹 buff重置",  userControl:false,eventType: EventTypeEnum.Chat, 
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^(刻印发动.*|Graven in flesh, the brand is awoken.*|肉体に刻まれし、刻印を発動する.*)$"])]
    public void 冰炎之纹重置(Event @event, ScriptAccessory accessory)
    {
        MyFire = 0;
        MyIce  = 0;
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

    public static uint StatusID(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StatusID"]);
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