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

namespace Zeromus;

[ScriptType(guid: "1d6d7238-e986-408c-9c25-b24955542ee0", name: "泽罗姆斯歼灭战", territorys: [1168],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Zeromus
{
    const string noteStr =
        """
        v0.0.0.1:
        LV90 泽罗姆斯歼灭战 初版绘制
        TTS请在“用户设置”中二选一启用，请勿同时开启
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    

    [ScriptMethod(name: "深渊回声（地板黑洞钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35578"])]
    public void 深渊回声(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深渊回声";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 15700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "日珥焰棘（火球直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35606"])]
    public void 日珥焰棘(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "日珥焰棘";
        dp.Scale = new (10, 60f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "加速度炸弹", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2657"])]
    public async void 加速度炸弹(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        
        uint AccelerationBomb = @event.DurationMilliseconds();
        TimeSpan delay = TimeSpan.FromMilliseconds(Math.Max(0, AccelerationBomb - 2000));
        await Task.Delay(delay);
        
        if (isText) accessory.Method.TextInfo("停止行动", duration: 1500, true);
        if (isTTS) accessory.Method.TTS("停止行动");
        if (isEdgeTTS) accessory.Method.EdgeTTS("停止行动");
    }
    
    [ScriptMethod(name: "深渊激流_次元涌动（场边直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35637"])]
    public void 次元涌动(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "次元涌动";
        dp.Scale = new (14, 60f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "天光轮回", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35619"])]
    public void 天光轮回(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "天光轮回";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.Delay = 2400;
        dp.DestoryAt = 5600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深渊连爪", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3562[89]|3563[012])$"])]
    public void 深渊连爪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深渊连爪";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60);
        dp.Radian = 40f.DegToRad();
        dp.DestoryAt = @event.DurationMilliseconds() - 900;
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