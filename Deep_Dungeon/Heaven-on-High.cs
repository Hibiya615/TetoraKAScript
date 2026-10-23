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

namespace Heaven_on_High;

[ScriptType(guid: "c63d669e-6a74-4338-a158-09673fabdc8d", name: "天之御柱", 
    territorys: [770,771,772,773,774,775,780,782,783,784,785],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

    // 1~10层: 
    // 11~20层: 
    // 21~30层: 
    // 31~40层: 782
    // 41~50层: 773
    // 51~60层: 783
    // 61~70层: 774
    // 71~80层: 784
    // 81~90层: 775
    // 91~100层: 785
    // 中途待机室：780

public class Heaven_on_High {
    const string noteStr =
        """
        v0.0.0.1:
        天之御柱 / Heaven_on_High / アメノミハシラ
        初版绘制
        """;

    #region 各种记录
    
    /*  StatusID
     *  眩晕 2（如近战扫腿、T下踢）
     *  睡眠 3（如法系催眠、奶妈沉静）
     *  变身 565
     *  诅咒 1087 （拟态怪 - 怨念）
     *  失明 1088
     *  最大体力减少 1089
     *  伤害降低 1090
     *  加速 1091
     *  能力封印 1092
     *  最大体力魔力提升 1093
     *  禁止使用道具  1094
     *  禁止冲刺 1095
     *  击退无效 1096
     *  禁止体力自然恢复 1097
     *  无法发动技能 1113 （形态变化）
     */
    
    #endregion
    
    #region 基础控制
    
    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("启用小工具（已确认设置完毕）")]
    public bool isMiniTools { get; set; } = false;
    
    [UserSetting("设置传送灯笼查找颜色")]
    public ScriptColor teleporter { get; set; } = new() { V4 = new(0f, 1f, 0f, 2f) };
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion

    #region 全局变量控制
    
    [ScriptMethod(name: "伤头&插言 打断销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void 打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "特殊状态销毁", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(2|3|1511|1113)$"], userControl: false)]
    public void 特殊状态销毁(Event @event, ScriptAccessory accessory)
    {
        // 赋予对应怪物 眩晕[2]、睡眠[3]、石化[1511]、(形态变化）无法发动技能[1113] 等状态都需要销毁绘图
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

    public bool KnockPenalty = false;
    
    [ScriptMethod(name: "天气：击退无效添加", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1096"],userControl: false)]
    public void 上击退buff(Event @event, ScriptAccessory accessory)
    {
    KnockPenalty = true;
    }
    
    [ScriptMethod(name: "天气：击退无效移除", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:1096"],userControl: false)]
    public void 下击退buff(Event @event, ScriptAccessory accessory)
    {
    KnockPenalty = false;
    }
    
    #endregion
    
    #region 小工具部分
    
    [ScriptMethod(name: "—————— 小工具部分（先自行关闭不需要的功能） / Tools ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 小工具部分(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "传送灯笼查找 / Teleportation Construct Finder", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2009507", "Operate:Add"])]
    public void 传送装置查找(Event @event, ScriptAccessory accessory)
    {
        if(!isMiniTools) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"传送装置查找";
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new (1f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Color = teleporter.V4;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp); 
    }
    
    [ScriptMethod(name: "抖动的宝箱_怨念提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12313"])]
    public void 抖动的宝箱_怨念(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("打断拟态怪", duration: 2300, true);
        if (isTTS) accessory.Method.TTS("打断拟态怪");
    }
    
    #endregion
    
    #region 1~10层
    
    [ScriptMethod(name: "—————— 1 ~ 10 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第1层(Event @event, ScriptAccessory accessory) { }

    #endregion
    
    #region 11~20层
    
    [ScriptMethod(name: "—————— 11 ~ 20 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第11层(Event @event, ScriptAccessory accessory) { }

    #endregion
    
    #region 21~30层
    
    [ScriptMethod(name: "—————— 21 ~ 30 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第21层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "24~27 天之旗本_天下五剑（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12103$"])]
    public void 天之旗本_天下五剑 (Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之旗本_天下五剑{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f + IbcHelper.GetHitboxRadius(obj)); // 没测目标圈
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "30 蛭子_踏脚（提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^11291$"])]
    public void 蛭子_踏脚(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"站在一朵云下", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"站在一朵云下");
    }

    #endregion
    
    #region 31~40层
    
    [ScriptMethod(name: "—————— 31 ~ 40 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第31层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "40 怖军_龙卷（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^11902$"])]
    public void 怖军_龙卷(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
    }
    
    [ScriptMethod(name: "40 怖军_古代暴风（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^11904$"])]
    public void 怖军_古代暴风(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"击退到安全区", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"击退到安全区");
        
        IntPtr omenHandle = accessory.Method.VfxMethod.CreateOmen(203, new Vector3(50f),
            @event.EffectPosition(), @event.SourceRotation(), new Vector4(1f,1f,0f,0.2f), 5000);
        accessory.Method.VfxMethod.SetVfxSpeed(omenHandle,1f);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"怖军_古代暴风击退预测{@event.SourceId}";
        dp.Scale = new(1f, 25f);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "40 旋风_风压（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^9187$"])]
    public void 旋风_风压(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"旋风_风压{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    #endregion
    
    #region 41~50层
    
    [ScriptMethod(name: "—————— 41 ~ 50 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第41层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "50 牛头_锈爪（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^11919$"])]
    public void 牛头_锈爪 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"牛头_锈爪{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(11.4f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "50 牛头_灾厄之语（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^11920$"])]
    public void 牛头_灾厄之语(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"牛头_灾厄之语{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (6f, 48.4f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "50 牛头_惧乱之眼（背对）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^11922$"])]
    public void 牛头_惧乱之眼(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"背对BOSS", duration: 2300, true);
        if (isTTS)accessory.Method.TTS($"背对BOSS");
    }
    
    [ScriptMethod(name: "50 暗黑气_虚无火花（圆形）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^9194$"])]
    public void 暗黑气_虚无火花(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"暗黑气_虚无火花{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 16100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    #endregion
    
    #region 51~60层
    
    [ScriptMethod(name: "—————— 51 ~ 60 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第51层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "51~54 天之那迦_钙化浓雾（背对顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12397$"])]
    public void 天之那迦_钙化浓雾 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之那迦_钙化浓雾{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.8f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "51~54 天之那迦_凶恶怒号（可打断钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12398$"])]
    public void 天之那迦_凶恶怒号(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之那迦_凶恶怒号{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.25f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(21.8f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "57~59 天之铁臂_电磁学（可打断吸引）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12392$"])]
    public void 天之铁臂_电磁学(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"吸引接钢铁");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之铁臂_电磁学{@event.SourceId}";
        dp.Color = new Vector4(1f, 0f, 1f, 0.15f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(21.4f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "57~59 天之铁臂_旋转挥打（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12392$"])]
    public void 天之铁臂_旋转挥打(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之铁臂_旋转挥打{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5.4f);
        dp.DestoryAt = 4100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "60 诅咒之杖_无尽痛苦（月环预兆）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^9189$"])]
    public void 诅咒之杖_无尽痛苦(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"诅咒之杖_无尽痛苦{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.InnerScale = new Vector2(6f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5100; // 实际上4600ms后就开始读条，给一点omen重合的视觉适应
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }

    [ScriptMethod(name: "60 忍冬_古代核爆（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^11928$"])]
    public void 忍冬_古代核爆(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
    }
    
    #endregion
    
    #region 61~70层
    
    [ScriptMethod(name: "—————— 61 ~ 70 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第61层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "70 犬蛊_猎食爪（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12205$"])]
    public void 犬蛊_猎食爪 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"犬蛊_猎食爪{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "70 犬蛊_狂吠（出圈提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12208$"])]
    public void 犬蛊_狂吠(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"AOE，离开缩小圈", duration: 2300, true);
        if (isTTS)accessory.Method.TTS($"离开缩小圈");
    }
    
    [ScriptMethod(name: "70 犬蛊_地狱冲（进圈提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12206$"])]
    public void 犬蛊_地狱冲(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo($"进入缩小圈", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"进入缩小圈");
    }

    #endregion
    
    #region 71~80层
    
    [ScriptMethod(name: "—————— 71 ~ 80 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第71层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "74~77 天之长毛象_史前咆哮 AOE提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12220$"])]
    public void 天之长毛象_史前咆哮(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"大象AOE");
    }
    
    [ScriptMethod(name: "76~79 天之白狮_咆吼 眩晕提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12265$"])]
    public void 天之白狮_咆吼(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"眩晕{@event.TargetName()}");
    }
    
    [ScriptMethod(name: "80 冰块_碎裂（圆形）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^9197$"])]
    public void 冰块_碎裂(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"冰块_碎裂{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Owner = @event.SourceId;
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 16200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "冰块碎裂销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^11992$"],userControl: false)]
    public void 冰块碎裂销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"冰块_碎裂{@event.SourceId}");
    }
    
    [ScriptMethod(name: "80 锻冶媪_月下咆哮（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^11987$"])]
    public void 锻冶媪_月下咆哮(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"AOE，远离所有冰块", duration: 2700, true);
        if (isTTS)accessory.Method.TTS($"AOE，远离所有冰块");
    }

    #endregion
    
    #region 81~90层
    
    [ScriptMethod(name: "—————— 81 ~ 90 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第81层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "81~84 天之送狼_寒冰咆哮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12431$"])]
    public void 天之送狼_寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之送狼_寒冰咆哮{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9.7f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "81~84 天之送狼_雷电咆哮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12432$"])]
    public void 天之送狼_雷电咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之送狼_雷电咆哮{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(7.5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "82~85 天之牛头_十一吨横扫（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12444$"])]
    public void 天之牛头_十一吨横扫 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之牛头_十一吨横扫{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(38.2f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "84~87 天之龙人_落肘（扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12450$"])]
    public void 天之龙人_落肘 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之龙人_落肘{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "85~88 天之独眼_怒视（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12451$"])]
    public void 天之独眼_怒视(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之独眼_怒视{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (7f, 20.2f);
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "85~88 天之独眼_百吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12452$"])]
    public void 天之独眼_百吨回转(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之独眼_百吨回转{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(11.2f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "86~89 天之罗刹_撕裂利爪（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12508$"])]
    public void 天之罗刹_撕裂利爪 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之罗刹_撕裂利爪{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "90 温罗_古代爆震（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12214$"])]
    public void 温罗_古代爆震(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
    }
    
    [ScriptMethod(name: "90 沙球_地层俯冲（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^9235$"])]
    public void 沙球_地层俯冲(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"沙球_地层俯冲{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Owner = @event.SourceId;
        dp.Scale = new Vector2(9f);
        dp.DestoryAt = 22800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "沙球消失销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:regex:^9235$"], userControl: false)]
    public void 沙球消失销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"沙球_地层俯冲{@event.SourceId()}");
    }


    #endregion
    
    #region 91~100层
    
    [ScriptMethod(name: "—————— 91 ~ 100 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第91层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "91~94 天之肉人（武僧）步法 眩晕提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12270$"])]
    public void 天之肉人武僧_步法(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"眩晕{@event.TargetName()}");
    }
    
    [ScriptMethod(name: "91~94 天之牛头_卅二吨重击（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12293$"])]
    public void 天之牛头_卅二吨重击 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之牛头_卅二吨重击{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(39f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "91~94 天之牛头_百廿八吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^12291$"])]
    public void 天之牛头_百廿八吨回转(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天之牛头_百廿八吨回转{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 4200;
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