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
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.MathHelpers;
using System.Threading.Tasks;

namespace Thornmarch_Hard;

[ScriptType(guid: "cd81e178-12e6-4e53-9b81-63002cc51ecb", name: "莫古力贤王歼灭战(?)", territorys: [1067],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class Thornmarch_Hard
{
    const string noteStr =
        """
        v0.0.0.2:
        LV50 莫古力贤王歼灭战
        纯整活无意义，不喜欢可以不用
        台词暂时适用于CN版，暂未适配其他语言端
        """;
    
    [UserSetting("台词TTS开关")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("台词文本弹窗开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("机制文本弹窗开关")]
    public bool isText2 { get; set; } = true;
    
    [UserSetting("打印气泡台词至聊天框")]
    public bool isSendMessage { get; set; } = true;
    
    [UserSetting("机制填充动画开关")]
    public bool isFill { get; set; } = true;
    
    #region 台词部分
    
    [ScriptMethod(name: "反抗莫古力家臣团的人，会被莫古们彻底击溃库啵！", userControl: false,eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements","Message:反抗莫古力家臣团的人\n会被莫古们彻底击溃库啵！"])]
    public void 会被莫古们彻底击溃库啵(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("反抗莫古力家臣团的人，会被莫古们彻底击溃库啵！", duration: 4000, true);
        if (isTTS)accessory.Method.EdgeTTS("反抗莫古力家臣团的人,会被莫古们彻底击溃,库啵！");
    }
    
    [ScriptMethod(name: "尝尝莫古的斧头库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1950"])]
    public void 尝尝莫古的斧头库啵(Event @event, ScriptAccessory accessory)
    {
        //if (isTTS)accessory.Method.EdgeTTS("尝尝莫古的斧头库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 毛毛之斧 库普塔·咔帕：尝尝莫古的斧头库啵！");
    }
    
    [ScriptMethod(name: "看莫古射你屁股库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1960"])]
    public void 看莫古射你屁股库啵(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("看莫古射你屁股库啵！", duration: 4000, true);
        if (isTTS)accessory.Method.EdgeTTS("看莫古射你屁股库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 软软之弓 库普括·叩吉：看莫古射你屁股库啵！");
    }
    
    [ScriptMethod(name: "居然输了库啵……", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1947"])]
    public void 居然输了库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("居然输了库啵……");
        if (isSendMessage)accessory.Method.SendChat("/e 绒绒之壁 库普迪·库普：居然输了库啵……");
    }
    
    [ScriptMethod(name: "被打倒了库啵……", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1952"])]
    public void 被打倒了库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("被打倒了库啵……");
        if (isSendMessage)accessory.Method.SendChat("/e 毛毛之斧 库普塔·咔帕：被打倒了库啵……");
    }
    
    [ScriptMethod(name: "被干掉了库啵……", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1962"])]
    public void 被干掉了库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("被干掉了库啵……");
        if (isSendMessage)accessory.Method.SendChat("/e 软软之弓 库普括·叩吉：被干掉了库啵……");
    }
    
    [ScriptMethod(name: "想被穿成肉串吗库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1955"])]
    public void 想被穿成肉串吗库啵(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("想被穿成肉串吗库啵！", duration: 4000, true);
        if (isTTS)accessory.Method.EdgeTTS("想被穿成肉串吗库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 茸茸之愈 库普洛·奇普：想被穿成肉串吗库啵！");
    }
    
    [ScriptMethod(name: "来跟莫古一起跳库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1573"])]
    public void 来跟莫古一起跳库啵(Event @event, ScriptAccessory accessory)
    {
        //if (isTTS)accessory.Method.EdgeTTS("来跟莫古一起跳库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 绵绵之音 普库嘻·皮叩：来跟莫古一起跳库啵！");
    }
    
    [ScriptMethod(name: "莫古输了库啵……", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1957"])]
    public void 莫古输了库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("莫古输了库啵……");
        if (isSendMessage)accessory.Method.SendChat("/e 茸茸之愈 库普洛·奇普：莫古输了库啵……");
    }
    
    [ScriptMethod(name: "已经没法唱歌了库啵……", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1975"])]
    public void 已经没法唱歌了库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("已经没法唱歌了库啵……");
        if (isSendMessage)accessory.Method.SendChat("/e 绵绵之音 普库嘻·皮叩：已经没法唱歌了库啵……");
    }
    
    [ScriptMethod(name: "玩火好有趣啊库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1965"])]
    public void 玩火好有趣啊库啵(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("玩火好有趣啊库啵！", duration: 4000, true);
        if (isTTS)accessory.Method.EdgeTTS("玩火好有趣啊库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 柔柔之力 普库啦·普奇：玩火好有趣啊库啵！");
    }
    
    [ScriptMethod(name: "真、真的要打吗库啵？", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1978"])]
    public void 真的要打吗库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("真、真的要打吗库啵？");
        if (isSendMessage)accessory.Method.SendChat("/e 蓬蓬之障 普库呐·帕叩：真、真的要打吗库啵？");
    }
    
    [ScriptMethod(name: "将力量集中到绒绒陨石上，把你们砸烂库啵！", userControl: false,eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements","Message:将力量集中到绒绒陨石上，\n把你们砸烂库啵！"])]
    public void 会把你们砸烂库啵(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("将力量集中到绒绒陨石上，把你们砸烂库啵！", duration: 4000, true);
        if (isTTS)accessory.Method.EdgeTTS("将力量集中到绒绒陨石上，把你们砸烂库啵！");
    }
    
    [ScriptMethod(name: "莫古已经燃尽了库啵……", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1967"])]
    public void 莫古已经燃尽了库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("莫古已经燃尽了库啵……");
        if (isSendMessage)accessory.Method.SendChat("/e 柔柔之力 普库啦·普奇：莫古已经燃尽了库啵……");
    }
    
    [ScriptMethod(name: "好可怕库啵……", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1980"])]
    public void 好可怕库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("好可怕库啵……");
        if (isSendMessage)accessory.Method.SendChat("/e 蓬蓬之障 普库呐·帕叩：好可怕库啵……");
    }
    
    [ScriptMethod(name: "挺、挺厉害的嘛库啵……但贤王大人会收拾你们的库啵！", userControl: false,eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements","Message:挺、挺厉害的嘛库啵……\n但贤王大人会收拾你们的库啵！"])]
    public void 贤王大人会收拾你们的库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("挺、挺厉害的嘛库啵……但贤王大人会收拾你们的库啵！");
    }
    
    [ScriptMethod(name: "贤王大人啊，请降下您的制裁库啵！", userControl: false,eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements","Message:贤王大人啊，\n请降下您的制裁库啵！"])]
    public void 请降下您的制裁库啵(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("贤王大人啊，请降下您的制裁库啵！", duration: 4000, true);
        if (isTTS)accessory.Method.EdgeTTS("贤王大人啊，请降下您的制裁库啵！");
    }
    
    [ScriptMethod(name: "贤王大人啊……后半句是啥来着库啵？", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1982"])]
    public void 后半句是啥来着库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("贤王大人啊……后半句是啥来着库啵？");
        if (isSendMessage)accessory.Method.SendChat("/e 蓬蓬之障 普库呐·帕叩：贤王大人啊……后半句是啥来着库啵？");
    }
    
    [ScriptMethod(name: "死亡莫古警告！你们会为忤逆行为付出代价库啵！", userControl: false,eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements","Message:死亡莫古警告！\n你们会为忤逆行为付出代价库啵！"])]
    public void 你们会为忤逆行为付出代价库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("死亡莫古警告！你们会为忤逆行为付出代价库啵！");
    }
    
    [ScriptMethod(name: "莫古们的战斗才刚刚开始库啵！", userControl: false,eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements","Message:莫古们的战斗才刚刚开始库啵！"])]
    public void 莫古们的战斗才刚刚开始库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("莫古们的战斗才刚刚开始库啵！");
    }
    
    [ScriptMethod(name: "家臣们，到你们出马的时候了库啵！", userControl: false,eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements","Message:家臣们，\n到你们出马的时候了库啵！"])]
    public void 到你们出马的时候了库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("家臣们，到你们出马的时候了库啵！");
    }
    
    [ScriptMethod(name: "让、让你们见识一下莫古真正的实力库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13788"])]
    public void 让你们见识一下莫古真正的实力库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("让、让你们见识一下莫古真正的实力库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 蓬蓬之障 普库呐·帕叩：让、让你们见识一下莫古真正的实力库啵！");
    }
    
    [ScriptMethod(name: "尝尝纷乱箭库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1963"])]
    public void 尝尝纷乱箭库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("尝尝纷乱箭库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 软软之弓 库普括·叩吉：尝尝纷乱箭库啵！");
    }
    
    [ScriptMethod(name: "沉到毒沼里去吧库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1981"])]
    public void 沉到毒沼里去吧库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("沉到毒沼里去吧库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 蓬蓬之障 普库呐·帕叩：沉到毒沼里去吧库啵！");
    }
    
    [ScriptMethod(name: "家臣们，给坏人以制裁库啵！", userControl: false,eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements","Message:家臣们，\n给坏人以制裁库啵！"])]
    public void 给坏人以制裁库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("家臣们，给坏人以制裁库啵！");
    }
    
    [ScriptMethod(name: "要有自知之明库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13781"])]
    public void 要有自知之明库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("要有自知之明库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 贤王莫古尔·莫古十二世：要有自知之明库啵！");
    }
    
    [ScriptMethod(name: "把你们打散架库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13783"])]
    public void 把你们打散架库啵(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("把你们打散架库啵！\n打得你们屁滚尿流库啵！ ", duration: 8700, true);
        if (isTTS)accessory.Method.EdgeTTS("把你们打散架库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 毛毛之斧 库普塔·咔帕：把你们打散架库啵！");
    }
    
    [ScriptMethod(name: "打得你们屁滚尿流库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13782"])]
    public void 打得你们屁滚尿流库啵(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("把你们打散架库啵！\n打得你们屁滚尿流库啵！ ", duration: 8700, true);
        if (isTTS)accessory.Method.EdgeTTS("打得你们屁滚尿流库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 绒绒之壁 库普迪·库普：打得你们屁滚尿流库啵！");
    }
    
    [ScriptMethod(name: "听莫古一曲库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13787"])]
    public void 听莫古一曲库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("听莫古一曲库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 绵绵之音 普库嘻·皮叩：听莫古一曲库啵！");
    }
    
    [ScriptMethod(name: "家臣们，联起手来，对抗敌人库啵！", userControl: false,eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements","Message:家臣们，\n联起手来，对抗敌人库啵！"])]
    public void 对抗敌人库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("家臣们，联起手来，对抗敌人库啵！");
    }
    
    [ScriptMethod(name: "莫古的魔力在熊熊燃烧库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13789"])]
    public void 莫古的魔力在熊熊燃烧库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("莫古的魔力在熊熊燃烧库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 柔柔之力 普库啦·普奇：莫古的魔力在熊熊燃烧库啵");
    }
    
    [ScriptMethod(name: "碾碎你们库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13784"])]
    public void 碾碎你们库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("碾碎你们库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 贤王莫古尔·莫古十二世：碾碎你们库啵！");
    }
    
    [ScriptMethod(name: "吃莫古一击库啵！", userControl: false,eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13785"])]
    public void 吃莫古一击库啵(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("吃莫古一击库啵！");
        if (isSendMessage)accessory.Method.SendChat("/e 茸茸之愈 库普洛·奇普：吃莫古一击库啵！");
    }
    #endregion
    
    #region 机制部分
    /*  每次贤王号令都是不同的 Action id 不用计数了
    private volatile int timeDecree=0;
    public void Init(ScriptAccessory accessory) {
      timeDecree=0;
    }
    */
    
    [ScriptMethod(name: "回转莫古盾（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29216"])]
    public void 回转莫古盾(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "回转莫古盾";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        if (isFill)dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "回转莫古盾销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:29216"],userControl: false)]
    public void 回转莫古盾销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("回转莫古盾");
    }
    
    [ScriptMethod(name: "莫古死亡雨（分散）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29191"])]
    public void 莫古死亡雨(Event @event, ScriptAccessory accessory)
    {
        if (isText2)accessory.Method.TextInfo("分散站位", duration: 2500, true);
        accessory.Method.EdgeTTS("分散站位");
    }
    
    [ScriptMethod(name: "百库啵横扫（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29215"])]
    public void 百库啵横扫(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "百库啵横扫";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 4700;
        if (isFill)dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "百库啵横扫销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:29215"],userControl: false)]
    public void 百库啵横扫销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("百库啵横扫");
    }

    [ScriptMethod(name: "绒绒神圣（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2921[01]$"])]
    public void 绒绒神圣(Event @event, ScriptAccessory accessory)
    {
       accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "莫古狂夜曲（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29213"])]
    public void 莫古狂夜曲(Event @event, ScriptAccessory accessory)
    {
        // 命中后附加 11s [3080] 狂欢 debuff
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "莫古狂夜曲";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 4700;
        if (isFill)dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "狂欢", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3080"],userControl: false)]
    public void 狂欢(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.TextInfo("黄圈出来你不躲？", duration: 11000, true);
        accessory.Method.EdgeTTS("黄圈出来你不躲？");
    }
    
    [ScriptMethod(name: "莫古狂夜曲销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:29213"],userControl: false)]
    public void 莫古狂夜曲销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("莫古狂夜曲");
    }
    
    [ScriptMethod(name: "绒绒陨石（踩塔）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29193"])]
    public void 绒绒陨石(Event @event, ScriptAccessory accessory)
    {
        // if (isText2)accessory.Method.TextInfo("踩塔", duration: 2700, true);
        accessory.Method.EdgeTTS("踩塔");
    }
    
    [ScriptMethod(name: "莫古刺（死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29214"])]
    public void 莫古刺(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS($"死刑点{@event.TargetName()}");
    }
    
    [ScriptMethod(name: "死亡莫古警告（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29217"])]
    public void 死亡莫古警告(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "贤王号令", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2918[89]|29190)$"])]
    public void 贤王号令(Event @event, ScriptAccessory accessory)
    {
        switch (@event.ActionId())
        {
            case 29188:
            {
                if (isText2)accessory.Method.TextInfo("一运：远离毒圈，先集合 > 再分散 ，最后AOE", duration: 3700, true);
            }
                break;
            case 29189:
            {
                if (isText2)accessory.Method.TextInfo("二运：三穿一大圈，远离顺劈，分摊死刑后AOE", duration: 3700, true);
            }
                break;
            case 29190:
            {
                if (isText2)accessory.Method.TextInfo("三运：先躲避三连黄圈 后踩塔，先集合分摊，再躲钢铁月环，最后AOE接死刑", duration: 3700, true);
            }
                break;
        }
    }
    
    [ScriptMethod(name: "绒绒沼泽（毒圈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29207"])]
    public void 绒绒沼泽(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("躲避毒池AOE");
        // 命中后附加 3082 猛毒
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "绒绒沼泽";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5000;
        if (isFill)dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "猛毒", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3082"],userControl: false)]
    public void 猛毒(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.TextInfo("让你别吃你还吃？", duration: 3000, true);
        accessory.Method.EdgeTTS("让你别吃你还吃？");
    }
    
    [ScriptMethod(name: "莫古崩石（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29204"])]
    public void 莫古崩石(Event @event, ScriptAccessory accessory)
    {
        if (isText2)accessory.Method.TextInfo($"靠近{@event.TargetName()}分摊", duration: 5300, true);
        accessory.Method.EdgeTTS($"靠近{@event.TargetName()}分摊");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "莫古崩石";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 6000;
        if (isFill)dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "莫古飞天乱（大圈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2919[67]$"])]
    public void 莫古飞天乱(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "莫古飞天乱";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = 8700;
        if (isFill)dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "绒绒双重陨石（分摊死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29206"])]
    public void 绒绒双重陨石(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS($"分摊死刑点{@event.TargetName()}");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "绒绒双重陨石";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        if (isFill)dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "莫古彗星（引导黄圈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29198"])]
    public void 莫古彗星提示(Event @event, ScriptAccessory accessory)
    {
        if (isText2)accessory.Method.TextInfo("引导三连黄圈", duration: 7700, true);
        accessory.Method.EdgeTTS("引导三连黄圈");
    }
    
    [ScriptMethod(name: "莫古彗星（三连黄圈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29199"])]
    public void 莫古彗星(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "莫古彗星";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        if (isFill)dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    
    [ScriptMethod(name: "绒绒垒石（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29619"])]
    public void 绒绒垒石月环(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "绒绒垒石月环";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(20f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    /*
    [ScriptMethod(name: "绒绒垒石（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(29202|29619)$"])]
    public void 绒绒垒石月环(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "绒绒垒石月环";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = @event.ActionId() == 29202 ? new Vector2(20f) : new Vector2(30f);
        dp.InnerScale = @event.ActionId() == 29202 ? new Vector2(10f) : new Vector2(20f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        // if (isFill)dp.ScaleMode = ScaleMode.ByTime;
        // 月环填充怎么是从中间扩散一个月环出去的啊好丑啊！
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    */
    
    [ScriptMethod(name: "绒绒垒石（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29201"])]
    public void 绒绒垒石钢铁(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "绒绒垒石钢铁";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 4700;
        if (isFill)dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "森罗贤王（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29208"])]
    public void 森罗贤王(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "森罗贤王";
        dp.Scale = new (10, 50f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "千库啵冲锋（死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29209"])]
    public void 千库啵冲锋(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS($"死刑点{@event.TargetName()}");
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
