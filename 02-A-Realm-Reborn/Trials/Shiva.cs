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

namespace Shiva;

[ScriptType(guid: "cddb93a8-e01e-40fe-989c-5153ad62bfbc", name: "希瓦歼灭战", territorys: [377],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Shiva
{
    const string noteStr =
        """
        v0.0.0.1:
        LV50 希瓦歼灭战 初版绘制
        """;
    
    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;

    
    uint FrostStaff = 0; // 冰霜之杖
    uint FrostBlade = 0; // 冰霜之剑
    uint Melt = 0; // 武器融化
    
    public void Init(ScriptAccessory accessory) {
        FrostStaff = 0; 
        FrostBlade = 0;
        Melt = 0;
    }
    
    [ScriptMethod(name: "冰霜之杖 变身提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2453"])]
    public void 冰霜之杖(Event @event, ScriptAccessory accessory)
    {
        FrostStaff = 1; 
        FrostBlade = 0;
        Melt = 0;
        
        if (isText)accessory.Method.TextInfo("分散", duration: 2000, false);
        if (isTTS)accessory.Method.TTS("分散");
        if (isEdgeTTS)accessory.Method.EdgeTTS("分散");
    }
    
    [ScriptMethod(name: "冰雹（分散）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:001D"])]
    public void 冰雹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冰雹";
        dp.Color = new Vector4(1f, 0.4f, 0f, 1.6f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6.4f);
        dp.DestoryAt = 3400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "冰柱冲击（圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2462"])]
    public void 冰柱冲击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冰柱冲击";
        dp.Color = new Vector4(0f, 1f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Delay = 2000;
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "冰印剑（顺劈死刑）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2451"])]
    public void 冰印剑(Event @event, ScriptAccessory accessory)
    {
        FrostBlade = 1;
        FrostStaff = 0; 
        Melt = 0;
        
        var boss = accessory.Data.Objects.GetByDataId(3100).FirstOrDefault();
        if (boss == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冰印剑";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = boss.GameObjectId;
        dp.TargetObject = @event.TargetId();
        dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.TargetOrderIndex = 1;
        dp.Scale = new Vector2(14);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    /*
    [ScriptMethod(name: "天降一击（小击退）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2454"])]
    public void 天降一击(Event @event, ScriptAccessory accessory)
    {
        // 注：在 “冰河怒击（顺劈）” 后平A几次也会发生，且在P3时 时间轴不一定固定（基本都压了）
        if (FrostBlade == 1)
        {
            if (isText)accessory.Method.TextInfo("小击退", duration: 5600, false);
            if (isTTS)accessory.Method.TTS("小击退");
            if (isEdgeTTS)accessory.Method.EdgeTTS("小击退");
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "天降一击";
            dp.Scale = new(1f, 5);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = @event.SourceId();
            dp.Rotation = float.Pi;
            dp.DestoryAt = 6300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
    }
    */
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("天降一击");
    }
    
    [ScriptMethod(name: "冰河怒击（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2465"])]
    public void 冰河怒击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冰河怒击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "钻石星尘（冻结）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2466"])]
    public void 钻石星尘(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("AOE", duration: 8700, false);
        if (isTTS)accessory.Method.TTS("AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "永久冻土 结冰提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2457"])]
    public void 强化寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("地面结冰", duration: 1200, true);
        if (isTTS)accessory.Method.TTS("停止移动");
        if (isEdgeTTS)accessory.Method.EdgeTTS("停止移动");
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