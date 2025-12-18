using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
using System.Threading.Tasks;

namespace TetoraKodakkuScript._00_Other;

[ScriptType(guid: "f85df3b8-baa4-42b7-8196-bc26c58fa251", name: "技能范围", territorys: [],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class ActionArea
{
    const string noteStr =
        """
        v0.0.0.1:
        【测试版】懒得写所以暂时不会更新，凑合用先
        技能范围绘制，全场景可用
        请先自行设置合适亮度并用任意技能测试
        """;
    
    #region 用户控制
    
    [UserSetting("位移技能预测显示距离圆")]
    public bool IsMoveActionsCircle { get; set; } = true;
    
    [UserSetting("位移技能预测显示时间(ms)")]
    public int MoveActionsTime { get; set; } = 10000;
    
    [UserSetting("位移技能预测颜色")]
    public ScriptColor MoveActionsColor { get; set; } = new() { V4 = new(0f, 1f, 1f, 1f) };
    
    [UserSetting("常驻AOE技能绘制一键开关")]
    public bool IsPersistentAoEs { get; set; } = true;
    
    [UserSetting("常驻AOE技能颜色")]
    public ScriptColor PersistentAoEsColor { get; set; } = new() { V4 = new(0f, 1f, 1f, 2f) };
    
    [UserSetting("常驻AOE描边亮度（推荐10以上）")]
    public float PersistentOutlineBrightness { get; set; } = 15;
    
    [UserSetting("常驻AOE填充亮度（推荐小于1）")]
    public float PersistentFillBrightness { get; set; } = 0.2f;
    
    [UserSetting("其它AOE技能颜色")]
    public ScriptColor ActionAoEsColor { get; set; } = new() { V4 = new(0f, 1f, 0f, 1f) };
    
    [UserSetting("其它AOE描边亮度（推荐10以上）")]
    public float ActionOutlineBrightness { get; set; } = 15;
    
    [UserSetting("其它AOE填充亮度（推荐小于1）")]
    public float ActionFillBrightness { get; set; } = 0.2f;
    
    [UserSetting("选择位移预测或范围描边绘制类型")]
    public BlendModeEnum BlendMode { get; set; } = BlendModeEnum.Default;
    
    private static List<string> _blendMode = ["Imgui", "VFX"];
    
    public enum BlendModeEnum
    {
        Default = 0,
        Imgui = 1,
        VFX = 2,
    }
    
    #endregion

    [ScriptMethod(name: "位移技能预测圆", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^1[05]m[FB]?$"])]
    public void 位移技能预测圆 (Event @event, ScriptAccessory accessory)
    {
        if (!IsMoveActionsCircle) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        /*
        switch (@event.)
        {
            case "10m":
                dp.Name = "10m圆";
                dp.Scale = new Vector2(10f);
                dp.InnerScale = new Vector2(9.95f);
                break;
            case "15m":
                dp.Name = "15m圆";
                dp.Scale = new Vector2(15f);
                dp.InnerScale = new Vector2(14.95f);
                break;
        }
        */

        dp.Name = "15m圆";
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(14.95f);
        dp.Owner = accessory.Data.Me; 
        dp.Color = MoveActionsColor.V4.WithW(10f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = MoveActionsTime;
        accessory.Method.SendDraw((DrawModeEnum)BlendMode, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "位移技能预测线 - 前", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^1[05]mF?$"])]
    public void 位移技能预测线前 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "15m前";
        dp.Scale = new(0.5f, 15f);
        dp.Owner = accessory.Data.Me; 
        dp.Color = MoveActionsColor.V4.WithW(2f);
        dp.DestoryAt = MoveActionsTime;
        dp.Rotation = float.Pi * 2;
        accessory.Method.SendDraw((DrawModeEnum)BlendMode, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "位移技能预测线 - 后", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^1[05]mB?$"])]
    public void 位移技能预测线后 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "15m后";
        dp.Scale = new(0.5f, 15f);
        dp.Owner = accessory.Data.Me; 
        dp.Color = MoveActionsColor.V4.WithW(2f);
        dp.DestoryAt = MoveActionsTime;
        dp.Rotation = float.Pi;
        accessory.Method.SendDraw((DrawModeEnum)BlendMode, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "位移技能预测销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(94|2440[12])$"],userControl: false)]
    public void 位移技能预测销毁 (Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return; 
        switch (@event.ActionId())
        {
            case 94:  // 龙骑 回避跳跃
                accessory.Method.RemoveDraw($"15m.*");
                break;
            case 16010:  // 舞者 前冲步
                accessory.Method.RemoveDraw($"10m.*");
                break;
            case 24401:  // 镰刀 地狱如境
                accessory.Method.RemoveDraw($"15m.*");
                break;
            case 24402:  // 镰刀 地狱出境
                accessory.Method.RemoveDraw($"15m.*");
                break;
            case 34684:  // 画家 速涂
                accessory.Method.RemoveDraw($"15m.*");
                break;
            case 37008:  // 白魔 以太变移
                accessory.Method.RemoveDraw($"15m.*");
                break;
            // 诗人 后跃射击
            // 盘子 必杀技夜天
            // 赤魔 移转
        }

    }
    
    [ScriptMethod(name: "[描边] 破阵法", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:17215"])]
    public void 破阵法描边 (Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "破阵法描边";
        dp.Color = PersistentAoEsColor.V4.WithW(PersistentOutlineBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.InnerScale = new Vector2(4.98f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 720000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[填充] 破阵法", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:17215"])]
    public void 破阵法填充 (Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "破阵法填充";
        dp.Color = PersistentAoEsColor.V4.WithW(PersistentFillBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 720000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "小仙女消失自动销毁破阵法绘制", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:1008"])]
    public void 小仙女消失自动销毁破阵法 (Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"破阵法.*");
    }
    
    [ScriptMethod(name: "常驻技能销毁", eventType: EventTypeEnum.CombatChanged, eventCondition: ["InCombat:False"],userControl: false)]
    public void 常驻技能销毁 (Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"破阵法.*");
    }
    
    [ScriptMethod(name: "[描边] 标准舞步", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1818"])]
    public void 标准舞步描边 (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "标准舞步描边";
        dp.Color = ActionAoEsColor.V4.WithW(ActionOutlineBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(14.96f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 15000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[填充] 标准舞步", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1818"])]
    public void 标准舞步填充 (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "标准舞步填充";
        dp.Color = ActionAoEsColor.V4.WithW(ActionFillBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 15000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[描边] 结束动作", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3868"])]
    public void 结束动作描边 (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "结束动作描边";
        dp.Color = ActionAoEsColor.V4.WithW(ActionOutlineBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(14.96f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[填充] 结束动作", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3868"])]
    public void 结束动作填充 (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "结束动作填充";
        dp.Color = ActionAoEsColor.V4.WithW(ActionFillBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[描边] 技巧舞步", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1819"])]
    public void 技巧舞步描边 (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "技巧舞步描边";
        dp.Color = ActionAoEsColor.V4.WithW(ActionOutlineBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(14.96f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 15000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[填充] 技巧舞步", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1819"])]
    public void 技巧舞步填充 (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "技巧舞步填充";
        dp.Color = ActionAoEsColor.V4.WithW(ActionFillBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 15000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[描边] 提拉纳", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2698"])]
    public void 提拉纳描边 (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "提拉纳描边";
        dp.Color = ActionAoEsColor.V4.WithW(ActionOutlineBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(14.96f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[填充] 提拉纳", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2698"])]
    public void 提拉纳填充 (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "提拉纳填充";
        dp.Color = ActionAoEsColor.V4.WithW(ActionFillBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "舞者技能销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^(181[89]|2698|3868)$"],userControl: false)]
    public void 舞者技能销毁 (Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return; 
        switch (@event.StatusID())
        {
            case 1818:
                accessory.Method.RemoveDraw($"标准舞步.*");
                break;
            case 1819: 
                accessory.Method.RemoveDraw($"技巧舞步.*");
                break;
            case 2698: 
                accessory.Method.RemoveDraw($"提拉纳.*");
                break;
            case 3868: 
                accessory.Method.RemoveDraw($"结束动作.*");
                break;
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

    public static uint StatusID(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StatusID"]);
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
public static class MathHelpers
{
    public static float DegToRad(float degrees)
    {
        return degrees * (float)(Math.PI / 180.0);
    }
    
    public static double DegToRad(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    
    public static float RadToDeg(float radians)
    {
        return radians * (float)(180.0 / Math.PI);
    }
    
    public static double RadToDeg(double radians)
    {
        return radians * 180.0 / Math.PI;
    }
}