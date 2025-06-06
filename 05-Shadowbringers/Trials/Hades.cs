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

namespace Hades;

[ScriptType(guid: "ebcac22b-8a1d-49c4-ae1f-1470be15f7e3", name: "哈迪斯歼灭战", territorys: [847],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Hades
{
    const string noteStr =
        """
        v0.0.0.1:
        LV80 哈迪斯歼灭战 初版绘制
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("DR 自动QTE")]
    public bool isDRHelper { get; set; } = true;
    
    [UserSetting("DR 我是绿玩（自动关闭Hack）")]
    public bool isDRHackOff { get; set; } = true;
    
    [ScriptMethod(name: "失信（对角矩形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1671[56]$"])]
    public void 失信(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "失信";
        dp.Scale = new (20, 20f);
        if (@event.ActionId == 16715) dp.Rotation = 90f.DegToRad();
        dp.Offset = new Vector3 (20 ,0 ,10);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "背信（大圈）", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id2:2", "Id1:1"])]
    public void 背信(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "背信";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.SourcePosition();
        dp.Scale = new Vector2(10f);
        dp.Delay = 4000;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "背信备用销毁", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id2:8", "Id1:4"],userControl: false)]
    public void 背信销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("背信");
    }
    
    uint AncientDarkness=0;
    
    public void Init(ScriptAccessory accessory) {
        AncientDarkness=0; 
    }
    
    [ScriptMethod(name: "古代黑暗 分散点名记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17811"],userControl: false)]
    public void 古代黑暗(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me) AncientDarkness=1; 
    }
    
    [ScriptMethod(name: "古代黑暗 分散点名重置", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:17811]"],userControl: false)]
    public void 古代黑暗重置(Event @event, ScriptAccessory accessory)
    {
        AncientDarkness=0; 
    }

    [ScriptMethod(name: "古代狂水（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17812"])]
    public async void 古代狂水(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(300);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "古代狂水";

        if (AncientDarkness == 1)
        {
            if (isText) accessory.Method.TextInfo("散开", duration: 4000, true);
            // if (isTTS)accessory.Method.TTS("散开");
            // if (isEdgeTTS)accessory.Method.EdgeTTS("散开");
            dp.Color = accessory.Data.DefaultDangerColor;
        }
        else
        {
            if (isText) accessory.Method.TextInfo("分摊", duration: 4000, false);
            // if (isTTS)accessory.Method.TTS("分摊");
            // if (isEdgeTTS)accessory.Method.EdgeTTS("分摊");
            dp.Color = accessory.Data.DefaultSafeColor;
        }

        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "暗影流（中间）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:16732"])]
    public void 暗影流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "暗影流";
        dp.Scale = new (16, 100f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "冥王净化（两侧）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1675[34]$"])]
    public void 冥王净化(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冥王净化";
        dp.Scale = new (16, 100f);
        dp.Offset = @event.ActionId == 16754 ? new Vector3 (12 ,0 ,0) : new Vector3 (-12 ,0 ,0);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "逝者的回声", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^167(39|40)$"])]
    public void 逝者的回声(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "逝者的回声";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(100);
        dp.Radian = 180f.DegToRad();
        dp.Offset = new Vector3 (0,0,12);
        dp.Rotation = @event.ActionId == 16739 ? 75f.DegToRad() : 285f.DegToRad();
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "地狱之声（大地摇动）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0028"])]
    public void 地狱之声(Event @event, ScriptAccessory accessory)
    {
        if (isText && @event.TargetId() == accessory.Data.Me) accessory.Method.TextInfo("诱导大地摇动", duration: 4000, false);
        // if (isTTS && @event.TargetId() == accessory.Data.Me)accessory.Method.TTS("诱导大地摇动");
        // if (isEdgeTTS && @event.TargetId() == accessory.Data.Me)accessory.Method.EdgeTTS("诱导大地摇动");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        var boss = accessory.Data.Objects.GetByDataId(10570).FirstOrDefault();
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;
        dp.TargetObject = @event.TargetId();
        
        dp.Name = "地狱之声";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.Scale = new Vector2(100);
        dp.Radian = 60f.DegToRad();
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "DR - 自动QTE", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:16758"])]
    public void 自动QTE(Event @event, ScriptAccessory accessory)
    {
        if (isDRHelper) accessory.Method.SendChat("/pdr load AutoQTE");
    }

    [ScriptMethod(name: "DR - 自动关闭免疫输入禁用", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:16758"])]
    public void 自动关闭Hack(Event @event, ScriptAccessory accessory)
    {
        if (isDRHackOff) accessory.Method.SendChat("/pdr unload AutoImmunizeInputDisable");
    }
    
    [ScriptMethod(name: "死亡宣告 踩光圈提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:210"])]
    public void 死亡宣告(Event @event, ScriptAccessory accessory)
    {
        if (isText && @event.TargetId() == accessory.Data.Me) accessory.Method.TextInfo("踩满全部光圈", duration: 3000, true);
        // if (isTTS && @event.TargetId() == accessory.Data.Me)accessory.Method.TTS("踩满光圈");
        // if (isEdgeTTS && @event.TargetId() == accessory.Data.Me)accessory.Method.EdgeTTS("踩满光圈");
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