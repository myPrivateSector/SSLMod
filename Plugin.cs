using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PlayerIOClient;
using UnityEngine;
using Vectrosity;
using Random = UnityEngine.Random;

namespace SSLMod
{
    public class Vector
    {
        public Vector2 start;
        public Vector2 end;
        public float x;
        public float y;

        public Vector(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
            this.x = end.x - start.x;
            this.y = end.y - start.y;
        }

        public Vector Negative()
        {
            return new Vector(this.end, this.start);
        }

        public float VectorProduct(Vector other)
        {
            return this.x * other.y - this.y * other.x;
        }
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony Harmony { get; } = new Harmony("VeryHarmonious");

        private static ManualLogSource Log;

        private static ConfigEntry<bool> _enableRainbowTracers;
        private static ConfigEntry<bool> _enableGearCollection;
        private static ConfigEntry<bool> _enableTracers;
        private static ConfigEntry<bool> _enableAutoReady;
        // private static ConfigEntry<bool> _enableMoveTowardCenter;
        private static ConfigEntry<bool> _enableMessageSentLogging;
        private static ConfigEntry<bool> _enableMessageReceivedLogging;
        private static ConfigEntry<bool> _enablePortalTracers;
        private static ConfigEntry<int> _cycleInterval;

        private static ConfigEntry<float> _tracerTimeInterval;
        private static ConfigEntry<float> _tracerTotalTime;
        private static ConfigEntry<bool> _tracerEnableBumperCalculation;
        private static ConfigEntry<bool> _tracerEnablePortalCalculation;

        private static void Print(string msg)
        {
            Log?.Log(LogLevel.Message, msg);
        }

        [HarmonyPatch(typeof(\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B), "FixedUpdate")]
        public static class GearPatch
        {
            public static void Prefix(\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B __instance)
            {
                if (_enableGearCollection.Value &&
                    !Traverse.Create(__instance).Field("[]][][[[]]][[]][][][[[][]]]][]][]]]]]]]][[[[[[[").GetValue<bool>())
                {
                    __instance.\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005D\u005B\u005B();
                }
            }
        }

