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

namespace The_Ageless_Necropolis;

[ScriptType(guid: "07a60fa1-8228-4e24-841d-f9000fcbf06e", name: "永远之暗歼灭战", territorys: [1295],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class The_Ageless_Necropolis
{
    const string noteStr =
        """
        v0.0.0.1:
        LV100 7.3 永远之暗歼灭战 初版绘制
        """;
    
    #region 设置与变量
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;
    
    uint Circle1=0; // 026D [青魂]
    uint Circle4=0; // 025C [青之连魂]
    uint Dount1=0; // 026E [青魂]
    uint Dount4=0; // 025D [青之连魂]
    uint MidDanger=0;  // 025F [青魂] [青之连魂]
    uint SideDanger=0;  // 025E [青魂] [青之连魂]
    
    public void Init(ScriptAccessory accessory) {
        Circle1=0;
        Circle4=0;
        Dount1=0; 
        Dount4=0; 
        MidDanger=0;
        SideDanger=0;
    }
    
    #endregion
    
    [ScriptMethod(name: "招死之手_压溃（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44523"])]
    public void 招死之手_压溃(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "招死之手_压溃";
        dp.Scale = new (6, 24f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "暗之死腕（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4452[45]$"])]
    public void 暗之死腕(Event @event, ScriptAccessory accessory)
    {
        // ActionId == 44525 为 面向BOSS的 左半场刀
        var isLeft = @event.ActionId == 44525 ? "右" : "左";
        if (isText)accessory.Method.TextInfo($"稍后向{isLeft}穿", duration: 6000, true);
        if (isTTS)accessory.Method.TTS($"稍后向{isLeft}穿");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"稍后向{isLeft}穿");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "暗之死腕";
        dp.Scale = new (22f, 37f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
       
        switch (@event.ActionId())
        {
            case 44524:
                dp.Offset = new Vector3 (-7, 0 ,0);
                break;
            case 44525:
                dp.Offset = new Vector3 (7, 0 ,0);
                break;
        }
      
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "暗气流";
        dp1.Scale = new (24f, 37f);
        dp1.Owner = @event.SourceId();
        dp1.Color = accessory.Data.DefaultDangerColor;
       
        switch (@event.ActionId())
        {
            case 44524:
                dp1.Offset = new Vector3 (-6, 0 ,0);
                break;
            case 44525:
                dp1.Offset = new Vector3 (6, 0 ,0);
                break;
        }

        dp1.Delay = 6000;
        dp1.DestoryAt = 1000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);  
    }
    
    /*
    [ScriptMethod(name: "暗气流（半场直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44526"])]
    public void 暗气流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "暗气流";
        dp.Scale = new (24, 30f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    */
    
    [ScriptMethod(name:"青魂记录", eventType:EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^02[56](C|D|E)$"], userControl:false)]
    public void 青魂记录(Event @event, ScriptAccessory accessory) 
    {
        if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG] 成功检测到TargetIcon生成, Id: {@event.Id().ToString("X4")}, targetid: {@event.TargetId()}");
         
        var idStr = @event.Id().ToString("X4");
        switch (idStr)
        {
            case "026D":
                Circle1 = 1;
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]已记录：青魂 = 钢铁");
                break;
            case "026E":
                Dount1  = 1;
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]已记录：青魂 = 月环");
                break;
            case "025C":
                Circle4 = 1;
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]已记录：青之连魂 = 钢铁");
                break;  
            case "025D":
                Dount4  = 1;
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]已记录：青之连魂 = 月环");
                break;
        }
    }
    
    

    [ScriptMethod(name: "青之波动（释放钢铁月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44528"])]
    public void 青之波动(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "青之波动";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.DestoryAt = 5600;
        if (Circle1 == 1)
        {
            dp.Scale = new Vector2(20f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        if (Dount1 == 1)
        {
            dp.Scale = new Vector2(41f);
            dp.InnerScale = new Vector2(16f);
            dp.Radian = float.Pi * 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
    }

    
    [ScriptMethod(name:"青魂销毁", eventType:EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^4518[12]$"], userControl:false)]
    public void 青魂销毁(Event @event, ScriptAccessory accessory) 
    {
        switch (@event.ActionId)
        {
            case 45181:
                Circle1 = 0;
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]已清除：青魂 = 钢铁");
                break;  
            case 45182:
                Dount1  = 0;
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]已清除：青魂 = 月环");
                break;
        }
    }
    
    [ScriptMethod(name: "青之魂块_大十字（距离衰减安全区）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44535"])]
    public void 青之魂块_大十字距离衰减安全区(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("去斜角贴边");
        if (isEdgeTTS)accessory.Method.EdgeTTS("去斜角贴边");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "青之魂块_大十字距离衰减安全区";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(0.5f);
        dp.DestoryAt = 4700;
        
        Vector3[] offsets = {
            new Vector3(6, 0, 6),
            new Vector3(6, 0, -6),
            new Vector3(-6, 0, 6),
            new Vector3(-6, 0, -6)
        };

        foreach (var offset in offsets)
        {
            dp.Offset = offset;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "暗之巨腕_拖入（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44817"])]
    public void 暗之巨腕_拖入(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "暗之巨腕_拖入";
        dp.Scale = new (10, 36f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
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
    
    public static uint Id(this Event @event)
    {
        var idStr = @event["Id"];
        if (string.IsNullOrEmpty(idStr)) return 0;
        
        if (idStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return uint.Parse(idStr.Substring(2), System.Globalization.NumberStyles.HexNumber);
        }
        
        if (idStr.Length <= 4 && idStr.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
        {
            return uint.Parse(idStr, System.Globalization.NumberStyles.HexNumber);
        }
        
        return uint.Parse(idStr);
    }
    
}
public static class MathHelpers
{
    public static float DegToRad(float degrees)
    {
        return degrees * (float)(Math.PI / 180.0);
    }
    
    public static double DegToRad(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    
    public static float RadToDeg(float radians)
    {
        return radians * (float)(180.0 / Math.PI);
    }
    
    public static double RadToDeg(double radians)
    {
        return radians * 180.0 / Math.PI;
    }
}