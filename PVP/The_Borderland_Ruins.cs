using System;
using System.Runtime;
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
using System.Threading;
using System.Threading.Tasks;

namespace The_Borderland_Ruins;

[ScriptType(guid: "ec9c9f7d-be48-4974-82cf-396b0fc29261", name: "周边遗迹群（阵地战）", territorys: [1273],
    version: "0.0.0.3", author: "Tetora", note: noteStr)]

public class The_Borderland_Ruins
{
    const string noteStr =
        """
        v0.0.0.3:
        周边遗迹群（阵地战）部分绘制
        """;
    
    #region 基础控制
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion
    
    [ScriptMethod(name: "对地炮击（地面黄圈）填充动画", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42935"])]
    public void 对地炮击(Event @event, ScriptAccessory accessory)
    {
        // 请求对地炮击 42933
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "对地炮击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(10f);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "集中炮击（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42936"])]
    public void 集中炮击(Event @event, ScriptAccessory accessory)
    {
        // 请求集中炮击 43158   
        // TargetIcon: 003E
        if (@event.TargetId() != accessory.Data.Me)
        {
            if (isText) accessory.Method.TextInfo("分摊", duration: 4300, false);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "集中炮击";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.TargetId();
            dp.Scale = new Vector2(6f);
            dp.DestoryAt = 4700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        else
        {
            if (isText)accessory.Method.TextInfo("分摊点名", duration: 4300, true);
            if (isTTS)accessory.Method.TTS("分摊点名");
            if (isEdgeTTS)accessory.Method.EdgeTTS("分摊点名");
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = "集中炮击点名";
            dp1.Color = accessory.Data.DefaultSafeColor;
            dp1.Owner = @event.TargetId();
            dp1.Scale = new Vector2(6f);
            dp1.DestoryAt = 4700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
        }
    }
    
    [ScriptMethod(name: "精密炮击（追踪黄圈）Imgui高亮", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4293[78]$"])]
    public void 精密炮击(Event @event, ScriptAccessory accessory)
    {
        // 请求精密炮击 43159
        // TargetIcon: 00C5
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "对地炮击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 900;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
    }
    
 
    private Guid _currentOperationId = Guid.Empty;
    
    [ScriptMethod(name: "物体130（高台狂暴）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2616"])]
    public async void 物体130(Event @event, ScriptAccessory accessory)
    {
        var operationId = Guid.NewGuid();
        _currentOperationId = operationId;
    
        await Task.Delay(19700);
    
        if (_currentOperationId != operationId) return;
        
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "物体130";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(38.4f);
        dp.DestoryAt = 10000;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "物体130描边";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(8f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(38.4f);
        dp1.InnerScale = new Vector2(38.3f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }

    [ScriptMethod(name: "物体130中断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:2616"], userControl: false)]
    public void 物体130中断销毁(Event @event, ScriptAccessory accessory)
    {
        _currentOperationId = Guid.NewGuid(); 
        accessory.Method.RemoveDraw($"物体130.*");
    }

    [ScriptMethod(name: "截击系统死亡销毁", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:3096"], userControl: false)]
    public void 截击系统死亡销毁(Event @event, ScriptAccessory accessory)
    {
        _currentOperationId = Guid.NewGuid(); 
        accessory.Method.RemoveDraw($"物体130.*");
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