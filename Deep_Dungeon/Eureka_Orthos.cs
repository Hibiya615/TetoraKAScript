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

namespace Eureka_Orthos;

[ScriptType(guid: "5e8a4051-53f7-4eb3-bb32-b18df8b113aa", name: "正统优雷卡", 
    //territorys: uint [Regex:(1099|110[0-8])], 
    territorys: [1099,1100,1101,1102,1103,1104,1105,1106,1107,1108],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Eureka_Orthos {
    const string noteStr =
        """
        v0.0.0.1:
        正统优雷卡绘制
        注：方法设置中的层数仅做分割线效果，并不是批量开关
        现支持层数：1~20、99（BOSS）
        严重错误：暂未支持【缓速】【形态变化】【石化】【眩晕】【催眠】等限制
        错误：第20层扇形绘制会有偏差
        """;
    
    //对应怪物死亡、眩晕、催眠、石化1511、无法发动技能1113等状态都需要销毁绘图，缓速3493需要额外注意没有omen的技能
            
    // 通用内容
    [ScriptMethod(name: "拟态怪_怨念提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32798"])]
    public void 拟态怪_怨念(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("打断拟态怪", duration: 2700, true);
        accessory.Method.TTS("打断拟态怪");
    }
    
    //精英怪
    [ScriptMethod(name: "\ue0c0 美拉西迪亚复制体 亚拉戈陨石", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^327(1[89]|20)$"])]
    public void 美拉西迪亚复制体_亚拉戈陨石(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "美拉西迪亚复制体_亚拉戈陨石";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);  // 目标圈为 3.6
        dp.DestoryAt = 7700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "\ue0c0 亚灵智慧之灵 起源（狂暴）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^327(0[89]|10)$"])]
    public void 亚灵智慧之灵_起源(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "亚灵智慧之灵_起源";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(82.6f);  // 目标圈为 2.6
        dp.DestoryAt = 9700;  //32710为9700，3270[89]为14700
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "\ue0c0 拉米亚女王 回旋斩（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3272[789]$"])]
    public void 拉米亚女王_回旋斩(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "拉米亚女王_回旋斩";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);  // 目标圈为 2.0
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    // 1~10层 小怪
    [ScriptMethod(name: "—————— \ue061 ~ \ue061\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第1层(Event @event, ScriptAccessory accessory) { }
    [ScriptMethod(name: "正统贝希摩斯 黄道陨石（狂暴卡墙提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33043"])]
    public void 正统贝希摩斯_黄道陨石(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("狂暴，躲在墙壁后", duration: 2700, true);
        accessory.Method.TTS("躲在墙壁后");
    }
    
    [ScriptMethod(name: "正统榴弹怪 大爆炸（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32381"])]
    public void 正统榴弹怪_大爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统榴弹怪_大爆炸";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "\ue05e 正统幽鬼之眼 恐怖视线（顺劈背对）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32386"])]
    public void 正统幽鬼之眼_恐怖视线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "正统幽鬼之眼_恐怖视线";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    // 10 BOSS 蜜言妖
    [ScriptMethod(name: "\ue061\ue060 蜜言妖 蔓德拉地雷（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31478"])]
    public void 蜜言妖_蔓德拉地雷(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "蜜言妖_蔓德拉地雷";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "\ue061\ue060 蜜言妖 蔓德拉地雷（麻将）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32700"])]
    public void 蜜言妖_蔓德拉地雷II(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "蜜言妖_蔓德拉地雷II";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Delay = 3700;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    // 11~20层 小怪
    [ScriptMethod(name: "—————— \ue061\ue061 ~ \ue062\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第11层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "正统锯齿花 均衡打击（扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32403"])]
    public void 正统锯齿花_均衡打击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "正统锯齿花_均衡打击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10);
        dp.Radian = 180f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    // 20 BOSS 盾龙复制体
    [ScriptMethod(name: "\ue062\ue060 盾龙复制体_火焰吐息", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32864"])]
    //顺劈范围需要一定偏移
    //需要捕获 “vfx/channeling/eff/gard_pow00bf.avfx” 连线对场外的小龙进行4秒的提前绘制（距离场中24~45m）
    //对于已经跳下来的场内小龙，需要在 32544 32864读条时取消绘制（距离场中23m）
    public void 盾龙复制体_火焰吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "盾龙复制体_火焰吐息";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50);
        dp.Radian = 30f.DegToRad();
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    // 21~30层 小怪
    [ScriptMethod(name: "—————— \ue062\ue061 ~ \ue063\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第21层(Event @event, ScriptAccessory accessory) { }
    
    
    // 30 BOSS 提亚马特复制体
    
    // 31~40层 小怪
    [ScriptMethod(name: "—————— \ue063\ue061 ~ \ue064\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第31层(Event @event, ScriptAccessory accessory) { }
    
    
    // 40 BOSS 双塔尼亚复制体
    
    // 41~50层 小怪
    [ScriptMethod(name: "—————— \ue064\ue061 ~ \ue065\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第41层(Event @event, ScriptAccessory accessory) { }
    
    
    // 50 BOSS 自控化奇美拉
    
    // 51~60层 小怪
    [ScriptMethod(name: "—————— \ue065\ue061 ~ \ue066\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第51层(Event @event, ScriptAccessory accessory) { }
    
    
    // 60 BOSS 自控化弥诺陶洛斯
    
    // 61~70层 小怪
    [ScriptMethod(name: "—————— \ue066\ue061 ~ \ue067\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第61层(Event @event, ScriptAccessory accessory) { }
    
    
    // 70 BOSS 永恒
    
    // 71~80层 小怪
    [ScriptMethod(name: "—————— \ue067\ue061 ~ \ue068\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第71层(Event @event, ScriptAccessory accessory) { }
    
    
    // 80 BOSS 原形卡利亚
    
    // 81~90层 小怪
    [ScriptMethod(name: "—————— \ue068\ue061 ~ \ue069\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第81层(Event @event, ScriptAccessory accessory) { }
    
    
    // 90 BOSS 管理者
    
    
    // 91~100层 小怪
    [ScriptMethod(name: "—————— \ue069\ue061 ~ \ue061\ue060\ue060 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第91层(Event @event, ScriptAccessory accessory) { }
    
    
    [ScriptMethod(name: "\ue05e 正统系统γ 高压电流（打断钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32878"])]
    public void 正统系统γ_高压电流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统系统γ_高压电流{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 0.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.DestoryAt = 7200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统系统γ 高压电流 打断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:32878"], userControl: false)]
    public void 正统系统γ_高压电流打断销毁(Event @event, ScriptAccessory accessory)
    {
        //伤头：7551
        accessory.Method.RemoveDraw($"正统系统γ_高压电流{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "\ue05e 正统系统γ 排斥炮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32877"])]
    public void 正统系统γ_排斥炮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统系统γ_排斥炮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "\ue05e 正统系统γ 环形炮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32876"])]
    public void 正统系统γ_环形炮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统系统γ_环形炮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "正统系统α 魔科学射线α（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32884"])]
    public void 正统系统α_魔科学射线α(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "正统系统α_魔科学射线α";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "正统采掘无人机 魔科学炮（半场刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33200"])]
    public void 正统采掘无人机_魔科学炮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "正统采掘无人机_魔科学炮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "正统无人机 雾散爆发（死后自爆）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32874"])]
    public void 正统无人机_雾散爆发(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统无人机_雾散爆发";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统斯芬克斯 重击（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32913"])]
    public void 正统斯芬克斯_重击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统斯芬克斯_重击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 60f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "正统恐慌装甲 猛扫（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32893"])]
    public void 正统恐慌装甲_猛扫(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统恐慌装甲_猛扫";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "正统米特里达梯 激光剑（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32880"])]
    public void 正统米特里达梯_激光剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统米特里达梯_激光剑";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20);
        dp.Radian = 270f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "正统浮游炮主板 攻城炮（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32900"])]
    public void 正统浮游炮主板_攻城炮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统浮游炮主板_攻城炮";
        dp.Scale = new (4, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "正统扎戈斧龙 残虐咆哮（脱战钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32937"])]
    public void 正统扎戈斧龙_残虐咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统扎戈斧龙_残虐咆哮{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统扎戈斧龙 残虐咆哮打断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:32937"], userControl: false)]
    public void 正统扎戈斧龙_残虐咆哮打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"正统扎戈斧龙_残虐咆哮{@event.SourceId()}");
    }

    /*  需要抓取点名跟随被点名玩家 TargetIcon：Id 0016
    [ScriptMethod(name: "正统扎戈斧龙 XXX突袭（击退点名）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32888"])]
    public void 正统扎戈斧龙_XXX突袭(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统扎戈斧龙_XXX突袭";
        dp.Color = new Vector4(1f, 0.4f, 0f, 1f);
        dp.TargetObject = @event.TargetId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    */
    
    [ScriptMethod(name: "正统整备工 邪圣（脱战钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32931"])]
    public void 正统整备工_邪圣(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统整备工_邪圣{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统整备工 邪圣打断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:32931"], userControl: false)]
    public void 正统整备工_邪圣打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"正统整备工_邪圣{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "\ue05e 正统自控化弥诺陶洛斯 百廿八吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32922"])]
    public void 正统自控化弥诺陶洛斯_百廿八吨回转(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统自控化弥诺陶洛斯_百廿八吨回转";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 3200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "\ue05e 正统自控化弥诺陶洛斯 卅二吨重击（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32921"])]
    public void 正统自控化弥诺陶洛斯_卅二吨重击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统自控化弥诺陶洛斯_卅二吨重击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(35);
        dp.Radian = 60f.DegToRad();
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "\ue05e 正统自控化奇美拉 雷鸣吐息（左上顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32907"])]
    public void 正统自控化奇美拉_雷鸣吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统自控化奇美拉_雷鸣吐息";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.Radian = 120f.DegToRad();
        //dp.Rotation = 45f; //不知道转多少才是左上.jpg  
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "\ue05e 正统自控化奇美拉 寒冰吐息（右上顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32906"])]
    public void 正统自控化奇美拉_寒冰吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统自控化奇美拉_寒冰吐息";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.Radian = 120f.DegToRad();
        //dp.Rotation = 45f; //不知道转多少才是右上.jpg  
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "\ue05e 正统自控化奇美拉 蝎尾毒刺（背后扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32908"])]
    public void 正统自控化奇美拉_蝎尾毒刺(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统自控化奇美拉_蝎尾毒刺";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7);
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f;
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "\ue05e 正统自控化奇美拉 寒冰咆哮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32909"])]
    public void 正统自控化奇美拉_寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统自控化奇美拉_寒冰咆哮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f);
        dp.DestoryAt = 3200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "\ue05e 正统自控化奇美拉 雷电咆哮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32910"])]
    public void 正统自控化奇美拉_雷电咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "正统自控化奇美拉_雷电咆哮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(6f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    
    // 99 BOSS 王者之剑
    [ScriptMethod(name: "\ue069\ue069 王者之剑 魂剑之实（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31327"])]
    public void 魂剑之实(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "魂剑之实";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 5700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "\ue069\ue069 王者之剑 魂剑之虚（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31328"])]
    public void 魂剑之虚(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "魂剑之虚";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "\ue069\ue069 真空斩（四向顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31342"])]
    public void 真空斩(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "真空斩";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20);
        dp.Radian = 60f.DegToRad();
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    

    [ScriptMethod(name: "\ue069\ue069 次元斩（扇环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31339|3134[01])$"])]
    public void 次元斩(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "次元斩内";
        dp.Color = new Vector4(1f, 0f, 0f, 3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7);
        dp.InnerScale = new Vector2(2);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp); 
        
        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "次元斩中";
        dp.Color = new Vector4(1f, 0f, 0f, 3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12);
        dp.InnerScale = new Vector2(7);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp); 
        
        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "次元斩外";
        dp.Color = new Vector4(1f, 0f, 0f, 3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(22);
        dp.InnerScale = new Vector2(17f);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp); 
    }

    
    [ScriptMethod(name: "\ue069\ue069 极寒冰川 (冰花) ", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31347"])]
    public void 极寒冰川(Event @event, ScriptAccessory accessory)
    {
        var currentProperty=accessory.Data.GetDefaultDrawProperties();
        currentProperty.Name="极寒冰川 (冰花) ";
        currentProperty.Scale=new(5,40f);
        currentProperty.Owner=@event.SourceId();
        currentProperty.Color=accessory.Data.DefaultDangerColor;
        currentProperty.DestoryAt=3700;
        
        currentProperty.Rotation=0;

        //旋转45°，绘制8次
        for(int i=1;i<=8;++i) {
            accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,currentProperty);
            currentProperty.Rotation+=45f.DegToRad();
        }
    }
    
    [ScriptMethod(name: "\ue069\ue069 极热炎流（地火）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31344"])]
    public void 极热炎流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "极热炎流";
        dp.Color = new Vector4(1f, 0f, 0f, 1.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 1500;
        dp.Delay = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
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