        // static bool force_right, force_left;
        //
        // [HarmonyPatch(typeof(əɛɚəəɘəɚɛəɚɘɚɘəɛɚɚɚɛɘɘɘ), "ɚɘɚɘəɚɛɛɚəɚɘɚɚɛɚɘɚəəɛɛɛ")]
        // public static class MovementController
        // {
        //     public static bool Prefix()
        //     {
        //         if (ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ == null ||
        //             ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ == null)
        //             return false;
        //         bool flag = false;
        //         if (əɛɚəəɘəɚɛəɚɘɚɘəɛɚɚɚɛɘɘɘ.əəəɘɛɚɘɚəəəəəɛəɚɘɚɛəɘɚɛ() &&
        //             !GameChatBox.ɘɛɛɘəɚəɚɘəɘɚɚɛəɛɛɘɘəɛəɘ.ɛəɘɚɛɘɘɚɛɚəəəəɚɛɛɚɛɚəɚɛ &&
        //             (!ɘəɚɛɛəɛəɚɚɚəɚəəɛɛəɘɛɛɛɘ.ɛɚəɚəɛɛɛɘɘəɛəəəɚɛɛɘɘɘɚɘ() || !Application.isConsolePlatform) &&
        //             !ChatWheel.ɛɚəɚəɛɛɛɘɘəɛəəəɚɛɛɘɘɘɚɘ())
        //         {
        //             if (force_left ||
        //                 ControllerMap_Game.ɚəɛəɚəɘɘɛɚɘəɚɛəɘəəɛɘɚɛɚ == ControllerMap_Game.ɚəɚɘɚɚəɛɚɘɚɚɘəɛəɛɛəɚɚɛɘ.TANK &&
        //                 Inputs.əɘɘəəəɘɘɚəɛɘɛəɛɘəəɚɘɛɚə.GetNegativeButton("Move") ||
        //                 Inputs.əɘɘəəəɘɘɚəɛɘɛəɛɘəəɚɘɛɚə.GetNegativeButton("Move_PC"))
        //             {
        //                 float num = (float) əɛɚəəɘəɚɛəɚɘɚɘəɛɚɚɚɛɘɘɘ.ɘɚɚɛəəɚɘɚɚɛəɛɚɚɘɚɘəɛɚɚɚ(
        //                     (double) ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ
        //                         .ɘɛɘɛɛɛɘɘɚɚɘɘɛɘɘəəɛəɘəɚɛ,
        //                     (double) ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ
        //                         .ɛɘɚɛɘɘəəɛɚəɘəəəɘɘəɘəəɘɛ,
        //                     (double) ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ
        //                         .ɚɘɚɚɛɛɛɚɘɘɘɛɛəɛɘəɘɛəɛɚɘ, true);
        //                 if ((double) num > 0.0 && əɛɚəəɘəɚɛəɚɘɚɘəɛɚɚɚɛɘɘɘ.ɚɚɛəɚəəəɚɛəəɚəɛɘɚəɛɘɛɚə(
        //                         Mathf.FloorToInt(ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ
        //                             .ɘɛɘɛɛɛɘɘɚɚɘɘɛɘɘəəɛəɘəɚɛ - num), true))
        //                 {
        //                     ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ
        //                         .ɘɛɘɛɛɛɘɘɚɚɘɘɛɘɘəəɛəɘəɚɛ -= num;
        //                     --ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ
        //                         .əɘɛɚɘɘəɚɛɛɛɛəɛɘɚɘɘɛɘɘɚɚ;
        //                     flag = true;
        //                     Inputs.ɘɚɘɛɚɚɘɚɚɛɚɘɛəɘɚɚɚəəɚɚɛ(Inputs.əɘɘəəəɘɘɚəɛɘɛəɛɘəəɚɘɛɚə, true, 0.2f, 0.1f);
        //                 }
        //             }
        //             else if (force_right ||
        //                      ControllerMap_Game.ɚəɛəɚəɘɘɛɚɘəɚɛəɘəəɛɘɚɛɚ ==
        //                      ControllerMap_Game.ɚəɚɘɚɚəɛɚɘɚɚɘəɛəɛɛəɚɚɛɘ.TANK &&
        //                      Inputs.əɘɘəəəɘɘɚəɛɘɛəɛɘəəɚɘɛɚə.GetButton("Move") ||
        //                      Inputs.əɘɘəəəɘɘɚəɛɘɛəɛɘəəɚɘɛɚə.GetButton("Move_PC"))
        //             {
        //                 float num = (float) əɛɚəəɘəɚɛəɚɘɚɘəɛɚɚɚɛɘɘɘ.ɘɚɚɛəəɚɘɚɚɛəɛɚɚɘɚɘəɛɚɚɚ(
        //                     (double) ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ
        //                         .ɘɛɘɛɛɛɘɘɚɚɘɘɛɘɘəəɛəɘəɚɛ,
        //                     (double) ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ
        //                         .ɛɘɚɛɘɘəəɛɚəɘəəəɘɘəɘəəɘɛ,
        //                     (double) ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ
        //                         .ɚɘɚɚɛɛɛɚɘɘɘɛɛəɛɘəɘɛəɛɚɘ, false);
        //                 if ((double) num > 0.0 && əɛɚəəɘəɚɛəɚɘɚɘəɛɚɚɚɛɘɘɘ.ɚɚɛəɚəəəɚɛəəɚəɛɘɚəɛɘɛɚə(
        //                         Mathf.CeilToInt(ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ
        //                             .ɘɛɘɛɛɛɘɘɚɚɘɘɛɘɘəəɛəɘəɚɛ + num), false))
        //                 {
        //                     ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ
        //                         .ɘɛɘɛɛɛɘɘɚɚɘɘɛɘɘəəɛəɘəɚɛ += num;
        //                     --ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ
        //                         .əɘɛɚɘɘəɚɛɛɛɛəɛɘɚɘɘɛɘɘɚɚ;
        //                     flag = true;
        //                     Inputs.ɘɚɘɛɚɚɘɚɚɛɚɘɛəɘɚɚɚəəɚɚɛ(Inputs.əɘɘəəəɘɘɚəɛɘɛəɛɘəəɚɘɛɚə, true, 0.2f, 0.1f);
        //                 }
        //             }
        //         }
        //
        //         var unknown_traverse =
        //             Traverse.Create(typeof(əɛɚəəɘəɚɛəɚɘɚɘəɛɚɚɚɛɘɘɘ)).Field("ɚɘəɘɚɘəəəɘɚɚɛɛɘɛɘɚɛɘɛəɘ");
        //         if (flag)
        //             unknown_traverse.SetValue(unknown_traverse.GetValue<int>() + 1);
        //         else
        //             unknown_traverse.SetValue(0);
        //
        //         if (əɛɚəəɘəɚɛəɚɘɚɘəɛɚɚɚɛɘɘɘ.ɛɘɚəɚɛəɚɛɘəɘɘɛɛəɚɚɚɛɚɘɘ && !flag || unknown_traverse.GetValue<int>() >=
        //             Traverse.Create(typeof(əɛɚəəɘəɚɛəɚɘɚɘəɛɚɚɚɛɘɘɘ)).Field("ɘɘɚəɚɘəəɛɛɘɛəɘɛɚɛəɘɛɛəɘ").GetValue<int>())
        //             əɛɚəəɘəɚɛəɚɘɚɘəɛɚɚɚɛɘɘɘ.ɘɚəɚɚɚɚɘəəɚəɘəɛɛɛɛɘɛɛɚə();
        //         əɛɚəəɘəɚɛəɚɘɚɘəɛɚɚɚɛɘɘɘ.ɛɘɚəɚɛəɚɛɘəɘɘɛɛəɚɚɚɛɚɘɘ = flag;
        //         return false;
        //     }
        // }

