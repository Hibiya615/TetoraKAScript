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
using KodakkuAssist.Extensions;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace NewDuty;

[ScriptType(guid: "80890eac-4730-4708-ad1b-05aba469c2a1", name: "最新最热临时绘制", territorys: [1307,1320,1322,1324,1326],
    version: "0.0.0.7", author: "Tetora", note: noteStr)]

/* MapID
 * 1307: 格莱杨拉波尔歼灭战
 * 1320~1326 M9N~M12N
 */

public class NewDuty
{
    const string noteStr =
        """
        v0.0.0.7:
        最新最热副本绘制，可能会电，介意请关闭
        别人的正式版发了这边就删
        """;
    
    #region 用户控制

    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;

    #endregion
    

    
    

    #region  格莱杨拉波尔歼灭战
    
    [ScriptMethod(name: "—————— 格莱杨拉波尔歼灭战 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 格莱杨拉波尔歼灭战(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "雷电爆发 双死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45660"])]
    public void 雷电爆发(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"双死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"双死刑");
    }
    
    [ScriptMethod(name: "雷光急行 击退提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45618"])]
    public void 雷光急行(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"击退", duration: 5000, true);
        if (isTTS)accessory.Method.TTS($"击退");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"击退");
    }
    
    [ScriptMethod(name: "抽雾 吸引提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45625"])]
    public void 抽雾(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"吸引", duration: 5000, true);
        if (isTTS)accessory.Method.TTS($"吸引");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"吸引");
    }
    
    [ScriptMethod(name: "抽雾 吸引（吸引预测）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45625"])]
    public void 抽雾_吸引预测(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "抽雾_吸引预测";
        dp.Scale = new(1f, 19f);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.Rotation = 180f.DegToRad();
        dp.FixRotation = true;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "抽雾吸引防击退销毁", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(160|1209|2663)$"],userControl: false)]
    public void 抽雾吸引防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("抽雾_吸引预测");
    }
        
    [ScriptMethod(name: "排雾 吸引矩形危险区", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45667"])]
    public void 排雾(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "排雾";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(20f, 20f); 
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "无控急行 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45624"])]
    public void 无控急行(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    /* 无法判断高低VFX
    
    [ScriptMethod(name: "雷光环_雷转质射线（直线）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:18993"])]
    public void 雷光环_雷转质射线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "雷光环_雷转质射线";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new(5f, 30f);
        dp.DestoryAt = 7000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    */
    
    [ScriptMethod(name: "护卫炮塔_雷转质射线（直线危险区）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45629|4563[0-3])$"])]
    public void 护卫炮塔_雷转质射线Danger(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "护卫炮塔_雷转质射线";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(5f, 50f); 
        switch (@event.ActionId())
        {
            case 45629:
                dp.Scale = new(5f, 25f); 
                break;
            case 45630:
                dp.Scale = new(5f, 10f); // 实际为25m，但是只有一格高台 不画太多
                break;
            case 45631:
                dp.Scale = new(5f, 20f); 
                break;
            case 45632:
                dp.Scale = new(5f, 15f); 
                break;
            case 45633:
                dp.Scale = new(5f, 10f); 
                break;
        }
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "前照光 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45637"])]
    public void 前照光(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"下安全", duration: 6000, false);
        if (isTTS)accessory.Method.TTS($"下下下");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"下下下");
    }
    
    /* 无法判断VFX高低
    [ScriptMethod(name: "前照光/雷鸣吐息 上下AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4563[57]$"])]
    public void 上下AOE(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "上下AOE";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(20f, 60f); 
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    */
    
    [ScriptMethod(name: "雷鸣吐息 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45635"])]
    public void 雷鸣吐息(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"上安全", duration: 6000, true);
        if (isTTS)accessory.Method.TTS($"上上上");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"上上上");
    }
    
    [ScriptMethod(name: "雷光雨（魔法阵钢铁最终位置）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45659"])]
    public void 雷光雨(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"雷光雨";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16f);
        dp.DestoryAt = 2900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion

    
    #region M9N
    
    [ScriptMethod(name: "—————— M9N ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void M9N(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "魅亡之音 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45921"])]
    public void 魅亡之音(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "月之半相（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4590[6-9]|4591[0-3])$"])]
    public void 月之半相(Event @event, ScriptAccessory accessory)
    {
        // 左刀: 45906 [二段右刀: 45907] ; 右刀: 45910 [二段左刀: 45911] ; 强化左刀: 45908 [二段右刀: 45909] ; 强化右刀: 45912 [二段左刀: 45913] ; 
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "月之半相";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60);
        dp.Radian = 180f.DegToRad();
        
        switch (@event.ActionId())
        {
            case 45906:
                dp.Rotation = 90f.DegToRad();
                dp.DestoryAt = 4700;
                break;
            case 45907:
                dp.Rotation = 270f.DegToRad();
                dp.Delay = 4700;
                dp.DestoryAt = 3500;
                break;
            case 45910:
                dp.Rotation = 270f.DegToRad();
                dp.DestoryAt = 4700;
                break;
            case 45911:
                dp.Rotation = 90f.DegToRad();
                dp.Delay = 4700;
                dp.DestoryAt = 3500;
                break;
            case 45908:
                dp.Offset = new Vector3(4, 0, 0);
                dp.Rotation = 90f.DegToRad();
                dp.DestoryAt = 4700;
                break;
            case 45909:
                dp.Offset = new Vector3(-4, 0, 0);
                dp.Rotation = 270f.DegToRad();
                dp.Delay = 4700;
                dp.DestoryAt = 3500;
                break;
            case 45912:
                dp.Offset = new Vector3(-4, 0, 0);
                dp.Rotation = 270f.DegToRad();
                dp.DestoryAt = 4700;
                break;
            case 45913:
                dp.Offset = new Vector3(4, 0, 0);
                dp.Rotation = 90f.DegToRad();
                dp.Delay = 4700;
                dp.DestoryAt = 3500;
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "硬核之声 双死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45914"])]
    public void 硬核之声(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"坦克死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"坦克死刑");
    }
    
    [ScriptMethod(name: "施虐的尖啸 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45875"])]
    public void 施虐的尖啸(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "致命刑锯_前进（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45877"])]
    public void 致命刑锯_前进(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "致命刑锯_前进";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(20f, 10f); 
        dp.Offset = new Vector3(0, 0, 5);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "致命刑锯_冲出（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^458(7[89]|80)$"])]
    public void 致命刑锯_冲出(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "致命刑锯_冲出";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f); 
        dp.Scale = new(5f, 32f);
        dp.Delay = 2000;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "尖锐的音调（分组分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45925"])]
    public void 尖锐的音调(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"分组分摊");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"分组分摊");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"尖锐的音调";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 7000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "全场杀伤 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45886"])]
    public void 全场杀伤(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "致命的闭幕曲 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4588[89]"])]
    public void 致命的闭幕曲(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "碎烂脉冲（圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45894"])]
    public void 碎烂脉冲(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "碎烂脉冲";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "以太流失（十字）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45897"])]
    public void 以太流失(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "以太流失";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f); 
        dp.Scale = new(6f, 80f); 
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
        
        for(int i=1;i<=2;++i) {
            accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Straight, dp);
            dp.Rotation+=90f.DegToRad();
        }
    }
    
    [ScriptMethod(name: "粗暴之雨（连续分摊）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0131"])]
    public void 粗暴之雨(Event @event, ScriptAccessory accessory)
    {
        if (HelperExtensions.GetCurrentTerritoryId() != 1320) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"粗暴之雨";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 9400; // 以太增强后会多一次，第一次分摊其实是8400ms
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "贪欲无厌 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45892"])]
    public void 贪欲无厌(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "致命轮锯（圆形）高亮", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:19173"])]
    public void 致命轮锯高亮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"致命轮锯高亮";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(2.5f);
        dp.DestoryAt = 63300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"致命轮锯高亮描边";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(2.5f);
        dp1.InnerScale = new Vector2(2.46f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 63300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "致命刑锯（直线）高亮", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:19174"])]
    public void 致命刑锯高亮(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "致命刑锯高亮";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f); 
        dp.Scale = new(5f, 15f);
        dp.Offset = new Vector3(0f, 0f, 4f);
        dp.DestoryAt = 63300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "致命刺锤_掉落 踩塔提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45883"])]
    public void 致命刺锤_掉落(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"踩塔", duration: 6000, false);
        if (isTTS)accessory.Method.TTS($"踩塔");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"踩塔");
    }
    
    [ScriptMethod(name: "致命刺锤_刚刺发射 攻击提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45885"])]
    public void 刚刺发射(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"攻击刺锤");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"攻击刺锤");
    }
    
    [ScriptMethod(name: "致命刑具销毁", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:regex:^1917[34]$"],userControl: false)]
    public void 致命刑具销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"致命.*");
    }
    
    
    #endregion
    
    #region M10N
    
    [ScriptMethod(name: "—————— M10N ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void M10N(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "炽焰冲击（分摊死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46464"])]
    public void 炽焰冲击(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        
        if (isTTS)accessory.Method.TTS($"分摊死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"分摊死刑");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"炽焰冲击";
        dp.Color = isTank ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "斗志昂扬 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4646[67]$"])]
    public void 斗志昂扬(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "破势乘浪（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46483"])]
    public void 破势乘浪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "破势乘浪";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(15f, 50f); 
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
    }
    
    [ScriptMethod(name: "惊涛骇浪（击退预测）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46481"])]
    public void 惊涛骇浪(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "惊涛骇浪";
        dp.Scale = new(1f, 10f);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.Rotation = @event.SourceRotation();
        dp.FixRotation = true;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "惊涛骇浪防击退销毁", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(160|1209|2663)$"],userControl: false)]
    public void 惊涛骇浪防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("惊涛骇浪");
    }
    
    [ScriptMethod(name: "浪尖转体（跳跃顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46488"])]
    public void 浪尖转体(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"浪尖转体";
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 6500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "深海冲击 死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46465"])]
    public void 深海冲击(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"坦克死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"坦克死刑");
    }
    
    [ScriptMethod(name: "极限炫技（距离衰减）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46499"])]
    public void 极限炫技(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"去两侧贴边");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"去两侧贴边");
    }
    
    [ScriptMethod(name: "腾火踏浪（四连踩塔）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46474"])]
    public void 腾火踏浪(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"四连踩塔");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"四连踩塔");
    }
    
    [ScriptMethod(name: "极限以太_混合爆炸（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46507"])]
    public void 极限以太_混合爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"极限以太_混合爆炸";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "旋绕烈火（分摊）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0293"])]
    public void 旋绕烈火(Event @event, ScriptAccessory accessory)
    {
        if (HelperExtensions.GetCurrentTerritoryId() != 1322) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"旋绕烈火";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    
    #endregion
    
    #region M11N
    
    [ScriptMethod(name: "—————— M11N ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void M11N(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "铸兵猛攻 - 斧（钢铁预备）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46007"])]
    public void 铸兵猛攻_斧预备(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"稍后钢铁");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"稍后钢铁");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"铸兵猛攻_斧";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 8100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "铸兵猛攻 - 斧（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46011"])]
    public void 铸兵猛攻_斧(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"远离");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"远离");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"铸兵猛攻_斧";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "铸兵猛攻 - 镰刀（月环预备）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46008"])]
    public void 铸兵猛攻_镰刀预备(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"稍后月环");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"稍后月环");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"铸兵猛攻_镰刀";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.InnerScale = new Vector2(4.9f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 8100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "铸兵猛攻 - 镰刀（月环危险区）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46013"])]
    public void 铸兵猛攻_镰刀(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"靠近");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"靠近");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"铸兵猛攻_镰刀";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "铸兵猛攻 - 剑（十字预备）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46009"])]
    public void 铸兵猛攻_剑预备(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"稍后十字");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"稍后十字");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "铸兵猛攻_剑";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.15f); 
        dp.Scale = new(10f, 80f); 
        dp.DestoryAt = 8100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
        
        for(int i=1;i<=2;++i) {
            accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Straight, dp);
            dp.Rotation+=90f.DegToRad();
        }
    }
    
    [ScriptMethod(name: "铸兵猛攻 - 剑（十字）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46015"])]
    public void 铸兵猛攻_剑(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"去四角");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"去四角");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "铸兵猛攻_剑";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f); 
        dp.Scale = new(10f, 80f); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
        
        for(int i=1;i<=2;++i) {
            accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Straight, dp);
            dp.Rotation+=90f.DegToRad();
        }
    }
    
    [ScriptMethod(name: "彗星雨 分散+连续黄圈", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46024"])]
    public void 彗星雨(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"分散并连续躲避黄圈");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"分散并连续躲避黄圈");
    }
    
    [ScriptMethod(name: "彗星风暴 黄圈躲避提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46026"],suppress:500)]
    public void 彗星风暴(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"快躲开");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"快躲开");
    }
    
    [ScriptMethod(name: "统治的战舞_爆炸（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:47033"])]
    public void 统治的战舞_爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "统治的战舞_爆炸";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f); 
        dp.Scale = new(10f, 60f); 
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
    }
    
    [ScriptMethod(name: "铸兵之令：轰击（分散+死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46037"])]
    public void 铸兵之令_轰击(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"分散加死刑");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"分散加死刑");
    }
    
    [ScriptMethod(name: "铸兵轰击（分摊死刑）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0258"])]
    public void 铸兵轰击(Event @event, ScriptAccessory accessory)
    {
        if (HelperExtensions.GetCurrentTerritoryId() != 1324) return;
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"铸兵轰击";
        dp.Color = isTank ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "霸王大漩涡（清血）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46039"])]
    public void 霸王大漩涡(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"清一雪");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"清一雪");
    }
    
    [ScriptMethod(name: "举世无双的霸王 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46044"])]
    public void 举世无双的霸王(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"AOE", duration: 8000, true);
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "大漩涡_强风（四向扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46041"])]
    public void 大漩涡_强风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"大漩涡_强风";
        dp.Color = new Vector4(1f, 0f, 0f, 2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 45f.DegToRad();
        dp.Delay = 3700;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "王者陨石雨（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46059"])] // 也许还有优化空间
    public void 王者陨石雨(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "王者陨石雨";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.2f); 
        dp.Scale = new(10f, 60f); 
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "王者陨石（黄圈高亮）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46058"])]
    public void 王者陨石(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"王者陨石";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Delay = 2200;
        dp.DestoryAt = 1500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "遮天陨石（核爆危险区）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46060"])]
    public void 遮天陨石(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"遮天陨石";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = 6200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "登天碎地（拆半场）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4606[3579]$"])]
    public void 登天碎地Front(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "登天碎地Front";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(40f, 26f); 
        dp.DestoryAt = 12300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "登天碎地（砸半场）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^460(6[468]|70)$"])]
    public void 登天碎地Back(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "登天碎地Back";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f); 
        dp.Scale = new(40f, 80f);
        dp.Delay = 4900;
        dp.DestoryAt = 12300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
    }
    
    #endregion
    
    #region M12N
    
    [ScriptMethod(name: "—————— M12N ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void M12N(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "流血 提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46182"])]
    public void 流血(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"观察触手方向", duration: 5500, true);
        if (isTTS)accessory.Method.TTS($"观察触手方向");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"观察触手方向");
    }
    
    [ScriptMethod(name: "灾变吐息（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:47547"])]
    public void 灾变吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"灾变吐息";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (20f, 30f);
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "极饿伸展（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46189"])]
    public void 极饿伸展(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"极饿伸展";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(35f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 10300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "大爆炸（屎块钢铁出现预兆）", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2015017","Operate:Add"])]
    public void 大爆炸预兆(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"大爆炸预兆{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.InnerScale = new Vector2(11.96f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "大爆炸（屎块钢铁出现预兆）销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:46191"],userControl: false)]
    public void 大爆炸预兆销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"大爆炸预兆.*");
    }
    
    [ScriptMethod(name: "大爆炸（屎块钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46191"])]
    public void 大爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"大爆炸";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "巨蟒绞缠（缠绕钢铁）判定时间", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46198"])]
    public void 巨蟒绞缠(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"巨蟒绞缠";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 15200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    

    [ScriptMethod(name: "盛大登场（拆地板蛇出现）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4620[23]$"])]
    public void 盛大登场(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"盛大登场";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (6f, 15f);
        dp.Offset = new Vector3 (0, 0 ,2);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    
    [ScriptMethod(name: "震场（拆地板）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46205"])]
    public void 震场(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"震场";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (10f, 15f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "分裂灾变（蛇头直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46207"])]
    public void 分裂灾变(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"分裂灾变";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (10f, 30f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "爆风细胞（击退距离预测）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^474[78]$"])]
    public void 爆风细胞(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = @event.StatusId==4747 ? "爆风细胞前" : "爆风细胞后";
        dp.Scale = new(1f,15f);
        dp.Owner = accessory.Data.Me;
        dp.Rotation = @event.StatusId==4747 ? float.Pi *2 : float.Pi;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.DestoryAt = 7000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "细胞失控（本体直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:48090"])]
    public void 细胞失控(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"细胞失控";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (8f, 30f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "细胞失控（直线2）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:48091"])]
    public void 细胞失控2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"细胞失控2";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (8f, 30f);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "细胞失控（直线3）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:48092"])]
    public void 细胞失控3(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"细胞失控3";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (8f, 30f);
        dp.DestoryAt = 7200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "细胞失控（大直线预兆）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:48095"])]
    public void 细胞失控大(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"细胞失控大";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.Scale = new (35f, 1f);
        dp.DestoryAt = 15800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
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

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;
        return attribute?.Description ?? value.ToString();
    }
}

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

