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
using System.Threading.Tasks;

namespace O5n;

[ScriptType(guid: "af11ce46-2c6a-46ba-b2bc-c542e5f5b7b5", name: "O5N", territorys: [748],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class O5n
{
    const string noteStr =
        """
        v0.0.0.1:
        LV70 欧米茄时空狭缝 西格玛幻境1（魔列车）初版绘制
        """;
    
    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    uint Ghost=0;
    
    public void Init(ScriptAccessory accessory) {
        Ghost = 0;
    }
    
    [ScriptMethod(name:"幽灵记录", eventType:EventTypeEnum.StartCasting, eventCondition: ["ActionId:10405"], userControl:false)]
    public void 幽灵记录(Event @event, ScriptAccessory accessory)
    {
        Ghost = 1;
    }
    
    [ScriptMethod(name: "追尾 防击退提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10415"])]
    public async void 追尾(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(2000); 
        if (isText)accessory.Method.TextInfo("防击退", duration: 1500, false);
        if (isTTS)accessory.Method.EdgeTTS("防击退");
    }
    
    [ScriptMethod(name: "圣光射线 点名提示", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0001"])]
    public async void 圣光射线点名(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        if (Ghost != 1 && isText)accessory.Method.TextInfo("圣光射线点名", duration: 3000, true);
        if (Ghost != 1 && isTTS)accessory.Method.EdgeTTS("圣光射线点名");
        if (Ghost == 1 && isText)accessory.Method.TextInfo("将AOE放置在幽灵下方", duration: 3000, true);
        if (Ghost == 1 && isTTS)accessory.Method.EdgeTTS("将AOE放置在幽灵下方");
        
        await Task.Delay(8000);
        if (isTTS)accessory.Method.EdgeTTS("快躲开");
        
        Ghost = 0;
    }
    
    [ScriptMethod(name: "幽灵连线提示", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0001"])]
    public void 幽灵连线(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("将幽灵引导至光照下", duration: 3000, true);
        if (isTTS)accessory.Method.EdgeTTS("将幽灵引导至光照下");
    }
    
    [ScriptMethod(name: "幽灵高亮", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:8511"])]
    public void 幽灵(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "幽灵";
        dp.Color = new Vector4(0f, 1f, 1f, 6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.86f);
        dp.InnerScale = new Vector2(1.8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 18000;
        if (Ghost == 1)accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "幽灵销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:8511"],userControl: false)]
    public void 幽灵销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("幽灵");
    }
    
    [ScriptMethod(name: "魔界前照灯（直线分摊）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:10989"])]
    public void 魔界前照灯(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "魔界前照灯";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.5f);
        dp.Scale = new (6f, 65.8f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "核爆放置提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1253"])]
    public async void 核爆放置提示(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("将核爆放置在后方", duration: 7500, true);
        if (isTTS)accessory.Method.EdgeTTS("将核爆放置在后方");
        
        await Task.Delay(8000);
        if (isTTS)accessory.Method.EdgeTTS("快躲开");
    }
    
    [ScriptMethod(name: "窒息提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:938"])]
    public void 窒息(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("被幽灵抓", duration: 2000, true);
        if (isTTS)accessory.Method.EdgeTTS("被幽灵抓");
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
