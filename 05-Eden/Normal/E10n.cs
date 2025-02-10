using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
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

namespace E10n;

[ScriptType(guid: "038e00e8-d378-4f43-89ab-e27df5561d5a", name: "E10N", territorys: [943],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class E10n
{
    const string noteStr =
        """
        v0.0.0.1:
        伊甸希望乐园 再生之章2（影之王）初版绘制
        """;
    
    public enum ImplosionType {
        FRONT,          // 22196
        BACK,           // 22199
        LEFT,           // 22193
        RIGHT,          // 22190
        SHADOWY_FRONT,  // 22197
        SHADOWY_BACK,   // 22200
        SHADOWY_LEFT,   // 22194
        SHADOWY_RIGHT   // 22191

    }
    
    [ScriptMethod(name: "向心聚爆 & 十亿斩击", eventType: EventTypeEnum.StartCasting,
        eventCondition:["ActionId:regex:^(2219[0134679]|22200)$"])]
    public void 半场刀判断(Event @event, ScriptAccessory accessory) {
        uint decimalActionID = Convert.ToUInt32(@event["ActionId"], 10);

        ImplosionType implosionType = ImplosionType.FRONT;
        bool actionIDConfirmed = false;

        switch (decimalActionID)
        {
            case 22196:
            {
                implosionType = ImplosionType.FRONT;
                actionIDConfirmed = true;
                break;
            }

            case 22199:
            {
                implosionType = ImplosionType.BACK;
                actionIDConfirmed = true;
                break;
            }

            case 22193:
            {
                implosionType = ImplosionType.LEFT;
                actionIDConfirmed = true;
                break;
            }

            case 22190:
            {
                implosionType = ImplosionType.RIGHT;
                actionIDConfirmed = true;
                break;
            }

            case 22197:
            {
                implosionType = ImplosionType.SHADOWY_FRONT;
                actionIDConfirmed = true;
                break;
            }

            case 22200:
            {
                implosionType = ImplosionType.SHADOWY_BACK;
                actionIDConfirmed = true;
                break;
            }

            case 22194:
            {
                implosionType = ImplosionType.SHADOWY_LEFT;
                actionIDConfirmed = true;
                break;
            }

            case 22191:
            {
                implosionType = ImplosionType.SHADOWY_RIGHT;
                actionIDConfirmed = true;
                break;
            }

            default:
            {
                accessory.Method.SendChat("/e 技能ID解析失败！");
                break;
            }
        }

            if (actionIDConfirmed) {
                    向心聚爆_十亿斩击(accessory, @event.SourceId(), implosionType);
            }
    }
    public void 向心聚爆_十亿斩击(ScriptAccessory accessory,uint entityId,ImplosionType implosionType)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        bool directionConfirmed = false;
        
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = entityId;
        dp.Scale = new Vector2(70);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 5700;
        
        switch(implosionType) {

            case ImplosionType.FRONT: {
                dp.Name = "前向心聚爆";
                directionConfirmed = true;
                break;
            }
            
            case ImplosionType.SHADOWY_FRONT: {
                dp.Name = "影·前向心聚爆";
                directionConfirmed = true;
                break;
            }

            case ImplosionType.BACK: {
                dp.Name = "后向心聚爆";
                dp.Rotation = 180f.DegToRad();
                directionConfirmed = true;
                break;
            }
            
            case ImplosionType.SHADOWY_BACK: {
                dp.Name = "影·后向心聚爆";
                dp.Rotation = 180f.DegToRad();
                directionConfirmed = true;
                break;
            }
            
            case ImplosionType.LEFT: {
                dp.Name = "左十亿斩击";
                dp.Rotation = 90f.DegToRad();
                directionConfirmed = true;
                break;
            }
            
            case ImplosionType.SHADOWY_LEFT: {
                dp.Name = "影·左十亿斩击";
                dp.Rotation = 90f.DegToRad();
                directionConfirmed = true;
                break;
            }
            
            case ImplosionType.RIGHT: {
                dp.Name = "右十亿斩击";
                dp.Rotation = -90f.DegToRad();
                directionConfirmed = true;
                break;
            }
            
            case ImplosionType.SHADOWY_RIGHT: {
                dp.Name = "影·右十亿斩击";
                dp.Rotation = -90f.DegToRad();
                directionConfirmed = true;
                break;
            }

            default: {
                accessory.Method.SendChat("/e 画图范围确定失败了！");
                break;
            }

        }
        if (directionConfirmed) {
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }
    
    [ScriptMethod(name: "痛苦钩刺(连线狗狗半场)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22233"])]
    public void 痛苦钩刺(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "痛苦钩刺";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40); //实际范围应为70m，此处为了方便看时间填充故设为场边半径
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "潜影（钻地钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:23313"])]
    public void 影之披风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "影之披风";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = 7200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "影之王权（起身钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22215"])]
    public void 影之王权(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "影之王权";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16f);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "影之王权描边";
        dp.Color = new Vector4(1f, 1f, 0f, 8f);
        dp.Scale = new(16.08f);
        dp.InnerScale = new(16);
        dp.Radian = float.Pi * 2;
        dp.Owner = @event.SourceId();
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "影之王权填充";
        dp.Color = new Vector4(1f, 1f, 0.2f, .8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16f);
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "影之斩击（直线死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:23307"])]
    public void 影之斩击(Event @event, ScriptAccessory accessory)
    {
        var pos = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "影之斩击";
        dp.Scale = new (8, 50f); 
        dp.Owner = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "影之喷发（脚下黄圈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22241"])]
    public void 影之喷发(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "影之喷发";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
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

    public static string DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<string>(@event["DurationMilliseconds"]) ?? string.Empty;
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