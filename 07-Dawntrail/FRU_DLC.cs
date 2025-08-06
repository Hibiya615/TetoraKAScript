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

namespace FRU_DLC;

[ScriptType(guid: "981e39b5-ea99-4b85-a4b1-0e698d4c4036", name: "FRU_DLC [光暗未来绝境战_额外补充]", territorys: [1238],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class FRU_DLC
{
    const string noteStr =
        """
        v0.0.0.1:
        光暗未来绝境战 额外补充部分
        可以与灵视的绘制与连桑的Patch补丁同时使用，并无严重冲突部分
        可能会有部分横幅提醒冲突，横幅只会显示最早出现的一个，可以自行参考时间轴关闭不需要的提醒
        如【P4 盖娅出现横幅倒计时】与【P4 忘却的此岸 AOE提示】和灵视的【P4 天光轮回躲避提示】横幅冲突，请三选一启用
        """;
    
    #region 基础控制
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    private static List<string> _BottleGemdraught = ["刚力宝药3级", "巧力宝药3级", "智力宝药3级", "意力宝药3级","刚力宝药2级", "巧力宝药2级", "智力宝药2级", "意力宝药2级"];
    
    public enum BottleGemdraughtEnum
    {
        None = -1,
        刚力宝药3级 = 0,
        巧力宝药3级 = 1,
        智力宝药3级 = 2,
        意力宝药3级 = 3,
        刚力宝药2级 = 4,
        巧力宝药2级 = 5,
        智力宝药2级 = 6,
        意力宝药2级 = 7,
    }
    
    /*
    
    [UserSetting(note: "请选择爆发药（仅HQ）")]
    public BottleGemdraughtEnum BottleGemdraught { get; set; } = BottleGemdraughtEnum.None;
    
    [UserSetting("P2.5 击退前自动爆发药")]
    public bool isAutoBottleGemdraught { get; set; } = false;
    
    */
    
    
    private static List<string> _AkhMorn = ["盖娅", "琳"];
    
    public enum AkhMornEnum
    {
        None = -1,
        盖娅 = 0,
        琳 = 1,
    }
    
    [UserSetting(note: "P4 死亡轮回分摊位置")]
    public AkhMornEnum AkhMorn { get; set; } = AkhMornEnum.None;
    
    
    [UserSetting("P4二运自动防击退 [Y字击退打法]")]
    public bool isAutoAntiKnockback { get; set; } = false;
    
    [UserSetting("龙骑开荒小提示")]
    public bool isDRG { get; set; } = false;
    
    [UserSetting("[龙骑] 光暴自动提前猛枪（踩塔前）")]
    public bool isAutoLanceCharge { get; set; } = false;
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion
    
    #region 记录与转换
    
    public enum FRU_Phase
    {
        Init,          // 初始
        Fatebreaker,   // P1
        FallOfFaith, // P1 罪壤断（雷火线）
        UsurperOfFrost, // P2
        LightRampant, // P2 光之失控
        OracleOfDarkness, // P3
        Les, // P4
        DarklitDragonsong, // P4一运 光与暗的龙诗
        CrystallizeTime, // P4二运 时间结晶
        Pandora, // P5
    }
    
    FRU_Phase phase = FRU_Phase.Init;
    
    // uint BurnishedGlory=0;  
    
    public void Init(ScriptAccessory accessory) {
        phase = FRU_Phase.Init;
    }
    
    [ScriptMethod(name: "P1 开场转换（记录暴风破）", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4014[48]$"])]
    public void P1开场转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.Init => FRU_Phase.Fatebreaker,
        };
        if(isDeveloper) accessory.Method.SendChat($"/e [虎のDebug]：阶段成功转换至开场");
    }
    
    [ScriptMethod(name: "P1 阶段转换 - 罪壤断", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40140"])]
    public void P1罪壤断转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.Fatebreaker => FRU_Phase.FallOfFaith,
        };
        if(isDeveloper) accessory.Method.SendChat($"/e [虎のDebug]：阶段成功转换至雷火线");
    }
    
    [ScriptMethod(name: "P2 开场转换", userControl: false, eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17823"])]
    public void P2开场转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.FallOfFaith => FRU_Phase.UsurperOfFrost,
        };
        if(isDeveloper) accessory.Method.SendChat($"/e [虎のDebug]：阶段成功转换至P2");
    }
    
    [ScriptMethod(name: "P2 阶段转换 - 光之失控", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40212"])]
    public void P2光之失控转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.UsurperOfFrost => FRU_Phase.LightRampant,
        };
        if(isDeveloper) accessory.Method.SendChat($"/e [虎のDebug]：阶段成功转换至光暴");
    }
    
    [ScriptMethod(name: "P3 开场转换", userControl: false, eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40226"])]
    public void P3开场转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.LightRampant => FRU_Phase.OracleOfDarkness,
        };
        if(isDeveloper) accessory.Method.SendChat($"/e [虎のDebug]：阶段成功转换至P3");
    }
    
    [ScriptMethod(name: "P4 开场转换", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40246"])]
    public void P4开场转换(Event @event, ScriptAccessory accessory)
    {
        // 注：P4的 [希瓦·米特隆] DataId为 [17833] 且圣龙出现时会再次生成一个 ，[暗之巫女] 为 [17835]  ； [圣龙] [17834]  [琳的幻影] [17844] [盖娅的幻影] [17845]
        phase = phase switch
        {
            FRU_Phase.OracleOfDarkness => FRU_Phase.Les,
        };
        if(isDeveloper) accessory.Method.SendChat($"/e [虎のDebug]：阶段成功转换至P4");
    }
    
    [ScriptMethod(name: "P4 阶段转换 - 一运 [光与暗的龙诗]", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40301"])]
    public void P4光暗龙诗转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.Les => FRU_Phase.DarklitDragonsong,
        };
        if(isDeveloper) accessory.Method.SendChat($"/e [虎のDebug]：阶段成功转换至P4一运[光与暗的龙诗]");
    }
    
    [ScriptMethod(name: "P4 阶段转换 - 二运 [时间结晶]", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40298"])]
    public void P4时间结晶转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.DarklitDragonsong => FRU_Phase.CrystallizeTime,
        };
        if(isDeveloper) accessory.Method.SendChat($"/e [虎のDebug]：阶段成功转换至P4二运[时间结晶]");
    }
    

    [ScriptMethod(name: "P5 阶段转换", userControl: false, eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17839"])]
    public void P5开场转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.CrystallizeTime => FRU_Phase.Pandora,
        };
        if(isDeveloper) accessory.Method.SendChat($"/e [虎のDebug]：阶段成功转换至P5");
    }

    
    #endregion
    
    #region 通用主体部分
    
    /*
    [ScriptMethod(name: "P2.5 击退前自动吃爆发药", eventType: EventTypeEnum.Chat, eventCondition: ["Type:NPCDialogueAnnouncements", "Message:啊——！" , "Sender:琳"])]
    public async void AutoBottleGemdraught(Event @event, ScriptAccessory accessory)
    {
        if (!isAutoBottleGemdraught) return;
        
        /*
        switch (@event.ItemId())
        {
            case 45995:  // 3级刚力之宝药
                accessory.Method.
                break;
            case 45996:  // 3级巧力之宝药
                accessory.Method.
                break;
            case 45998:  // 3级智力之宝药
                accessory.Method.
                break;
            case 45999:  // 3级意力之宝药
                accessory.Method.
                break;
                
            case 44162:  // 2级刚力之宝药
                accessory.Method.
                break;
            case 44163:  // 2级巧力之宝药
                accessory.Method.
                break;
            case 44165:  // 2级智力之宝药
                accessory.Method.
                break;
            case 44166:  // 2级意力之宝药
                accessory.Method.
                break;
        }
        
        
    }

    */
    
    [ScriptMethod(name: "P2.5 严冬风暴（大圈）判定时间绘制", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40256"])]
    public void 严冬风暴(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"严冬风暴{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 3000;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "严冬风暴销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:40256"],userControl: false)]
    public void 严冬风暴销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"严冬风暴{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "P4 盖娅出现横幅倒计时", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40186"])]
    public async void P4AddGaia(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("盖娅即将出现，注意团辅时机", duration: 5300, true);
        await Task.Delay(3800);
        if (isTTS)accessory.Method.TTS("盖娅即将出现");
        if (isEdgeTTS)accessory.Method.EdgeTTS("盖娅即将出现");
    }
    
    [ScriptMethod(name: "P4 未来的碎片 目标圈绘制", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17841"])]
    public async void FragmentOfFate(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "未来的碎片";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3.5f);
        dp.DestoryAt = 180000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "P4 未来的碎片 绘制销毁", userControl:false, eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:17841"])]
    public async void RemoveFragmentOfFate(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"未来的碎片");
    }
    
    [ScriptMethod(name: "P4 忘却的此岸 AOE提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40174"])]
    public void EdgeOfOblivion(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("水晶AOE", duration: 4300, false);
        if (isTTS)accessory.Method.TTS("水晶AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS("水晶AOE");
    }
    
    [ScriptMethod(name: "P4 死亡轮回 盖娅连线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40302"])]
    public void P4连线盖娅(Event @event, ScriptAccessory accessory)
    {
        if (AkhMorn == AkhMornEnum.盖娅) {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "连线盖娅";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }
    
    [ScriptMethod(name: "P4 死亡轮回 琳连线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40247"])]
    public void P4连线琳(Event @event, ScriptAccessory accessory)
    {
        if (AkhMorn == AkhMornEnum.琳) {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "连线琳";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }
    
    [ScriptMethod(name: "P4 二运Y字击退提醒（与自动防击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40229"])]
    public async void P4YAutoAntiKnockback(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(2500);
        
        if (isAutoAntiKnockback)
        {
            accessory.Method.SendChat($"/ac 亲疏自行");
            accessory.Method.SendChat($"/ac 沉稳咏唱");
            accessory.Method.SendChat($"/e [虎のDebug]：已尝试自动使用防击退");
        }
        
        if (isText)accessory.Method.TextInfo("防击退", duration: 1500, true);
        if (isTTS)accessory.Method.TTS("防击退");
        if (isEdgeTTS)accessory.Method.EdgeTTS("防击退");
    }
    
    [ScriptMethod(name: "P4 最后延后120提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40247"])]
    public void P4Finale(Event @event, ScriptAccessory accessory)
    {
        if (phase != FRU_Phase.CrystallizeTime) return;
        if (isText)accessory.Method.TextInfo("延后120爆发至P5", duration: 5000, true);
        if (isTTS)accessory.Method.TTS("延后爆发");
        if (isEdgeTTS)accessory.Method.EdgeTTS("延后爆发");
    }
    
    #endregion
    
    #region 龙骑妙妙小工具
    
    [ScriptMethod(name: "————龙骑妙妙小工具（此选项并无实际意义）————", eventType: EventTypeEnum.StartCasting, eventCondition: ["DataId:1"])]
    public void 龙骑妙妙小工具(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "P1 开场提示", eventType: EventTypeEnum.Chat, eventCondition: ["Message:距离战斗开始还有5秒！"])]
    public async void FatebreakerStart(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        await Task.Delay(3000);
        if (isText)accessory.Method.TextInfo("1G高跳，2G连祷猛枪", duration: 7000, false);
        if (isTTS)accessory.Method.TTS("1G高跳，2G连祷猛枪");
        if (isEdgeTTS)accessory.Method.EdgeTTS("1G高跳，2G连祷猛枪");
    }
    
    [ScriptMethod(name: "P1 雾龙上天前提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40168"])]
    public void UtopianSkyEarly(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        if (phase != FRU_Phase.Fatebreaker) return;
        if (isText)accessory.Method.TextInfo("提前后跳，强化标枪代替龙4收尾", duration: 10000, false);
        if (isTTS)accessory.Method.TTS("提前后跳，标枪收尾");
        if (isEdgeTTS)accessory.Method.EdgeTTS("提前后跳，标枪收尾");
    }
    
    [ScriptMethod(name: "P1 雾龙落地提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40167"] ,suppress:1000)]
    public void FatebreakerTargetable(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        if (isText)accessory.Method.TextInfo("1G后牵制，2G高跳，延后猛枪蓝炮，打完身位排队", duration: 6000, true);
        if (isTTS)accessory.Method.TTS("1G牵制，延后爆发");
        if (isEdgeTTS)accessory.Method.EdgeTTS("1G牵制，延后爆发");
    }
    
    [ScriptMethod(name: "P2 光之暴走 踩塔前猛枪提示（与自动猛枪）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4022[01]$"])]
    public void P2LanceCharge(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        if (phase != FRU_Phase.LightRampant) return;
        if (isAutoLanceCharge) accessory.Method.SendChat($"/ac 猛枪");
        if (isAutoLanceCharge) accessory.Method.SendChat($"/e [虎のDebug] 龙骑小工具：已尝试自动使用猛枪");
        if (isText)accessory.Method.TextInfo("提前猛枪", duration: 5000, true);
        if (isTTS)accessory.Method.TTS("提前猛枪");
        if (isEdgeTTS)accessory.Method.EdgeTTS("提前猛枪");
    }
    
    [ScriptMethod(name: "P2.5 龙骑时间轴小抄", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17829"])]
    public async void P25DRGTimeLine(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        if (isText)accessory.Method.TextInfo("[1G] 连祷爆发药  [2G] 高跳  [3G] 疾跑猛枪 \n [4G] 龙炎冲龙剑  [5G] 后撤放大圈  [6G] 蓝炮好了单插 \n [7G] 幻象冲+坠星冲 ", duration: 17500, false);
        
        await Task.Delay(8200);
        if (isTTS)accessory.Method.TTS("后撤引导大圈");
        if (isEdgeTTS)accessory.Method.EdgeTTS("后撤引导大圈");
    }
    
    [ScriptMethod(name: "P4 开场提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17841"])]
    public async void P4AddFragmentOfFate(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        if (isText)accessory.Method.TextInfo("躲完天光轮回1G后，爆发药+猛枪 \n 盖娅出现开团辅", duration: 6000, false);
        if (isTTS)accessory.Method.TTS("注意爆发时机");
        if (isEdgeTTS)accessory.Method.EdgeTTS("注意爆发时机");
    }
    
    [ScriptMethod(name: "P4 天光轮回猛枪提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40237"])]
    public async void P4AkhRhai(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        if (isTTS)accessory.Method.TTS("爆发药加猛枪");
        if (isEdgeTTS)accessory.Method.EdgeTTS("爆发药加猛枪");
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