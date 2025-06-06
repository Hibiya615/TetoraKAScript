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

namespace Zeromus;

[ScriptType(guid: "1d6d7238-e986-408c-9c25-b24955542ee0", name: "泽罗姆斯歼灭战", territorys: [1168],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class Zeromus
{
    const string noteStr =
        """
        v0.0.0.2:
        LV90 泽罗姆斯歼灭战 初版绘制
        TTS请在“用户设置”中二选一启用，请勿同时开启
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    

    [ScriptMethod(name: "深渊回声（地板黑洞钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35578"])]
    public void 深渊回声(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深渊回声";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 15700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "漆黑射线（直线分摊）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:35567"])]
    public void 漆黑射线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "漆黑射线";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.8f);
        dp.Scale = new (12,60f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.DestoryAt = 11700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "暗物质（圆形死刑）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:016C"])]
    public void 暗物质(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "暗物质";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(8f);
        dp.Delay = 4000;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "旋骨利爪", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3558[0134]$"])]
    public void 旋骨利爪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "旋骨利爪";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.DestoryAt = 8500;
        switch (@event.ActionId())
        {
            case 35580:  // 左上 短
                dp.Scale = new (28, 29f);
                break;
            case 35581:  // 右上 长
                dp.Scale = new (28, 60f);
                dp.Offset = new Vector3(-14, 0, 0);
                break;
            case 35583:  // 右上 短
                dp.Scale = new (28, 29f);
                break;
            case 35584:  // 左上 长
                dp.Scale = new (28, 60f);
                dp.Offset = new Vector3(14, 0, 0);
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "日珥焰棘（火球直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35606"])]
    public void 日珥焰棘(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "日珥焰棘";
        dp.Scale = new (10, 60f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "虚空毒菌_有毒气泡", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:16556"])]
    public void 有毒气泡(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"有毒气泡{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(2.25f);
        dp.InnerScale = new Vector2(2.2f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 20000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "有毒气泡销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:35608"],userControl: false)]
    public void 飞散(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"有毒气泡{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "宇宙大爆炸 & 宇宙大挤压（地板黄圈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35589|36144)$"])]
    public void 宇宙大黄圈(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "宇宙大黄圈";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "黑暗侵蚀：重击（分摊）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0064"])]
    public void 黑暗侵蚀重击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "黑暗侵蚀重击";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.8f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "加速度炸弹", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2657"])]
    public async void 加速度炸弹(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        
        uint AccelerationBomb = @event.DurationMilliseconds();
        TimeSpan delay = TimeSpan.FromMilliseconds(Math.Max(0, AccelerationBomb - 2000));
        await Task.Delay(delay);
        
        if (isText) accessory.Method.TextInfo("停止行动", duration: 1500, true);
        if (isTTS) accessory.Method.TTS("停止行动");
        if (isEdgeTTS) accessory.Method.EdgeTTS("停止行动");
    }
    
    [ScriptMethod(name: "黑龙闪 起始位置指路", eventType: EventTypeEnum.StartCasting, eventCondition:["ActionId:36134"])]
    public void 黑龙闪Start(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "黑龙闪Start";
        dp.Scale = new(1);
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = accessory.Data.Me; 
        dp.ScaleMode |= ScaleMode.YByDistance;
        
        if (@event.TargetPosition().X < 91)
        {
            dp.TargetPosition = new Vector3 (85, 0, 85);
        }
        else
        {
            dp.TargetPosition = new Vector3 (115, 0, 85);
        }
        
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "黑洞 最大范围", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36134"])]
    public void 黑洞(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "黑洞";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(35f);
        dp.Delay = 18800;
        dp.DestoryAt = 2800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "陨石冲击 彗星危险范围", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:35601"])]
    public void 彗星(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "彗星";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 12800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深渊激流_次元涌动（场边直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35637"])]
    public void 次元涌动(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "次元涌动";
        dp.Scale = new (14, 60f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "天光轮回预备", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
    public void 天光轮回预备(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "天光轮回预备";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "天光轮回", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35619"])]
    public void 天光轮回(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "天光轮回";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.Delay = 2400;
        dp.DestoryAt = 5600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深渊连爪", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3562[89]|3563[012])$"])]
    public void 深渊连爪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深渊连爪";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60);
        dp.Radian = 40f.DegToRad();
        dp.Delay = @event.ActionId() == 35628 ? @event.DurationMilliseconds() - 1700 : @event.DurationMilliseconds() - 900;
        dp.DestoryAt = @event.ActionId() == 35628 ? 1700 : 600 ;
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