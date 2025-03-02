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
using System.Threading;
using System.Threading.Tasks;

namespace the_Palace_of_the_Dead;

[ScriptType(guid: "4210c323-eba4-4d67-a7e7-b90799494729", name: "死者宫殿", author: "Tetora", 
    territorys: [561,562,563,564,565,593,594,595,596,597,598,599,600,601,602,603,604,605,606,607],
    version: "0.0.0.2",note: noteStr)]

public class the_Palace_of_the_Dead
{
    const string noteStr =
        """
        v0.0.0.2:
        死者宫殿绘制
        注：方法设置中的层数仅做分割线效果，并不是批量开关
        重要：1~80层暂无录像证实，可能会有部分绘制错误，如发现错误请带着ARR联系我
        严重错误：暂未支持【形态变化】【眩晕】【催眠】等限制
        """;
    
    //眩晕、催眠、无法发动技能1113等状态都需要销毁绘图
    
    
    [ScriptMethod(name: "伤头&插言 打断销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75[38|51]$"], userControl: false)]
    public void 打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"深宫假面_强麻痹{@event.SourceId()}");
        accessory.Method.RemoveDraw($"深宫浮灵_强麻痹{@event.SourceId()}");
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
    private int timesRemedyBomb=0;
    public void Init(ScriptAccessory accessory) {
        
        timesRemedyBomb=0;  //190 治疗爆弹怪 计数
        
    }
    
    private bool isExplosionInterrupted = false;  // 190层 特大爆炸打断状态
    
    // 通用内容
    [ScriptMethod(name: "拟态怪_怨念提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6397"])]
    public void 拟态怪_怨念(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("打断拟态怪", duration: 2700, true);
        accessory.Method.TTS("打断拟态怪");
    }
    
    #region  1~10层
    // 1~10层
    [ScriptMethod(name: "—————— 1 ~ 10 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第1层(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region  11~20层
    // 11~20层
    [ScriptMethod(name: "—————— 11 ~ 20 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第11层(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 21~30层
    // 21~30层
    [ScriptMethod(name: "—————— 21 ~ 30 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第21层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "地宫斯卡尼特 唧唧咋咋（睡眠钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6365"])]
    public void 斯卡尼特_唧唧咋咋(Event @event, ScriptAccessory accessory)
    {
        //121~129 深宫斯卡尼特 同技能ID
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "斯卡尼特_唧唧咋咋";
        dp.Color = new Vector4(1f, 0f, 1f, 0.4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(21.6f);
        dp.DestoryAt = 2200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "30 宁吉兹济达 恐惧迷雾（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6429"])]
    public void 宁吉兹济达_恐惧迷雾(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "宁吉兹济达_恐惧迷雾";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(54f);
        dp.InnerScale = new Vector2(4.8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    #endregion
    
    #region  31~40层
    // 31~40层
    [ScriptMethod(name: "—————— 31 ~ 40 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第31层(Event @event, ScriptAccessory accessory) { }
    
    // 注：此处的噩梦曼提克 撕裂利爪 的技能ID 同 141~149 深宫曼提克 撕裂利爪 ； 均为 5m + 目标圈 ，暂未核实目标圈是否相同
    
    #endregion
    
    #region  41~50层
    // 41~50层
    [ScriptMethod(name: "—————— 41 ~ 50 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第41层(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region  51~60层
    // 51~60层
    [ScriptMethod(name: "—————— 51 ~ 60 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第51层(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region  61~70层
    // 61~70层
    [ScriptMethod(name: "—————— 61 ~ 70 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第61层(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region  71~80层
    // 71~80层
    [ScriptMethod(name: "—————— 71 ~ 80 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第71层(Event @event, ScriptAccessory accessory) { }
    
    // 地宫独眼巨人 百吨回转（钢铁）同 171~179层的 深宫独眼雪巨人 百吨回转（钢铁），猜测目标圈为 4m 暂未实证。
    
    [ScriptMethod(name: "地宫独眼巨人 眼魔之眼（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6972"])]
    public void 地宫独眼巨人_眼魔之眼(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "地宫独眼巨人_眼魔之眼";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(4f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    /*  没有龙卷风的 DataID
    // 80 BOSS 古丹纳
    [ScriptMethod(name: "80 古丹纳 吸引（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:？？？"])]
    public void 古丹纳_吸引(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"古丹纳_吸引{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 44000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    */
    
    #endregion
    
    #region  81~90层
    // 81~90层 小怪
    [ScriptMethod(name: "—————— 81 ~ 90 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第81层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "地宫奇美拉 寒冰咆哮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7078","SourceName:地宫奇美拉"])]
    public void 地宫奇美拉_寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "地宫奇美拉_寒冰咆哮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9.7f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "地宫奇美拉 雷电咆哮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7079","SourceName:地宫奇美拉"])]
    public void 地宫奇美拉_雷电咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "地宫奇美拉_雷电咆哮";
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
        dp.Name = "熔岩爆弹怪_自爆";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6.6f);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "90 灰色爆弹怪 击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6376"])]
    public void 灰色爆弹怪_击杀提示(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("击杀灰色爆弹怪", duration: 5, true);
        accessory.Method.TTS("击杀灰色爆弹怪");
    }
    
    [ScriptMethod(name: "90 眩晕爆弹怪 寒霜弹提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6378"])]
    public void 眩晕爆弹怪_震撼弹(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("将眩晕爆弹怪推至BOSS脚下", duration: 5, true);
        accessory.Method.TTS("将眩晕爆弹怪推至BOSS脚下");
        
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
    
    #region 91~100层
    // 91~100层
    [ScriptMethod(name: "—————— 91 ~ 100 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第91层(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 101~110层
    // 101~110层
    [ScriptMethod(name: "—————— 101 ~ 110 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第101层(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 111~120层
    // 111~120层
    [ScriptMethod(name: "—————— 111 ~ 120 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第111层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫蝾螈 粘膜 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7014"])]
    public void 深宫蝾螈_粘膜(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("打断 <深宫蝾螈>", duration: 2700, true);
        accessory.Method.TTS("打断深宫蝾螈");
    }
    #endregion
    
    #region 121~130层
    // 121~130层
    [ScriptMethod(name: "—————— 121 ~ 130 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第121层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫弥诺陶洛斯 百一十吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6364"])]
    public void 深宫弥诺陶洛斯_百一十吨回转(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深宫弥诺陶洛斯_百一十吨回转";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.8f);
        dp.DestoryAt = 4200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "130 埃尔法德 恐惧迷雾（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7141"])]
    public void 埃尔法德_恐惧迷雾(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "埃尔法德_恐惧迷雾";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(54f);
        dp.InnerScale = new Vector2(4.8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    #endregion
    
    #region 131~140层
    // 131~140层
    [ScriptMethod(name: "—————— 131 ~ 140 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第131层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫鬼鱼 吸蚀（吸引）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6372"])]
    public void 深宫鬼鱼_吸蚀(Event @event, ScriptAccessory accessory)
    {
        if(!KnockPenalty) {
            accessory.Method.TTS("吸引后钢铁");
        }
        else {
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深宫鬼鱼_吸蚀";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫鬼鱼 洪水（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6372"])]
    public void 深宫鬼鱼_洪水(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深宫鬼鱼_洪水";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫冥鬼之眼 5级石化 （扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7031"])]
    public void 深宫冥鬼之眼_5级石化(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "深宫冥鬼之眼_5级石化";
        dp.Color = new Vector4(1f, 0f, 0f, 0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.1f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    #endregion
    
    #region 141~150层
    // 141~150层 小怪
    [ScriptMethod(name: "—————— 141 ~ 150 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第141层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫浮灵 强麻痹 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6386"])]
    public void 深宫浮灵_强麻痹(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("打断 <深宫浮灵>", duration: 4700, true);
        accessory.Method.TTS("打断深宫浮灵");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫浮灵_强麻痹{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫假面 强麻痹 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6388"])]
    public void 深宫假面_强麻痹(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("打断 <深宫假面>", duration: 4700, true);
        accessory.Method.TTS("打断深宫假面");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫假面_强麻痹{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫曼提克 撕裂利爪（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6394"])]
    public void 深宫曼提克_撕裂利爪(Event @event, ScriptAccessory accessory)
    {
        // 此绘制的技能ID同 31~49层的 噩梦曼提克 撕裂利爪，均为 5m + 目标圈 
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "深宫曼提克_撕裂利爪";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.2f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 1900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    #endregion
    
    #region 151~160层
    // 151~160层 小怪
    [ScriptMethod(name: "—————— 151 ~ 160 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第151层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫小恶魔 冰棘屏障 打断提醒", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6943"])]
    public void 方法名(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("打断 <深宫小恶魔>", duration: 1200, true);
        accessory.Method.TTS("打断深宫小恶魔");
    }
    #endregion
    
    #region 161~170层
    // 161~170层
    [ScriptMethod(name: "—————— 161 ~ 170 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第161层(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 171~180层 小怪
    // 171~180层 小怪
    [ScriptMethod(name: "—————— 171 ~ 180 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第171层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "深宫独眼雪巨人 怒视（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7061"])]
    public void 深宫独眼雪巨人_怒视(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深宫独眼雪巨人_怒视";
        dp.Scale = new (7, 21f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "深宫独眼雪巨人 百吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6971"])]
    public void 深宫独眼雪巨人_百吨回转(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深宫独眼雪巨人_百吨回转";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫大脚巨猿 捶胸（脱战钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6973"])]
    public void 深宫大脚巨猿_捶胸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"深宫大脚巨猿_捶胸{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0.8f, 0f, 0.3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(53.6f);
        dp.Delay = 1700;
        dp.DestoryAt = 15300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫大脚巨猿 捶胸销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:6499"], userControl: false)]
    public void 深宫大脚巨猿_捶胸销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"深宫大脚巨猿_捶胸{@event.SourceId()}");
    }
    #endregion

    #region 180层 BOSS 丹代恩索涅
    //180 BOSS 丹代恩索涅
    [ScriptMethod(name: "180 丹代恩索涅 吸引（钢铁）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6384"])]
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
    
    [ScriptMethod(name: "丹代恩索涅 吸引销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:6384"], userControl: false)]
    public void 深宫大脚巨猿_吸引销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"丹代恩索涅_吸引{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "180 丹代恩索涅 黄道陨石", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7166"])]
    public void 丹代恩索涅_黄道陨石(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("80%真伤", duration: 5, true);
        accessory.Method.TTS("80%真伤");
    }
    #endregion

    #region 181~190层 小怪
    // 181~190层 小怪
    [ScriptMethod(name: "—————— 181 ~ 190 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第181层(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "深宫加姆 寒冰咆哮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7078","SourceName:深宫加姆"])]
    public void 深宫加姆_寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        // 此处的 深宫加姆 与 81~90层的 地宫奇美拉 所释放的 钢铁月环 技能ID相同 ，但其对应的目标圈不同 ，所以需要区分画图
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深宫加姆_寒冰咆哮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.4f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫加姆 雷电咆哮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7079","SourceName:深宫加姆"])]
    public void 深宫加姆_雷电咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深宫加姆_雷电咆哮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(7f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    #endregion
    
    #region 190层 BOSS 爆弹怪教父
    // 190层 BOSS 爆弹怪教父
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
        //Interlocked.Increment(ref timesRemedyBomb);  //防止多线程出问题，锁定一下
        //++timesRemedyBomb; //记录 治疗爆弹怪出现次数  注意：还没有写重置次数条件
    }
    
    
    /*  有点计数问题，先放着等有缘人修
    [ScriptMethod(name: "190 熔岩爆弹怪 出现位置预测", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6385"])]
    public void 熔岩爆弹怪_震撼弹预测(Event @event, ScriptAccessory accessory)
    {
        //在治疗爆弹怪 刷新约54s后 出现熔岩爆弹怪 ，期间BOSS会读条3次 [7169]地面爆破  第3次与第2次间隔较长，应在第3次黄圈读条时准备将BOSS拉去刷新位置
        //暂未知 治疗爆弹怪死亡时间 与BOSS黄圈技能时间轴是否有关联
        //在刷新45s时 大约会读条第3次黄圈，故应 Delay = 45000，DestoryAt = 9000
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "熔岩爆弹怪_震撼弹预测";
        dp.Color = new Vector4(0f, 0f, 1f, 1.5f);
        dp.Scale = new Vector2(7.2f);
        dp.Delay = 45000;
        dp.DestoryAt = 9000;
        
        switch(timesRemedyBomb) {

            case 1: {
                dp.Position = new Vector3(-288.63f, 0.14f,-300.26f);
                accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }

            case 2:{
                dp.Position = new Vector3(-297.46f, 0.12f,-297.52f);
                accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }

            case 3:{
                dp.Position = new Vector3(-288.84f, 0.12f,-305.54f);
                accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }
            
            case 4:{
                dp.Position = new Vector3(-309.13f, 0.05f,-303.74f);
                accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }
            
            case 5:{
                dp.Position = new Vector3(-298.36f, 0.05f,-293.63f);
                accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }
            
            case 6:{
                dp.Position = new Vector3(-301.96f, 0.05f,-314.29f);
                accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }
            
            case 7:{
                dp.Position = new Vector3(-299.12f, 0.05f,-297.56f);
                accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }

            default: {
                accessory.Method.SendChat($"/e 调试信息 timeRemedyBomb={timesRemedyBomb}");
                break;
            }

        }
        
        //accessory.Method.TextInfo("已预测 <熔岩爆弹怪> 刷新位置", duration: 5, false);
        //accessory.Method.TTS("已预测熔岩爆弹怪刷新位置");   
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "熔岩爆弹怪 震撼弹预测销毁", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6386"], userControl: false)]
    public void 熔岩爆弹怪_震撼弹预测销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"熔岩爆弹怪_震撼弹预测");
    }
    
    */
    
    [ScriptMethod(name: "190 熔岩爆弹怪 震撼弹提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6386"])]
    public void 熔岩爆弹怪_震撼弹(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("将熔岩爆弹怪推至BOSS脚下", duration: 5, true);
        accessory.Method.TTS("将熔岩爆弹怪推至BOSS脚下");
        
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
            accessory.Method.TextInfo("99.9%真伤，注意瞬回", duration: 10, true);
            accessory.Method.TTS("99.9%真伤，注意瞬回");
        }
        else
        {
        }
    }
    
    [ScriptMethod(name: "特大爆炸打断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:7103"], userControl: false)]
    public void 特大爆炸打断销毁(Event @event, ScriptAccessory accessory)
    {
        isExplosionInterrupted = true;
    }

    #endregion
    
    #region 191~200层 小怪
    // 191~200层 小怪
    [ScriptMethod(name: "—————— 191 ~ 200 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第191层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "奥尼克斯龙 邪视（背对）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7043"])]
    public void 奥尼克斯龙_邪视(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "奥尼克斯龙_邪视";
        dp.Color = new Vector4(1f, 0f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f); 
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深宫幽鬼之眼 5级即死 （扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7084"])]
    public void 深宫幽鬼之眼_5级即死(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "深宫幽鬼之眼_5级即死";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.1f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
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
