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

namespace E12S;

[ScriptType(guid: "f2ebc170-00af-415d-8dfe-30bc27f7fc06", name: "E12S", territorys: [949],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class E12S
{
    const string noteStr =
        """
        v0.0.0.1:
        LV80 伊甸零式希望乐园 再生之章4（伊甸之约） 初版绘制
        打法及各种选项请在“用户设置”中设置
        青魔攻略及减伤安排采用“驷马攻略组”6.x版本
        """;
    
    #region 用户设置
    
    private static List<string> _role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4", "粉毒", "蓝毒", "双耀星"];
    private static List<string> _party = ["单T单H", "单T双H", "双T单H", "双T双H"];
    
    [UserSetting("Debug模式，非开发用请关闭")]
    public static bool DebugMode { get; set; } = false;
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("青魔打法")]
    public bool isBlue { get; set; } = false;
    
    [UserSetting("青魔减伤提示")]
    public bool isBlueDefense { get; set; } = false;
    
    [UserSetting(note: "请选择你的队伍构成")]
    public PartyEnum Party { get; set; } = PartyEnum.None;
    
    [UserSetting(note: "请额外选择你的位置")]
    public RoleEnum Role { get; set; } = RoleEnum.None;
    
    public enum PartyEnum
    {
        None = -1,
        单T单H = 0,
        单T双H = 1,
        双T单H = 2,
        双T双H = 3,
    }
    
    public enum RoleEnum
    {
        None = -1,
        MT = 0,
        ST = 1,
        H1 = 2,
        H2 = 3,
        D1 = 4,
        D2 = 5,
        D3 = 6,
        D4 = 7,
        粉毒 = 8,
        蓝毒 = 9,
        双耀星 = 10,
    }
    
    #endregion
    
    #region 阶段转换
    
    private E12S_Phase phase = E12S_Phase.Init;
    public enum E12S_Phase
    {
        Init,    // 初始
        Shiva,  // 22676 融合：希瓦
        Titan,  // 22677 融合：泰坦
        Sculpture, // 22706 创造巨兵
        Beastly, // 狮子
        BasicRelativity, // 22752 时间压缩·起
        SingularApocalypse, // 22757 单一启示
        IntermediateRelativity, // 22723 时间压缩·承
        DualApocalypse, // 20508 双重启示
        AdvancedRelativity, // 22754 时间压缩·转
        TripleApocalypse, // 20509 三重启示
        TerminalRelativity, // 22755 时间压缩·合
    }
    
    [ScriptMethod(name: "阶段转换：希瓦", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22676"], userControl: false)]
    public void 希瓦阶段转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            E12S_Phase.Init => E12S_Phase.Shiva,
            _ => E12S_Phase.Titan,
        };
    }

    [ScriptMethod(name: "阶段转换：泰坦", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22677"],
        userControl: false)]
    public void 泰坦阶段转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            E12S_Phase.Init => E12S_Phase.Titan,
            _ => E12S_Phase.Shiva,
        };
    }
    
    [ScriptMethod(name: "阶段转换：兄贵", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:22706"], userControl: false)]
    public void 兄贵阶段转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            E12S_Phase.Shiva => E12S_Phase.Sculpture,
            E12S_Phase.Titan => E12S_Phase.Sculpture,
        };
    }
    
    [ScriptMethod(name: "阶段转换：狮子", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:22713"], userControl: false)]
    public void 狮子阶段转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            E12S_Phase.Sculpture => E12S_Phase.Beastly,
        };
    }

    [ScriptMethod(name: "阶段转换：一运", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22752"], userControl: false)]
    public void 一运阶段转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            E12S_Phase.Beastly => E12S_Phase.BasicRelativity,
        };
    }
    
    [ScriptMethod(name: "阶段转换：一火", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22752"], userControl: false)]
    public void 一火阶段转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            E12S_Phase.BasicRelativity => E12S_Phase.SingularApocalypse,
        };
    }
    
    [ScriptMethod(name: "阶段转换：二运", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22753"], userControl: false)]
    public void 二运阶段转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            E12S_Phase.SingularApocalypse => E12S_Phase.IntermediateRelativity,
        };
    }
    
    [ScriptMethod(name: "阶段转换：二火", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:20508"], userControl: false)]
    public void 二火阶段转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            E12S_Phase.IntermediateRelativity => E12S_Phase.DualApocalypse,
        };
    }
    
    [ScriptMethod(name: "阶段转换：三运", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22754"], userControl: false)]
    public void 三运阶段转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            E12S_Phase.DualApocalypse => E12S_Phase.AdvancedRelativity,
        };
    }
    
    [ScriptMethod(name: "阶段转换：三火", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:20509"], userControl: false)]
    public void 三火阶段转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            E12S_Phase.AdvancedRelativity => E12S_Phase.TripleApocalypse,
        };
    }
    
    [ScriptMethod(name: "阶段转换：软狂暴", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22755"], userControl: false)]
    public void 软狂暴阶段转换(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            E12S_Phase.TripleApocalypse => E12S_Phase.TerminalRelativity,
        };
    }
    
    #endregion
    
    #region 测试项目
    #endregion
    
    #region 机制画图 - 门神
    
    [ScriptMethod(name: "愉悦之手（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2270[12]$"])]
    public void 愉悦之手(Event @event, ScriptAccessory accessory)
    {
        var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
        var isR = aid == 22701;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "愉悦之手";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = float.Pi / 180 * 210;
        dp.Rotation = float.Pi / 180 * 90 * (isR ? -1 : 1);
        dp.DestoryAt = 4700;
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "制裁之界雷_元素雷钢铁", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:20039"])]
    public void 制裁之界雷(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "制裁之界雷";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "临时洋流_元素水两侧", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:20036"])]
    public void 临时洋流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "临时洋流";
        dp.Scale = new (20, 80f);
        dp.Owner = @event.SourceId();
        dp.Offset = new Vector3 (-15,0,15);
        dp.Rotation = 90f.DegToRad();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
    }
    
    [ScriptMethod(name: "凶猛风暴_元素风四边", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:20038"])]
    public void 凶猛风暴(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "凶猛风暴";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 45f.DegToRad();
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "瞬燃强袭_元素火前后", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:20037"])]
    public void 瞬燃强袭(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "瞬燃强袭";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 150f.DegToRad();
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    
    #endregion
    
    #region 机制画图 - 本体
    #endregion
    
    #region 机制提示 - 门神
    #endregion
    
    #region 机制提示 - 本体
    #endregion
    
    #region 减伤提示 - 门神
    #endregion
    
    #region 减伤提示 - 本体
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