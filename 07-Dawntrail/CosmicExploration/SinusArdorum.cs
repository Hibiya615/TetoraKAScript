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

namespace SinusArdorum;

[ScriptType(guid: "8f9e094e-2aa2-4dfc-9020-ac9dac53e525", name: "宇宙探索小工具", territorys: [1237],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class SinusArdorum
{
    const string noteStr =
        """
        v0.0.0.1:
        宇宙探索小工具
        """;
    
    #region 基础控制
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("紧急事件指路")]
    public bool isWarning { get; set; } = true;
    
    [UserSetting("机甲行动协助员技能绘制")]
    public bool isAssistant { get; set; } = true;
    
    #endregion
    
    #region 变量控制
    
    uint Spore=0; // 孢子雾
    uint AstromagneticStorm=0; // 磁暴
    uint Meteor = 0; // 陨石
    
    uint MushroomMoldbeds = 0; // 有害菌床驱除指令
    uint CorruptedCrystals = 0; // 巨型偏属性水晶破坏指令
    public void Init(ScriptAccessory accessory) {
        Spore = 0;
        AstromagneticStorm = 0;
        Meteor = 0;
        
        MushroomMoldbeds = 0;
        CorruptedCrystals = 0;
    }
    
    [ScriptMethod(name: "紧急事件变量销毁", userControl: false, eventType: EventTypeEnum.Chat,
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^(感谢各位的倾力贡献|在各位的帮助下).+"])]
    public void 紧急事件变量销毁(Event @event, ScriptAccessory accessory)
    {
       /*
          感谢各位的倾力贡献，修理用的物资已经集齐了！
          各位如果还有余力，可以继续筹集储备物资，以供不时之需。 
          
          在各位的帮助下，基地已经恢复了正常秩序！
          我宣布解除紧急状态！感谢各位的鼎力相助！
        */
       
        Spore=0;
        AstromagneticStorm=0;
        Meteor = 0;
    }
    
    [ScriptMethod(name: "机甲行动变量销毁", userControl: false, eventType: EventTypeEnum.Chat,
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^(行动成功|感谢各位驾驶员和协助员提供的帮助).+"])]
    public void 机甲行动变量销毁(Event @event, ScriptAccessory accessory)
    {
        /*
            // 有害菌床驱除指令
            [NPCDialogueAnnouncements] 行动成功！眼下这片区域的安全有了保障！
            感谢各位驾驶员和协助员提供的帮助！
            
            // 巨型偏属性水晶破坏指令
            [NPCDialogueAnnouncements] 行动成功！我们的计划前进了一大步！
            感谢各位驾驶员和协助员提供的帮助！
         */
       
        MushroomMoldbeds = 0;
        CorruptedCrystals = 0;
    }
    #endregion
    
    #region 紧急事件判断

    [ScriptMethod(name: "紧急事件判断_孢子雾", userControl: false, eventType: EventTypeEnum.Chat,
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^观测到孢子雾爆发的预兆.+"])]
    public void 紧急事件判断_孢子雾(Event @event, ScriptAccessory accessory)
    {
        
     /* 观测到孢子雾爆发的预兆！能见度将会受到影响，
     出于安全考虑，请工作人员们做好准备，依据情况停工。
     
     孢子雾α：观测到有可能会出现孢子雾！得防止它进一步扩散……要是这些孢子持续繁殖，可就麻烦了……！
     孢子雾β：观测到有可能会出现孢子雾！感觉可见度变差了。必须要保持冷静！
     */
        Spore=1;
    
    }

    [ScriptMethod(name: "紧急事件判断_磁暴", userControl: false, eventType: EventTypeEnum.Chat,
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^即将发生大规模磁暴.+"])]
    public void 紧急事件判断_磁暴(Event @event, ScriptAccessory accessory)
    {
        
    /* 即将发生大规模磁暴……
    所有机械设备都可能会受到影响！请大家提高警惕！

    磁暴α：预测到大规模磁暴！精密设备可能会发生故障……必须做好最坏的打算！
    磁暴β：预测到大规模磁暴！我担心会影响到物流……讨论一下对策吧。
     */
        AstromagneticStorm=1;
    
    }


    [ScriptMethod(name: "紧急事件判断_陨石", userControl: false, eventType: EventTypeEnum.Chat,
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^观测到小型陨石群正在靠近.+"])]
    public void 紧急事件判断_陨石(Event @event, ScriptAccessory accessory)
    {
        
    /* 观测到小型陨石群正在靠近！
    陨石落点可能在憧憬湾，所有人提高警惕谨防陨石灾害！

    小陨石群α：感知到小陨石群正在靠近！可能会对设施和器材造成损害……要保护好重要器材。
    小陨石群β：感知到小陨石群正在靠近！不知对地表的冲击会造成什么影响……见招拆招吧！
     */
    
        Meteor = 1;
    
    }
    
    #endregion
    
    #region 机甲行动判断

    [ScriptMethod(name: "机甲行动判断_有害菌床驱除指令", userControl: false, eventType: EventTypeEnum.Chat,
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:.*请使用宇宙钻头粉碎小型偏属性水晶.*"])]
    public void 机甲行动判断_有害菌床驱除指令(Event @event, ScriptAccessory accessory)
    {
        MushroomMoldbeds = 1;
    }

    [ScriptMethod(name: "机甲行动判断_巨型偏属性水晶破坏指令", userControl: false, eventType: EventTypeEnum.Chat,
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:.*请各位协助者善用宇宙火焰喷射器.*"])]
    public void 机甲行动判断_巨型偏属性水晶破坏指令(Event @event, ScriptAccessory accessory)
    {
        CorruptedCrystals = 1;
    }
    
    #endregion
    
    #region 机甲行动协助员绘制
    
    [ScriptMethod(name: "机甲行动协助绘制 - 宇宙火焰喷射器", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4230"])]
    public void 宇宙火焰喷射器(Event @event, ScriptAccessory accessory)
    {
        // ActionId：42258
        if (isAssistant && MushroomMoldbeds == 1) {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "协助绘制宇宙火焰喷射器";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(7f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 1200000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
        }
    }
    
    [ScriptMethod(name: "机甲行动协助绘制 - 宇宙钻头", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4230"])]
    public void 宇宙钻头(Event @event, ScriptAccessory accessory)
    {
        // ActionId：42150
        if (isAssistant && CorruptedCrystals == 0) {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "协助绘制宇宙钻头";
        dp.Scale = new (5, 7f);
        dp.Owner = @event.TargetId();
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = 120000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
        }
    }
    #endregion
    
    #region 销毁绘制

    [ScriptMethod(name: "紧急事件指路销毁", userControl: false, eventType: EventTypeEnum.Chat,
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^(.*?(?:爆发孢子雾|磁暴造成|陨石雨坠落).*?)\n现在宣布进入紧急状态！(.*)$"])]
    public void 紧急事件指路销毁(Event @event, ScriptAccessory accessory)
    {

        /*
         憧憬湾东部爆发孢子雾！
         现在宣布进入紧急状态！请尽快查明是否出现变异菌床！
         
         磁暴α：
         磁暴造成憧憬湾多个地区受灾！
         现在宣布进入紧急状态！请尽快对受灾地区展开救灾活动！
         
         月门基地北部及南部滑石坑附近发生紧急事件！
         急需人手筹集物资，请各位工作人员尽快赶往事发地点！
         
         磁暴β：
         已确认磁暴造成恶劣影响，现在宣布进入紧急状态！
         所有人注意，请收集救灾所需的物资，尽快恢复基地工作秩序！
         
         有小型陨石雨坠落在月门基地附近！
         现在宣布进入紧急状态！请尽快查明受灾情况，恢复生产建设秩序！

         */
    
    }
    
    [ScriptMethod(name: "机甲行动协助员销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4230"],userControl: false)]
    public void 机甲行动协助员销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("协助绘制.+");
        
        /*  由于在机甲行动中担当协助员，接下来数次探索任务能获得的信用点报酬提升。
            完成了机甲行动“机甲行动”！  */
    }
    
    #endregion
    
    #region Debug / 文本备份
    
    /*
     [Type] NPCDialogue  [Message]憧憬湾 (人数)  [Sender] Story Selection
     
     [SystemMessage] 当前所在副本区为“憧憬湾”。
                     使用“/副本区检查”指令可以随时查看当前所在副本区。
                     
     [???] 机甲行动已发令。
           想成为驾驶员的话请先进行申请。
                     
     [SystemMessage] 附近开始了新的机甲行动！
     
     [SystemMessage] 参加了机甲行动！
                     此机甲行动将使用特殊热键栏上的专用技能。
                     
     [ErrorMessage] 此机甲行动将使用特殊热键栏上的专用技能。
                    参加时请按下任务情报的“加入行动”按键。
                    
     [NPCDialogueAnnouncements] 请把菌床烧成的灰交纳至野外探查器处！
                                协助轮式工程车完成行动！
     
     [NPCDialogueAnnouncements] 请把粉碎的偏属性水晶碎片交纳至野外探查器处！
                                协助强力机兵完成行动！
     
     胡萝卜授权 StatusID:4205
                    
     */
    
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