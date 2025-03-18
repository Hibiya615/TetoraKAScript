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
using System.Threading.Tasks;

namespace DevoutPilgrimsVSDaivadipa;

[ScriptType(guid: "da82aeb0-9635-4f13-a1c1-39a0c859f596", name: "兽道诸神信仰：伪神降临", territorys: [957],
    version: "0.0.0.11", author: "Tetora", note: noteStr)]

public class Daivadipa
{
    const string noteStr =
        """
        v0.0.0.11:
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
        dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "迷失连线销毁", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:regex:^758[67]$"],userControl: false)]
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
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new(10f, 50f);
        dp.DestoryAt = 6700;
        
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Scale = new(10f, 50f);
        dp1.DestoryAt = 2200;

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
                    dp1.Name = "圣火猛击蓝";
                    dp1.Owner = item.EntityId;
                    dp1.Delay = 6900;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);
                }
                break;

            case 26499:
                foreach (var item in IbcHelper.GetByDataId(13680))
                {
                    dp.Name = "圣火猛击蓝";
                    dp.Owner = item.EntityId;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
                
                foreach (var item in IbcHelper.GetByDataId(13679))
                {
                    dp1.Name = "圣火猛击红";
                    dp1.Owner = item.EntityId;
                    dp1.Delay = 6900;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);
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
        var dp1 = accessory.Data.GetDefaultDrawProperties();

        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 6700;
        
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Scale = new Vector2(10f);
        dp1.DestoryAt = 3700;
        
        switch (@event.ActionId())
        {
            case 26498:
                foreach (var item in IbcHelper.GetByDataId(13681))
                {
                    dp.Name = "燃烧红";
                    dp.Owner = item.EntityId;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                
                foreach (var item in IbcHelper.GetByDataId(13682))
                {
                    dp1.Name = "燃烧蓝";
                    dp1.Owner = item.EntityId;
                    dp1.Delay = 7200;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
                }
                break;

            case 26499:
                foreach (var item in IbcHelper.GetByDataId(13682))
                {
                    dp.Name = "燃烧蓝";
                    dp.Owner = item.EntityId;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                
                foreach (var item in IbcHelper.GetByDataId(13681))
                {
                    dp1.Name = "燃烧红";
                    dp1.Owner = item.EntityId;
                    dp1.Delay = 7200;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
                }
                break;
        }
    }
    
    [ScriptMethod(name: "移动命令 位置预测", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^19(5[89]|6[01])$"])]
    public async void 移动命令(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return;
        await Task.Delay(8000);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "移动命令";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(1f, 5f);
        dp.DestoryAt = 3000;
        
        switch (@event["StatusID"])
        {
            case "1958":
                dp.Rotation = 0f.DegToRad();
                accessory.Method.EdgeTTS("向前移动到安全区");
                accessory.Method.TextInfo("强制移动：前", duration: 3000, true);
                break;
            case "1959":
                dp.Rotation = 180f.DegToRad();
                accessory.Method.EdgeTTS("向后移动到安全区");
                accessory.Method.TextInfo("强制移动：后", duration: 3000, true);
                break;
            case "1960":
                dp.Rotation = 90f.DegToRad();
                accessory.Method.EdgeTTS("向左移动到安全区");
                accessory.Method.TextInfo("强制移动：左", duration: 3000, true);
                break;
            case "1961":
                dp.Rotation = 270f.DegToRad();
                accessory.Method.EdgeTTS("向右移动到安全区");
                accessory.Method.TextInfo("强制移动：右", duration: 3000, true);
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
    }
    
    [ScriptMethod(name: "移动命令销毁备用", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1257"],userControl: false)]
    public void 移动命令销毁(Event @event, ScriptAccessory accessory)
    {
        // 开始移动时，强制移动buff Id 为 1257 ，销毁移动预测
        accessory.Method.RemoveDraw("移动命令");
    }
    
    [ScriptMethod(name: "大象死亡销毁", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:13677"],userControl: false)]
    public void 大象死亡销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
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
