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
using KodakkuAssist.Extensions;

namespace the_Whorleater_Extreme;

[ScriptType(guid: "07f20e0e-9463-4a10-9dd1-956fde6a9c46", name: "利维亚桑歼殛战", territorys: [359],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class the_Whorleater_Extreme
{
    const string noteStr =
        """
        v0.0.0.2:
        LV50 利维亚桑歼殛战 初版绘制
        """;
    
    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    #region 记录 & 阶段转换
    uint Dive = 0;
    uint Phase = 0;
    
    public void Init(ScriptAccessory accessory) {
        Dive = 0;
        Phase = 0;
    }
    
    [ScriptMethod(name: "阶段转换：P1", eventType: EventTypeEnum.Chat, userControl: false,eventCondition: ["Type:NPCDialogueAnnouncements",
        "Message:regex:^(被光明所污染的人们.*|You trespass upon my domain.*|我が海域を侵すか.*)$"])]
    public void 阶段转换1(Event @event, ScriptAccessory accessory)
    {
        Phase = 1;
    }
    
    [ScriptMethod(name: "阶段转换：P2", eventType: EventTypeEnum.Chat, userControl: false,eventCondition: ["Type:NPCDialogueAnnouncements",
        "Message:regex:^(蓝色清净的大海之水将会扫去一切污秽.*|Drink deep of the sea's bitter draught.*|清浄なる青き水を以て、穢れ清めん.*)$"])]
    public void 阶段转换2(Event @event, ScriptAccessory accessory)
    {
        Phase = 2;
    }
    
    [ScriptMethod(name: "阶段转换：P3", eventType: EventTypeEnum.Chat, userControl: false,eventCondition: ["Type:NPCDialogueAnnouncements",
        "Message:regex:^(竟然对我卖弄小伎俩.*|You challenge me with trickery.*|ぬぅ、小細工を弄したか.*)$"])]
    public void 阶段转换3(Event @event, ScriptAccessory accessory)
    {
        Phase = 3;
    }
    
    [ScriptMethod(name: "下潜记录", eventType: EventTypeEnum.Targetable, eventCondition: ["SourceName:regex:^(利维亚桑|Leviathan|リヴァイアサン)$", "Targetable:False"],userControl: false)]
    public void 下潜记录(Event @event, ScriptAccessory accessory)
    {
        Dive = 1;
    }
    
    [ScriptMethod(name: "下潜取消", eventType: EventTypeEnum.Targetable, eventCondition: ["SourceName:regex:^(利维亚桑|Leviathan|リヴァイアサン)$", "Targetable:True"],userControl: false)]
    public void 下潜取消(Event @event, ScriptAccessory accessory)
    {
        Dive = 0;
    }
    #endregion
    
    
    [ScriptMethod(name: "开场提示", eventType: EventTypeEnum.Countdown, eventCondition: ["Type:Stop","SourceId:E0000000"])]
    public async void 开场提示(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(3000); 
        if (isText)accessory.Method.TextInfo("难度：★★，TH不会建议退，小抄已发至聊天框 \nT：MT拉头，ST拉尾、小怪ST拉，晕波齿鱼人（60%血以下免晕）\nD：出黄球打黄球，小怪优先波齿鱼人，注意避开T的平A \nH：ST交给小仙女和再生奶，群抬错开ST，尽量避免水镜debuff"
            , duration: 10000, true);
        accessory.Method.SendChat("/e ————小抄————\nT：MT拉头，ST拉尾、小怪ST拉，晕波齿鱼人（60%血以下免晕）\n蓝球ST拉着溜，等一次拍击后拉离人群开大减炸掉 \nD：出黄球打黄球，小怪优先波齿鱼人，注意避开T的平A \nH：ST交给小仙女和再生奶，群抬错开ST，尽量避免水镜debuff");
    }
    
    
    [ScriptMethod(name: "水神的面纱 提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2165"])]
    public void 水神的面纱(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("法系打头部，物理打尾巴\n龙骑舞者赤魔DK额外注意自己的技能！\n尾巴有身位！", duration: 5000, true);
    }
    
    [ScriptMethod(name: "波齿鱼人 击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:2807"])]
    public void 波齿鱼人提示(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("优先击杀 < 波齿鱼人 >", duration: 2500, true);
    }
    
    [ScriptMethod(name: "波齿鱼人_恐慌风暴", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1865"])]
    public void 恐慌风暴(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.EdgeTTS("不要踩进恐慌圈");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "恐慌风暴";
        dp.Color = new Vector4(1f, 0f, 1f, 1.5f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
    }
    
    [ScriptMethod(name: "波齿鱼人_恐慌洗礼", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1866"])]
    public void 恐慌洗礼(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("眩晕 < 波齿鱼人 >", duration: 3200, true);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "恐慌洗礼";
        dp.Color = new Vector4(1f, 0f, 1f, 2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "下潜提示", eventType: EventTypeEnum.Targetable, eventCondition: ["SourceName:regex:^(利维亚桑|Leviathan|リヴァイアサン)$", "Targetable:False"])]
    public void 下潜提示(Event @event, ScriptAccessory accessory)
    {
        if (isText && Phase == 1)accessory.Method.TextInfo("去水柱对侧击退", duration: 5000, true);
        if (isText && Phase != 1)accessory.Method.TextInfo("中间集合，躲开南北俯冲，远离侧面水柱击退", duration: 5000, true);
    }
    
    [ScriptMethod(name: "旋转下潜 俯冲", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:2804"])]
    public void 旋转下潜(Event @event, ScriptAccessory accessory)
    {
        if ( Dive != 1) return;  
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "旋转下潜";
        dp.Scale = new (16, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "巨浪（击退）", eventType: EventTypeEnum.Chat,eventCondition: ["Type:NPCDialogueAnnouncements",
        "Message:regex:^(竟然对我卖弄小伎俩.*|You challenge me with trickery.*|ぬぅ、小細工を弄したか.*)$"])]
    public void 巨浪(Event @event, ScriptAccessory accessory)
    {
        // 击退距离为25m ，实际Scale宽=36m即为安全
        foreach (var item in accessory.Data.Objects.GetByDataId(2802))
        {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "巨浪";
        dp.Scale = new (40f, 30f);
        dp.Owner = item.EntityId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
        }
    }
    
    [ScriptMethod(name: "巨浪泡沫 提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:2810"])]
    public void 巨浪泡沫(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("ST接走蓝泡泡，避开人群溜溜球，转场完开减伤远离人群炸球", duration: 34500, false);
        accessory.Method.EdgeTTS("ST拉蓝球，约半分钟后爆炸");
    }
    
    #region 绘制销毁
    // 巨浪击退距离 25m ， 场地长 40m
    [ScriptMethod(name: "猛撞销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2090"],userControl: false)]
    public void 猛撞销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("巨浪");  // 销毁水柱击退
    }
    
    [ScriptMethod(name: "恐慌洗礼销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:1866"],userControl: false)]
    public void 恐慌洗礼销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("恐慌洗礼");
    }
    
    [ScriptMethod(name: "旋转下潜销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2186"],userControl: false)]
    public void 旋转下潜销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("旋转下潜");
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