        [HarmonyPatch(typeof(\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B), "FixedUpdate")]
        public static class ReadyBtnFixedUpdate
        {
            private static int i = 0;
        
            public static void Prefix(\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B __instance)
            {
                if (_enableAutoReady.Value)
                {
                    if (i != 64)
                    {
                        i++;
                        return;
                    }
        
                    i = 0;
                    var nowReady = __instance.\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005D.\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005B\u005D\u005B;
                    if (!nowReady)
                    {
                        MethodInfo clickButtonMethod = typeof(\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B).GetMethod("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance);
                        clickButtonMethod.Invoke(__instance, null);
                    }
                }
            }
        }

        // [HarmonyPatch(typeof(ItemListBox), "əɛɛɚɚəɚəɘɛəɚəɛɛɚɛɘɚəɚəɘ")]
        // public static class ItemBoxPrinter
        // {
        //     public static void Prefix(ItemListBox __instance)
        //     {
        //         Print(__instance.ɛɚəɘəɛəɚɚəɛɛəəəɛəɛəəɘɚɚ.ɘəɚɛɘɚɛɘəɛɚɘɚɛɚɘəɘɚəɘɚɚ);
        //     }
        // }

        [HarmonyPatch(typeof(\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B), "[][[[][]][[[[[[[[][]][[]]][]]][[]]]]][[[[]][[[[",
            new Type[] {typeof(string), typeof(object[])})]
        public static class MessageLogger
        {
            public static void Prefix(string _param1, object[] _param2)
            {
                if (!_enableMessageSentLogging.Value) return;
                var data_string = "[";
                foreach (var d in _param2)
                {
                    data_string += d + ", ";
                }

                data_string += "]";
                Print("[Message] " + _param1 + ": " + data_string);
            }
        }

        [HarmonyPatch(typeof(\u005B\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005B), "[[[]]][[[]]]]][]][][[]]]]]]][]]]]]]][][]][[][][")]
        public static class MessageReceiveLogger
        {
            public static void Prefix(object _param0, Message _param1)
            {
                var type = _param1.Type;
                if (type == null)
                    return;

                if (type == "Turn")
                {
                    bumper_list.Clear();
                    cbumper_list.Clear();
                    portal_list.Clear();
                }

                if (!_enableMessageReceivedLogging.Value) return;
                Print("[Message] " + _param1.Type);
            }
        }
        
        private static List<\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D> bumper_list;
        private static List<\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005D\u005D> cbumper_list;
        private static List<\u005D\u005D\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005D> portal_list;

        [HarmonyPatch(typeof(\u005D\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D), "][[]][[[[[][]][[[[]][[[[[[[[][][[][[[][[]]]]]]]")] // Bumper from Message Received
        class BumperPatch
        {
            static void Postfix(\u005D\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D __instance)
            {
                var bumper_type = typeof(\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D);
                var cbumper_type = typeof(\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005D\u005D);
                var portal_type = typeof(\u005D\u005D\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005D);

                foreach (var props in __instance.\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005B)
                {
                    if (props.GetType() == bumper_type)
                    {
                        bumper_list.Add((\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D) props);
                    }
                    else if (props.GetType() == cbumper_type)
                    {
                        cbumper_list.Add((\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005D\u005D) props);
                    }
                    else if (props.GetType() == portal_type)
                    {
                        portal_list.Add((\u005D\u005D\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005D) props);
                    }
                }
            }
        }

