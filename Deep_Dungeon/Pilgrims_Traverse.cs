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
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Pilgrims_Traverse;

[ScriptType(guid: "3f65b3c0-df48-4ef8-89ae-b8091b7690f1", name: "朝圣交错路", author: "Tetora", 
    territorys: [1281, 1282, 1283, 1284, 1285, 1286, 1287, 1288, 1289, 1290, 1311, 1333],
    version: "0.0.1.3",note: noteStr)]

public class Pilgrims_Traverse
{
    const string noteStr =
        """
        v0.0.1.3:
        朝圣交错路基础绘制
        更新日志见dc，出现问题请带ARR录像文件反馈
        注：方法设置中的层数仅做分割线效果，并不是批量开关
        """;

    
    #region 各种记录
    
    /*  StatusID
     *  变身 4708 [StackCount:54 Param:566 泥球 ]  [StackCount:55 Param:567 爆弹之母 ]
     *  极速冲刺 4709
     *  禁止使用杜松香 4591
     *  诅咒 1087 （拟态怪 - 怨念）
     *  失明 1088 （妖宫没有）
     *  最大体力减少 1089
     *  伤害降低 1090
     *  能力封印 1092（妖宫没有）
     *  最大体力魔力提升 1093
     *  禁止使用道具  1094
     *  禁止冲刺 1095
     *  击退无效 1096
     *  禁止体力自然恢复 1097
     *  无法发动技能 1113 （形态变化）
     *  冲刺 1548 （天气的普通冲刺）
     *  加速 4718 （妖宫为魔陶器，不是天气）
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
    
    [UserSetting("是否启用遁地特殊绘制")]
    public bool isUnderGround { get; set; } = true;
    
    [UserSetting("遁地能打到的AOE颜色")]
    public ScriptColor UnderGround_AOEs { get; set; } = new() { V4 = new(1f, 0f, 0f, 1.2f) };
    
    [UserSetting("设置吸引类技能颜色")]
    public ScriptColor InhaleColor { get; set; } = new() { V4 = new(0f, 1f, 1f, 0.4f) };
    
    [UserSetting("启用小工具（已确认设置完毕）")]
    public bool isMiniTools { get; set; } = false;
    
    [UserSetting("设置传送装置查找颜色")]
    public ScriptColor teleporter { get; set; } = new() { V4 = new(0f, 1f, 0f, 2f) };
    
    [UserSetting("设置光耀烛台查找颜色")]
    public ScriptColor votiveCandelabra { get; set; } = new() { V4 = new(1f, 0f, 1f, 1.4f) };
    
    [UserSetting(note: "选择自动苏生之炎对象")]
    public RekindleEnum Rekindle { get; set; } = RekindleEnum.TargetsTarget;
    
    [UserSetting("烈焰领域自动防击退（T职除外，底裤防击退需要同时启用底裤选项）")]
    public AutoAntiKnockbackEnum AutoAntiKnockback { get; set; } = AutoAntiKnockbackEnum.None;
    
    private static List<string> _AutoAntiKnockback = ["亲疏自行", "沉稳咏唱"];
    
    public enum AutoAntiKnockbackEnum
    {
        None = -1,
        亲疏自行 = 0,
        沉稳咏唱 = 1,
        DR = 2,
        IChing = 3,
    }
    
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
        public const uint PilgrimsTraverse1 = 1282; // 第 11~20朝圣路
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
    
    public void Init(ScriptAccessory accessory) {
        PerilousLair = 0; // 80 BOSS 伤痛圆戒 钢铁
        RoaringRing = 0; // 80 BOSS 紫雷环戒 月环
        _myLightVengeance=0; // 99 光之回音
        _myDarkVengeance=0; // 99 暗之回音
        _blackandwhite = 0; // 卓异的悲寂深想战 黑白配 点名记录
        _spinelash = 0; // 卓异的悲寂深想战 棘刺尾 直线点名次数记录
        ResetMechanic(); // 卓异的悲寂 地火晶体重置
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
        if(!isMiniTools) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"爆弹之母_大爆炸{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 1500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "传送装置查找", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2014756", "Operate:Add"])]
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
    
    [ScriptMethod(name: "光耀烛台查找", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2014759", "Operate:Add"])]
    public void 光耀烛台查找(Event @event, ScriptAccessory accessory)
    {
        if(!isMiniTools) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"光耀烛台";
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new (1f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Color = votiveCandelabra.V4;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp); 
    }

    [ScriptMethod(name: "光耀烛台销毁", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:16", "Id2:32"], userControl: false)]
    public void 光耀烛台销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"光耀烛台");
    }
    
    /*
    [ScriptMethod(name: "光耀烛台 温暖气息提示", eventType: EventTypeEnum.Chat, eventCondition: ["Type:SystemMessage", "Message:察觉到了灯火般温暖的神秘气息……"])]
    public void 光耀烛台_温暖气息提示(Event @event, ScriptAccessory accessory)
    {
        if(!isMiniTools) return;
        if (isTTS)accessory.Method.TTS("本层有烛台");
        if (isEdgeTTS)accessory.Method.EdgeTTS("本层有烛台");
    }
    */
    
    /*
    [ScriptMethod(name: "队友爆弹之母 大爆炸范围绘制", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44629"])]
    public void 爆弹之母_大爆炸Party (Event @event, ScriptAccessory accessory)
    {
        if(!isMiniTools) return;
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
        
        if(!isMiniTools ||  @event.SourceId() != accessory.Data.Me || @event.TargetId() != accessory.Data.Me) return;
        accessory.Method.SendChat($"/ac 星极超流 {rekindleValue}");
        accessory.Method.SendChat($"/e 鸭鸭：已尝试自动给《{rekindleValue}》挂上不死鸟热水");
    }
    
    #endregion
    
    #region  1~10层 
    [ScriptMethod(name: "—————— 1 ~ 10 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第1层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "1~2 交错路石莲猬_飞叶快刀（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44631"])]
    public void 交错路石莲猬_飞叶快刀(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路石莲猬_飞叶快刀{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 15f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "1~3 交错路草小人_草风（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44641"])]
    public void 交错路草小人_草风(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路草小人_草风{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 15f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "6~7 交错路杀人蜂_未终针（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44637"])]
    public void 交错路杀人蜂_未终针(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路杀人蜂_未终针{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (3f, 9f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "6~9 交错路铁线莲_旋转攻击（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44638"])]
    public void 交错路铁线莲_旋转攻击(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路铁线莲_旋转攻击{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 10f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
        
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
    
    [ScriptMethod(name: "11~12 交错路帕克_拍手（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44657"])]
    public void 交错路帕克_拍手(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路帕克_拍手{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 14f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "11~12 得到宽恕的无知_蛛网屏（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44646"])]
    public void 得到宽恕的无知_蛛网屏(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的无知_蛛网屏{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 16f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "16~17 得到宽恕的违命_冲顶（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44651"])]
    public void 得到宽恕的违命_冲顶(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的违命_冲顶{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (3f, 10f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
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
    
    [ScriptMethod(name: "21~23 得到宽恕的贿赂_以太火花（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44680"])]
    public void 得到宽恕的贿赂_以太火花(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的贿赂_以太火花{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 12f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "24~25 交错路冰海天使_捕食行动 死刑提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44681"])]
    public void 交错路冰海天使_捕食行动(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("眩晕冰海天使死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS("眩晕冰海天使死刑");
    }
    
    [ScriptMethod(name: "26~29 得到宽恕的残忍_流明无限（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44668"])]
    public void 得到宽恕的残忍_流明无限(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的残忍_流明无限{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (5f, 40f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "26~27 得到宽恕的奢望_撕裂利爪（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44669"])]
    public void 得到宽恕的奢望_撕裂利爪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的奢望_撕裂利爪{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "28~29 得到宽恕的狭隘_灾厄之语（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44683"])]
    public void 得到宽恕的狭隘_灾厄之语(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的狭隘_灾厄之语{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (6f, 47f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "28~29 得到宽恕的狭隘_重击（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44684"])]
    public void 得到宽恕的狭隘_重击 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的狭隘_重击{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "28~29 交错路魔像_巨像之光（穿墙直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44675"])]
    public void 交错路魔像_巨像之光(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路魔像_巨像之光{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (6f, 60f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "30 得到宽恕的背信_光耀颂词 光轮（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^39(642|643|674|743)$"])]
    public void 得到宽恕的背信_光耀颂词_光轮 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的背信_光耀颂词_光轮";
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
            case 39674:
                dp.Scale = new Vector2(24f);
                dp.InnerScale = new Vector2(19f);
                break;
            case 39743:
                dp.Scale = new Vector2(29f);
                dp.InnerScale = new Vector2(24f);
                break;
        }
        
        dp.Radian = float.Pi * 2;
        dp.Delay = 7000;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "30 得到宽恕的背信_救赎圣拳（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40401|40549)$"])]
    public void 得到宽恕的背信_救赎圣拳(Event @event, ScriptAccessory accessory)
    {
        // 右刀 无意义 40401 伤害源 40551 接左刀 救赎圣臂 40552 ； 左刀 40549  接右刀 救赎圣臂 40553
        var isR = @event.ActionId == 40401;
        
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的背信_救赎圣拳";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f + IbcHelper.GetHitboxRadius(obj)); // 30m + 目标圈
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    /* 好像实体从一开始就存在，暂时不画，需要筛选可见性
    [ScriptMethod(name: "30 恕罪圣环（旋转圆形）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17930"])]
    public void 得到宽恕的背信_恕罪圣环 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的背信_恕罪圣环";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(4f);
        dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "恕罪圣环销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:17930"],userControl: false)]
    public void 恕罪圣环销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("得到宽恕的背信_恕罪圣环");
    }
    */
    
