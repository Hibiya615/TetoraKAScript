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

namespace E1n;

[ScriptType(guid: "35c751e5-2958-4f55-b783-405b4acfde1b", name: "E1N", territorys: [849],
    version: "0.0.0.11" +
             "", author: "Tetora", note: noteStr)]

public class E1n
{
    const string noteStr =
        """
        v0.0.0.11:
        LV80 伊甸希望乐园 觉醒之章1（至尊伊甸）初版绘制
        """;
    
    public static class IbcHelper
    {
        public static IBattleChara? GetById(uint id)
        {
            return (IBattleChara?)Svc.Objects.SearchByEntityId(id);
        }
    
        public static IBattleChara? GetMe()
        {
            return Svc.ClientState.LocalPlayer;
        }
    
        public static IGameObject? GetFirstByDataId(uint dataId)
        {
            return Svc.Objects.Where(x => x.DataId == dataId).FirstOrDefault();
        }
    
        public static IEnumerable<IGameObject?> GetByDataId(uint dataId)
        {
            return Svc.Objects.Where(x => x.DataId == dataId);
        }
    }
    
    [ScriptMethod(name: "伊甸重力（百分比AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15764"])]
    public void 伊甸重力(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("60%真伤AOE，血盾有效", duration: 3700, true);
    }
    
    [ScriptMethod(name: "恶习与美德1 月环范围", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17637"])]
    public void 恶习与美德1(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "恶习与美德1";
        dp.Color = new Vector4(0f, 1f, 0f, 4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.InnerScale = new Vector2(4.95f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 8400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "恶习与美德1 点名提示", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:001C"])]
    public void 冷漠之恶(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.TextInfo("出去放黑泥，然后回中间", duration: 5700, true);
    }
    
    [ScriptMethod(name: "恶习与美德2 直线死刑", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0011"])]
    public void 虚荣之恶(Event @event, ScriptAccessory accessory)
    {
        // 也有 TargetIcon 可以用 ， Id:00AC
        accessory.Method.TTS("坦克直线死刑");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        var boss = IbcHelper.GetFirstByDataId(9369);
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;
        
        dp.Name = "虚荣之恶";
        dp.Scale = new (6, 100f);
        dp.TargetObject = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    uint ParadiseLost=0;
    
    public void Init(ScriptAccessory accessory) {
        ParadiseLost=0;
    }
    
    [ScriptMethod(name:"失乐园记录", eventType:EventTypeEnum.StartCasting, eventCondition: ["ActionId:15775"], userControl:false)]
    public void 失乐园(Event @event, ScriptAccessory accessory) 
    {
        ParadiseLost=1;
    }
    
    [ScriptMethod(name: "净土之光（半场刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15779"])]
    public void 净土之光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "净土之光";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60);
        dp.Radian = 180f.DegToRad();
        if ( ParadiseLost==1 )
        {
            dp.Delay = 3700;
        }
        dp.DestoryAt = 6500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
        
        ParadiseLost=0;
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
