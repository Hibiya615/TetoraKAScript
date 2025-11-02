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

namespace Pilgrims_Traverse;

[ScriptType(guid: "3f65b3c0-df48-4ef8-89ae-b8091b7690f1", name: "朝圣交错路", author: "Tetora", 
    territorys: [1281, 1282, 1283, 1284, 1285, 1286, 1287, 1288, 1289, 1290, 1311, 1333],
    version: "0.0.0.1",note: noteStr)]

public class Pilgrims_Traverse
{
    const string noteStr =
        """
        v0.0.0.1:
        朝圣交错路测试绘制
        注：方法设置中的层数仅做分割线效果，并不是批量开关
        出现问题请携带ARR反馈！
        """;

    
    #region 各种记录
    
    /*  StatusID
     *  变身 565 [StackCount:42 曼提克 ]
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
    [ScriptMethod(name: "拟态怪_怨念提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6397"])]
    public void 拟态怪_怨念(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("打断拟态怪", duration: 2300, true);
        if (isTTS) accessory.Method.TTS("打断拟态怪");
        if (isEdgeTTS)accessory.Method.EdgeTTS("打断拟态怪");
    }
    
    #region  1~10层 
    [ScriptMethod(name: "—————— 1 ~ 10 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第1层(Event @event, ScriptAccessory accessory) { }
    
        
    [ScriptMethod(name: "花こびと_百花繚乱（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44054"])]
    public void 百花繚乱 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"百花繚乱{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "花人_押し花（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44057"])]
    public void 押し花 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"押し花";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region  11~20层 
    [ScriptMethod(name: "—————— 11 ~ 20 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第11层(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "フォーギヴン・エミュレーション_タッチダウン（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4345[6-9]$"])]
    public void タッチダウン (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・エミュレーション_タッチダウン";
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
    
    [ScriptMethod(name: "フォーギヴン・トレチャリー_救済の拳", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40401|40549)$"])]
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
        // 右刀 44690 ； 左刀 44691
        var isR = @event.ActionId == 44690;
        
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
    
    #region  51~60层
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
    
    [ScriptMethod(name: "元祖マリクテンダー_ニードルシュート（前后扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44866"])]
    public void ニードルシュート (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"元祖マリクテンダー_ニードルシュート";
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
    
    [ScriptMethod(name: "元祖マリクテンダー_株分け（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4485[89]$"])]
    public void 株分け(Event @event, ScriptAccessory accessory)
    {
        var isBig = @event.ActionId == 44859;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"元祖マリクテンダー_株分け";
        dp.Scale = isBig? new (30f, 30f): new (10f, 10f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
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
    
    [ScriptMethod(name: "トラバース・タロース_サブダックション（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42516"])]
    public void サブダックション (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・タロース_サブダックション{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(11f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 5000;
        dp.DestoryAt = @event.DurationMilliseconds() - 5000;
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
    
    [ScriptMethod(name: "フォーギヴン・ライオティング_ショックウェーブ（二连左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4221[46]$"])]
    public void ショックウェーブ(Event @event, ScriptAccessory accessory)
    {
        // 先右刀 42216 ； 先左刀 42214
        var isR = @event.ActionId == 42216;
        
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ライオティング_ショックウェーブ1";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f + IbcHelper.GetHitboxRadius(obj)); // 30m + 目标圈
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"フォーギヴン・ライオティング_ショックウェーブ2";
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
    
    // 70 BOSS
    
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
    
    [ScriptMethod(name: "フォーギヴン・ズィール_ズィールグラワー_光球2（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43417"])]
    public void ズィールグラワー_光球2 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ズィール_ズィールグラワー_光球2";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
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
    
    [ScriptMethod(name: "トラバース・フルドゥ_炸裂（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44770"])]
    public void フルドゥ_炸裂 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・フルドゥ_炸裂{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = @event.DurationMilliseconds();
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
    
    [ScriptMethod(name: "フォーギヴン・スパイト_双水流（钢铁月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42578|42748)$"])]
    public void スパイト_双水流 (Event @event, ScriptAccessory accessory)
    {
        // 先钢铁: 42578  ; 先月环: 42748
        var isCircle = @event.ActionId == 42578;
    
        var innerRadius = isCircle ? 10f : 40f;
        var outerRadius = isCircle ? 40f : 10f;
    
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・スパイト_双水流{(isCircle ? "钢铁" : "月环")}{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(innerRadius);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"フォーギヴン・スパイト_双水流{(isCircle ? "月环" : "钢铁")}{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(outerRadius);
        dp1.InnerScale = new Vector2(innerRadius);
        dp1.Radian = float.Pi * 2;
        dp1.Delay = @event.DurationMilliseconds();
        dp1.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
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
    
    [ScriptMethod(name: "フォーギヴン・スランダー_メタモーフィックブラスト（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44761"])]
    public void メタモーフィックブラスト(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・スランダー_メタモーフィックブラスト{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(11f + IbcHelper.GetHitboxRadius(obj)); // 11m + 目标圈
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "フォーギヴン・ディセンション_マイティスピン（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42523"])]
    public void ディセンション_マイティスピン (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"フォーギヴン・ディセンション_マイティスピン{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(45f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
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
    
    [ScriptMethod(name: "フォーギヴン・プロファニティー_紫雷の輪戒（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43468"])]
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
    
    [ScriptMethod(name: "フォーギヴン・プロファニティー_痛傷の円戒（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43472"])]
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
    
    [ScriptMethod(name: "フォーギヴン・プロファニティー_冒涜の光（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43473"])]
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
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"トラバース・キャマ_キックアンドテイル2{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f + IbcHelper.GetHitboxRadius(obj)); // 6m + 目标圈
        dp.Radian = 120f.DegToRad();
        dp.Delay = 3000;
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
        dp.Name = "インヴォークド・カイム_大凶眼";
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
    
    [ScriptMethod(name: "インヴォークド・バエル_インシネレイトラハール（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43133"])]
    public void インシネレイトラハール (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"インヴォークド・バエル_インシネレイトラハール{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(46f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
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
    
    [ScriptMethod(name: "インヴォークド・ケルベロス_テイルブロー（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44786"])]
    public void テイルブロー(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"インヴォークド・ケルベロス_テイルブロー{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f + IbcHelper.GetHitboxRadius(obj)); // 15m + 目标圈
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    // 90 BOSS
    
    [ScriptMethod(name: "マラコーダ_旋背撃（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4425[01]$"])]
    public void マラコーダ_旋背撃(Event @event, ScriptAccessory accessory)
    {
        // 左下安全: 44250 ; 右下安全: 44251
        var isR = @event.ActionId == 44250;
        
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"マラコーダ_旋背撃";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f + IbcHelper.GetHitboxRadius(obj)); // 30m + 目标圈
        dp.Radian = 270f.DegToRad();
        dp.Rotation = isR ? 45f.DegToRad() : 315f.DegToRad();
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "マラコーダ_縦断衝/横断衝（前后/左右扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4425[89]$"])]
    public void マラコーダ_縦断横断衝(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
    
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"マラコーダ_縦断横断衝";
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
    
    // マラコーダ_尽滅 
    
    [ScriptMethod(name: "指向魔法陣_魔陣光（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43796|44257)$"])]
    public void 指向魔法陣_魔陣光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"指向魔法陣_魔陣光";
        dp.Scale = new (10f, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    #endregion
    
    #region 91~100层 
    [ScriptMethod(name: "—————— 91 ~ 100 层 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 第91层(Event @event, ScriptAccessory accessory) { }
    #endregion

    
    /*
    #region 底裤部分
    
    [ScriptMethod(name: "—————— 底裤部分（需要对应插件与权限） ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 底裤部分(Event @event, ScriptAccessory accessory) { }
    
    // 过层时候会触发一次 解除变身（60s）和变身（剩余时间），所以需要额外限制 Duration 以免在过层时触发

    [ScriptMethod(name: "[DR] 变身曼提克时，移速改为1.5倍", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:565", "StackCount:42", "Duration:60.00"])]
    public void AddManticoreSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.5");
        accessory.Method.SendChat($"/e 鸭鸭：[DR] 移速已更改：1.5x");
        if (isTTS)accessory.Method.TTS("移速已更改至1.5倍");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已更改至1.5倍");
    }
    
    
    [ScriptMethod(name: "[DR] 曼提克取消时，移速复原至默认值", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:565", "StackCount:42", "Duration:0.00"])]
    public void RemoveManticoreSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed -1");
        accessory.Method.SendChat($"/e 鸭鸭：[DR] 移速已更改：默认");
        if (isTTS)accessory.Method.TTS("移速已复原至默认值");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已复原至默认值");
    }
    

    [ScriptMethod(name: "[DR] 变身梦魔时，移速改为1.2倍", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:565", "StackCount:43", "Duration:60.00"])]
    public void AddSuccubusSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.2");
        accessory.Method.SendChat($"/e 鸭鸭：[DR] 移速已更改：1.2x");
        if (isTTS)accessory.Method.TTS("移速已更改至1.2倍");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已更改至1.2倍");
    }
    
    [ScriptMethod(name: "[DR] 梦魔取消时，移速复原至默认值", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:565", "StackCount:43", "Duration:0.00"])]
    public void RemoveSuccubusSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed -1");
        accessory.Method.SendChat($"/e 鸭鸭：[DR] 移速已更改：默认");
        if (isTTS)accessory.Method.TTS("移速已复原至默认值");
        if (isEdgeTTS)accessory.Method.EdgeTTS("移速已复原至默认值");
    }
    
    
    // 梦魔和基路伯会用于BOSS房，不做自动遁地处理
    
    [ScriptMethod(name: "[IC] 变身曼提克时，取消遁地", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:565", "StackCount:42", "Duration:60.00"])]
    public void AddManticoreDepths(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/i-ching-commander y_adjust 0");
        accessory.Method.SendChat($"/e 鸭鸭：[IC] 已取消遁地");
        if (isText) accessory.Method.TextInfo("已取消遁地", duration: 1300, true);
        // if (isTTS)accessory.Method.TTS("已取消遁地");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("已取消遁地");
    }
    
    
    [ScriptMethod(name: "[IC] 曼提克取消时，自动遁地", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:565", "StackCount:42", "Duration:0.00"])]
    public void RemoveManticoreDepths(Event @event, ScriptAccessory accessory)
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
    */
    
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