public enum MarkType
{
    None = -1,
    Attack1 = 0,
    Attack2 = 1,
    Attack3 = 2,
    Attack4 = 3,
    Attack5 = 4,
    Bind1 = 5,
    Bind2 = 6,
    Bind3 = 7,
    Ignore1 = 8,
    Ignore2 = 9,
    Square = 10,
    Circle = 11,
    Cross = 12,
    Triangle = 13,
    Attack6 = 14,
    Attack7 = 15,
    Attack8 = 16,
    Count = 17
}

public static class IbcHelper
{
    public static IGameObject? GetById(this ScriptAccessory sa, ulong gameObjectId)
    {
        return sa.Data.Objects.SearchById(gameObjectId);
    }

    public static IGameObject? GetMe(this ScriptAccessory sa)
    {
        return sa.Data.Objects.LocalPlayer;
    }

    public static IEnumerable<IGameObject?> GetByDataId(this ScriptAccessory sa, uint dataId)
    {
        return sa.Data.Objects.Where(x => x.DataId == dataId);
    }

    public static string GetPlayerJob(this ScriptAccessory sa, IPlayerCharacter? playerObject, bool fullName = false)
    {
        if (playerObject == null) return "None";
        return fullName ? playerObject.ClassJob.Value.Name.ToString() : playerObject.ClassJob.Value.Abbreviation.ToString();
    }

