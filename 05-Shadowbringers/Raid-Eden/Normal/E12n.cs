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

namespace E12n;

[ScriptType(guid: "3f88ad9c-e7a7-4e00-b19e-546609b319ba", name: "E12N", territorys: [945],
    version: "0.0.0.1" , author: "Tetora", note: noteStr)]

public class E12n
{
    const string noteStr =
        """
        v0.0.0.1:
        LV80 伊甸希望乐园 再生之章4（伊甸之约）初版绘制
        """;
    
    [ScriptMethod(name: "制裁之界雷（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(20016|22623)$"])]
    public void 制裁之界雷(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "制裁之界雷";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "临时洋流（两边）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(20013|22620)$"])]
    public void 临时洋流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "临时洋流";
        dp.Scale = new (40f, 25f);
        dp.Offset = new Vector3 (0,0,-5f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "凶猛风暴（四角）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(22622|20015)$"])]
    public void 凶猛风暴(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "凶猛风暴";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 45f.DegToRad();
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "瞬燃强袭（前后）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(22621|20014)$"])]
    public void 瞬燃强袭(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "瞬燃强袭";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 150f.DegToRad();
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    uint ExplosionFirst=0;
    
    public void Init(ScriptAccessory accessory) {
        ExplosionFirst=0;
    }
    
    [ScriptMethod(name:"泰坦岩石爆炸记录", eventType:EventTypeEnum.StartCasting, eventCondition: ["ActionId:22637"], userControl:false)]
    public void 岩石爆炸记录(Event @event, ScriptAccessory accessory)
    {
        ++ExplosionFirst;
    }
     
    
    [ScriptMethod(name: "大地之怒 爆破岩石", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2263[79]$"])]
    public void 爆破岩石(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "爆破岩石";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(22f);
        
        switch (@event.ActionId())
        {
            case 22637:
                dp.Scale = new Vector2(10f);
                
                if (ExplosionFirst <=3 )
                {
                    dp.DestoryAt = 5700;
                }
                else
                {
                    dp.Delay = 2700;
                    dp.DestoryAt = 3000;
                }
                
                break;
            
            case 22639:
                dp.Scale = new Vector2(22f);
                dp.DestoryAt = 5700;
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "钻石星尘 冰面提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22630"])]
    public void 钻石星尘(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("停止移动", duration: 5500, true);
    }
    
    
    [ScriptMethod(name: "愉悦之手（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2264[78]$"])]
    public void 愉悦之手(Event @event, ScriptAccessory accessory)
    {
        ExplosionFirst=0;
        
        var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
        var isR = aid == 22647;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "愉悦之手";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = float.Pi / 180 * 210;
        dp.Rotation = float.Pi / 180 * 90 * (isR ? -1 : 1);
        dp.DestoryAt = 4700;
        
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