        static float x,
            y,
            angle,
            power,
            wind,
            len,
            offset,
            color_hue,
            color_saturation,
            color_value,
            bounce_force,
            radius_modifier;

        static double g, v, ww;
        
        private void Awake()
        {
            // Plugin startup logic
            Log = Logger;
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Harmony.PatchAll();

            _enableTracers = Config.Bind("Tracer", "Enable", true, "启用追踪线");
            _enableGearCollection = Config.Bind("Gear Collection", "Enable", true, "启用自动收集齿轮");
            _enableRainbowTracers = Config.Bind("Tracer", "Enable Rainbow \u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005D\u005B\u005B\u005D", true, "启用RGB追踪");
            _enableAutoReady = Config.Bind("Auto Farm", "Auto Ready Enable", true, "启用自动准备");
            // _enableMoveTowardCenter = Config.Bind("Auto Farm", "Move Toward Center Enable", true,
            //     "向中间移动并在第三回合开始向上射击，在点数XP Farm中很有用。");
            _enableMessageSentLogging =
                Config.Bind("Message Logging", "Message Sent Enable", false, "启用发送消息记录");
            _enableMessageReceivedLogging =
                Config.Bind("Message Logging", "Message Received Enable", false, "启用接收消息记录");
            _enablePortalTracers =
                Config.Bind("Tracer", "Enable Portal \u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005D\u005B\u005B\u005D", true, "让你知道哪个传送门连接到哪个传送门，在 Vortex 模式使用");
            
            _cycleInterval = Config.Bind("General", "Cycle Interval", 8, "每次循环的间隔数量，越低每个主循环越快但是会降低性能");
            
            _tracerTimeInterval = Config.Bind("Tracer", "Tracer Time Interval", 0.005f, "追踪线的时间间隔，越低越精确但是会降低性能");
            _tracerTotalTime = Config.Bind("Tracer", "Tracer Total Time", 4f, "追踪线的总时间，越长会延长距离但是会降低性能");
            _tracerEnableBumperCalculation = Config.Bind("Tracer", "Enable Bumper Calculation", true,
                "启用弹力板和球的碰撞计算，这会使追踪线更精确但是会降低性能");
            _tracerEnablePortalCalculation =
                Config.Bind("Tracer", "Enable Portal Calculation", true, "启用传送门的碰撞计算，这会使追踪线更精确但是会降低性能");

            // force_left = false;
            // force_right = false;

            bumper_list = new List<\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D>();
            cbumper_list = new List<\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005D\u005D>();
            portal_list = new List<\u005D\u005D\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005D>();
            
            angle = 85;
            power = 100;
            g = 9.8d;
            v = 0.113d;
            wind = 0;
            ww = 0.0125d;
            len = 0;
            offset = 0.125f;
            bounce_force = 0.5f;
            radius_modifier = 0.96f;

            color_hue = 0.5f;
            color_saturation = 1f;
            color_value = 1f;
        }

