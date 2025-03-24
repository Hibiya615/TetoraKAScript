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

namespace Pandæmonium.Normal;

[ScriptType(guid: "f28cc2f2-6ce2-4526-a303-56fe1c02dea8", name: "P10N", territorys: [1149],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class P10n
{
    const string noteStr =
        """
        v0.0.0.1:
        LV90 万魔殿 荒天之狱2（万魔殿）初版绘制
        """;
    
    public static class IbcHelper
    {
        public static IBattleChara? GetById(uint id)
        {
            return (IBattleChara?)Svc.Objects.SearchByEntityId(id);
        }
        
        public static IEnumerable<IGameObject?> GetByDataId(uint dataId)
        {
            return Svc.Objects.Where(x => x.DataId == dataId);
        }
    }
    
    [ScriptMethod(name: "喷吐丝网 分散提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33369"])]
    public void 喷吐丝网(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("分散，远离柱子", duration: 2700, true);
        accessory.Method.TTS("分散，远离柱子");
    }
    
    /*
    [ScriptMethod(name: "喷吐丝网 分散柱子高亮", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:01CE"])]
    public void 柱子高亮(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        foreach (var item in IbcHelper.GetByDataId(16156))
        {
            dp.Name = "柱子危险区";
            dp.Color = new Vector4(1f, 0f, 0f, 2f);
            dp.Owner = item.EntityId;
            dp.Scale = new Vector2(0.6f);
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    */
    
    [ScriptMethod(name: "喷吐丝网 分散柱子危险区", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:01CE"])]
    public void 柱子危险区(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        foreach (var item in IbcHelper.GetByDataId(16156))
        {
            dp.Name = "柱子危险区";
            dp.Color = new Vector4(1f, 1f, 1f, 0.4f);
            dp.Owner = item.EntityId;
            dp.Scale = new Vector2(7f);
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "塔钢铁（入狱）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:33374","SourceDataId:16154"])]
    public void 入狱(Event @event, ScriptAccessory accessory)
    {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "入狱";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = @event.SourceId();
                dp.Scale = new Vector2(4f);
                dp.DestoryAt = 7050;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "塔月环(扩散炮)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:33374","SourceDataId:16155"])]
    public void 扩散炮(Event @event, ScriptAccessory accessory)
    {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扩散炮";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(7f);
            dp.InnerScale = new Vector2(3f);
            dp.Radian = float.Pi * 2;
            dp.DestoryAt = 7050;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "万魔殿熔毁（直线分摊）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:34722"])]
    public void 万魔殿熔毁(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "万魔殿熔毁";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new (3f, 50f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "攥魂（分摊死刑）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:01D3"])]
    public void 攥魂(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "攥魂";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(4f);
        dp.DestoryAt = 10500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "万魔殿射线（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33382"])]
    public void 万魔殿射线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "万魔殿射线";
        dp.Scale = new (50, 30);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "分割之羽（大风车）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33393"])]
    public void 分割之羽(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "分割之羽";
        dp.Color = new Vector4(1f, 0f, 0f, 1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.Radian = 20f.DegToRad();
        dp.Delay = 2700;
        dp.DestoryAt = 1000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "空降", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33385"])]
    public void 空降(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "空降";
        dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = 10700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "空降描边";
        dp1.Color = new Vector4(1f, 0f, 0f, 8f);
        dp1.Owner = @event.TargetId();
        dp1.Scale = new Vector2(20.1f);
        dp1.InnerScale = new Vector2(20f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 10700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
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
