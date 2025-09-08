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

namespace the_Palace_of_the_Dead;

[ScriptType(guid: "4210c323-eba4-4d67-a7e7-b90799494729", name: "死者宫殿", author: "Tetora", 
    territorys: [561,562,563,564,565,593,594,595,596,597,598,599,600,601,602,603,604,605,606,607],
    version: "0.0.0.5",note: noteStr)]

public class the_Palace_of_the_Dead
{
    const string noteStr =
        """
        v0.0.0.5:
        死者宫殿绘制
        注：方法设置中的层数仅做分割线效果，并不是批量开关
        出现问题请携带ARR反馈！
        暂未支持【形态变化】【眩晕】【催眠】等限制，有需可以考虑使用Splatoon
        """;
    
    // 眩晕、催眠、无法发动技能（形态变化）等状态都需要销毁绘图
    // 190 BOSS 有待修复的问题
    // 70 BOSS 会同时满足 170 BOSS 或天气的条件 导致触发重复 需要额外判断sid , 170 BOSS 的伤害提高 也与强化自身 冲突 需额外判断sid
    // 1~99层 与 101~199层 大量怪物技能ID重复，需要额外判断sid
    
    #region 各种记录
    
    /*  StatusID
     *  变身 565 [StackCount:42 曼提克 ] [StackCount:43 梦魔 ]
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
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("测试性功能（慎用）")]
    public bool isTest { get; set; } = false;
    
    [UserSetting("启用底裤（需要对应插件与权限）")]
    public bool isHack { get; set; } = false;
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion
    
    #region 全局变量
    
    [ScriptMethod(name: "伤头&插言 打断销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void 打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"深宫假面_强麻痹{@event.TargetId()}");
        accessory.Method.RemoveDraw($"深宫浮灵_强麻痹{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw(".*(击退|吸蚀).*");
    }
    
    public bool KnockPenalty = false;
    
    [ScriptMethod(name: "天气：击退无效添加", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1096"],userControl: false)]
    public void 添加击退buff(Event @event, ScriptAccessory accessory)
    {
        KnockPenalty = true;
    }
    
    [ScriptMethod(name: "天气：击退无效移除", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:1096"],userControl: false)]
    public void 删除击退buff(Event @event, ScriptAccessory accessory)
    {
        KnockPenalty = false;
    }
    
    //190 治疗爆弹怪 计数  注意暂时还没有写重置计数!!
    private int timesRemedyBomb = 0;
    private int Sap = 0;
    public void Init(ScriptAccessory accessory) {
        
        timesRemedyBomb=0;  // 190 治疗爆弹怪 生成计数
        Sap = 0; // 190 地面爆破 读条计数
        
    }
    
    private bool isExplosionInterrupted = false;  // 190层 特大爆炸打断状态
    
    #endregion
    
    // 通用内容
    [ScriptMethod(name: "拟态怪_怨念提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6397"])]
    public void 拟态怪_怨念(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("打断拟态怪", duration: 2300, true);
        if (isTTS) accessory.Method.TTS("打断拟态怪");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断拟态怪");
    }
    
    #region  1~10层 没有东西可以画
    [ScriptMethod(name: "—————— 1 ~ 10 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第1层(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region  11~20层 没有东西可以画
    [ScriptMethod(name: "—————— 11 ~ 20 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第11层(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 21~30层
    [ScriptMethod(name: "—————— 21 ~ 30 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第21层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "地宫弥诺陶洛斯 百一十吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6364", "SourceDataId:5802"])]
    public void 地宫弥诺陶洛斯_百一十吨回转(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"地宫弥诺陶洛斯_百一十吨回转{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.8f); // 表格8m + 目标圈 2.8m
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw( DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    
    [ScriptMethod(name: "地宫斯卡尼特 唧唧咋咋（睡眠钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6365", "SourceDataId:5803"])]
    public void 斯卡尼特_唧唧咋咋(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"地宫斯卡尼特_唧唧咋咋{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 0.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(21.6f);
        dp.DestoryAt = 2200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "30 宁吉兹济达 恐惧迷雾（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6429"])]
    public void 宁吉兹济达_恐惧迷雾(Event @event, ScriptAccessory accessory)
    {
        // BOSS SourceDataId:5804 ，与 130层BOSS 技能ID不同 无需限制
        
        if (isText)accessory.Method.TextInfo("靠近场中BOSS", duration: 5300, true);
        if (isTTS)accessory.Method.TTS("靠近场中BOSS");
        if (isEdgeTTS)accessory.Method.EdgeTTS("靠近场中BOSS");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "宁吉兹济达_恐惧迷雾";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(58.8f);
        dp.InnerScale = new Vector2(4.8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    #endregion
    
    #region  31~40层
    [ScriptMethod(name: "—————— 31 ~ 40 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第31层(Event @event, ScriptAccessory accessory) { }
    

    [ScriptMethod(name: "噩梦鬼鱼 吸蚀（吸引）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6372", "SourceDataId:5811"])]
    public void 噩梦鬼鱼_吸蚀(Event @event, ScriptAccessory accessory)
    {
        if(!KnockPenalty) {
            if(isTTS) accessory.Method.TTS("吸引");
            if(isEdgeTTS) accessory.Method.EdgeTTS("吸引");
        }
        /*
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"噩梦鬼鱼_吸蚀{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);  // 噩梦鬼鱼暂不知道会不会释放钢铁，忽略钢铁绘制
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        */
    }
    
    #endregion
    
    #region  41~50层
    [ScriptMethod(name: "—————— 41 ~ 50 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第41层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "噩梦曼提克 撕裂利爪（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6394", "SourceDataId:5827"])]
    public void 噩梦曼提克_撕裂利爪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties(); 
        dp.Name = $"噩梦曼提克_撕裂利爪{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.2f); // 表格 5m + 目标圈 2.2m
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 1900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    #endregion
    
    #region  51~60层
    [ScriptMethod(name: "—————— 51 ~ 60 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第51层(Event @event, ScriptAccessory accessory) { }
    
    // 不确定这边的 地宫小魔精 (SourceDataId:6180) 是否会使用 冰棘屏障 (ActionId:6943) 
    
    [ScriptMethod(name: "60 深黑骑士 残杀（击退)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7089"])]
    public void 深黑骑士_残杀(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("击退到安全区", duration: 3300, true);
        if (isTTS)accessory.Method.TTS("击退到安全区");
        if (isEdgeTTS)accessory.Method.EdgeTTS("击退到安全区");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深黑骑士_残杀击退";
        dp.Scale = new(1.6f, 25f);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 3400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
        
    [ScriptMethod(name: "60 妖异之影 怨念 钢铁提前显示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6374"])]
    public void 妖异之影_怨念(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "妖异之影_怨念";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f); // 需要加1m的目标圈
        dp.DestoryAt = 5500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region  61~70层
    [ScriptMethod(name: "—————— 61 ~ 70 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第61层(Event @event, ScriptAccessory accessory) { }

    // 不确定这边的 地宫笠头螈 (SourceDataId:6190) 是否会使用 粘膜 (ActionId:7014) 
    
    /*
    [ScriptMethod(name: "70 水虎龙亚夸虏 浸泡 提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:226"])]
    public void 水虎龙亚夸虏_浸泡(Event @event, ScriptAccessory accessory)
    {
        // 与 170层 虹蛇 (SourceDataId:6174) 相同，都会附加 加速 buff 【甚至天气都会！】
        if (isText)accessory.Method.TextInfo("将BOSS拉出水圈至buff消失", duration: 3000, true);
        if (isTTS)accessory.Method.TTS("将BOSS拉出水圈至buff消失");
        if (isEdgeTTS)accessory.Method.EdgeTTS("将BOSS拉出水圈至buff消失");
    }
    */
    
    #endregion
    
    #region  71~80层
    [ScriptMethod(name: "—————— 71 ~ 80 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第71层(Event @event, ScriptAccessory accessory) { }
    

    [ScriptMethod(name: "地宫独眼巨人 百吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6971", "SourceDataId:6203"])]
    public void 地宫独眼巨人_百吨回转(Event @event, ScriptAccessory accessory)
    {
        // 技能ID同 171~179层的 深宫独眼雪巨人 百吨回转（钢铁）
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"地宫独眼巨人_百吨回转{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.8f); // 表格8m + 目标圈2.8m
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    
    [ScriptMethod(name: "地宫独眼巨人 眼魔之眼（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6972", "SourceDataId:6203"])]
    public void 地宫独眼巨人_眼魔之眼(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"地宫独眼巨人_眼魔之眼{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(4f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    // 80 BOSS 古丹纳
    [ScriptMethod(name: "80 古丹纳 吸引（此选项无意义）【与180层共用】", eventType: EventTypeEnum.StartCasting, eventCondition: ["DataId:01"])]
    public void 古丹纳_吸引(Event @event, ScriptAccessory accessory) { }
    
    #endregion
    
    #region  81~90层
    [ScriptMethod(name: "—————— 81 ~ 90 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第81层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "地宫奇美拉 寒冰咆哮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7078","SourceDataId:6220"])]
    public void 地宫奇美拉_寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"地宫奇美拉_寒冰咆哮{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9.7f); // 表格 6m + 目标圈 3.7m
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "地宫奇美拉 雷电咆哮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7079","SourceDataId:6220"])]
    public void 地宫奇美拉_雷电咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"地宫奇美拉_雷电咆哮{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(6.7f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    // 90层 BOSS 爆弹怪教母
    [ScriptMethod(name: "90 熔岩爆弹怪 自爆（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6377"])]
    public void 熔岩爆弹怪_自爆(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"熔岩爆弹怪_自爆{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6.6f);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "90 灰色爆弹怪 击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6376"])]
    public void 灰色爆弹怪_击杀提示(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("击杀灰色爆弹怪", duration: 3000, true);
        if(isTTS) accessory.Method.TTS("击杀灰色爆弹怪");
        if(isEdgeTTS) accessory.Method.EdgeTTS("击杀灰色爆弹怪");
    }
    
    [ScriptMethod(name: "90 眩晕爆弹怪 寒霜弹提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6378"])]
    public void 眩晕爆弹怪_震撼弹(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("将眩晕爆弹怪推至BOSS脚下", duration: 3000, true);
        if(isTTS) accessory.Method.TTS("将眩晕爆弹怪推至BOSS脚下");
        if(isEdgeTTS) accessory.Method.EdgeTTS("将眩晕爆弹怪推至BOSS脚下");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "眩晕爆弹怪_寒霜弹";
        dp.Color = new Vector4(1f, 0f, 0f, 1.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.2f);
        dp.DestoryAt = 24700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "眩晕爆弹怪 寒霜弹销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:7104"], userControl: false)]
    public void 眩晕爆弹怪_震撼弹销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"眩晕爆弹怪_寒霜弹");
    }
    #endregion
    
    #region 91~100层 没有东西可以画
    [ScriptMethod(name: "—————— 91 ~ 100 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第91层(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 101~110层 没有东西可以画
    [ScriptMethod(name: "—————— 101 ~ 110 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第101层(Event @event, ScriptAccessory accessory) { }
    
    // 彩蛋怪物 帝国兵驾驶员 DataId:6404 , NpcYell Id 4223 "魔导死神改……只要有那个……"
    #endregion
    
    #region 111~120层
    [ScriptMethod(name: "—————— 111 ~ 120 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第111层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫蝾螈 粘膜 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7014", "SourceDataId:6249"])]
    public void 深宫蝾螈_粘膜(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("打断 <深宫蝾螈>", duration: 2300, true);
        if(isTTS) accessory.Method.TTS("打断深宫蝾螈");
        if(isEdgeTTS) accessory.Method.EdgeTTS("打断深宫蝾螈");
    }
    
    #endregion
    
    #region 121~130层
    [ScriptMethod(name: "—————— 121 ~ 130 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第121层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫斯卡尼特 唧唧咋咋（睡眠钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6365", "SourceDataId:6267"])]
    public void 深宫斯卡尼特_唧唧咋咋(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫斯卡尼特_唧唧咋咋{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 0.1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(21.6f); // 20m圆形 +目标圈1.6m
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫弥诺陶洛斯 百一十吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6364", "SourceDataId:6266"])]
    public void 深宫弥诺陶洛斯_百一十吨回转(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫弥诺陶洛斯_百一十吨回转{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.8f); // 8m圆形 +目标圈2.8m
        dp.DestoryAt = 4200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "130 埃尔法德 恐惧迷雾（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7141"])]
    public void 埃尔法德_恐惧迷雾(Event @event, ScriptAccessory accessory)
    {
        // BOSS SourceDataId:6170 ，技能ID与30层的不同 无需区分
        if (isText)accessory.Method.TextInfo("靠近场中BOSS", duration: 1300, true);
        if (isTTS)accessory.Method.TTS("靠近场中BOSS");
        if (isEdgeTTS)accessory.Method.EdgeTTS("靠近场中BOSS");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "埃尔法德_恐惧迷雾";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(58.8f); // 表格54m +目标圈4.8m
        dp.InnerScale = new Vector2(4.8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    #endregion
    
    #region 131~140层
    [ScriptMethod(name: "—————— 131 ~ 140 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第131层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫鬼鱼 吸蚀（吸引）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6372", "SourceDataId:6274"])]
    public void 深宫鬼鱼_吸蚀(Event @event, ScriptAccessory accessory)
    {
        if(!KnockPenalty) {
            if(isTTS) accessory.Method.TTS("吸引后钢铁");
            if(isEdgeTTS) accessory.Method.EdgeTTS("吸引后钢铁");
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫鬼鱼_吸蚀{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f); // 8m 为吸引后的钢铁范围，直接提前绘制
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫鬼鱼 洪水（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6373", "SourceDataId:6274"])]
    public void 深宫鬼鱼_洪水(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫鬼鱼_洪水{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫冥鬼之眼 5级石化 （扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7031", "SourceDataId:6272"])]
    public void 深宫冥鬼之眼_5级石化(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫冥鬼之眼_5级石化{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.1f); // 6m扇形 +目标圈1.1m
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    #endregion
    
    #region 141~150层
    [ScriptMethod(name: "—————— 141 ~ 150 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第141层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫浮灵 强麻痹 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6386", "SourceDataId:6284"])]
    public void 深宫浮灵_强麻痹(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("打断 <深宫浮灵>", duration: 4300, true);
        if (isTTS)accessory.Method.TTS("打断 <深宫浮灵>");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断 <深宫浮灵>");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫浮灵_强麻痹{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫假面 强麻痹 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6388", "SourceDataId:6286"])]
    public void 深宫假面_强麻痹(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("打断 <深宫假面>", duration: 4300, true);
        if (isTTS)accessory.Method.TTS("打断 <深宫假面>");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断 <深宫假面>");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫假面_强麻痹{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫曼提克 撕裂利爪（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6394", "SourceDataId:6289"])]
    public void 深宫曼提克_撕裂利爪(Event @event, ScriptAccessory accessory)
    {
        // 此绘制的技能ID同 31~49层的 噩梦曼提克 撕裂利爪，均为 5m + 目标圈 
        var dp = accessory.Data.GetDefaultDrawProperties(); 
        dp.Name = $"深宫曼提克_撕裂利爪{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.2f); // 表格 5m + 目标圈 2.2m
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 1900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    #endregion
    
    #region 151~160层
    [ScriptMethod(name: "—————— 151 ~ 160 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第151层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫小恶魔 冰棘屏障 打断提醒", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6943", "SourceDataId:6295"])]
    public void 深宫小恶魔_冰棘屏障(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("打断 <深宫小恶魔>", duration: 800, true);
        if (isTTS)accessory.Method.TTS("打断 <深宫小恶魔>");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断 <深宫小恶魔>");
    }
    
    [ScriptMethod(name: "160 非生骑士 残杀（击退)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7156"])]
    public void 非生骑士_残杀(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("击退到安全区", duration: 3300, true);
        if (isTTS)accessory.Method.TTS("击退到安全区");
        if (isEdgeTTS)accessory.Method.EdgeTTS("击退到安全区");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "非生骑士_残杀击退";
        dp.Scale = new(1.6f, 25f);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 3400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
        
    [ScriptMethod(name: "160 大妖异之影 怨念 钢铁提前显示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6383"])]
    public void 大妖异之影_怨念(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "大妖异之影_怨念";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f); // 需要加1m的目标圈
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region 161~170层
    [ScriptMethod(name: "—————— 161 ~ 170 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第161层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫笠头螈 粘膜 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7014", "SourceDataId:6305"])]
    public void 深宫笠头螈_粘膜(Event @event, ScriptAccessory accessory)
    {
        // 61~70层 的 地宫笠头螈 (SourceDataId:6190) 暂未知是否会同款技能
        if (isText)accessory.Method.TextInfo("打断 <深宫笠头螈>", duration: 2300, true);
        if (isTTS)accessory.Method.TTS("打断 <深宫笠头螈>");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断 <深宫笠头螈>");
    }
    
    /*
    [ScriptMethod(name: "170 虹蛇 浸泡 提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:290"])]
    public void 虹蛇_浸泡(Event @event, ScriptAccessory accessory)
    {
        // 70层的 水虎龙亚夸虏 (SourceDataId:6165) 没有 StatusID:290 的 伤害提高 buff
        if (isText)accessory.Method.TextInfo("将BOSS拉出水圈至buff消失", duration: 3000, true);
        if (isTTS)accessory.Method.TTS("将BOSS拉出水圈至buff消失");
        if (isEdgeTTS)accessory.Method.EdgeTTS("将BOSS拉出水圈至buff消失");
    }
    */
    
    #endregion
    
    #region 171~180层 小怪
    [ScriptMethod(name: "—————— 171 ~ 180 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第171层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫独眼雪巨人 怒视（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7061", "SourceDataId:6317"])]
    public void 深宫独眼雪巨人_怒视(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫独眼雪巨人_怒视{@event.SourceId()}";
        dp.Scale = new (7, 21f); // 表格长度17m +目标圈4m
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "深宫独眼雪巨人 百吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6971", "SourceDataId:6317"])]
    public void 深宫独眼雪巨人_百吨回转(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫独眼雪巨人_百吨回转{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);// 表格圆形8m +目标圈4m
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫大脚巨猿 捶胸（脱战钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6973", "SourceDataId:6318"])]
    public void 深宫大脚巨猿_捶胸(Event @event, ScriptAccessory accessory)
    {
        // 从读条 [6973]吃香蕉（读条1.7s）开始 ， 随后会给自己附加持续 约15s的 [53]物理伤害提高 buff，持续时间内会不断造成 7063 捶胸 大范围圆形伤害，进入战斗后不会发动该技能（但平A也吃物理伤害提高）
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫大脚巨猿_捶胸{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0.8f, 0f, 0.3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(53.6f); // 50m圆形 +目标圈3.6m
        dp.Delay = 1700; // 读条吃香蕉时暂不显示
        dp.DestoryAt = 15300; // 物理伤害提高buff存在时间
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫大脚巨猿 捶胸销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:6499", "SourceDataId:6318"], userControl: false)]
    public void 深宫大脚巨猿_捶胸销毁(Event @event, ScriptAccessory accessory)
    {
        // 进入战斗状态后，触发平A即销毁捶胸绘制
        accessory.Method.RemoveDraw($"深宫大脚巨猿_捶胸{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "深宫大脚巨猿 捶胸销毁备用", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:53", "SourceDataId:6318"], userControl: false)]
    public void 深宫大脚巨猿_捶胸销毁备用(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"深宫大脚巨猿_捶胸{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "深宫妖鸟 极乐之风 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7060", "SourceDataId:6316"])]
    public void 深宫妖鸟_极乐之风(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("打断 <深宫妖鸟>", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("打断 <深宫妖鸟>");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断 <深宫妖鸟>");
    }
    
    #endregion

    #region 180层 BOSS 丹代恩索涅
    [ScriptMethod(name: "180 古丹纳 & 丹代恩索涅 吸引（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6384"])]
    public void 丹代恩索涅_吸引(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"丹代恩索涅_吸引{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 44000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "古丹纳 & 丹代恩索涅 吸引销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:6384"], userControl: false)]
    public void 深宫大脚巨猿_吸引销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"丹代恩索涅_吸引{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "180 丹代恩索涅 黄道陨石", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7166"])]
    public void 丹代恩索涅_黄道陨石(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("80%真伤", duration: 5000, true);
        if(isTTS) accessory.Method.TTS("80%真伤");
        if(isEdgeTTS) accessory.Method.EdgeTTS("80%真伤");
    }
    #endregion

    #region 181~190层 小怪
    [ScriptMethod(name: "—————— 181 ~ 190 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第181层(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "深宫加姆 寒冰咆哮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7078", "SourceDataId:6335"])]
    public void 深宫加姆_寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        // 此处的 深宫加姆 与 81~90层的 地宫奇美拉 所释放的 钢铁月环 技能ID相同 ，但其对应的目标圈不同 ，所以需要区分画图
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫加姆_寒冰咆哮{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.4f); // 6m圆形 +目标圈4.4m
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫加姆 雷电咆哮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7078", "SourceDataId:6335"])]
    public void 深宫加姆_雷电咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫加姆_雷电咆哮{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(7.4f); // 内径约为 3m + 目标圈4.4m 
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    /*
    [ScriptMethod(name: "深宫洪水巨虫 大流沙（狂暴）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7070", "SourceDataId:6329"])]
    public void 深宫洪水巨虫_大流沙(Event @event, ScriptAccessory accessory)
    {
        // 狂暴技能，可打断，可遁地逃
        // dp.Name = $"深宫洪水巨虫_大流沙{@event.SourceId()}";
        // dp.Scale = new Vector2(23.6f); // 20m圆形 +目标圈3.6m
        // dp.DestoryAt = 700;
    }
    */
    
    // 高危怪 水龙 SourceDataId:6328
    
    #endregion
    
    #region 190层 BOSS 爆弹怪教父 [熔岩爆弹怪预测计数问题待修复]
    [ScriptMethod(name: "190 眩晕爆弹怪 冰碎（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6387"])]
    public void 眩晕爆弹怪_冰碎(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "眩晕爆弹怪_冰碎";
        dp.Color = new Vector4(1f, 0.5f, 0f, 1.5f);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6.6f);
        dp.DestoryAt = 8400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "190 治疗爆弹怪 击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6385"])]
    public void 治疗爆弹怪_击杀提示(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("击杀治疗爆弹怪", duration: 5, true);
        accessory.Method.TTS("击杀治疗爆弹怪"); 
        // Interlocked.Increment(ref timesRemedyBomb);  //防止多线程出问题，锁定一下
        ++timesRemedyBomb; //记录 治疗爆弹怪出现次数  注意：还没有写重置次数条件 需要在团灭重生时销毁 （或者反正这个也就单挑比较有用死了就重置算了x）
        if(isDeveloper) accessory.Method.SendChat($"/e 调试信息：已记录 治疗爆弹怪生成 第{timesRemedyBomb}次");
    }
    
    [ScriptMethod(name: "190 地面爆破 读条计数", userControl:false, eventType: EventTypeEnum.AddCombatant, eventCondition: ["ActionId:7169"])]
    public void 地面爆破读条计数(Event @event, ScriptAccessory accessory)
    {
        ++Sap;
        if(isDeveloper) accessory.Method.SendChat($"/e 调试信息：地面爆破读条次数： {Sap}");
    }
    
    
    /*/  有点计数问题，先放着等有缘人修
    [ScriptMethod(name: "190 熔岩爆弹怪 出现位置预测", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6385"])]
    public void 熔岩爆弹怪_震撼弹预测(Event @event, ScriptAccessory accessory)
    {
        //在治疗爆弹怪 刷新约54s后 出现熔岩爆弹怪 ，期间BOSS会读条3次 [7169]地面爆破  第3次与第2次间隔较长，应在第3次黄圈读条时准备将BOSS拉去刷新位置
        //暂未知 治疗爆弹怪死亡时间 与BOSS黄圈技能时间轴是否有关联
        //在刷新45s时 大约会读条第3次黄圈，故应 Delay = 45000，DestoryAt = 9000
        
        if(isDeveloper) accessory.Method.SendChat($"/e 调试信息：已捕获 治疗爆弹怪生成{timesRemedyBomb}次，开始45秒后绘制倒计时");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "熔岩爆弹怪_震撼弹预测";
        dp.Color = new Vector4(0f, 0f, 1f, 1.5f);
        dp.Scale = new Vector2(7.2f);
        dp.Delay = 45000;
        dp.DestoryAt = 9000;
        
        // 根据 治疗爆弹怪 出现次数 决定 熔岩爆弹怪 生成位置
        // 或者根据 地面爆破 读条3n次 决定位置
        switch(timesRemedyBomb) {

            case 1: {
                dp.Position = new Vector3(-288.63f, 0.14f,-300.26f);
                if(isDeveloper) accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }

            case 2:{
                dp.Position = new Vector3(-297.46f, 0.12f,-297.52f);
                if(isDeveloper) accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }

            case 3:{
                dp.Position = new Vector3(-288.84f, 0.12f,-305.54f);
                if(isDeveloper) accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }
            
            case 4:{
                dp.Position = new Vector3(-309.13f, 0.05f,-303.74f);
                if(isDeveloper) accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }
            
            case 5:{
                dp.Position = new Vector3(-298.36f, 0.05f,-293.63f);
                if(isDeveloper) accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }
            
            case 6:{
                dp.Position = new Vector3(-301.96f, 0.05f,-314.29f);
                if(isDeveloper) accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }
            
            case 7:{
                dp.Position = new Vector3(-299.12f, 0.05f,-297.56f);
                if(isDeveloper) accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }

            default: {
                if(isDeveloper) accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }

        }
        
        if(isText) accessory.Method.TextInfo("已预测 <熔岩爆弹怪> 刷新位置", duration: 5000, false);
        if(isTTS) accessory.Method.TTS("已预测熔岩爆弹怪刷新位置");   
        if(isEdgeTTS) accessory.Method.EdgeTTS("已预测熔岩爆弹怪刷新位置");  
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "熔岩爆弹怪 震撼弹预测销毁", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6386"], userControl: false)]
    public void 熔岩爆弹怪_震撼弹预测销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"熔岩爆弹怪_震撼弹预测");
    }
    
    // */
    
    [ScriptMethod(name: "190 熔岩爆弹怪 震撼弹提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6386"])]
    public void 熔岩爆弹怪_震撼弹(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("将熔岩爆弹怪推至BOSS脚下", duration: 5000, true);
        if(isTTS) accessory.Method.TTS("将熔岩爆弹怪推至BOSS脚下");
        if(isEdgeTTS) accessory.Method.EdgeTTS("将熔岩爆弹怪推至BOSS脚下");  
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "熔岩爆弹怪_震撼弹";
        dp.Color = new Vector4(1f, 0f, 0f, 3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.2f);
        dp.DestoryAt = 24700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "熔岩爆弹怪 震撼弹销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:7170"], userControl: false)]
    public void 熔岩爆弹怪_震撼弹销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"熔岩爆弹怪_震撼弹");
    }
    
    
    [ScriptMethod(name: "190 爆弹怪教父 特大爆炸提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7103"])]
    public async void 爆弹怪教父_特大爆炸提示(Event @event, ScriptAccessory accessory)
    { 
        isExplosionInterrupted = false;
        
        await Task.Delay(14800);
            
        if (!isExplosionInterrupted)
        {
            if(isText) accessory.Method.TextInfo("99.9%真伤，注意瞬回", duration: 9500, true);
            if(isTTS) accessory.Method.TTS("99.9%真伤，注意瞬回");
            if(isEdgeTTS) accessory.Method.EdgeTTS("99.9%真伤，注意瞬回");  
        }
    }
    
    [ScriptMethod(name: "特大爆炸打断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:7103"], userControl: false)]
    public void 特大爆炸打断销毁(Event @event, ScriptAccessory accessory)
    {
        isExplosionInterrupted = true;
    }

    #endregion
    
    #region 191~200层 小怪
    [ScriptMethod(name: "—————— 191 ~ 200 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第191层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "奥尼克斯龙 邪视（背对）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7043", "SourceDataId:6338"])]
    public void 奥尼克斯龙_邪视(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"奥尼克斯龙_邪视{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f); // 技能范围8m圆形 +目标圈5m
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫幽鬼之眼 5级即死 （扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7084", "SourceDataId:6341"])]
    public void 深宫幽鬼之眼_5级即死(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫幽鬼之眼_5级即死{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.1f); // 技能范围6m扇形 +目标圈1.1m
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    #endregion
    
    #region 底裤部分
    
    [ScriptMethod(name: "—————— 底裤部分（需要对应插件与权限） ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 底裤部分(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "[DR] 变身曼提克时，移速改为1.5倍", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:565", "StackCount:42"])]
    public void AddManticore(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.5");
        accessory.Method.SendChat($"/e 可达鸭：移速已更改：1.5x");
        if (isTTS)accessory.Method.TTS("移速已更改至1.5倍");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已更改至1.5倍");
    }
    
    [ScriptMethod(name: "[DR] 曼提克取消时，移速复原至1倍", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:565", "StackCount:42"])]
    public void RemoveManticore(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1");
        accessory.Method.SendChat($"/e 可达鸭：移速已更改：1");
        if (isTTS)accessory.Method.TTS("移速已复原至1倍");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已复原至1倍");
    }

    [ScriptMethod(name: "[DR] 变身梦魔时，移速改为1.2倍", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:565", "StackCount:43"])]
    public void AddSuccubus(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.2");
        accessory.Method.SendChat($"/e 可达鸭：移速已更改：1.2x");
        if (isTTS)accessory.Method.TTS("移速已更改至1.2倍");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已更改至1.2倍");
    }
    
    [ScriptMethod(name: "[DR] 梦魔取消时，移速复原至1倍", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:565", "StackCount:43"])]
    public void RemoveSuccubus(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1");
        accessory.Method.SendChat($"/e 可达鸭：移速已更改：1");
        if (isTTS)accessory.Method.TTS("移速已复原至1倍");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已复原至1倍");
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