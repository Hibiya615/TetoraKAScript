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

namespace PVPToy;

[ScriptType(guid: "2312052e-6575-4a91-9e3d-b84699dca8fe", name: "PVP小玩具", territorys: [250, 431, 554, 888, 1273],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class PVPToy
{
    const string noteStr =
        """
        v0.0.0.1:
        PVP小玩具，仅纷争前线可用
        可在狼狱进行测试
        """;
    
    #region 基础控制
    
    [UserSetting("启用目标标记播报")]
    public bool isTargetTTS { get; set; } = false;
    
    [UserSetting("启用自动选中目标标记")]
    public bool isAutoTarget { get; set; } = false;
    
    [UserSetting("启用自动净化")]
    public bool isAutoPurify { get; set; } = false;
    
    [UserSetting("请确认你已经有相关插件与对应权限")]
    public bool isHack { get; set; } = false;
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;
    
    [UserSetting(note: "请选择冲天时的移速")]
    public SkySpeedEnum SkySpeed { get; set; } = SkySpeedEnum.Default;
    
    public enum SkySpeedEnum
    {
        Default = 0,
        AddPoint1 = 1,
        AddPoint2 = 2,
        AddPoint3 = 3,
        AddPoint4 = 4,
        AddPoint5 = 5,
    }
    
    [UserSetting(note: "请选择复原时的移速")]
    public RemoveSpeedEnum RemoveSpeed { get; set; } = RemoveSpeedEnum.Speed1;
    
    public enum RemoveSpeedEnum
    {
        Default = 0,
        Speed1 = 1,
    }
    
    #endregion
    
    [ScriptMethod(name: "被狙自动开盾", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29415"],suppress:5000)]
    public void MarksmansTarget(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pvpac 防御");
        accessory.Method.SendChat($"/e 侦测到被狙！鸭鸭试着帮你开了盾！<se.1> <se.1>");
    }
    
    [ScriptMethod(name: "被蛮荒崩裂播报", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29084"],suppress:2000)]
    public void PrimalRendTargetTTS(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.EdgeTTS("被晕了");
    }
    
    [ScriptMethod(name: "被蛮荒崩裂自动诗人净化", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29084"],suppress:25000)]
    public void PrimalRendAutoPurify(Event @event, ScriptAccessory accessory)
    {
        // 蛮荒崩裂 技能ID 29084 ； 附加眩晕 StatusID 1343 ； 生效间隔 约1s
        if (isAutoPurify)
        {
            if (@event.TargetId() != accessory.Data.Me) return; 
            accessory.Method.SendChat($"/pvpac 光阴神的礼赞凯歌");
            accessory.Method.SendChat($"/e 侦测到成为蛮荒崩裂目标！鸭鸭试着帮你开了光阴神净化！<se.3> <se.3>");
        }
    }
    
    [ScriptMethod(name: "被涤罪之心播报", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29230"],suppress:2000)]
    public void AfflatusPurgationTargetTTS(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.EdgeTTS("被晕了");
    }
    
    [ScriptMethod(name: "被涤罪之心自动诗人净化", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29230"],suppress:25000)]
    public void AfflatusPurgationAutoPurify(Event @event, ScriptAccessory accessory)
    {
        // 涤罪之心 技能ID 29230 ； 附加眩晕 StatusID 1343 ； 生效间隔 约0.8s
        if (isAutoPurify)
        {
            if (@event.TargetId() != accessory.Data.Me) return;
            accessory.Method.SendChat($"/pvpac 光阴神的礼赞凯歌");
            accessory.Method.SendChat($"/e 侦测到成为涤罪之心目标！鸭鸭试着帮你开了光阴神净化！<se.3> <se.3>");
        }
    }
    
    #region 目标标记播报
    
    //  方块 11 ； 圆圈 12 ； 十字 13 ； 三角 14 ； 止步1 06 ； 止步2 07 ； 止步3 08 ； 禁止1 09 ； 禁止2 10
    //  攻击1 01 ； 攻击2 02 ； 攻击3 03 ； 攻击4 04 ； 攻击5 05 ； 攻击6 15； 攻击7 16； 攻击8 17
    
    [ScriptMethod(name: "三角标记播报", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:14"])]
    public void TriangleMark(Event @event, ScriptAccessory accessory)
    {
        if(isTargetTTS) accessory.Method.EdgeTTS("三角已标记");
        if(isTargetTTS) accessory.Method.SendChat($"/e 三角已标记<<targetclass>> 》 <<t>>");
    }
    
    [ScriptMethod(name: "十字标记播报", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:13"])]
    public void CrossMark(Event @event, ScriptAccessory accessory)
    {
        if(isTargetTTS) accessory.Method.EdgeTTS("十字已标记");
        if(isTargetTTS) accessory.Method.SendChat($"/e 十字已标记<<targetclass>> 》 <<t>>");
    }
    
    [ScriptMethod(name: "方块标记播报", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:11"])]
    public void SquareMark(Event @event, ScriptAccessory accessory)
    {
        if(isTargetTTS) accessory.Method.EdgeTTS("方块已标记");
        if(isTargetTTS) accessory.Method.SendChat($"/e 方块已标记<<targetclass>> 》 <<t>>");
    }
    
    [ScriptMethod(name: "攻击1标记播报", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:01"])]
    public void Attack1Mark(Event @event, ScriptAccessory accessory)
    {
        if(isTargetTTS) accessory.Method.EdgeTTS("攻击1已标记");
        if(isTargetTTS) accessory.Method.SendChat($"/e 攻击1已标记<<targetclass>> 》 <<t>>");
    }
    
    #endregion
    
    #region 自动选中目标标记
    
    [ScriptMethod(name: "自动焦点大饼", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:12"])]
    public void CircleTarget(Event @event, ScriptAccessory accessory)
    {
        // if(isAutoTarget) accessory.Method.SendChat($"/target <circle>");
        if(isAutoTarget) accessory.Method.SendChat($"/focustarget <circle>");
        if(isAutoTarget) accessory.Method.SendChat($"/e 已自动焦点大饼 <<targetclass>> 》 <<t>>");
    }
    
    [ScriptMethod(name: "自动选中三角", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:14"])]
    public void TriangleTarget(Event @event, ScriptAccessory accessory)
    {
        if(isAutoTarget) accessory.Method.SendChat($"/target <triangle>");
        if(isAutoTarget) accessory.Method.SendChat($"/e 三角已选中<<targetclass>> 》 <<t>>");
    }
    
    [ScriptMethod(name: "自动选中十字", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:13"])]
    public void CrossTarget(Event @event, ScriptAccessory accessory)
    {
        if(isAutoTarget) accessory.Method.SendChat($"/target <cross>");
        if(isAutoTarget) accessory.Method.SendChat($"/e 十字已选中<<targetclass>> 》 <<t>>");
    }
    
    [ScriptMethod(name: "自动选中方块", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:11"])]
    public void SquareTarget(Event @event, ScriptAccessory accessory)
    {
        if(isAutoTarget) accessory.Method.SendChat($"/target <square>");
        if(isAutoTarget) accessory.Method.SendChat($"/e 方块已选中<<targetclass>> 》 <<t>>");
    }
    
    [ScriptMethod(name: "自动选中攻击1", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:01"])]
    public void Attack1Target(Event @event, ScriptAccessory accessory)
    {
        if(isAutoTarget) accessory.Method.SendChat($"/target <attack1>");
        if(isAutoTarget) accessory.Method.SendChat($"/e 攻击1已选中<<targetclass>> 》 <<t>>");
    }
    #endregion
    
    
    [ScriptMethod(name: "[DR] 冲天时更改移速（不与敏捷共存）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3180"])]
    public void 冲天Add(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (SkySpeed == SkySpeedEnum.Default)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed -1");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 移速已恢复为默认值");
        }
        else if (SkySpeed == SkySpeedEnum.AddPoint1)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed 1.1");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 移速已设置为1.1");
        }
        else if (SkySpeed == SkySpeedEnum.AddPoint2)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed 1.2");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 移速已设置为1.2");
        }
        else if (SkySpeed == SkySpeedEnum.AddPoint3)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed 1.3");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 移速已设置为1.3");
        }
        else if (SkySpeed == SkySpeedEnum.AddPoint4)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed 1.4");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 移速已设置为1.4");
        }
        else if (SkySpeed == SkySpeedEnum.AddPoint5)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed 1.5");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 移速已设置为1.5");
        }
    }
    
    [ScriptMethod(name: "[DR] 恢复冲天移速", eventType: EventTypeEnum.Targetable, eventCondition: ["DataId:0", "Targetable:True"])]
    public void SpeedRestore1(Event @event, ScriptAccessory accessory)
    {
        // 考虑到可能会在空中死亡 没法正常判定 ActionEffect，所以使用自己的 Targetable 代替
        if (@event.SourceId() != accessory.Data.Me) return; 
        
        if (RemoveSpeed == RemoveSpeedEnum.Default)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed -1");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 移速已恢复为默认值");
        }
        else if (RemoveSpeed == RemoveSpeedEnum.Speed1)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed 1");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 移速已恢复为1");
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