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