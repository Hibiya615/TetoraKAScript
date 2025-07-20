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

namespace Ala_Mhigo;

[ScriptType(guid: "aa0ae191-da3f-48f1-9340-46a56cfeeabf", name: "鏖战红莲阿拉米格", territorys: [1146],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Ala_Mhigo
{
    const string noteStr =
        """
        v0.0.0.1:
        LV70 鏖战红莲阿拉米格 初版绘制
        TTS请在“用户设置”中二选一启用，请勿同时开启
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    
    [ScriptMethod(name: "BOSS1_守卫机蝎 锁定目标", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:8263"])]
    public void 锁定目标(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("快躲开");
        if (isEdgeTTS)accessory.Method.EdgeTTS("快躲开");
    }
    
    [ScriptMethod(name: "BOSS3_芝诺斯 不动三段（扇形死刑）", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:5372"])]
    public void 不动三段死刑(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("扇形死刑", duration: 2500, true);
        if(isTTS) accessory.Method.TTS("扇形死刑");
        if(isEdgeTTS) accessory.Method.EdgeTTS("扇形死刑");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "不动三段死刑";
        dp.Scale = new Vector2(10f);
        dp.Radian = 120f.DegToRad();
        dp.Owner = @event.SourceId();
        dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.TargetOrderIndex = 1;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);  
    }
    
    [ScriptMethod(name: "BOSS3_芝诺斯 不动三段（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8290"])]
    public void 不动三段扇形(Event @event, ScriptAccessory accessory)
    {
        if(isTTS) accessory.Method.TTS("快躲开");
        if(isEdgeTTS) accessory.Method.EdgeTTS("快躲开");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "不动三段直线";
        dp.Scale = new Vector2(10f);
        dp.Radian = 120f.DegToRad();
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 1.2f);
        dp.DestoryAt = 1800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);  
    }
    
    [ScriptMethod(name: "BOSS3_芝诺斯 雷切一闪（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(8294|9607)$"])]
    public void 雷切一闪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "雷切一闪";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = @event.ActionId() == 8294 ? 5700 : 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_芝诺斯 风断一闪（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(8293|9606)$"])]
    public void 风断一闪(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("向安全方向击退", duration: 5000, true);
        if(isTTS) accessory.Method.TTS("击退");
        if(isEdgeTTS) accessory.Method.EdgeTTS("击退");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "风断一闪";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("风断一闪");
    }
    
    [ScriptMethod(name: "BOSS3_芝诺斯 妖刀一闪（直线分散）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(8296|9608)$"])]
    public void 妖刀一闪(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("分散", duration: 4500, true);
        if(isTTS) accessory.Method.TTS("分散");
        if(isEdgeTTS) accessory.Method.EdgeTTS("分散");
        
        for (var i = 0; i <4;i++)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"妖刀一闪{i}";
            dp.Scale = new(6, 41);
            dp.Owner = @event.SourceId();
            dp.TargetObject = accessory.Data.PartyList[i];
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
    }
    
    [ScriptMethod(name: "BOSS3_芝诺斯 无明闪（连线诱导扇形）", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0029"])]
    public void 无明闪(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText) accessory.Method.TextInfo("诱导扇形至场外", duration: 7400, true);
            if (isTTS) accessory.Method.TTS("诱导扇形至场外");
            if (isEdgeTTS) accessory.Method.EdgeTTS("诱导扇形至场外");
        }

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "无明闪";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(41);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 8100;
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