        public void Update()
        {
            if (!_enableTracers.Value)
            {
                return;
            }

            len++;
            if (len >= _cycleInterval.Value)
            {
                len = 0;
            }
            else
            {
                return;
            }

            var tanks = FindObjectsOfType(typeof(\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005B));
            var found = false;
            \u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005B aimer = null;

            foreach (\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005B tank in tanks)
            {
                if (tank.\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005D == \u005D\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D.\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D.\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005B\u005D\u005D\u005D\u005B)
                {
                    x = tank.transform.position.x;
                    y = tank.transform.position.y;
                    aimer = Traverse.Create(tank).Field("[[[]][][[]][[[[[[]]]][][[][[][]][][[[[]]][[[]][").GetValue<\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005B>();
                    angle = 90 - aimer.\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B;
                    if (angle > 360)
                    {
                        angle -= 360;
                    }

                    power = aimer.\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B;
                    found = true;
                }
            }

            if (!found)
            {
                return;
            }

            // // Automatic move towards the 0 
            // if (!ɚɘɛɚəɛɛɚɚəɛɘəɛəɚɛɛɘəɚəɚ.əɛəɛəɚɛɚɘɚɚɚɛɘɛɛɘəɛəɘəə && _enableMoveTowardCenter.Value) // If not locked in
            // {
            //     if (x < -0.05)
            //     {
            //         force_right = true;
            //         force_left = false;
            //     }
            //     else if (x > 0.05)
            //     {
            //         force_right = false;
            //         force_left = true;
            //     }
            //     else
            //     {
            //         force_right = false;
            //         force_left = false;
            //         if (ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘɚɚɚəɛɚɘɘɚɛəɛəɚɛɘɘɚəɘɘ + 1 >= 3)
            //         {
            //             aimer.əəɘɚɚɚɚɘəɘɚəɚɛəəəɚɛɘɛɛɚ = 0;
            //             aimer.ɚəəɘəɚɛɚəəɚɛɚɛəɘɛəɚəɘɛɘ = 15;
            //
            //             ɚɘɛɚəɛɛɚɚəɛɘəɛəɚɛɛɘəɚəɚ.əɚɛɛəɚəɛɛɚɚɘəɚəɛəɛəəɘɚə(); // Shoot
            //         }
            //         else
            //         {
            //             ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɛɛɘɚɛɛɚɚɛɚəɛɘɘəɘɚɚəɛɚɘ("overcharge");
            //         }
            //     }
            // }

            var wind_dec = Traverse.Create(\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D.\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005B).Field("[]]][[][[]][[[[][][]]]]][[[][]]][]]]]][[]]][]]]").GetValue<float>();
            wind = wind_dec * 100;
            var time_interval = _tracerTimeInterval.Value;
            var time_limit = _tracerTotalTime.Value;

            \u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005D\u005B\u005B\u005D.\u005D\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D(); // Tracer.Clear()
            var color = Color.white;
            if (_enableRainbowTracers.Value)
            {
                color = Color.HSVToRGB(color_hue, color_saturation, color_value);
                color_hue += time_interval;
                if (color_hue > 1)
                {
                    color_hue = 0;
                }
            }

            \u005B\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005D\u005D\u005B tracer_controller =
                new \u005B\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005D\u005D\u005B(new VectorLine("Tracer", new List<Vector3>(), null, 4f, LineType.Discrete),
                    color);
            \u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005D\u005B\u005B\u005D.\u005B\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005D.Add(tracer_controller);

            double radians;

            radians = angle * (Math.PI / 180);

            x += offset * (float) Math.Cos(radians);
            y += offset * (float) Math.Sin(radians);
            Vector2 projectile_position = new Vector2(x, y);
            double velocity = power * v;
            Vector2 projectile_velocity = new Vector2((float) (velocity * Math.Cos(radians)),
                (float) (velocity * Math.Sin(radians)));
            Vector2 gravity_a = new Vector2(0, (float) -g);
            Vector2 wind_a = new Vector2((float) (wind * ww), 0);

            Vector2 projectile_acceleration = gravity_a + wind_a;

            Vector2 last_position = projectile_position;

            bool teleported = false;

            // if (_enablePortalTracers.Value)
            // {
            //     foreach (var portal in portal_list)
            //     {
            //         Vector3 portal1_pos = portal.\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D.transform.Find("P1").position;
            //         Vector3 portal2_pos = portal.\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D.transform.Find("P2").position;
            //         color = RandomColorBaseOnVector3(portal1_pos + portal2_pos);
            //         DrawLine(portal1_pos, portal2_pos, color, 0.1f);
            //     }
            // }

            for (float t = 0; t < time_limit; t += time_interval)
            {
                projectile_velocity += projectile_acceleration * time_interval;
                projectile_position += projectile_velocity * time_interval;

                // Check collision with bumper, if hit bounce
                // This bumper is a line that have no width
                if (_tracerEnableBumperCalculation.Value)
                {
                    foreach (var bumper in bumper_list)
                    {
                        GameObject bumper_obj = bumper.\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D;
                        float bumper_radius = bumper_obj.transform.localScale.y * radius_modifier;
                        Vector2 bumper_center = bumper_obj.transform.position;

                        // Optimization: if the projectile is not in the bumper radius, skip it
                        if (Vector2.Distance(projectile_position, bumper_center) > bumper_radius)
                        {
                            continue;
                        }

                        var bumper_rotation = bumper_obj.transform.rotation.eulerAngles.z;

                        // The bumper is vertical when rotation is 0 or 180
                        var bumer_start = bumper_center + new Vector2(0, bumper_radius);
                        var bumper_end = bumper_center + new Vector2(0, -bumper_radius);

                        // Rotate the bumper if it is not vertical
                        if (bumper_rotation != 0 && bumper_rotation != 180)
                        {
                            bumer_start = RotatePointAroundPivot(bumer_start, bumper_center,
                                new Vector3(0, 0, bumper_rotation));
                            bumper_end = RotatePointAroundPivot(bumper_end, bumper_center,
                                new Vector3(0, 0, bumper_rotation));
                        }

                        var intersection = IsIntersect(last_position, projectile_position, bumer_start, bumper_end);

                        if (intersection)
                        {
                            // bumper is like a mirror, so we need to calculate the new velocity

                            // Get the normal of the bumper
                            var normal = bumper_end - bumer_start;
                            normal = new Vector2(normal.y, -normal.x);
                            normal.Normalize();

                            // Calculate the new velocity
                            var new_velocity = projectile_velocity -
                                               2 * Vector2.Dot(projectile_velocity, normal) * normal;

                            // Set the new velocity
                            projectile_velocity = new_velocity;
                            projectile_position += projectile_velocity * time_interval;
                        }
                    }

                    // These bumper is a circle
                    foreach (var bumper in cbumper_list)
                    {
                        GameObject bumper_obj = bumper.\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D;
                        float bumper_radius = bumper_obj.transform.localScale.y * radius_modifier;
                        Vector2 bumper_center = bumper_obj.transform.position;

                        // Optimization: if the projectile is not in the bumper radius, skip it
                        if (Vector2.Distance(projectile_position, bumper_center) > bumper_radius)
                        {
                            continue;
                        }

                        var intersection = ClosestIntersection(bumper_center.x, bumper_center.y, bumper_radius,
                            last_position,
                            projectile_position);

                        if (intersection != Vector2.zeroVector)
                        {
                            // Find the normal of the circle at the intersection point
                            var normal = intersection - bumper_center;
                            normal.Normalize();

                            // Use the intersection point as the new projectile position
                            projectile_position = intersection;

                            // Calculate the new velocity
                            var new_velocity = projectile_velocity -
                                               2 * Vector2.Dot(projectile_velocity, normal) * normal;

                            // Set the new velocity
                            projectile_velocity = new_velocity;

                            // Calculate the time taken to reach the intersection point
                            var time_taken = Vector2.Distance(last_position, intersection) /
                                             projectile_velocity.magnitude;

                            // Add to the time to get the new projectile position
                            projectile_position += projectile_velocity * (time_interval - time_taken);
                        }
                    }
                }

                if (_tracerEnablePortalCalculation.Value)
                {
                    var not_in_portal = true;
                    // Check collision with portals, if hit teleport to the other portal
                    foreach (var portal in portal_list)
                    {
                        var portal1_location = (Vector2) portal.\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D.transform.Find("P1").position;
                        var portal2_location = (Vector2) portal.\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D.transform.Find("P2").position;

                        var portal1_scale = (Vector2) portal.\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D.transform.Find("P1").localScale;
                        var portal2_scale = (Vector2) portal.\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005B\u005B\u005B\u005D\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D.transform.Find("P2").localScale;

                        // When the portal is at 1 scale it have a radius of  0.55 units
                        var portal1_radius = portal1_scale.y * 0.55f;
                        var portal2_radius = portal2_scale.y * 0.55f;

                        // The teleport will teleport the projectile to the relative position of the other portal
                        // Check if the projectile is in the portal1 radius
                        if (Vector2.Distance(projectile_position, portal1_location) < portal1_radius)
                        {
                            not_in_portal = false;
                            if (teleported)
                            {
                                break;
                            }

                            // Calculate the relative position of the projectile to the portal1
                            var relative_position = projectile_position - portal1_location;

                            // Teleport the projectile to the portal2
                            projectile_position = portal2_location + relative_position;

                            teleported = true;
                        }
                        else if (Vector2.Distance(projectile_position, portal2_location) < portal2_radius)
                        {
                            not_in_portal = false;
                            if (teleported)
                            {
                                break;
                            }

                            // Calculate the relative position of the projectile to the portal2
                            var relative_position = projectile_position - portal2_location;

                            // Teleport the projectile to the portal1
                            projectile_position = portal1_location + relative_position;

                            teleported = true;
                        }
                    }

                    if (not_in_portal && teleported)
                    {
                        teleported = false;
                    }
                }

                addVectorToVectorLine(new Vector3(projectile_position.x, projectile_position.y, 6.0f),
                    tracer_controller);
                last_position = projectile_position;
            }

            tracer_controller.\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005D.Draw3D();

            // var random_pos = new Vector3(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10));
            // var random_end = new Vector3(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10));
            // DrawLine(random_pos, random_end, Color.red);
        }

