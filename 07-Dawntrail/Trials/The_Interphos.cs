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
using System.Threading;

namespace the_Interphos;

[ScriptType(guid: "16a531a4-28b9-414d-9209-ba6673d1f268", name: "永恒女王歼灭战", territorys: [1202],
    version: "0.0.0.3", author: "Poetry", note: noteStr)]

public class TheInterphos
    {
        const string noteStr =
            """
            v0.0.0.3:
            LV100 永恒女王歼灭战 绘制
            原作者：Poetry
            """;
    
        [UserSetting("TTS开关（TTS请二选一开启）")]
        public bool isTTS { get; set; } = false;
    
        [UserSetting("EdgeTTS开关（TTS请二选一开启）")]
        public bool isEdgeTTS { get; set; } = true;
    
        [UserSetting("弹窗文本提示开关")]
        public bool isText { get; set; } = true;
        
        [UserSetting("开发者模式")]
        public bool isDeveloper { get; set; } = false;
        
        private bool stopBuff = false;
        private uint bossId = 0;

        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(@".*");
            stopBuff = false;
            bossId = 0;
        }

        private static bool ParseObjectId(string? idStr, out uint id)
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
        // 左右刀，该技能存在两次都打同一边的情况。
        // 36638 先左后左
        // 36642

        // 36639 先左后右
        // 36643

        // 36641 先右后左
        // 36642

        // 36640 先右再右
        [ScriptMethod(name: "合法武力：左右刀", eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(366(38|39|40|41))$"])]
        public void LegitimateForce1(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var idStr = @event["ActionId"];
            var firstLeft = idStr switch
            {
                "36638" => true,
                "36639" => true,
                "36640" => false,
                "36641" => false,
                _ => false
            };
            var secondLeft = idStr switch
            {
                "36638" => true,
                "36639" => false,
                "36640" => false,
                "36641" => true,
                _ => false
            };
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"左右刀-先{(firstLeft ? "左" : "右")}";
            dp1.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp1.Owner = sid;
            dp1.Scale = new(100);
            dp1.Rotation = firstLeft ? float.Pi / -2 : float.Pi / 2;
            dp1.DestoryAt = 7700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);

            dp2.Name = $"左右刀-后{(secondLeft ? "左" : "右")}";
            dp2.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp2.Owner = sid;
            dp2.Scale = new(100);
            dp2.Delay = 8000;
            dp2.DestoryAt = 3500;
            dp2.Rotation = secondLeft ? float.Pi / -2 : float.Pi / 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);
        }

        // 以太税 获取 Boss id
        [ScriptMethod(name: "获取 Boss id", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36604"],
            userControl: false)]
        public void findBossId(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            bossId = sid;
        }

        // DotAoE+左中右扇形范围AoE
        [ScriptMethod(name: "以太税（扭曲地板）", eventType: EventTypeEnum.EnvControl,
            eventCondition: ["Id:regex:^(10000100|04000100|08000100)$"])]
        public void AethertitheRight(Event @event, ScriptAccessory accessory)
        {
            var id = @event["Id"];
            var dp = accessory.Data.GetDefaultDrawProperties();
            
            if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG] 成功检测到EnvControl生成, Id: {id}");
         
            switch (id)
            {
                case "10000100":
                    dp.Name = $"以太税（扭曲地板）右";
                    dp.Rotation = float.Pi * 11 / 36;
                    break;
                case "04000100":
                    dp.Name = $"以太税（扭曲地板）左";
                    dp.Rotation = float.Pi * -11 / 36;
                    break;
                case "08000100":
                    dp.Name = $"以太税（扭曲地板）中";
                    dp.Rotation = 0;
                    break;  
            }

            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = bossId;
            dp.Scale = new Vector2(60);
            dp.Radian = float.Pi / 2.5f;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        // 诉诸武力
        [ScriptMethod(name: "诉诸武力:死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36602"])]
        public void 诉诸武力(Event @event, ScriptAccessory accessory)
        {
            if(isText) accessory.Method.TextInfo("死刑", 4300);
            if(isTTS) accessory.Method.TTS("死刑");
            if(isEdgeTTS) accessory.Method.EdgeTTS("死刑");
            
        }

        // // 王土、虚景切换、
        // [ScriptMethod(name: "AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3660[678]"])]
        // public void VirtualShift(Event @event, ScriptAccessory accessory)
        // {
        //     if(isText) accessory.Method.TextInfo("AOE", 4300);
        //     if(isTTS) accessory.Method.TTS("AOE");
        //     if(isEdgeTTS) accessory.Method.EdgeTTS("AOE");
        // }
        //


        // [ScriptMethod(name: "护城墙", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36613"])]
        // public void Castellation(Event @event, ScriptAccessory accessory)
        // {
        //     
        // }


        //以场地上的一点为中心造成全屏击退效果
        [ScriptMethod(name: "下行突风 击退连线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36610"])]
        public void DownwardGale(Event @event, ScriptAccessory accessory)
        {
            if(isText) accessory.Method.TextInfo("斜角击退", 5300);
            if(isTTS) accessory.Method.TTS("斜角击退");
            if(isEdgeTTS) accessory.Method.EdgeTTS("斜角击退");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"斜角击退";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(2, 10);
            dp.Rotation = float.Pi;
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultSafeColor.WithW(3);
            dp.TargetPosition = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
            dp.DestoryAt = 6000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }

        // AoE，并从场地的一侧造成接近场地宽度的横向击退效果，此脚本为指路连线测试用
        [ScriptMethod(name: "强风 击退位置指路", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36612"])]
        public void PowerfulGust(Event @event, ScriptAccessory accessory)
        {
            if(isText) accessory.Method.TextInfo("横向击退", 5300);
            if(isTTS) accessory.Method.TTS("横向击退");
            if(isEdgeTTS) accessory.Method.EdgeTTS("横向击退");

            var targetPos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
            var isRight = targetPos.X > 110;
            var rPos = new Vector3(112, 0, 82);
            var lPos = new Vector3(88, 0, 82);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"横向击退";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.Color = dp.Color = accessory.Data.DefaultSafeColor;
            dp.TargetPosition = isRight ? rPos : lPos;
            dp.DestoryAt = 6000;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        
        [ScriptMethod(name: "防击退销毁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
        public void 防击退销毁(Event @event, ScriptAccessory accessory)
        {
            if ( @event.TargetId() != accessory.Data.Me) return; 
            accessory.Method.RemoveDraw(".*击退");
        }
        

        //召唤出数个终端机组成圆，随后对范围之外造成环形伤害。
        [ScriptMethod(name: "王权残暴:月环", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36633"])]
        public void BrutalCrown(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if(isText) accessory.Method.TextInfo("月环", 7300);
            if(isTTS) accessory.Method.TTS("月环");
            if(isEdgeTTS) accessory.Method.EdgeTTS("月环");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"月环";
            dp.Scale = new(60);
            dp.InnerScale = new(5);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }

        // 绝对君权
        [ScriptMethod(name: "绝对君权 圆圈高亮", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36622"])]
        public void 圆圈提示(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"绝对君权";
            dp.Scale = new(8);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        // 4130 空间掌控：制动
        // 3815 空间掌控：石化光 
        [ScriptMethod(name: "绝对君权 停止buff提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4130"])]
        public void Buff收集1(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid == accessory.Data.Me)
            {
                stopBuff = true;
                if(isText) accessory.Method.TextInfo("稍后停止移动", 10000);
                Thread.Sleep(14000);
                if(isText) accessory.Method.TextInfo("停止移动", 1300, true);
                if(isTTS) accessory.Method.TTS("停止移动");
                if(isEdgeTTS) accessory.Method.EdgeTTS("停止移动");

            }
        }

        [ScriptMethod(name: "绝对君权 背对buff提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3815"])]
        public void Buff收集2(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid == accessory.Data.Me)
            {
                stopBuff = true;
                if(isText) accessory.Method.TextInfo("稍后背对Boss", 10000);
                Thread.Sleep(14000);
                if(isText) accessory.Method.TextInfo("背对Boss", 1300, true);
                if(isTTS) accessory.Method.TTS("背对Boss");
                if(isEdgeTTS) accessory.Method.EdgeTTS("背对Boss");

            }
        }
        
        [ScriptMethod(name: "放逐射线（扇形）", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36647"])]
        public void 放逐射线(Event @event, ScriptAccessory accessory)
        {
            Thread.Sleep(2200);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "放逐射线";
            dp.Color = accessory.Data.DefaultDangerColor.WithW(10);
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(100f);
            dp.Radian = MathHelpers.DegToRad(30f);
            dp.DestoryAt = 1000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
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
public static class MathHelpers
{
    public static float DegToRad(float degrees)
    {
        return degrees * (float)(Math.PI / 180.0);
    }
    
    public static double DegToRad(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    
    public static float RadToDeg(float radians)
    {
        return radians * (float)(180.0 / Math.PI);
    }
    
    public static double RadToDeg(double radians)
    {
        return radians * 180.0 / Math.PI;
    }
}