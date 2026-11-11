using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using HarmonyLib;
using Planetbase;

namespace PlanetbaseColonyLeisure
{
    internal static class LeisureActivities
    {
        internal const int CasinoStake = 2;
        internal const float RoundSeconds = 30f;
        private static readonly ConditionalWeakTable<Interaction, CasinoRound> Rounds =
            new ConditionalWeakTable<Interaction, CasinoRound>();

        internal sealed class CasinoRound
        {
            internal bool Paid;
            internal bool Settled;
            internal float Elapsed;
        }

        private static bool IsCasino(Interaction interaction) =>
            interaction.getSelectable() is ConstructionComponent component &&
            component.getComponentType() is CasinoTerminal;

        [HarmonyPatch(typeof(Selectable), nameof(Selectable.getPotentialUserCount))]
        private static class AvoidUnaffordableCasino
        {
            private static void Postfix(Selectable __instance, ref int __result)
            {
                if (__instance is ConstructionComponent component && component.getComponentType() is CasinoTerminal &&
                    Resource.getCountOfType(ResourceTypeList.CoinsInstance) < CasinoStake)
                    __result = int.MaxValue;
            }
        }

        [HarmonyPatch(typeof(InteractionComponentRestore), nameof(InteractionComponentRestore.update))]
        private static class Visit
        {
            private static bool Prefix(InteractionComponentRestore __instance, ref bool __result,
                out CasinoRound __state)
            {
                __state = null;
                if (!(__instance.getSelectable() is ConstructionComponent component) ||
                    !(component.getComponentType() is LeisureEquipment))
                    return true;

                if (component.getComponentType() is DanceFloor &&
                    __instance.getCharacter().isStatusVeryLow(CharacterIndicator.Sleep))
                {
                    __result = true;
                    return false;
                }
                if (!IsCasino(__instance))
                    return true;

                __state = Rounds.GetOrCreateValue(__instance);
                if (__state.Paid)
                    return true;

                if (component.isOperational() &&
                    Resource.removeInmaterialResource(new ResourceAmount(ResourceTypeList.CoinsInstance, CasinoStake)))
                {
                    __state.Paid = true;
                    return true;
                }
                __result = true;
                return false;
            }

            private static void Postfix(InteractionComponentRestore __instance, float timeStep,
                bool __result, CasinoRound __state)
            {
                if (__result || !(__instance.getSelectable() is ConstructionComponent component))
                    return;

                Character character = __instance.getCharacter();
                if (component.getComponentType() is ArcadeCabinet && HasPartner(component))
                    character.restoreStatus(CharacterIndicator.Morale, timeStep / 360f);
                if (component.getComponentType() is DanceFloor)
                    character.decayIndicator(CharacterIndicator.Sleep, timeStep / 900f);

                if (__state == null || !__state.Paid || __state.Settled)
                    return;

                __state.Elapsed += timeStep;
                if (__state.Elapsed >= RoundSeconds)
                {
                    int roll = UnityEngine.Random.Range(0, 100);
                    int prize = roll < 60 ? 0 : roll < 90 ? 2 : 6;
                    AddCoins(prize);
                    __state.Settled = true;
                    ColonyLeisurePlugin.Log.LogInfo($"Casino · apuesta {CasinoStake}, premio {prize} monedas.");
                }
            }
        }

        private static bool HasPartner(ConstructionComponent current)
        {
            foreach (ConstructionComponent other in current.getParentConstruction().getComponents())
            {
                if (other != current && other.getComponentType() is ArcadeCabinet &&
                    other.isOperational() && other.anyInteractions())
                    return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(InteractionComponent), nameof(InteractionComponent.destroy))]
        private static class RefundInterruptedRound
        {
            private static void Prefix(InteractionComponent __instance)
            {
                if (Rounds.TryGetValue(__instance, out CasinoRound round) && round.Paid && !round.Settled)
                {
                    AddCoins(CasinoStake);
                    round.Settled = true;
                }
            }
        }

        private static void AddCoins(int amount)
        {
            int balance = Resource.getCountOfType(ResourceTypeList.CoinsInstance);
            int credited = Math.Min(amount, int.MaxValue - balance);
            if (credited > 0)
                Resource.addInmaterialResource(new ResourceAmount(ResourceTypeList.CoinsInstance, credited));
        }

        [HarmonyPatch(typeof(InteractionComponent), "serialize")]
        private static class SaveRound
        {
            private static void Postfix(InteractionComponent __instance, XmlNode parent)
            {
                if (!IsCasino(__instance))
                    return;

                CasinoRound round = Rounds.GetOrCreateValue(__instance);
                XmlElement node = parent.OwnerDocument.CreateElement("colony-leisure-casino");
                node.SetAttribute("paid", XmlConvert.ToString(round.Paid));
                node.SetAttribute("settled", XmlConvert.ToString(round.Settled));
                node.SetAttribute("elapsed", XmlConvert.ToString(round.Elapsed));
                parent.LastChild.AppendChild(node);
            }
        }

        [HarmonyPatch(typeof(InteractionComponent), "deserialize")]
        private static class RestoreRound
        {
            private static void Postfix(InteractionComponent __instance, XmlNode node)
            {
                if (!IsCasino(__instance) || !(node["colony-leisure-casino"] is XmlElement saved))
                    return;

                CasinoRound round = Rounds.GetOrCreateValue(__instance);
                round.Paid = XmlConvert.ToBoolean(saved.GetAttribute("paid"));
                round.Settled = XmlConvert.ToBoolean(saved.GetAttribute("settled"));
                round.Elapsed = XmlConvert.ToSingle(saved.GetAttribute("elapsed"));
                if (float.IsNaN(round.Elapsed) || float.IsInfinity(round.Elapsed) || round.Elapsed < 0f)
                    throw new FormatException("La ronda guardada del casino tiene una duración inválida.");
            }
        }

        [HarmonyPatch(typeof(ConstructionComponent), nameof(ConstructionComponent.getDescription))]
        private static class DescribeCasino
        {
            private static void Postfix(ConstructionComponent __instance, ref string __result)
            {
                if (__instance.getComponentType() is CasinoTerminal)
                    __result += $"\nApuesta por ronda · {CasinoStake} monedas\nSaldo de la colonia · " +
                        Resource.getCountOfType(ResourceTypeList.CoinsInstance);
            }
        }
    }
}
