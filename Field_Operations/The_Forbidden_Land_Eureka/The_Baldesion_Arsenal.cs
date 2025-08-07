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

namespace The_Baldesion_Arsenal_Eureka;


[ScriptType(guid: "cb001385-81e0-4c16-8bb3-fb51fd70336a", name: "禁地优雷卡 - 巴尔德西昂兵武塔", territorys: [827],
    version: "0.0.0.3", author: "Tetora", note: noteStr)]

public class The_Baldesion_Arsenal
{
    const string noteStr =
        """
        v0.0.0.3:
        禁地优雷卡 - 巴尔德西昂兵武塔 初版绘制
        【施工中】没写完，没测过，必电，如果你是铁人可以帮我录个像
        """;
    
    #region 基础控制
    
    [UserSetting("TTS开关（TTS请二选一开启）")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("关闭与ACT冲突的TTS")]
    public bool isACT { get; set; } = true;
    
    [UserSetting("弹窗文本提示开关")]
    public bool isText { get; set; } = true;
    
    [UserSetting("开发者模式")]
    public bool isDeveloper { get; set; } = false;
    
    
    #endregion
    
    #region 全局设置
    
    uint AstralEssence = 0; // 光之腕
    uint UmbralEssence = 0; // 暗之腕
    uint ball = 0; // 球形态
    uint Cube = 0; // 立方体形态
    uint Pyramid = 0; // 三角锥形态
    uint Stellation = 0; // 星形多面体状态
    
    public void Init(ScriptAccessory accessory) {
        AstralEssence = 0;
        UmbralEssence = 0;
        ball = 0;
        Cube = 0;
        Pyramid = 0;
        Stellation = 0;
    }
    
    // ActionId: 7538 插言 ； 7551 伤头 ； 7540 下踢 ； 7863 扫腿 ； 25880 催眠 ； 16560 沉静 ; 139 神圣
    
