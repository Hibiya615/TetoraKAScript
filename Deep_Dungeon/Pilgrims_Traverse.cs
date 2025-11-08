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
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Pilgrims_Traverse;

[ScriptType(guid: "3f65b3c0-df48-4ef8-89ae-b8091b7690f1", name: "朝圣交错路", author: "Tetora", 
    territorys: [1281, 1282, 1283, 1284, 1285, 1286, 1287, 1288, 1289, 1290, 1311, 1333],
    version: "0.0.0.6",note: noteStr)]

public class Pilgrims_Traverse
{
    const string noteStr =
        """
        v0.0.0.6:
        朝圣交错路测试绘制
        未全部测试，可能部分有误，更新日志见dc
        注：方法设置中的层数仅做分割线效果，并不是批量开关
        出现问题请携带ARR反馈！
        """;

    
    #region 各种记录
    
    /*  StatusID
     *  变身 4708 [StackCount:54 Param:566 泥球 ]  [StackCount:55 Param:567 爆弹之母 ]
     *  极速冲刺 4709
     *  禁止使用杜松香 4591
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
    
    /*  DataId
     *  传送装置 2014756
     *  再生祭坛 2014755
     *  光耀烛台 2014759
     *  埋藏的宝藏 2007542
     * 
     *  金宝箱 2007358
     *  银宝箱 2007357
     *  铜宝箱（拟态怪）2006020
     *  铜宝箱  [1~10层] 1882        [11~20层] 1881       [21~30层] 1883 1884   [31~40层] 1885 1886   [41~50层] 1887 1888
     *         [51~60层] 1889 1906  [61~70层] 1890 1907  [71~80层] 1891 1908   [81~90层] 1892        [91~100层] 1893
     *
     *  地雷陷阱 2007182
     *  诱饵陷阱 2007183
     *  弱化陷阱 2007184
     *  妨碍陷阱 2007185
     *  妖灵陷阱 2014939
     */
    
    #endregion
    
    #region 基础控制
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    // [UserSetting("测试性功能（慎用）")]
    // public bool isTest { get; set; } = false;
    
    [UserSetting("启用小工具（已确认设置完毕）")]
    public bool isMiniTools { get; set; } = false;
    
    [UserSetting(note: "选择自动苏生之炎对象")]
    public RekindleEnum Rekindle { get; set; } = RekindleEnum.TargetsTarget;
    
    public enum RekindleEnum
    {
        [Description("<tt>")]
        TargetsTarget = 0,
        [Description("<2>")]
        PartyList2 = 1,
        [Description("<me>")]
        Me = 2,
    }
    
    [UserSetting("启用底裤（需要对应插件与权限）")]
    public bool isHack { get; set; } = false;
    
    [UserSetting(note: "选择默认遁地深度")]
    public DepthsEnum Depths { get; set; } = DepthsEnum.Default;
    
    public enum DepthsEnum
    {
        [Description("0")]
        Default = 0,
        [Description("2")]
        Depths2 = 1,
        [Description("3")]
        Depths3 = 2,
        [Description("5")]
        Depths5 = 3,
        [Description("7")]
        Depths7 = 4,
        [Description("20")]
        Depths20 = 5,
        [Description("50")]
        Depths50 = 6,
    }
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion
    
    #region 全局变量
    
    public static class MapIds
    {
        public const uint PilgrimsTraverse0 = 1281; // 第 1~10朝圣路
        public const uint PilgrimsTraverse1 = 1282;
        public const uint PilgrimsTraverse2 = 1283;
        public const uint PilgrimsTraverse3 = 1284;
        public const uint PilgrimsTraverse4 = 1285;
        public const uint PilgrimsTraverse5 = 1286;
        public const uint PilgrimsTraverse6 = 1287;
        public const uint PilgrimsTraverse7 = 1288;
        public const uint PilgrimsTraverse8 = 1289;
        public const uint PilgrimsTraverse9 = 1290;
        public const uint TheFinalVerse = 1333; // 卓异的悲寂歼灭战
        public const uint TheFinalVerseQuantum = 1311; // 卓异的悲寂深想战
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
        accessory.Method.RemoveDraw($".*{@event.SourceId()}");
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
    
    
    /*
     
    [ScriptMethod(name: "死亡重置", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:0"], userControl: false)]
    public void 死亡重置(Event @event, ScriptAccessory accessory)
    {
        if (isDeveloper) accessory.Method.SendChat($"/e 调试信息：检测到死亡，已重置变量");
    }
    
    */
    
    #endregion
    
    // 通用内容
    [ScriptMethod(name: "拟态怪_怨念提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44852"])]
    public void 拟态怪_怨念(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("打断拟态怪", duration: 2000, true);
        if (isTTS) accessory.Method.TTS("打断拟态怪");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断拟态怪");
    }
    
    #region 小工具部分
    
