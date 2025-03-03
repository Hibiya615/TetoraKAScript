using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Objects.Types;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.MathHelpers;
using Lumina.Excel.Sheets;

namespace The_Serpentlord_Seethes;

[ScriptType(guid: "ab67129e-880f-48e8-852e-f92b4afa68e5", name: "蛇王得酷热涅：荒野的死斗", territorys: [1190],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Ttokrrone
{
    const string noteStr =
        """
        v0.0.0.1:
        LV100 特殊Fate 绘制
        蛇王得酷热涅：荒野的死斗
        已知问题：回旋沙暴可能会无法销毁、可能存在方向绘制错误
        """;
    
    public static string GetBNpcName(uint key)
    {
        var sheet = Svc.Data.GetExcelSheet<BNpcName>();
        if (sheet == null) return $"Invalid sheet: ({key})";
        var row = sheet.GetRow(key);

        return row.Singular.ToString() ?? $"Invalid Rowid: ({key})";
    }
    
    [ScriptMethod(name: "迷失连线", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^758[67]$"])]
    public void 迷失连线(Event @event, ScriptAccessory accessory)
    {
        // Data ID 7586: 迷失少女 ；7587：迷失者
        accessory.Method.TextInfo("迷失出现", duration: 5000, true);
        accessory.Method.TTS("迷失出现");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "迷失连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "迷失连线销毁", eventType: EventTypeEnum.Death, userControl: false)]
    public void 迷失连线销毁(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetName() == GetBNpcName(6737) || @event.TargetName() == GetBNpcName(6738))
        {
            accessory.Method.RemoveDraw("迷失连线");
        }
    }
    
    [ScriptMethod(name: "单侧沙暴", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3731[3-6]$"])]
    public void 单侧沙暴(Event @event, ScriptAccessory accessory)
    {
        // 37313 前方沙暴 ； 37314 后方沙暴 ； 37315 右方沙暴 ； 37316 左方沙暴
        // 可达鸭的旋转为逆时针，所以左刀为90°，右刀为270°
        accessory.Method.TextInfo("出目标圈", duration: 4900, true);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "单侧沙暴";
        dp.Color = new Vector4(1f, 1f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 4900;
        
        switch (@event.ActionId())
        {
            case 37313:  // 前方沙暴
                dp.Rotation = 0f.DegToRad();
                break;
            case 37314:  // 后方沙暴
                dp.Rotation = 180f.DegToRad();
                break;
            case 37315:  // 右方沙暴
                dp.Rotation = 270f.DegToRad();
                break;
            case 37316:  // 左方沙暴
                dp.Rotation = 90f.DegToRad();
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "砂砾钢铁";
        dp1.Color = new Vector4(1f, 1f, 0f, 1.2f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(13f);
        dp1.DestoryAt = 4900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
    }

    [ScriptMethod(name: "回旋沙暴", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3731[78]|3732[12])$"])]
    public void 回旋沙暴(Event @event, ScriptAccessory accessory)
    {
        // 【前方回旋沙暴】 37317 顺时针、37321 逆时针 ；【后方回旋沙暴】 37318 顺时针 、37322 逆时针
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60);
        dp.Radian = 90f.DegToRad();
        dp.Color = accessory.Data.DefaultDangerColor;

        switch (@event.ActionId())
        {
            case 37317:  // 顺时针
            case 37321:  // 逆时针
                dp.Name = "前方回旋沙暴F";
                dp.Rotation = 0f.DegToRad(); 
                dp.DestoryAt = 24700;
                break;

            case 37318:  // 顺时针
            case 37322:  // 逆时针
                dp.Name = "后方回旋沙暴B";
                dp.Rotation = 180f.DegToRad();
                dp.DestoryAt = 7800;
                
                dp1.Name = "后方回旋沙暴B2";
                dp1.Rotation = 180f.DegToRad();
                dp1.Delay = 7800;
                dp1.DestoryAt = 16900;
                break;
        }

        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }
    
    [ScriptMethod(name: "回旋沙暴 下一转预测", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3731[78]|3732[12])$"])]
    public void 回旋沙暴预测(Event @event, ScriptAccessory accessory)
    {
        // 【前方回旋沙暴】 37317 顺时针、37321 逆时针 ；【后方回旋沙暴】 37318 顺时针 、37322 逆时针
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60);
        dp.Radian = 90f.DegToRad();
        dp.Color = new Vector4(1f, 0f, 0f, 0.8f);

        switch (@event.ActionId())
        {
            case 37317:  // 前方 顺时针
                dp.Name = "前方回旋沙暴FR";
                dp.Rotation = 270f.DegToRad();
                dp.DestoryAt = 22700;
                break;

            case 37321:  // 前方 逆时针
                dp.Name = "前方回旋沙暴FL";
                dp.Rotation = 90f.DegToRad();
                dp.DestoryAt = 22700;
                break;

            case 37318:  // 后方 顺时针
                dp.Name = "后方回旋沙暴BR1";
                dp.Rotation = 90f.DegToRad();
                dp.DestoryAt = 7800;
                
                dp1.Name = "后方回旋沙暴BR2";
                dp1.Rotation = 270f.DegToRad();
                dp1.Delay = 7800;
                dp1.DestoryAt = 14900;
                break;

            case 37322:  // 后方 逆时针 
                dp.Name = "后方回旋沙暴BL1";
                dp.Rotation = 270f.DegToRad();
                dp.DestoryAt = 7800;
                
                dp1.Name = "后方回旋沙暴BL2";
                dp1.Rotation = 90f.DegToRad();
                dp1.Delay = 7800;
                dp1.DestoryAt = 14900;
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }

    [ScriptMethod(name: "回旋沙暴销毁", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37327|3731[78]|3732[12])$"], userControl: false)]
    public void 回旋沙暴销毁(Event @event, ScriptAccessory accessory)
    {
        // 因每次旋转的持续时间不固定，所以需要根据技能释放方式提前销毁绘制 ，已知回旋完可能接捕食，也可能接一次反向回旋 （待证实：是方向相反 或 顺逆时针相反）
        // 需在 37327 捕食 读条时销毁全部绘制 ， 在 37317 37321 读条前方回旋时 销毁后方绘制 ， 在 31318 37322 读条后方回旋时 销毁前方绘制
        accessory.Method.RemoveDraw(".*");
        
        switch (@event.ActionId())
        {
            case 37327:
                accessory.Method.RemoveDraw(".*");
                break;
            
            case 37317: // 前方 顺时针
            case 37321: // 前方 逆时针
                accessory.Method.RemoveDraw($"后方回旋沙暴\\w*");
                break;
            
            case 37318: // 后方 顺时针
            case 37322: // 后方 逆时针
                accessory.Method.RemoveDraw($"前方回旋沙暴\\w*");
                break;
        }
    }

    
    [ScriptMethod(name: "回旋沙暴_砂砾（持续钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3731[78]|3732[12])$"])]
    public void 砂砾(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("出目标圈", duration: 16000, true);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "砂砾预兆";
        dp.Color = new Vector4(1f, 1f, 0f, 1.4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 6900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "砂砾持续";
        dp1.Color = new Vector4(1f, 0f, 0f, 1.6f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(13f);
        dp1.Delay = 6900;
        dp1.DestoryAt = 9100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
    }
    
    [ScriptMethod(name: "飞沙（钢铁月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3733[1-4]$"])]
    public void 飞沙(Event @event, ScriptAccessory accessory)
    {
        // 37331 钢铁 ； 37332 月环 ； 37333 左半钢铁 + 右半月环 ； 37334 左半月环 + 右半钢铁

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.SourceId();
        dp.DestoryAt = 7000;

        switch (@event.ActionId())
        {
            case 37331: // 钢铁
                dp.Name = "飞沙钢铁";
                dp.Color = new Vector4(1f, 0f, 0f, 1f);
                dp.Scale = new Vector2(19f);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                break;

            case 37332: // 月环
                dp.Name = "飞沙月环";
                dp.Color = new Vector4(0f, 0.6f, 1f, 1f);
                dp.Scale = new Vector2(60f);
                dp.InnerScale = new Vector2(14f);
                dp.Radian = float.Pi * 2;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                break;

            case 37333: // 左半钢铁 + 右半月环
                dp.Name = "飞沙左半钢铁";
                dp.Color = new Vector4(1f, 0f, 0f, 1f);
                dp.Scale = new Vector2(19);
                dp.Radian = 180f.DegToRad();
                dp.Rotation = 90f.DegToRad();
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                var dp1 = accessory.Data.GetDefaultDrawProperties();
                dp1.Name = "飞沙右半月环";
                dp1.Color = new Vector4(0f, 0.6f, 1f, 1f);
                dp1.Owner = @event.SourceId();
                dp1.Scale = new Vector2(60);
                dp1.InnerScale = new Vector2(14);
                dp1.Radian = 180f.DegToRad();
                dp1.Rotation = 270f.DegToRad();
                dp1.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
                break;

            case 37334: // 右半钢铁 + 左半月环
                dp.Name = "飞沙右半钢铁";
                dp.Color = new Vector4(1f, 0f, 0f, 1f);
                dp.Scale = new Vector2(19);
                dp.Radian = 180f.DegToRad();
                dp.Rotation = 270f.DegToRad();
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = "飞沙左半月环";
                dp2.Color = new Vector4(0f, 0.6f, 1f, 1f);
                dp2.Owner = @event.SourceId();
                dp2.Scale = new Vector2(60);
                dp2.InnerScale = new Vector2(14);
                dp2.Radian = 180f.DegToRad();
                dp2.Rotation = 90f.DegToRad();
                dp2.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
                break;
        }
    }   
    
    [ScriptMethod(name: "沙球_大爆炸（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3924[56]$"])]
    public void 大爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "大爆炸";
        dp.Color = new Vector4(1f, 1f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = @event.ActionId() == 39245 ? 7700 : 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "吞地巨蛇（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3757[89]|3758[0-3])$"])]
    public void 吞地巨蛇(Event @event, ScriptAccessory accessory)
    {
        /*
          37578 预警1，13.8s，宽14m，22m长
          37579 预警2，14.8s，宽14m，22m长
          37580 预警3，16.2s，宽14m，22m长
          37581 预警4，17.6s，宽14m，22m长
          37582 预警5，19.0s，宽14m，22m长
          37583 预警6，20.4s，宽14m，22m长

          38642 施放1，14.0s，宽38m，27m长
          38645 施放2，无读条，宽50m，27m长
          38644 施放3，无读条，宽68m，27m长
          38646 施放4，无读条，宽63m，27m长
         */
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "吞地巨蛇";
        dp.Scale = new (27, 68f);
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 0.8f);
        dp.DestoryAt = @event.DurationMilliseconds() + 1400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "蛇王死亡销毁", eventType: EventTypeEnum.Death, userControl: false)]
    public void 蛇王死亡销毁(Event @event, ScriptAccessory accessory)
    {
        string NpcName = GetBNpcName(12733);
        if (@event.TargetName() == NpcName)
        {
            accessory.Method.RemoveDraw(".*");
        }
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
