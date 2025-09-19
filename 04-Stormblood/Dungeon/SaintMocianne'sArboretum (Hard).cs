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
using KodakkuAssist.Extensions;

namespace SaintMociannesArboretum_Hard;

[ScriptType(guid: "7e87b5d1-ae21-4115-9483-d8dc0f1d1652", name: "污染庭园圣茉夏娜植物园", territorys: [788],
    version: "0.0.0.4", author: "Tetora", note: noteStr)]

public class SaintMociannesArboretum_Hard
{
    const string noteStr =
        """
        v0.0.0.3:
        LV70 污染庭园圣茉夏娜植物园 初版绘制
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
    
    #region BOSS1_泥口花
    [ScriptMethod(name: "BOSS1_泥口花 泥浆炸弹（点名毒圈预测）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0001"])]
    public void 泥浆炸弹(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() == accessory.Data.Me && isTTS) accessory.Method.TTS("毒圈放置点名");
        if ( @event.TargetId() == accessory.Data.Me && isEdgeTTS) accessory.Method.EdgeTTS("毒圈放置点名");
            
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "泥浆炸弹";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1_泥口花 泥浆炸弹（毒圈提前显示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:11854"])]
    public void 泥浆炸弹毒圈(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "泥浆炸弹毒圈";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.Delay = 2700;
        dp.DestoryAt = 2800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "BOSS1_泥口花 地层穿刺（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:11850"])]
    public void 地层穿刺(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "地层穿刺";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    
    [ScriptMethod(name: "BOSS1_泥口花 捕食_恶意毒境", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:11855"])]
    public void 恶意毒境(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("躲在小花后", duration: 8000, true);
        if(isTTS) accessory.Method.TTS("躲在小花后");
        if(isEdgeTTS) accessory.Method.EdgeTTS("躲在小花后");
        
        foreach (var item in accessory.Data.Objects.GetByDataId(9264))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "恶意毒境";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = item.EntityId;
            dp.TargetPosition = new Vector3(0, 3, -82);
            dp.Scale = new Vector2(40);
            dp.Radian = 180f.DegToRad();
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
        }

    }
    #endregion
    
    [ScriptMethod(name: "小怪_绽放比洛克 咕噜咕噜灵", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12507"])]
    public void 咕噜咕噜灵(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"咕噜咕噜灵{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "咕噜咕噜灵销毁", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:12507"], userControl: false)]
    public void 咕噜咕噜灵销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"咕噜咕噜灵{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "咕噜咕噜灵销毁2", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(2|9|1608)$"], userControl: false)]
    public void 咕噜咕噜灵销毁2(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"咕噜咕噜灵{@event.SourceId()}");
    }
    
    #region BOSS2_拉哈穆
    // 12594 滑坡（本体） 12595 <<< 向左滑  / 12607 向右滑 >>>
    
    [ScriptMethod(name: "BOSS2_淤泥巨像 岩石崩溃（直线）", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:9258"])]
    public void 岩石崩溃(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "岩石崩溃";
        dp.Scale = new (10, 45f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.6f);
        dp.DestoryAt = 12000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS2_拉哈穆 大地摇动", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0028"])]
    public void 大地摇动(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        var boss = accessory.Data.Objects.GetByDataId(9257).FirstOrDefault();
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;
        dp.TargetObject = @event.TargetId();
        
        dp.Name = "大地摇动";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Scale = new Vector2(77);
        dp.Radian = 30f.DegToRad();
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    #endregion
    

    [ScriptMethod(name: "打开放水栓提示", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2009586", "Operate:Add", 
        "SourcePosition:{\"X\":287.36,\"Y\":-353.81,\"Z\":-230.91}"])]
    public void 放水栓(Event @event, ScriptAccessory accessory)
    {
        // if(isText) accessory.Method.TextInfo("打开放水栓", duration: 3000, false);
        // if(isTTS) accessory.Method.TTS("打开放水栓");
        // if(isEdgeTTS) accessory.Method.EdgeTTS("打开放水栓");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "放水栓";
        dp.Color = new Vector4(0f, 1f, 0f, 3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(2.04f);
        dp.InnerScale = new Vector2(2f);
        dp.Radian = 2 * float.Pi;
        dp.Offset = new Vector3 (0,1,0);
        dp.DestoryAt = 180000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "放水栓销毁", userControl: false,eventType: EventTypeEnum.ObjectEffect, 
        eventCondition: ["SourceName:regex:^(放水栓|water conduit)$", "Id2:2", "Id1:1","SourcePosition:{\"X\":287.36,\"Y\":-353.81,\"Z\":-230.91}"])]
    public void 放水栓销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("放水栓");
    }

    
    #region BOSS3_枯腐泥妖

    uint Tokkapchi = 0;
    public void Init(ScriptAccessory accessory) {
        Tokkapchi = 0;
    }
    [ScriptMethod(name: "BOSS3_枯腐泥妖 开场记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12597"],userControl: false)]
    public void 污泥猛抽(Event @event, ScriptAccessory accessory)
    {
        Tokkapchi = 1;
    }
    
    [ScriptMethod(name: "BOSS3_枯腐泥妖 污泥喷出", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id2:2", "Id1:1"])]
    public void 污泥喷出(Event @event, ScriptAccessory accessory)
    {
        if (Tokkapchi == 1)
        {
            if(isText) accessory.Method.TextInfo("站台子上", duration: 7500, false);
            if(isTTS) accessory.Method.TTS("站台子上");
            if(isEdgeTTS) accessory.Method.EdgeTTS("站台子上");
        }
        
    }
    
    [ScriptMethod(name: "BOSS3_枯腐泥妖 污泥", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12600"])]
    public void 污泥(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("分散，离开台子", duration: 4500, true);
        if(isTTS) accessory.Method.TTS("分散，离开台子");
        if(isEdgeTTS) accessory.Method.EdgeTTS("分散，离开台子");
    }
    
    [ScriptMethod(name: "BOSS3_污泥粘液怪 提示", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:9262"])]
    public void 污泥粘液怪(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("将粘液怪推到台子上，并远离任何伤害", duration: 5000, true);
        if(isTTS) accessory.Method.TTS("将小怪推到台子上，并远离任何伤害");
        if(isEdgeTTS) accessory.Method.EdgeTTS("将小怪推到台子上，并远离任何伤害");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "污泥粘液怪";
        dp.Color = new Vector4(1f, 1f, 0f, 6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.1f);
        dp.InnerScale = new Vector2(1.0f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "BOSS3_污泥粘液怪 炸裂&腐爆", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(1319[67]|13216)$"])]
    public void 腐爆(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("污泥粘液怪");
        
        switch (@event.ActionId())
        {
            case 13196:
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "炸裂";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = @event.SourceId();
                dp.Scale = new Vector2(6f);
                dp.DestoryAt = 2700;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                break;
            
            case 13197:
                if(isText) accessory.Method.TextInfo("大AOE伤害，注意减伤&盾", duration: 2500, false);
                if(isTTS) accessory.Method.TTS("大AOE伤害，注意减伤与盾");
                if(isEdgeTTS) accessory.Method.EdgeTTS("大AOE伤害，注意减伤与盾");
                break;
            
            case 13216:
                if(isText) accessory.Method.TextInfo("超大AOE伤害，给出全部场地减&盾", duration: 2500, true);
                if(isTTS) accessory.Method.TTS("超大AOE伤害，给出全部减伤盾");
                if(isEdgeTTS) accessory.Method.EdgeTTS("超大AOE伤害，给出全部减伤盾");
                
                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = "腐爆";
                dp2.Color = new Vector4(1f, 0f, 0f, 1f);
                dp2.Owner = @event.SourceId();
                dp2.Scale = new Vector2(60f);
                dp2.DestoryAt = 2700;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);
                break;
        }
    }
    
    [ScriptMethod(name: "BOSS3_枯腐泥妖 污泥泼洒", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12604"])]
    public void 污泥泼洒(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("将粘液怪推至安全区", duration: 7000, true);
        if(isTTS) accessory.Method.TTS("将小怪推至安全区");
        if(isEdgeTTS) accessory.Method.EdgeTTS("将小怪推至安全区");
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

#region 计算函数

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

#endregion 计算函数