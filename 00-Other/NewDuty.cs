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

[ScriptType(guid: "80890eac-4730-4708-ad1b-05aba469c2a1", name: "最新最热临时绘制", territorys: [1307, 1346, 1339, 1340, 1341, 1342, 1343],
    version: "0.0.2.2", author: "Tetora", note: noteStr)]

/* MapID
 * 1307: 格莱杨拉波尔歼灭战
 * 1346：新月岛北征
 * 1339~1343 斗兽奇弈
 */

public class NewDuty
{
    const string noteStr =
        """
        v0.0.2.2:
        最新最热副本绘制，可能会电，介意请关闭
        别人的正式版发了这边就删
        """;
    
    #region 用户控制

    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("下一次AOE预兆颜色")]
    public ScriptColor Next_AOEs { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
    
    [UserSetting("下一次AOE预兆亮度（推荐小于1）")]
    public float Next_AOEsBrightness { get; set; } = 0.5f;
    
    // dp.Color = Next_AOEs.V4.WithW(Next_AOEsBrightness);
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;

    #endregion
    
    #region 全局销毁
    
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
    
    uint WildSpeed = 0;
    
    public void Init(ScriptAccessory accessory) {
        WildSpeed = 0; // 曼提克-猛冲重锤
    }
    
    #region 斗兽奇弈
    
    [ScriptMethod(name: "百虫肤防击退销毁", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^4620$"],userControl: false)]
    public void 百虫肤防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw(".*击退.*");
    }
    
    [ScriptMethod(name: "—————— 斗兽奇弈 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 斗兽奇弈(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "奇子·主教_死亡螺旋（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46868$"])]
    public void 主教_死亡螺旋(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"靠近主教月环", duration: 5000, true);
        if (isTTS)accessory.Method.TTS($"靠近主教月环");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"死亡螺旋{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.InnerScale = new Vector2(4f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "奇子·骑士_骨化（打断提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46871$"])]
    public void 骑士_骨化(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"打断骑士伤害提高（控制或借用-咒具碎魂）", duration: 7000, false);
        if (isTTS)accessory.Method.TTS($"打断骑士");
    }
    
    [ScriptMethod(name: "奇子·骑士_物理伤害提高（驱散提示）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^2074$"])]
    public void 骑士_物理伤害提高(Event @event, ScriptAccessory accessory)
    {
        if (HelperExtensions.GetCurrentTerritoryId() != 1339) return;
        if (isText)accessory.Method.TextInfo($"驱散骑士（借用-水栖波）", duration: 5000, false);
        if (isTTS)accessory.Method.TTS($"驱散骑士");
    }
    
    [ScriptMethod(name: "奇子·主教_古代疾风（面前直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46870$"])]
    public void 主教_古代疾风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"古代疾风{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (8f, 40f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "奇子·骑士_前线护卫（魔兽吸引注意提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46864$"])]
    public void 骑士_前线护卫(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"使用魔兽技 [吸引注意]", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"使用魔兽吸引注意");
    }
    
    [ScriptMethod(name: "奇子·骑士_古墓（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46866$"])]
    public void 骑士_古墓(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"远离骑士");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"古墓{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "奇子·上级恶魔_深渊贯穿（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4688(2|4)$"])]
    public void 上级恶魔_深渊贯穿(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深渊贯穿{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = @event.ActionId() == 46882 ? new Vector2(6f) : new Vector2(3f);
        dp.DestoryAt = @event.ActionId() == 46882 ? 3400 : 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "奇子·上级恶魔_深渊回转（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46886$"])]
    public void 上级恶魔_深渊回转 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深渊回转{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "奇子·夺灵魔_澄澈之心（驱散提示）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^1225$"])]
    public void 夺灵魔_澄澈之心(Event @event, ScriptAccessory accessory)
    {
        if (HelperExtensions.GetCurrentTerritoryId() != 1339) return;
        if (isText)accessory.Method.TextInfo($"驱散伤害提高（借用-水栖波）", duration: 5000, false);
        if (isTTS)accessory.Method.TTS($"驱散");
    }
    
    [ScriptMethod(name: "奇子·夺灵魔_魔力冲击（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46899$"])]
    public void 夺灵魔_魔力冲击(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"驱散伤害提高（借用-水栖波）", duration: 7000, false);
        if (isTTS)accessory.Method.TTS($"AOE");
    }
    
    [ScriptMethod(name: "奇子·夺灵魔_虚空暴风（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46897$"])]
    public void 夺灵魔_虚空暴风(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"靠近月环", duration: 5000, true);
        if (isTTS)accessory.Method.TTS($"靠近月环");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"虚空暴风{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "奇子·祸蛛蝎_地面隆起（地震钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46902$"])]
    public void 祸蛛蝎_地面隆起(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"扩散地震");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"地面隆起{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "奇子·祸蛛蝎_致命尾刺（带毒死刑TTS）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46906$"])]
    public void 祸蛛蝎_致命尾刺(Event @event, ScriptAccessory accessory)
    {
        // 回头得再看看毒耐的StatusID!
        if (isText)accessory.Method.TextInfo($"带毒死刑，可用 <死尸净化> 驱散", duration: 4000, false);
        if (isTTS)accessory.Method.TTS($"带毒死刑");
    }
    
    [ScriptMethod(name: "奇子·夺灵魔_虚空冰封（步进地火TTS）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46887$"])]
    public void 夺灵魔_虚空冰封(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"步进地火，二穿一", duration: 2000, true);
        if (isTTS)accessory.Method.TTS($"准备穿地火");
    }
    
    [ScriptMethod(name: "奇子·夺灵魔_虚无耀星（核爆）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46895$"])]
    public void 夺灵魔_虚无耀星(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"远离核爆", duration: 5000, false);
        if (isTTS)accessory.Method.TTS($"远离核爆");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"虚无耀星{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.DestoryAt = 5700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "奇子·祸蛛蝎_毒蛛网（九连环TTS）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46907$"])]
    public void 祸蛛蝎_毒蛛网(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"九连环", duration: 2000, true);
        if (isTTS)accessory.Method.TTS($"九连环");
    }
    
    [ScriptMethod(name: "奇子·食人魔_燃烧猛击（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46912|49688)$"])]
    public void 食人魔_燃烧猛击 (Event @event, ScriptAccessory accessory)
    {
        if (isTTS & @event.ActionId() == 49688) accessory.Method.TTS($"去对侧");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"燃烧猛击{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = @event.ActionId() == 46912 ? 5700 : 9000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "奇子·食人魔_火球生成（TTS）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46921$"])]
    public void 食人魔_火球生成(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"躲避连线火球", duration: 3000, true);
        if (isTTS)accessory.Method.TTS($"躲避连线火球");
    } 
    
    [ScriptMethod(name: "奇子·食人魔_火球（追踪钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^19343$"])]
    public void 食人魔_火球(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"火球延烧{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    } 
    
    [ScriptMethod(name: "火球延烧触发销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^46922$"],userControl: false)]
    public void 火球延烧销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"火球延烧.*");
    }
    
    [ScriptMethod(name: "火球消失销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:regex:^19343$"],userControl: false)]
    public void 火球消失销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"火球延烧.*");
    }
    
    [ScriptMethod(name: "魅惑女妖 帕德索_血雨（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46926$"])]
    public void 帕德索_血雨钢铁(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"远离", duration: 5000, true);
        if (isTTS)accessory.Method.TTS($"远离");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"血雨钢铁{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "魅惑女妖 帕德索_血雨（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46924$"])]
    public void 帕德索_血雨(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"靠近月环", duration: 5000, true);
        if (isTTS)accessory.Method.TTS($"靠近月环");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"血雨月环{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "魅惑女妖 帕德索_虚空烈风（面前直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46932$"])]
    public void 帕德索_虚空烈风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"虚空烈风{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (8f, 60f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "魅惑女妖 帕德索_寒毒接触（带毒死刑TTS）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46935$"])]
    public void 帕德索_寒毒接触(Event @event, ScriptAccessory accessory)
    {
        // 回头得再看看毒耐的StatusID!
        if (isText)accessory.Method.TextInfo($"带毒死刑，可用 <死尸净化> 驱散", duration: 4000, false);
        if (isTTS)accessory.Method.TTS($"带毒死刑");
    }
    
    [ScriptMethod(name: "奇子·梦魔法师_盲信（打断提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^46928$"])]
    public void 梦魔法师_盲信(Event @event, ScriptAccessory accessory)
    {
        // 赋予BOSS 伤害提高 StatusID 1225
        if (isText)accessory.Method.TextInfo($"打断 <梦魔法师> 伤害提高（借用-咒具碎魂）", duration: 5000, false);
        if (isTTS)accessory.Method.TTS($"打断梦魔法师");
    }
    
    // 46936 欺瞒雾 附加噩梦？ 召唤两个连线爱心[信息素 DataId:19347] 随后释放 24m钢铁[46938 碎裂]
    
    // 第二盘
    
    [ScriptMethod(name: "奇子·曼提克_臂锤（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4812(3|5)$"])]
    public void 曼提克_臂锤(Event @event, ScriptAccessory accessory)
    {
        var isR = @event.ActionId == 48123;

        if (isR)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"臂锤_右{@event.SourceId}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(30);
            dp.Radian = 180f.DegToRad(); 
            dp.Rotation = 270f.DegToRad();
            dp.DestoryAt = 5300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        else
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"臂锤_左{@event.SourceId}";
            dp1.Color = accessory.Data.DefaultDangerColor;
            dp1.Owner = @event.SourceId();
            dp1.Scale = new Vector2(30f);
            dp1.Radian = 180f.DegToRad(); 
            dp1.Rotation = 90f.DegToRad();
            dp1.DestoryAt = 5300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
        }
    }
    
    [ScriptMethod(name: "奇子·曼提克_重锤跃击（跳跃钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48137$"])]
    public void 曼提克_重锤跃击(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"远离跳跃落点");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"重锤跃击{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.DestoryAt = 7800;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"重锤跃击描边{@event.SourceId}";
        dp1.Color = new Vector4(1f, 1f, 0f, 10f);
        dp1.Scale = new(30f);
        dp1.InnerScale = new(29.96f);
        dp1.Radian = float.Pi * 2;
        dp1.Owner = @event.SourceId();
        dp1.DestoryAt = 7800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "奇子·曼提克_利爪凶尾（前后刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4814(0|2)$"])]
    public void 曼提克_利爪凶尾(Event @event, ScriptAccessory accessory)
    {
        var isF = @event.ActionId == 48140;

        if (isF)
        {
            if (isText)accessory.Method.TextInfo($"后 > 前", duration: 6700, true);
            if (isTTS)accessory.Method.TTS($"后，然后前");
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"利爪凶尾_前{@event.SourceId}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(40f);
            dp.Radian = 180f.DegToRad(); 
            dp.Rotation = 0f.DegToRad();
            dp.DestoryAt = 3600;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        else
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"利爪凶尾_后{@event.SourceId}";
            dp1.Color = accessory.Data.DefaultDangerColor;
            dp1.Owner = @event.SourceId();
            dp1.Scale = new Vector2(40f);
            dp1.Radian = 180f.DegToRad(); 
            dp1.Rotation = 180f.DegToRad();
            dp1.DestoryAt = 2100;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
        }
    }
    
    [ScriptMethod(name: "奇子·曼提克_凶尾利爪（前后刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5041(1|3)$"])]
    public void 曼提克_凶尾利爪(Event @event, ScriptAccessory accessory)
    {
        var isF = @event.ActionId == 50413;

        if (isF)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"凶尾利爪_前{@event.SourceId}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(40f);
            dp.Radian = 180f.DegToRad(); 
            dp.Rotation = 0f.DegToRad();
            dp.DestoryAt = 2100;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        else
        {
            if (isText)accessory.Method.TextInfo($"前 > 后", duration: 6700, true);
            if (isTTS)accessory.Method.TTS($"前，然后后");
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"凶尾利爪_后{@event.SourceId}";
            dp1.Color = accessory.Data.DefaultDangerColor;
            dp1.Owner = @event.SourceId();
            dp1.Scale = new Vector2(40f);
            dp1.Radian = 180f.DegToRad(); 
            dp1.Rotation = 180f.DegToRad();
            dp1.DestoryAt = 3600;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
        }
    }

    [ScriptMethod(name: "奇子·曼提克_狂野冲锋（连续冲锋）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48128$"])]
    public void 曼提克_狂野冲锋(Event @event, ScriptAccessory accessory)
    {
        if (@event.ActionId == 48128)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"狂野冲锋{@event.SourceId}";
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
            dp.Owner = @event.SourceId();
            dp.TargetPosition = @event.EffectPosition;
            dp.ScaleMode = ScaleMode.YByDistance;
            dp.Scale = new(8f);
            dp.DestoryAt = 8800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        WildSpeed = 1;
    }
    
    [ScriptMethod(name: "奇子·曼提克_猛冲重锤（冲锋左右刀）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(2056|2193)$"],suppress:9700)]
    public void 曼提克_猛冲重锤(Event @event, ScriptAccessory accessory)
    {
        var isR = @event.StatusId == 2056; // Param分别为: 2056:1044 ; 2193:1043
        // 右刀 ActionId 48132, 左刀 48134, 固定为反方向二连
        
        if (WildSpeed != 1) return;
        
        if (isText) accessory.Method.TextInfo (isR? $"最后：左 > 右" : "最后：右 > 左", duration: 20300, true);
        if (isTTS) accessory.Method.TTS (isR? $"最后：左穿右" : "最后：右穿左");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = isR ? $"臂锤_右{@event.SourceId}" : $"臂锤_左{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.Delay = 16600;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = isR ? $"臂锤_左{@event.SourceId}" : $"臂锤_右{@event.SourceId}";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.TargetId();
        dp1.Scale = new Vector2(30f);
        dp1.Radian = 180f.DegToRad(); 
        dp1.Rotation = isR ? 90f.DegToRad() : 270f.DegToRad();
        dp1.Delay = 18600;
        dp1.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);

        WildSpeed = 0;
    }
    
    [ScriptMethod(name: "奇子·双足飞龙_台风（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48168$"])]
    public void 双足飞龙_台风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"台风击退预测{@event.SourceId}";
        dp.Scale = new(1f, 10);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "奇子·双足飞龙_炽热弥漫（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48175$"])]
    public void 双足飞龙_炽热弥漫 (Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"去背后");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"炽热弥漫{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "奇子·双足飞龙_风暴轨迹（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48178$"])]
    public void 双足飞龙_风暴轨迹 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"风暴轨迹{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "奇子·牛头魔_致命射线（死宣提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48143$"])]
    public void 牛头魔_致命射线(Event @event, ScriptAccessory accessory)
    {
        // EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^5421$"]
        // 虽然这副本有死宣抗性但是万一没有的话早踩早解除，还是用读条播报。地板为EnvControl但是没什么用感觉没必要写
        if (isText)accessory.Method.TextInfo($"踩在地板上蓄力8秒解除死宣", duration: 8700, true);
        if (isTTS)accessory.Method.TTS($"踩地板");
    }
    
    [ScriptMethod(name: "奇子·牛头魔_中心破灭（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^481(55|59|65)$"])]
    public void 牛头魔_中心破灭(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"中心破灭{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        switch (@event.ActionId())
        {
            case 48155:  // 普通单钢铁
                dp.Delay = 0;
                dp.DestoryAt = 5700;
                break;
            case 48159:  // 双重先钢铁
                dp.Delay = 5700;
                dp.DestoryAt = 3500;
                break;
            case 48165:  // 双重后钢铁
                dp.Delay = 0;
                dp.DestoryAt = 5700;
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "奇子·牛头魔_外环破灭（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^481(57|61|63)$"])]
    public void 牛头魔_外环破灭(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"外环破灭{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        switch (@event.ActionId())
        {
            case 48157:  // 普通单月环
                dp.Delay = 0;
                dp.DestoryAt = 5700;
                break;
            case 48161:  // 双重后月环
                dp.Delay = 5700;
                dp.DestoryAt = 3500;
                break;
            case 48163:  // 双重先月环
                dp.Delay = 0;
                dp.DestoryAt = 5700;
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }

    [ScriptMethod(name: "奇子·牛头魔_暗黑射线（点名放球）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^00EA$"])]
    public void 牛头魔_暗黑射线(Event @event, ScriptAccessory accessory)
    {
        if (HelperExtensions.GetCurrentTerritoryId() == 1340 && @event.TargetId() == accessory.Data.Me)
        {
            // if (isText)accessory.Method.TextInfo($"放球点名", duration: 4000, true);
            if (isTTS) accessory.Method.TTS($"放球点名");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"暗黑射线{@event.SourceId}";
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
            dp.Owner = @event.TargetId();
            dp.Scale = new Vector2(6f);
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "奇子·牛头魔_以太波（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48150$"])]
    public void 牛头魔_以太波(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"以太波{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (10f, 50f);
        dp.Delay = 2800;
        dp.DestoryAt = 2900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "奇子·虚灵法师_死亡驱动（地板圈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48182$"])]
    public void 虚灵法师_死亡驱动(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"死亡驱动{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Position = @event.EffectPosition;
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "奇子·虚灵法师_暗天球（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48186$"])]
    public void 虚灵法师_暗天球(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"远离", duration: 5000, true);
        if (isTTS)accessory.Method.TTS($"远离");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"暗天球{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(18f);
        dp.DestoryAt = 7700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"暗天球描边{@event.SourceId}";
        dp1.Color = new Vector4(1f, 1f, 0f, 10f);
        dp1.Scale = new(18f);
        dp1.InnerScale = new(17.95f);
        dp1.Radian = float.Pi * 2;
        dp1.Owner = @event.SourceId();
        dp1.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "奇子·牛魔兄弟_千百一十吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4819(4|6)$"])]
    public void 牛魔哥弟_千百一十吨回转(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = @event.ActionId == 48194 ? $"千百一十吨回转Fast{@event.SourceId}" : $"千百一十吨回转Slow{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(23f);
        dp.Delay = @event.ActionId == 48194 ? 0 : 7700;
        dp.DestoryAt = @event.ActionId == 48194 ? 7700 : 6000;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "奇子·牛魔兄弟_千吨横扫（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4819(0|2)$"])]
    public void 牛魔哥弟_千吨横扫(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = @event.ActionId == 48190 ? $"千吨横扫Fast{@event.SourceId}" : $"千吨横扫Slow{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (60f, 60f);
        dp.Delay = @event.ActionId == 48190 ? 0 : 6700;
        dp.DestoryAt = @event.ActionId == 48190 ? 6700 : 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "奇子·牛魔兄弟_十吨重踏 冲击波（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48(199|202)$"])]
    public void 牛魔兄弟_十吨重踏_冲击波(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = @event.ActionId == 48199 ? $"冲击波击退Fast{@event.SourceId}" : $"冲击波击退Slow{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(4f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.Scale = new Vector2(1f, 20f);
        dp.Delay = @event.ActionId == 48199 ? 0 : 6700;
        dp.DestoryAt = @event.ActionId == 48199 ? 6700 : 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

        if (@event.ActionId == 48199) 
        {        
            IntPtr omenHandle = accessory.Method.VfxMethod.CreateOmen(203, new Vector3(60f),
                @event.SourcePosition(), @event.SourceRotation(), new Vector4(1f,1f,0f,0.4f), 6700);
            accessory.Method.VfxMethod.SetVfxSpeed(omenHandle,1f);
        }

    }
    
    [ScriptMethod(name: "奇子·牛魔老弟_声援（打断提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48203$"])]
    public void 牛魔老弟_声援(Event @event, ScriptAccessory accessory)
    {
        // 伤害提高 StatusID 1225
        if (isText)accessory.Method.TextInfo($"打断 <牛魔老弟> 伤害提高（借用-咒具碎魂）", duration: 6000, false);
        if (isTTS)accessory.Method.TTS($"打断牛魔老弟");
    }

    [ScriptMethod(name: "奇子·牛魔老哥_百吨挥打（直线死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48204$"])]
    public void 牛魔老哥_百吨挥打(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"百吨挥打{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId;
        dp.Scale = new (8f, 65f);
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    // 48206 无尽回转 读条完变成不可选中状态，持续追踪玩家释放8m钢铁 48207
    // 48210 无尽横扫 持续顺/逆时针顺劈 组合技，共约24.7s，转向后会有0.2s的读条，需要提前判断顺逆时针绘制，目前仅为模板
    
    [ScriptMethod(name: "奇子·牛魔老弟_无尽横扫（大风车）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48210$"])]
    public void 牛魔老弟_无尽横扫 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"无尽横扫{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = 24700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "奇子·牛魔兄弟_无尽挥打（软狂暴）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48214$"])]
    public void 牛魔兄弟_无尽挥打(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"[软狂暴] 大AOE伤害", duration: 7000, true);
        if (isTTS)accessory.Method.TTS($"大AOE伤害");
    }
    
    [ScriptMethod(name: "小地豆_黄沙吐息（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48227$"])]
    public void 小地豆_黄沙吐息 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"黄沙吐息{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "路斯福洛克斯_哥布流回旋剑（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48229$"])]
    public void 路斯福洛克斯_哥布流回旋剑钢铁(Event @event, ScriptAccessory accessory)
    { 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"哥布流回旋剑钢铁{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "路斯福洛克斯_小地豆肤（驱散提示）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^5423$"])]
    public void 路斯福洛克斯_小地豆肤(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"驱散BOSS（借用-水栖波）", duration: 5000, false);
        if (isTTS)accessory.Method.TTS($"驱散BOSS");
    }
    
    [ScriptMethod(name: "路斯福洛克斯_哥布炸弹（提示）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^19561$"],suppress: 1000)]
    public void 路斯福洛克斯_哥布炸弹(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"将 <哥布炸弹> 推进流沙", duration: 5000, true);
        if (isTTS)accessory.Method.TTS($"将炸弹推进流沙");
    }
    
    [ScriptMethod(name: "路斯福洛克斯_哥布流回旋剑（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48231$"])]
    public void 路斯福洛克斯_哥布流回旋剑月环(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"靠近月环", duration: 5000, true);
        if (isTTS)accessory.Method.TTS($"靠近月环");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"路斯福洛克斯_哥布流回旋剑月环{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.InnerScale = new Vector2(4f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "小地豆_地震（软狂暴）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48239$"])]
    public void 小地豆_地震(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"[软狂暴] 大AOE伤害", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"大AOE伤害");
    }
    
    // 第三盘
    
    [ScriptMethod(name: "奇子·游侠骑士_钢铁裂斩（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48473$"])]
    public void 游侠骑士_钢铁裂斩 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"钢铁裂斩{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 130f.DegToRad(); 
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "奇子·游侠骑士的幻影_收割者的恐吓（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48464$"])]
    public void 游侠骑士的幻影_收割者的恐吓(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"收割者的恐吓{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = 5700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"收割者的恐吓描边{@event.SourceId}";
        dp1.Color = new Vector4(1f, 1f, 0f, 10f);
        dp1.Scale = new(20f);
        dp1.InnerScale = new(19.95f);
        dp1.Radian = float.Pi * 2;
        dp1.Owner = @event.SourceId();
        dp1.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "奇子·游侠骑士的幻影_残杀（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48462$"])]
    public void 游侠骑士的幻影_残杀(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"残杀{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (8f, 60f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "奇子·游侠骑士_粉碎剑（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48471$"])]
    public void 游侠骑士_粉碎剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"粉碎剑击退预测{@event.SourceId}";
        dp.Scale = new(1f, 15);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "奇子·鱼人_麻痹尖刺（反击提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50532$"])]
    public void 鱼人_麻痹尖刺(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"停止攻击 <鱼人>", duration: 6000, true);
        if (isTTS)accessory.Method.TTS($"停止攻击鱼人");
    }
    
    [ScriptMethod(name: "奇子·鱼人_恐慌洗礼（打断提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48485$"])]
    public void 鱼人_恐慌洗礼(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"使用 <咒具碎魂> 打断鱼人", duration: 7000, true);
        if (isTTS)accessory.Method.TTS($"打断鱼人");
    }
    
    [ScriptMethod(name: "奇子·鱼人_大海啸（击退安全区）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48481$"])]
    public void 鱼人_大海啸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"大海啸击退安全区{@event.SourceId}";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (40f, 7f); // 击退距离为35m，场地宽40m，电网2m
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "奇子·尤弥尔_万雷（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48479$"])]
    public void 提示(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"AOE", duration: 4000, false);
        if (isTTS)accessory.Method.TTS($"AOE");
    }
    
    [ScriptMethod(name: "奇子·卡托布莱帕斯_魔兽咆哮（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48504$"])]
    public void 卡托布莱帕斯_魔兽咆哮(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"AOE", duration: 2000, false);
        if (isTTS)accessory.Method.TTS($"AOE");
    }
    
    // 钢铁球 DataId 19612, 读条48506 ; 月环球 DataId 19613 , 读条48508。 钢铁+月环22飞的情况按开始飞动顺序沿固定直线飞行到场边后释放
    // 魔眼移植 StatusID 2056 , Param 174; 带背对的情况固定一个个轮流来
    // [Tether 00C3] [VFX Channeling 195] vfx/channeling/eff/chn_ice_mouth01x.avfx / vfx/common/eff/mon_status01et.avfx
    
    [ScriptMethod(name: "奇子·祖_十字风（指路）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48491$"])]
    public void 祖_十字风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"十字风指路{@event.SourceId}";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor.WithW(1f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = new Vector3(120f, 0f, -420f);
        dp.Scale = new(0.5f);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "奇子·巨像_岩石崩溃（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48555$"])]
    public void 巨像_岩石崩溃(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"岩石崩溃{@event.SourceId}";
        dp.Color = new Vector4(1f, 0f, 0f, 2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new (10f, 45f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "沙球_爆炸（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48562$"])]
    public void 沙球_爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"沙球_爆炸{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region 蜃景幻界新月岛 北征之章

    [ScriptMethod(name: "—————— 蜃景幻界新月岛 北征之章 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 蜃景幻界新月岛北征之章(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "[FATE] 忍耐基路伯_圆形魔法剑（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5011[89]$"])]
    public void 忍耐基路伯_圆形魔法剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"忍耐基路伯_圆形魔法剑{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[FATE] 忍耐基路伯_环形魔法剑（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5012[01]$"])]
    public void 忍耐基路伯_环形魔法剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"忍耐基路伯_环形魔法剑{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.InnerScale = new Vector2(12f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[FATE] 忍耐基路伯_神圣（步进地火）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49913$"])]
    public void 忍耐基路伯_神圣(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"忍耐基路伯_神圣{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[FATE] 妖艳魔花珊迪_藤条抽打（四向）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4894[567]$"])]
    public void 妖艳魔花珊迪_藤条抽打(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"妖艳魔花珊迪_藤条抽打{@event.SourceId}";
        dp.Scale = new (6f, 60f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.DestoryAt = @event.ActionId() == 48947 ? 2700 : 4700;
        
        float[] rotations = { 45f, 135f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    
    [ScriptMethod(name: "[FATE] 妖艳魔花珊迪_毒气团 飞散（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48950$"])]
    public void 毒气团_飞散(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"毒气团_飞散{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[FATE] 邪瞳_诅咒目光（背对连线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47152$"])]
    public void 邪瞳_诅咒目光背对连线(Event @event, ScriptAccessory accessory)
    {
        if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 测试成功");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"邪瞳_诅咒目光背对连线{@event.SourceId}";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(2);
        dp.Delay = 1800;
        dp.DestoryAt = 2900;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"邪瞳_诅咒目光判定时间{@event.SourceId}";
        dp1.Color = new Vector4(1f, 0f, 0f, 2f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(2f);
        dp1.DestoryAt = 4700;
        dp1.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
    }
    
    [ScriptMethod(name: "[FATE] 邪瞳_诅咒目光（背对范围）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47152$"])]
    public void 邪瞳_诅咒目光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"邪瞳_诅咒目光{@event.SourceId}";
        dp.Color = new Vector4(1f, 0f, 0f, 10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(49.8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[FATE] 遗迹冰狼_内吹雪（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49756$"])]
    public void 遗迹冰狼_内吹雪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"内吹雪{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[FATE] 遗迹冰狼_外吹雪（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49757$"])]
    public void 遗迹冰狼_外吹雪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"外吹雪{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.InnerScale = new Vector2(10f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[FATE] 新月女王_王权闪光（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49494$"])]
    public void 新月女王_王权闪光顺劈 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"新月女王_王权闪光{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[FATE] 新月女王_王权闪光（扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49495$"])]
    public void 新月女王_王权闪光扫尾 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"新月女王_王权闪光{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Rotation = 180f.DegToRad();
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[FATE] 统领奇美拉_寒冰咆哮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48633$"])]
    public void 统领奇美拉_寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"寒冰咆哮{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    /* 暂时废弃
    
    [ScriptMethod(name: "[FATE] 雷球_寒冰咆哮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48635$"])]
    public void 雷球_寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"寒冰咆哮{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12);
        dp.DestoryAt = 700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    */
    
    [ScriptMethod(name: "[FATE] 统领奇美拉_雷电咆哮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48634$"])]
    public void 统领奇美拉_雷电咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"雷电咆哮{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[FATE] 雷球_雷电咆哮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48636$"])]
    public void 雷球_雷电咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"雷电咆哮{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    /* 显示时间不确定，且没看到Remove日志 暂时废弃

    [ScriptMethod(name: "[FATE] 嘈杂的噪音_无序的和声（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^19313$"])]
    public void 嘈杂的噪音_无序的和声(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"无序的和声{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12);
        dp.DestoryAt = 20000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

*/
    
    [ScriptMethod(name: "无序的和声销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:regex:^19313$"], userControl: false)]
    public void 无序的和声销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"无序的和声{@event.TargetId()}");
    }
    
    /* 抓不到 Status 5106?
    
    [ScriptMethod(name: "[FATE] 伊阿姆柏的种子_爆炸（钢铁）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^5106$"])]
    public void 伊阿姆柏的种子_爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"伊阿姆柏的种子_爆炸{@event.TargetId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15);
        dp.DestoryAt = 1800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    */
    
    [ScriptMethod(name: "伊阿姆柏的种子_爆炸销毁", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48033$"], userControl: false)]
    public void 伊阿姆柏的种子_爆炸销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"伊阿姆柏的种子_爆炸{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "[FATE] 缺陷拉米亚_诅咒目光（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48253$"])]
    public void 缺陷拉米亚_诅咒目光 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"缺陷拉米亚_诅咒目光{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 诅咒的继承者——惨白魔人] 咒力的替身_臭气（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49777$"])]
    public void 咒力的替身_臭气 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"咒力的替身_臭气";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.Radian = 100f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 诅咒的继承者——惨白魔人] 咒力的替身_平原震裂（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49779$"])]
    public void 咒力的替身_平原震裂(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"咒力的替身_平原震裂";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 天道好轮回——魔亡灵法师] 古代探险家的亡灵_爆炸（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47175$"])]
    public void 古代探险家的亡灵_爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"古代探险家的亡灵_爆炸";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 天道好轮回——魔亡灵法师] 古代海贼的亡灵_爆炸（十字）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47176$"])]
    public void 古代海贼的亡灵_爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"古代海贼的亡灵_爆炸";
        dp.Scale = new (7f, 80f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.DestoryAt = 3700;
        
        float[] rotations = { 0f, 90f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    
    [ScriptMethod(name: "[CE 天道好轮回——魔亡灵法师] 魔法阵展开_昏暗奔流（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47180$"])]
    public void 魔法阵展开_昏暗奔流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"魔法阵展开_昏暗奔流";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new (12f, 70f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
    }
    
    [ScriptMethod(name: "[CE 禁书化形——古术魔典] 魔力书写_草书（步进地火）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47306$"])]
    public void 魔力书写_草书(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"魔力书写_草书{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new (6f, 50f);
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
    }
    
    [ScriptMethod(name: "[CE 苏醒的多头龙——魔许德拉] 近远冲击波_元素冲击波（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47199|4720[0123])$"])]
    public void 元素冲击波(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"元素冲击波{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 四颚斧花——提蔛] 中央鞭打（辣尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47220$"])]
    public void 中央鞭打(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"中央鞭打";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new (10f, 52f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
    }
    
    [ScriptMethod(name: "[CE 四颚斧花——提蔛] 侧方鞭打（辣翅）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47221|49729)$"])]
    public void 侧方鞭打(Event @event, ScriptAccessory accessory)
    {
        var isR = @event.ActionId == 47221;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"侧方鞭打{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(26f);
        dp.Radian = 180f.DegToRad(); 
        dp.Offset = isR ? new Vector3(5f, 0f, 0f) : new Vector3(-5f, 0f, 0f);
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
    }
    
    [ScriptMethod(name: "[CE 四颚斧花——提蔛] 毒雾喷射（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47228|5054[789])$"])]
    public void 毒雾喷射(Event @event, ScriptAccessory accessory)
    {
        // 猜测：50547 左 ; 50548 下
        // 已验证：47228 正 ; 50549 右
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"毒雾喷射{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 90f.DegToRad(); 
        switch (@event.ActionId())
        {
            case 47228:
                dp.Rotation = 0f.DegToRad();
                break;
            case 50547:
                dp.Rotation = 90f.DegToRad();
                break;
            case 50548:
                dp.Rotation = 180f.DegToRad();
                break;
            case 50549:
                dp.Rotation = 270f.DegToRad();
                break;
        }
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 纯白守护者——雪石膏之剑] 称誉（四连攻击命令_顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47158$"])]
    public void 称誉 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"称誉{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 纯白守护者——雪石膏之剑] 连续斩（二连左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4716[67]$"])]
    public void 连续斩(Event @event, ScriptAccessory accessory)
    {
        var isR = @event.ActionId == 47166;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"连续斩1";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"连续斩2";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(40f);
        dp1.Radian = 180f.DegToRad(); 
        dp1.Rotation = isR ? 90f.DegToRad() : 270f.DegToRad();
        dp1.Delay = 4700;
        dp1.DestoryAt = 2300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }

    [ScriptMethod(name: "[CE 暴食咒鬼——阿尔戈尔] 捕食（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50469$"])]
    public void 捕食 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"捕食";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 6500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "[CE 暴食咒鬼——阿尔戈尔] 吸引（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48104$"])]
    public void 吸引 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"吸引";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 30f.DegToRad(); 
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 暴食咒鬼——阿尔戈尔] 洋葱瘴气（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48112$"])]
    public void 洋葱瘴气 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"洋葱瘴气";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 30f.DegToRad(); 
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 暴食咒鬼——阿尔戈尔] 番茄瘴气（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48111$"])]
    public void 番茄瘴气(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"番茄瘴气";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (6f, 50f);
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
    }
    
    [ScriptMethod(name: "[CE 魔法军团——小小法师] 小小核爆（钢铁预兆）", eventType: EventTypeEnum.StatusAdd, 
        eventCondition: ["StatusID:regex:^3795$","Param:2","TargetName:regex:^(核爆球|フレアスフィア|flare sphere)$"])]
    public void 小小核爆预兆(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"小小核爆预兆{@event.TargetId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(18f);
        dp.DestoryAt = 3500;
        // dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 魔法军团——小小法师] 小小神圣（水击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48312$"])]
    public void 小小神圣(Event @event, ScriptAccessory accessory)
    {
        if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 测试成功");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "小小神圣";
        dp.Scale = new(1f, 15f);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "[CE 魔法军团——小小法师] 立体魔法阵_彗星 连线", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^3445$", "Param:21"])]
    public void 立体魔法阵_彗星(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"立体魔法阵_{@event.TargetId}";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "[CE 魔法军团——小小法师] 小小爆炎（钢铁动画）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48318$"])]
    public void 小小爆炎(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"小小爆炎";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14f);
        dp.DestoryAt = 5700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 魔女复制体——卡洛菲斯提莉二重身] 双重魔发斩（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5069[12]$"])]
    public void 双重魔发斩(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"双重魔发斩{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 180f.DegToRad(); 
        dp.Delay = @event.ActionId() == 50691 ? 0 : 2500;
        dp.DestoryAt = @event.ActionId() == 50691 ? 2500 : 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 魔女复制体——卡洛菲斯提莉二重身] 发牢（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47072$"])]
    public void 发牢(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"发牢{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 魔女复制体——卡洛菲斯提莉二重身] 剪发（冰花）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4707[67]$"])]
    public void 剪发(Event @event, ScriptAccessory accessory)
    {
        if (@event.ActionId() == 47076)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"剪发钢铁 {@event.SourceId}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(10f);
            dp.DestoryAt = 4700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        else if  (@event.ActionId() == 47077)
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"剪发冰花{@event.SourceId}";
            dp1.Scale = new (4f, 120f);
            dp1.Owner = @event.SourceId();
            dp1.Color = accessory.Data.DefaultDangerColor.WithW(1f);
            dp1.DestoryAt = 4700;
        
            float[] rotations = { 0f, 90f };
    
            foreach (float rotation in rotations)
            {
                dp1.Rotation = rotation.DegToRad();
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);
            }
        }
        else
        {
            
        }

    }
    
    [ScriptMethod(name: "[CE 叛逆使魔——负隅宝石兽] 黄宝石射线（贴墙钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48281$"])]
    public void 黄宝石射线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"黄宝石射线{@event.SourceId}";
        dp.Color = new Vector4(1f, 0f, 0f, 5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 叛逆使魔——负隅宝石兽] 利爪凶尾（前后刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48294$"])]
    public void 利爪凶尾 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"利爪凶尾_前";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(45f);
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"利爪凶尾_后";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(45f);
        dp1.Rotation = 180f.DegToRad();
        dp1.Radian = 180f.DegToRad();
        dp1.Delay = 5700;
        dp1.DestoryAt = 3300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }
    
    [ScriptMethod(name: "[CE 叛逆使魔——负隅宝石兽] 凶尾利爪（后前刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48295$"])]
    public void 凶尾利爪 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"凶尾利爪_前";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(45f);
        dp.Rotation = 180f.DegToRad();
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"凶尾利爪_后";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(45f);
        dp1.Radian = 180f.DegToRad();
        dp1.Delay = 5700;
        dp1.DestoryAt = 3300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }
    
    [ScriptMethod(name: "[CE 拟态使魔——变形法师] 圆火（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48341$"])]
    public void 圆火(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"圆火";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 拟态使魔——变形法师] 旋风环（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48354$"])]
    public void 变形法师_旋风环(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"变形法师_旋风环";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.InnerScale = new Vector2(10f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[CE 孤岛的绑架犯——诱拐魔] 天降（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47448$"])]
    public void 天降(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"远离");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天降";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 5200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 孤岛的绑架犯——诱拐魔] 旋风环（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47449$"])]
    public void 诱拐魔_旋风环(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"靠近");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"诱拐魔_旋风环";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[CE 孤岛的绑架犯——诱拐魔] 散羽（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47443$"])]
    public void 散羽(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"散羽";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 孤岛的绑架犯——诱拐魔] 突风（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48250$"])]
    public void 突风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "突风";
        dp.Scale = new(1f, 24);
        dp.Color = new Vector4(0f, 1f, 1f, 4f);
        dp.Owner = accessory.Data.Me;
        dp.Rotation = @event.SourceRotation();
        dp.FixRotation = true;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    

    /* 不准 用不了
    [ScriptMethod(name: "[CE 孤岛的绑架犯——诱拐魔] 撕裂之风（移动冰花）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^01FA$"])]
    public void 撕裂之风预备(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"撕裂之风预备{@event.SourceId}";
        dp.Scale = new (8f, 120f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.DestoryAt = 5100;
        
        float[] rotations = { 52f, 97f, 142f, 187f};
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    */

    [ScriptMethod(name: "[CE 孤岛的绑架犯——诱拐魔] 撕裂之风（冰花读条）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47439$"])]
    public void 撕裂之风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"撕裂之风{@event.SourceId}";
        dp.Scale = new (8f, 120f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.DestoryAt = 700;
        
        float[] rotations = { 0f, 90f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    
    
    #endregion
    
    #region  7.4 格莱杨拉波尔歼灭战
    
    [ScriptMethod(name: "—————— 7.4 格莱杨拉波尔歼灭战 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 格莱杨拉波尔歼灭战(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "雷电爆发 双死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45660"])]
    public void 雷电爆发(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"双死刑");
    }
    
    [ScriptMethod(name: "雷光急行 击退提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45618"])]
    public void 雷光急行(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"击退", duration: 5000, true);
        if (isTTS)accessory.Method.TTS($"击退");
    }
    
    [ScriptMethod(name: "抽雾 吸引提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45625"])]
    public void 抽雾(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"吸引", duration: 5000, true);
        if (isTTS)accessory.Method.TTS($"吸引");
    }
    
    [ScriptMethod(name: "抽雾 吸引（吸引预测）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45625"])]
    public void 抽雾_吸引预测(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "抽雾_吸引预测";
        dp.Scale = new(1f, 19f);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.Rotation = 180f.DegToRad();
        dp.FixRotation = true;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "抽雾吸引防击退销毁", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(160|1209|2663)$"],userControl: false)]
    public void 抽雾吸引防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("抽雾_吸引预测");
    }
        
    [ScriptMethod(name: "排雾 吸引矩形危险区", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45667"])]
    public void 排雾(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "排雾";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(20f, 20f); 
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "无控急行 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45624"])]
    public void 无控急行(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
    }
    
    /* 无法判断高低VFX
    
    [ScriptMethod(name: "雷光环_雷转质射线（直线）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:18993"])]
    public void 雷光环_雷转质射线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "雷光环_雷转质射线";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new(5f, 30f);
        dp.DestoryAt = 7000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    */
    
    [ScriptMethod(name: "护卫炮塔_雷转质射线（直线危险区）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45629|4563[0-3])$"])]
    public void 护卫炮塔_雷转质射线Danger(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "护卫炮塔_雷转质射线";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(5f, 50f); 
        switch (@event.ActionId())
        {
            case 45629:
                dp.Scale = new(5f, 25f); 
                break;
            case 45630:
                dp.Scale = new(5f, 10f); // 实际为25m，但是只有一格高台 不画太多
                break;
            case 45631:
                dp.Scale = new(5f, 20f); 
                break;
            case 45632:
                dp.Scale = new(5f, 15f); 
                break;
            case 45633:
                dp.Scale = new(5f, 10f); 
                break;
        }
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "前照光 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45637"])]
    public void 前照光(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"下安全", duration: 6000, false);
        if (isTTS)accessory.Method.TTS($"下下下");
    }
    
    /* 无法判断VFX高低
    [ScriptMethod(name: "前照光/雷鸣吐息 上下AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4563[57]$"])]
    public void 上下AOE(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "上下AOE";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(20f, 60f); 
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    */
    
    [ScriptMethod(name: "雷鸣吐息 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45635"])]
    public void 雷鸣吐息(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"上安全", duration: 6000, true);
        if (isTTS)accessory.Method.TTS($"上上上");
    }
    
    [ScriptMethod(name: "雷光雨（魔法阵钢铁最终位置）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45659"])]
    public void 雷光雨(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"雷光雨";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16f);
        dp.DestoryAt = 2900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
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
