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

namespace Windurst_The_Third_Walk;

[ScriptType(guid: "fa374b84-5ce5-405c-a22d-3e7ea1c9591b", name: "LV100 7.5 温达斯：第三巡行", territorys: [1368],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Windurst_The_Third_Walk
{
    const string noteStr =
        """
        LV100 7.5 温达斯：第三巡行 v0.0.0.1:
        初版绘制，不完整，凑合着先用！
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
    
    #region 小怪
    
    // 1.5
    
    [ScriptMethod(name: "————————————  小怪部分  ————————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 小怪部分(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "1.5 阿巴佐恩_一刀两断 中心击退", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50481$"])]
    public void 阿巴佐恩_一刀两断(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "一刀两断_中心击退预测";
        dp.Scale = new(0.5f, 22f);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "1.5 尼米亚猛狮_风涡（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50091$"])]
    public void 尼米亚猛狮_风涡(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"尼米亚猛狮_风涡";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (50f, 40f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
        
    }
    
    [ScriptMethod(name: "1.5 美杜莎_左/右侧影之斩击（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5009[89]$"])]
    public void 影之斩击(Event @event, ScriptAccessory accessory)
    {
        var isR = @event.ActionId == 50098;

        if (isR)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"右侧影之斩击";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(60);
            dp.Radian = 180f.DegToRad(); 
            dp.Rotation = 270f.DegToRad();
            dp.DestoryAt = 4700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        else
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"左侧影之斩击";
            dp1.Color = accessory.Data.DefaultDangerColor;
            dp1.Owner = @event.SourceId();
            dp1.Scale = new Vector2(60f);
            dp1.Radian = 180f.DegToRad(); 
            dp1.Rotation = 90f.DegToRad();
            dp1.DestoryAt = 4700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
        }
    }
    
    /*
    
    [ScriptMethod(name: "1.5 美杜莎_破石之舞（步进地火）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50096$"])]
    public void 美杜莎_破石之舞(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"破石之舞{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 7000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    */
    
    #endregion
    
    #region BOSS1_恶魔香托托

    [ScriptMethod(name: "————————  BOSS1_恶魔香托托  ————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void BOSS1_恶魔香托托(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "怒不可遏的核爆 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50215$"])]
    public void 怒不可遏的核爆(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"AOE", duration: 4300, false);
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "维多夫尼尔 分摊死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50214$"])]
    public void 维多夫尼尔(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"分摊死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"分摊死刑");
        
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"维多夫尼尔";
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 6000;
        if(!isTank) 
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        else
        {
            dp.Color = accessory.Data.DefaultSafeColor;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "粉身碎骨的地震（夹心直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50196$"])]
    public void 粉身碎骨的地震(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"粉身碎骨的地震";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (12f, 30f);
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "实证研究（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50208$"])]
    public void 实证研究(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"实证研究";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (12f, 80f);
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "展开步法阵_钻环跳圈的火炎（连续月环）", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^01(80|7F)$"])]
    public void 钻环跳圈的火炎(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"钻环跳圈的火炎";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(10f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.InnerScale = new Vector2(5.9f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = @event.Id == 0180 ? 20200 : 23900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "靠近乘凉的冰结（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50203$"])]
    public void 靠近乘凉的冰结(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"远离加分散");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"远离加分散");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"靠近乘凉的冰结";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 1900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "陨星（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50185$"])]
    public void 陨星(Event @event, ScriptAccessory accessory)
    {
        string targetName = @event["TargetName"]?.ToString();
        if (!string.IsNullOrEmpty(targetName))
        {
            if (isText)accessory.Method.TextInfo($"靠近{targetName}集合", duration: 5300, true);
            if (isTTS) accessory.Method.TTS($"靠近{targetName}集合");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"靠近{targetName}集合");
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"陨星";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "冲击波 大AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50187$"])]
    public void 冲击波(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"大AOE伤害", duration: 9500, true);
        if (isTTS)accessory.Method.TTS($"大AOE伤害");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"大AOE伤害");
    }
    

    
    #endregion
    
    #region BOSS2_巨神重现 亚历山大
    
    [ScriptMethod(name: "————————  BOSS2_巨神重现 亚历山大  ————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void BOSS2_亚历山大(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "BOSS2_亚历山大 圣箭（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5012[45]$"])]
    public void 圣箭(Event @event, ScriptAccessory accessory)
    {
        var isR = @event.ActionId == 50125;

        if (isR)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"右圣箭";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(45f);
            dp.Radian = 90f.DegToRad(); 
            dp.Rotation = 270f.DegToRad();
            dp.DestoryAt = 9700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        else
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"左圣箭";
            dp1.Color = accessory.Data.DefaultDangerColor;
            dp1.Owner = @event.SourceId();
            dp1.Scale = new Vector2(45f);
            dp1.Radian = 90f.DegToRad(); 
            dp1.Rotation = 90f.DegToRad();
            dp1.DestoryAt = 9700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
        }
    }
    
    [ScriptMethod(name: "BOSS2_亚历山大 审理之光（半场）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5014[67]$"])]
    public void 审理之光 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"审理之光";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.Radian = 180f.DegToRad(); 
        dp.Delay = @event.ActionId == 50146 ? 0 : 6800;
        dp.DestoryAt = @event.ActionId == 50146 ? 6700 : 2900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "BOSS2_亚历山大 圣炎（三角但是先凑合用）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50143$"])]
    public void 圣炎 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"圣炎";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "BOSS2_亚历山大 百万神圣（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50158$"])]
    public void 百万神圣(Event @event, ScriptAccessory accessory)
    {
        string targetName = @event["TargetName"]?.ToString();
        if (!string.IsNullOrEmpty(targetName))
        {
            if (isText)accessory.Method.TextInfo($"靠近{targetName}集合", duration: 5300, true);
            if (isTTS) accessory.Method.TTS($"靠近{targetName}集合");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"靠近{targetName}集合");
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"百万神圣{@event.TargetId}";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 9600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS2_戈尔狄系统 反射防壁", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50686$"])]
    public void 反射防壁(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"停止攻击 <反射防壁>", duration: 3300, true);
        if (isTTS)accessory.Method.TTS($"停止攻击反射防壁");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"停止攻击反射防壁");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"反射防壁{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 20000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region BOSS3_普罗玛西亚
    
    [ScriptMethod(name: "————————  BOSS3_普罗玛西亚  ————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void BOSS3_普罗玛西亚(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "BOSS3_普罗玛西亚 转瞬_爆炸（流动钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50320$"])]
    public void 转瞬_爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"转瞬_爆炸{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16f);
        // dp.Delay = 2900;
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_普罗玛西亚 虚幻之环（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50321$"])]
    public void 虚幻之环(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"虚幻之环";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 12400;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_普罗玛西亚 黄泉灶食_黄昏乐土（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50331$"])]
    public void 黄昏乐土(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"去背后加靠近");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"去背后加靠近");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"黄昏乐土";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "BOSS3_空虚叹息之物 极光帷幕（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50355$"])]
    public void 极光帷幕(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"极光帷幕";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (7f, 7f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "BOSS3_空虚思考之物 虚无之风（旋转直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50354$"])]
    public void 虚无之风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"虚无之风";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (6f, 16f);
        dp.DestoryAt = 300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "BOSS3_普罗玛西亚 回天挽日（半场刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50329$"])]
    public void 回天挽日(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"回天挽日";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (50f, 50f);
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "BOSS3_普罗玛西亚的羁绊 黄泉灶食（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50332$"])]
    public void 黄泉灶食(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"黄泉灶食";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (5f, 50f);
        dp.DestoryAt = 7200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "BOSS3_普罗玛西亚 诱引（踩塔钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50565$"])]
    public void 诱引(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"离开原地");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"离开原地");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"诱引{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region BOSS4_神龙
    
    [ScriptMethod(name: "————————  BOSS4_神龙  ————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void BOSS4_神龙(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "BOSS4_神龙 宇宙吐息（上半场）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49107$"])]
    public void 宇宙吐息(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"↓ 去下方平台 ↓", duration: 6300, true);
        if (isTTS)accessory.Method.TTS($"去下方平台");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"去下方平台");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"宇宙吐息";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (70f, 50f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "BOSS4_神龙 宇宙甩尾（下半场）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49110$"])]
    public void 宇宙甩尾(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"↑ 去上方平台 ↑", duration: 6300, true);
        if (isTTS)accessory.Method.TTS($"去上方平台");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"去上方平台");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"宇宙甩尾";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (70f, 50f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "BOSS4_神龙 黄昏披光_黄昏光辉 debuff提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^5352$"])]
    public void 黄昏披光_黄昏光辉(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo($"debuff结束前呆在楼上（光）", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"呆在楼上（光）");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"呆在楼上（光）");
    }
    
    [ScriptMethod(name: "BOSS4_神龙 黄昏披暗_黄昏暗影 debuff提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^5353$"])]
    public void 黄昏披暗_黄昏暗影(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo($"debuff结束前呆在楼下（暗）", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"呆在楼下（暗）");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"呆在楼下（暗）");
    }
    
    [ScriptMethod(name: "BOSS4_神龙 原子甩尾（下半场）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49130$"])]
    public void 原子甩尾(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"↑ 去上方平台，平台破坏 ↑", duration: 6300, true);
        if (isTTS)accessory.Method.TTS($"去上方平台");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"去上方平台");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"原子甩尾";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (70f, 50f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    /* 不确定 先放着
    [ScriptMethod(name: "BOSS4_神龙 灾变涡旋 头标提示", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^02A(8|9|A|B)$"])]
    public void 灾变涡旋(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"", duration: 3000, true);
        if (isTTS)accessory.Method.TTS($"");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"");
    }
    */
    
    #endregion
    
    #region BOSS4_虚无之王
    
    [ScriptMethod(name: "————————  BOSS4_虚无之王  ————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void BOSS4_虚无之王(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "BOSS4_虚无之王 左/右侧交错剑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4915[3-6]$"])]
    public void 交错剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp.Scale = new (30f, 60f);
        dp1.Scale = new (36f, 70f);
        dp1.Color = dp.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = dp.Owner = @event.SourceId();
        dp1.DestoryAt = dp.DestoryAt = 8700;
        
        switch (@event.ActionId())
        {
            case 49153:
                dp.Name = "右侧交错剑_斜";
                dp.Offset = new Vector3(15, 0, 15);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
                break;
            case 49154:
                dp.Name = "左侧交错剑_斜";
                dp.Offset = new Vector3(-15, 0, 15);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
                break;
            case 49155:
                dp1.Name = "左侧交错剑_直";
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1); 
                break;
            case 49156:
                dp1.Name = "右侧交错剑_直";
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1); 
                break;
        }
        
    }
    
    [ScriptMethod(name: "BOSS4_虚无之王 双重吐息（钢铁月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^491(59|60)$"])]
    public void 双重吐息(Event @event, ScriptAccessory accessory)
    {
        switch (@event.ActionId())
        {
            case 49159:
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"双重吐息_月环";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = @event.SourceId();
                dp.Scale = new Vector2(60f);
                dp.InnerScale = new Vector2(20f);
                dp.Radian = 90f.DegToRad(); 
                dp.DestoryAt = 5700;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                break;
            case 49160:
                var dp1 = accessory.Data.GetDefaultDrawProperties();
                dp1.Name = $"双重吐息_钢铁";
                dp1.Color = accessory.Data.DefaultDangerColor;
                dp1.Owner = @event.SourceId();
                dp1.Scale = new Vector2(35f);
                dp1.Radian = 90f.DegToRad(); 
                dp1.DestoryAt = 5700;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
                break;
        }
        
    }
    
    [ScriptMethod(name: "BOSS4_虚无之王 灾变之刃（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49162$"])]
    public void 灾变之刃 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"灾变之刃";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 45f.DegToRad(); 
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    /* 丑了点 回头再改
    [ScriptMethod(name: "BOSS4_虚无之王 磁暴（钢铁月环动画）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4917[1-3]$"])]
    public void 磁暴(Event @event, ScriptAccessory accessory)
    {
        switch (@event.ActionId())
        {
            case 49171:
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"磁暴_钢铁";
                dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
                dp.Owner = @event.SourceId();
                dp.Scale = new Vector2(10f);
                dp.DestoryAt = 4700;
                dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                break;
            case 49172:
                var dp1 = accessory.Data.GetDefaultDrawProperties();
                dp1.Name = $"磁暴_月环1";
                dp1.Color = accessory.Data.DefaultDangerColor;
                dp1.Owner = @event.SourceId();
                dp1.Scale = new Vector2(20f);
                dp1.InnerScale = new Vector2(10f);
                dp1.Radian = float.Pi * 2;
                dp1.Delay = 4800;
                dp1.DestoryAt = 1900;
                dp1.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
                break;
            case 49173:                
                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = $"磁暴_月环2";
                dp2.Color = accessory.Data.DefaultDangerColor;
                dp2.Owner = @event.SourceId();
                dp2.Scale = new Vector2(30f);
                dp2.InnerScale = new Vector2(20f);
                dp2.Radian = float.Pi * 2;
                dp2.Delay = 6800;
                dp2.DestoryAt = 1900;
                dp2.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);

                break;
        }
    }
    */
    
    // BOSS4_虚无之王 宇宙烈焰（步进地火）麻烦的以后再说.jpg
    
    [ScriptMethod(name: "BOSS4_立体魔法阵 原子射线（移动直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49165$"])]
    public void 原子射线(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"躲避直线");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"躲避直线");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"原子射线";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (15f, 60f);
        dp.DestoryAt = 1200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    #endregion

    #region 绘制销毁

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
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(160|1209|2663)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($".*(击退|吸引).*");
    }
    
    [ScriptMethod(name: "反射防壁销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^5377$"], userControl: false)]
    public void 反射防壁销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"反射防壁.*");
    }

    #endregion
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

    public static string GetPlayerJob(this ScriptAccessory sa, IPlayerCharacter? playerObject, bool fullName = false)
    {
        if (playerObject == null) return "None";
        return fullName ? playerObject.ClassJob.Value.Name.ToString() : playerObject.ClassJob.Value.Abbreviation.ToString();
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
