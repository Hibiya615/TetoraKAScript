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
using Newtonsoft.Json;

namespace TetoraKodakkuScript.Field_Operations.Bozja;

[ScriptType(guid: "f3c31234-9daf-4a1d-81f2-2022c7e20277", name: "南方博兹雅战线", territorys: [920, 975],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class Bozja
{
    const string noteStr =
        """
        v0.0.0.1:
        南方博兹雅战线 & 扎杜诺尔高原 初版绘制
        未完成，缓慢更新中（也可能不更）
        """;
    
    #region 用户控制

    [UserSetting("TTS开关")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;

    #endregion

    #region 全局变量
    
    private ulong LimitCutTarget1 = 0; // 004F
    private ulong LimitCutTarget2 = 0; // 0050
    private ulong LimitCutTarget3 = 0; // 0051
    private ulong LimitCutTarget4 = 0; // 0052

    private List<(int id1, int id2, Vector3 position)> _objectEffectRecords;
    private int _currentRecordIndex;
    private int _jumpCount = 0;
    private float _dawonRotation = 0f;
    private bool _isFirstJumpDrawn = false;
    private int _pendingCrossIndex = -1;
    private string _pendingCrossName = "";
    
    public Bozja()
    {
        _objectEffectRecords = new List<(int id1, int id2, Vector3 position)>();
        _currentRecordIndex = 0;
        _jumpCount = 0;
        _dawonRotation = 0f;
        _pendingCrossIndex = -1;
        _pendingCrossName = "";
    }

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
    
    #region 高原CE - 赫德提特
    
    [ScriptMethod(name: "[高原CE-赫德提特] 水晶针（死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^24085$"])]
    public void 水晶针(Event @event, ScriptAccessory accessory)
    {
        if (isText && @event.TargetId() == accessory.Data.Me) accessory.Method.TextInfo($"坦克死刑点名", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"坦克死刑");
    }
    
    [ScriptMethod(name: "[高原CE-赫德提特] 碎片打击（分散）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^24086$"])]
    public void 碎片打击(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"分散");
    }
    
    private float[] _HailfiredestroyTimes = new float[] { 8.4f, 10.6f, 12.8f, 15.1f }; // 对应冰雹发射每个麻将的销毁时间

    [ScriptMethod(name: "[高原CE-赫德提特] 冰雹发射（麻将捕获）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^00(4F|5[012])$"], userControl: false)]
    public void 冰雹发射捕获(Event @event, ScriptAccessory accessory)
    {
        var iconId = @event.IconId();
        var targetId = @event.TargetId;

        if (iconId == 0x004F)
        {
            LimitCutTarget1 = targetId;
        }
        else if (iconId == 0x0050)
        {
            LimitCutTarget2 = targetId;
        }
        else if (iconId == 0x0051)
        {
            LimitCutTarget3 = targetId;
        }
        else if (iconId == 0x0052)
        {
            LimitCutTarget4 = targetId;
        }
    }
    
    [ScriptMethod(name: "[高原CE-赫德提特] 冰雹发射（麻将直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^24088$"])]
    public void 冰雹发射(Event @event, ScriptAccessory accessory)
    {
        DrawTarget(accessory, @event, LimitCutTarget1, 1, 8.4f);
        DrawTarget(accessory, @event, LimitCutTarget2, 2, 10.6f);
        DrawTarget(accessory, @event, LimitCutTarget3, 3, 12.8f);
        DrawTarget(accessory, @event, LimitCutTarget4, 4, 15.1f);
    
        // 绘制完成后清空所有目标
        LimitCutTarget1 = 0;
        LimitCutTarget2 = 0;
        LimitCutTarget3 = 0;
        LimitCutTarget4 = 0;
        
        void DrawTarget(ScriptAccessory accessory, Event @event, ulong target, int index, float destroyTime)
        {
            if (target == 0) return;
    
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"冰雹发射{index}_{target:X8}";
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
            dp.Owner = @event.SourceId();
            dp.TargetObject = target;
            dp.Scale = new(4f, 40f);
            dp.DestoryAt = (uint)(destroyTime * 1000); // 转换为毫秒
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
    }
    
    [ScriptMethod(name: "[高原CE-赫德提特] 水晶刺", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^24077$"])]
    public void 水晶刺(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"躲在白色水晶后", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"躲在白色水晶后");
        foreach (var item in accessory.Data.Objects.GetByDataId(12749))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"褪色水晶";
            dp.Color = accessory.Data.DefaultSafeColor.WithW(2f);
            dp.Owner = item.EntityId;
            dp.Scale = new Vector2(1.6f);
            dp.DestoryAt = 4700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "[高原CE-赫德提特] 以太刺", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^24078$"])]
    public void 以太刺(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"躲在黄色水晶后", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"躲在黄色水晶后");
        foreach (var item in accessory.Data.Objects.GetByDataId(12750))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"偏属性水晶";
            dp.Color = accessory.Data.DefaultSafeColor.WithW(2f);
            dp.Owner = item.EntityId;
            dp.Scale = new Vector2(1.6f);
            dp.DestoryAt = 4700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "[高原CE-赫德提特] 沙块_地层俯冲（圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^24080$"])]
    public void 地层俯冲(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"地层俯冲{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region 高原单挑 - 兽王莱昂

    [ScriptMethod(name: "[高原单挑-兽王莱昂] 炎帝热波冲（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^23865$"])]
    public void 炎帝热波冲(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
    }
    
    [ScriptMethod(name: "[高原单挑-兽王莱昂] 炎帝热气烧（驱散提示）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^23864$"])]
    public void 炎帝热气烧(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"准备驱魔", duration: 3000, true);
        if (isTTS)accessory.Method.TTS($"准备驱魔");
    }

    #endregion

    #region 48 - 旗舰达尔里阿达号攻略战

    [ScriptMethod(name: "————————  [高原] 旗舰达尔里阿达号攻略战  ————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^$"])]
    public void 旗舰达尔里阿达号攻略战(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "[BOSS1 上_铁胆狱火 萨托瓦尔] 火动（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^24189$"])]
    public void 火动(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
    }
    
    [ScriptMethod(name: "[BOSS1 上_铁胆狱火 萨托瓦尔] 时空地火喷发", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2417[4-7]$"])]
    public void 时空地火喷发(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"时空地火喷发{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new (20f, 20f);
        switch (@event.ActionId())
        {
            case 24174:  // 普通 - 先
                dp.Delay = 0;
                dp.DestoryAt = 4700;
                break;
            case 24175:  // 普通 - 后
                dp.Delay = 4700;
                dp.DestoryAt = 2000;
                break;
            case 24176:  // 逆转 - 后
                dp.Delay = 4700;
                dp.DestoryAt = 2000;
                break;
            case 24177:  // 逆转 - 先
                dp.Delay = 0;
                dp.DestoryAt = 4700;
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
    }
    
    [ScriptMethod(name: "[BOSS1 上_呼玛] 烈焰俯冲（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^24179$"])]
    public void 烈焰俯冲(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"烈焰俯冲{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (5f, 55f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "[BOSS1 上_铁胆狱火 萨托瓦尔] 燃烧之刃（死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^24208$"])]
    public void 燃烧之刃(Event @event, ScriptAccessory accessory)
    {
        if (isText && @event.TargetId() == accessory.Data.Me) accessory.Method.TextInfo($"坦克死刑点名", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"坦克死刑");
    }

    [ScriptMethod(name: "[BOSS1 上_铁胆狱火 萨托瓦尔] 曼托瓦之炎（AOE+引导）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^24200$"])]
    public void 曼托瓦之炎引导(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"AOE，集合诱导", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"AOE，集合诱导");
    }

    [ScriptMethod(name: "[BOSS1 上_铁胆狱火 萨托瓦尔] 曼托瓦之炎（钢铁+直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2420[12]$"])]
    public void 曼托瓦之炎(Event @event, ScriptAccessory accessory)
    {
        switch (@event.ActionId())
        {
            case 24201: // 直线
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"曼托瓦之炎直线{@event.SourceId}";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = @event.SourceId();
                dp.Scale = new (8f, 50f);
                dp.DestoryAt = 4700;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
                break;
            case 24202: // 钢铁
                var dp1 = accessory.Data.GetDefaultDrawProperties();
                dp1.Name = $"曼托瓦之炎钢铁{@event.SourceId}";
                dp1.Color = accessory.Data.DefaultDangerColor;
                dp1.Owner = @event.SourceId();
                dp1.Scale = new Vector2(10f);
                dp1.DestoryAt = 4700;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
                break;

        }
    }
    
    [ScriptMethod(name: "[BOSS1 上_铁胆狱火 萨托瓦尔] 左印/右印（左右刀）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2420[34]$"])]
    public void 左右印(Event @event, ScriptAccessory accessory)
    {
        var isR = @event.ActionId == 24203;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = isR ? $"右印{@event.SourceId}" : $"左印{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    uint MyPyrocrisis=0;
    
    public void Init(ScriptAccessory accessory) {
        MyPyrocrisis=0; 
    }
    
    [ScriptMethod(name: "[BOSS1 上_铁胆狱火 萨托瓦尔] 火危 分散点名记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:24207"],userControl: false)]
    public void 火危记录(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me) MyPyrocrisis=1; 
    }
    
    [ScriptMethod(name: "[BOSS1 上_铁胆狱火 萨托瓦尔] 火念 分散点名重置", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^2420[67]$"],userControl: false)]
    public void 火念重置(Event @event, ScriptAccessory accessory)
    {
        MyPyrocrisis=0; 
    }

    [ScriptMethod(name: "[BOSS1 上_铁胆狱火 萨托瓦尔] 火念（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:24206"])]
    public async void 火念(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(300);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"火念{@event.TargetId}";

        if (MyPyrocrisis == 1)
        {
            if (isText) accessory.Method.TextInfo("散开", duration: 7000, true);
            if (isTTS)accessory.Method.TTS("散开");
            dp.Color = accessory.Data.DefaultDangerColor;
        }
        else
        {
            if (isText) accessory.Method.TextInfo("分摊", duration: 7000, false);
            if (isTTS)accessory.Method.TTS("分摊");
            dp.Color = accessory.Data.DefaultSafeColor;
        }

        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[BOSS2 丘库雷因] 污染波（AOE）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^23695$"])]
    public void 污染波(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
    }
    
    [ScriptMethod(name: "[BOSS2 丘库雷因] 恐惧波动_移动命令 位置预测", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^216[1-4]$"])]
    public async void 移动命令(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "移动命令";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(5f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(1f, 16f);
        dp.DestoryAt = 7000;
        
        switch (@event["StatusID"])
        {
            case "2161":
                dp.Rotation = 0f.DegToRad();
                if(isText) accessory.Method.TextInfo("强制移动：前", duration: 3000, true);
                if(isTTS) accessory.Method.TTS("向前移动到安全区");
                break;
            case "2162":
                dp.Rotation = 180f.DegToRad();
                if(isText) accessory.Method.TextInfo("强制移动：后", duration: 3000, true);
                if(isTTS) accessory.Method.TTS("向后移动到安全区");
                break;
            case "2163":
                dp.Rotation = 90f.DegToRad();
                if(isText) accessory.Method.TextInfo("强制移动：左", duration: 3000, true);
                if(isTTS) accessory.Method.TTS("向左移动到安全区");
                break;
            case "2164":
                dp.Rotation = 270f.DegToRad();
                if(isText) accessory.Method.TextInfo("强制移动：右", duration: 3000, true);
                if(isTTS) accessory.Method.TTS("向右移动到安全区");
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "[BOSS2 丘库雷因] 不净压杀", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^23682$"])]
    public void 不净压杀(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"进入绿圈", duration: 7000, true);
        if (isTTS)accessory.Method.TTS($"进入绿圈");
    }

    [ScriptMethod(name: "[BOSS2 丘库雷因] 腐朽烧杀（引导黄圈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^23686$"])]
    public void 腐朽烧杀(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"诱导三连黄圈", duration: 4000, false);
        if (isTTS)accessory.Method.TTS($"诱导三连黄圈");
    }
    
    [ScriptMethod(name: "[BOSS2 丘库雷因] 魔脉瘤（三穿一）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^23693$"])]
    public void 魔脉瘤(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo($"三穿一", duration: 4000, false);
        if (isTTS)accessory.Method.TTS($"三穿一");
    }
    
    [ScriptMethod(name: "[BOSS2 丘库雷因] 重踢击（死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^23698$"])]
    public void 重踢击(Event @event, ScriptAccessory accessory)
    {
        if (isText && @event.TargetId() == accessory.Data.Me) accessory.Method.TextInfo($"坦克死刑点名", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"坦克死刑");
    }
    
    [ScriptMethod(name: "[BOSS2 丘库雷因] 丧失波动（AOE+目押）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^24910$"])]
    public void 丧失波动(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"目押，靠近绿圈", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"目押，靠近绿圈");
    }
    
    [ScriptMethod(name: "[BOSS2 丘库雷因] 污浊奔流（大地摇动点名）", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^0028$"])]
    public void 污浊奔流(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo($"大地摇动点名", duration: 4000, true);
        if (isTTS)accessory.Method.TTS($"大地摇动点名");
        
        var dp = accessory.Data.GetDefaultDrawProperties();

        var boss = accessory.Data.Objects.GetByDataId(12715).FirstOrDefault();
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;
        
        dp.Name = "污浊奔流";
        dp.TargetObject = @event.SourceId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(50f);
        dp.Radian = 30f.DegToRad();
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "[BOSS3 桑尼恩] 连装导弹（分摊）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^24003$"])]
    public void 连装导弹(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"连装导弹{@event.TargetId}";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "[BOSS3 小达温] 服从初始化", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:24013"])]
    public void 服从初始化(Event @event, ScriptAccessory accessory)
    { 
        // object比服从读条生成的更早，所以使用偶数次组合技【炎帝风翼破】来初始化
        _objectEffectRecords = new List<(int id1, int id2, Vector3 position)>();
        _currentRecordIndex = 0;
        _jumpCount = 0;
        _dawonRotation = 0f;
        _pendingCrossIndex = -1;
        _pendingCrossName = "";
        if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 服从Object初始化完成");
    }
    
    [ScriptMethod(name: "[BOSS3 小达温] 服从记录", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:regex:^(1|64)$", "Id2:regex:^(2|128)$"])]
    public void 服从记录(Event @event, ScriptAccessory accessory)
    {
        int id1 = int.Parse(@event["Id1"]);
        int id2 = int.Parse(@event["Id2"]);
    
        bool isValidDonut = (id1 == 1 && id2 == 2);      // 月环
        bool isValidCrossAOE = (id1 == 64 && id2 == 128); // 十字
    
        // 只记录月环或十字，排除干扰项，最多记录3次
        if ((isValidDonut || isValidCrossAOE) && _objectEffectRecords.Count < 3)
        {
            // 从SourcePosition JSON中解析位置
            Vector3 position = Vector3.Zero;
            string sourcePosStr = @event["SourcePosition"];
            if (!string.IsNullOrEmpty(sourcePosStr))
            {
                try
                {
                    var posObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, float>>(sourcePosStr);
                    if (posObj != null && posObj.ContainsKey("X") && posObj.ContainsKey("Y") && posObj.ContainsKey("Z"))
                    {
                        position = new Vector3(posObj["X"], posObj["Y"], posObj["Z"]);
                    }
                }
                catch { }
            }
        
            _objectEffectRecords.Add((id1, id2, position));
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 记录第{_objectEffectRecords.Count}次：id1={id1}, id2={id2}，位置：{position}");
        }
        else
        {
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 过滤干扰项：id1={id1}, id2={id2}");
        }
    }
    
    [ScriptMethod(name: "[BOSS3 小达温] 服从面向记录", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:24010"])]
    public void 服从面向记录(Event @event, ScriptAccessory accessory)
    {
        if (float.TryParse(@event["SourceRotation"], out float rotation))
        {
            _dawonRotation = rotation;
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 更新BOSS面向：{_dawonRotation}");
        }
        
        // 如果有待更新的十字，销毁旧的并重新绘制正式十字
        if (_pendingCrossIndex >= 0 && _pendingCrossIndex < _objectEffectRecords.Count)
        {
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 面向已更新，销毁旧十字并重新绘制");
            accessory.Method.RemoveDraw(_pendingCrossName);
            绘制十字正式(_pendingCrossIndex, @event, accessory);
            _pendingCrossIndex = -1;
            _pendingCrossName = "";
        }
    }

    [ScriptMethod(name: "[BOSS3 小达温] 服从（跳跃）", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:regex:^4$", "Id2:regex:^8$"])]
    public void 服从(Event @event, ScriptAccessory accessory)
    {
        _jumpCount++;
        
        if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 服从第{_jumpCount}次跳跃触发，当前记录数：{_objectEffectRecords.Count}");
        
        // 第一次4|8：立即绘制第一个技能
        if (_jumpCount == 1 && _objectEffectRecords.Count > 0)
        {
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 第一次跳跃，立即绘制第一个技能");
            绘制技能(0, @event, accessory, 0);
            return;
        }
        
        // 第二次及之后的4|8：延迟2400ms后绘制
        if (_jumpCount >= 2)
        {
            int recordIndex = _jumpCount - 1;
            if (recordIndex < _objectEffectRecords.Count)
            {
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 第{_jumpCount}次跳跃，延迟2400ms后绘制第{recordIndex + 1}个技能");
                绘制技能(recordIndex, @event, accessory, 2400);
            }
        }
    }

    private void 绘制技能(int recordIndex, Event @event, ScriptAccessory accessory, int delay)
    {
        if (recordIndex >= _objectEffectRecords.Count) return;
        
        var currentRecord = _objectEffectRecords[recordIndex];
        int id1 = currentRecord.id1;
        int id2 = currentRecord.id2;
        Vector3 position = currentRecord.position;
        
        bool isDonut = (id1 == 1 && id2 == 2);
        bool isCrossAOE = (id1 == 64 && id2 == 128);
        
        if (isDonut)
        {
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 绘制月环，位置：{position}");
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"寒冷脉冲";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.Position = position;
            dp.Scale = new Vector2(60f);
            dp.InnerScale = new Vector2(12f);
            dp.Radian = float.Pi * 2;
            dp.Delay = delay;
            dp.DestoryAt = 6500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        else if (isCrossAOE)
        {
            // 十字：先绘制预测版本
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 绘制十字预测，位置：{position}，当前面向：{_dawonRotation}");
            _pendingCrossIndex = recordIndex;
            _pendingCrossName = $"炎帝十文字_预测_{recordIndex}";
            
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = _pendingCrossName;
            dp1.Position = position;
            dp1.Scale = new (14f, 100f);
            dp1.Owner = @event.SourceId();
            dp1.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
            dp1.Delay = delay;
            dp1.DestoryAt = 6500;
            
            float[] rotations = { 0f, 90f };
            foreach (float rotationOffset in rotations)
            {
                dp1.Rotation = _dawonRotation + rotationOffset.DegToRad();
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);
            }
        }
    }

    private void 绘制十字正式(int recordIndex, Event @event, ScriptAccessory accessory)
    {
        if (recordIndex >= _objectEffectRecords.Count) return;
        
        var currentRecord = _objectEffectRecords[recordIndex];
        Vector3 position = currentRecord.position;
        
        if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 绘制十字正式，位置：{position}，BOSS面向：{_dawonRotation}");
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"炎帝十文字_正式_{recordIndex}";
        dp1.Position = position;
        dp1.Scale = new (14f, 100f);
        dp1.Owner = @event.SourceId();
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp1.Delay = 0;
        dp1.DestoryAt = 2400;
        
        float[] rotations = { 0f, 90f };
        foreach (float rotationOffset in rotations)
        {
            dp1.Rotation = _dawonRotation + rotationOffset.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);
        }
    }
    
    #endregion
}

#region EventExtensions

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
    
    public static uint IconId(this Event @event)
    {
        return ParseHexId(@event["Id"], out var id) ? id : 0;
    }
    
    public static uint ObjectEffectId(this Event @event)
    {
        return ParseHexId(@event["Id"], out var id) ? id : 0;
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

    public static uint StatusID(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StatusID"]);
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
    public static string GetPlayerJob(this ScriptAccessory accessory, IPlayerCharacter? playerObject, bool fullName = false)
    {
        if (playerObject == null) return "None";
        return fullName ? playerObject.ClassJob.Value.Name.ToString() : playerObject.ClassJob.Value.Abbreviation.ToString();
    }

    /// <summary>
    /// 获取玩家的职能
    /// Return: "Tank"(坦克) / "Healer"(治疗) / "Melee DPS"(近战) / "Ranged DPS"(远程) / "Unknown" / "None"
    /// </summary>
    public static string GetPlayerRole(this ScriptAccessory sa, IPlayerCharacter? playerObject)
    {
        if (playerObject == null) return "None";
        return playerObject.ClassJob.Value.Role switch
        {
            1 => "Tank",        // 坦克
            4 => "Healer",      // 治疗
            2 => "Melee DPS",   // 近战DPS
            3 => "Ranged DPS",  // 远程DPS
            _ => "Unknown"
        };
    }
    
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
#endregion EventExtensions