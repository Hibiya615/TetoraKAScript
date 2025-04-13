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
using KodakkuAssist.Extensions;

namespace Bismarck_Extreme;

[ScriptType(guid: "56751be8-113f-4496-8b46-982ff96d90c3", name: "俾斯麦歼殛战", territorys: [447],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Bismarck_Extreme
{
    const string noteStr =
        """
        v0.0.0.1:
        LV60 俾斯麦歼殛战 初版绘制
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    

    uint MagitekFieldGenerator = 1;
    
    public void Init(ScriptAccessory accessory) {
        MagitekFieldGenerator = 1; 
    }
    
    [ScriptMethod(name: "魔导结界 记录", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:4778"])]
    public void 魔导结界(Event @event, ScriptAccessory accessory)
    {
        MagitekFieldGenerator = 0; 
    }
    
    [ScriptMethod(name: "开场提示", eventType: EventTypeEnum.Director, eventCondition: ["Command:40000001"])]
    public async void 开场提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("难度：☆\n小怪打异色蛇，转场注意天气", duration: 5000, true);

        // if (isText)accessory.Method.TextInfo("难度：☆\n小怪打异色蛇，转场注意天气", duration: 5000, true);

        accessory.Method.SendChat("/e ————小抄————\n蛇刷新后：蓝buff打绿怪，绿buff打蓝怪\n雷雨：分散，不打水炮\n小雨：中间钢铁，打水炮\n暴雨：中间击退接月环，打水泡");
        
    }
    
    [ScriptMethod(name: "拉怪提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3827"])]
    public void 拉怪提示(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (!isTank) return; 
        if (isText)accessory.Method.TextInfo("将其他小怪拉至远程怪 <温杜乌麻义> 处", duration: 5000, false);
        if (isTTS)accessory.Method.TTS("将其他小怪拉到远程怪");
        if (isEdgeTTS)accessory.Method.EdgeTTS("将其他小怪拉到远程怪");
        
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "温杜乌麻义";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new Vector2(1f);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = @event.SourceId();
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 5700;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

    }
    
    [ScriptMethod(name: "白鲸之怒 首次提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4010"])]
    public void 白鲸之怒(Event @event, ScriptAccessory accessory)
    {
        if (MagitekFieldGenerator == 0) return;
        if (isText)accessory.Method.TextInfo("使用 <魔导结界发生器>", duration: 5000, true);
        if (isTTS)accessory.Method.TTS("使用魔导结界发生器");
        if (isEdgeTTS)accessory.Method.EdgeTTS("使用魔导结界发生器");
    }
    
    [ScriptMethod(name: "上背提示", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4355", "SourceDataId:3820"])]
    public void 上背提示(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("上背");
        if (isEdgeTTS)accessory.Method.EdgeTTS("上背");
    }
    
    [ScriptMethod(name: "鲸须爆弹（诱导水圈提示）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:4021"])]
    public void 鲸须爆弹(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("诱导三连水圈", duration: 3000, false);
        if (isTTS)accessory.Method.TTS("诱导三连水圈");
        if (isEdgeTTS)accessory.Method.EdgeTTS("诱导三连水圈");
    }
    
    [ScriptMethod(name: "漏斗云（龙卷风）高亮", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3830"])]
    public void 漏斗云(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "漏斗云";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(4.5f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "漏斗云描边";
        dp2.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp2.Owner = @event.SourceId();
        dp2.Scale = new Vector2(4.6f);
        dp2.InnerScale = new Vector2(4.5f);
        dp2.Radian = float.Pi * 2;
        dp2.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
    }
    
    [ScriptMethod(name: "漏斗云销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:3830"],userControl: false)]
    public void 漏斗云销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("漏斗云.*");
    }
    
    [ScriptMethod(name: "死水/死风 读条提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4032"])]
    public void 死风水(Event @event, ScriptAccessory accessory)
    {
        // ActionId:4032 死风; 4034 死水
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank)
        {
            if (isText)accessory.Method.TextInfo("赋予buff，稍后挑衅反色目标", duration: 2000, false);
            if (isTTS)accessory.Method.TTS("赋予buff，稍后挑衅反色目标");
            if (isEdgeTTS)accessory.Method.EdgeTTS("赋予buff，稍后挑衅反色目标");
        }
        else
        {
            if (isText)accessory.Method.TextInfo("赋予buff，稍后攻击反色目标", duration: 2000, false);
            if (isTTS)accessory.Method.TTS("赋予buff，稍后攻击反色目标");
            if (isEdgeTTS)accessory.Method.EdgeTTS("赋予buff，稍后攻击反色目标");
        }

    }
    
    [ScriptMethod(name: "支配于风/水 buff提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^71[78]$"])]
    public void 支配于风水(Event @event, ScriptAccessory accessory)
    {
        // StatusID: 717 支配于风 ；718 支配于水
        if (@event.TargetId() != accessory.Data.Me) return; 
        var elemental = @event.StatusId == 717 ? "蓝色水蛇" : "绿色风蛇";
        if (isText)accessory.Method.TextInfo($"攻击{elemental}", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"攻击{elemental}");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"攻击{elemental}");
    }
    
    [ScriptMethod(name: "呼风唤雨 提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:4021"])]
    public void 呼风唤雨(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("雷雨：不打水泡+分散\n暴雨：打水泡，中间击退接月环\n小雨：打水泡，远离中间", duration: 10000, true);
        if (isTTS)accessory.Method.TTS("注意天气变化");
        if (isEdgeTTS)accessory.Method.EdgeTTS("注意天气变化");
        accessory.Method.SendChat("/e ————呼风唤雨————\n雷雨：分散，不打水炮\n小雨：中间钢铁，打水炮\n暴雨：中间击退接月环，打水泡");
    }
    
    [ScriptMethod(name: "雷雨_落雷&雷暴云砧 分散提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:4021"])]
    public void 雷雨(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("分散，不打水泡", duration: 3000, true);
        if (isTTS)accessory.Method.TTS("分散，不打水泡");
        if (isEdgeTTS)accessory.Method.EdgeTTS("分散，不打水泡");
    }
    
    [ScriptMethod(name: "小雨_荒蛮之泪（中间钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4014"])]
    public void 荒蛮之泪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "荒蛮之泪";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Offset = new Vector3 (-15, 0, 0);
        dp.Scale = new Vector2(8);
        dp.DestoryAt = 6100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "暴雨_锋利之风（中间击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4015"])]
    public void 锋利之风(Event @event, ScriptAccessory accessory)
    {
        // 该击退为 随机方向
        if (isText)accessory.Method.TextInfo("中间击退", duration: 5400, true);
        if (isTTS)accessory.Method.TTS("中间击退");
        if (isEdgeTTS)accessory.Method.EdgeTTS("中间击退");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "击退连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = @event.TargetPosition + new Vector3 (-15, 0,0);
        dp.Scale = new(1);
        dp.DestoryAt = 6100;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("锋利之风");
    }
    
    [ScriptMethod(name: "暴雨_暴风骤雨（中间月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4012"])]
    public void 暴风骤雨(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "暴风骤雨";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.InnerScale = new Vector2(5f);
        dp.Offset = new Vector3 (-15, 0, 0);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
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