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
using System.Threading.Tasks;


namespace Eureka_Orthos;

[ScriptType(guid: "5e8a4051-53f7-4eb3-bb32-b18df8b113aa", name: "正统优雷卡", 
    territorys: [1099,1100,1101,1102,1103,1104,1105,1106,1107,1108],
    version: "0.0.0.4", author: "Tetora", note: noteStr)]

public class Eureka_Orthos {
    const string noteStr =
        """
        v0.0.0.4:
        正统优雷卡绘制
        注：方法设置中的层数仅做分割线效果，并不是批量开关
        现支持层数：1~20、71~100
        严重错误：暂未支持【缓速】，遇到没特效的技能可能会比判定提早消失
        """;

    // 缓速[3493] 需要额外注意没有omen的技能
    // 如71~79层 【正统狼獾 杀人爪】【正统雷兽 尾镰】【正统雷兽 电火花】均未支持缓速
            

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
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("启用底裤（需要对应插件与权限）")]
    public bool isHack { get; set; } = false;
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion
    
    #region 全局变量控制
    
    //20 攻击命令_火焰吐息 连线计数 , 因为方法抓取的连线，同一时间有多个连线，故加volatile
    private volatile int timeOrderToFire=0;
    
    //99层 石中剑 读条计数
    uint timesCaliburniHasBeenCast=0;
    public void Init(ScriptAccessory accessory) {
        
        timeOrderToFire=0;      //20 小龙连线计数
        timesCaliburniHasBeenCast=0;  //99层 石中剑 读条计数
        
    }
    
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
    
    public bool Slow = false;
    
    [ScriptMethod(name: "缓速记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3493"],suppress:1000, userControl: false)]
    public void 缓速记录(Event @event, ScriptAccessory accessory)
    {
        Slow = true;
        if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 已记录缓速");
    }
    
    [ScriptMethod(name: "缓速移除", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3493"],suppress:1000, userControl: false)]
    public void 缓速移除(Event @event, ScriptAccessory accessory)
    {
        Slow = false;
        if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 已移除缓速");
    }
    
    #endregion
    
    // 通用内容
    [ScriptMethod(name: "拟态怪_怨念提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32798"])]
    public void 拟态怪_怨念(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("打断拟态怪", duration: 2300, true);
        if (isTTS) accessory.Method.TTS("打断拟态怪");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断拟态怪");
    }
    
    #region 精英怪
    [ScriptMethod(name: "★ 美拉西迪亚复制体 亚拉戈陨石", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^327(1[89]|20)$"])]
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
    
    [ScriptMethod(name: "★ 亚灵智慧之灵 起源（狂暴）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^327(0[89]|10)$"])]
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
    
    [ScriptMethod(name: "★ 拉米亚女王 回旋斩（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3272[789]$"])]
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
    #endregion
    
    #region 1~10层 小怪
    [ScriptMethod(name: "—————— 1 ~ 10 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第1层(Event @event, ScriptAccessory accessory) { }
    [ScriptMethod(name: "正统贝希摩斯 黄道陨石（狂暴）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33043"])]
    public void 正统贝希摩斯_黄道陨石(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("狂暴，躲在墙壁后", duration: 2700, true);
        if (isTTS) accessory.Method.TTS("躲在墙壁后");
        if (isEdgeTTS)accessory.Method.EdgeTTS("躲在墙壁后");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统贝希摩斯_黄道陨石{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 0.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.DestoryAt = 11700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统贝希摩斯 黄道陨石销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:33043"], userControl: false)]
    public void 正统贝希摩斯_黄道陨石销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw( $"正统贝希摩斯_黄道陨石{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "正统榴弹怪 大爆炸（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32381"])]
    public void 正统榴弹怪_大爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统榴弹怪_大爆炸{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统幽鬼之眼 恐怖视线（顺劈背对）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32386"])]
    public void 正统幽鬼之眼_恐怖视线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = $"正统幽鬼之眼_恐怖视线{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    #endregion
    
    #region 10 BOSS 蜜言妖
    [ScriptMethod(name: "10 蜜言妖 蔓德拉地雷（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31478"])]
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
    
    [ScriptMethod(name: "10 蜜言妖 蔓德拉地雷（麻将）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32700"])]
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
    #endregion

    #region 11~20层 小怪
    [ScriptMethod(name: "—————— 11 ~ 20 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第11层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "正统锯齿花 均衡打击（扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32403"])]
    public void 正统锯齿花_均衡打击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = $"正统锯齿花_均衡打击{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10);
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    #endregion
    
    #region 20 BOSS 复制系统
    [ScriptMethod(name:"Reset_OrderToFire_攻击命令重置",userControl:false,eventType:EventTypeEnum.Chat,
        eventCondition:["Type:NPCDialogueAnnouncements","Message:regex:^发现入侵者.*","Sender:复制系统"])]
    public void Reset_OrderToFire_攻击命令重置(Event @event, ScriptAccessory accessory) {
        // 根据开场台词重置
        // 注意开头添加了变量和Init函数 Init函数是在每次整个副本reset时调用的
        timeOrderToFire=0;
    }
    
    
    [ScriptMethod(name: "20 盾龙复制体_连线预兆", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0016"])]
    public void 盾龙复制体_连线预兆(Event @event, ScriptAccessory accessory)
    {        
        //开场 第1次连线3根、第2次连线5根 实体在场外，需要作一定偏移，之后正常
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50);
        dp.Radian = 30f.DegToRad();

        if (timeOrderToFire <= 7)
        {
            dp.Name = "盾龙复制体_开场连线";
            dp.Offset = new Vector3(0, 0, -22f);
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            
            dp.Name = "盾龙复制体_开场连线2";
            dp.Offset = new Vector3(0, 0, 0f);
            dp.Delay = 4500;
            dp.DestoryAt = 2200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            //accessory.Method.SendChat($"/e 调试信息 timeOrder={timeOrderToFire}");
        }

        else
        {
            dp.Name = "盾龙复制体_连线预兆";
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            //accessory.Method.SendChat($"/e 调试信息 timeOrder={timeOrderToFire}");
        }

        ++timeOrderToFire;
        
        /* 有点BUG 先放着
        // 小龙被连线 0016 时会有一个技能ID为 32552 的 ActionEffect
        // 开场小龙被连线 跳下来时疑似有个32554的ActionEffect，跳下来后约 3.2s 开始读条火焰吐息
        // 仇恨栏出现读条时候，同时会有一个1.7s ID为 32544 的火焰吐息读条，意义不明但是伤害类型为魔法伤害
        // 仇恨栏看到的 3.2s ID为 32864 的火焰吐息 为真实伤害读条（且有约 1s 的可见omen）
        // 在【攻击命令 32543】读条时需显示 6.5s ，如果没有在读条 【攻击命令 32543】 ，则显示 4s
        // 连续攻击命令：32545
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50);
        dp.Radian = float.Pi / 6;

        if (Svc.Objects.Any(x => x is IBattleChara y && y.CastActionId == 32543))
        {
            dp.Name = "盾龙复制体_单次预兆";
            dp.DestoryAt = 6500;
        }
        else
        {
            dp.Name = "盾龙复制体_连续预兆";
            dp.DestoryAt = 4000;
        }

        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        */
    }
    
    [ScriptMethod(name: "20 盾龙复制体_火焰吐息", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32864"])]
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
    #endregion
    
    #region 21~30层 小怪
    [ScriptMethod(name: "—————— 21 ~ 30 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第21层(Event @event, ScriptAccessory accessory) { }
    #endregion

    #region 30 BOSS 提亚马特复制体
    #endregion

    #region 31~40层 小怪
    [ScriptMethod(name: "—————— 31 ~ 40 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第31层(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 40 BOSS 双塔尼亚复制体
    #endregion

    #region 41~50层 小怪
    [ScriptMethod(name: "—————— 41 ~ 50 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第41层(Event @event, ScriptAccessory accessory) { }
    #endregion

    #region 50 BOSS 自控化奇美拉
    #endregion

    #region 51~60层 小怪
    [ScriptMethod(name: "—————— 51 ~ 60 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第51层(Event @event, ScriptAccessory accessory) { }
    #endregion

    #region 60 BOSS 自控化弥诺陶洛斯
    #endregion

    #region 61~70层 小怪
    [ScriptMethod(name: "—————— 61 ~ 70 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第61层(Event @event, ScriptAccessory accessory) { }
    #endregion

    #region 70 BOSS 永恒
    #endregion

    #region 71~80层 小怪
    [ScriptMethod(name: "—————— 71 ~ 80 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第71层(Event @event, ScriptAccessory accessory) { }
    

    [ScriptMethod(name: "正统曙象 怒踏（钢铁）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:33191"])]
    public void 正统曙象_怒踏(Event @event, ScriptAccessory accessory)
    {
        //33191 猛进 释放完2s后 读条 33192怒踏 1.2s
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统曙象_怒踏{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    
    /* 备用
    [ScriptMethod(name: "正统曙象 怒踏（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33192"])]
    public void 正统曙象_怒踏(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统曙象_怒踏{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 1200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    */
    
    [ScriptMethod(name: "正统巨嘴鸟 减速咒（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32635"])]
    public void 正统巨嘴鸟_减速咒(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = $"正统巨嘴鸟_减速咒{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "正统狼獾 杀人爪（二连顺劈）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:33193"])]
    public void 正统狼獾_杀人爪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = $"正统狼獾_杀人爪{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 3000;  // 缓速情况下 需要绘制 11200
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "正统焰兽 怒吼（穿墙直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33199"])]
    public void 正统焰兽_怒吼(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统焰兽_怒吼{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Scale = new (20, 40f);
        dp.Owner = @event.SourceId();
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "正统斯卡尼特 唧唧咋咋（睡眠钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32637"])]
    public void 正统斯卡尼特_唧唧咋咋(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统斯卡尼特_唧唧咋咋{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    /*
    [ScriptMethod(name: "正统卡尔加斯 凛冬之风（狂暴钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32785"])]
    public void 正统卡尔加斯_凛冬之风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统卡尔加斯_凛冬之风{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    */
    
    [ScriptMethod(name: "正统长须豹 回旋尾（扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32647"])]
    public void 正统长须豹_回旋尾(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = $"正统长须豹_回旋尾{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.Radian = 60f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "正统雷兽 尾镰（趴地钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32656"])]
    public void 正统雷兽_尾镰(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统雷兽_尾镰{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3200;  // 暂无缓速数据
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统雷兽 电火花（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32660"])]
    public void 正统雷兽_电火花(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统雷兽_电火花{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(12f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3200;  // 暂无缓速数据
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "正统大脚巨猿 吃香蕉", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33195"])]
    public void 正统大脚巨猿_吃香蕉(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统大脚巨猿_吃香蕉{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(2f);
        dp.DestoryAt = 6700; 
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统大脚巨猿 吃香蕉销毁", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32680"], userControl: false)]
    public void 正统大脚巨猿_吃香蕉销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"正统大脚巨猿_吃香蕉{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "正统大脚巨猿 捶胸（脱战钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32680"])]
    public void 正统大脚巨猿_捶胸(Event @event, ScriptAccessory accessory)
    {
        //33195 吃香蕉 1.7s ； 32680 捶胸 1.7s ； 伤害提高 61
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统大脚巨猿_捶胸{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(52f);
        dp.DestoryAt = 1700; 
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统大脚巨猿 捶胸销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(6499|3266[12])$"], userControl: false)]
    public void 正统大脚巨猿_捶胸销毁(Event @event, ScriptAccessory accessory)
    {
        // 6499 平A攻击 ； 32661 打飞 ； 32662 殴打
        accessory.Method.RemoveDraw($"正统大脚巨猿_捶胸{@event.SourceId()}");
    }
    #endregion

    #region 80 BOSS 原形卡利亚
    [ScriptMethod(name: "80 原形卡利亚 共鸣（顺劈死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31422"])]
    public void 原形卡利亚_共鸣(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "原形卡利亚_共鸣";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Scale = new Vector2(12f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "80 原形卡利亚 气压领域 内圈即死范围", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31427|32930)$"])]
    public void 原形卡利亚_气压领域_内(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "原形卡利亚_气压领域_内";
        dp.Color = new Vector4(1f, 0f, 0f, 1.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 6900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "80 原形卡利亚 气压领域 外圈即死范围", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31427"])]
    public void 原形卡利亚_气压领域_外(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "原形卡利亚_气压领域_外";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.InnerScale = new Vector2(20f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "80 原形卡利亚 神经毒气喷射", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3293[3-5])$"])]
    public void 原形卡利亚_神经毒气喷射(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.DestoryAt = 5000;

    switch (@event.ActionId())
    {
        case 32933: 
            dp.Name = "前方神经毒气喷射";
            dp.Radian = 120f.DegToRad();
        break;

        case 32934: 
            dp.Name = "左侧神经毒气喷射";
            dp.Radian = 180f.DegToRad();
            dp.Rotation = 135f.DegToRad();
        break;

        case 32935: 
            dp.Name = "右侧神经毒气喷射";
            dp.Radian = 180f.DegToRad();
            dp.Rotation = 225f.DegToRad();
        break;
    }
    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
}
    
    [ScriptMethod(name: "80 原形卡利亚 环状神经毒气喷射", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32930"])]
    public void 原形卡利亚_环状神经毒气喷射(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "原形卡利亚_环状神经毒气喷射";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "炮击无人机 自动火炮（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31432"])]
    public void 炮击无人机_自动火炮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "炮击无人机_自动火炮";
        dp.Scale = new (5, 43f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    #endregion

    #region 81~90层 小怪
    [ScriptMethod(name: "—————— 81 ~ 90 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第81层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "正统卡托布莱帕斯 恶魔之瞳（脱战背对）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32762"])]
    public void 正统卡托布莱帕斯_恶魔之瞳(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统卡托布莱帕斯_恶魔之瞳{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(33f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统卡托布莱帕斯_恶魔之瞳打断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:32762"], userControl: false)]
    public void 正统卡托布莱帕斯_恶魔之瞳打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"正统卡托布莱帕斯_恶魔之瞳{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "正统深瞳 绝叫（易伤）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:32738"])]
    public void 正统深瞳_绝叫(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统深瞳_绝叫{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Delay = 16000;   // 表格的Recast为18000，考虑到误差故此设置
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统深瞳 绝叫销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:32738"], userControl: false)]
    public void 正统深瞳_绝叫销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"正统深瞳_绝叫{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "正统百目妖 凶眼（背对）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32731"])]
    public void 正统百目妖_凶眼(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统百目妖_凶眼{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统地生人 三重强击（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32740"])]
    public void 正统地生人_三重强击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = $"正统地生人_三重强击{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "正统幽灵 尖啸（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32772"])]
    public void 正统幽灵_尖啸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统百目妖_凶眼{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统黑天马 天马嘶啸（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32768"])]
    public void 正统黑天马_天马嘶啸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统黑天马_天马嘶啸{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 3200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统铁面腐尸 追打 TTS", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:32757"])]
    public void 正统铁面腐尸_追打(Event @event, ScriptAccessory accessory)
    {
        if(isTTS) accessory.Method.TTS("远离正面");
        if(isEdgeTTS) accessory.Method.EdgeTTS("远离正面");
        // 如果在此技能就开始画图会过早显示，延迟的话也就显示个半秒反而不好看，故只留TTS作提醒
    }
    

    [ScriptMethod(name: "正统铁面腐尸 挥舞（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32759"])]
    public void 正统铁面腐尸_挥舞(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = $"正统铁面腐尸_挥舞{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }

    [ScriptMethod(name: "正统贪吃鬼 发霉喷嚏（顺劈）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:32748"])]
    public void 正统贪吃鬼_发霉喷嚏(Event @event, ScriptAccessory accessory)
    {
        //此处捕获的为 “吸气” 以更早触发提示 
        if(isTTS) accessory.Method.TTS("远离正面");
        if(isEdgeTTS) accessory.Method.EdgeTTS("远离正面");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统贪吃鬼_发霉喷嚏{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6);
        dp.Radian = 120f.DegToRad();
        dp.Delay = 1000;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    /*
    [ScriptMethod(name: "正统贪吃鬼 发霉喷嚏（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32749"])]
    public void 正统贪吃鬼_发霉喷嚏(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = $"正统贪吃鬼_发霉喷嚏{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    */
    
    [ScriptMethod(name: "正统妖影 左侧横扫", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32776"])]
    public void 正统妖影_左侧横扫(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = $"正统妖影_左侧横扫{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.Radian = 210f.DegToRad();
        dp.Rotation = 75f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "正统妖影 右侧横扫", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32777"])]
    public void 正统妖影_右侧横扫(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = $"正统妖影_右侧横扫{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.Radian = 210f.DegToRad();
        dp.Rotation = -75f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "正统妖影 圆形爆发（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32774"])]
    public void 正统妖影_圆形爆发(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统妖影_圆形爆发{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统妖影 环形爆发（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32775"])]
    public void 正统妖影_环形爆发(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统妖影_环形爆发{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(7f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    #endregion
    
    #region 90 BOSS 管理者
    [ScriptMethod(name: "90 协作程序 魔科学射线α（蛋扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31451"])]
    public void 协作程序_魔科学射线α(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "协作程序_魔科学射线α";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50);
        dp.Radian = 120f.DegToRad();
        dp.Delay = 2500;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "90 协作程序 魔科学射线β（方块激光）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31452"])]
    public void 协作程序_魔科学射线β(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "协作程序_魔科学射线β";
        dp.Scale = new (5, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = 2500;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "90 协作程序 魔科学射线γ（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31453"])]
    public void 协作程序_魔科学射线γ(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "协作程序_魔科学射线γ";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 2500;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "90 协作程序 魔科学射线γ（月环内径）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31453"])]
    public void 协作程序_魔科学射线γ内径(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "协作程序_魔科学射线γ内径";
        dp.Color = new Vector4(0f, 1f, 0f, 8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8.08f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    //齐射程序 的直线激光，可以考虑抓 BOSS本体读条 31455 delay（或者不用）来给边缘描个框 以作提前绘制 [?
    [ScriptMethod(name: "90 齐射程序 魔科学射线α（蛋扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32832"])]
    public void 齐射程序_魔科学射线α(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "齐射程序_魔科学射线α";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "90 杀戮程序 魔科学射线β（方块激光）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32833"])]
    public void 杀戮程序_魔科学射线β(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "杀戮程序_魔科学射线β";
        dp.Scale = new (5, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = 4300;
        dp.DestoryAt = 3400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "90 管理者 环状射线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31447"])]
    public void 管理者_环状射线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "管理者_环状射线";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 2700;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "90 管理者 十字射线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31448"])]
    public void 管理者_十字射线(Event @event, ScriptAccessory accessory)
    {
        var currentProperty = accessory.Data.GetDefaultDrawProperties();
        currentProperty.Name = "管理者_十字射线";
        currentProperty.Scale = new (10, 60f);
        currentProperty.Owner = @event.SourceId();
        currentProperty.Color = accessory.Data.DefaultDangerColor;
        currentProperty.Delay = 2700;
        currentProperty.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, currentProperty);  

        //旋转90°，使其绘制结果为一个十字
        for(int i=1;i<=2;++i) {
            accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Straight,currentProperty);
            currentProperty.Rotation+=90f.DegToRad();
        }
        
    }
    
    [ScriptMethod(name: "90 管理者 执行杀戮程序 诱导黄圈提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31454"])]
    public void 管理者_执行杀戮程序(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("诱导五连AOE", duration: 3000, true);
        accessory.Method.TTS("诱导五连AOE");
    }
    #endregion
    
    #region 91~100层 小怪
    [ScriptMethod(name: "—————— 91 ~ 100 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第91层(Event @event, ScriptAccessory accessory) { }
    
    
    [ScriptMethod(name: "正统系统γ 高压电流（打断钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32878"])]
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
    
    [ScriptMethod(name: "正统系统γ 排斥炮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32877"])]
    public void 正统系统γ_排斥炮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统系统γ_排斥炮{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统系统γ 环形炮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32876"])]
    public void 正统系统γ_环形炮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统系统γ_环形炮{@event.SourceId()}";
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

        dp.Name = $"正统系统α_魔科学射线α{@event.SourceId()}";
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

        dp.Name = $"正统采掘无人机_魔科学炮{@event.SourceId()}";
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
        dp.Name = $"正统无人机_雾散爆发{@event.SourceId()}";
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
        dp.Name = $"正统斯芬克斯_重击{@event.SourceId()}";
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
        dp.Name = $"正统恐慌装甲_猛扫{@event.SourceId()}";
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
        dp.Name = $"正统米特里达梯_激光剑{@event.SourceId()}";
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
        dp.Name = $"正统浮游炮主板_攻城炮{@event.SourceId()}";
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

    [ScriptMethod(name: "正统扎戈斧龙 XXX突袭（击退点名）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32888"])]
    public void 正统扎戈斧龙_XXX突袭(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统扎戈斧龙_XXX突袭{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0.4f, 0f, 1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
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
    
    [ScriptMethod(name: "正统自控化弥诺陶洛斯 百廿八吨回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32922"])]
    public void 正统自控化弥诺陶洛斯_百廿八吨回转(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统自控化弥诺陶洛斯_百廿八吨回转{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 3200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统自控化弥诺陶洛斯 卅二吨重击（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32921"])]
    public void 正统自控化弥诺陶洛斯_卅二吨重击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统自控化弥诺陶洛斯_卅二吨重击{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(35);
        dp.Radian = 60f.DegToRad();
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "正统自控化奇美拉 雷鸣吐息（左上顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32907"])]
    public void 正统自控化奇美拉_雷鸣吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统自控化奇美拉_雷鸣吐息{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.Radian = 120f.DegToRad();
        dp.Rotation = 30f;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "正统自控化奇美拉 寒冰吐息（右上顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32906"])]
    public void 正统自控化奇美拉_寒冰吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统自控化奇美拉_寒冰吐息{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.Radian = 120f.DegToRad();
        dp.Rotation = -30f;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "正统自控化奇美拉 蝎尾毒刺（背后扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32908"])]
    public void 正统自控化奇美拉_蝎尾毒刺(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统自控化奇美拉_蝎尾毒刺{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7);
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f;
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "正统自控化奇美拉 寒冰咆哮（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32909"])]
    public void 正统自控化奇美拉_寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统自控化奇美拉_寒冰咆哮{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f);
        dp.DestoryAt = 3200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "正统自控化奇美拉 雷电咆哮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32910"])]
    public void 正统自控化奇美拉_雷电咆哮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"正统自控化奇美拉_雷电咆哮{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(6f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    #endregion

    #region 99 BOSS 王者之剑
    [ScriptMethod(name:"Reset_Caliburni_石中剑重置",userControl:false,eventType:EventTypeEnum.Chat,
        eventCondition:["Type:NPCDialogueAnnouncements","Message:regex:^连接良好.*","Sender:斗神 王者之剑"])]
    public void Reset_Caliburni_石中剑重置(Event @event, ScriptAccessory accessory) {
        // 根据开场台词重置
        // 注意开头添加了变量和Init函数 Init函数是在每次整个副本reset时调用的
        timesCaliburniHasBeenCast=0;
    }
    
    [ScriptMethod(name: "99 石中剑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31333"])]
    public void 石中剑_直线(Event @event, ScriptAccessory accessory)
    {
        //本体无意义读条：31333，组合对应的飞剑强袭直线读条：31334
        //开场为正面10个直线，且顺时针为6°，逆时针为4°，间隔10°，左5右5
        //之后为3组共15个直线，每个直线固定间隔10°，每组固定间隔120°； 
        
        var dp=accessory.Data.GetDefaultDrawProperties();

        if(timesCaliburniHasBeenCast<=0) {

            for(int i=1,currentRotation=4;i<=4;++i,currentRotation+=10) {
                // 开场 左4
                // -4 -14 -24 -34
                dp=accessory.Data.GetDefaultDrawProperties();
                
                dp.Name="石中剑_开场";
                dp.Scale=new(4,20f);
                dp.Owner=@event.SourceId();
                dp.Color=accessory.Data.DefaultDangerColor;
                dp.DestoryAt=4700;
                dp.Rotation=((float)currentRotation).DegToRad();
                
                accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);  
            }
            
            for(int i=1,currentRotation=-6;i<=6;++i,currentRotation-=10) {
                // 开场 右6
                // 6 16 26 36 46 56
                dp=accessory.Data.GetDefaultDrawProperties();
                
                dp.Name="石中剑_开场";
                dp.Scale=new(4,20f);
                dp.Owner=@event.SourceId();
                dp.Color=accessory.Data.DefaultDangerColor;
                dp.DestoryAt=4700;
                dp.Rotation=((float)currentRotation).DegToRad();
                
                accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);  
            }
            
        }

        else {
            
            for(int axisRotation=0;axisRotation<360;axisRotation+=120) {
                // 一共三组 中轴线相隔120度
                // 0 120 240
                // 注意axisRotation<360所以360不会被执行 一共三次

                for(int i=1,currentRotation=axisRotation-20;i<=5;++i,currentRotation+=10) {
                    // 分别是当前中轴的-20 -10 0 10 20度
                    
                    dp=accessory.Data.GetDefaultDrawProperties();
                
                    dp.Name="石中剑_战斗途中";
                    dp.Scale=new(4,20f);
                    dp.Owner=@event.SourceId();
                    dp.Color=accessory.Data.DefaultDangerColor;
                    dp.DestoryAt=4700;
                    dp.Rotation=((float)currentRotation).DegToRad();
                
                    accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp); 
                    
                }

            }
            
        }
        
        ++timesCaliburniHasBeenCast;
        
    }
    
    [ScriptMethod(name: "99 王者之剑 魂剑之实（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31327"])]
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
    
    [ScriptMethod(name: "99 王者之剑 魂剑之虚（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31328"])]
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
    
    [ScriptMethod(name: "99 真空斩（四向顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31342"])]
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


    [ScriptMethod(name: "99 次元斩（扇环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31339|3134[01])$"])]
    public void 次元斩(Event @event, ScriptAccessory accessory)
    {
        if (@event["ActionId"].Equals("31339"))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "次元斩内"; //31339
            dp.Color = new Vector4(1f, 0f, 0f, 1.8f);
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(7);
            dp.InnerScale = new Vector2(2);
            dp.Radian = 180f.DegToRad();
            dp.DestoryAt = 6700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }

        if (@event["ActionId"].Equals("31340"))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "次元斩中"; //31340
            dp.Color = new Vector4(1f, 0f, 0f, 1.6f);
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(12);
            dp.InnerScale = new Vector2(7);
            dp.Radian = 180f.DegToRad();
            dp.DestoryAt = 6700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }

        if (@event["ActionId"].Equals("31341"))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "次元斩外"; //31341
            dp.Color = new Vector4(1f, 0f, 0f, 1.6f);
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(22);
            dp.InnerScale = new Vector2(17f);
            dp.Radian = 180f.DegToRad();
            dp.DestoryAt = 6700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
    }


    [ScriptMethod(name: "99 极寒冰川 (冰花) ", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31347"])]
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
    
    [ScriptMethod(name: "99 极热炎流（地火）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31344"])]
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
    #endregion

    #region 底裤部分
    
    [ScriptMethod(name: "—————— 底裤部分（需要对应插件与权限） ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 底裤部分(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "[DR] 变身恐慌装甲时，移速改为1.5倍", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:565"])]
    public void AddProtonaught(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.5");
        accessory.Method.SendChat($"/e 可达鸭：移速已更改：1.5x");
        if (isTTS)accessory.Method.TTS("移速已更改至1.5倍");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已更改至1.5倍");
    }
    
    [ScriptMethod(name: "[DR] 恐慌装甲取消时，移速复原至1倍", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:565"])]
    public void RemoveProtonaught(Event @event, ScriptAccessory accessory)
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




#region 计算函数

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

#endregion 计算函数