        public bool IsZero(float value)
        {
            return Math.Abs(value) < 0.0001f;
        }

        public Vector2 ClosestIntersection(float cx, float cy, float radius, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 intersection1;
            Vector2 intersection2;
            int intersections = FindLineCircleIntersections(cx, cy, radius, lineStart, lineEnd, out intersection1,
                out intersection2);

            if (intersections == 1)
                return intersection1; // one intersection

            if (intersections == 2)
            {
                double dist1 = Distance(intersection1, lineStart);
                double dist2 = Distance(intersection2, lineStart);

                if (dist1 < dist2)
                    return intersection1;
                else
                    return intersection2;
            }

            return Vector2.zeroVector; // no intersections at all
        }

        public Color RandomColorBaseOnVector3(Vector3 vector3)
        {
            Random.InitState((int) ((vector3.x + vector3.y + vector3.z) * 10000));
            return Color.HSVToRGB(Random.Range(0, 1f), 1, 1);
        }

        private double Distance(Vector2 p1, Vector2 p2)
        {
            return Math.Sqrt(Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2.y - p1.y, 2));
        }

// Find the points of intersection.
        private int FindLineCircleIntersections(float cx, float cy, float radius,
            Vector2 point1, Vector2 point2, out
                Vector2 intersection1, out Vector2 intersection2)
        {
            float dx, dy, A, B, C, det, t;

            dx = point2.x - point1.x;
            dy = point2.y - point1.y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (point1.x - cx) + dy * (point1.y - cy));
            C = (point1.x - cx) * (point1.x - cx) + (point1.y - cy) * (point1.y - cy) - radius * radius;

