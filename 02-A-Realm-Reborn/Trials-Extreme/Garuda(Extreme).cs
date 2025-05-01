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

namespace Garuda_Extreme;

[ScriptType(guid: "3e4685ed-1425-4ddc-8222-a2c15dcc9ad5", name: "迦楼罗歼殛战", territorys: [297],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Garuda_Extreme
{
    const string noteStr =
        """
        v0.0.0.1:
        LV50 迦楼罗歼殛战 初版绘制
        """;
    
    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "开场提示", eventType: EventTypeEnum.Director, eventCondition: ["Command:40000001"])]
    public async void 开场提示(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;

        if (isTank && isText)accessory.Method.TextInfo("难度：★，顺劈伤害尽量避开柱子\nMT拉美翼&本体去北开减伤，ST拉妙翅去南", duration: 5000, true);
        if (!isTank && isText)accessory.Method.TextInfo("难度：☆，打羽毛，优先柔羽，小怪出现后先打妙翅", duration: 5000, true);

        accessory.Method.SendChat("/e ————小抄————\nT：伤害避开柱子，MT拉美翼&本体去北，ST拉妙翅去南\nD：打羽毛，优先柔羽，小怪出现后先打妙翅\nH：多留意T血量、以及瞬发顺劈钢铁");
        
    }
    
    [ScriptMethod(name: "台风眼（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1671"])]
    public void 台风眼(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "台风眼";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.InnerScale = new Vector2(11f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "螺旋气流（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1382"])]
    public void 螺旋气流(Event @event, ScriptAccessory accessory)
    {
        var r = accessory.Data.Objects.SearchByEntityId(@event.SourceId())?.HitboxRadius?? 0;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "螺旋气流";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10+r);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "寒风之歌 躲在石头后提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1761"])]
    public void 寒风之歌(Event @event, ScriptAccessory accessory)
    {        
        if (isTTS)accessory.Method.TTS("躲在石柱后");
        if (isEdgeTTS)accessory.Method.EdgeTTS("躲在石柱后");
    }
    
    [ScriptMethod(name: "飞翎雨（Imgui）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1550"])]
    public void 飞翎雨(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "飞翎雨";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "美翼&妙翅出现提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:1569"])]
    public void 美翼妙翅出现提示(Event @event, ScriptAccessory accessory)
    {        
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank && isText)accessory.Method.TextInfo("MT拉 <美翼> <本体> 去北开减伤，ST拉 <妙翅> 去南", duration: 7000, true);
        if (!isTank && isText)accessory.Method.TextInfo("优先击杀妙翅", duration: 5000, true);
        if (!isTank && isTTS)accessory.Method.TTS("优先击杀妙翅");
        if (!isTank && isEdgeTTS)accessory.Method.EdgeTTS("优先击杀妙翅");
    }
    
    [ScriptMethod(name: "柔羽击杀提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:1537"])]
    public void 柔羽(Event @event, ScriptAccessory accessory)
    {        
        // 刚羽：244
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (!isTank && isTTS)accessory.Method.TTS("击杀柔羽");
        if (!isTank && isEdgeTTS)accessory.Method.EdgeTTS("击杀柔羽");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "柔羽连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "刺羽提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:1536"])]
    public void 刺羽(Event @event, ScriptAccessory accessory)
    {        
        if (isText)accessory.Method.TextInfo("在大AOE时再击杀刺羽，进高气压圈回避", duration: 5000, true);
        if (isTTS)accessory.Method.TTS("别打刺羽");
        if (isEdgeTTS)accessory.Method.EdgeTTS("别打刺羽");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "刺羽";
        dp.Color = new Vector4(1f, 0f, 1f, 1.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 50000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "刺羽销毁1", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1555"],userControl: false)]
    public void 刺羽销毁1(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("刺羽");
    }
    
    [ScriptMethod(name: "刺羽销毁2", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:1536"],userControl: false)]
    public void 刺羽销毁2(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("刺羽");
    }
    
    /*  不知道是谁给的TargetIcon，无法正常瞄准，暂时作废
    [ScriptMethod(name: "下行突风（点名顺劈）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0001"])]
    public void 下行突风(Event @event, ScriptAccessory accessory)
    {
        var r = accessory.Data.Objects.SearchByEntityId(@event.SourceId())?.HitboxRadius?? 0;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "下行突风";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(10+r);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
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