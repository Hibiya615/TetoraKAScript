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

namespace Hydaelyn;

[ScriptType(guid: "d9106d0c-3c5e-4feb-ae49-f27a70acf39f", name: "海德林歼灭战", territorys: [995],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Hydaelyn
{
    const string noteStr =
        """
        v0.0.0.1:
        LV89 海德林歼灭战 初版绘制
        """;
    
    uint DawnMantle = 0; // 职责更换
    uint SpreadOut = 0; // 水晶化：分散
    uint Stack = 0; // 水晶化：分摊
    uint Beacon = 0; // 幻日：光芒（直线）
    
    public void Init(ScriptAccessory accessory) {
        DawnMantle = 0;
        SpreadOut = 0;
        Stack = 0;
        Beacon = 0;
    }
    
    [ScriptMethod(name: "职责更换记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27660"], userControl:false)]
    public void 职责更换(Event @event, ScriptAccessory accessory)
    {
        DawnMantle = 1;
    }
    
    [ScriptMethod(name: "反假日（变身月环）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2878"])]
    public void 反假日(Event @event, ScriptAccessory accessory)
    {
        if ( DawnMantle != 1) return; 
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "反假日";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(40f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        DawnMantle = 0;
    }
    
    [ScriptMethod(name: "至高神圣（变身钢铁）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2877"])]
    public void 至高神圣(Event @event, ScriptAccessory accessory)
    {
        if ( DawnMantle != 1) return; 
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "至高神圣";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 3800;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        DawnMantle = 0;
    }
    
    [ScriptMethod(name: "昼夜二分（变身十字）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2876"])]
    public void 昼夜二分(Event @event, ScriptAccessory accessory)
    {
        if ( DawnMantle != 1) return; 
        
        var currentProperty = accessory.Data.GetDefaultDrawProperties();
        currentProperty.Name = "昼夜二分";
        currentProperty.Owner = @event.TargetId();
        currentProperty.Color = accessory.Data.DefaultDangerColor; 
        currentProperty.Scale = new(10f, 40f); 
        currentProperty.DestoryAt = 4500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, currentProperty);  
        
        for(int i=1;i<=2;++i) {
            accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Straight,currentProperty);
            currentProperty.Rotation+=90f.DegToRad();
        }
        
        DawnMantle = 0;
    }
    
    [ScriptMethod(name: "演绎家的蔑视（分摊死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26070"])]
    public void 演绎家的蔑视(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "演绎家的蔑视";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(4f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "守护者的斩断（顺劈死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26069"])]
    public void 守护者的斩断(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "守护者的斩断";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Scale = new Vector2(40);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "水晶垒石（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27737"])]
    public void 水晶垒石(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "水晶垒石";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "水晶化：分散", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2056", "Param:339"])]
    public void 水晶化分散(Event @event, ScriptAccessory accessory)
    {
        SpreadOut = 1;
        if ( SpreadOut == 1) accessory.Method.TextInfo("水晶化：分散", duration: 5000, false);
    }
    
    [ScriptMethod(name: "水晶化：分摊", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2056", "Param:338"])]
    public void 水晶化分摊(Event @event, ScriptAccessory accessory)
    {
        Stack = 1;
        if ( Stack == 1) accessory.Method.TextInfo("水晶化：分摊 ", duration: 5000, true);
    }
    

    // 26032 幻日 ； 映幻日 27734 ； 光芒 Beacon（直线）26062射出，26063返回   需要在第三组直线的前端，穿进第一组直线的末端位置 
    [ScriptMethod(name: "映幻日（回转直线）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26062"])]
    public void 光芒(Event @event, ScriptAccessory accessory)
    {
        if ( Beacon >= 10) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "光芒";
        dp.Scale = new (6, 45f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = 5000;
        dp.DestoryAt = 10600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
        
        ++Beacon;
    }

    
    [ScriptMethod(name: "幻日环_幻闪光（丢球钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:13571"])]
    public void 幻闪光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "幻闪光";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Delay = 5800;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "回声（分摊）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0131"])]
    public void 回声(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "回声";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 9600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "光波", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2011723"])]
    public void 光波(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "光波";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 1f, 0f, 0.3f);
        dp.Scale = new(16f, 44f); 
        dp.DestoryAt = 19000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
    }
    
}

#region 
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
#endregion