            det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                // No real solutions.
                intersection1 = new Vector2(float.NaN, float.NaN);
                intersection2 = new Vector2(float.NaN, float.NaN);
                return 0;
            }
            else if (det == 0)
            {
                // One solution.
                t = -B / (2 * A);
                intersection1 = new Vector2(point1.x + t * dx, point1.y + t * dy);
                intersection2 = new Vector2(float.NaN, float.NaN);
                return 1;
            }
            else
            {
                // Two solutions.
                t = (float) ((-B + Math.Sqrt(det)) / (2 * A));
                intersection1 = new Vector2(point1.x + t * dx, point1.y + t * dy);
                t = (float) ((-B - Math.Sqrt(det)) / (2 * A));
                intersection2 = new Vector2(point1.x + t * dx, point1.y + t * dy);
                return 2;
            }
        }

        public bool IsIntersect(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
        {
            Vector AC = new Vector(A, C);
            Vector AD = new Vector(A, D);
            Vector BC = new Vector(B, C);
            Vector BD = new Vector(B, D);
            Vector CA = AC.Negative();
            Vector CB = BC.Negative();
            Vector DA = AD.Negative();
            Vector DB = BD.Negative();

            return (AC.VectorProduct(AD) * BC.VectorProduct(BD) <= 1e-9) &&
                   (CA.VectorProduct(CB) * DA.VectorProduct(DB) <= 1e-9);
        }

        public void addVectorToVectorLine(Vector3 pos, \u005B\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005B\u005D\u005D\u005D\u005B tracer_controller)
        {
            ++tracer_controller.\u005D\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005B;
            if (tracer_controller.\u005D\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005B % 1 == 0)
                tracer_controller.\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005B\u005D\u005B\u005B\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005B\u005D\u005D\u005B\u005B\u005D.points3.Add(pos);
        }

        public Vector2 RotatePointAroundPivot(Vector2 point, Vector2 pivot, Vector3 angles)
        {
            Vector2 dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }

        // void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
        // {
        //     GameObject myLine = new GameObject();
        //     myLine.transform.position = start;
        //     myLine.AddComponent<LineRenderer>();
        //     LineRenderer lr = myLine.GetComponent<LineRenderer>();
        //     lr.material = new Material(Shader.FindBuiltin("Standard Shader"));
        //     lr.SetColors(color, color);
        //     lr.SetWidth(0.05f, 0.05f);
        //     lr.SetPosition(0, start);
        //     lr.SetPosition(1, end);
        //     GameObject.Destroy(myLine, duration);
        // }
    }
}

