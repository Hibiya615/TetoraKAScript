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
using KodakkuAssist.Extensions;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace TetoraKodakkuScript._00_Other;

[ScriptType(guid: "f85df3b8-baa4-42b7-8196-bc26c58fa251", name: "技能范围", territorys: [],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class ActionArea
{
    const string noteStr =
        """
        v0.0.0.2: 技能范围绘制，全场景可用
        【先行版】主要用于位移技能预测，其它技能范围再说，短时间内不会更新
        （除了位移技能外不含PVP，PVP技能绘制的在另一个脚本里）
        请先自行设置合适亮度并用任意技能测试
        位移技能预测用宏触发，用户设置中开启打印宏后点击左侧小飞机图案可打印宏至聊天栏默语  
        """;
    
    #region 用户控制
    
    [UserSetting("打印宏（保存宏后关闭即可）")]
    public bool IsPrintingMacros { get; set; } = false;
    
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
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion
    public void 龙骑(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "位移技能预测圆", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^(Front|Back)?DisplacementPrediction$"])]
    public void 位移技能预测圆 (Event @event, ScriptAccessory accessory)
    {
        // if (IsPrintingMacros) accessory.Method.SendChat($"/e [打印宏]: /e DisplacementPrediction");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Color = MoveActionsColor.V4.WithW(10f);
        dp.Owner = accessory.Data.Me; 
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = MoveActionsTime;
        
        var myJob = IbcHelper.GetPlayerJob(accessory, accessory.Data.MyObject, false);
        if (myJob == "DRG" || myJob == "RPR" || myJob == "RDM" || myJob == "PCT" || myJob == "WTM" || myJob == "AST")
            // 龙骑: 回避跳跃 ; 镰刀: 地狱入/出境 ; 赤魔: 移转 ; 画家: 速涂 ; 白魔: 以太变移 ; 占星: 本轮 (PVP)
        {
            dp.Name = "位移预测-15m圆";
            dp.Scale = new Vector2(15f);
            dp.InnerScale = new Vector2(14.95f);
            accessory.Method.SendDraw((DrawModeEnum)BlendMode, DrawTypeEnum.Donut, dp);
        }
        else if (myJob == "SAM" || myJob == "DNC" || myJob == "BRD")
            // 武士: 必杀剑·夜天 ; 舞者: 前冲步 ; 诗人: 后跃射击
        {
            dp.Name = "位移预测-10m圆";
            dp.Scale = new Vector2(10f);
            dp.InnerScale = new Vector2(9.95f);
            accessory.Method.SendDraw((DrawModeEnum)BlendMode, DrawTypeEnum.Donut, dp);
        }
        else
        {
            return;
        }
        
    }
    
    [ScriptMethod(name: "位移技能预测线 - 前 [ 触发宏: /e FrontDisplacementPrediction ]", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^FrontDisplacementPrediction$"])]
    public void 位移技能预测线前 (Event @event, ScriptAccessory accessory)
    {
        if (IsPrintingMacros) accessory.Method.SendChat($"/e [打印宏]: /e FrontDisplacementPrediction");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Color = MoveActionsColor.V4.WithW(2f);
        dp.Owner = accessory.Data.Me; 
        dp.Rotation = float.Pi * 2;
        dp.DestoryAt = MoveActionsTime;
        
        var myJob = IbcHelper.GetPlayerJob(accessory, accessory.Data.MyObject, false);
        if (myJob == "RPR" || myJob == "PCT" || myJob == "WTM" || myJob == "AST")
            // 镰刀: 地狱入境 ; 画家: 速涂 ; 白魔: 以太变移 ; 占星: 本轮 (PVP)
        {
            dp.Name = "位移预测-15m前";
            dp.Scale = new(0.5f, 15f);
            accessory.Method.SendDraw((DrawModeEnum)BlendMode, DrawTypeEnum.Displacement, dp);
        }
        else if (myJob == "DNC")
            // 舞者: 前冲步
        {
            dp.Name = "位移预测-10m前";
            dp.Scale = new(0.5f, 10f);
            accessory.Method.SendDraw((DrawModeEnum)BlendMode, DrawTypeEnum.Displacement, dp);
        }
        else
        {
            return;
        }

    }
    
    [ScriptMethod(name: "位移技能预测线 - 后 [ 触发宏: /e BackDisplacementPrediction ]", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^BackDisplacementPrediction$"])]
    public void 位移技能预测线后 (Event @event, ScriptAccessory accessory)
    {
        if (IsPrintingMacros) accessory.Method.SendChat($"/e [打印宏]: /e BackDisplacementPrediction");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Color = MoveActionsColor.V4.WithW(2f);
        dp.Owner = accessory.Data.Me; 
        dp.Rotation = float.Pi;
        dp.DestoryAt = MoveActionsTime;
        
        var myJob = IbcHelper.GetPlayerJob(accessory, accessory.Data.MyObject, false);
        if (myJob == "DRG" || myJob == "RPR" || myJob == "RDM")
            // 龙骑: 回避跳跃 ; 镰刀: 地狱出境 ; 赤魔: 移转
        {
            dp.Name = "位移预测-15m后";
            dp.Scale = new(0.5f, 15f);
            accessory.Method.SendDraw((DrawModeEnum)BlendMode, DrawTypeEnum.Displacement, dp);
        }
        else if (myJob == "BRD")
            // 武士: 必杀剑·夜天 ; 诗人: 后跃射击
        {
            dp.Name = "位移预测-10m后";
            dp.Scale = new(0.5f, 10f);
            accessory.Method.SendDraw((DrawModeEnum)BlendMode, DrawTypeEnum.Displacement, dp);
        }
        else
        {
            return;
        }
    }
    
    [ScriptMethod(name: "位移技能预测销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: 
        ["ActionId:regex:^(94|112|7493|7515|16010|2440[12]|29399|29430|29494|29550|29700|34684|37008|39210|41506)$"],userControl: false)]
    public void 位移技能预测销毁 (Event @event, ScriptAccessory accessory)
    {
        // PVE ActionId: 94 - 龙骑 回避跳跃 ; 24401 - 镰刀 地狱入境 ; 24402 - 镰刀 地狱出境 ; 34684 - 画家 速涂 ; 37008 - 白魔 以太变移 ; 16010 - 舞者 前冲步 ;
        //               112 - 诗人 后跃射击 ; 7493 - 盘子 必杀技·夜天 ; 7515 - 赤魔 移转
        // PVP ActionId: 29494 - 龙骑 回避跳跃 ; 29550 - 镰刀 地狱入境 ; 39210 - 画家 速涂 ; 29430 - 舞者 前冲步 ; 41506 - 占星 本轮
        //               29399 - 诗人 后跃射击 ; 29700 - 赤魔 移转
        
        if (@event.SourceId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($"位移预测.*");
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

#region EventExtensions

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

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;
        return attribute?.Description ?? value.ToString();
    }
}

public static class MathTools
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;

    /// <summary>
    /// 获得任意点与中心点的弧度值，以(0, 0, 1)方向为0，以(1, 0, 0)方向为pi/2。
    /// 即，逆时针方向增加。
    /// </summary>
    /// <param name="point">任意点</param>
    /// <param name="center">中心点</param>
    /// <returns></returns>
    public static float GetRadian(this Vector3 point, Vector3 center)
        => MathF.Atan2(point.X - center.X, point.Z - center.Z);

    /// <summary>
    /// 获得任意点与中心点的长度。
    /// </summary>
    /// <param name="point">任意点</param>
    /// <param name="center">中心点</param>
    /// <returns></returns>
    public static float GetLength(this Vector3 point, Vector3 center)
        => new Vector2(point.X - center.X, point.Z - center.Z).Length();

    /// <summary>
    /// 将任意点以中心点为圆心，逆时针旋转并延长。
    /// </summary>
    /// <param name="point">任意点</param>
    /// <param name="center">中心点</param>
    /// <param name="radian">旋转弧度</param>
    /// <param name="length">基于该点延伸长度</param>
    /// <returns></returns>
    public static Vector3 RotateAndExtend(this Vector3 point, Vector3 center, float radian, float length)
    {
        var baseRad = point.GetRadian(center);
        var baseLength = point.GetLength(center);
        var rotRad = baseRad + radian;
        return new Vector3(
            center.X + MathF.Sin(rotRad) * (length + baseLength),
            center.Y,
            center.Z + MathF.Cos(rotRad) * (length + baseLength)
        );
    }

    /// <summary>
    /// 获得某角度所在划分区域
    /// </summary>
    /// <param name="radian">输入弧度</param>
    /// <param name="regionNum">区域划分数量</param>
    /// <param name="baseRegionIdx">0度所在区域的初始Idx</param>>
    /// <param name="isDiagDiv">是否为斜分割，默认为false</param>
    /// <param name="isCw">是否顺时针增加，默认为false</param>
    /// <returns></returns>
    public static int RadianToRegion(this float radian, int regionNum, int baseRegionIdx = 0, bool isDiagDiv = false, bool isCw = false)
    {
        var sepRad = float.Pi * 2 / regionNum;
        var inputAngle = radian * (isCw ? -1 : 1) + (isDiagDiv ? sepRad / 2 : 0);
        var rad = (inputAngle + 4 * float.Pi) % (2 * float.Pi);
        return ((int)Math.Floor(rad / sepRad) + baseRegionIdx + regionNum) % regionNum;
    }

    /// <summary>
    /// 将输入点左右折叠
    /// </summary>
    /// <param name="point">待折叠点</param>
    /// <param name="centerX">中心折线坐标点</param>
    /// <returns></returns>
    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
        => point with { X = 2 * centerX - point.X };

    /// <summary>
    /// 将输入点上下折叠
    /// </summary>
    /// <param name="point">待折叠点</param>
    /// <param name="centerZ">中心折线坐标点</param>
    /// <returns></returns>
    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
        => point with { Z = 2 * centerZ - point.Z };

    /// <summary>
    /// 将输入点中心对称
    /// </summary>
    /// <param name="point">输入点</param>
    /// <param name="center">中心点</param>
    /// <returns></returns>
    public static Vector3 PointCenterSymmetry(this Vector3 point, Vector3 center)
        => point.RotateAndExtend(center, float.Pi, 0);

    /// <summary>
    /// 获取给定数的指定位数
    /// </summary>
    /// <param name="val">给定数值</param>
    /// <param name="x">对应位数，个位为1</param>
    /// <returns></returns>
    public static int GetDecimalDigit(this int val, int x)
    {
        var valStr = val.ToString();
        var length = valStr.Length;
        if (x < 1 || x > length) return -1;
        var digitChar = valStr[length - x]; // 从右往左取第x位
        return int.Parse(digitChar.ToString());
    }
}

public enum MarkType
{
    None = -1,
    Attack1 = 0,
    Attack2 = 1,
    Attack3 = 2,
    Attack4 = 3,
    Attack5 = 4,
    Bind1 = 5,
    Bind2 = 6,
    Bind3 = 7,
    Ignore1 = 8,
    Ignore2 = 9,
    Square = 10,
    Circle = 11,
    Cross = 12,
    Triangle = 13,
    Attack6 = 14,
    Attack7 = 15,
    Attack8 = 16,
    Count = 17
}

public static class IbcHelper
{
    public static string GetPlayerJob(this ScriptAccessory accessory, IPlayerCharacter? playerObject, bool fullName = false)
    {
        if (playerObject == null) return "None";
        return fullName ? playerObject.ClassJob.Value.Name.ToString() : playerObject.ClassJob.Value.Abbreviation.ToString();
    }

    /// <summary>
    /// 获取玩家的职能
    /// Return: "Tank"(坦克) / "Healer"(治疗) / "Melee DPS"(近战) / "Ranged DPS"(远程) / "Unknown" / "None"
    /// </summary>
    public static string GetPlayerRole(this ScriptAccessory sa, IPlayerCharacter? playerObject)
    {
        if (playerObject == null) return "None";
        return playerObject.ClassJob.Value.Role switch
        {
            1 => "Tank",        // 坦克
            4 => "Healer",      // 治疗
            2 => "Melee DPS",   // 近战DPS
            3 => "Ranged DPS",  // 远程DPS
            _ => "Unknown"
        };
    }
    
    public static IGameObject? GetById(this ScriptAccessory sa, ulong gameObjectId)
    {
        return sa.Data.Objects.SearchById(gameObjectId);
    }

    public static IGameObject? GetMe(this ScriptAccessory sa)
    {
        return sa.Data.Objects.LocalPlayer;
    }

    public static IEnumerable<IGameObject?> GetByDataId(this ScriptAccessory sa, uint dataId)
    {
        return sa.Data.Objects.Where(x => x.DataId == dataId);
    }

    public static float GetStatusRemainingTime(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return 0;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return charaStruct->GetStatusManager()->GetRemainingTime(statusIdx);
        }
    }

    public static bool HasStatus(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return false;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return statusIdx != -1;
        }
    }

    /// <summary>
    /// 获取指定标记索引的对象EntityId
    /// </summary>
    public static unsafe ulong GetMarkerEntityId(uint markerIndex)
    {
        var markingController = MarkingController.Instance();
        if (markingController == null) return 0;
        if (markerIndex >= 17) return 0;

        return markingController->Markers[(int)markerIndex];
    }

    /// <summary>
    /// 获取对象身上的标记
    /// </summary>
    /// <returns>MarkType</returns>
    public static MarkType GetObjectMarker(IGameObject? obj)
    {
        if (obj == null || !obj.IsValid()) return MarkType.None;

        ulong targetEntityId = obj.EntityId;
            
        for (uint i = 0; i < 17; i++)
        {
            var markerEntityId = GetMarkerEntityId(i);
            if (markerEntityId == targetEntityId)
            {
                return (MarkType)i;
            }
        }

        return MarkType.None;
    }

    /// <summary>
    /// 检查对象是否有指定的标记
    /// </summary>
    public static bool HasMarker(IGameObject? obj, MarkType markType)
    {
        return GetObjectMarker(obj) == markType;
    }

    /// <summary>
    /// 检查对象是否有任何标记
    /// </summary>
    public static bool HasAnyMarker(IGameObject? obj)
    {
        return GetObjectMarker(obj) != MarkType.None;
    }

    private static ulong GetMarkerForObject(IGameObject? obj)
    {
        if (obj == null) return 0;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return markerEntityId;
                }
            }
        }
        return 0;
    }

    private static MarkType GetMarkerTypeForObject(IGameObject? obj)
    {
        if (obj == null) return MarkType.None;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return (MarkType)i;
                }
            }
        }
        return MarkType.None;
    }

    /// <summary>
    /// 获取标记的名称
    /// </summary>
    public static string GetMarkerName(MarkType markType)
    {
        return markType switch
        {
            MarkType.Attack1 => "攻击1",
            MarkType.Attack2 => "攻击2",
            MarkType.Attack3 => "攻击3",
            MarkType.Attack4 => "攻击4",
            MarkType.Attack5 => "攻击5",
            MarkType.Bind1 => "止步1",
            MarkType.Bind2 => "止步2",
            MarkType.Bind3 => "止步3",
            MarkType.Ignore1 => "禁止1",
            MarkType.Ignore2 => "禁止2",
            MarkType.Square => "方块",
            MarkType.Circle => "圆圈",
            MarkType.Cross => "十字",
            MarkType.Triangle => "三角",
            MarkType.Attack6 => "攻击6",
            MarkType.Attack7 => "攻击7",
            MarkType.Attack8 => "攻击8",
            _ => "无标记"
        };
    }
    
    public static float GetHitboxRadius(IGameObject obj)
    {
        if (obj == null || !obj.IsValid()) return -1;
        return obj.HitboxRadius;
    }

}

public static class HelperExtensions
{
    public static unsafe uint GetCurrentTerritoryId()
    {
        return AgentMap.Instance()->CurrentTerritoryId; // 额外进行地图ID判断
    }
}

public unsafe static class ExtensionVisibleMethod
{
    public static bool IsCharacterVisible(this ICharacter chr)
    {
        var v = (IntPtr)(((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)chr.Address)->GameObject.DrawObject);
        if (v == IntPtr.Zero) return false;
        return Bitmask.IsBitSet(*(byte*)(v + 136), 0);
    }

    public static class Bitmask
    {
        public static bool IsBitSet(ulong b, int pos)
        {
            return (b & (1UL << pos)) != 0;
        }

        public static void SetBit(ref ulong b, int pos)
        {
            b |= 1UL << pos;
        }

        public static void ResetBit(ref ulong b, int pos)
        {
            b &= ~(1UL << pos);
        }

        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static bool IsBitSet(short b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
#endregion EventExtensions