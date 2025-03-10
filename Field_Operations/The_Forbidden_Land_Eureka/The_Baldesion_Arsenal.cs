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

namespace The_Baldesion_Arsenal_Eureka;


[ScriptType(guid: "cb001385-81e0-4c16-8bb3-fb51fd70336a", name: "禁地优雷卡 - 巴尔德西昂兵武塔", territorys: [827],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class The_Baldesion_Arsenal
{
    const string noteStr =
        """
        v0.0.0.1:
        禁地优雷卡 - 巴尔德西昂兵武塔 初版绘制
        【施工中】
        """;
    
    #region 全局销毁
    //全局
    //7538 插言 ； 7551 伤头 ； 7540 下踢 ； 7863 扫腿 ； 25880 催眠 ； 16560 沉静
    
    [ScriptMethod(name: "伤头 & 插言 打断销毁", eventType: EventTypeEnum.ActionEffect, userControl: false, 
     eventCondition: ["ActionId:regex:^(7538|7551)$"] )]
    public void 伤头插言打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"兵武元精_强放逐{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "眩晕打断销毁", eventType: EventTypeEnum.ActionEffect, userControl: false, 
        eventCondition: ["ActionId:regex:^(7540|7863)$"] )]
    public void 眩晕打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"兵武元精_强放逐{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "睡眠打断销毁", eventType: EventTypeEnum.ActionEffect, userControl: false, 
        eventCondition: ["ActionId:regex:^(25880|16560)$"] )]
    public void 睡眠打断销毁(Event @event, ScriptAccessory accessory)
    {
        // accessory.Method.RemoveDraw($"暂时还没有{@event.TargetId()}");
    }
    #endregion
    
    #region 小怪
    // 入口小怪
    [ScriptMethod(name: "兵武比布鲁斯 尾部碎击", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15356"])]
    public void 兵武比布鲁斯_尾部碎击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "兵武比布鲁斯_尾部碎击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12);
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "兵武元精 强放逐", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15383"])]
    public void 兵武元精_强放逐(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"兵武元精_强放逐{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    // BOSS1后
    [ScriptMethod(name: "兵武卡尔克布莉娜 恐惧之瞳（石化顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15366"])]
    public void 兵武卡尔克布莉娜_恐惧之瞳(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "兵武卡尔克布莉娜_恐惧之瞳";
        dp.Color = new Vector4(1f, 0f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(42.2f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    #endregion
    

    #region BOSS1 亚特
    // 14632：本体读条 [妖枪振]钢铁 ； 14633：本体读条 [妖枪旋]月环 ； 14631：幻枪招来
    //幻枪招来后，BOSS（DataID：9818）会连线4根 烈焰金枪（DataID：9819），并读条随机[妖枪振]钢铁月环，随后烈焰金枪将瞬发BOSS的钢铁月环 [幻枪振] 14634 / 14635
    /*
    [ScriptMethod(name: "BOSS1_亚特 妖枪振（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14632"])]
    public void 妖枪振(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "妖枪振";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 4200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1_亚特 妖枪旋（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14633"])]
    public void 妖枪旋(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "妖枪旋";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(22f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    */
    #endregion

    
    #region BOSS1 欧文
    #endregion

    #region BOSS2 莱丁
    [ScriptMethod(name: "BOSS2_莱丁 天逆矛（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14441"])]
    public void 天逆矛(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "天逆矛";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.DestoryAt = 7200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS2_莱丁 旋・斩铁剑（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14442"])]
    public void 旋斩铁剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "旋斩铁剑";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    

    [ScriptMethod(name: "BOSS2_莱丁 片・斩铁剑（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1444[34]$"])]
    public void 片斩铁剑(Event @event, ScriptAccessory accessory)
    {
        bool isLeft=true;
        //14444 左 ， 14443右

        if(@event["ActionId"].Equals("14444")) {

            isLeft=true;

        }
        
        if(@event["ActionId"].Equals("14443")) {

            isLeft=false;

        }
        
        var dp=accessory.Data.GetDefaultDrawProperties();
        dp.Name="片斩铁剑";
        dp.Scale=new(70,39);
        dp.Owner=@event.SourceId();
        dp.Color=accessory.Data.DefaultDangerColor;
        dp.DestoryAt=6200;

        if(isLeft) {
            
            dp.Rotation=90f.DegToRad();
            dp.Offset=new Vector3(3.5f,0,0);

        }

        else {
            
            dp.Rotation=-90f.DegToRad();
            dp.Offset=new Vector3(-3.5f,0,0);
            
        }
        
        //dp.Offset=new Vector3(0,0,3.5f);
        // 向后移动3.5f
        // 我不确定rotation是否会影响偏差
        // 如果并没有按照预期向左向右偏移 注释或者删掉掉上面这个offset设置^ 然后解除注释if里的两个注释
        // 如果这样就已经生效了 就删除if里的两个注释
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }


    [ScriptMethod(name: "BOSS2_莱丁 战死击（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14460"])]
    public void 战死击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "战死击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.4f);
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
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