// [HarmonyPatch(typeof(Proj), "ɛɚɘəɚɘəɛɛɛəɘɘɘɘɘɛəɘɚəɛə")]
// public static class ProjPatch
// {
//     public static void Postfix(Proj __instance, ɘɚɘɚɛɘɛɛɛɛəɚɚəɚɚɚɛɘɘɘɚɘ ɛɚɛɛɚɚɛəɚɘɚəɛəɘəɚɛəɘɚəɘ)
//     {
//         Print("ProjUpdate" + __instance.ɛɘɚɚɚɚɚəɘɘɘəɚɚəəɚɛɛəɛɘə);
//         \u005D\u005B\u005D\u005D\u005D\u005B\u005D\u005B\u005D\u005D\u005D\u005D\u005B\u005D\u005B\u005B\u005D\u005D\u005B\u005D\u005D\u005B\u005B\u005D\u005D\u005D\u005D\u005B\u005B\u005D\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005B\u005D\u005B\u005D\u005B\u005B\u005D\u005B\u005B\u005D.ɛɘəɛɚɚəɘɘəɚɛəɚəɘɚɛɘəɛɛə(__instance,
//             Color.Lerp(
//                 ɘɚəɘɛɘɚɚɛɘəɘɘɘɘɚəɘɘɘɚɘɛ.ɛəɘɘəɚɛɘəɘɘəɘɛəɚɛəɘɘəɘɘ(ɛɚɛɛɚɚɛəɚɘɚəɛəɘəɚɛəɘɚəɘ
//                     .ɛɛɚəəəɛəɘɚɘəəɛɚɘəəɚɘɛəɚ),
//                 ɘɚəɘɛɘɚɚɛɘəɘɘɘɘɚəɘɘɘɚɘɛ.ɛəɘɘəɚɛɘəɘɘəɘɛəɚɛəɘɘəɘɘ(ɛɚɛɛɚɚɛəɚɘɚəɛəɘəɚɛəɘɚəɘ
//                     .ɘəɛɘɛɚəɘɚɚɛɘɛɚɘɚəɛɚɛɘɘə), 0.5f));
//     }
// }

// [HarmonyPatch(typeof(WeaponSelector), "ɚɛəɛɘɚəɛɚəəəɘɘɘɘɚɚəəɚɛɛ")]
// public static class WeaponSelectorPatch
// {
//     public static bool Prefix(ref List<əəɛɛɛəɚəəɚɘəɛɛɘɚəɛɛɛɚɛə> __result, WeaponSelector __instance)
//     {
//         // List<əəɛɛɛəɚəəɚɘəɛɛɘɚəɛɛɛɚɛə> əəɛɛɛəɚəəɚɘəɛɛɘɚəɛɛɛɚɛəList = new List<əəɛɛɛəɚəəɚɘəɛɛɘɚəɛɛɛɚɛə>();
//         // əəɛɛɛəɚəəɚɘəɛɛɘɚəɛɛɛɚɛəList.Sort(sortByName);
//         // __result = əəɛɛɛəɚəəɚɘəɛɛɘɚəɛɛɛɚɛəList;
//         ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ.ɘɛɛɚɘɘɛɛɚɚɘɚɚəɘəɛɘəɘəəɛ
//             .əəɘɘɛɛɘɚəɛəəɚɚɚəəəɘɘɘɛɘ.Clear();
//         foreach (əəɛɛɛəɚəəɚɘəɛɛɘɚəɛɛɛɚɛə weapon in ɚɚɘɘəɛɘəəəɘɚəɘɛɘɘɘɛɛɚɘɚ.əɘɚɚɚəəɚɛɘəɚəɛɛɘɘɘəəɚɚɘ.ɛɚəɚɘəəɛɛɚɛɚɛɚɘəəɚɘɛɘəɘ)
//         {
//             var tierCount = weapon.əɚɛəɚɛɛɛɛɘəɚɛəɚɚəɘɛəɘɚɚ.Length;
//             // Get a array of int that contain {tierCount} of 100
//             var tierWeights = Enumerable.Repeat(100, tierCount).ToArray();
//             ɚɘɛɘəɛɚɚɚɚɘɚɚəɚəɛɛɘɚɛɘɘ.ɘɘəəəɘɘəɘɛəɚəɘɚɛɚəɘɚəɚɛ.ɚɘəɘɚɘəɘəɘɚɛɚɛɛɚəɛɚɚɚɘɘ.ɘɛɛɚɘɘɛɛɚɚɘɚɚəɘəɛɘəɘəəɛ
//                 .əəɘɘɛɛɘɚəɛəəɚɚɚəəəɘɘɘɛɘ.Add(weapon.ɘəɚɛɘɚɛɘəɛɚɘɚɛɚɘəɘɚəɘɚɚ, tierWeights);
//         }
//         return true;
//     }
// }