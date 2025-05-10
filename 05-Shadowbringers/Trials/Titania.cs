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

namespace Titania;

[ScriptType(guid: "71a3a109-0ac3-4041-aa87-ff156f5aaeea", name: "缇坦妮雅歼灭战", territorys: [845],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Titania
{
    const string noteStr =
        """
        v0.0.0.1:
        LV73 缇坦妮雅歼灭战 初版绘制
        TTS请在“用户设置”中二选一启用，请勿同时开启
        """;

    [UserSetting("TTS开关（TTS请二选一开启）")] public bool isTTS { get; set; } = false;

    [UserSetting("EdgeTTS开关（TTS请二选一开启）")] public bool isEdgeTTS { get; set; } = true;

    [UserSetting("弹窗文本提示开关")] public bool isText { get; set; } = true;


    uint FrostRune = 0; // 冰之符文
    public void Init(ScriptAccessory accessory)
    {
        FrostRune = 0;
    }


    [ScriptMethod(name: "魔之符文（扇形顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15707"])]
    public void 魔之符文(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "魔之符文";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Scale = new Vector2(60);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "烈火锤（分摊）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:003E"])]
    public void 烈火锤(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("水圈分摊", duration: 4500, false);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "烈火锤";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    // 15664 仲夏夜之梦 切换场地至森林 , 随后释放根之符文

    [ScriptMethod(name: "（仅T）第一轮根之符文预测 ", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:15664"])]
    public void 根之符文预测(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (!isTank) return;
        if (isText) accessory.Method.TextInfo("将BOSS预先拉出树根范围", duration: 15000, false);
        if (isTTS) accessory.Method.TTS("将BOSS预先拉出树根范围");
        if (isEdgeTTS) accessory.Method.EdgeTTS("将BOSS预先拉出树根范围");
        var currentProperty = accessory.Data.GetDefaultDrawProperties();
        currentProperty.Name = "根之符文预测";
        currentProperty.Scale = new(6, 40f);
        currentProperty.Position = new Vector3(100f, 0f, 100f);
        currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(0.3f);
        currentProperty.Delay = 5000;
        currentProperty.DestoryAt = 10600;
        currentProperty.Rotation = 0;

        for (int i = 1; i <= 8; ++i)
        {
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);
            currentProperty.Rotation += 45f.DegToRad();
        }
    }

    [ScriptMethod(name: "根之符文 ", eventType: EventTypeEnum.ObjectChanged,
        eventCondition: ["DataId:2010084", "Operate:Add"])]
    public void 根之符文(Event @event, ScriptAccessory accessory)
    {
        var currentProperty = accessory.Data.GetDefaultDrawProperties();
        currentProperty.Name = "根之符文";
        currentProperty.Scale = new(6, 40f);
        currentProperty.Owner = @event.SourceId();
        currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        currentProperty.DestoryAt = 12000;
        currentProperty.Rotation = 0;

        for (int i = 1; i <= 8; ++i)
        {
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);
            currentProperty.Rotation += 45f.DegToRad();
        }
    }

    [ScriptMethod(name: "冰之符文（提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15658"])]
    public void 冰之符文提示(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("外圈等待 > 穿进中间", duration: 9000, false);
        if (isTTS) accessory.Method.TTS("外圈等待然后穿进中间");
        if (isEdgeTTS) accessory.Method.EdgeTTS("外圈等待然后穿进中间");
    }

    [ScriptMethod(name: "冰之符文（九连环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15694"])]
    public void 冰之符文(Event @event, ScriptAccessory accessory)
    {
        FrostRune++;

        if (FrostRune > 5) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "冰之符文";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        if (FrostRune == 1)
        {
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = "中间连线";
            dp2.Owner = accessory.Data.Me;
            dp2.Color = accessory.Data.DefaultSafeColor;
            dp2.ScaleMode |= ScaleMode.YByDistance;
            dp2.TargetPosition = new Vector3(100f, 0f, 100f);
            dp2.Scale = new(1);
            dp2.Delay = 6000;
            dp2.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
        }
    }
    
    [ScriptMethod(name: "欢快的安息日清空 ", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15708"], userControl: false)]
    public void 欢快的安息日(Event @event, ScriptAccessory accessory)
    {
        FrostRune = 0; // 每次AOE时清空冰之符文计数
    }

    [ScriptMethod(name: "P2 拉怪提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:15665"])]
    public void P2拉怪提示(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank && isText) accessory.Method.TextInfo("将 <帕克> <豌豆花> 拉至东侧 <芥子> 处", duration: 5000, false);
        if (isTank && isTTS) accessory.Method.TTS("将小怪拉至芥子处");
        if (isTank && isEdgeTTS) accessory.Method.EdgeTTS("将小怪拉至芥子处");
    }

    [ScriptMethod(name: "P2 爱懒花", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15677"])]
    public void 爱懒花(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank)
        {
            if (isText) accessory.Method.TextInfo("在西侧或北侧待机，并拉 <帕克> 或 <豌豆花>", duration: 21300, false);
            if (isTTS) accessory.Method.TTS("在西侧或北侧待机");
            if (isEdgeTTS) accessory.Method.EdgeTTS("在西侧或北侧待机");
        }
        else
        {
            if (isText) accessory.Method.TextInfo("东侧待机，优先攻击 <芥子>", duration: 21300, false);
            if (isTTS) accessory.Method.TTS("东侧待机，优先攻击芥子");
            if (isEdgeTTS) accessory.Method.EdgeTTS("东侧待机，优先攻击芥子");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "中间连线";
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.TargetPosition = new Vector3(111f, 0f, 100f);
            dp.Scale = new(1);
            dp.DestoryAt = 22000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }

    [ScriptMethod(name: "帕克的吐息（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15703"])]
    public void 帕克的吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "帕克的吐息";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "终有一死（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15697"])]
    public void 终有一死(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 11500, true);
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