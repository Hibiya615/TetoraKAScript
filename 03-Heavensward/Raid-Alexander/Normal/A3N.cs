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
// using ECommons;
// using ECommons.DalamudServices;
// using ECommons.GameFunctions;
// using ECommons.MathHelpers;
using System.Threading.Tasks;

namespace A3N;

[ScriptType(guid: "69712d1f-26c4-441f-97f0-e63f8baf98a6", name: "A3N", territorys: [444],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class A3N
{
    const string noteStr =
        """
        v0.0.0.2:
        LV60 亚历山大机神城 启动之章3（有生命活水） 初版绘制
        """;
    
    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    
    [ScriptMethod(name: "冲洗 圆形点名", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:001A"])]
    public void 冲洗(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冲洗";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "冲净 场中击退", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4863"])]
    public void 冲净(Event @event, ScriptAccessory accessory)
    {
         // 该击退为随机方向击退，击退距离20m，场地半径22.5m
         var dp = accessory.Data.GetDefaultDrawProperties();
         dp.Name = "冲净击退连线";
         dp.Owner = accessory.Data.Me;
         dp.Color = accessory.Data.DefaultSafeColor;
         dp.ScaleMode |= ScaleMode.YByDistance;
         dp.TargetPosition = new Vector3(58f, -9f, -63f);
         dp.Scale = new(1);
         dp.DestoryAt = 3000;
         accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("冲净.*");
    }

    uint MyMagnetism = 0;
    uint PartnerMagnetism = 0;

    
    public void Init(ScriptAccessory accessory) {
        MyMagnetism = 0;
        PartnerMagnetism = 0;
    }
    
    [ScriptMethod(name:"磁力buff记录", eventType:EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^69[89]$"], userControl:false)]
    public void 磁力记录(Event @event, ScriptAccessory accessory)
    {         
        if (@event.TargetId() == accessory.Data.Me)
            switch (@event.StatusID())
            {
                case 698: //磁力：+
                    MyMagnetism = 1;
                    break;
                case 699: // 磁力：-
                    MyMagnetism = 2;
                    break;
            }
        
        if (@event.TargetId() != accessory.Data.Me)
            switch (@event.StatusID())
            {
                case 698: //磁力：+
                    PartnerMagnetism = 1;
                    break;
                case 699: // 磁力：-
                    PartnerMagnetism  = 2;
                    break;
            }
    }
    
    [ScriptMethod(name: "磁力 连线提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4858"])]
    public void 磁力提示(Event @event, ScriptAccessory accessory)
    {
        if ( MyMagnetism == 0 ) return;  
        if ( MyMagnetism == PartnerMagnetism)
        {
            if(isText) accessory.Method.TextInfo("靠近连线搭档，击退到安全区", duration: 5000, true);
            if(isTTS) accessory.Method.TTS("靠近连线搭档，击退到安全区");
            if(isEdgeTTS) accessory.Method.EdgeTTS("靠近连线搭档，击退到安全区");
        }
        else
        {
            if(isText) accessory.Method.TextInfo("远离连线搭档，吸引到安全区", duration: 5000, true);
            if(isTTS) accessory.Method.TTS("远离连线搭档，吸引到安全区");
            if(isEdgeTTS) accessory.Method.EdgeTTS("远离连线搭档，吸引到安全区");
        }
        
    }
        
    [ScriptMethod(name: "磁力buff销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:4871"], userControl:false)]
    public void 磁力销毁(Event @event, ScriptAccessory accessory)
    {
        MyMagnetism = 0;
        PartnerMagnetism = 0;
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