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
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;


namespace PVPToy;

[ScriptType(guid: "2312052e-6575-4a91-9e3d-b84699dca8fe", name: "PVP小玩具", territorys: [250, 431, 554, 888, 1273],
    version: "0.0.0.2", author: "Tetora", note: noteStr)]

public class PVPToy
{
    const string noteStr =
        """
        v0.0.0.2:
        PVP小玩具，仅纷争前线可用，可在狼狱进行测试
        推荐先自行过一遍设置关闭不需要的功能，底裤功能使用后果自行承担
        标记一般是防四小，所以启用仅标记选项的话就是防四小的播报
        """;
    
    #region 基础控制
    
    [UserSetting("EdgeTTS开关")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("启用目标标记播报及连线")]
    public bool isTargetBroadcast { get; set; } = false;
    
    [UserSetting("启用自动选中目标标记")]
    public bool isAutoTarget { get; set; } = false;
    
    [UserSetting("启用仅适用敌方目标标记 [适用:占星诗人LB播报]" )]
    public bool isOnlyMark { get; set; } = false;
    
    [UserSetting("启用自动诗人光阴神净化")]
    public bool isAutoWarden { get; set; } = false;
    
    [UserSetting("启用被抓自动防御")]
    public bool isAutoGuard { get; set; } = false;
    
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
    
    private List<MarkType> checkMark = new List<MarkType>()
    {
        MarkType.Attack1,
        MarkType.Attack2,
        MarkType.Attack3,
        MarkType.Attack4,
        MarkType.Attack5,
        MarkType.Bind1,
        MarkType.Bind2,
        MarkType.Bind3,
        MarkType.Ignore1,
        MarkType.Ignore2,
        MarkType.Attack6,
        MarkType.Attack7,
        MarkType.Attack8,
    };
    
    #endregion
    
    #region 自动播报/执行
    
    public bool isPartyMember(ScriptAccessory accessory, uint SourceId)
    {
        return accessory.Data.PartyList.Contains(SourceId);
    }
    
    private bool PartyFilter(ScriptAccessory sa, IGameObject? obj)
    {
        if (obj == null || !obj.IsValid()) return false;
        
        if (obj.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player) return false;

        var targetPlayer = obj as IPlayerCharacter;
        if (targetPlayer == null) return false;
        
        bool isInMyAlliance = false;
        
        isInMyAlliance = sa.Data.PartyList?.Any(p => p == targetPlayer.EntityId) ?? false;
        
        if (!isInMyAlliance)
        {
            if (targetPlayer.StatusFlags.HasFlag(Dalamud.Game.ClientState.Objects.Enums.StatusFlags.AllianceMember))
            {
                isInMyAlliance = true;
            }
        }

        return isInMyAlliance;
    }
    
