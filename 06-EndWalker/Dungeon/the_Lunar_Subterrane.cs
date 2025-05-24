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

namespace the_Lunar_Subterrane;

[ScriptType(guid: "080d15f8-1663-45c4-8919-73c8dea22617", name: "异界深渊月面地下溪谷", territorys: [1164],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class the_Lunar_Subterrane
{
    const string noteStr =
        """
        v0.0.0.1:
        LV90 异界深渊月面地下溪谷 初版绘制
        目前跟Splatoon的预设内容一致，等有时间抄一下小刺猬的触发器（？
        TTS请在“用户设置”中二选一启用，请勿同时开启
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "小怪_虚月洪巴巴 缺氧吐息（顺劈-Imgui）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35957"])]
    public void 缺氧吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "缺氧吐息";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
    }
    
    #region BOSS1_暗黑精灵
    
    [ScriptMethod(name: "BOSS1_暗黑精灵 十字魔爆", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34783"])]
    public void 十字魔爆(Event @event, ScriptAccessory accessory)
    {
        var currentProperty = accessory.Data.GetDefaultDrawProperties();
        currentProperty.Name = "十字魔爆";
        currentProperty.Owner = @event.SourceId();
        currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(0.4f); 
        currentProperty.Scale = new(8f, 56f); 
        currentProperty.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, currentProperty);  
        
        for(int i=1;i<=2;++i) {
            accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Straight,currentProperty);
            currentProperty.Rotation+=90f.DegToRad();
        }
    }
    
    [ScriptMethod(name: "BOSS1_暗黑精灵 黑暗纹章（地板爆炸）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34787"])]
    public void 黑暗纹章(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "黑暗纹章";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(8f, 8); 
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
    }
    
    
    #endregion
    
    #region BOSS2_达姆希安蚁狮
    
    /*
    [ScriptMethod(name: "BOSS2_达姆希安蚁狮 蚁狮行军（直线冲锋）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35871"])]
    public void 蚁狮行军(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"蚁狮行军{@event.SourceId()}";
        dp.Scale = new (8f, 50f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = 1200;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    */
    
    [ScriptMethod(name: "蚁狮行军销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:34817"],userControl: false)]
    public void 蚁狮行军销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"蚁狮行军{@event.SourceId()}");
    }
    #endregion
    
    #region BOSS3_杜兰特
    
    /*
    [ScriptMethod(name: "BOSS3_杜兰特 双连魔弹", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36058"])]
    public void 双连魔弹(Event @event, ScriptAccessory accessory)
    {
        // 伤害来源 34994 双连魔弹
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "双连魔弹";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.Delay = 3200;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    */
    
    [ScriptMethod(name: "BOSS3_杜兰特 魔力球", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35006"])]
    public void 魔力球(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "魔力球";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(11f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    
    [ScriptMethod(name: "BOSS3_杜兰特 因罪施罚（分裂魔力球）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35005"])]
    public void 因罪施罚(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "因罪施罚";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f);
        dp.DestoryAt = 11700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_杜兰特 重斩（广角顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35009"])]
    public void 重斩(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "重斩";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "BOSS3_杜兰特 暮光相（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36056"])]
    public void 暮光相(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "暮光相";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f); 
        dp.Scale = new(20f, 60); 
        dp.DestoryAt = 7000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
    }
    
    [ScriptMethod(name: "BOSS3_杜兰特 暗黑魔弹（圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35002"])]
    public void 暗黑魔弹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "暗黑魔弹";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_杜兰特 死亡之旅（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34995"])]
    public void 死亡之旅钢铁(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "死亡之旅钢铁";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_杜兰特 死亡之旅（斜线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34996"])]
    public void 死亡之旅斜线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "死亡之旅斜线";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 30f.DegToRad();
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
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