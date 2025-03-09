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
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Thornmarch_Extreme;

[ScriptType(guid: "fc6a6125-4a1d-4669-be4c-9b375dc70ae0", name: "莫古力贤王歼殛战", territorys: [364],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Thornmarch_Extreme
{
    const string noteStr =
        """
        v0.0.0.1:
        LV50 莫古力贤王歼殛战 初版绘制
        """;
    
    // Todo: 判断 莫古联合 的add目标以提前判断组合攻击类型 ， 可以靠 NpcYell 判断
    // 缺少机制：千库啵横扫 、 莫古助威歌
    
    #region 副本提示
    // BOSS复活技能 位高任重 计数
    private volatile int timeMooglesseOblige=0;
    public void Init(ScriptAccessory accessory) {
        timeMooglesseOblige=0;
    }
    
    [ScriptMethod(name: "开场提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:236"])]
    public void 开场提示(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("难度：☆\n重点机制：修小怪血，并一起击杀 ", duration: 5000, true);
        accessory.Method.TTS("出啥躲啥，注意修血");
        accessory.Method.SendChat("/e T：MT拉斧 & 贤王，ST拉壁，注意顺劈面向\nD：注意修血，前两次需要同时击杀\nH：组合技时注意驱散[莫古乱乱乱]与[怒发冲冠]、注意AOE");
    }
    
    [ScriptMethod(name: "小怪出现提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2070"])]
    public void 小怪出现提示(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("MT拉 < 斧 > & < 贤王 > 、ST拉 < 壁 >，都有顺劈\n全部小怪需要同时击杀两次", duration: 5000, true);
        accessory.Method.TTS("注意面向，注意修血");
        accessory.Method.SendChat("/e 拉怪提示：MT拉斧&贤王，ST拉壁，都有顺劈\n全部小怪需要同时击杀两次，随后AOE并狂暴");
    }
    
    [ScriptMethod(name:"位高任重计数重置",userControl:false,eventType:EventTypeEnum.Chat,
        eventCondition:["Type:NPCDialogueAnnouncements","Message:regex:^家臣们，\n到你们出马的时候了库啵！","Sender:贤王莫古尔·莫古十二世"])]
    public void Reset_MooglesseOblige(Event @event, ScriptAccessory accessory) {
        timeMooglesseOblige=0;
    }
    
    [ScriptMethod(name: "位高任重 复活提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2069"])]
    public void 位高任重(Event @event, ScriptAccessory accessory)
    {
        ++timeMooglesseOblige;
        switch (timeMooglesseOblige)
        {
            case 1:
            {
                accessory.Method.TextInfo($"第{timeMooglesseOblige}次复活", duration: 2700, false);
                accessory.Method.TTS($"第{timeMooglesseOblige}次复活");
                accessory.Method.SendChat($"/e 已复活次数：{timeMooglesseOblige}");
            }
                break;
            case 2:
            {
                accessory.Method.TextInfo($"第{timeMooglesseOblige}次复活,即将超大AOE\n然后打死一只小怪后读条狂暴", duration: 2700, false);
                accessory.Method.TTS($"第{timeMooglesseOblige}次复活，即将超大AOE");
                accessory.Method.SendChat($"/e 已复活次数：{timeMooglesseOblige}");
            }
                break;
            default:
            {
                accessory.Method.SendChat($"/e 计数错误！当前计数：{timeMooglesseOblige}");
            }
                break;
        }
    }
    #endregion
    
    #region 小怪基础机制
    [ScriptMethod(name: "毛毛之斧_莫古之暗（顺劈）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2052"])]
    public void 莫古之暗(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "莫古之暗";
        dp.Color = new Vector4(1f, 0f, 1f, 0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6.9f);
        dp.Radian = 120f.DegToRad();
        dp.Delay = 8000;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "绒绒之壁_莫古之光（顺劈）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2054"])]
    public void 莫古之光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "莫古之光";
        dp.Color = new Vector4(1f, 1f, 1f, 0.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6.9f);
        dp.Radian = 120f.DegToRad();
        dp.Delay = 8000;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "绵绵之音_莫古力进行曲（增益圈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1623"])]
    public void 莫古力进行曲(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("将其它莫古拉出增益圈", duration: 5200, false);
        accessory.Method.TTS("将其它莫古拉出增益圈");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "莫古力进行曲";
        dp.Color = new Vector4(0f, 1f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "莫古神准箭 点名提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:1635"])]
    public void 莫古神准箭(Event @event, ScriptAccessory accessory)
    {
        string tname = @event["TargetName"]?.ToString() ?? "未知目标";

        if (@event.TargetId() == accessory.Data.Me)
        {
            accessory.Method.TextInfo("四连突刺点名", duration: 3700, true);
            accessory.Method.TTS("四连突刺点名");
            accessory.Method.SendChat("四连突刺点名");
        } else
        {
            accessory.Method.TextInfo($"四连攻击点 <{tname}>", duration: 3700, false);
            accessory.Method.TTS($"四连攻击点{tname}");
            accessory.Method.SendChat($"/e 四连攻击点<{@event.TargetName()}>");
        }
    }
    #endregion
    
    #region 组合技阶段
    //【组合技：柔柔之力 + 茸茸之愈】
    [ScriptMethod(name: "绒绒陨石（圆形AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2056"])]
    public void 绒绒陨石(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "绒绒陨石";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    //【组合技：柔柔之力 + 茸茸之愈 + 毛毛之斧】
    //莫古的诅咒：柔柔之力 对自身周围角色附加60s莫古乱乱乱，使受到 全屏AOE [莫古飞天乱] 的攻击伤害大幅提高。
    //绒绒神圣：茸茸之愈 开始咏唱，咏唱完毕时对全场范围造成高伤害无属性魔法伤害。茸茸之愈 受到一定量伤害后停止咏唱。
    [ScriptMethod(name: "莫古乱乱乱 驱散提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:473"])]
    public void 莫古乱乱乱(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("驱散 <莫古乱乱乱> ", duration: 5000, true);
        accessory.Method.TTS("驱散《莫古乱乱乱》");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"莫古乱乱乱{@event.TargetId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1f);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "莫古乱乱乱 驱散销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:473"],userControl: false)]
    public void 莫古乱乱乱驱散销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"莫古乱乱乱{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "茸茸之愈_绒绒神圣 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2059"])]
    public void 绒绒神圣(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("攻击 < 茸茸之愈 > 以打断 < 绒绒神圣 > ", duration: 3000, true);
        accessory.Method.TTS("攻击《茸茸之愈》打断读条");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "绒绒神圣";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "绒绒神圣 打断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:2059"],userControl: false)]
    public void 绒绒神圣销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("绒绒神圣");
        accessory.Method.SendChat("/e 绒绒神圣 已打断");
    }
    
    //【组合技：蓬蓬之障 + 绒绒之壁 + 软软之弓】
    //绒绒沼泽：蓬蓬之障 在场上召唤绿色毒圈，角色进入会赋予 猛毒。
    //放马过来库啵！：绒绒之壁 对自身周围最近的3名角色连线，同时接近蓬蓬之障。稍后为角色附加18s怒发冲冠，[怒发冲冠]debuff中的玩家会被挑衅，在毒池里持续受到伤害。
    //莫古死亡雨：软软之弓 随机指定任意3名角色为目标，为其附加3把小弓箭组成的褐色圆形 “莫古力剑雨目标” 标记，随后对角色所在位置造成圆形范围无属性突刺伤害。
    
    [ScriptMethod(name: "放马过来库啵 远离提示", eventType: EventTypeEnum.Tether, eventCondition: ["Id:000D"])]
    public void 放马过来库啵(Event @event, ScriptAccessory accessory)
    {
        if ( @event.SourceId() != accessory.Data.Me) return;
        accessory.Method.TextInfo("尽量远离 <蓬蓬之障> 与 <绒绒之壁>", duration: 5000, true);
        accessory.Method.TTS("远离连线目标");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "蓬蓬之障连线";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = accessory.Data.Me;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1);
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "怒发冲冠 驱散提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:402"])]
    public void 怒发冲冠(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("驱散 <怒发冲冠> ", duration: 5000, true);
        accessory.Method.TTS("驱散《怒发冲灌》");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"怒发冲冠{@event.TargetId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1f);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        accessory.Method.RemoveDraw("蓬蓬之障连线");
    }
    
    [ScriptMethod(name: "怒发冲冠 驱散销毁", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:402"],userControl: false)]
    public void 怒发冲冠驱散销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"怒发冲冠{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "莫古死亡雨（点名圆形）", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:1636"])]
    public void 莫古死亡雨(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "莫古死亡雨";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    #endregion
    
    #region 狂暴阶段
    [ScriptMethod(name: "莫古陨石（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2072","SourceDataId:236"])]
    public void 莫古陨石(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("大AOE伤害 ", duration: 4700, true);
        accessory.Method.TTS("大AOE伤害");
    }
    
    [ScriptMethod(name: "死亡莫古警告 狂暴提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2121"])]
    public void 死亡莫古警告(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("攻击贤王，狂暴", duration: 5000, true);
        accessory.Method.TTS("集中攻击贤王");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "死亡莫古警告";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 1500;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
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
