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

namespace Pharos_Sirius;

[ScriptType(guid: "8b0c94a9-2d05-4df8-9823-bed2a8d891a5", name: "领航明灯天狼星灯塔", territorys: [160],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Pharos_Sirius
{
    const string noteStr =
        """
        v0.0.0.1
        LV50 领航明灯天狼星灯塔 初版绘制及部分机制提示
        """;
    
    #region 用户控制
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion
    
    #region 阶段记录与绘制销毁
    
    private bool _isHelper = true;

    [ScriptMethod(name: "地图记录", eventType: EventTypeEnum.ChangeMap, eventCondition: ["MapId:regex:^(108|13[0-4])$"], userControl: false)]
    public void 地图记录(Event @event, ScriptAccessory accessory)
    {
        phase = @event.MapId() switch
        {
            108 => Phase.Start,
            130 => Phase.BOSS1,
            131 => Phase.GoUpstair,
            132 => Phase.BOSS2,
            133 => Phase.GoUpstair2,
            134 => Phase.BOSS4,
            _ => phase
        };
    
        _isHelper = true;
    
        if(isDeveloper) 
        {
            accessory.Method.SendChat($"/e [DEBUG]: 检测到MapId: {@event.MapId()}, 阶段设置为: {phase}");
        }
    }
    
    public enum Phase
    {
        Init,    // 初始
        Start,  // 进本
        BOSS1, // 一波小怪后老一（小狗）
        GoUpstair, // 老一后上楼
        BOSS2,  // 一波小怪后老二（祖）
        GoUpstair2, // 老二后上楼
        BOSS4,  // 暴君是BOSS3，上楼后尾王
    }
    
    private Phase phase = Phase.Init;
    
    
    [ScriptMethod(name: "伤头&插言 打断销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void 打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "咏唱中断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: [], userControl: false)]
    public void 咏唱中断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "死亡销毁", eventType: EventTypeEnum.Death, eventCondition: [], userControl: false)]
    public void 死亡销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }
    
    #endregion
    
    [ScriptMethod(name:"开场提示", eventType:EventTypeEnum.CombatChanged, eventCondition: ["Type:ResetCombat", "InCombat:False"])]
    public void 开场提示(Event @event, ScriptAccessory accessory)
    {
        if (_isHelper && phase == Phase.Start)
        {
            if(isText) accessory.Method.TextInfo("此副本奶妈压力高，请少吃机制", duration: 5000, true);
            _isHelper = false;
        }
    }
    
    #region BOSS1_不沉者 西蒙德

    [ScriptMethod(name:"BOSS1前提示", eventType:EventTypeEnum.AddCombatant, eventCondition: ["DataId:2299"])]
    public void BOSS1前提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("该BOSS没有眩晕抗性,可踢晕钢铁 \n debuff吃到3层会爆炸", duration: 5000, true);
    }
    
    [ScriptMethod(name: "BOSS1_不沉者 西蒙德 天上击 - 水晶弹雨", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:regex:^200303[23]$"])]
    public void 水晶弹雨(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "水晶弹雨";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Radian = float.Pi * 2;
        dp.Delay = 2000;
        dp.DestoryAt = 3000;
        // dp.ScaleMode = ScaleMode.ByTime; // 好丑啊(x

        switch (@event.DataId())
        {
            case 2003032: // 天上击落下时，触发中圈绘制
                dp.Scale = new Vector2(11.5f);
                dp.InnerScale = new Vector2(9f);
                break;
            case 2003033: // 最大圈 DataId为 2003034
                dp.Scale = new Vector2(14f);
                dp.InnerScale = new Vector2(11.5f);
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "BOSS1_不沉者 西蒙德 十亿斩击 眩晕提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1502"])]
    public void 十亿斩击(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("眩晕BOSS", duration: 3000, true);
        if (isTTS)accessory.Method.TTS("眩晕BOSS");
        if (isEdgeTTS)accessory.Method.EdgeTTS("眩晕BOSS");
    }
    
    [ScriptMethod(name: "BOSS1_被魅惑的军犬 腐坏唾液 Imgui高亮", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1499"])]
    public void 腐坏唾液(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "腐坏唾液";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1_偏属性水晶 腐坏爆发 (2层预兆)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:374", "StackCount:2"])]
    public void 腐坏爆发预兆(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"腐坏爆发预兆{@event.TargetId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.2f);
        dp.Owner = @event.TargetId;
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "腐坏爆发2层预兆销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:374", "StackCount:2"], userControl:false)]
    public void 腐坏爆发预兆销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"腐坏爆发预兆{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "战斗结束腐坏爆发2层预兆销毁", eventType: EventTypeEnum.Director, eventCondition: ["Command:80000001", "Instance:80030011"], userControl:false)]
    public void 战斗结束腐坏爆发预兆销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"腐坏爆发预兆.*");
    }
    
    [ScriptMethod(name: "BOSS1_偏属性水晶 腐坏爆发 (3层)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:374", "StackCount:3"])]
    public void 腐坏爆发(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "腐坏爆发";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId;
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    [ScriptMethod(name: "浮灵_强麻痹 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1662"])]
    public void 浮灵_强麻痹提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("打断或眩晕浮灵麻痹", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("打断或眩晕浮灵");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断或眩晕浮灵");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"浮灵{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(0.9f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "浮灵_强麻痹 点名绘制", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1662"])]
    public void 浮灵_强麻痹绘制(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"浮灵_强麻痹{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "腐化元精_强放逐 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1664"])]
    public void 腐化元精_强放逐提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("打断或眩晕腐坏元精AOE", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("打断或眩晕腐坏元精");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断或眩晕腐坏元精");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"腐化元精{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "腐化元精_强放逐 AOE绘制", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1664"])]
    public void 腐化元精_强放逐绘制(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"腐化元精_强放逐{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    #region BOSS2_祖
    
    [ScriptMethod(name:"BOSS2前提示", eventType:EventTypeEnum.AddCombatant, eventCondition: ["DataId:2292"])]
    public void BOSS2前提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("别打鸟蛋，避开鸟蛋，平A均为顺劈", duration: 5000, true);
        if (isTTS)accessory.Method.TTS("别打鸟蛋，避开鸟蛋");
        if (isEdgeTTS)accessory.Method.EdgeTTS("别打鸟蛋，避开鸟蛋");
    }
    
    // BOSS2 祖 目标圈3.6m，雄/雌性祖雏鸟 目标圈0.4m 平A均为 目标圈 +9m的顺劈
    // 狂乱为固定点DPS的6m圆形死刑，当buff＞10层后应绘制两个DPS（每打死一个鸟蛋+8层）
    
    /* 还要考虑触发间隔等问题 先放着
    [ScriptMethod(name: "BOSS2_祖 狂乱（DPS圆形死刑）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:372", "StackCount:regex:^1[0-6]$"])]
    public void 狂乱(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "狂乱";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    */
    
    #endregion
    
    [ScriptMethod(name: "偏属性以太云高亮", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:2329"])]
    public void 偏属性以太云(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"偏属性以太云{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "偏属性以太云_以太爆发预测", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:2329"])]
    public void 偏属性以太云_以太爆发预测(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"偏属性以太云{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.25f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "偏属性以太云以太爆发销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:1668"],userControl: false)]
    public void 偏属性以太云以太爆发销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"偏属性以太云{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "偏属性以太云销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:2329"],userControl: false)]
    public void 偏属性以太云销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"偏属性以太云{@event.SourceId()}");
    }

    
    #region  BOSS3_塞壬

    
    [ScriptMethod(name: "BOSS3_塞壬 死者之歌 奶满提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1483"])]
    public void 莫古神准箭(Event @event, ScriptAccessory accessory)
    {
        string tname = @event["TargetName"]?.ToString() ?? "未知目标";

        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText)accessory.Method.TextInfo("奶满自己", duration: 2000, true);
            if (isTTS)accessory.Method.TTS("奶满自己");
            if (isEdgeTTS)accessory.Method.EdgeTTS("奶满自己");
        } else
        {
            if (isText)accessory.Method.TextInfo($"奶满 <{tname}>", duration: 2000, false);
            if (isTTS)accessory.Method.TTS($"奶满{tname}");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"奶满{tname}");
        }
    }
    
    [ScriptMethod(name: "BOSS3_塞壬 错乱之声 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1485"])]
    public void 错乱之声(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("打断BOSS", duration: 3000, true);
        if (isTTS)accessory.Method.TTS("打断BOSS");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断BOSS");
    }
    
    [ScriptMethod(name: "BOSS3_塞壬 苦闷之歌 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1486"])]
    public void 苦闷之歌(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo("坦克流血死刑", duration: 3000, true);
        if (isTTS)accessory.Method.TTS("坦克流雪死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS("坦克流雪死刑");
    }
    
    [ScriptMethod(name: "BOSS3_塞壬 亡者之歌（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1487"])]
    public void 亡者之歌(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "亡者之歌";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(52.4f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "BOSS3_塞壬 急袭（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1484"])]
    public void 急袭(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "急袭";
        dp.Scale = new (12f, 52.4f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
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
    
    public static uint DataId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DataId"]);
    }
    
    public static uint MapId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["MapId"]);
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