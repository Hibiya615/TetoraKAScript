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

namespace A5N;

[ScriptType(guid: "1462516c-2bc2-4560-8244-387845cc098d", name: "A5N", territorys: [520],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class A5N
{
    const string noteStr =
        """
        v0.0.0.1:
        LV60 亚历山大机神城 律动之章1（奇才 拉特芬克斯） 初版绘制
        """;
    
    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "全力重拳 换T提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:995","StackCount:3"])]
    public void 全力重拳T(Event @event, ScriptAccessory accessory)
    {
        // BOSS使用4次 牵制刺拳（ActionId：5525）后，会对4层冲击（StatusID:995）的玩家直接赋予脑震荡（StatusID:996）buff十秒，随后对一仇目标瞬发直线死刑 全力重拳（ActionId：5526）
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (@event.TargetId() != accessory.Data.Me)
        {
            if (isTank && isText) accessory.Method.TextInfo("挑衅，稍后直线死刑", duration: 10500, true);
            if (isTank && isTTS) accessory.Method.TTS("挑衅BOSS"); 
            if (isTank && isEdgeTTS) accessory.Method.EdgeTTS("挑衅BOSS");
        }
        else
        {
            if (isTank && isText) accessory.Method.TextInfo("大减伤或换T", duration: 10500, true); 
            if (isTank && isTTS) accessory.Method.TTS("连续死刑"); 
            if (isTank && isEdgeTTS) accessory.Method.EdgeTTS("连续死刑");
        }
    }
    
    [ScriptMethod(name: "全力重拳 减伤提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:996"])]
    public void 全力重拳DH(Event @event, ScriptAccessory accessory)
    {
        var boss = accessory.Data.Objects.GetByDataId(5353).FirstOrDefault();
        if (boss == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "全力重拳";
        dp.Scale = new (4, 45.2f);
        dp.Owner = boss.GameObjectId;
        dp.TargetObject = @event.TargetId();
        dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.TargetOrderIndex = 1;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
        
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (!isTank && isTTS) accessory.Method.TTS("坦克直线死刑，给减伤"); 
        if (!isTank && isEdgeTTS) accessory.Method.EdgeTTS("坦克直线死刑，给减伤");
    }
    
    [ScriptMethod(name: "全力重拳销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:5526"],userControl:false)]
    public void 全力重拳销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("全力重拳");
    }
    
    [ScriptMethod(name: "强酸剧毒药 毒池点名", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
    public void 强酸剧毒药(Event @event, ScriptAccessory accessory)
    {
        if (isTTS && @event.TargetId() == accessory.Data.Me) accessory.Method.TTS("远离放置毒池"); 
        if (isEdgeTTS && @event.TargetId() == accessory.Data.Me) accessory.Method.EdgeTTS("远离放置毒池"); 
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "强酸剧毒药";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "投放炸弹（红毒提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5520"])]
    public void 投放炸弹(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank)
        {
            if(isText)accessory.Method.TextInfo("将BOSS拉至角落，躲避炸弹", duration: 19000, true);
            if (isTTS)accessory.Method.TTS("将BOSS拉至角落");
            if (isEdgeTTS)accessory.Method.EdgeTTS("将BOSS拉至角落");
        }
        else
        {
            if (isText)accessory.Method.TextInfo("激活装置，吃红毒，变成猩猩推炸弹至对角", duration: 19000, true);
            if (isTTS)accessory.Method.TTS("激活装置吃红毒，变成猩猩推炸弹至对角");
            if (isEdgeTTS)accessory.Method.EdgeTTS("激活装置吃红毒，变成猩猩推炸弹至对角");
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "红毒连线";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Position = new Vector3(0f, -10f, -190f);
            dp.TargetPosition = new Vector3(16f, -10f, -190f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Scale = new(1);
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }
    
    [ScriptMethod(name: "超高性能炸弹_大爆炸", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:5354"])]
    public void 大爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "大爆炸";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new(35);
        dp.DestoryAt = 16000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "大爆炸描边";
        dp2.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp2.Owner = @event.SourceId();
        dp2.Scale = new Vector2(35.06f);
        dp2.InnerScale = new Vector2(35f);
        dp2.Radian = float.Pi * 2;
        dp2.DestoryAt = 16000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
    }

    [ScriptMethod(name: "大爆炸销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:5521"],userControl:false)]
    public void 大爆炸销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("大爆炸.*");
    }

    [ScriptMethod(name: "蓄力（紫毒提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5522"])]
    public void 蓄力(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("激活装置，吃紫毒躲避4次AOE", duration: 15400, true);
        if (isTTS) accessory.Method.TTS("吃紫毒躲避4次AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS("吃紫毒躲避4次AOE");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "紫毒连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = new Vector3(-16f, -10f, -190f);
        dp.Scale = new(1);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    uint vulture = 0;
    
    public void Init(ScriptAccessory accessory) {
        vulture = 0; 
    }
    
    [ScriptMethod(name: "紫毒连线销毁", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:999"],userControl:false)]
    public void 紫毒连线销毁(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me) accessory.Method.RemoveDraw("紫毒连线");
        if (@event.TargetId() == accessory.Data.Me) vulture = 1;
    }
    
    [ScriptMethod(name: "排出药剂提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:5531"])]
    public void 排出药剂(Event @event, ScriptAccessory accessory)
    {
        // if (vulture == 1 && isText) accessory.Method.TextInfo("排出药剂", duration: 2000, false);
        if (vulture == 1 && isTTS) accessory.Method.TTS("排出药剂");
        if (vulture == 1 && isEdgeTTS) accessory.Method.EdgeTTS("排出药剂");
    }
    
    [ScriptMethod(name: "药物排出销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:5476"],userControl:false)]
    public void 药物排出销毁(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me) vulture = 0; 
    }
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