    [ScriptMethod(name: "—————— 小工具部分（先自行关闭不需要的功能） ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 小工具部分(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "自身爆弹之母 大爆炸范围绘制", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44629"])]
    public void 爆弹之母_大爆炸Self (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"爆弹之母_大爆炸{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 1500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    /*
    [ScriptMethod(name: "队友爆弹之母 大爆炸范围绘制", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44629"])]
    public void 爆弹之母_大爆炸Party (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me) return; 

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"爆弹之母_大爆炸{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 1500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    */
    
    [ScriptMethod(name: "自动取消二段火神冲（防止遁地打不到导致自动循环卡死）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4403"])]
    public void AutoRemoveCrimsonStrike(Event @event, ScriptAccessory accessory)
    {
        if(!isMiniTools || @event.TargetId() != accessory.Data.Me) return;
        accessory.Method.SendChat($"/statusoff 深红强袭预备");
        if (isDeveloper) accessory.Method.SendChat($"/e 鸭鸭：已取消《深红强袭预备》");
    }
    
    [ScriptMethod(name: "自动尝试挂不死鸟热水（遁地了也很team）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1868"])]
    public void AutoUseRekindle(Event @event, ScriptAccessory accessory)
    {
        string rekindleValue = Rekindle.GetDescription();
        
        if(!isMiniTools || @event.TargetId() != accessory.Data.Me) return;
        accessory.Method.SendChat($"/ac 星极超流 {rekindleValue}");
        accessory.Method.SendChat($"/e 鸭鸭：已尝试自动给《{rekindleValue}》挂上不死鸟热水");
    }
    
    #endregion
    
    #region  1~10层 
    [ScriptMethod(name: "—————— 1 ~ 10 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第1层(Event @event, ScriptAccessory accessory) { }
    
        
    [ScriptMethod(name: "10 花小人_百花齐放（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44855"])]
    public void 花小人_百花齐放 (Event @event, ScriptAccessory accessory)
    {
        // 44054 为无意义读条 (应是黄圈生成的过程), 伤害源为 44855 二者读条时间不同
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"花小人_百花齐放";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14f);
        dp.DestoryAt = 13200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"花小人_百花齐放描边";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(14f);
        dp1.InnerScale = new Vector2(13.9f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 13200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "10 花人_压花（跳跃钢铁）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:44058"])]
    public void 花人_压花 (Event @event, ScriptAccessory accessory)
    {
        // 花人本体读条 [ ActionId: 44055 ; Cast 9.7s] ,四连跳跃标记为 [ ActionId: 44058 ] 约在释放后 8.9s 造成伤害 每次间隔约 1.8s, 伤害源 [ ActionId: 44057 ; Cast 1.6s]
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"花人_压花";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(15f);
        dp.Delay = 2700;
        dp.DestoryAt = 6200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"花人_压花描边";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Position = @event.EffectPosition();
        dp1.Scale = new Vector2(15f);
        dp1.InnerScale = new Vector2(14.94f);
        dp1.Radian = float.Pi * 2;
        dp1.Delay = 2700;
        dp1.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
        
    }
    
    #endregion
    
    #region  11~20层 
    [ScriptMethod(name: "—————— 11 ~ 20 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第11层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "20 得到宽恕的模仿_爆炸（四连钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4345[6-9]$"])]
    public void 得到宽恕的模仿_爆炸 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的模仿_爆炸";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(11f);
        
        switch (@event.ActionId())
        {
            case 43456:
                dp.Delay = 0;
                dp.DestoryAt = 1700;
                break;
            case 43457:
                dp.Delay = 1700;
                dp.DestoryAt = 1300;
                break;
            case 43458:
                dp.Delay = 3000;
                dp.DestoryAt = 1300;
                break;
            case 43459:
                dp.Delay = 4300;
                dp.DestoryAt = 1300;
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "20 树根_缠绕（冰花）", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2014795", "Operate:Add"])]
    public void 树根_缠绕(Event @event, ScriptAccessory accessory)
    {
        // 放置的头标点名 TargetIconId: 0017 , 缠绕 ActionId: 43462 
        float[] degrees = { 0f, 45f, 90f, 135f };
    
        foreach (float degree in degrees)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"树根_缠绕_{degree}°";
            dp.Scale = new (6f, 60f);
            dp.Owner = @event.SourceId();
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
            dp.DestoryAt = 19200;
            dp.Rotation = degree * (float)Math.PI / 180f;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    
    #endregion
    
    #region 21~30层
    [ScriptMethod(name: "—————— 21 ~ 30 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第21层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "フォーギヴン・トレチャリー_光耀讃詞（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3964[2-4]$"])]
    public void 光耀讃詞 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・トレチャリー_光耀讃詞";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        
        switch (@event.ActionId())
        {
            case 39642:
                dp.Scale = new Vector2(14f);
                dp.InnerScale = new Vector2(9f);
                break;
            case 39643:
                dp.Scale = new Vector2(19f);
                dp.InnerScale = new Vector2(14f);
                break;
            case 39644:
                dp.Scale = new Vector2(24f);
                dp.InnerScale = new Vector2(19f);
                break;
        }
        
        dp.Radian = float.Pi * 2;
        dp.Delay = 7000;
        dp.DestoryAt = @event.DurationMilliseconds() - 7000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "フォーギヴン・トレチャリー_救済の拳（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40401|40549)$"])]
    public void 救済の拳(Event @event, ScriptAccessory accessory)
    {
        // 右刀 40401 ； 左刀 40549
        // var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
        var isR = @event.ActionId == 40401;
        
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        var hitboxRadius = IbcHelper.GetHitboxRadius(obj);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・トレチャリー_救済の拳";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f + hitboxRadius); // 30m + 目标圈
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    #endregion
    
    #region  31~40层
    [ScriptMethod(name: "—————— 31 ~ 40 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第31层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "トラバース・ペガサス_天馬のいななき（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44703"])]
    public void 天馬のいななき (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・ペガサス_天馬のいななき{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "フォーギヴン・ナイヴテイ_ブレッシングブロウ(黄)（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42129"])]
    public void ナイヴテイ_ブレッシングブロウ (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ナイヴテイ_ブレッシングブロウ(黄)";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "フォーギヴン・ナイヴテイ_ラウンドタイド（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45169"])]
    public void ラウンドタイド (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ナイヴテイ_ラウンドタイド";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "フォーギヴン・ナイヴテイ_リングタイド（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45170"])]
    public void リングタイド (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ナイヴテイ_リングタイド";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(28f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "フォーギヴン・ペチュランス_テンタクル（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4469[01]$"])]
    public void テンタクル(Event @event, ScriptAccessory accessory)
    {
        // 右刀 44691 ； 左刀 44690
        var isR = @event.ActionId == 44691;
        
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ペチュランス_テンタクル";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f + IbcHelper.GetHitboxRadius(obj)); // 60m + 目标圈
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "トラバース・ストーンライオン_キャッチクロウ（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44698"])]
    public void キャッチクロウ(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・ストーンライオン_キャッチクロウ{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6.5f + IbcHelper.GetHitboxRadius(obj)); // 6.5m + 目标圈
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "トラバース・インクイジター_葬送击（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44694"])]
    public void 葬送击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・インクイジター_葬送击{@event.SourceId()}";
        dp.Scale = new (2f, 21f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    #endregion
    
    #region  41~50层
    [ScriptMethod(name: "—————— 41 ~ 50 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第41层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "トラバース・ウェポン_怒りの旋風（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44715"])]
    public void 怒りの旋風 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・ウェポン_怒りの旋風{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "トラバース・トルバ_トータスストンプ（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41724"])]
    public void トータスストンプ (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・トルバ_トータスストンプ{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region  51~59层
    [ScriptMethod(name: "—————— 51 ~ 60 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第51层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "トラバース・ギガント_ヘビーラリアット（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44737"])]
    public void ヘビーラリアット (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・ギガント_ヘビーラリアット{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "トラバース・サンドサーペント_アースオーガー（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42091"])]
    public void アースオーガー (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・サンドサーペント_アースオーガー{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 270f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "トラバース・ハウラー_サイズテール（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44731"])]
    public void サイズテール (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・ハウラー_サイズテール{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "トラバース・ハウラー_外环雷（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44732"])]
    public void 外环雷(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "トラバース・ハウラー_外环雷";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "トラバース・サイカニア_トラウンス（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42147"])]
    public void トラウンス (Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・サイカニア_トラウンス{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f + IbcHelper.GetHitboxRadius(obj)); // 40m + 目标圈
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "トラバース・サイカニア_マイティスピン（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42148"])]
    public void マイティスピン (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・サイカニア_マイティスピン{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region 60 BOSS 始祖马利克巨人掌
    
    [ScriptMethod(name: "60 始祖马利克巨人掌_飞针射击（前后扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44866"])]
    public void 始祖马利克巨人掌_飞针射击 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"始祖马利克巨人掌_飞针射击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
    
        float[] rotations = { 0f, 180f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }
    
    [ScriptMethod(name: "60 始祖马利克巨人掌_飞针回旋（旋转扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44868"])]
    public void 始祖马利克巨人掌_飞针回旋 (Event @event, ScriptAccessory accessory)
    {
        // 飞针回旋 [ ActionId: 44868 - Cast: 4.7s ] ; 共判定 10 次，每次旋转 60° , 间隔约 1s , 伤害来源 ActionId: 44909 ；
        // 顺时针旋转 [ TargetIcon: 00A7 ] ; 逆时针旋转 [ TargetIcon: 00A7 ] 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"始祖马利克巨人掌_飞针回旋";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = 15200; 
    
        float[] rotations = { 0f, 180f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }
    
    [ScriptMethod(name: "60 始祖马利克巨人掌_分株（仙人掌方形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4485[89]$"])]
    public void 始祖马利克巨人掌_分株(Event @event, ScriptAccessory accessory)
    {
        // 本体读条: 44857
        var isBig = @event.ActionId == 44859;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"始祖马利克巨人掌_分株";
        dp.Scale = isBig? new (30f, 30f): new (10f, 10f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    #endregion
    
    #region  61~70层
    [ScriptMethod(name: "—————— 61 ~ 70 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第61层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "トラバース・ノーム_プレインパウン（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44753"])]
    public void プレインパウン (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・ノーム_プレインパウン{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "トラバース・クリフモール_ヘッドバット（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44740"])]
    public void ヘッドバット(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・クリフモール_ヘッドバット{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f + IbcHelper.GetHitboxRadius(obj)); // 6m + 目标圈
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "交错路塔罗斯_地层俯冲（二段月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42516"])]
    public void サブダックション (Event @event, ScriptAccessory accessory)
    {
        // 在 执行贯穿 [ActionId:42516 / 3.7s] 4s后 显示第2段月环  两次判定间隔约3.1s
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・タロース_サブダックション{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(11f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 4000;
        dp.DestoryAt = 3100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "フォーギヴン・ダウト_ボディプレス（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44748"])]
    public void ボディプレス (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ダウト_ボディプレス{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "得到宽恕的暴动_左/右侧震荡波（二连左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4221[46]$"])]
    public void 得到宽恕的暴动_左右侧震荡波(Event @event, ScriptAccessory accessory)
    {
        // 先右刀 42214 ； 先左刀 42216
        var isR = @event.ActionId == 42214;
        
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的暴动_左右侧震荡波1{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f + IbcHelper.GetHitboxRadius(obj)); // 30m + 目标圈
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"得到宽恕的暴动_左右侧震荡波2{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(30f + IbcHelper.GetHitboxRadius(obj));
        dp1.Radian = 180f.DegToRad(); 
        dp1.Rotation = isR ? 90f.DegToRad() : 270f.DegToRad();
        dp1.Delay = @event.DurationMilliseconds();
        dp1.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }
    
    [ScriptMethod(name: "フォーギヴン・グラッジ_ヘイルファイア（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42515"])]
    public void ヘイルファイア(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・グラッジ_ヘイルファイア{@event.SourceId()}";
        dp.Scale = new (4f, 45f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "フォーギヴン・イムパリティー_ロックスライド（十字）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44743"])]
    public void ロックスライド(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・イムパリティー_ロックスライド{@event.SourceId()}";
        dp.Scale = new (10f, 80f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.DurationMilliseconds();
        
        float[] rotations = { 0f, 90f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    
    [ScriptMethod(name: "フォーギヴン・アタッチメント_スーウェッジウォーター（前后刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4475[01]$"])]
    public void スーウェッジウォーター(Event @event, ScriptAccessory accessory)
    {
        // 前刀 44750 ； 后刀 44751
        var isF = @event.ActionId == 44750;

        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・アタッチメント_スーウェッジウォーター";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f + IbcHelper.GetHitboxRadius(obj)); // ??m + 目标圈
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isF ? 0f.DegToRad() : 180f.DegToRad();
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "トラバース・クイーンビー_アンファイナルスティング（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42510"])]
    public void アンファイナルスティング(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・クイーンビー_アンファイナルスティング{@event.SourceId()}";
        dp.Scale = new (3f, 8f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "フォーギヴン・コンテンション_針幾千本（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42220"])]
    public void 針幾千本(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・コンテンション_針幾千本{@event.SourceId()}";
        dp.Scale = new (8f, 20f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "トラバース・ンゴツィ_土石流（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44755"])]
    public void 土石流(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・ンゴツィ_土石流{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f + IbcHelper.GetHitboxRadius(obj)); // 9m + 目标圈
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "フォーギヴン・ヴォラシティ_ストーンゲイズ（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44746"])]
    public void ストーンゲイズ (Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ヴォラシティ_ストーンゲイズ{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.7f + IbcHelper.GetHitboxRadius(obj)); // ??m + 目标圈
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "フォーギヴン・ヴォラシティ_ボディスラム（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44747"])]
    public void ボディスラム (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ヴォラシティ_ボディスラム{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    // 70 BOSS 得到宽恕的热忱
    
    [ScriptMethod(name: "フォーギヴン・ズィール_ズィールグラワー（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^434(06|11)$"])]
    public void ズィールグラワー(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ズィール_ズィールグラワー";
        dp.Scale = new (10f, 25f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    /*
    [ScriptMethod(name: "フォーギヴン・ズィール_ズィールグラワー_光球1", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^434(06|11|18|26)$"])]
    public void ズィールグラワー_光球1 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ズィール_ズィールグラワー_光球1";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1f);
        dp.DestoryAt = 1000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    */
    
    [ScriptMethod(name: "70 得到宽恕的热忱_光球 光轮（月环安全区）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43417"])]
    public void 得到宽恕的热忱_光球_光轮安全区 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的热忱_光球 光轮安全区";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"得到宽恕的热忱_光球 光轮描边";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(20f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(3f);
        dp1.InnerScale = new Vector2(2.95f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    /*
    [ScriptMethod(name: "70 得到宽恕的热忱_光球 光轮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43417"])]
    public void 得到宽恕的热忱_光球 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的热忱_光球 光轮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.InnerScale = new Vector2(3f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    */
    
    [ScriptMethod(name: "フォーギヴン・ズィール_アイ・オブ・ズィール（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:^434(18|26)$"])]
    public void ズィール (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ズィール_アイ・オブ・ズィール";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 5000;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "2000ミナ・スイング（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43429"])]
    public void スイング (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"2000ミナ・スイング";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region  71~80层
    [ScriptMethod(name: "—————— 71 ~ 80 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第71层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "フォーギヴン・アンビリーフ_グラベルシャワー（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44763"])]
    public void アンビリーフ_グラベルシャワー(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・アンビリーフ_グラベルシャワー{@event.SourceId()}";
        dp.Scale = new (4f, 10f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "交错路爆岩怪_炸裂（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42770"])]
    public void 交错路爆岩怪_炸裂 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路爆岩怪_炸裂{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "交错路爆岩怪_自爆（距离衰减）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42771"])]
    public void 交错路爆岩怪_自爆 (Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("远离自爆（距离衰减）", duration: 4000, true);
        if (isTTS)accessory.Method.TTS("远离自爆");
        if (isEdgeTTS)accessory.Method.EdgeTTS("远离自爆");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路爆岩怪_自爆{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "トラバース・フルドゥ_自爆（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44771"])]
    public void フルドゥ_自爆 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・フルドゥ_自爆{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "トラバース・シザージョウ_サンドブラスト（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44769"])]
    public void サンドブラスト(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・シザージョウ_サンドブラスト{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3.3f + IbcHelper.GetHitboxRadius(obj)); // ??m + 目标圈
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "得到宽恕的恶意_双水流（钢铁月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42578|42748)$"])]
    public void スパイト_双水流 (Event @event, ScriptAccessory accessory)
    {
        // 先钢铁: 42578  ; 先月环: 42748
        if (@event.ActionId == 42578)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"得到宽恕的恶意_圆环双水流_钢铁{@event.SourceId()}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(10);
            dp.DestoryAt = 4700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"得到宽恕的恶意_圆环双水流_月环{@event.SourceId()}";
            dp1.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
            dp1.Owner = @event.SourceId();
            dp1.Scale = new Vector2(40);
            dp1.InnerScale = new Vector2(10);
            dp1.Radian = float.Pi * 2;
            dp1.Delay = 4700;
            dp1.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
        }
        else
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"得到宽恕的恶意_环圆双水流_月环{@event.SourceId()}";
            dp1.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
            dp1.Owner = @event.SourceId();
            dp1.Scale = new Vector2(40);
            dp1.InnerScale = new Vector2(10);
            dp1.Radian = float.Pi * 2;
            dp1.DestoryAt = 4700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"得到宽恕的恶意_环圆双水流_钢铁{@event.SourceId()}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(10);
            dp.Delay = 4700;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "フォーギヴン・アロガンス_連蹴り（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44759"])]
    public void 連蹴り(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・アロガンス_連蹴り{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f + IbcHelper.GetHitboxRadius(obj)); // 6m + 目标圈
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "トラバース・ウォーム_地震（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44767"])]
    public void ウォーム_地震 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・ウォーム_地震{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "トラバース・アメミット_トップル（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44765"])]
    public void トップル (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・アメミット_トップル{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "フォーギヴン・ディセンション_トラウンス（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42522"])]
    public void ディセンション_トラウンス(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ディセンション_トラウンス{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f + IbcHelper.GetHitboxRadius(obj)); // 40m + 目标圈
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
        
    [ScriptMethod(name: "得到宽恕的诋毁_造山风暴（点名圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44762"])]
    public void 地火喷发(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "地火喷发";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "得到宽恕的诋毁_变质岩波（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44761"])]
    public void メタモーフィックブラスト(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・スランダー_メタモーフィックブラスト{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "得到宽恕的腐败_旋转火力（脱战大钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42523"])]
    public void 得到宽恕的腐败_旋转火力 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的腐败_旋转火力{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(45f);
        dp.DestoryAt = 15700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"得到宽恕的腐败_旋转火力描边{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(45f);
        dp1.InnerScale = new Vector2(44.94f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 15700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "フォーギヴン・コラプション_フロントバラージ（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42577"])]
    public void フロントバラージ(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・コラプション_フロントバラージ{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f + IbcHelper.GetHitboxRadius(obj)); // 5m + 目标圈
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "フォーギヴン・ヴァニティー_サークルレーザー（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44758"])]
    public void サークルレーザー (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ヴァニティー_サークルレーザー{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "フォーギヴン・ヴァニティー_クロスレーザー（十字）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44757"])]
    public void クロスレーザー(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ヴァニティー_クロスレーザー{@event.SourceId()}";
        dp.Scale = new (10f, 120f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.DurationMilliseconds();
        
        float[] rotations = { 0f, 90f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }

    
    // 80 BOSS
    
    [ScriptMethod(name: "80 フォーギヴン・プロファニティー_紫雷の輪戒（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43468"])]
    public void 紫雷の輪戒 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・プロファニティー_紫雷の輪戒";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(48f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "80 フォーギヴン・プロファニティー_痛傷の円戒（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43472"])]
    public void 痛傷の円戒 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・プロファニティー_痛傷の円戒";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "80 フォーギヴン・プロファニティー_冒涜の光（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43473"])]
    public void 冒涜の光(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・プロファニティー_冒涜の光";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f + IbcHelper.GetHitboxRadius(obj)); // 40m + 目标圈
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    /*
    [ScriptMethod(name: "フォーギヴン・プロファニティー_雷気循環（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43476"])]
    public void 雷気循環 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・プロファニティー_雷気循環";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    */
    
    // フォーギヴン・プロファニティー_死出の道行き
    
    #endregion
    
    #region  81~90层
    [ScriptMethod(name: "—————— 81 ~ 90 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第81层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "トラバース・ライダー_ストームスラッシュ（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43321"])]
    public void ストームスラッシュ(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・ライダー_ストームスラッシュ{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5.2f + IbcHelper.GetHitboxRadius(obj)); // ??m + 目标圈
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "インヴォークド・グレムリン_クロウ（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44770"])]
    public void クロウ(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"インヴォークド・グレムリン_クロウ{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(4.2f + IbcHelper.GetHitboxRadius(obj)); // ??m + 目标圈
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "トラバース・カブス_ダーラ（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44775"])]
    public void カブス_ダーラ(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・カブス_ダーラ{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f + IbcHelper.GetHitboxRadius(obj)); // 40m + 目标圈
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "トラバース・ノール_ノックスブラスト（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44774"])]
    public void ノックスブラスト(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・ノール_ノックスブラスト{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f + IbcHelper.GetHitboxRadius(obj)); // 7m + 目标圈
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "インヴォークド・アークデーモン_アビサルスイング（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44780"])]
    public void アビサルスイング(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"インヴォークド・アークデーモン_アビサルスイング{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6.7f + IbcHelper.GetHitboxRadius(obj)); // ??m + 目标圈
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "トラバース・キャマ_キックアンドテイル（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43132"])]
    public void キックアンドテイル1(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・キャマ_キックアンドテイル1{@event.SourceId()}";
        dp.Scale = new (3f, 8f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "トラバース・キャマ_キックアンドテイル（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43132"])]
    public void キックアンドテイル2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・キャマ_キックアンドテイル2{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9.4f); // 6m + 目标圈 3.4m
        dp.Radian = 120f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.Delay = 2700;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    /*
    [ScriptMethod(name: "インヴォークド・カイム_大凶眼（背对）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44782"])]
    public void 大凶眼(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo("背对", duration: 2000, true);
        // if (isTTS)accessory.Method.TTS("背对");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("背对");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"インヴォークド・カイム_大凶眼{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId;
        dp.TargetObject = accessory.Data.Me;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Scale = new(1);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    */
    
    [ScriptMethod(name: "インヴォークド・トルバドゥール_ダーラ（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44787"])]
    public void ダーラ(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"インヴォークド・トルバドゥール_ダーラ{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f + IbcHelper.GetHitboxRadius(obj)); // 20m + 目标圈
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "86-89 召引巴尔_火山泥流爆（脱战大钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43133"])]
    public void 召引巴尔_火山泥流爆 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引巴尔_火山泥流爆{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(46f);
        dp.DestoryAt = 15700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"召引巴尔_火山泥流爆描边{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(46f);
        dp1.InnerScale = new Vector2(45.96f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 15700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "インヴォークド・バエル_アビサルレイ（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43134"])]
    public void アビサルレイ(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"インヴォークド・バエル_アビサルレイ{@event.SourceId()}";
        dp.Scale = new (20f, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "86-89 召引刻耳柏洛斯_摆尾（背后扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44786"])]
    public void 召引刻耳柏洛斯_摆尾(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引刻耳柏洛斯_摆尾{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(19f);
        dp.Radian = 90f.DegToRad(); 
        dp.Rotation = 180f;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    // 90 BOSS
    
    [ScriptMethod(name: "90 马纳果达_旋背击（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4425[01]$"])]
    public void 马纳果达_旋背击(Event @event, ScriptAccessory accessory)
    {
        // 左下安全: 44250 ; 右下安全: 44251
        var isR = @event.ActionId == 44250;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"马纳果达_旋背击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 270f.DegToRad();
        dp.Rotation = isR ? 315f.DegToRad() : 45f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "90 马纳果达_纵断击/横断击（前后/左右扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44262"])]
    public void 马纳果达_魔冲击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"马纳果达_魔冲击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(35f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 7300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    /* 90 马纳果达_纵断击/横断击（前后/左右扇形） - 遗产
    [ScriptMethod(name: "90 马纳果达_纵断击/横断击（前后/左右扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4425[89]$"])]
    public void 马纳果达_纵断横断击(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
    
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"马纳果达_纵断横断击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f + IbcHelper.GetHitboxRadius(obj));
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = @event.DurationMilliseconds();
    
        // 44258 前后(0°,180°)，44259 左右(90°,270°)
        float[] rotations = @event.ActionId == 44258 
            ? new[] { 0f, 180f } 
            : new[] { 90f, 270f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }
    */
    
    [ScriptMethod(name: "90 指向魔法阵_魔阵光（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43796|44257)$"])]
    public void 指向魔法阵_魔阵光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"指向魔法阵_魔阵光";
        dp.Scale = new (10f, 50f);
        dp.Rotation = 180f.DegToRad(); // 实体为朝场外发射, 需要旋转回场内
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.ActionId == 43796 ? 5300 : 7300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "90 马纳果达_陨星（地面黄圈判定时间）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44268"])]
    public void 马纳果达_陨星 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"马纳果达_陨星";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "90 马纳果达_灭尽 击退安全区", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44266"])]
    public void 马纳果达_灭尽安全区(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"马纳果达_灭尽安全区";
        dp.Scale = new (40f, 10f); // 场地40m (10m x 4格), 击退距离30m, BOSS面前一行即为击退安全区
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.8f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "90 马纳果达_灭尽 击退预测", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44266"])]
    public void 马纳果达_灭尽击退预测(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("靠近击退", duration: 4300, true);
        if (isTTS)accessory.Method.TTS("靠近击退");
        if (isEdgeTTS)accessory.Method.EdgeTTS("靠近击退");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "马纳果达_灭尽击退预测";
        dp.Scale = new(1.5f, 30);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.Rotation = @event.SourceRotation();
        dp.FixRotation = true;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    
    #endregion
    
    #region 91~98层 
    [ScriptMethod(name: "—————— 91 ~ 100 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第91层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "91~94 召引玩偶_呀一一（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44512"])]
    public void 召引玩偶_呀一一 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引玩偶_呀一一{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "91~98 召引梦祸_梦祸视线（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44349"])]
    public void 召引梦祸_梦祸视线(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("穿墙直线");
        if (isEdgeTTS)accessory.Method.EdgeTTS("穿墙直线");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引梦祸_梦祸视线{@event.SourceId()}";
        dp.Scale = new (5f, 41f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "91~98 召引梦祸_死重爆（自爆钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44350"])]
    public void 召引梦祸_死重爆 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引梦祸_死重爆{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(18f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"召引梦祸_死重爆描边{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(18f);
        dp1.InnerScale = new Vector2(17.96f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "91~93 交错路紫罗兰三尖树_藤枝伏地（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44499"])]
    public void 交错路紫罗兰三尖树_藤枝伏地(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路紫罗兰三尖树_藤枝伏地{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "91~93 交错路食人花_腐烂恶臭（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44500"])]
    public void 交错路食人花_腐烂恶臭(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("穿墙直线");
        if (isEdgeTTS)accessory.Method.EdgeTTS("穿墙直线");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路食人花_腐烂恶臭{@event.SourceId()}";
        dp.Scale = new (12f, 45f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
        
    [ScriptMethod(name: "93~95 交错路座狼_三/四重猛击 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^443(39|40)$"])]
    public void 交错路座狼_三四重猛击(Event @event, ScriptAccessory accessory)
    {
        string skullDasherCount = @event.ActionId == 44339 ? "三" : "四";
    
        if (isText) accessory.Method.TextInfo($"{skullDasherCount}连平A + 钢铁", duration: 4000, true);
        if (isTTS) accessory.Method.TTS($"{skullDasherCount}连平A接钢铁");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"{skullDasherCount}连平A接钢铁");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路座狼_重挥碎击预测{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.InnerScale = new Vector2(5.98f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "93~95 交错路座狼_重挥碎击（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44342"])]
    public void 交错路座狼_重挥碎击 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路座狼_重挥碎击{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 1200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "93~95 召引扎哈克_石化吐息（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44508"])]
    public void 召引扎哈克_石化吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引扎哈克_石化吐息{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(28f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    /*
    [ScriptMethod(name: "93~95 召引扎哈克_尾部打击（瞬发扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 召引扎哈克_尾部打击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引扎哈克_尾部打击{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.5f); // 7m + 目标圈 3.5m
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    */

    
    [ScriptMethod(name: "93~98 召引锯齿花_前方/左侧/右侧花蜜喷吐", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4435[6-8]$"])]
    public void 召引锯齿花_花蜜喷吐(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引锯齿花_花蜜喷吐{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.DestoryAt = 6700;
    
        switch (@event.ActionId())
        {
            case 44356: // 前方扇形
                dp.Name = $"召引锯齿花_前方花蜜喷吐{@event.SourceId()}";
                dp.Radian = 120f.DegToRad();
                dp.Rotation = 0f;
                break;
            case 44357: // 左下半场
                dp.Name = $"召引锯齿花_左侧花蜜喷吐{@event.SourceId()}";
                dp.Radian = 180f.DegToRad();
                dp.Rotation = 135f.DegToRad();
                break;
            case 44358: // 右下半场
                dp.Name = $"召引锯齿花_右侧花蜜喷吐{@event.SourceId()}";
                dp.Radian = 180f.DegToRad();
                dp.Rotation = 225f.DegToRad();
                break;
        }
    
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "95~97 交错路恶意灵_碎震强击（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44513"])]
    public void 交错路恶意灵_碎震强击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路恶意灵_碎震强击{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "95~98 召引古恶魔_黑暗神圣（点名圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44514"])]
    public void 召引古恶魔_黑暗神圣 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引古恶魔_黑暗神圣{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "95_98 召引古恶魔_生命停止（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44515"])]
    public void 召引古恶魔_生命停止(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引古恶魔_生命停止{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(35f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "96~98 召引阿刻戎_权威斩（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^445(09|10)$"])]
    public void 召引阿刻戎_权威斩(Event @event, ScriptAccessory accessory)
    {
        // 右刀 44509 ； 左刀 44510
        var isR = @event.ActionId == 44509;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引阿刻戎_权威斩{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "96~98 召引古辛_左/右侧震击（二连左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4434[57]$"])]
    public void 召引古辛_左右侧震击(Event @event, ScriptAccessory accessory)
    {
        // 先右刀 44345 ； 先左刀 44347
        // 该小怪原形为 雪山奥窟冥魂石洞 的 BOSS1_白雪兽 其技能名及 (XAxis, EffectRange) 也一致，冥魂石洞中的场地为矩形，更方便看实际范围
        var isR = @event.ActionId == 44345;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引古辛_左右侧震击{@event.SourceId()}";
        dp.Scale = new (40f, 80f);
        dp.Offset = new Vector3(isR ? 20f : -20f, 0f, 0f);
        // dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"召引古辛_右左侧震击{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new (40f, 80f);
        dp1.Offset = new Vector3(isR ? -20f : 20f, 0f, 0f);
        // dp1.Rotation = isR ? 90f.DegToRad() : 270f.DegToRad();
        dp1.Delay = 5000;
        dp1.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1); 
    }
    
    [ScriptMethod(name: "96~98 召引破坏本能_怪光线（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44354"])]
    public void 召引破坏本能_怪光线(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("穿墙直线");
        if (isEdgeTTS)accessory.Method.EdgeTTS("穿墙直线");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引破坏本能_怪光线{@event.SourceId()}";
        dp.Scale = new (8f, 60f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "96~98 召引破坏本能_死亡视线（背对连线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44355"])]
    public void 召引破坏本能_死亡视线连线(Event @event, ScriptAccessory accessory)
    { 
        if (isText)accessory.Method.TextInfo("背对 <召引破坏本能>", duration: 2300, true);
        if (isTTS)accessory.Method.TTS("背对大眼");
        if (isEdgeTTS)accessory.Method.EdgeTTS("背对大眼");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引破坏本能_死亡视线连线{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId;
        dp.TargetObject = accessory.Data.Me;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Scale = new(1);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "96~98 召引破坏本能_死亡视线（背对范围描边）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44355"])]
    public void 召引破坏本能_死亡视线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "召引破坏本能_死亡视线";
        dp.Color = new Vector4(1f, 0f, 1f, 10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(59.95f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    #endregion

    #region 99层 BOSS 卓异的悲寂
    
    // 卓异的悲寂      NPCID: 14037 目标圈 28.5m
    // 被侵蚀的食罪灵  NPCID: 14038 目标圈 15.0m
    // 深渊烈焰（步进地火）Q0  先上下 ActionId: 44075 / 后左右 ActionId: 44076 / 召唤晶体: 44078 / 晶体爆炸: 44079
    // 深渊烈焰（步进地火）Y99 先左右 ActionId: 44074 / 后上下 ActionId: 44077 / 召唤晶体: 44078 / 晶体爆炸: 44079
    // 以太吸取 Q0&Y99  暗 6.7s ActionId: 44088 / 光 10.7s ActionId: 44092 [无范围] / 暗 11.7s ActionId: 44093 (但是暗比光快)
    
    
    [ScriptMethod(name: "99 烈焰锢（热病）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4406[39]$"])]
    public void 烈焰锢(Event @event, ScriptAccessory accessory)
    {
        // 本体无意义读条 快: 44063 源: 44064  / 慢: 44069 源:44070 , 其中 源 比 无意义 读条更多 0.7s
        // 烈焰锢 (热病) StatusID: 4562 , 赋予时间约 2.6s
        
        var chara = accessory.Data.Objects.OfType<IBattleChara>().FirstOrDefault();
        var nameId = chara?.NameId;
        if (chara == null) return;
        
        int duration = @event.ActionId == 44069 ? 10900 : 7900;
    
        if (isText) accessory.Method.TextInfo("停止移动", duration: duration, true);
        if (isTTS) accessory.Method.TTS("停止移动");
        if (isEdgeTTS) accessory.Method.EdgeTTS("停止移动");
    }
    
    [ScriptMethod(name: "99 火球（旋风）预备提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4406[18]$"])]
    public void 火球预备(Event @event, ScriptAccessory accessory)
    {
        int duration = @event.ActionId == 44061 ? 5000 : 8000;
    
        if (isText) accessory.Method.TextInfo("旋风预备", duration: duration, true);
        if (isTTS) accessory.Method.TTS("旋风预备");
        if (isEdgeTTS) accessory.Method.EdgeTTS("旋风预备");
    }
    
    [ScriptMethod(name: "99 火球（旋风）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44062"])]
    public void 火球 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"火球";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 1800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "99 卓异的悲寂_棘刺尾（穿刺点名直线预兆）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:00EA"])]
    public void 棘刺尾预兆(Event @event, ScriptAccessory accessory)
    {
        if (HelperExtensions.GetCurrentTerritoryId() != MapIds.TheFinalVerse) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        var boss = accessory.Data.Objects.GetByDataId(18666).FirstOrDefault();  // 歼灭战 - 卓异的悲寂 DataId: 18666
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;

        dp.Name = $"棘刺尾预兆";
        dp.Scale = new (4f, 60f);
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.6f);
        dp.FixRotation = true;
        dp.DestoryAt = 6400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "99 卓异的悲寂_棘刺尾（穿刺点名直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45118"])]
    public void 棘刺尾(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("快躲开", duration: 800, true);
        if (isTTS)accessory.Method.TTS("快躲开");
        if (isEdgeTTS)accessory.Method.EdgeTTS("快躲开");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"棘刺尾";
        dp.Scale = new (4f, 60f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 1500;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "99 光耀之剑（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^440(67|73)$"])]
    public void 光耀之剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"光耀之剑";
        dp.Scale = new (15f, 30f);
        dp.Owner = @event.SourceId();
        dp.Offset = new Vector3 (0, 0 ,15); // 原本实体在直线中间，应用 Straight，但考虑到omen效果还是选用 Rect偏移
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "99 净罪之环 TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44082"])]
    public void 强化寒冰咆哮(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("抓人牢狱", duration: 3000, true);
        if (isTTS)accessory.Method.TTS("抓人牢狱");
        if (isEdgeTTS)accessory.Method.EdgeTTS("抓人牢狱");
    }
    
    [ScriptMethod(name: "99 净罪之环（抓人牢狱 - 判定动画）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44083"])]
    public void 净罪之环 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"净罪之环";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    
    #endregion
    
    #region 卓异的悲寂深想战 满贡品难度Q40
    
    [ScriptMethod(name: "—————— 卓异的悲寂深想战 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 卓异的悲寂深想战(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "火球（旋风）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44098"])]
    public void Q40_火球 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Q40_火球";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 1800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "深渊爆焰（地火）读条提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4479[78]$"])]
    public void Q40_深渊爆焰提示(Event @event, ScriptAccessory accessory)
    {
        // 44797 先东西 ; 44798 先南北 ; 水晶DataId: 2014832
    
        var isFirst = @event.ActionId == 44797 ? "左右" : "上下";
        var isSecond = @event.ActionId == 44797 ? "上下" : "左右";
        
        if (isText) accessory.Method.TextInfo($"地火：  先{isFirst} ， 再{isSecond}", duration: 16700, true);
        if (isTTS) accessory.Method.TTS($"先{isFirst}, 后{isSecond}");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"先{isFirst}, 后{isSecond}");
        accessory.Method.SendChat($"/e [Kodakku] 地火记录: 先{isFirst}，再{isSecond}");

    }
    
    [ScriptMethod(name: "净罪之环（抓人牢狱）读条TTS提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4479[78]$"])]
    public void Q40_净罪之环提示(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"抓人牢狱");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"抓人牢狱");
    }
    
    [ScriptMethod(name: "净罪之环（抓人牢狱 - 判定动画）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44122"])]
    public void Q40_净罪之环 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Q40_净罪之环";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    // TargetIconId: 0061 拉线预兆
    
    uint Spinelash = 0; // 棘刺尾 读条点名直线记录
    
    public void Init(ScriptAccessory accessory) {
        Spinelash = 0;
    }
    
    [ScriptMethod(name: "棘刺尾（点名分摊提示）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:020F"])]
    public void Q40_棘刺尾提示(Event @event, ScriptAccessory accessory)
    {
        if (HelperExtensions.GetCurrentTerritoryId() != MapIds.TheFinalVerseQuantum) return; // 深想战 - 卓异的悲寂 DataId: 18670
        
        Spinelash++;
        
        if (isTTS)accessory.Method.TTS("挡枪分摊");
        if (isEdgeTTS)accessory.Method.EdgeTTS("挡枪分摊");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"棘刺尾预兆";
        dp.Scale = new (1f, 30f);
        dp.Color = accessory.Data.DefaultSafeColor.WithW(1.4f);
        
        switch (Spinelash)
        {
            case 1 :
                dp.Position = new Vector3(-613.4f, 0f, -315f); // 左
                break;
            case 2 :
                dp.Position = new Vector3(-586.6f, 0f, -315f); // 右
                break;
            case 3 :
                dp.Position = new Vector3(-600f, 0f, -315f); // 中
                break;
        }
        
        dp.DestoryAt = 6400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
        
        if (isDeveloper)  accessory.Method.SendChat($"/e [DEBUG] 当前点名次数:{Spinelash}");
    }
    
    [ScriptMethod(name: "棘刺尾（直线分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45119"])]
    public void Q40_棘刺尾(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"棘刺尾";
        dp.Scale = new (8f, 60f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "戒律的光链：恢复 [奶妈治疗热风]", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4564"])]
    public void Q40_戒律的光链_恢复 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Q40_戒律的光链_恢复";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(21f);
        dp.InnerScale = new Vector2(20.95f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 65000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "戒律的光链：能力 [DPS]", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4565"])]
    public void Q40_戒律的光链_能力 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Q40_戒律的光链_能力{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(6f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(8f);
        dp.InnerScale = new Vector2(7.96f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 65000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "戒律的光链备用销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^456[45]$"],userControl: false)]
    public void 戒律的光链备用销毁(Event @event, ScriptAccessory accessory)
    {
        if (@event.StatusId == 4564) accessory.Method.RemoveDraw("Q40_戒律的光链_恢复");
        if (@event.StatusId == 4565) accessory.Method.RemoveDraw($"Q40_戒律的光链_能力{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "尾连击（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44161"])]
    public void Q40_尾连击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"尾连击";
        dp.Scale = new (9f, 42f);
        dp.Offset = new Vector3(0f, 0f, 10f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    #endregion
    
    #region 底裤部分
    
    [ScriptMethod(name: "—————— 底裤部分（需要对应插件与权限） ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 底裤部分(Event @event, ScriptAccessory accessory) { }
    
    // 这次 BOSS房 不可能变身 不用考虑 ， 由于变身全是烛台给的buff, 也不用考虑过层时触发的 Duration

    [ScriptMethod(name: "[DR] 变身泥球时，移速改为1.2倍", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4708", "StackCount:54"])]
    public void AddMudPieSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.2");
        accessory.Method.SendChat($"/e 鸭鸭：[DR] 移速已更改：1.2x");
        if (isTTS)accessory.Method.TTS("移速已更改至1.2倍");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已更改至1.2倍");
    }
    
    
    [ScriptMethod(name: "[DR] 泥球取消时，移速复原至默认值", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4708", "StackCount:54"])]
    public void RemoveMudPieSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed -1");
        accessory.Method.SendChat($"/e 鸭鸭：[DR] 移速已更改：默认");
        if (isTTS)accessory.Method.TTS("移速已复原至默认值");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已复原至默认值");
    }
    

    [ScriptMethod(name: "[DR] 变身爆弹之母时，移速改为1.5倍", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4708", "StackCount:55"])]
    public void AddProgenitrixSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.5");
        accessory.Method.SendChat($"/e 鸭鸭：[DR] 移速已更改：1.5x");
        if (isTTS)accessory.Method.TTS("移速已更改至1.5倍");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已更改至1.5倍");
    }
    
    [ScriptMethod(name: "[DR] 爆弹之母取消时，移速复原至默认值", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4708", "StackCount:55"])]
    public void RemoveProgenitrixSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed -1");
        accessory.Method.SendChat($"/e 鸭鸭：[DR] 移速已更改：默认");
        if (isTTS)accessory.Method.TTS("移速已复原至默认值");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已复原至默认值");
    }
    
    [ScriptMethod(name: "[IC] 变身泥球时，取消遁地", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4708", "StackCount:54"])]
    public void AddMudPieDepths(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/i-ching-commander y_adjust 0");
        accessory.Method.SendChat($"/e 鸭鸭：[IC] 已取消遁地");
        if (isText) accessory.Method.TextInfo("已取消遁地", duration: 1300, true);
        // if (isTTS)accessory.Method.TTS("已取消遁地");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("已取消遁地");
    }
    
    
    [ScriptMethod(name: "[IC] 泥球取消时，自动遁地", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4708", "StackCount:54"])]
    public void RemoveMudPieDepths(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
    
        // 获取深度的描述值
        string depthValue = Depths.GetDescription();
        
        accessory.Method.SendChat($"/i-ching-commander y_adjust -{depthValue}");
        accessory.Method.SendChat($"/e 鸭鸭：[IC] 已自动遁地 -{depthValue}m");
        if (isText) accessory.Method.TextInfo($"已自动遁地 -{depthValue}m", duration: 1300, true);
        // if (isTTS)accessory.Method.TTS("已自动遁地");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("已自动遁地");
    }
    
    [ScriptMethod(name: "[IC] 变身爆弹之母时，取消遁地", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4708", "StackCount:55"])]
    public void AddProgenitrixDepths(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/i-ching-commander y_adjust 0");
        accessory.Method.SendChat($"/e 鸭鸭：[IC] 已取消遁地");
        if (isText) accessory.Method.TextInfo("已取消遁地", duration: 1300, true);
        // if (isTTS)accessory.Method.TTS("已取消遁地");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("已取消遁地");
    }
    
    [ScriptMethod(name: "[IC] 爆弹之母取消时，自动遁地", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4708", "StackCount:55"])]
    public void RemoveProgenitrixDepths(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
    
        // 获取深度的描述值
        string depthValue = Depths.GetDescription();
        
        accessory.Method.SendChat($"/i-ching-commander y_adjust -{depthValue}");
        accessory.Method.SendChat($"/e 鸭鸭：[IC] 已自动遁地 -{depthValue}m");
        if (isText) accessory.Method.TextInfo($"已自动遁地 -{depthValue}m", duration: 1300, true);
        // if (isTTS)accessory.Method.TTS("已自动遁地");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("已自动遁地");
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