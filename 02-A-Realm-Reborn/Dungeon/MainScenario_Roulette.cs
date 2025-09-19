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
// using ECommons;
// using ECommons.DalamudServices;
// using ECommons.GameFunctions;
// using ECommons.MathHelpers;
using System.Threading.Tasks;

namespace MainScenario_Roulette;

[ScriptType(guid: "84c15eea-2a19-4477-ad21-cd43d1263cfa", name: "随机任务：主线任务", territorys: [1043, 1044, 1048],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class MainScenario_Roulette
{
    const string noteStr =
        """
        v0.0.0.2:
        LV50 随机任务：主线任务 初版绘制
        选项中的DR辅助需要你正确安装Daily Routines插件才可使用
        """;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("DR 动画城上机甲自动使用魔导加农炮")]
    public bool isDRHelper { get; set; } = true;
    
    [ScriptMethod(name: "DR 动画城上机甲自动使用魔导加农炮", eventType: EventTypeEnum.ChangeMap, eventCondition: ["MapId:100"])]
    public void AutoUse(Event @event, ScriptAccessory accessory)
    {
        if (isDRHelper) accessory.Method.SendChat("/pdr load ThePraetoriumHelper");
    }
    
    #region 神兵要塞帝国南方堡
    
    [ScriptMethod(name: "————神兵要塞帝国南方堡————", eventType: EventTypeEnum.Tether, eventCondition: ["ActionId:"])]
    public void 神兵要塞帝国南方堡(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "BOSS1_魔导死神改 高功率魔导激光（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28773"])]
    public void 高功率魔导激光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "高功率魔导激光";
        dp.Scale = new (4, 60f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS1_魔导死神改 请求轰炸（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29268"])]
    public void 请求轰炸(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("AOE", duration: 2000, false);
        if (isTTS)accessory.Method.TTS("AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "BOSS2_魔导先锋强袭型 气化炸弹（陨石）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28779"])]
    public void 气化炸弹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "气化炸弹";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = 6700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_莉维亚 魔导离子（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28033"])]
    public void 魔导离子(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("AOE", duration: 4000, false);
        if (isTTS)accessory.Method.TTS("AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS("AOE");
    }
    
    /*  太丑了算了
    [ScriptMethod(name: "BOSS3_莉维亚 枪击（圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28794"])]
    public void 枪击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "枪击";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Delay = 5400;
        dp.DestoryAt = 1500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_莉维亚 无限刃（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28793"])]
    public void 无限刃(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "无限刃";
        dp.Scale = new (4, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Delay = 5400;
        dp.DestoryAt = 1500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    */
    
    #endregion
    
    #region 最终决战天幕魔导城
    
    [ScriptMethod(name: "————最终决战天幕魔导城————", eventType: EventTypeEnum.Tether, eventCondition: ["ActionId:"])]
    public void 最终决战天幕魔导城(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "路中_轰炸 & 魔导加农炮（圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^29(180|049)$"])]
    public void 魔导加农炮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"魔导加农炮{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "魔导加农炮 打断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:29180"], userControl: false)]
    public void 寒冰雷电咆哮销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"魔导加农炮{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "BOSS1_魔导巨兵二型 青磷放射（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28474"])]
    public void 青磷放射(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("AOE", duration: 4000, false);
        if (isTTS)accessory.Method.TTS("AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "BOSS2_尼禄 过载震碎（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28477"])]
    public void 过载震碎(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "过载震碎";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId;
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_盖乌斯 恐怖战争（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28495"])]
    public void 恐怖战争(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("AOE", duration: 4000, false);
        if (isTTS)accessory.Method.TTS("AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "BOSS3_盖乌斯 恩惠终结（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2848[78]$"])]
    public void 恩惠终结(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "恩惠终结";
        dp.Scale = new (4, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS3_盖乌斯 从容不迫（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28493"])]
    public void 从容不迫(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "从容不迫";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId;
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_盖乌斯 指引（圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29052"])]
    public void 指引(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "指引";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Delay = 2700;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region 究极神兵破坏作战
    
    [ScriptMethod(name: "————究极神兵破坏作战————", eventType: EventTypeEnum.Tether, eventCondition: ["ActionId:"])]
    public void 究极神兵破坏作战(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "魔导核爆（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29022"])]
    public void 魔导核爆(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("AOE", duration: 4000, false);
        if (isTTS)accessory.Method.TTS("AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "攻城炮（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29020"])]
    public void 攻城炮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "攻城炮";
        dp.Scale = new (12, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "魔导激光（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2900[89]|29010)$"])]
    public void 魔导激光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "魔导激光";
        dp.Scale = new (6, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        switch (@event.ActionId())
        {
            case 29010:  // 左
                dp.Rotation = MathHelpers.DegToRad(45f);
                break;
            case 29008:  // 中
                dp.Rotation = MathHelpers.DegToRad(0f);
                break;
            case 29009:  // 右
                dp.Rotation = MathHelpers.DegToRad(315f);
                break;
        }
        dp.DestoryAt = 1900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "以太波动（中间击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29015"])]
    public void 以太波动(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("中间击退 + 撞球", duration: 3000, false);
        if (isTTS)accessory.Method.TTS("中间击退");
        if (isEdgeTTS)accessory.Method.EdgeTTS("中间击退");
    }
    
    [ScriptMethod(name: "集束激光（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29014"])]
    public void 集束激光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "集束激光";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId;
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
 /*
    [ScriptMethod(name: "以太爆雷（撞球）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:14594"])]
    public void 以太爆雷(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"以太爆雷{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "以太爆雷销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29016"], userControl: false)]
    public void 以太爆雷销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"以太爆雷{@event.SourceId()}");
    }
 */
 
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