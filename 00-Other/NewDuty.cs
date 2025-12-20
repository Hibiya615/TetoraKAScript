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

namespace NewDuty;

[ScriptType(guid: "80890eac-4730-4708-ad1b-05aba469c2a1", name: "最新最热临时绘制", territorys: [1314,1307,1308,1320,1322,1324,1326],
    version: "0.0.0.4", author: "Tetora", note: noteStr)]

/* MapID
 * 1314: 遗忘行路雾之迹
 * 1307: 格莱杨拉波尔歼灭战
 * 1308: 格莱杨拉波尔歼殛战
 */

public class NewDuty
{
    const string noteStr =
        """
        v0.0.0.4:
        最新最热副本绘制，可能会电，介意请关闭
        别人的正式版发了这边就删
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
    

    #region  遗忘行路雾之迹
    
    [ScriptMethod(name: "—————— 遗忘行路雾之迹 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 遗忘行路雾之迹(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "BOSS1_特雷诺卡托布莱帕斯 地震 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43327"])]
    public void 地震(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"AOE", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "BOSS1_特雷诺卡托布莱帕斯 暴雷 死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43329"])]
    public void 特雷诺卡托布莱帕斯_暴雷(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"坦克死刑", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"坦克死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"坦克死刑");
    }
    
    [ScriptMethod(name: "BOSS1_特雷诺卡托布莱帕斯 雷光射线（直线分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44825"])]
    public void 雷光射线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"雷光射线";
        dp.Scale = new (5f, 50f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultSafeColor; 
        dp.DestoryAt = 5300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS1_特雷诺卡托布莱帕斯 震雷 分散提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43331"])]
    public void 强化寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"分散，避开石头", duration: 2800, true);
        if (isTTS)accessory.Method.TTS($"分散，避开石头");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"分散，避开石头");
    }
    
    [ScriptMethod(name: "BOSS1_特雷诺卡托布莱帕斯 恶魔之光 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43330"])]
    public void 恶魔之光(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"躲在石头后", duration: 6300, true);
        if (isTTS)accessory.Method.TTS($"躲在石头后");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"躲在石头后");
    }
        
    [ScriptMethod(name: "BOSS1_特雷诺卡托布莱帕斯 石化吐息（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43335"])]
    public void 石化吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"石化吐息";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "BOSS2_安度西亚斯 猛毒菌 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45345"])]
    public void 猛毒菌(Event @event, ScriptAccessory accessory)
    {
        // 产生毒气团 DataId：19064
        // if (isText)accessory.Method.TextInfo($"AOE", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "BOSS2_安度西亚斯 霹雷 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45351"])]
    public void 霹雷(Event @event, ScriptAccessory accessory)
    {
        // 引爆剩余毒气团【45349 飞散】
        // if (isText)accessory.Method.TextInfo($"AOE，远离剩余雷球", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"AOE,远离剩余雷球");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE，远离剩余雷球");
    }
    
    // 45348 雷电飞驰 直线预兆
    
    [ScriptMethod(name: "BOSS2_安度西亚斯 暴雷（连续分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45353"])]
    public void 安度西亚斯_暴雷(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"暴雷";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 7200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS2_安度西亚斯 雷电震击 死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45356"])]
    public void 雷电震击(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"坦克死刑", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"坦克死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"坦克死刑");
    }
    
    [ScriptMethod(name: "BOSS2_安度西亚斯 惊雷协奏曲（大扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45337|45342)$"])]
    public void 惊雷协奏曲(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"惊雷协奏曲";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 310f.DegToRad(); 
        dp.Rotation = @event.ActionId() == 45337 ? 0f.DegToRad() : 180f.DegToRad();
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "BOSS3_雷狮鹫 电光 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45291"])]
    public void 电光(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"AOE", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "BOSS3_雷狮鹫 黄金爪 死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45305"])]
    public void 黄金爪(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"坦克死刑", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"坦克死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"坦克死刑");
    }
    
    [ScriptMethod(name: "BOSS3_雷狮鹫 霹雳 电球直线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4529[78]|4694[34])$"])]
    public void 霹雳(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "霹雳";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Scale = new (6f, 92f);
        dp.DestoryAt = 5200;
        
        /*
        switch (@event.ActionId())
        {
            case 45297:
                dp.Scale = new (92f, 6f);
                dp.DestoryAt = 5200;
                break;
            case 45298:
                dp.Scale = new (92f, 6f);
                dp.DestoryAt = 5200;
                break;
            case 46943:
                dp.Scale = new (20f, 3f);
                dp.DestoryAt = 4000;
                break;
            case 46944:
                dp.Scale = new (16f, 3f);
                dp.DestoryAt = 4000;
                break;
        }
        */

        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }

    [ScriptMethod(name: "BOSS3_雷狮鹫 雷光坠击 直线击退", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45301"])]
    public void 雷光坠击(Event @event, ScriptAccessory accessory)
    {
        // 击退距离为 12m
        // if (isText)accessory.Method.TextInfo($"中间击退然后躲避直线", duration: 5300, true);
        if (isTTS)accessory.Method.TTS($"中间击退然后躲避直线");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"中间击退然后躲避直线");
    }
    
    
    #endregion

    
    
    /*
    #region  格莱杨拉波尔歼灭战
    
    [ScriptMethod(name: "—————— 格莱杨拉波尔歼灭战 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 格莱杨拉波尔歼灭战(Event @event, ScriptAccessory accessory) { }
    
    #endregion
    */
    

    #region  格莱杨拉波尔歼殛战
    
    [ScriptMethod(name: "—————— 格莱杨拉波尔歼殛战 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 格莱杨拉波尔歼殛战(Event @event, ScriptAccessory accessory) { }
    
    uint Sperad = 0;
    uint Stack = 0;
    
    public void Init(ScriptAccessory accessory) {
        Sperad = 0;
        Stack = 0;
    }
    
    [ScriptMethod(name:"分散分摊备用重置", eventType:EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4566[34]$"], userControl:false)]
    public void 分散分摊备用重置(Event @event, ScriptAccessory accessory) 
    {
         Sperad = 0;
         Stack  = 0;
    }
    
    [ScriptMethod(name:"分散分摊记录", eventType:EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^4566[34]$"], userControl:false)]
    public void 分散分摊记录(Event @event, ScriptAccessory accessory) 
    {
        switch (@event.ActionId())
        {
            case 45663:
                Sperad = 1;
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 已记录【分散】");
                break;
            case 45664:
                Stack  = 1;
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 已记录【分摊】");
                break;
        }
    }
    
    [ScriptMethod(name:"超增压 分散/分摊 存储播报", eventType:EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4566[34]$"])]
    public void 超增压存储(Event @event, ScriptAccessory accessory) 
    {
        switch (@event.ActionId())
        {
            case 45663:
                if (isTTS)accessory.Method.TTS($"存储分散");
                if (isEdgeTTS)accessory.Method.EdgeTTS($"存储分散");
                break;
            case 45664:
                if (isTTS)accessory.Method.TTS($"存储分摊");
                if (isEdgeTTS)accessory.Method.EdgeTTS($"存储分摊");
                break;
        }
    }
    
    [ScriptMethod(name:"超增压 击退/吸引 分散/分摊 播报", eventType:EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4567[07]$"])]
    public void 超增压(Event @event, ScriptAccessory accessory) 
    {
        if (@event.ActionId == 45670)
        {
            string knockbackText = "击退";
            string knockbackTTS = "击退";
        
            if (Sperad == 1)
            {
                knockbackText += " + 分散";
                knockbackTTS += " 后分散";
            }
            else if (Stack == 1)
            {
                knockbackText += " + 分摊";
                knockbackTTS += " 后分摊";
            }
        
            if (isText) accessory.Method.TextInfo(knockbackText, duration: 10500, true);
            if (isTTS) accessory.Method.TTS(knockbackTTS);
            if (isEdgeTTS) accessory.Method.EdgeTTS(knockbackTTS);
        }
        else if (@event.ActionId == 45677)
        {
            string attractText = "吸引";
            string attractTTS = "吸引";
        
            if (Sperad == 1)
            {
                attractText += " + 分散";
                attractTTS += " 后分散";
            }
            else if (Stack == 1)
            {
                attractText += " + 分摊";
                attractTTS += " 后分摊";
            }
        
            if (isText) accessory.Method.TextInfo(attractText, duration: 10500, true);
            if (isTTS) accessory.Method.TTS(attractTTS);
            if (isEdgeTTS) accessory.Method.EdgeTTS(attractTTS);
        }
        else
        {
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 超增压播报错误");
        }
    }
    
    [ScriptMethod(name:"分散分摊重置", eventType:EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^4567[07]$"], userControl:false)]
    public void 分散分摊重置(Event @event, ScriptAccessory accessory) 
    {
        // 在 【超增压急行】 或 【超增压抽雾】判定后，重置
        Sperad = 0;
        Stack  = 0;
    }
    
    /*
    
    [ScriptMethod(name: "前照光 / 雷鸣吐息（上下AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^456(87|90)]$"])]
    public void 前照光_雷鸣吐息(Event @event, ScriptAccessory accessory)
    {
        if (@event.ActionId == 45687)
        {
            if (isTTS)accessory.Method.TTS($"上");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"上");
        }
        else if (@event.ActionId == 45690)
        {
            if (isTTS)accessory.Method.TTS($"下");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"下");
        }
        else
        {
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 上下判断错误");
        }
    }
    
    */
    
    [ScriptMethod(name: "护卫炮塔_雷转质射线（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4568[1-3]$"])]
    public void 护卫炮塔_雷转质射线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"护卫炮塔_雷转质射线{@event.SourceId}";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6700;
        
        switch (@event.ActionId())
        {
            case 45681:
                dp.Scale = new (5f, 25f);
                break;
            case 45682:
                dp.Scale = new (5f, 10f); // 表格是长25，但是日本人的Spl画的10，先信日本人.jpg
                break;
            case 45683:
                dp.Scale = new (5f, 20f); 
                break;
        }

        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "雷光一闪（旋风）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45666"])]
    public void 雷光一闪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "雷光一闪";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(4f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "雷光雨（魔法阵钢铁最终位置）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45713"])]
    public void 雷光雨(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "雷光雨";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16f);
        dp.DestoryAt = 2400;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"雷光雨描边";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(16f);
        dp1.InnerScale = new Vector2(15.8f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 2400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    
    
    
    #endregion
    
    #region M9N
    
    [ScriptMethod(name: "—————— M9N ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void M9N(Event @event, ScriptAccessory accessory) { }
    
    
    #endregion
    
    #region M10N
    
    [ScriptMethod(name: "—————— M10N ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void M10N(Event @event, ScriptAccessory accessory) { }
    
    
    #endregion
    
    #region M11N
    
    [ScriptMethod(name: "—————— M11N ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void M11N(Event @event, ScriptAccessory accessory) { }
    
    
    #endregion
    
    #region M12N
    
    [ScriptMethod(name: "—————— M12N ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void M12N(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "巨蟒绞缠（缠绕钢铁）判定时间", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46198"])]
    public void 巨蟒绞缠(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"巨蟒绞缠";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 39700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "灾变吐息（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:47547"])]
    public void 灾变吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"灾变吐息";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (20f, 30f);
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "极饿伸展（扇形）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:46189"])]
    public void 极饿伸展(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"极饿伸展";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(35f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 10300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "大爆炸（屎块钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46191"])]
    public void 大爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"大爆炸";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    /*
    [ScriptMethod(name: "盛大登场（拆地板蛇出现）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4620[23]$"])]
    public void 盛大登场(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"盛大登场";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (2.5f, 15f);
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    */
    
    [ScriptMethod(name: "震场（拆地板）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46205"])]
    public void 震场(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"震场";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (10f, 15f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "分裂灾变（蛇头直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46207"])]
    public void 分裂灾变(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"分裂灾变";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (10f, 30f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "爆风细胞（击退距离预测）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^474[78]$"])]
    public void 爆风细胞(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = @event.StatusId==4747 ? "爆风细胞前" : "爆风细胞后";
        dp.Scale = new(1f,15f);
        dp.Owner = accessory.Data.Me;
        dp.Rotation = @event.StatusId==4747 ? float.Pi *2 : float.Pi;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.DestoryAt = 7000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "细胞失控（本体直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:48090"])]
    public void 细胞失控(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"细胞失控";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (8f, 30f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "细胞失控（直线2）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:48091"])]
    public void 细胞失控2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"细胞失控2";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (8f, 30f);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "细胞失控（大直线预兆）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:48095"])]
    public void 细胞失控大(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"细胞失控大";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.Scale = new (35f, 0.5f);
        dp.DestoryAt = 15800;
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

#region 特殊函数
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
#endregion 特殊函数
