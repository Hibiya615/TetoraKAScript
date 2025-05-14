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

namespace Sophia;

[ScriptType(guid: "0909b7dc-2079-449a-97ca-f46204180bc0", name: "索菲娅歼灭战", territorys: [576],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Sophia
{
    const string noteStr =
        """
        v0.0.0.1:
        LV60 索菲娅歼灭战 初版绘制
        TTS请在“用户设置”中二选一启用，请勿同时开启
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    
    [ScriptMethod(name: "类星体（核爆）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6792"])]
    public void 类星体_核爆(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "类星体_核爆";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 5000;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    /*
    [ScriptMethod(name: "突袭（俯冲）", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4572","SourceDataId:6026"],suppress:9000)]
    public void 突袭(Event @event, ScriptAccessory accessory)
    {
        // 存在问题：其他时候瞬移时也会触发，如俯冲完恢复位置、转场瞬移至北侧时
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "突袭";
        dp.Scale = new (16f, 55f); // 特效像宽9m，待实证
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    */
    
    [ScriptMethod(name: "信徒其一_反射提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^652[56]$"])]
    public void 反射(Event @event, ScriptAccessory accessory)
    {
        // 招架：StatusID 680
        if (isText)accessory.Method.TextInfo("注意 <信徒其一> 反射", duration: 7000, true);
        if (isTTS)accessory.Method.TTS("停止攻击信徒其一");
        if (isEdgeTTS)accessory.Method.EdgeTTS("停止攻击信徒其一");
    }
    
    [ScriptMethod(name: "信徒其二_灼热射线（背对）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6521"])]
    public void 灼热射线(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("背对信徒其二");
        if (isEdgeTTS)accessory.Method.EdgeTTS("背对信徒其二");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "灼热射线";
        dp.Color = new Vector4(1f, 0f, 1f, 1f).WithW(2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.8f);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "灼热射线销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:6521"],userControl: false)]
    public void 灼热射线销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("灼热射线");
    }
    
    // 转场天平击退 两次均为19m
    // 类星体 差1条击退19m，差2条击退25m
    
    [ScriptMethod(name: "类星体 击退", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6511"])]
    public void 类星体_击退(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("击退", duration: 4200, true);
        if (isTTS)accessory.Method.TTS("击退");
        if (isEdgeTTS)accessory.Method.EdgeTTS("击退");
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