    public static float GetStatusRemainingTime(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return 0;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return charaStruct->GetStatusManager()->GetRemainingTime(statusIdx);
        }
    }

    public static bool HasStatus(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return false;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return statusIdx != -1;
        }
    }

    /// <summary>
    /// 获取指定标记索引的对象EntityId
    /// </summary>
    public static unsafe ulong GetMarkerEntityId(uint markerIndex)
    {
        var markingController = MarkingController.Instance();
        if (markingController == null) return 0;
        if (markerIndex >= 17) return 0;

        return markingController->Markers[(int)markerIndex];
    }

    /// <summary>
    /// 获取对象身上的标记
    /// </summary>
    /// <returns>MarkType</returns>
    public static MarkType GetObjectMarker(IGameObject? obj)
    {
        if (obj == null || !obj.IsValid()) return MarkType.None;

        ulong targetEntityId = obj.EntityId;
            
        for (uint i = 0; i < 17; i++)
        {
            var markerEntityId = GetMarkerEntityId(i);
            if (markerEntityId == targetEntityId)
            {
                return (MarkType)i;
            }
        }

        return MarkType.None;
    }

    /// <summary>
    /// 检查对象是否有指定的标记
    /// </summary>
    public static bool HasMarker(IGameObject? obj, MarkType markType)
    {
        return GetObjectMarker(obj) == markType;
    }

    /// <summary>
    /// 检查对象是否有任何标记
    /// </summary>
    public static bool HasAnyMarker(IGameObject? obj)
    {
        return GetObjectMarker(obj) != MarkType.None;
    }

    private static ulong GetMarkerForObject(IGameObject? obj)
    {
        if (obj == null) return 0;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return markerEntityId;
                }
            }
        }
        return 0;
    }

    private static MarkType GetMarkerTypeForObject(IGameObject? obj)
    {
        if (obj == null) return MarkType.None;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return (MarkType)i;
                }
            }
        }
        return MarkType.None;
    }

    /// <summary>
    /// 获取标记的名称
    /// </summary>
    public static string GetMarkerName(MarkType markType)
    {
        return markType switch
        {
            MarkType.Attack1 => "攻击1",
            MarkType.Attack2 => "攻击2",
            MarkType.Attack3 => "攻击3",
            MarkType.Attack4 => "攻击4",
            MarkType.Attack5 => "攻击5",
            MarkType.Bind1 => "止步1",
            MarkType.Bind2 => "止步2",
            MarkType.Bind3 => "止步3",
            MarkType.Ignore1 => "禁止1",
            MarkType.Ignore2 => "禁止2",
            MarkType.Square => "方块",
            MarkType.Circle => "圆圈",
            MarkType.Cross => "十字",
            MarkType.Triangle => "三角",
            MarkType.Attack6 => "攻击6",
            MarkType.Attack7 => "攻击7",
            MarkType.Attack8 => "攻击8",
            _ => "无标记"
        };
    }
    
    public static float GetHitboxRadius(IGameObject obj)
    {
        if (obj == null || !obj.IsValid()) return -1;
        return obj.HitboxRadius;
    }

}

public static class HelperExtensions
{
    public static unsafe uint GetCurrentTerritoryId()
    {
        return AgentMap.Instance()->CurrentTerritoryId; // 额外进行地图ID判断
    }
}

#region 特殊函数
public unsafe static class ExtensionVisibleMethod
{
    public static bool IsCharacterVisible(this ICharacter chr)
    {
        var v = (IntPtr)(((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)chr.Address)->GameObject.DrawObject);
        if (v == IntPtr.Zero) return false;
        return Bitmask.IsBitSet(*(byte*)(v + 136), 0);
    }

    public static class Bitmask
    {
        public static bool IsBitSet(ulong b, int pos)
        {
            return (b & (1UL << pos)) != 0;
        }

        public static void SetBit(ref ulong b, int pos)
        {
            b |= 1UL << pos;
        }

        public static void ResetBit(ref ulong b, int pos)
        {
            b &= ~(1UL << pos);
        }

        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static bool IsBitSet(short b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
#endregion 特殊函数