    [ScriptMethod(name: "伤头 & 插言 打断销毁", eventType: EventTypeEnum.ActionEffect, userControl: false, 
     eventCondition: ["ActionId:regex:^(7538|7551)$"] )]
    public void 伤头插言打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"兵武元精_强放逐{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "眩晕打断销毁", eventType: EventTypeEnum.ActionEffect, userControl: false, 
        eventCondition: ["ActionId:regex:^(139|7540|7863)$"] )]
    public void 眩晕打断销毁(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"兵武元精_强放逐{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "睡眠打断销毁", eventType: EventTypeEnum.ActionEffect, userControl: false, 
        eventCondition: ["ActionId:regex:^(25880|16560)$"] )]
    public void 睡眠打断销毁(Event @event, ScriptAccessory accessory)
    {
        // accessory.Method.RemoveDraw($"暂时还没有{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void 防击退销毁(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw(".*击退.*");
    }
    
    #endregion
    
    #region 小怪提示
    
    [ScriptMethod(name: "——————  小怪基础提示（此选项并无意义）——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:0"])]
    public void 小怪基础提示(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "兵武智蛙_玩具锤 沉静提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15363"])]
    public void 兵武智蛙_玩具锤(Event @event, ScriptAccessory accessory)
    { 
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (isHealer || isDeveloper)
        {
            if (isText)accessory.Method.TextInfo("沉静 <兵武智蛙>", duration: 3000, true);
            if (isACT) return; 
            if (isTTS)accessory.Method.TTS("沉静 <兵武智蛙>");
            if (isEdgeTTS)accessory.Method.EdgeTTS("沉静 <兵武智蛙>");
        }
    }
    
    [ScriptMethod(name: "兵武元精_强放逐 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15383"])]
    public void 兵武元精_强放逐提示(Event @event, ScriptAccessory accessory)
    { 
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank || isDeveloper)
        {
            if (isText)accessory.Method.TextInfo("打断 <兵武元精>", duration: 5300, true);
            if (isACT) return; 
            if (isTTS)accessory.Method.TTS("打断 <兵武元精>");
            if (isEdgeTTS)accessory.Method.EdgeTTS("打断 <兵武元精>");
        }
    }
    
    [ScriptMethod(name: "兵武半人马_狂暴 沉静提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15358"])]
    public void 兵武半人马_狂暴(Event @event, ScriptAccessory accessory)
    { 
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (isHealer || isDeveloper)
        {
            if (isText)accessory.Method.TextInfo("沉静 <兵武半人马>", duration: 3000, true);
            if (isACT) return; 
            if (isTTS)accessory.Method.TTS("沉静 <半人马>");
            if (isEdgeTTS)accessory.Method.EdgeTTS("沉静 <半人马>");
        }
    }
    
    [ScriptMethod(name: "兵武卡尔克布莉娜_闹剧 沉静提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15368"])]
    public void 兵武卡尔克布莉娜_闹剧(Event @event, ScriptAccessory accessory)
    { 
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (isHealer || isDeveloper)
        {
            if (isText)accessory.Method.TextInfo("沉静 <兵武卡尔克布莉娜>", duration: 3000, true);
            if (isACT) return; 
            if (isTTS)accessory.Method.TTS("沉静 <卡尔克布莉娜>");
            if (isEdgeTTS)accessory.Method.EdgeTTS("沉静 <卡尔克布莉娜>");
        }
    }
    
    [ScriptMethod(name: "兵武魔导书_沉默&麻痹 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^153(21|76)$"])]
    public void 兵武魔导书_沉默麻痹(Event @event, ScriptAccessory accessory)
    { 
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank || isDeveloper)
        {
            if (isText)accessory.Method.TextInfo("打断 <兵武魔导书>", duration: 4300, true);
            if (isACT) return; 
            if (isTTS)accessory.Method.TTS("打断 <兵武魔导书>");
            if (isEdgeTTS)accessory.Method.EdgeTTS("打断 <兵武魔导书>");
        }
    }
    
    [ScriptMethod(name: "兵武博学林鸠_昏暗之章 打断提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15373"])]
    public void 兵武博学林鸮_昏暗之章(Event @event, ScriptAccessory accessory)
    { 
        // 实际上应该是博学林鸮，但是界面显示不出来鸮字所以找个形近字替代.jpg
        
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank || isDeveloper)
        {
            if (isText)accessory.Method.TextInfo("打断 <兵武博学林鸮>", duration: 2300, true);
            if (isACT) return; 
            if (isTTS)accessory.Method.TTS("打断 <博学林鸮>");
            if (isEdgeTTS)accessory.Method.EdgeTTS("打断 <博学林鸮>");
        }
    }
    
    
    #endregion
    
    #region 小怪绘制
    
    [ScriptMethod(name: "——————  技能绘制（此选项并无意义）——————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:0"])]
    public void 小怪技能绘制(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "兵武比布鲁斯_尾部碎击（扫尾）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15356"])]
    public void 兵武比布鲁斯_尾部碎击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "兵武比布鲁斯_尾部碎击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12);
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "兵武元精_强放逐（点名圆形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15383"])]
    public void 兵武元精_强放逐绘制(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"兵武元精_强放逐{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "兵武卡尔克布莉娜_恐惧之瞳（石化顺劈）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15366"])]
    public void 兵武卡尔克布莉娜_恐惧之瞳(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "兵武卡尔克布莉娜_恐惧之瞳";
        dp.Color = new Vector4(1f, 0f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(42.2f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "兵武扇_古代暴风（击退连线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15372"])]
    public void 兵武扇_古代暴风(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("靠近 <兵武扇> 击退", duration: 4300, true);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "古代暴风击退";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    #endregion
    
    #region BOSS1 亚特 / 欧文
    // 14632：本体读条 [妖枪振]钢铁 ； 14633：本体读条 [妖枪旋]月环 ； 14631：幻枪招来
    //幻枪招来后，BOSS（DataID：9818）会连线4根 烈焰金枪（DataID：9819），并读条随机[妖枪振]钢铁月环，随后烈焰金枪将瞬发BOSS的钢铁月环 [幻枪振] 14634 / 14635
    /*
    [ScriptMethod(name: "BOSS1_亚特 妖枪振（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14632"])]
    public void 妖枪振(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "妖枪振";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 4200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1_亚特 妖枪旋（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14633"])]
    public void 妖枪旋(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "妖枪旋";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(22f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    */
    
    [ScriptMethod(name: "BOSS1_欧文_红颈妖枪 元素魔法（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1465[01]$"])]
    public void 元素魔法(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "元素魔法";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);  // 表格为13m，待实战验证
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1_欧文 连装魔_血之魔术（孤独感提示）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1753"])]
    public void 连装魔_血之魔术(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("孤独感集合", duration: 2300, true);
        if (!isACT && isTTS) accessory.Method.TTS("孤独感集合");
        if (!isACT && isEdgeTTS) accessory.Method.TTS("孤独感集合");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "连装魔_血之魔术";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(0.5f);
        dp.DestoryAt = 2300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion

    #region BOSS2 莱丁
    [ScriptMethod(name: "BOSS2_莱丁 天逆矛（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14441"])]
    public void 天逆矛(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "天逆矛";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.DestoryAt = 7200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS2_莱丁 旋の斩铁剑（月环）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14442"])]
    public void 旋斩铁剑(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "旋斩铁剑";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    

    [ScriptMethod(name: "BOSS2_莱丁 片の斩铁剑（直线）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1444[34]$"])]
    public void 片斩铁剑(Event @event, ScriptAccessory accessory)
    {
        var dp=accessory.Data.GetDefaultDrawProperties();
        dp.Name="片斩铁剑";
        dp.Scale=new(70,39);
        dp.Owner=@event.SourceId();
        dp.Color=accessory.Data.DefaultDangerColor;
        dp.DestoryAt=6200;
        
        switch (@event.ActionId())
        {
            case 14444:  // 左刀
                dp.Rotation=90f.DegToRad();
                dp.Offset=new Vector3(3.5f,0,0);
                break;
            case 14443:  // 右刀
                dp.Rotation=-90f.DegToRad();
                dp.Offset=new Vector3(-3.5f,0,0);
                break;
        }
        
        /*
        bool isLeft=true;
        //14444 左 ， 14443右

        if(@event["ActionId"].Equals("14444")) {
            isLeft=true;
        }
        
        if(@event["ActionId"].Equals("14443")) {
            isLeft=false;
        }
        
        var dp=accessory.Data.GetDefaultDrawProperties();
        dp.Name="片斩铁剑";
        dp.Scale=new(70,39);
        dp.Owner=@event.SourceId();
        dp.Color=accessory.Data.DefaultDangerColor;
        dp.DestoryAt=6200;

        if(isLeft) {
            dp.Rotation=90f.DegToRad();
            dp.Offset=new Vector3(3.5f,0,0);
        }

        else {
            dp.Rotation=-90f.DegToRad();
            dp.Offset=new Vector3(-3.5f,0,0);
        }
        */
        
        //dp.Offset=new Vector3(0,0,3.5f);
        // 向后移动3.5f
        // 我不确定rotation是否会影响偏差
        // 如果并没有按照预期向左向右偏移 注释或者删掉掉上面这个offset设置^ 然后解除注释if里的两个注释
        // 如果这样就已经生效了 就删除if里的两个注释
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }


    [ScriptMethod(name: "BOSS2_莱丁 战死击（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14460"])]
    public void 战死击(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "战死击";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.4f);  // 6m + 目标圈 4.4m
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region BOSS3 绝对的美德
    
    [ScriptMethod(name:"光暗之腕记录", eventType:EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^171[01]$"], userControl:false)]
    public void 光暗之腕记录(Event @event, ScriptAccessory accessory) 
    {
        switch (@event.StatusID())
        {
            case 1710:  //光之腕
                AstralEssence = 1;
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 已记录 <光之腕>");
                break;
            case 1711:  // 暗之腕
                UmbralEssence = 1;
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 已记录 <暗之腕>");
                break;
        }
    }
    
    [ScriptMethod(name: "BOSS3_绝对的美德 极性波动（钢铁）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1422[0-3]$"])]
    public void 极性波动(Event @event, ScriptAccessory accessory)
    { 
        /// var Balls = accessory.Data.Objects.Where(x => x.DataId == 还没找到DataId && x 
        ///             is IBattleChara chara && chara.HasStatus(new uint[] { 1710, 1711 })).ToList();
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "极性波动";
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 4200;
        switch (@event.ActionId())
        {
            case 14220:  // 光之波动
                dp.Color = new Vector4(1f, 1f, 1f, 1f); // Omen: m0531_light_o0v
                break;
            case 14221:  // 光之波动
                dp.Color = new Vector4(1f, 1f, 1f, 1f); // Omen: m0531_light_o0v
                break;
            case 14222:  // 暗之波动
                dp.Color = new Vector4(0f, 0f, 0f, 1f); // Omen: m0531_dark_o0v
                break;
            case 14223:  // 暗之波动
                dp.Color = new Vector4(0f, 0f, 0f, 1f); // Omen: m0531_dark_o0v
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_绝对的美德 光明/黑暗极光（强化半场）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^142(17|18|30|31)$"])]
    public void 半场极光(Event @event, ScriptAccessory accessory)
    {
        var dp=accessory.Data.GetDefaultDrawProperties();
        dp.Name="半场极光";
        dp.Scale=new(50,30);
        dp.Owner=@event.SourceId();
        switch (@event.ActionId())
        {
            case 14217:  // 光明极光
                dp.Color = new Vector4(1f, 1f, 1f, 1f); // Omen: m0531_sp03l_o0v
                dp.DestoryAt=2700;
                break;
            case 14218:  // 黑暗极光
                dp.Color = new Vector4(0f, 0f, 0f, 1f); // Omen: m0531_sp03d_o0v
                dp.DestoryAt=2700;
                break;
            case 14230:  // 光明极光
                dp.Color = new Vector4(1f, 1f, 1f, 1f); // Omen: m0531_sp03l_o0v
                dp.DestoryAt=4700;
                break;
            case 14231:  // 黑暗极光
                dp.Color = new Vector4(0f, 0f, 0f, 1f); // Omen: m0531_sp03d_o0v
                dp.DestoryAt=4700;
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "BOSS3_绝对的美德 极光之风（圆形死刑）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14234"])]
    public void 极光之风(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "极光之风";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId;
        dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.CentreOrderIndex = 1;
        dp.Scale = new Vector2(5);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_相对的美德 冲击流（12点查找）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14229"])]
    public void 相对冲击流(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "相对冲击流";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region BOSS4 奥兹玛原形
    
    [ScriptMethod(name: "BOSS4_奥兹玛原形 形态变化重置", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:"])]
    public async void 形态变化重置(Event @event, ScriptAccessory accessory)
    {
        // await Task.Delay(2000);
        ball = 0;
        Cube = 0;
        Pyramid = 0;
        Stellation = 0;
        if(isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: 形态变化已重置");
    }
    
    [ScriptMethod(name: "BOSS4_奥兹玛原形 球形态转换（黑洞）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:"])]
    public void 奥兹玛原形_球形态(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "BOSS4_奥兹玛原形 立方体形态转换（月环）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1070"])]
    public void 奥兹玛原形_立方体形态(Event @event, ScriptAccessory accessory)
    {
        Cube = 1;
        if (isText) accessory.Method.TextInfo("<立方体> 普攻：一仇直线", duration: 10000, true);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "耀星";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(38f); // 表格38m + 目标圈 m
        dp.InnerScale = new Vector2(32f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

    }
    
    [ScriptMethod(name: "BOSS4_奥兹玛原形 三角锥形态转换（直线）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1071"])]
    public void 奥兹玛原形_三角锥形态(Event @event, ScriptAccessory accessory)
    {
        Pyramid = 0;
        if (isText) accessory.Method.TextInfo("<三角锥> 普攻: 最远圆形AOE（带出血）", duration: 10000, true);
        
        for(int axisRotation=0;axisRotation<360;axisRotation+=120) {
            var dp=accessory.Data.GetDefaultDrawProperties();
            dp.Name="缩小射线";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(5.5f,40f); // 长度为40m + 目标圈 m
            dp.Owner = @event.SourceId();
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
        }
        
    }
    
    [ScriptMethod(name: "BOSS4_奥兹玛原形 星形多面体形态转换（钢铁）", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1744"])]
    public void 奥兹玛原形_星形多面体形态(Event @event, ScriptAccessory accessory)
    {
        Stellation = 1;
        if (isText) accessory.Method.TextInfo("<圣晶石> 普攻: 圆形分摊（随机点名）", duration: 10000, true);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "启明星";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(27f);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "BOSS4_奥兹玛原形 神圣（击退预测）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17394"])]
    public void 神圣(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("靠近击退", duration: 3300, true);
        if (!isACT && isTTS) accessory.Method.TTS("靠近击退");
        if (!isACT && isEdgeTTS) accessory.Method.TTS("靠近击退");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "神圣击退预测";
        dp.Scale = new(1f, 3f);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "BOSS4_奥兹玛原形 流星（击退预测）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17394"])]
    public void 流星(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("靠近击退", duration: 4300, true);
        if (!isACT && isTTS) accessory.Method.TTS("靠近击退");
        if (!isACT && isEdgeTTS) accessory.Method.TTS("靠近击退");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "流星击退预测";
        dp.Scale = new(1f, 8f);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "BOSS4_奥兹玛原形 流星（击退源）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17394"])]
    public void 流星击退源(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "流星击退源";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(0.1f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "BOSS4_奥兹玛原形 加速度炸弹 提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1072"])]
    public async void 运动体探知干扰器(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return;
        await Task.Delay(13500);
        accessory.Method.TextInfo("停止行动", duration: 800, true);
        accessory.Method.TTS("停止行动");
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

public static class IbcHelper
{
    public static bool HasStatus(this IBattleChara chara, uint statusId)
    {
        return chara.StatusList.Any(x => x.StatusId == statusId);
    }

    public static bool HasStatus(this IBattleChara chara, uint[] statusIds)
    {
        return chara.StatusList.Any(x => statusIds.Contains(x.StatusId));
    }
}