    #endregion
    
    #region  31~40层
    [ScriptMethod(name: "—————— 31 ~ 40 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第31层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "31~33 交错路天马_天马嘶啸（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44703"])]
    public void 交错路天马_天马嘶啸 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路天马_天马嘶啸{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "32~35 交错路飞翼兽_高山气流（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44700"])]
    public void 交错路飞翼兽_高山气流(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路飞翼兽_高山气流{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (5f, 45f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "36~38 交错路判官_葬送击（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44694"])]
    public void 交错路判官_葬送击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路判官_葬送击{@event.SourceId()}";
        dp.Scale = new (2f, 21f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "36~39 得到宽恕的疫病_双重毒粉（二段扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41344"])]
    public void 得到宽恕的疫病_双重毒粉(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的疫病_双重毒粉{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 120f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.Delay = 2700;
        dp.DestoryAt = 5600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "36~39 交错路石兵_冲波炮（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40891"])]
    public void 交错路石兵_冲波炮(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路石兵_冲波炮{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (8f, 60f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "36~39 交错路石兵_爆发拳（二段钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40558"])]
    public void 交错路石兵_爆发拳 (Event @event, ScriptAccessory accessory)
    {
        // 在 重拳波 [ActionId:40558 / 3.7s] 4s后 显示第2段钢铁  两次判定间隔约2s
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路石兵_爆发拳{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(15f);
        dp.Delay = 4000;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "37~39 得到宽恕的暴躁_左/右触手（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4469[01]$"])]
    public void 得到宽恕的暴躁_左右触手(Event @event, ScriptAccessory accessory)
    {
        // 右刀 44691 ； 左刀 44690
        var isR = @event.ActionId == 44691;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的暴躁_左右触手{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "40 得到宽恕的天真_祝圣吹息 播报", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4212[34]$"])]
    public void 得到宽恕的天真_祝圣吹息 (Event @event, ScriptAccessory accessory)
    {
        if (@event.ActionId == 42123)
        {
            if (isText)accessory.Method.TextInfo("三连击退", duration: 2000, true);
            if (isTTS)accessory.Method.TTS("三连击退");
            if (isEdgeTTS)accessory.Method.EdgeTTS("三连击退");
        }
        else
        {
            if (isText)accessory.Method.TextInfo("三连钢铁", duration: 2000, true);
            if (isTTS)accessory.Method.TTS("三连钢铁");
            if (isEdgeTTS)accessory.Method.EdgeTTS("三连钢铁");
        }
    }
    
    [ScriptMethod(name: "40 得到宽恕的天真_光水弹（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42129"])]
    public void 得到宽恕的天真_光水弹 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的天真_光水弹";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.Delay = @event.SourceDataId() == 9020 ? 0 : 4800;
        dp.DestoryAt = @event.SourceDataId() == 9020 ? 9700 : 4900;
        dp.ScaleMode = @event.SourceDataId() == 9020 ? ScaleMode.ByTime : ScaleMode.None;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "40 得到宽恕的天真_震水弹（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42128"])]
    public void 得到宽恕的天真_震水弹击退(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "得到宽恕的天真_震水弹击退";
        dp.Scale = new Vector2(1f, 21f);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = @event.TargetPosition();
        dp.Rotation = float.Pi;
        dp.Delay = @event.SourceDataId() == 9020 ? 0 : 7500;
        dp.DestoryAt = @event.SourceDataId() == 9020 ? 9700 : 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "40 得到宽恕的天真_震水弹（第一次击退位置）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42128", "SourceDataId:18467"], suppress:9700)]
    public void 得到宽恕的天真_震水弹击退位置(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "得到宽恕的天真_震水弹击退位置";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.InnerScale = new Vector2(4.92f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 9700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "40 得到宽恕的天真_圆浪（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45169"])]
    public void 得到宽恕的天真_圆浪 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的天真_圆浪";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "40 得到宽恕的天真_环浪（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45170"])]
    public void 得到宽恕的天真_环浪 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的天真_环浪";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.InnerScale = new Vector2(8f);
        dp1.Radian = dp.Radian = float.Pi * 2;
        dp1.DestoryAt = dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        dp1.Name = $"得到宽恕的天真_环浪安全区";
        dp1.Color = accessory.Data.DefaultSafeColor.WithW(10f);
        dp1.Scale = new Vector2(7.9f);
        dp1.InnerScale = new Vector2(7.75f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    #endregion
    
    #region  41~50层
    [ScriptMethod(name: "—————— 41 ~ 50 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第41层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "42~45 交错路兵装_愤怒一击（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44714"])]
    public void 交错路兵装_愤怒一击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路兵装_愤怒一击{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 6f);
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "42~45 交错路兵装_愤怒旋风（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44715"])]
    public void 交错路兵装_愤怒旋风 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路兵装_愤怒旋风{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "43~46 交错路托尔巴龟_龟足踏（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41724"])]
    public void 交错路托尔巴龟_龟足踏 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路托尔巴龟_龟足踏{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 11700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "46~49 交错路塑像_吸引震动（吸引）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41427"])]
    public void 交错路塑像_吸引震动 (Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("吸引");
        if (isEdgeTTS)accessory.Method.EdgeTTS("吸引");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路塑像_吸引震动{@event.SourceId()}";
        dp.Color = InhaleColor.V4;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"交错路塑像_吸引震动{@event.SourceId()}";
        dp1.Color = InhaleColor.V4.WithW(5f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(15f);
        dp1.InnerScale = new Vector2(14.94f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "46~49 交错路塑像_平原震裂（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41512"])]
    public void 交错路塑像_平原震裂 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路塑像_平原震裂{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "46~49 交错路三尖树_双重藤枝伏地（扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41854"])]
    public void 交错路三尖树_双重藤枝伏地(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路三尖树_双重藤枝伏地{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 90f.DegToRad(); 
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 6000; // 读条2.7s, 约3s后转向再打一次正面
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "47~49 交错路蚁狮_二连行军（往返直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4470[89]$"])]
    public void 交错路蚁狮_二连行军(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路蚁狮_二连行军{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (8f, 15f);
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = @event.ActionId() == 44708 ? 2700: 700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        if ( @event.ActionId() == 44708)
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"交错路蚁狮_二连行军折返预测{@event.SourceId()}";
            dp1.Owner = @event.SourceId();
            dp1.Scale = new (8f, 15f);
            if (isUnderGround) {dp1.Color = UnderGround_AOEs.V4; }
            else {dp1.Color = accessory.Data.DefaultDangerColor; }
            dp1.Rotation = 180f.DegToRad();
            dp1.Delay = 2700;
            dp1.DestoryAt = 2500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1); 
        }
    }
    
    // 50 BOSS 奥格布那巴利
    
    [ScriptMethod(name: "50 奥格布那巴利_地面液化 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43531"])]
    public void 奥格布那巴利_地面液化(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("站在石头上", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("站在石头上");
        if (isEdgeTTS)accessory.Method.EdgeTTS("站在石头上");
    }
    
    [ScriptMethod(name: "50 奥格布那巴利_进沙坑 点名提示", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0280"])]
    public void 奥格布那巴利_进沙坑点名(Event @event, ScriptAccessory accessory)
    {
        // 进沙坑 ActionId: 43533
        if (HelperExtensions.GetCurrentTerritoryId() != MapIds.PilgrimsTraverse4) return;
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText)accessory.Method.TextInfo("追踪AOE点名 (避开队友)", duration: 13800, true);
            if (isTTS)accessory.Method.TTS("追踪AOE点名");
            if (isEdgeTTS)accessory.Method.EdgeTTS("追踪AOE点名");
        }
        else
        {
            if (isText)accessory.Method.TextInfo("躲避追踪AOE (跟着跑,别挡路)", duration: 13800, true);
            if (isTTS)accessory.Method.TTS("躲避追踪AOE");
            if (isEdgeTTS)accessory.Method.EdgeTTS("躲避追踪AOE");
        }
    }
    
    [ScriptMethod(name: "50 奥格布那巴利_破坑而出（首次追踪）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43534"])]
    public void 奥格布那巴利_破坑而出 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"奥格布那巴利_破坑而出";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "50 奥格布那巴利_冽风 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43537"])]
    public void 奥格布那巴利_冽风(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("稍后进流沙躲避击退\n(防击退无效)", duration: 3000, true);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冽风";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = new Vector3(-300f, 0f, -300f);
        dp.Scale = new Vector2(20f);
        dp.InnerScale = new Vector2(15f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "50 奥格布那巴利_烈风 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43538"])]
    public async void 奥格布那巴利_烈风(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("击退倒计时", duration: 7000, false);
        await Task.Delay(4700);
        if (isTTS)accessory.Method.TTS("进入流沙");
        if (isEdgeTTS)accessory.Method.EdgeTTS("进入流沙");
    }
    
    #endregion
    
    #region  51~59层
    [ScriptMethod(name: "—————— 51 ~ 60 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第51层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "53~56 交错路巨蜥_火鳞甲 读条提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42212"])]
    public void 交错路巨蜥_火鳞甲(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("巨蜥反伤");
        if (isEdgeTTS)accessory.Method.EdgeTTS("巨蜥反伤");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路巨蜥_火鳞甲{@event.SourceId()}";
        dp.Color = UnderGround_AOEs.V4.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3.2f);
        dp.InnerScale = new Vector2(3f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "53~56 交错路巨蜥_火棘屏障 反伤提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4579"])]
    public void 交错路巨蜥_火棘屏障(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路巨蜥_火棘屏障{@event.SourceId()}";
        dp.Color = UnderGround_AOEs.V4.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3.2f);
        dp.InnerScale = new Vector2(3f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "交错路巨蜥_火棘屏障销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4579"],userControl: false)]
    public void 交错路巨蜥_火棘屏障销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"交错路巨蜥_火棘屏障{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "56~59 交错路沙地巨蟒_大地钻击（270°顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42091"])]
    public void 交错路沙地巨蟒_大地钻击 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路沙地巨蟒_大地钻击{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 270f.DegToRad(); 
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "56~59 交错路巨人_蛮力金刚臂（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44737"])]
    public void 交错路巨人_蛮力金刚臂 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路巨人_蛮力金刚臂{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "57~59 交错路怒嚎_高速撞击（直线冲锋）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44730"])]
    public void 交错路怒嚎_高速撞击(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路怒嚎_高速撞击{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (7f, 12f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "57~59 交错路怒嚎_尾镰（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44731"])]
    public void 交错路怒嚎_尾镰 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路怒嚎_尾镰{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "57~59 交错路怒嚎_外环雷（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44732"])]
    public void 交错路怒嚎_外环雷(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路怒嚎_外环雷{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "57~59 交错路美甲兽_飞散性惑乱花粉块（精神失常）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42144"])]
    public void 交错路美甲兽_飞散性惑乱花粉块 (Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("打断美甲兽目押 （无任何抗性）", duration: 4300, true);
        if (isTTS)accessory.Method.TTS("打断或眩晕美甲兽");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断或眩晕美甲兽");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路美甲兽_飞散性惑乱花粉块{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"交错路美甲兽_飞散性惑乱花粉块描边{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(8f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(40f);
        dp1.InnerScale = new Vector2(39.94f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "57~59 交错路美甲兽_呵斥（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42147"])]
    public void 交错路美甲兽_呵斥(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路美甲兽_呵斥{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "57~59 交错路美甲兽_迅猛回旋（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42148"])]
    public void 交错路美甲兽_迅猛回旋 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路美甲兽_迅猛回旋{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14f);
        dp.DestoryAt = 4700;
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
    
    [ScriptMethod(name: "61~64 得到宽恕的疑念 潜伏透视", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:18832"])]
    public void 得到宽恕的疑念_潜伏 (Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的疑念_潜伏{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(IbcHelper.GetHitboxRadius(obj)); // 目标圈为2.4m，只有在+5m内才可见，开怪后状态消失
        dp.DestoryAt = 600000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "61~64 得到宽恕的疑念 潜伏面向", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:18832"])]
    public void 得到宽恕的疑念_潜伏面向 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的疑念_潜伏面向{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f); // 参考视觉引战范围
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 600000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "得到宽恕的疑念 潜伏销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:676"],userControl: false)]
    public void 得到宽恕的疑念_潜伏销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"得到宽恕的疑念_潜伏.*{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "得到宽恕的疑念 潜伏备用销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44749|45130)"],userControl: false)]
    public void 得到宽恕的疑念_潜伏备用销毁(Event @event, ScriptAccessory accessory)
    {
        // 44749 滑跃 ; 45130 平A攻击
        accessory.Method.RemoveDraw($"得到宽恕的疑念_潜伏.*{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "61~69 交错路塔罗斯_执行贯穿（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42516"])]
    public void 交错路塔罗斯_执行贯穿 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路塔罗斯_执行贯穿{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "61~69 交错路塔罗斯_地层俯冲（二段月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42516"])]
    public void 交错路塔罗斯_地层俯冲 (Event @event, ScriptAccessory accessory)
    {
        // 在 执行贯穿 [ActionId:42516 / 3.7s] 4s后 显示第2段月环  两次判定间隔约3.1s
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路塔罗斯_地层俯冲{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(11f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 3700;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "61~64 得到宽恕的疑念_躯体重压（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44748"])]
    public void 得到宽恕的疑念_躯体重压 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的疑念_躯体重压{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "61~63 交错路诺姆_平地捶打（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44753"])]
    public void 交错路诺姆_平地捶打 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路诺姆_平地捶打{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "61~63 交错路壁崖鼹鼠_冲顶（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44740"])]
    public void 交错路壁崖鼹鼠_冲顶(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路壁崖鼹鼠_冲顶{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "62~65 得到宽恕的暴动_左/右侧震荡波（二连左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4221[46]$"])]
    public void 得到宽恕的暴动_左右侧震荡波(Event @event, ScriptAccessory accessory)
    {
        // 先右刀 42214 ； 先左刀 42216
        var isR = @event.ActionId == 42214;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的暴动_左右侧震荡波1{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"得到宽恕的暴动_左右侧震荡波2{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(30f);
        dp1.Radian = 180f.DegToRad(); 
        dp1.Rotation = isR ? 90f.DegToRad() : 270f.DegToRad();
        dp1.Delay = 4700;
        dp1.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }
    
    [ScriptMethod(name: "63~66 得到宽恕的怨恨_冰雹发射（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42515"])]
    public void 得到宽恕的怨恨_冰雹发射(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的怨恨_冰雹发射{@event.SourceId()}";
        dp.Scale = new (4f, 45f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "64~66 得到宽恕的不公_岩石崩溃（十字）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44743"])]
    public void 得到宽恕的不公_岩石崩溃(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的不公_岩石崩溃{@event.SourceId()}";
        dp.Scale = new (10f, 80f); // 范围待确定，可能是10,40
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3700;
        
        float[] rotations = { 0f, 90f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    
    [ScriptMethod(name: "65~67 得到宽恕的依恋_臭水（前后刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4475[01]$"])]
    public void 得到宽恕的依恋_臭水(Event @event, ScriptAccessory accessory)
    {
        // 前刀 44750 ； 后刀 44751
        var isF = @event.ActionId == 44750;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的依恋_臭水";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isF ? 0f.DegToRad() : 180f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "65~68 交错路蜂后_未终针（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42510"])]
    public void 交错路蜂后_未终针(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路蜂后_未终针{@event.SourceId()}";
        dp.Scale = new (3f, 8f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "66~69 得到宽恕的主见_几千针刺（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42220"])]
    public void 得到宽恕的主见_几千针刺(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的主见_几千针刺{@event.SourceId()}";
        dp.Scale = new (8f, 20f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "67~69 交错路恩戈齐_泥石流（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44755"])]
    public void 交错路恩戈齐_泥石流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路恩戈齐_泥石流{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "67~69 得到宽恕的贪食_石化凝视（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44746"])]
    public void 得到宽恕的贪食_石化凝视 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的贪食_石化凝视{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "67~69 得到宽恕的贪食_猛撞（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44747"])]
    public void 得到宽恕的贪食_猛撞 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的贪食_猛撞{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    // 70 BOSS 得到宽恕的热忱
    
    [ScriptMethod(name: "70 得到宽恕的热忱_热忱怒视（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^434(06|11)$"])]
    public void 得到宽恕的热忱_热忱怒视(Event @event, ScriptAccessory accessory)
    {
        // 两个 ActionId对应 从近到远 (43406) / 从远到近 (43411) 为本体无意义读条，之后连续4个id为接下来的分段伤害
        var zealousGlower = @event.ActionId == 43411 ? "从远到近" : "从近到远";
        if (isText)accessory.Method.TextInfo($"光球月环: {zealousGlower}", duration: 8600, true);
        if (isTTS)accessory.Method.TTS($"月环{zealousGlower}");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"月环{zealousGlower}");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的热忱_热忱怒视";
        dp.Scale = new (10f, 25f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "70 得到宽恕的热忱_月环安全区预测", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^434(07|12|18|23)$"])]
    public void 得到宽恕的热忱_月环安全区预测 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        
        dp1.InnerScale = dp.InnerScale = new Vector2(2.92f);
        dp1.Scale = dp.Scale = new Vector2(3f);
        dp1.Radian = dp.Radian = float.Pi * 2;
        dp.Color = accessory.Data.DefaultSafeColor.WithW(10f);
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        
        switch (@event.ActionId())
        {
            case 43407: // 热忱怒视 从近到远
                dp1.Name = dp.Name = $"得到宽恕的热忱_热忱怒视_从近到远预边";
                dp1.Position = dp.Position = @event.EffectPosition();
                dp1.DestoryAt = dp.Delay = 4700;
                dp.DestoryAt = 6000;
                break;
            case 43412: // 热忱怒视 从远到近
                dp1.Name = dp.Name = $"得到宽恕的热忱_热忱怒视_从远到近预测";
                dp1.Position = dp.Position = @event.EffectPosition();
                dp1.DestoryAt = dp.Delay = 4700;
                dp.DestoryAt = 6000;
                break;
            case 43418: // 热忱之眼 从左顺时针
                dp1.Owner = dp.Owner = @event.SourceId();
                dp1.Offset = dp.Offset = new Vector3(-7.5f, 0f, 0f);
                dp1.Name = dp.Name = $"得到宽恕的热忱_热忱之眼_从左顺时针预测";
                dp1.DestoryAt = dp.Delay = 8400;
                dp.DestoryAt = 1600;
                break;
            case 43423: // 热忱之眼 从右逆时针
                dp1.Owner = dp.Owner = @event.SourceId();
                dp1.Offset = dp.Offset = new Vector3(7.5f, 0f, 0f);
                dp1.Name = dp.Name = $"得到宽恕的热忱_热忱之眼_从右逆时针预测";
                dp1.DestoryAt = dp.Delay = 8400;
                dp.DestoryAt = 1600;
                break;            
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);

    }
    
    /*
    [ScriptMethod(name: "70 得到宽恕的热忱_热忱怒视（分段直线绘制）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^434(0[789]|10)$"])]
    public void 得到宽恕的热忱_热忱怒视分段(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的热忱_热忱怒视分段";
        dp.Scale = new (10f, 10f);
        dp.Position = @event.EffectPosition();
        dp.Offset = new Vector3(0f, 0f, 5f);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
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
        dp.DestoryAt = 1700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"得到宽恕的热忱_光球 光轮描边";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(20f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(3f);
        dp1.InnerScale = new Vector2(2.94f);
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
    
    [ScriptMethod(name: "70 得到宽恕的热忱_热忱之眼（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^434(18|23)$"])]
    public void 得到宽恕的热忱_热忱之眼 (Event @event, ScriptAccessory accessory)
    {
        // 两个技能id对应 从左顺时针 (43418) / 从右逆时针 (43423)
        var ardorousEye = @event.ActionId == 43418 ? "从BOSS左侧开始，顺时针" : "从BOSS右侧开始，逆时针";
        if (isText)accessory.Method.TextInfo($"光球月环: {ardorousEye}", duration: 10000, true);
        if (isTTS)accessory.Method.TTS($"月环{ardorousEye}");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"月环{ardorousEye}");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的热忱_热忱之眼";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 5000;
        dp.DestoryAt = 3400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "70 得到宽恕的热忱_二千迈纳回转（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43429"])]
    public void 得到宽恕的热忱_二千迈纳回转 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的热忱_二千迈纳回转";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "70 得到宽恕的热忱_吼叫（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43431"])]
    public void 得到宽恕的热忱_吼叫(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("中间击退（防击退有效）", duration: 6000, true);
        if (isTTS)accessory.Method.TTS("击退");
        if (isEdgeTTS)accessory.Method.EdgeTTS("击退");
    }
    
    [ScriptMethod(name: "70 得到宽恕的热忱_八重横扫（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43432"])]
    public void 得到宽恕的热忱_八重横扫(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的热忱_八重横扫";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 24600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    // 八重横扫 ActionId: 43432 从开始读条到第8下判定约 26.2s , 预兆读条 43437 , 伤害源 4343[3456] 估计对应四个方向
    
    #endregion
    
    #region  71~80层
    [ScriptMethod(name: "—————— 71 ~ 80 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第71层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "71~74 交错路爆岩怪_炸裂（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42770"])]
    public void 交错路爆岩怪_炸裂 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路爆岩怪_炸裂{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "71~74 交错路爆岩怪_自爆（距离衰减）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42771"])]
    public void 交错路爆岩怪_自爆 (Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("远离自爆（距离衰减）", duration: 4000, true);
        if (isTTS)accessory.Method.TTS("远离自爆");
        if (isEdgeTTS)accessory.Method.EdgeTTS("远离自爆");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路爆岩怪_自爆危险区{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(11f);
        dp.DestoryAt = 4700;
        // dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"交错路爆岩怪_自爆{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(20f);
        dp1.DestoryAt = 4700;
        dp1.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
    }
    
    [ScriptMethod(name: "71~74 得到宽恕的怀疑_砂砾雨（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44763"])]
    public void 得到宽恕的怀疑_砂砾雨(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的怀疑_砂砾雨{@event.SourceId()}";
        dp.Scale = new (4f, 10f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "71~73 交错路铰颌蚁_喷沙（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44769"])]
    public void 交错路铰颌蚁_喷沙(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路铰颌蚁_喷沙{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "72~75 得到宽恕的恶意_双水流（钢铁月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42578|42748)$"])]
    public void 得到宽恕的恶意_双水流 (Event @event, ScriptAccessory accessory)
    {
        // 先钢铁: 42578  ; 先月环: 42748
        const float innerScale = 10f;  // 钢铁范围 / 月环内径
        const float outerScale = 40f;  // 月环外径
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Color = dp.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = dp.Owner = @event.SourceId();
        dp1.Delay = dp.DestoryAt = 4700;
        dp1.DestoryAt = 3000;
        
        if (@event.ActionId == 42578)
        {
            dp.Name = $"得到宽恕的恶意_圆环双水流_钢铁{@event.SourceId()}";
            dp.Scale = new Vector2(innerScale);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp1.Name = $"得到宽恕的恶意_圆环双水流_月环{@event.SourceId()}";
            dp1.Scale = new Vector2(outerScale);
            dp1.InnerScale = new Vector2(innerScale);
            dp1.Radian = float.Pi * 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
        }
        else
        {
            dp.Name = $"得到宽恕的恶意_环圆双水流_月环{@event.SourceId()}";
            dp.Scale = new Vector2(outerScale);
            dp.InnerScale = new Vector2(innerScale);
            dp.Radian = float.Pi * 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
            dp1.Name = $"得到宽恕的恶意_环圆双水流_钢铁{@event.SourceId()}";
            dp1.Scale = new Vector2(innerScale);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
        }
    }
    
    [ScriptMethod(name: "73~76 得到宽恕的傲慢_连踢（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44759"])]
    public void 得到宽恕的傲慢_连踢(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的傲慢_连踢{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 9700; // 读条时间 2.7s, 会连续发动4次
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "74~76 交错路巨虫_地震（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44767"])]
    public void 交错路巨虫_地震 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路巨虫_地震{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "75~77 交错路阿米特_推翻（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44765"])]
    public void 交错路阿米特_推翻 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路阿米特_推翻{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "75~78 得到宽恕的争执_呵斥（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42522"])]
    public void 得到宽恕的争执_呵斥(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的争执_呵斥{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "75~78 得到宽恕的争执_迅猛回旋（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42520"])]
    public void 得到宽恕的争执_迅猛回旋 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的争执_迅猛回旋{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "76~79得到宽恕的腐败_旋转火力（脱战大钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42523"])]
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
    
    [ScriptMethod(name: "76~79 得到宽恕的腐败_正面火力（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42577"])]
    public void 得到宽恕的腐败_正面火力(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的腐败_正面火力{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "77~79 得到宽恕的诋毁_造山风暴（点名圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44762"])]
    public void 得到宽恕的诋毁_造山风暴(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "得到宽恕的诋毁_造山风暴";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "77~79 得到宽恕的诋毁_变质岩波（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44761"])]
    public void 得到宽恕的诋毁_变质岩波(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的诋毁_变质岩波{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "77~79 得到宽恕的虚夸_环状射线（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44758"])]
    public void 得到宽恕的虚夸_环状射线 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的虚夸_环状射线{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"得到宽恕的虚夸_环状射线{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(60f);
        dp1.InnerScale = new Vector2(59.95f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "77~79 得到宽恕的虚夸_十字射线（十字）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44757"])]
    public void 得到宽恕的虚夸_十字射线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的虚夸_十字射线{@event.SourceId()}";
        dp.Scale = new (10f, 120f); // 待纠正
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        
        float[] rotations = { 0f, 90f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }

    
    // 80 BOSS
    
    uint PerilousLair=0; // 伤痛圆戒 钢铁
    uint RoaringRing=0; // 紫雷环戒 月环
    
    [ScriptMethod(name: "80 得到宽恕的不敬_伤痛圆戒（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43472"])]
    public void 得到宽恕的不敬_伤痛圆戒 (Event @event, ScriptAccessory accessory)
    {
        PerilousLair = 1;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的不敬_伤痛圆戒";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "80 得到宽恕的不敬_紫雷环戒（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43468"])]
    public void 得到宽恕的不敬_紫雷环戒 (Event @event, ScriptAccessory accessory)
    {
        RoaringRing = 1;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的不敬_紫雷环戒";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(48f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "伤痛圆戒&紫雷环戒变量销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^434(68|72)$"],userControl: false)]
    public void 伤痛圆戒_紫雷环戒销毁(Event @event, ScriptAccessory accessory)
    {
        PerilousLair = 0;
        RoaringRing = 0;
    }
    
    [ScriptMethod(name: "80 得到宽恕的不敬_亵渎之光（光半场刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43473"])]
    public void 得到宽恕的不敬_亵渎之光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的不敬_亵渎之光";
        dp.Color = new Vector4(1f, 1f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "80 得到宽恕的不敬_亡途黑暗 吃白色提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4518"])]
    public void 得到宽恕的不敬_亡途黑暗(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("吃白色半场刀", duration: 10000, true);
        if (isTTS)accessory.Method.TTS("吃白色半场刀");
        if (isEdgeTTS)accessory.Method.EdgeTTS("吃白色半场刀");
    }
    
    [ScriptMethod(name: "80 得到宽恕的不敬_亡途重负 少移动提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4519", "Param:1"])]
    public void 得到宽恕的不敬_亡途重负(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("减少移动，别到8层", duration: 16300, true);
        if (isTTS)accessory.Method.TTS("减少移动");
        if (isEdgeTTS)accessory.Method.EdgeTTS("减少移动");
    }
    
    /*
    [ScriptMethod(name: "80 得到宽恕的不敬_雷气循环（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43476"])]
    public void 得到宽恕的不敬_雷气循环 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"得到宽恕的不敬_雷气循环";
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
    
    [ScriptMethod(name: "81~83 召引格雷姆林_利爪（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44770"])]
    public void 召引格雷姆林_利爪(Event @event, ScriptAccessory accessory)
    {var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引格雷姆林_利爪{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "81~83 交错路异豺_夜冲击（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44774"])]
    public void 交错路异豺_夜冲击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路异豺_夜冲击{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "81~83 交错路异豺_咬杀（死刑提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44773"])]
    public void 交错路异豺_咬杀(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("打断异豺死刑 （无任何抗性）", duration: 11300, true);
        if (isTTS)accessory.Method.TTS("打断异豺死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断异豺死刑");
    }
    
    [ScriptMethod(name: "81~84 交错路卡部斯_昏暗（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44775"])]
    public void 交错路卡部斯_昏暗(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路卡部斯_昏暗{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
        
    [ScriptMethod(name: "81~84 交错路骑兵_风暴斩（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43321"])]
    public void 交错路骑兵_风暴斩(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路骑兵_风暴斩{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "81~84 交错路骑兵_残杀（冲锋击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43319"])]
    public void 交错路骑兵_残杀(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("冲锋击退+三连击退");
        if (isEdgeTTS)accessory.Method.EdgeTTS("冲锋击退+三连击退");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路骑兵_残杀{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Scale = new (6f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "83~85 召引小恶灵_冰结陷阱（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44779"])]
    public void 召引小恶灵_冰结陷阱 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引小恶灵_冰结陷阱{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "84~86 召引上级恶魔_深渊回转（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44780"])]
    public void 召引上级恶魔_深渊回转(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引上级恶魔_深渊回转{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "84~88 交错路卡玛_飞踢后摆尾（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43132"])]
    public void 交错路卡玛_飞踢后摆尾1(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路卡玛_飞踢后摆尾1{@event.SourceId()}";
        dp.Scale = new (3f, 8f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "84~88 交错路卡玛_飞踢后摆尾（扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43132"])]
    public void 交错路卡玛_飞踢后摆尾2(Event @event, ScriptAccessory accessory)
    {
        // 二段扫尾 ActionId: 43131
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路卡玛_飞踢后摆尾2{@event.SourceId()}";
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
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
    
    [ScriptMethod(name: "86~89 召引洪巴巴_三/四重强击 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4332[34]$"])]
    public void 召引洪巴巴_三四重强击(Event @event, ScriptAccessory accessory)
    {
        string skullDasherCount = @event.ActionId == 43323 ? "三" : "四";
    
        if (isText) accessory.Method.TextInfo($"{skullDasherCount}连平A + 大顺劈", duration: 4000, true);
        if (isTTS) accessory.Method.TTS($"{skullDasherCount}连平A接顺劈");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"{skullDasherCount}连平A接顺劈");
    }
    
    [ScriptMethod(name: "86~89 召引洪巴巴_长啸（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44293"])]
    public void 召引洪巴巴_长啸 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引洪巴巴_长啸{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 1200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "84~86 召引梦魔_攻心欲火（热病点名）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43322"])]
    public void 召引梦魔_攻心欲火 (Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"踢晕 <召引梦魔> 范围热病", duration: 4300, true);
        if (isTTS) accessory.Method.TTS($"踢晕梦魔热病");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"踢晕梦魔热病");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引梦魔_攻心欲火{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "86~89 召引行吟诗人_昏暗（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44787"])]
    public void 召引行吟诗人_昏暗(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引行吟诗人_昏暗{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.Radian = 60f.DegToRad();
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "86~89 召引行吟诗人_心魔（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44788"])]
    public void 召引行吟诗人_心魔 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引行吟诗人_心魔{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "86~89 召引刻耳柏洛斯_闪电（面前圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44784"])]
    public void 召引刻耳柏洛斯_闪电 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引刻耳柏洛斯_闪电{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "86~89 召引刻耳柏洛斯_地狱爪袭（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44785"])]
    public void 召引刻耳柏洛斯_地狱爪袭(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引刻耳柏洛斯_地狱爪袭{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
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
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 4700;
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
    
    [ScriptMethod(name: "86~89 召引巴尔_深渊射线（穿墙直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43134"])]
    public void 召引巴尔_深渊射线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引巴尔_深渊射线{@event.SourceId()}";
        dp.Scale = new (20f, 40f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
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
    
    [ScriptMethod(name: "91~98 召引梦祸_梦祸视线（穿墙直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44349"])]
    public void 召引梦祸_梦祸视线(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("穿墙直线");
        if (isEdgeTTS)accessory.Method.EdgeTTS("穿墙直线");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引梦祸_梦祸视线{@event.SourceId()}";
        dp.Scale = new (5f, 41f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
                      else {dp.Color = accessory.Data.DefaultDangerColor; }
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
    
    [ScriptMethod(name: "91~93 交错路食人花_腐烂恶臭（穿墙直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44500"])]
    public void 交错路食人花_腐烂恶臭(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("穿墙直线");
        if (isEdgeTTS)accessory.Method.EdgeTTS("穿墙直线");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"交错路食人花_腐烂恶臭{@event.SourceId()}";
        dp.Scale = new (12f, 45f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
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
    

    [ScriptMethod(name: "93~95 召引扎哈克_尾部打击（瞬发扫尾）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:18854"])]
    public void 召引扎哈克_尾部打击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引扎哈克_尾部打击{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.5f); // 参考值, 目标圈 3.5m
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 600000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "召引扎哈克_尾部打击销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:18854"],userControl: false)]
    public void 召引扎哈克_尾部打击销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"召引扎哈克_尾部打击{@event.SourceId()}");
    }

    [ScriptMethod(name: "召引扎哈克_尾部打击备用销毁", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:18854"],userControl: false)]
    public void 召引扎哈克_尾部打击备用销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"召引扎哈克_尾部打击{@event.SourceId()}");
    }
    
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
    
    [ScriptMethod(name: "96~98 召引破坏本能_怪光线（穿墙直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44354"])]
    public void 召引破坏本能_怪光线(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("穿墙直线");
        if (isEdgeTTS)accessory.Method.EdgeTTS("穿墙直线");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召引破坏本能_怪光线{@event.SourceId()}";
        dp.Scale = new (8f, 60f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
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
    // 深渊烈焰（步进地火）普通难度          先上下 ActionId: 44075 / 后左右 ActionId: 44076
    // 召唤晶体: 44078 / 晶体爆炸: 44079   先左右 ActionId: 44074 / 后上下 ActionId: 44077
    // debuff: 4559 暗 / 4560 光
    // 以太吸取 普通难度  44088 短暗 / 44089 长暗 / 44090 短光 / 44092 长光
    
    uint _myLightVengeance=0;
    uint _myDarkVengeance=0; 
    
    
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
    public void 净罪之环tts(Event @event, ScriptAccessory accessory)
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
    
    // 卓异的悲寂      NPCID: 14037 目标圈 28.5m
    // 被侵蚀的食罪灵  NPCID: 14038 目标圈 15.0m
    
    // P1 深渊爆焰（黑白配 + 踩塔 + 地火） → 光耀之剑 + 烈焰锢 / 火球 + 拉线 & 十字火 → 棘刺尾（挡枪分摊） → 集火小怪后职能站位准备进P2
    
    // 生成水晶:44115（每次6个）/ 水晶读条爆炸:44118 / 水晶DataId: 2014832 // 每次移动4m, 爆炸间隔 0.8~0.9s
    
    private readonly Dictionary<uint, string> crystalDirections = new Dictionary<uint, string>();
    private readonly HashSet<uint> processedCrystals = new HashSet<uint>();
    private readonly Dictionary<Vector3, string> crystalPositionDirections = new Dictionary<Vector3, string>();

    private int firstGroupCount = 0;
    private int secondGroupCount = 0;
    private int firstGroup44115Count = 0;
    private int secondGroup44115Count = 0;
    private bool isFirstGroupComplete = false;
    private string firstGroupDirection = "vertical";
    private string secondGroupDirection = "horizontal"; 
    private bool resetScheduled = false;

    [ScriptMethod(name: "深渊爆焰（地火）读条提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4479[78]$"])]
    public void Q40_深渊爆焰提示(Event @event, ScriptAccessory accessory)
    {
        var isFirst = @event.ActionId == 44797 ? "左右" : "上下";
        var isSecond = @event.ActionId == 44797 ? "上下" : "左右";
        
        if (isText) accessory.Method.TextInfo($"地火：  先{isFirst} ， 再{isSecond}", duration: 16700, true);
        if (isTTS) accessory.Method.TTS($"先{isFirst}, 后{isSecond}");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"先{isFirst}, 后{isSecond}");
        accessory.Method.SendChat($"/e [Kodakku] 地火记录: 先{isFirst}，再{isSecond}");
    }
    
    [ScriptMethod(name: "深渊爆焰晶体生成技能", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:44115"], userControl:false)]
    public void Q40_深渊爆焰晶体生成技能(Event @event, ScriptAccessory accessory)
    {
        Vector3 spawnPosition = @event.EffectPosition;
        
        string direction;
        
        if (firstGroup44115Count < 6)
        {
            direction = firstGroupDirection;
            firstGroup44115Count++;
            if(isDeveloper) accessory.Method.SendChat($"/e [地火44115] 第一组{firstGroup44115Count}/6: 位置{spawnPosition}, 方向{direction}");
        }
        else if (secondGroup44115Count < 6)
        {
            direction = secondGroupDirection;
            secondGroup44115Count++;
            if(isDeveloper) accessory.Method.SendChat($"/e [地火44115] 第二组{secondGroup44115Count}/6: 位置{spawnPosition}, 方向{direction}");
        }
        else
        {
            direction = "vertical";
            if(isDeveloper) accessory.Method.SendChat($"/e [地火警告] 44115技能超出12个限制");
        }
        
        // 直接记录位置和方向
        crystalPositionDirections[spawnPosition] = direction;
        
        if (firstGroup44115Count + secondGroup44115Count == 12)
        {
            if(isDeveloper) accessory.Method.SendChat($"/e [地火] 通过44115完成12个晶体记录");
        }
    }
    
    [ScriptMethod(name: "深渊爆焰晶体生成调试", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2014832"], userControl:false)]
    public void Q40_深渊爆焰晶体生成调试(Event @event, ScriptAccessory accessory)
    {
        uint crystalSourceId = @event.SourceId();
        Vector3 crystalPosition = @event.SourcePosition();
        
        if(isDeveloper) accessory.Method.SendChat($"/e [地火调试] ObjectChanged: {crystalSourceId} 位置{crystalPosition}");
    }
    
    [ScriptMethod(name: "深渊爆焰BOSS读条记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4479[78]$"] ,userControl:false)]
    public void Q40_深渊爆焰BOSS读条记录(Event @event, ScriptAccessory accessory)
    {
        ResetMechanic();
        
        if (@event.ActionId == 44798)
        {
            firstGroupDirection = "vertical"; // 先上下
            secondGroupDirection = "horizontal";
        }
        else if (@event.ActionId == 44797)
        {
            firstGroupDirection = "horizontal"; // 先左右
            secondGroupDirection = "vertical";
        }
        
        if(isDeveloper) accessory.Method.SendChat($"/e [地火] 重置: 先{firstGroupDirection}后{secondGroupDirection}, 44115计数清零");
    }
    
    [ScriptMethod(name: "深渊爆焰（地火）初始炸绘制", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44118"])]
    public void Q40_深渊爆焰初始(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "深渊爆焰初始";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 6700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "深渊爆焰（地火）步进炸", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:44118"])]
    public void Q40_深渊爆焰步进(Event @event, ScriptAccessory accessory)
    {
        uint fireSourceId = @event.SourceId();
        Vector3 firePosition = @event.SourcePosition();
        
        string direction = FindDirectionByPosition(firePosition);
        
        if (string.IsNullOrEmpty(direction))
        {
            direction = "vertical";
            if(isDeveloper) accessory.Method.SendChat($"/e [地火警告] 未找到位置{firePosition}的方向记录");
            return;
        }
        
        if(isDeveloper) accessory.Method.SendChat($"/e [地火] 地火源{fireSourceId}使用方向: {direction}");
        
        int maxSteps = (direction == "vertical") ? 7 : 10;
        
        for (int predictStep = 1; predictStep <= 2; predictStep++)
        {
            var predictPositions = CalculateStepPositions(firePosition, direction, predictStep);
            foreach (var predictPos in predictPositions)
            {
                var predictDp = accessory.Data.GetDefaultDrawProperties();
                predictDp.Name = $"深渊爆焰步进{predictStep}预测";
                predictDp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
                predictDp.Owner = fireSourceId;
                predictDp.Position = predictPos;
                predictDp.Scale = new Vector2(5f);
                predictDp.DestoryAt = 800 * predictStep;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, predictDp);
            }
        }
        
        for (int step = 1; step <= maxSteps; step++) 
        {
            var stepPositions = CalculateStepPositions(firePosition, direction, step);
            
            foreach (var stepPos in stepPositions)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"深渊爆焰步进{step}";
                dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
                dp.Owner = fireSourceId;
                dp.Position = stepPos;
                dp.Scale = new Vector2(5f);
                dp.DestoryAt = 800;
                dp.Delay = 800 * step;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                
                for (int predictStep = step + 1; predictStep <= step + 2; predictStep++)
                {
                    if (predictStep <= maxSteps)
                    {
                        var nextStepPositions = CalculateStepPositions(firePosition, direction, predictStep);
                        foreach (var nextStepPos in nextStepPositions)
                        {
                            var predictDp = accessory.Data.GetDefaultDrawProperties();
                            predictDp.Name = $"深渊爆焰步进{predictStep}预测";
                            
                            float alpha = predictStep == step + 1 ? 0.8f : 0.4f;
                            predictDp.Color = accessory.Data.DefaultDangerColor.WithW(alpha);
                            
                            predictDp.Owner = fireSourceId;
                            predictDp.Position = nextStepPos;
                            predictDp.Scale = new Vector2(5f);
                            predictDp.DestoryAt = 800;
                            predictDp.Delay = 800 * step; 
                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, predictDp);
                        }
                    }
                }
            }
        }
    }

    private string FindDirectionByPosition(Vector3 position, float tolerance = 0.5f)
    {
        foreach (var kvp in crystalPositionDirections)
        {
            if (IsPositionMatch(kvp.Key, position, tolerance))
            {
                return kvp.Value;
            }
        }
        return null;
    }
    
    private bool IsPositionMatch(Vector3 pos1, Vector3 pos2, float tolerance = 0.5f)
    {
        return Math.Abs(pos1.X - pos2.X) < tolerance &&
               Math.Abs(pos1.Y - pos2.Y) < tolerance &&
               Math.Abs(pos1.Z - pos2.Z) < tolerance;
    }
    
    private List<Vector3> CalculateStepPositions(Vector3 startPos, string direction, int step)
    {
        float offset = 4f * step;
        var positions = new List<Vector3>();

        switch (direction)
        {
            case "vertical":
                positions.Add(new Vector3(startPos.X, startPos.Y, startPos.Z + offset));
                positions.Add(new Vector3(startPos.X, startPos.Y, startPos.Z - offset));
                break;
            case "horizontal":
                positions.Add(new Vector3(startPos.X + offset, startPos.Y, startPos.Z));
                positions.Add(new Vector3(startPos.X - offset, startPos.Y, startPos.Z));
                break;
            default:
                positions.Add(startPos);
                break;
        }
        return positions;
    }
    private async void ScheduleDelayedReset(ScriptAccessory accessory)
    {
        int maxWaitTime = 30000;
        
        await System.Threading.Tasks.Task.Delay(maxWaitTime);
        
        if (processedCrystals.Count > 0)
        {
            ResetMechanic();
            if(isDeveloper) accessory.Method.SendChat($"/e [Debug] 已重置地火晶体计数");
        }
    }

    private void ResetMechanic()
    {
        firstGroup44115Count = 0;
        secondGroup44115Count = 0;
        crystalPositionDirections.Clear();
        firstGroupDirection = "vertical";
        secondGroupDirection = "horizontal";
        resetScheduled = false;
        processedCrystals.Clear();
        crystalDirections.Clear();
    }
    
    [ScriptMethod(name: "深渊极光 踩塔提示", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:2", "Index:27"])]
    public void Q40_深渊极光提示 (Event @event, ScriptAccessory accessory)
    {
        // 实际上 Index 为 27~30 ，对应场上4座塔
        if (isText)accessory.Method.TextInfo($"吃白色，准备踩塔", duration: 2000, false);
        if (isTTS)accessory.Method.TTS($"吃白色，准备踩塔");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"吃白色，准备踩塔");
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
    
    uint _blackandwhite = 0; // 黑白配 点名记录
    
    [ScriptMethod(name: "黑白配 点名记录", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^004(D|E)$"] ,userControl:false)]
    public void Q40_黑白配点名记录 (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        _blackandwhite = 1;
        if(isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 成功记录黑白配点名");
    }

    [ScriptMethod(name: "黑白配 判定提示", eventType: EventTypeEnum.Director, eventCondition: ["Command:80000026", "Instance:8003EA93"],suppress: 1000)]
    public async void Q40_黑白配判定提示 (Event @event, ScriptAccessory accessory)
    {
        // 之后的参数为 [31~34|9|1|0]，但是鸭子用不了.jpg
        // 光与暗的以太中和了……
        if (_blackandwhite == 0)return;
        await Task.Delay(1000);
        Console.Beep(2000, 200); 
        // if (isTTS)accessory.Method.TTS($"重！");
        // if (isEdgeTTS)accessory.Method.EdgeTTS($"重！");
        // accessory.Method.SendChat($"/e [黑白配] 中! <se.4> <se.4> <se.4> ");

        _blackandwhite = 0;
    }
    
    [ScriptMethod(name: "光耀之剑（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^441(04|10)$"])]
    public void Q40_光耀之剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"光耀之剑";
        dp.Scale = new (15f, 30f);
        dp.Owner = @event.SourceId();
        dp.Offset = new Vector3 (0, 0 ,15); // 原本实体在直线中间，应用 Straight，但考虑到omen效果还是选用 Rect偏移
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.ActionId() == 44104 ? 4700 : 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
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
    
    [ScriptMethod(name: "烈焰链 准备提示", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0061"])]
    public void Q40_烈焰链_准备提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("回中间准备拉线", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("回中间准备拉线");
        if (isEdgeTTS)accessory.Method.EdgeTTS("回中间准备拉线");
    }
    
    [ScriptMethod(name: "烈焰链 拉线提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4563"])]
    public void Q40_烈焰链_拉线提示(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        // if (isText)accessory.Method.TextInfo("扯断连线", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("扯断连线");
        if (isEdgeTTS)accessory.Method.EdgeTTS("扯断连线");
    }
    
    uint _spinelash = 0; // 棘刺尾 读条点名直线记录
    
    [ScriptMethod(name: "棘刺尾（点名分摊提示）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:020F"])]
    public void Q40_棘刺尾提示(Event @event, ScriptAccessory accessory)
    {
        if (HelperExtensions.GetCurrentTerritoryId() != MapIds.TheFinalVerseQuantum) return; // 深想战 - 卓异的悲寂 DataId: 18670
        
        _spinelash++;
        
        if (isTTS)accessory.Method.TTS("挡枪分摊");
        if (isEdgeTTS)accessory.Method.EdgeTTS("挡枪分摊");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"棘刺尾预兆";
        dp.Scale = new (1f, 30f);
        dp.Color = accessory.Data.DefaultSafeColor.WithW(1.4f);
        
        switch (_spinelash)
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
        
        if (isDeveloper)  accessory.Method.SendChat($"/e [DEBUG] 当前点名次数:{_spinelash}");
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
    
    // P2 戒律的光链（职能debuff）→ 烈焰领域（引导牢笼连线 +吸引） → 引导三连黄圈 → 尾连击（死刑塔 + 斜线AOE） → 黑暗神圣（AOE+DOT）→ 尾连击（死刑塔 + 斜线AOE）
    // → 深渊爆焰（存储地火）+ 引导三连黄圈 → 净罪之环（抓人牢笼）+黑白配 → 地火判定
    
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
    
    [ScriptMethod(name: "烈焰领域（吸引）读条提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44153"])]
    public void Q40_烈焰领域提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("吸引，坦克最远引导连线", duration: 5000, true);
        if (isTTS) accessory.Method.TTS($"吸引");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"吸引");
    }
    
    [ScriptMethod(name: "烈焰领域（吸引）自动防击退 [T职以外]", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44153"])]
    public void Q40_烈焰领域自动防击退(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank) return;

        if (AutoAntiKnockback == AutoAntiKnockbackEnum.亲疏自行) 
        { 
            accessory.Method.SendChat($"/ac 亲疏自行"); 
            accessory.Method.SendChat($"/e [Kodakku]：已尝试自动使用防击退 - 亲疏自行");
        }
        else if (AutoAntiKnockback == AutoAntiKnockbackEnum.沉稳咏唱) 
        { 
            accessory.Method.SendChat($"/ac 沉稳咏唱"); 
            accessory.Method.SendChat($"/e [Kodakku]：已尝试自动使用防击退 - 沉稳咏唱");
        }
        else if (AutoAntiKnockback == AutoAntiKnockbackEnum.DR && isHack) 
        { 
            accessory.Method.SendChat($"/pdr load AutoAntiKnockback"); 
            accessory.Method.SendChat($"/e [Kodakku]：已尝试自动开启防击退 - DR");
        }
        else if (AutoAntiKnockback == AutoAntiKnockbackEnum.IChing && isHack) 
        {
            accessory.Method.SendChat($"/i-ching-commander anti_knock 0 0"); 
            accessory.Method.SendChat($"/e [Kodakku]：已尝试自动开启防击退 - I-Ching");
        }
        
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
    
    [ScriptMethod(name: "尾连击安全区指路", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^4415[78]$"])]
    public void Q40_尾连击指路(Event @event, ScriptAccessory accessory)
    {
        // 44157 打右上左下 左上安全 ; 44158 打左上右下 右上安全
        
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (!isTank) return; 
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "尾连击安全区指路";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = @event.ActionId() ==44157 ? new Vector3(-603.5f, 0f, -312f) : new Vector3(-596.5f, 0f, -312.4f);
        dp.Scale = new(0.3f);
        dp.Delay = 1500;
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "黑暗神圣（AOE）读条提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44164"])]
    public void Q40_黑暗神圣提示(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo("流血AOE", duration: 6000, true);
        if (isTTS) accessory.Method.TTS($"流雪AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"流雪AOE");
    }
    
    // P3 罪与罚（传毒） [第一次] 光耀之剑（直线）+ 烈焰锢 / 火球 → [第二次] 棘刺尾（挡枪分摊 + 小怪） → 净罪之环（抓人牢狱）+ 拉线+十字火
    // → [第三次] 深渊爆焰 存储地火 + 黑白配 → 以太吸取（buff检测）+ 地火判定 → 黑暗神圣（AOE+DOT）
    
    [ScriptMethod(name: "罪与罚（传毒）读条提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44165"])]
    public void Q40_罪与罚提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("传毒阶段: 先吃暗", duration: 5000, true);
        if (isTTS) accessory.Method.TTS($"吃暗，准备传毒");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"吃暗，准备传毒");
    }
    
    [ScriptMethod(name: "罪积蓄（毒）点名播报", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4567", "Param:1"])]
    public void 罪积蓄点名播报(Event @event, ScriptAccessory accessory)
    {
        string tname = @event["TargetName"]?.ToString() ?? "未知目标";
        if (isTTS)accessory.Method.TTS($"毒点{tname}");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"毒点{tname}");
    }
    
    [ScriptMethod(name: "罪积蓄（毒）绘制", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4567"])]
    public async void 罪积蓄绘制(Event @event, ScriptAccessory accessory)
    {
        uint layerCount = @event.StatusParam;
    
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = dp.Name = $"罪积蓄{layerCount}";
        dp1.Owner = dp.Owner = @event.TargetId();
        
        if (layerCount == 1)
        {
            accessory.Method.RemoveDraw($"罪积蓄.*");
            await Task.Delay(50); 
            dp.Color = new Vector4(1f, 1f, 1f, 2f);
            dp1.Color = new Vector4(1f, 1f, 1f, 10f);
            dp1.Scale = dp.Scale = new Vector2(0.7f); // 描边外径
            dp1.InnerScale = new Vector2(0.65f);
            dp1.Radian = float.Pi * 2;
            dp1.DestoryAt = dp.DestoryAt = 30000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        else if (layerCount == 12)
        {
            accessory.Method.RemoveDraw($"罪积蓄.*");
            await Task.Delay(50); 
            dp.Color = new Vector4(1f, 1f, 1f, 0.8f);
            dp.Scale = new Vector2(4f);
            dp.DestoryAt = 12000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        else if (layerCount >= 12 && layerCount <= 15)
        {
            if (isTTS) accessory.Method.TTS($"{layerCount}");
            if (isEdgeTTS) accessory.Method.EdgeTTS($"{layerCount}");
        }
        else
        {
            return;
        }
    }
    
    [ScriptMethod(name: "罪积蓄销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4567"],userControl: false)]
    public void 罪积蓄销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"罪积蓄.*");
    }
    
    [ScriptMethod(name: "罪与罚（传毒）驱散死宣提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4594"])]
    public void Q40_罪与罚驱散死宣提示(Event @event, ScriptAccessory accessory)
    {
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (isHealer && isText)accessory.Method.TextInfo("驱散死宣", duration: 3300, true);
        if (isHealer && isTTS) accessory.Method.TTS($"驱散死宣");
        if (isHealer && isEdgeTTS) accessory.Method.EdgeTTS($"驱散死宣");
    }
    
    
    /*
    [ScriptMethod(name: "以太吸取（buff检测）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4413[13]$"])]
    public void Q40_以太吸取(Event @event, ScriptAccessory accessory)
    {
        // debuff: 4559 暗 / 4560 光
        // 需要额外判断当前自身是否持有【毒 4567】
        (string firstDrainAether, string secondDrainAether) = @event.ActionId switch
        {
            // 44129 => ("吃光", "吃暗"), // 短暗
            // 44130 => ("吃暗", "吃光"), // 长暗
            44131 => ("吃暗", "吃光"), // 短光
            44133 => ("吃光", "吃暗"), // 长光
            _ => ("未知", "未知")
        };
    
        if (isText)accessory.Method.TextInfo($"先{firstDrainAether}，再{secondDrainAether}", duration: 10000, true);
        if (isTTS)accessory.Method.TTS($"先{firstDrainAether}，再{secondDrainAether}");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"先{firstDrainAether}，再{secondDrainAether}");
    }
    */
    
    // P4 烈焰缠身 → 深渊爆焰（存储地火）
    
    [ScriptMethod(name: "烈焰缠身（火人阶段）读条TTS提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44170"])]
    public void Q40_烈焰缠身提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("火人阶段就位", duration: 3000, true);
        if (isTTS) accessory.Method.TTS($"火人阶段就位");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"火人阶段就位");
    }
    
    [ScriptMethod(name: "自爆（火人爆炸）TTS提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44171"])]
    public void Q40_火焰分身_自爆(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo("AOE", duration: 1000, true);
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }
    
    #endregion
    
    #region 底裤部分
    
    [ScriptMethod(name: "—————— 底裤部分（需要对应插件与权限） ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 底裤部分(Event @event, ScriptAccessory accessory) { }
    
    // 这次 BOSS房 不可能变身 不用考虑 ， 由于变身全是烛台给的buff, 也不用考虑过层时触发的 Duration
    
    /*
    [ScriptMethod(name: "[DR] 天气冲刺时，移速复原至默认值", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1548"])]
    public void SprintSpeedDefault(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed -1");
        accessory.Method.SendChat($"/e 鸭鸭：[DR] 移速已更改：默认");
        if (isTTS)accessory.Method.TTS("移速已复原至默认值");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已复原至默认值");
    }
    */
    
    [ScriptMethod(name: "[DR] 天气冲刺时，将移速固定至1.4倍", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1548"])]
    public void SprintSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.4");
        accessory.Method.SendChat($"/e 鸭鸭：[DR] 移速已更改：1.4x");
        if (isTTS)accessory.Method.TTS("移速已更改至1.4倍");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已更改至1.4倍");
    }
    
    [ScriptMethod(name: "[DR] 极速冲刺时，移速复原至默认值", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4709"])]
    public void SwiftSprintSpeedDefault(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed -1");
        accessory.Method.SendChat($"/e 鸭鸭：[DR] 移速已更改：默认");
        if (isTTS)accessory.Method.TTS("移速已复原至默认值");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已复原至默认值");
    }

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