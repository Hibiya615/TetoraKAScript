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
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.MathHelpers;
using System.Threading.Tasks;

namespace theTwinning;

[ScriptType(guid: "bfb00cd3-ccec-4b21-b3d7-e290f49e6a75", name: "异界遗构希尔科斯孪晶塔", territorys: [840],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class theTwinning
{
    const string noteStr =
        """
        v0.0.0.2:
        LV80 异界遗构希尔科斯孪晶塔 初版绘制
        """;
    
    #region 基础控制
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "伤头&插言 打断销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void 打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"小怪.*百廿八吨回转{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "下踢&扫腿&神圣 眩晕销毁", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2"], userControl: false)]
    public void 眩晕销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"小怪.*百廿八吨回转{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "取消读条销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15802"], userControl: false)]
    public void 取消读条销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"小怪.*{@event.SourceId()}");
    }

    
    #endregion
    
    #region 小怪部分
    
    [ScriptMethod(name: "小怪_自控化弥诺陶洛斯 百廿八吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15802"])]
    public void 百廿八吨回转(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("打断或踢晕 <自控化弥诺陶洛斯>", duration: 4000, false);
        // if (isTTS)accessory.Method.TTS("打断或踢晕 <自控化弥诺陶洛斯>");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("打断或踢晕 <自控化弥诺陶洛斯>");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"小怪_百廿八吨回转{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "小怪_量产型卡利亚 主首（死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15804"])]
    public void 主首(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("<主首>  坦克死刑", duration: 2000, false);
        if (isTTS)accessory.Method.TTS("坦克死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS("坦克死刑");
    }
    
    [ScriptMethod(name: "小怪_活力化爬虫半人马 狂暴 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15808"],suppress:30000)]
    public void 狂暴(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("打断或踢晕 <活力化爬虫半人马>", duration: 4300, false);
        // if (isTTS)accessory.Method.TTS("打断或踢晕 <活力化爬虫半人马>");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("打断或踢晕 <活力化爬虫半人马>");
    }
    
    #endregion
    
    #region BOSS部分
    
    [ScriptMethod(name: "BOSS1_扎戈斧龙一型 预兆（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15717"])]
    public void 预兆(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank && isText)accessory.Method.TextInfo("躲避顺劈", duration: 3300, true);
        if (isTank && isTTS)accessory.Method.TTS("躲避顺劈");
        if (isTank && isEdgeTTS)accessory.Method.EdgeTTS("躲避顺劈");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "预兆";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "BOSS1_扎戈斧龙一型 绝望冲击（直线分散）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^003[2-5]$"])]
    public void 绝望冲击(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("分散，避开笼子", duration: 7100, true);
        // if (isTTS)accessory.Method.TTS("分散，避开笼子");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("分散，避开笼子");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        var boss = accessory.Data.Objects.GetByDataId(10193).FirstOrDefault();
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;
        dp.Name = "绝望冲击";
        dp.Scale = new (6, 50f);
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        
        switch (@event["Id"])
        {
            case "0032":
                dp.DestoryAt = 7200;
                break;
            case "0033":
                dp.DestoryAt = 7400;
                break;
            case "0034":
                dp.DestoryAt = 7600;
                break;
            case "0035":
                dp.DestoryAt = 7800;
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS1_扎戈斧龙一型 歼灭弹（火圈分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15715"])]
    public void 歼灭弹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "歼灭弹";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.4f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("分摊并放置火圈", duration: 4300, true);
        if (isTTS)accessory.Method.TTS("分摊并放置火圈");
        if (isEdgeTTS)accessory.Method.EdgeTTS("分摊并放置火圈");
    }
    
    //  压了一个 XXX突袭 的点名 [https://ff14.huijiwiki.com/wiki/%E5%BC%82%E7%95%8C%E9%81%97%E6%9E%84%E5%B8%8C%E5%B0%94%E7%A7%91%E6%96%AF%E5%AD%AA%E6%99%B6%E5%A1%94] 
    
    [ScriptMethod(name: "BOSS2_米特里达梯 雷弹（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:10244"])]
    public void 雷弹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "雷弹";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "雷弹 销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:15857"], userControl: false)]
    public void 雷弹销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("雷弹");
    }
    
    [ScriptMethod(name: "BOSS3_泰空 魔导交叉激光（直线）", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2010169", "Operate:Add"])]
        // 备用 EventTypeEnum.ObjectEffect, eventCondition: ["Id1:64", "Id2:128"])]
    public void 魔导激光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "魔导激光";
        dp.Scale = new (8, 50f);
        dp.Offset = new Vector3 (0, 0, 10);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "魔导激光 销毁", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:4", "Id2:512"], userControl: false)]
    public void 魔导激光销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("魔导激光");
    }
    
    [ScriptMethod(name: "魔导激光 销毁2", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:15859"], userControl: false)]
    public void 魔导激光销毁2(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("魔导激光");
    }
    
    [ScriptMethod(name: "BOSS3_泰空 人造重力（扩大黑圈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15866"])]
    public void 人造重力(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "人造重力";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion

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