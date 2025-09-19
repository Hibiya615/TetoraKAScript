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
using System.Threading.Tasks;
using KodakkuAssist.Extensions;

namespace Thornmarch_Extreme;

[ScriptType(guid: "fc6a6125-4a1d-4669-be4c-9b375dc70ae0", name: "莫古力贤王歼殛战", territorys: [364],
    version: "0.0.0.4", author: "Tetora", note: noteStr)]

public class ThornmarchExtreme
{
    const string noteStr =
        """
        v0.0.0.3:
        LV50 莫古力贤王歼殛战 初版绘制
        不看攻略基本能打，没有做职能限制，不需要的提示自行关闭
        """;
    
    // 缺少机制：莫古助威歌 
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("【开发用】Debug模式")]
    public bool isDebug { get; set; } = false;

    public static bool isTank;
    public static bool isDps;
    public static bool isHealer;
   
    public void Init(ScriptAccessory accessory)
    {
        var player = accessory.Data.MyObject;
        isTank = player?.IsTank() ?? false;
        isDps = player?.IsDps() ?? false;
        isHealer = player?.IsHealer() ?? false;
    }
    
    // BOSS复活技能 位高任重 计数
    private volatile int timeMooglesseOblige=0;
    public void 位高任重(ScriptAccessory accessory) {
        timeMooglesseOblige=0;
    }
    
    #region 副本提示
    
    [ScriptMethod(name: "开场提示", eventType: EventTypeEnum.Director, eventCondition: ["Command:40000001"])]
    public async void 开场提示(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        var isDps = accessory.Data.MyObject?.IsDps() ?? false;
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;

        if (isTank && isText)accessory.Method.TextInfo("难度：☆，重点：修小怪血，并一起击杀\nT：贤王吸蓝，不推荐DK拉，注意面向 ", duration: 5000, true);
        if (isDps && isText)accessory.Method.TextInfo("难度：☆，重点：修小怪血，并一起击杀\nD：注意修血，前两次需要同时击杀 ", duration: 5000, true);
        if (isHealer && isText)accessory.Method.TextInfo("难度：☆，重点：修小怪血，并一起击杀\nH：组合技时注意驱散[莫古乱乱乱]与[怒发冲冠]、注意AOE ", duration: 5000, true);
        // if (isText)accessory.Method.TextInfo("难度：☆\n重点机制：修小怪血，并一起击杀 ", duration: 5000, true);
        
        if (isTTS)accessory.Method.TTS("出啥躲啥，注意修血");
        if (isEdgeTTS)accessory.Method.EdgeTTS("出啥躲啥，注意修血");
        accessory.Method.SendChat("/e ————小抄————\nT：MT拉斧 & 贤王，ST拉壁，注意顺劈面向（贤王会吸蓝，不推荐DK拉）\nD：注意修血，前两次需要同时击杀\nH：组合技时注意驱散[莫古乱乱乱]与[怒发冲冠]、注意AOE");
        
    }
    
    [ScriptMethod(name: "小怪出现提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2070"])]
    public void 小怪出现提示(Event @event, ScriptAccessory accessory)
    {        
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (!isTank) return; 
        if (isText)accessory.Method.TextInfo("MT拉 < 斧 > & < 贤王 > 、ST拉 < 壁 >，都有顺劈\n全部小怪需要同时击杀两次", duration: 5000, true);
        if (isTTS)accessory.Method.TTS("注意面向，注意修血");
        if (isEdgeTTS)accessory.Method.EdgeTTS("注意面向，注意修血");
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
                if (isText)accessory.Method.TextInfo($"第{timeMooglesseOblige}次复活", duration: 2700, false);
                if (isTTS)accessory.Method.TTS($"第{timeMooglesseOblige}次复活");
                if (isEdgeTTS)accessory.Method.EdgeTTS($"第{timeMooglesseOblige}次复活");
                accessory.Method.SendChat($"/e 已复活次数：{timeMooglesseOblige}");
            }
                break;
            case 2:
            {
                if (isText)accessory.Method.TextInfo($"第{timeMooglesseOblige}次复活,即将超大AOE\n然后打死一只小怪后读条狂暴", duration: 2700, false);
                if (isTTS)accessory.Method.TTS($"第{timeMooglesseOblige}次复活，即将超大AOE");
                if (isEdgeTTS)accessory.Method.EdgeTTS($"第{timeMooglesseOblige}次复活，即将超大AOE");
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
        if (isText)accessory.Method.TextInfo("将其它莫古拉出增益圈", duration: 5200, false);
        if (isTTS)accessory.Method.TTS("将其它莫古拉出增益圈");
        if (isEdgeTTS)accessory.Method.EdgeTTS("将其它莫古拉出增益圈");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "莫古力进行曲";
        dp.Color = new Vector4(0f, 1f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "绒绒核爆（大钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2055"])]
    public void 绒绒核爆(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "绒绒核爆";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(20.9f);
        dp.DestoryAt = 7700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "绒绒核爆销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:2055"],userControl: false)]
    public void 绒绒核爆销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("绒绒核爆");
    }
    
    [ScriptMethod(name: "莫古神准箭 点名提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:1635"])]
    public void 莫古神准箭(Event @event, ScriptAccessory accessory)
    {
        string tname = @event["TargetName"]?.ToString() ?? "未知目标";

        if (@event.TargetId() == accessory.Data.Me)
        {
            // if (isText)accessory.Method.TextInfo("四连突刺点名", duration: 3700, true);
            if (isTTS)accessory.Method.TTS("四连突刺点名");
            if (isEdgeTTS)accessory.Method.EdgeTTS("四连突刺点名");
            accessory.Method.SendChat("/e 四连突刺点名");
        } else
        {
            // if (isText)accessory.Method.TextInfo($"四连攻击点 <{tname}>", duration: 3700, false);
            if (isTTS)accessory.Method.TTS($"四连攻击点{tname}");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"四连攻击点{tname}");
            accessory.Method.SendChat($"/e 四连攻击点 <{@event.TargetName()}>");
        }
    }
    #endregion
    
    #region 组合技阶段
    //【组合技：柔柔之力 + 茸茸之愈】
    //【台词】茸茸之愈 库普洛·奇普 2111 把你们都打下来库啵！ ； 柔柔之力 普库啦·普奇 2117 打下来，打下来库啵！
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
    
    //【组合技：软软之弓 + 绵绵之音 + 蓬蓬之障】
    //【台词】软软之弓 库普括·叩吉 2114 三角攻击库啵！ ； 绵绵之音 普库嘻·皮叩 2119 三角关系库啵！ ； 蓬蓬之障 普库呐·帕叩 2121 好、好的库啵！
    [ScriptMethod(name: "莫古三角攻击", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2114"])]
    public void 莫古三角攻击(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("远离中间，靠场边躲避大三角", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("远离中间");
        if (isEdgeTTS) accessory.Method.EdgeTTS("远离中间");
    }
    
    //【组合技：柔柔之力 + 茸茸之愈 + 毛毛之斧】
    //莫古的诅咒：柔柔之力 对自身周围角色附加60s莫古乱乱乱，使受到 全屏AOE [莫古飞天乱] 的攻击伤害大幅提高。
    //绒绒神圣：茸茸之愈 开始咏唱，咏唱完毕时对全场范围造成高伤害无属性魔法伤害。茸茸之愈 受到一定量伤害后停止咏唱。
    //【台词】毛毛之斧 库普塔·咔帕 2110 转啊转啊库啵！ ； 茸茸之愈 库普洛·奇普 2112 莫古也一起攻击库啵！ ； 柔柔之力 普库啦·普奇 2118 削弱敌人库啵！
    [ScriptMethod(name: "莫古乱乱乱 驱散提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:473"])]
    public void 莫古乱乱乱(Event @event, ScriptAccessory accessory)
    {
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (!isHealer) return; 
        if (isText) accessory.Method.TextInfo("驱散 <莫古乱乱乱> ", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("驱散《莫古乱乱乱》");
        if (isEdgeTTS) accessory.Method.EdgeTTS("驱散《莫古乱乱乱》");

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
        if (isText) accessory.Method.TextInfo("攻击 < 茸茸之愈 > 以打断 < 绒绒神圣 > ", duration: 3000, true);
        if (isTTS) accessory.Method.TTS("攻击《茸茸之愈》打断读条");
        if (isEdgeTTS) accessory.Method.EdgeTTS("攻击《茸茸之愈》打断读条");

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
    //【台词】绒绒之壁 库普迪·库普 2108 好机会库啵！ ； 软软之弓 库普括·叩吉 2115 莫古来支援库啵！ ； 蓬蓬之障 普库呐·帕叩 2122 和、和练习时一样库啵！
    
    [ScriptMethod(name: "放马过来库啵 远离提示", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2108"])]
    public void 放马过来库啵(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("远离 <蓬蓬之障> 与 <绒绒之壁>", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("远离连线目标");
        if (isEdgeTTS) accessory.Method.EdgeTTS("远离连线目标");
        
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "绒绒之壁连线";
            dp.Color = new Vector4(1f, 0f, 0f, 1f);
            dp.Owner = @event.SourceId();
            dp.TargetObject = accessory.Data.Me;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Scale = new(1);
            dp.DestoryAt = 6700;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    /*
    [ScriptMethod(name: "放马过来库啵 连线提示", eventType: EventTypeEnum.Tether, eventCondition: ["Id:000D"])]
    public void 放马过来库啵Thther(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("远离 <蓬蓬之障> 与 <绒绒之壁>", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("远离连线目标");
        if (isEdgeTTS) accessory.Method.EdgeTTS("远离连线目标");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "绒绒之壁连线";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.TargetId();
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Scale = new(1);
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    */
    
    [ScriptMethod(name: "怒发冲冠 驱散提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:402"])]
    public void 怒发冲冠(Event @event, ScriptAccessory accessory)
    {
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (!isHealer) return; 
        if (isText) accessory.Method.TextInfo("驱散 <怒发冲冠> ", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("驱散《怒发冲灌》");
        if (isEdgeTTS)  accessory.Method.EdgeTTS("驱散《怒发冲灌》");
            
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"怒发冲冠{@event.TargetId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1f);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        accessory.Method.RemoveDraw("（绒绒之壁|蓬蓬之障)连线");
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
        if(isText) accessory.Method.TextInfo("大AOE伤害 ", duration: 4700, true);
        if(isTTS) accessory.Method.TTS("大AOE伤害");
        if(isEdgeTTS) accessory.Method.EdgeTTS("大AOE伤害");
    }
    
    [ScriptMethod(name: "死亡莫古警告 狂暴提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2121"])]
    public void 死亡莫古警告(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("攻击贤王，狂暴", duration: 5000, true);
        if(isTTS) accessory.Method.TTS("集中攻击贤王");
        if(isEdgeTTS) accessory.Method.EdgeTTS("集中攻击贤王");

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

public static class DirectionCalc
{
    // 以北为0建立list
    // Game         List    Logic
    // 0            - 4     pi
    // 0.25 pi      - 3     0.75pi
    // 0.5 pi       - 2     0.5pi
    // 0.75 pi      - 1     0.25pi
    // pi           - 0     0
    // 1.25 pi      - 7     1.75pi
    // 1.5 pi       - 6     1.5pi
    // 1.75 pi      - 5     1.25pi
    // Logic = Pi - Game (+ 2pi)

    /// <summary>
    /// 将游戏基角度（以南为0，逆时针增加）转为逻辑基角度（以北为0，顺时针增加）
    /// 算法与Logic2Game完全相同，但为了代码可读性，便于区分。
    /// </summary>
    /// <param name="radian">游戏基角度</param>
    /// <returns>逻辑基角度</returns>
    public static float Game2Logic(this float radian)
    {
        // if (r < 0) r = (float)(r + 2 * Math.PI);
        // if (r > 2 * Math.PI) r = (float)(r - 2 * Math.PI);

        var r = float.Pi - radian;
        r = (r + float.Pi * 2) % (float.Pi * 2);
        return r;
    }

    /// <summary>
    /// 将逻辑基角度（以北为0，顺时针增加）转为游戏基角度（以南为0，逆时针增加）
    /// 算法与Game2Logic完全相同，但为了代码可读性，便于区分。
    /// </summary>
    /// <param name="radian">逻辑基角度</param>
    /// <returns>游戏基角度</returns>
    public static float Logic2Game(this float radian)
    {
        // var r = (float)Math.PI - radian;
        // if (r < Math.PI) r = (float)(r + 2 * Math.PI);
        // if (r > Math.PI) r = (float)(r - 2 * Math.PI);

        return radian.Game2Logic();
    }

    /// <summary>
    /// 输入逻辑基角度，获取逻辑方位（斜分割以正上为0，正分割以右上为0，顺时针增加）
    /// </summary>
    /// <param name="radian">逻辑基角度</param>
    /// <param name="dirs">方位总数</param>
    /// <param name="diagDivision">斜分割，默认true</param>
    /// <returns>逻辑基角度对应的逻辑方位</returns>
    public static int Rad2Dirs(this float radian, int dirs, bool diagDivision = true)
    {
        var r = diagDivision
            ? Math.Round(radian / (2f * float.Pi / dirs))
            : Math.Floor(radian / (2f * float.Pi / dirs));
        r = (r + dirs) % dirs;
        return (int)r;
    }

    /// <summary>
    /// 输入坐标，获取逻辑方位（斜分割以正上为0，正分割以右上为0，顺时针增加）
    /// </summary>
    /// <param name="point">坐标点</param>
    /// <param name="center">中心点</param>
    /// <param name="dirs">方位总数</param>
    /// <param name="diagDivision">斜分割，默认true</param>
    /// <returns>该坐标点对应的逻辑方位</returns>
    public static int Position2Dirs(this Vector3 point, Vector3 center, int dirs, bool diagDivision = true)
    {
        double dirsDouble = dirs;
        var r = diagDivision
            ? Math.Round(dirsDouble / 2 - dirsDouble / 2 * Math.Atan2(point.X - center.X, point.Z - center.Z) / Math.PI) % dirsDouble
            : Math.Floor(dirsDouble / 2 - dirsDouble / 2 * Math.Atan2(point.X - center.X, point.Z - center.Z) / Math.PI) % dirsDouble;
        return (int)r;
    }

    /// <summary>
    /// 以逻辑基弧度旋转某点
    /// </summary>
    /// <param name="point">待旋转点坐标</param>
    /// <param name="center">中心</param>
    /// <param name="radian">旋转弧度</param>
    /// <returns>旋转后坐标点</returns>
    public static Vector3 RotatePoint(this Vector3 point, Vector3 center, float radian)
    {
        // 围绕某点顺时针旋转某弧度
        Vector2 v2 = new(point.X - center.X, point.Z - center.Z);
        var rot = MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian;
        var length = v2.Length();
        return new Vector3(center.X + MathF.Sin(rot) * length, center.Y, center.Z - MathF.Cos(rot) * length);
    }

    /// <summary>
    /// 以逻辑基角度从某中心点向外延伸
    /// </summary>
    /// <param name="center">待延伸中心点</param>
    /// <param name="radian">旋转弧度</param>
    /// <param name="length">延伸长度</param>
    /// <returns>延伸后坐标点</returns>
    public static Vector3 ExtendPoint(this Vector3 center, float radian, float length)
    {
        // 令某点以某弧度延伸一定长度
        return new Vector3(center.X + MathF.Sin(radian) * length, center.Y, center.Z - MathF.Cos(radian) * length);
    }

    /// <summary>
    /// 寻找外侧某点到中心的逻辑基弧度
    /// </summary>
    /// <param name="center">中心</param>
    /// <param name="newPoint">外侧点</param>
    /// <returns>外侧点到中心的逻辑基弧度</returns>
    public static float FindRadian(this Vector3 newPoint, Vector3 center)
    {
        var radian = MathF.PI - MathF.Atan2(newPoint.X - center.X, newPoint.Z - center.Z);
        if (radian < 0)
            radian += 2 * MathF.PI;
        return radian;
    }

    /// <summary>
    /// 将输入点左右折叠
    /// </summary>
    /// <param name="point">待折叠点</param>
    /// <param name="centerX">中心折线坐标点</param>
    /// <returns></returns>
    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
    {
        return point with { X = 2 * centerX - point.X };
    }

    /// <summary>
    /// 将输入点上下折叠
    /// </summary>
    /// <param name="point">待折叠点</param>
    /// <param name="centerZ">中心折线坐标点</param>
    /// <returns></returns>
    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
    {
        return point with { Z = 2 * centerZ - point.Z };
    }
    
    /// <summary>
    /// 将输入点朝某中心点往内/外同角度延伸，默认向内
    /// </summary>
    /// <param name="point">待延伸点</param>
    /// <param name="center">中心点</param>
    /// <param name="length">延伸长度</param>
    /// <param name="isOutside">是否向外延伸</param>>
    /// <returns></returns>
    public static Vector3 PointInOutside(this Vector3 point, Vector3 center, float length, bool isOutside = false)
    {
        Vector2 v2 = new(point.X - center.X, point.Z - center.Z);
        var targetPos = (point - center) / v2.Length() * length * (isOutside ? 1 : -1) + point;
        return targetPos;
    }

    /// <summary>
    /// 获得两点之间距离
    /// </summary>
    /// <param name="point"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static float DistanceTo(this Vector3 point, Vector3 target)
    {
        Vector2 v2 = new(point.X - target.X, point.Z - target.Z);
        return v2.Length();
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