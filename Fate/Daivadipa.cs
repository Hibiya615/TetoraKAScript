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

namespace DevoutPilgrimsVSDaivadipa;

[ScriptType(guid: "da82aeb0-9635-4f13-a1c1-39a0c859f596", name: "兽道诸神信仰：伪神降临", territorys: [957],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Daivadipa
{
    const string noteStr =
        """
        v0.0.0.1:
        LV90 特殊Fate 绘制
        兽道诸神信仰：伪神降临
        """;
    
    [ScriptMethod(name: "迷失连线", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^758[67]$"])]
    public void 迷失连线(Event @event, ScriptAccessory accessory)
    {
        // Data ID 7586: 迷失少女 ；7587：迷失者
        accessory.Method.TextInfo("迷失出现", duration: 5000, true);
        accessory.Method.TTS("迷失出现");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "迷失连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 900000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "迷失连线销毁", eventType: EventTypeEnum.Death, eventCondition: ["DataId:regex:^758[67]$"], userControl: false)]
    public void 迷失连线销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("迷失连线");
    }
    
    [ScriptMethod(name: "操焰仪 弹窗提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2649[89]$"])]
    public void 操焰仪提示(Event @event, ScriptAccessory accessory)
    {
        switch (@event.ActionId())
        {
            case 26498:
                accessory.Method.TextInfo("先蓝色安全", duration: 5000, false);
                break;

            case 26499:
                accessory.Method.TextInfo("先红色安全", duration: 5000, true);
                break;
        }
    }
    
    [ScriptMethod(name: "左侧三叉戟 & 右侧圣战斧", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2650[89]$"])]
    public void 半场扇形(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(65);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 6700;
        
        switch (@event.ActionId())
        {
            case 26508:
                dp.Name = "左侧三叉戟";
                dp.Rotation = 90f.DegToRad(); 
                break;

            case 26509:
                dp.Name = "右侧圣战斧";
                dp.Rotation = 270f.DegToRad(); 
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    public static class IbcHelper
    {
        public static IBattleChara? GetById(uint id)
        {
            return (IBattleChara?)Svc.Objects.SearchByEntityId(id);
        }
        
        public static IEnumerable<IGameObject?> GetByDataId(uint dataId)
        {
            return Svc.Objects.Where(x => x.DataId == dataId);
        }
    }
    
    [ScriptMethod(name: "圣火猛击（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2649[89]$"])]
    public void 圣火猛击(Event @event, ScriptAccessory accessory)
    {
        // 26497 空华焰 生成焰宝珠 ：DataID 13679 红色  ；13680 蓝色
        // 操焰仪：26498 红色先炸 ； 26499 蓝色先炸
        // 技能ID 26500 红色 ； 26501 蓝色
        
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new(10f, 50f);
        dp.DestoryAt = 2700;
        
        switch (@event.ActionId())
        {
            case 26498:
                foreach (var item in IbcHelper.GetByDataId(13679))
                {
                    dp.Name = "圣火猛击红";
                    dp.Owner = item.EntityId;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
                
                foreach (var item in IbcHelper.GetByDataId(13680))
                {
                    dp.Name = "圣火猛击蓝";
                    dp.Owner = item.EntityId;
                    dp.Delay = 3000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
                break;

            case 26499:
                foreach (var item in IbcHelper.GetByDataId(13680))
                {
                    dp.Name = "圣火猛击蓝";
                    dp.Owner = item.EntityId;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
                break;
        }
    }
    
    [ScriptMethod(name: "燃烧（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2649[89]$"])]
    public void 燃烧(Event @event, ScriptAccessory accessory)
    {
        // 26502 26503 爆炎招来 生成爆宝珠 ：DataID 13681 红色  ；13682 蓝色
        // 操焰仪：26498 红色先炸 ； 26499 蓝色先炸
        // 技能ID 26506 红色 ； 26507 蓝色
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 2700;
        
        switch (@event.ActionId())
        {
            case 26498:
                foreach (var item in IbcHelper.GetByDataId(13681))
                {
                    dp.Name = "燃烧红";
                    dp.Owner = item.EntityId;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                break;

            case 26499:
                foreach (var item in IbcHelper.GetByDataId(13682))
                {
                    dp.Name = "燃烧蓝";
                    dp.Owner = item.EntityId;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                break;
        }
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
