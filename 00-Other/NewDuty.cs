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

[ScriptType(guid: "80890eac-4730-4708-ad1b-05aba469c2a1", name: "最新最热临时绘制", territorys: [1307, 1346],
    version: "0.0.1.1", author: "Tetora", note: noteStr)]

/* MapID
 * 1307: 格莱杨拉波尔歼灭战
 * 1346：新月岛北征
 */

public class NewDuty
{
    const string noteStr =
        """
        v0.0.1.1:
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
    
    [UserSetting("下一次AOE预兆颜色")]
    public ScriptColor Next_AOEs { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
    
    [UserSetting("下一次AOE预兆亮度（推荐小于1）")]
    public float Next_AOEsBrightness { get; set; } = 0.5f;
    
    // dp.Color = Next_AOEs.V4.WithW(Next_AOEsBrightness);
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;

    #endregion
    
    #region 全局销毁
    
    [ScriptMethod(name: "咏唱中断销毁", eventType: EventTypeEnum.CancelAction, eventCondition: [], userControl: false)]
    public void 咏唱中断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "死亡销毁", eventType: EventTypeEnum.Death, eventCondition: [], userControl: false)]
    public void 死亡销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }
    
    #endregion
    
    #region 蜃景幻界新月岛 北征之章

    [ScriptMethod(name: "—————— 蜃景幻界新月岛 北征之章 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 蜃景幻界新月岛北征之章(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "[FATE] 忍耐基路伯_圆形魔法剑（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5011[89]$"])]
    public void 圆形魔法剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"圆形魔法剑";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[FATE] 忍耐基路伯_环形魔法剑（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5012[01]$"])]
    public void 环形魔法剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"环形魔法剑";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.InnerScale = new Vector2(12f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[FATE] 邪瞳_诅咒目光（背对范围）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47152$"])]
    public void 邪瞳_诅咒目光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"邪瞳_诅咒目光{@event.SourceId}";
        dp.Color = new Vector4(1f, 0f, 0f, 10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(49.8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[FATE] 忍耐基路伯_神圣（步进地火）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49913$"])]
    public void 忍耐基路伯_神圣(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"忍耐基路伯_神圣";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 诅咒的继承者——惨白魔人] 咒力的替身_臭气（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49777$"])]
    public void 咒力的替身_臭气 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"咒力的替身_臭气";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.Radian = 100f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 诅咒的继承者——惨白魔人] 咒力的替身_平原震裂（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49779$"])]
    public void 咒力的替身_平原震裂(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"咒力的替身_平原震裂";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 天道好轮回——魔亡灵法师] 古代探险家的亡灵_爆炸（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47175$"])]
    public void 古代探险家的亡灵_爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"古代探险家的亡灵_爆炸";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 天道好轮回——魔亡灵法师] 古代海贼的亡灵_爆炸（十字）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47176$"])]
    public void 古代海贼的亡灵_爆炸(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"古代海贼的亡灵_爆炸";
        dp.Scale = new (7f, 80f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.DestoryAt = 3700;
        
        float[] rotations = { 0f, 90f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    
    [ScriptMethod(name: "[CE 天道好轮回——魔亡灵法师] 魔法阵展开_昏暗奔流（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47180$"])]
    public void 魔法阵展开_昏暗奔流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"魔法阵展开_昏暗奔流";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new (12f, 70f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
    }
    
    [ScriptMethod(name: "[CE 禁书化形——古术魔典] 魔力书写_草书（步进地火）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47306$"])]
    public void 魔力书写_草书(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"魔力书写_草书{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new (6f, 50f);
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
    }
    
    [ScriptMethod(name: "[CE 苏醒的多头龙——魔许德拉] 近远冲击波_元素冲击波（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47199|4720[0123])$"])]
    public void 元素冲击波(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"元素冲击波{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 四颚斧花——提蔛] 中央鞭打（辣尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47220$"])]
    public void 中央鞭打(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"中央鞭打";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new (10f, 52f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
    }
    
    [ScriptMethod(name: "[CE 四颚斧花——提蔛] 侧方鞭打（辣翅）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47221|49729)$"])]
    public void 侧方鞭打(Event @event, ScriptAccessory accessory)
    {
        var isR = @event.ActionId == 47221;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"侧方鞭打{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(26f);
        dp.Radian = 180f.DegToRad(); 
        dp.Offset = isR ? new Vector3(5f, 0f, 0f) : new Vector3(-5f, 0f, 0f);
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
    }
    
    [ScriptMethod(name: "[CE 四颚斧花——提蔛] 毒雾喷射（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47228|5054[789])$"])]
    public void 毒雾喷射(Event @event, ScriptAccessory accessory)
    {
        // 猜测：50547 左 ; 50548 下
        // 已验证：47228 正 ; 50549 右
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"毒雾喷射{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 90f.DegToRad(); 
        switch (@event.ActionId())
        {
            case 47228:
                dp.Rotation = 0f.DegToRad();
                break;
            case 50547:
                dp.Rotation = 90f.DegToRad();
                break;
            case 50548:
                dp.Rotation = 180f.DegToRad();
                break;
            case 50549:
                dp.Rotation = 270f.DegToRad();
                break;
        }
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 纯白守护者——雪石膏之剑] 称誉（四连攻击命令_顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47158$"])]
    public void 称誉 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"称誉{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 纯白守护者——雪石膏之剑] 连续斩（二连左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4716[67]$"])]
    public void 连续斩(Event @event, ScriptAccessory accessory)
    {
        var isR = @event.ActionId == 47166;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"连续斩1";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"连续斩2";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(40f);
        dp1.Radian = 180f.DegToRad(); 
        dp1.Rotation = isR ? 90f.DegToRad() : 270f.DegToRad();
        dp1.Delay = 4700;
        dp1.DestoryAt = 2300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }

    [ScriptMethod(name: "[CE 暴食咒鬼——阿尔戈尔] 捕食（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50469$"])]
    public void 捕食 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"捕食";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 6500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "[CE 暴食咒鬼——阿尔戈尔] 吸引（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48104$"])]
    public void 吸引 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"吸引";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 30f.DegToRad(); 
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 暴食咒鬼——阿尔戈尔] 洋葱瘴气（顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48112$"])]
    public void 洋葱瘴气 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"洋葱瘴气";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 30f.DegToRad(); 
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 暴食咒鬼——阿尔戈尔] 番茄瘴气（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48111$"])]
    public void 番茄瘴气(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"番茄瘴气";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (6f, 50f);
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
    }
    
    [ScriptMethod(name: "[CE 魔女复制体——卡洛菲斯提莉二重身] 双重魔发斩（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5069[12]$"])]
    public void 双重魔发斩(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"双重魔发斩{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 180f.DegToRad(); 
        dp.Delay = @event.ActionId() == 50691 ? 0 : 2500;
        dp.DestoryAt = @event.ActionId() == 50691 ? 2500 : 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "[CE 魔女复制体——卡洛菲斯提莉二重身] 发牢（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47072$"])]
    public void 发牢(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"发牢{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 魔女复制体——卡洛菲斯提莉二重身] 剪发（冰花）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4707[67]$"])]
    public void 剪发(Event @event, ScriptAccessory accessory)
    {
        if (@event.ActionId() == 47076)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"剪发钢铁 {@event.SourceId}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(10f);
            dp.DestoryAt = 4700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        else if  (@event.ActionId() == 47077)
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"剪发冰花{@event.SourceId}";
            dp1.Scale = new (4f, 120f);
            dp1.Owner = @event.SourceId();
            dp1.Color = accessory.Data.DefaultDangerColor.WithW(1f);
            dp1.DestoryAt = 4700;
        
            float[] rotations = { 0f, 90f };
    
            foreach (float rotation in rotations)
            {
                dp1.Rotation = rotation.DegToRad();
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);
            }
        }
        else
        {
            
        }

    }
    
    [ScriptMethod(name: "[CE 叛逆使魔——负隅宝石兽] 利爪凶尾（前后刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48294$"])]
    public void 利爪凶尾 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"利爪凶尾_前";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(45f);
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"利爪凶尾_后";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(45f);
        dp1.Rotation = 180f.DegToRad();
        dp1.Radian = 180f.DegToRad();
        dp1.Delay = 5700;
        dp1.DestoryAt = 3300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }
    
    [ScriptMethod(name: "[CE 叛逆使魔——负隅宝石兽] 凶尾利爪（后前刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48295$"])]
    public void 凶尾利爪 (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"凶尾利爪_前";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(45f);
        dp.Rotation = 180f.DegToRad();
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"凶尾利爪_后";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(45f);
        dp1.Radian = 180f.DegToRad();
        dp1.Delay = 5700;
        dp1.DestoryAt = 3300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }
    
    [ScriptMethod(name: "[CE 拟态使魔——变形法师] 圆火（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48341$"])]
    public void 圆火(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"圆火";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 拟态使魔——变形法师] 旋风环（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48354$"])]
    public void 变形法师_旋风环(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"变形法师_旋风环";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.InnerScale = new Vector2(10f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[CE 孤岛的绑架犯——诱拐魔] 天降（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47448$"])]
    public void 天降(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"远离");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"远离");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"天降";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 5200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 孤岛的绑架犯——诱拐魔] 旋风环（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47449$"])]
    public void 诱拐魔_旋风环(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"靠近");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"靠近");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"诱拐魔_旋风环";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[CE 孤岛的绑架犯——诱拐魔] 散羽（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47443$"])]
    public void 散羽(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"散羽";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[CE 孤岛的绑架犯——诱拐魔] 突风（击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48250$"])]
    public void 突风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "突风";
        dp.Scale = new(1f, 24);
        dp.Color = new Vector4(0f, 1f, 1f, 4f);
        dp.Owner = accessory.Data.Me;
        dp.Rotation = @event.SourceRotation();
        dp.FixRotation = true;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    /* 歪到姥姥家了
    [ScriptMethod(name: "[CE 孤岛的绑架犯——诱拐魔] 撕裂之风（移动冰花）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^01FA$"])]
    public void 撕裂之风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"撕裂之风{@event.SourceId}";
        dp.Scale = new (8f, 120f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.DestoryAt = 5100;
        
        float[] rotations = { 0f, 45f, 90f, 135f};
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    */
    
    [ScriptMethod(name: "[CE 孤岛的绑架犯——诱拐魔] 撕裂之风（移动冰花）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47439$"])]
    public void 撕裂之风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"撕裂之风{@event.SourceId}";
        dp.Scale = new (8f, 120f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.DestoryAt = 700;
        
        float[] rotations = { 0f, 90f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }

    
    #endregion
    
    #region 两岐塔：魔之塔
    
    [ScriptMethod(name: "—————— 两岐塔：超魔之塔 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void 两岐塔超魔之塔(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "BOSS1 蓝头_冰柱赋格（面前钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47641$"])]
    public void 蓝头_冰柱赋格(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"蓝头_冰柱赋格";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition;
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1 蓝头_剧毒吐息（中心钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47639$"])]
    public void 蓝头_剧毒吐息(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"蓝头_剧毒吐息";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition;
        dp.Scale = new Vector2(18f);
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1 绿头_风暴吐息（中心击退）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^48245$"])]
    public void 绿头_风暴吐息(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"击退至安全区", duration: 8000, true);
        if (isTTS)accessory.Method.TTS($"中间击退");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"中间击退");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "绿头_风暴吐息击退预测";
        dp.Scale = new(1f, 14f);
        dp.Color = new Vector4(0f, 1f, 1f, 4f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "BOSS1 绿头_雷电赋格（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47640$"])]
    public void 绿头_雷电赋格(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"月环靠近", duration: 8000, true);
        if (isTTS)accessory.Method.TTS($"靠近");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"靠近");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"绿头_雷电赋格";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(18f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "BOSS1 双头怪鸟_雷霜暴风雨 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47739$"])]
    public void 提示(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"AOE", duration: 4000, false);
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "BOSS1 绿头_冰焰凝环（月环安全区）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47686$"])]
    public void 绿头_冰焰凝环(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        
        dp.Name = $"绿头_冰焰凝环安全区描边";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.InnerScale = new Vector2(4.9f);
        dp.Radian = float.Pi * 2;
        dp1.DestoryAt = dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        dp1.Name = "绿头_冰焰凝环连线";
        dp1.Owner = accessory.Data.Me;
        dp1.Color = accessory.Data.DefaultSafeColor;
        dp1.ScaleMode |= ScaleMode.YByDistance;
        dp1.TargetObject = @event.SourceId();
        dp1.Scale = new(1f);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
    }
    
    [ScriptMethod(name: "BOSS1 蓝头_冰焰交错（十字)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^47685$"])]
    public void 蓝头_冰焰交错(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"蓝头_冰焰交错";
        dp.Scale = new (10f, 70f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.DestoryAt = 1700;
        
        float[] rotations = { 0f, 90f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    
    [ScriptMethod(name: "BOSS1 立体魔法阵_魔阵光（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4772[12]$"])]
    public void 立体魔法阵_魔阵光(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"立体魔法阵_魔阵光";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new (5f, 60f);
        dp.DestoryAt = 700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
    }
    
    [ScriptMethod(name: "BOSS1 绿头_冰雷簇（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50(699|700)$"])]
    public void 冰雷簇(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"冰雷簇{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 17000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1 绿头_冰雷簇2（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5070[12]$"])]
    public void 冰雷簇2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"冰雷簇2{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 1000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region  7.4 格莱杨拉波尔歼灭战
    
    [ScriptMethod(name: "—————— 7.4 格莱杨拉波尔歼灭战 ——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
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
