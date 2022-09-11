using ApacheTech.VintageMods.FluentChatCommands;
using Gantry.Core;
using Gantry.Core.ModSystems;
using System.Text;
using ApacheTech.Common.DependencyInjection.Abstractions;
using ApacheTech.Common.DependencyInjection.Abstractions.Extensions;
using Gantry.Core.DependencyInjection.Registration;
using Gantry.Services.FileSystem.DependencyInjection;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Gantry.Core.DependencyInjection;
using Gantry.Services.Network;
using Vintagestory.API.Client;
using Gantry.Services.HarmonyPatches.Annotations;
using JetBrains.Annotations;
using HarmonyLib;
using System.Linq;
using System;
using ApacheTech.Common.Extensions.Harmony;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
using Vintagestory.API.MathTools;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.Knapster.Features.EasySmithing
{
    [HarmonySidedPatch(EnumAppSide.Universal)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EasySmithing : UniversalModSystem, IServerServiceRegistrar
    {
        private static EasySmithingSettings _serverSettings = new();
        private static EasySmithingPacket _clientSettings;
        private static bool Enabled => ApiEx.OneOf(_clientSettings.Enabled, _serverSettings.Enabled);
        private static int CostPerClick => ApiEx.OneOf(_clientSettings.CostPerClick, _serverSettings.CostPerClick);

        private IServerNetworkChannel _serverChannel;

        public void ConfigureServerModServices(IServiceCollection services)
        {
            services.AddFeatureWorldSettings<EasySmithingSettings>();
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            _serverSettings = IOC.Services.Resolve<EasySmithingSettings>();
            FluentChat.ServerCommand("knapster")
                .HasSubCommand("smithing").WithHandler(OnSmithingCommand)
                .HasSubCommand("s").WithHandler(OnSmithingCommand);

            _serverChannel = IOC.Services.Resolve<IServerNetworkService>()
                .DefaultServerChannel
                .RegisterMessageType<EasySmithingPacket>();

            api.Event.PlayerJoin += player =>
            {
                _serverChannel.SendPacket(EasySmithingPacket.FromSettings(_serverSettings), player);
            };
        }

        private void OnSmithingCommand(string subCommandName, IServerPlayer player, int groupId, CmdArgs args)
        {
            var sb = new StringBuilder();
            switch (args.PopWord())
            {
                case "1":
                case "true":
                case "enable":
                case "enabled":
                    _serverSettings.Enabled = true;
                    _serverChannel.BroadcastPacket(EasySmithingPacket.FromSettings(_serverSettings));
                    break;

                case "0":
                case "false":
                case "disable":
                case "disabled":
                    _serverSettings.Enabled = false;
                    _serverChannel.BroadcastPacket(EasySmithingPacket.FromSettings(_serverSettings));
                    break;

                case "c":
                case "d":
                case "cost":
                case "durability":
                    _serverSettings.CostPerClick = GameMath.Clamp(args.PopInt().GetValueOrDefault(1), 1, 10);
                    _serverChannel.BroadcastPacket(EasySmithingPacket.FromSettings(_serverSettings));
                    break;
            }
            sb.AppendLine(LangEx.FeatureString("EasySmithing", "Enabled", LangEx.BooleanString(_serverSettings.Enabled)));
            sb.Append(LangEx.FeatureString("EasySmithing", "Cost", _serverSettings.CostPerClick));
            if (sb.Length > 0) Sapi.SendMessage(player, groupId, sb.ToString(), EnumChatType.Notification);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            IOC.Services.Resolve<IClientNetworkService>()
                .DefaultClientChannel
                .RegisterMessageType<EasySmithingPacket>()
                .SetMessageHandler<EasySmithingPacket>(packet =>
                {
                    _clientSettings = packet;
                });
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemHammer), nameof(ItemHammer.GetToolModes))]
        public static void ClientPatch_ItemHammer_GetToolModes_Postfix(ItemHammer __instance, ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel, ref SkillItem[] __result, ref SkillItem[] ___toolModes)
        {
            if (__result is null || !Enabled)
            {
                if (__instance.GetToolMode(slot, forPlayer, blockSel) > 0)
                    __instance.SetToolMode(slot, forPlayer, blockSel, 0);
                ___toolModes = ___toolModes.Take(6).ToArray();
                __result = ___toolModes.Take(6).ToArray();
                return;
            }
            if (ApiEx.Side.IsServer()) return;

            if (!ModEx.IsCurrentlyOnMainThread()) return;
            if (___toolModes.Length > 6) return;
            var skillItem = new SkillItem
            {
                Code = new AssetLocation("auto"),
                Name = Lang.Get("Auto Complete", Array.Empty<object>())
            }.WithIcon(ApiEx.Client, ApiEx.Client.Gui.Icons.Drawfloodfill_svg);
            ___toolModes = ___toolModes.AddToArray(skillItem);
            __result = __result.AddToArray(skillItem);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlockEntityAnvil), "OnUseOver", typeof(IPlayer), typeof(Vec3i), typeof(BlockSelection))]
        public static bool UniversalPatch_BlockEntityClayForm_OnUseOver_Prefix(BlockEntityAnvil __instance,
            IPlayer byPlayer, Vec3i voxelPos, BlockSelection blockSel)
        {
            var slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Itemstack is null || !__instance.CanWorkCurrent) return true;
            if (slot.Itemstack.Collectible is not ItemHammer hammer) return true;
            var toolMode = hammer.GetToolMode(slot, byPlayer, blockSel);

            if (!Enabled)
            {
                if (toolMode > 5) hammer.SetToolMode(slot, byPlayer, blockSel, 0);
                return true;
            }

            if (toolMode < 6) return true;

            if (__instance.Api.Side.IsClient())
            {
                __instance.CallMethod("SendUseOverPacket", byPlayer, voxelPos);
            }
            OnHit(__instance);
            __instance.CallMethod("RegenMeshAndSelectionBoxes");
            __instance.Api.World.BlockAccessor.MarkBlockDirty(__instance.Pos);
            __instance.Api.World.BlockAccessor.MarkBlockEntityDirty(__instance.Pos);
            slot.Itemstack.Collectible.DamageItem(__instance.Api.World, byPlayer.Entity, slot, CostPerClick);
            if (!__instance.CallMethod<bool>("HasAnyMetalVoxel"))
            {
                __instance.CallMethod("clearWorkSpace");
                return false;
            }

            __instance.CheckIfFinished(byPlayer);
            __instance.MarkDirty();

            return false;
        }

        private static void OnHit(BlockEntityAnvil anvil)
        {
            var recipe = anvil.SelectedRecipe;
            var yMax = recipe.QuantityLayers;
            var usableMetalVoxel = anvil.CallMethod<Vec3i>("findFreeMetalVoxel");
            for (var x = 0; x < 16; x++)
            {
                for (var z = 0; z < 16; z++)
                {
                    for (var y = 0; y < 6; y++)
                    {
                        var requireMetalHere = y < yMax && recipe.Voxels[x, y, z];
                        var mat = (EnumVoxelMaterial)anvil.Voxels[x, y, z];
                        if (mat == EnumVoxelMaterial.Slag)
                        {
                            anvil.Voxels[x, y, z] = 0;
                            anvil.CallMethod("onHelveHitSuccess", mat, null, x, y, z);
                            return;
                        }

                        if (!requireMetalHere || usableMetalVoxel == null || mat != EnumVoxelMaterial.Empty) continue;
                        anvil.Voxels[x, y, z] = 1;
                        anvil.Voxels[usableMetalVoxel.X, usableMetalVoxel.Y, usableMetalVoxel.Z] = 0;
                        anvil.CallMethod("onHelveHitSuccess", mat, usableMetalVoxel, x, y, z);
                        return;
                    }
                }
            }

            if (usableMetalVoxel is null) return;
            anvil.Voxels[usableMetalVoxel.X, usableMetalVoxel.Y, usableMetalVoxel.Z] = 0;
            anvil.CallMethod("onHelveHitSuccess", EnumVoxelMaterial.Metal, null, usableMetalVoxel.X, usableMetalVoxel.Y, usableMetalVoxel.Z);
        }
    }
}
