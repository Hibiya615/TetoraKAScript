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

namespace Suzaku;

[ScriptType(guid: "fc907987-bad9-4c70-97f6-b91e2e70544b", name: "朱雀镇魂战", territorys: [810],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Suzaku
{
    const string noteStr =
        """
        v0.0.0.1:
        LV70 朱雀镇魂战 初版绘制
        跳楼机问就是不会，希望有人帮我画了
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    
    uint EternalFlame=1;
    
    public void Init(ScriptAccessory accessory) {
        EternalFlame=1; 
    }
    
    [ScriptMethod(name: "苏生大火 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12834"])]
    public void 苏生大火(Event @event, ScriptAccessory accessory)
    {
        if (EternalFlame != 1) return;
        if (isText)accessory.Method.TextInfo("拉小鸟至中间击杀", duration: 2300, true);
        if (isTTS)accessory.Method.TTS("拉小鸟至中间击杀");
        if (isEdgeTTS)accessory.Method.EdgeTTS("拉小鸟至中间击杀");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "中间圈";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(6f);
        dp.Position = new Vector3(100f, 0f, 100f);
        dp.Scale = new Vector2(6f);
        dp.InnerScale = new Vector2(5.95f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 24000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "苏生之羽 提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:12836"])]
    public void 苏生之羽(Event @event, ScriptAccessory accessory)
    {
        EternalFlame=0; 
        if (isText)accessory.Method.TextInfo("击杀中间羽毛", duration: 2600, false);
        if (isTTS)accessory.Method.TTS("击杀中间羽毛");
        if (isEdgeTTS)accessory.Method.EdgeTTS("击杀中间羽毛");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "中间羽毛";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(8f);
        dp.Position = new Vector3(100f, 0f, 100f);
        dp.Scale = new Vector2(4.5f);
        dp.InnerScale = new Vector2(4.45f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "引诱旋律（吸引）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12847"])]
    public void 引诱旋律(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "引诱旋律";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "拒绝旋律（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12848"])]
    public void 拒绝旋律(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "拒绝旋律";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9.3f);
        dp.InnerScale = new Vector2(4.3f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "拒绝旋律（击退预测）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1284[78]$"])]
    public void 预测(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.DestoryAt = 4000;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Rotation = float.Pi;
        
        if (@event.ActionId == 12848)
        {
            dp.Name = "击退预测";
            dp.Scale = new(1.2f, 8);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = @event.SourceId();
        }
        else
        {
            /*  不知道为什么总是画成击退 作废！
            dp.Name = "吸引预测";
            dp.Scale = new(1.2f, 11);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = @event.SourcePosition;
            */
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw(".*(旋律|预测)");
    }
    
    [ScriptMethod(name: "鬼宿脚（半场顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12851"])]
    public void 鬼宿脚(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "鬼宿脚";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(41);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "跳楼机小鸟正面显示", eventType: EventTypeEnum.Tether, eventCondition: ["Id:004F"])]
    public void 小鸟面向(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "小鸟面向";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 80000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "红莲炎（跳楼机）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12856"])]
    public void 红莲炎(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "红莲炎";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(21);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 900;
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