    [ScriptMethod(name: "被狙自动开盾", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29415"])]
    public void MarksmansTarget(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        if (ActionExt.IsSpellReady(29054) && !IbcHelper.HasStatus(accessory, accessory.Data.MyObject, 0xBEE))
        {
            if (isText) accessory.Method.TextInfo("已尝试自动使用《防御》", duration: 1800, true);
            accessory.Method.SendChat($"/pvpac 防御");
            accessory.Method.SendChat($"/e 侦测到被狙！鸭鸭试着帮你开了盾！<se.1> <se.1>");
        }
    }
    
    [ScriptMethod(name: "被战士抓自动防御", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29081"])]
    public void AutoGuardWhenDedication(Event @event, ScriptAccessory accessory)
    {
        // 献身 技能ID 29081 ； 附加减速 StatusID 1344 ； 生效间隔 约0.45s
        if (isAutoGuard)
        {
            if (@event.TargetId() != accessory.Data.Me) return;
            if (ActionExt.IsSpellReady(29054) && !IbcHelper.HasStatus(accessory, accessory.Data.MyObject, 0xBEE))
            {
                if (isText) accessory.Method.TextInfo("已尝试自动使用《防御》", duration: 1800, true);
                accessory.Method.SendChat($"/pvpac 防御");
                accessory.Method.SendChat($"/e 侦测到被战士抓了！鸭鸭试着帮你进行了防御！<se.3> <se.3>");
            }
        }
    }
    
    [ScriptMethod(name: "对方诗人LB播报", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29401"],suppress:3000)]
    public void FinalFantasiaTTS(Event @event, ScriptAccessory accessory)
    {
        // 英雄的幻想曲 ActionId:29401 ； 英雄的幻想曲（GCD缩短 30s） StatusID:3144； 英豪的幻想曲（周边30m内 加攻击加速度，LB增长，持续判定，每次5s） StatusID:3145 
        if (isOnlyMark) return;  // 未启用选项，播报对方全体，而非四小标记
        // 检测敌对状态
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj))
        {
            string tname = @event["TargetName"]?.ToString();
            if (isText) accessory.Method.TextInfo("检测到对方《诗人LB》", duration: 1800, true);
            if (isEdgeTTS)  accessory.Method.EdgeTTS("对方诗人LB");
            accessory.Method.SendChat($"/e 鸭鸭:检测到对方诗人LB！来源《{tname}》");
        }
    }
    
    [ScriptMethod(name: "对方标记诗人LB播报", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3144"],suppress:3000)]
    public void FinalFantasiaMarkTTS(Event @event, ScriptAccessory accessory)
    {
        if (!isOnlyMark) return; // 开启选项，只播报对方四小，采用有头顶标记的人获得 LB Buff时判断
        // 检测敌对状态
        var obj = IbcHelper.GetById(accessory, @event.SourceId); 
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj)) 
        {
            // 检测目标标记
            var tid = @event.TargetId(); 
            var tobj = IbcHelper.GetById(accessory, tid);
            if (tobj == null || !tobj.IsValid()) return;

            if (!IbcHelper.HasAnyMarker(tobj)) return;

            foreach (var mark in checkMark)
            {
                if (IbcHelper.HasMarker(tobj, mark))
                {
                    string tname = @event["TargetName"]?.ToString();
                    if (isText) accessory.Method.TextInfo("检测到对方《诗人LB》", duration: 1800, true);
                    if (isEdgeTTS)  accessory.Method.EdgeTTS("对方诗人LB");
                    accessory.Method.SendChat($"/e 鸭鸭:检测到标记对象获得诗人LB！目标《{tname}》");
                }
            }
        }
    }
    
    [ScriptMethod(name: "对方占星LB播报", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29255"],suppress:3000)]
    public void CelestialRiverTTS(Event @event, ScriptAccessory accessory)
    {
        // 星河漫天 ActionId:29255 ； 星河漫天（队友buff） StatusID:3105； 星河漫天（敌方debuff） StatusID:3106 
        if (isOnlyMark) return;  // 未启用选项，播报对方全体，而非四小标记
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj))
        {
            string tname = @event["TargetName"]?.ToString();
            if (isText) accessory.Method.TextInfo("检测到对方《占星LB》", duration: 1800, true);
            if (isEdgeTTS)  accessory.Method.EdgeTTS("对方占星LB");
            accessory.Method.SendChat($"/e 鸭鸭:检测到对方占星LB！来源《{tname}》");
        }
    }
    
    [ScriptMethod(name: "对方标记占星LB播报", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3105"],suppress:3000)]
    public void CelestialRiverMarkTTS(Event @event, ScriptAccessory accessory)
    {
        if (!isOnlyMark) return; // 开启选项，只播报对方四小，采用有头顶标记的人获得 LB Buff时判断
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj))
        {
            var tid = @event.TargetId();
            var tobj = IbcHelper.GetById(accessory, tid);
            if (tobj == null || !tobj.IsValid()) return;

            if (!IbcHelper.HasAnyMarker(tobj)) return;

            foreach (var mark in checkMark)
            {
                if (IbcHelper.HasMarker(tobj, mark))
                {
                    string tname = @event["TargetName"]?.ToString();
                    if (isText) accessory.Method.TextInfo("检测到对方《占星LB》", duration: 1800, true);
                    if (isEdgeTTS)  accessory.Method.EdgeTTS("对方占星LB");
                    accessory.Method.SendChat($"/e 鸭鸭:检测到标记对象获得占星LB！目标《{tname}》");
                }
            }
        }
    }
    
    [ScriptMethod(name: "被蛮荒崩裂播报", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29084"],suppress:2000)]
    public void PrimalRendTargetTTS(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isEdgeTTS)  accessory.Method.EdgeTTS("被晕了");
    }
    
    [ScriptMethod(name: "被蛮荒崩裂自动诗人净化", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29084"])]
    public void PrimalRendAutoWarden(Event @event, ScriptAccessory accessory)
    {
        // 蛮荒崩裂 技能ID 29084 ； 附加眩晕 StatusID 1343 ； 生效间隔 约1s
        if (isAutoWarden)
        {
            if (@event.TargetId() != accessory.Data.Me) return;
            if (ActionExt.IsSpellReady(29400) && !IbcHelper.HasStatus(accessory, accessory.Data.MyObject, 0xC47))
            {
                if (isText) accessory.Method.TextInfo("已尝试自动使用《光阴神净化》", duration: 1800, true);
                accessory.Method.SendChat($"/pvpac 光阴神的礼赞凯歌");
                accessory.Method.SendChat($"/e 侦测到成为蛮荒崩裂目标！鸭鸭试着帮你开了光阴神净化！<se.3> <se.3>");
            }
        }
    }
    
    [ScriptMethod(name: "被涤罪之心播报", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29230"],suppress:2000)]
    public void AfflatusPurgationTargetTTS(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isEdgeTTS)  accessory.Method.EdgeTTS("被晕了");
    }
    
    [ScriptMethod(name: "被涤罪之心自动诗人净化", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29230"])]
    public void AfflatusPurgationAutoWarden(Event @event, ScriptAccessory accessory)
    {
        // 涤罪之心 技能ID 29230 ； 附加眩晕 StatusID 1343 ； 生效间隔 约0.8s
        if (isAutoWarden)
        {
            if (@event.TargetId() != accessory.Data.Me) return;
            if (ActionExt.IsSpellReady(29400) && !IbcHelper.HasStatus(accessory, accessory.Data.MyObject, 0xC47))
            {
                if (isText) accessory.Method.TextInfo("已尝试自动使用《光阴神净化》", duration: 1800, true);
                accessory.Method.SendChat($"/pvpac 光阴神的礼赞凯歌");
                accessory.Method.SendChat($"/e 侦测到成为涤罪之心目标！鸭鸭试着帮你开了光阴神净化！<se.3> <se.3>");
            }
        }
    }
        
    #endregion
    
    #region 目标标记播报及连线
    
    //  方块 11 ； 圆圈 12 ； 十字 13 ； 三角 14 ； 止步1 06 ； 止步2 07 ； 止步3 08 ； 禁止1 09 ； 禁止2 10
    //  攻击1 01 ； 攻击2 02 ； 攻击3 03 ； 攻击4 04 ； 攻击5 05 ； 攻击6 15； 攻击7 16； 攻击8 17
    
    [ScriptMethod(name: "—————— 目标标记播报及连线 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 目标标记播报及连线(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "三角标记播报", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:14"])]
    public void TriangleMark(Event @event, ScriptAccessory accessory)
    {
        if(isTargetBroadcast && isEdgeTTS) accessory.Method.EdgeTTS("三角已标记");
        if(isTargetBroadcast) accessory.Method.SendChat($"/e 三角已标记<<targetclass>> 》 <<t>>");
    }
    
    [ScriptMethod(name: "三角标记连线", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:14"])]
    public void TriangleConnected(Event @event, ScriptAccessory accessory)
    {
        if(!isTargetBroadcast) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "三角标记连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = new Vector4(0f, 1f, 1f, 1);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1);
        dp.DestoryAt = 500;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "十字标记播报", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:13"])]
    public void CrossMark(Event @event, ScriptAccessory accessory)
    {
        if(isTargetBroadcast && isEdgeTTS) accessory.Method.EdgeTTS("十字已标记");
        if(isTargetBroadcast) accessory.Method.SendChat($"/e 十字已标记<<targetclass>> 》 <<t>>");
    }
    
    [ScriptMethod(name: "十字标记连线", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:13"])]
    public void CrossConnected(Event @event, ScriptAccessory accessory)
    {
        if(!isTargetBroadcast) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "十字标记连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = new Vector4(0f, 1f, 1f, 1);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1);
        dp.DestoryAt = 500;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "方块标记播报", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:11"])]
    public void SquareMark(Event @event, ScriptAccessory accessory)
    {
        if(isTargetBroadcast && isEdgeTTS) accessory.Method.EdgeTTS("方块已标记");
        if(isTargetBroadcast) accessory.Method.SendChat($"/e 方块已标记<<targetclass>> 》 <<t>>");
    }
    
    [ScriptMethod(name: "方块标记连线", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:11"])]
    public void SquareConnected(Event @event, ScriptAccessory accessory)
    {
        if(!isTargetBroadcast) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "方块标记连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = new Vector4(0f, 1f, 1f, 1);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1);
        dp.DestoryAt = 500;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "攻击1标记播报", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:01"])]
    public void Attack1Mark(Event @event, ScriptAccessory accessory)
    {
        if(isTargetBroadcast && isEdgeTTS) accessory.Method.EdgeTTS("攻击1已标记");
        if(isTargetBroadcast) accessory.Method.SendChat($"/e 攻击1已标记<<targetclass>> 》 <<t>>");
    }
    
    [ScriptMethod(name: "攻击1标记连线", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:01"])]
    public void Attack1Connected(Event @event, ScriptAccessory accessory)
    {
        if(!isTargetBroadcast) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "攻击1标记连线";
        dp.Owner = accessory.Data.Me;
        dp.Color = new Vector4(0f, 1f, 1f, 1);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1);
        dp.DestoryAt = 500;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    #endregion
    
    #region 自动选中目标标记
    [ScriptMethod(name: "—————— 自动选中目标标记 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 自动选中目标标记(Event @event, ScriptAccessory accessory) { }
    
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
    
    #region 底裤功能
    
    [ScriptMethod(name: "—————— 底裤功能 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 底裤功能(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "[DR] 冲天时更改移速（不与敏捷共存）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3180"])]
    public void 冲天Add(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (SkySpeed == SkySpeedEnum.Default)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed -1");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 冲天移速已恢复为默认值");
        }
        else if (SkySpeed == SkySpeedEnum.AddPoint1)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed 1.1");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 冲天移速已设置为1.1");
        }
        else if (SkySpeed == SkySpeedEnum.AddPoint2)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed 1.2");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 冲天移速已设置为1.2");
        }
        else if (SkySpeed == SkySpeedEnum.AddPoint3)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed 1.3");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 冲天移速已设置为1.3");
        }
        else if (SkySpeed == SkySpeedEnum.AddPoint4)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed 1.4");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 冲天移速已设置为1.4");
        }
        else if (SkySpeed == SkySpeedEnum.AddPoint5)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed 1.5");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 冲天移速已设置为1.5");
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
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 冲天移速已恢复为默认值");
        }
        else if (RemoveSpeed == RemoveSpeedEnum.Speed1)
        {
            if(isHack) accessory.Method.SendChat($"/pdrspeed 1");
            if(isHack && isDeveloper) accessory.Method.SendChat($"/e 冲天移速已恢复为1");
        }
        
    }
    
    #endregion
    
    #region 测试项目

    /*
    [ScriptMethod(name: "—————— 测试项目 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 测试项目(Event @event, ScriptAccessory accessory) { }
    
    
    [ScriptMethod(name: "[Debug] 头标检测", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:test"])]
    public void 头标检测(Event @event, ScriptAccessory accessory)
    {
        var j = IbcHelper.HasAnyMarker(accessory.Data.MyObject?.TargetObject);
        accessory.Log.Debug($"{j}");
    
        var x = IbcHelper.GetObjectMarker(accessory.Data.MyObject?.TargetObject);
        accessory.Log.Debug($"{x}");
        
        MarkType[] attackMarkers = {
            MarkType.Attack1, MarkType.Attack2, MarkType.Attack3, MarkType.Attack4,
            MarkType.Attack5, MarkType.Attack6, MarkType.Attack7, MarkType.Attack8,
            MarkType.Bind1, MarkType.Bind2, MarkType.Bind3, MarkType.Ignore1, MarkType.Ignore2
        };
    
        foreach (var markType in attackMarkers)
        {
            var hasMarker = IbcHelper.HasMarker(accessory.Data.MyObject?.TargetObject, markType);
            accessory.Log.Debug($"标记 {markType}: {hasMarker}");
        }
    }
    
    */
    
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
}

public static class ActionExt
{
    public static unsafe bool IsReadyWithCanCast(uint actionId, ActionType actionType)
    {
        var am = ActionManager.Instance();
        if (am == null) return false;

        var adjustedId = am->GetAdjustedActionId(actionId);

        // 0 = Ready）
        if (am->GetActionStatus(actionType, adjustedId) != 0)
            return false;

        ulong targetId = 0;
        var ts = TargetSystem.Instance();
        if (ts != null && ts->GetTargetObject() != null)
            targetId = ts->GetTargetObject()->GetGameObjectId();

        return am->GetActionStatus(actionType, adjustedId, targetId) == 0;
    }

    public static bool IsSpellReady(this uint spellId) => IsReadyWithCanCast(spellId, ActionType.Action);
    public static bool IsAbilityReady(this uint abilityId) => IsReadyWithCanCast(abilityId, ActionType.Ability);
}