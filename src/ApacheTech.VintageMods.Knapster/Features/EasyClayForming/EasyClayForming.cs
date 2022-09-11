using System.Linq;
using ApacheTech.Common.DependencyInjection.Abstractions;
using ApacheTech.Common.DependencyInjection.Abstractions.Extensions;
using ApacheTech.VintageMods.FluentChatCommands;
using Gantry.Core.DependencyInjection.Registration;
using Gantry.Core.DependencyInjection;
using Gantry.Core.ModSystems;
using Gantry.Services.FileSystem.DependencyInjection;
using Vintagestory.API.Server;
using Gantry.Core;
using System.Text;
using ApacheTech.Common.Extensions.Harmony;
using Gantry.Services.Network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Gantry.Services.HarmonyPatches.Annotations;
using HarmonyLib;
using System;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
using Gantry.Services.FileSystem.Configuration;
using JetBrains.Annotations;
using Vintagestory.API.MathTools;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.Knapster.Features.EasyClayForming
{
    [HarmonySidedPatch(EnumAppSide.Universal)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EasyClayForming : UniversalModSystem, IServerServiceRegistrar
    {
        private static EasyClayFormingSettings _serverSettings = new();
        private static EasyClayFormingPacket _clientSettings;
        private static bool Enabled => ApiEx.OneOf(_clientSettings.Enabled, _serverSettings.Enabled);
        private static int VoxelsPerClick => ApiEx.OneOf(_clientSettings.VoxelsPerClick, _serverSettings.VoxelsPerClick);
        private IServerNetworkChannel _serverChannel;

        public void ConfigureServerModServices(IServiceCollection services)
        {
            services.AddFeatureWorldSettings<EasyClayFormingSettings>();
        }
        
        public override void StartServerSide(ICoreServerAPI api)
        {
            _serverSettings = ModSettings.World.Feature<EasyClayFormingSettings>();
            FluentChat.ServerCommand("knapster")
                .HasSubCommand("clayforming").WithHandler(OnClayFormingCommand)
                .HasSubCommand("c").WithHandler(OnClayFormingCommand);

            _serverChannel = IOC.Services.Resolve<IServerNetworkService>()
                .ServerChannel("EasyClayForming")
                .RegisterMessageType<EasyClayFormingPacket>();

            api.Event.PlayerJoin += player =>
            {
                _serverChannel.SendPacket(EasyClayFormingPacket.FromSettings(_serverSettings), player);
            };
        }
        
        private void OnClayFormingCommand(string subCommandName, IServerPlayer player, int groupId, CmdArgs args)
        {
            var sb = new StringBuilder();
            switch (args.PopWord())
            {
                case "1":
                case "true":
                case "enable":
                case "enabled":
                    _serverSettings.Enabled = true;
                    _serverChannel.BroadcastPacket(EasyClayFormingPacket.FromSettings(_serverSettings));
                    break;

                case "0":
                case "false":
                case "disable":
                case "disabled":
                    _serverSettings.Enabled = false;
                    _serverChannel.BroadcastPacket(EasyClayFormingPacket.FromSettings(_serverSettings));
                    break;

                case "v":
                case "voxel":
                case "voxels":
                    _serverSettings.VoxelsPerClick = GameMath.Clamp(args.PopInt().GetValueOrDefault(1), 1, 8);
                    _serverChannel.BroadcastPacket(EasyClayFormingPacket.FromSettings(_serverSettings));
                    break;
            }
            sb.AppendLine(LangEx.FeatureString("EasyClayForming", "Enabled", LangEx.BooleanString(_serverSettings.Enabled)));
            sb.Append(LangEx.FeatureString("EasyClayForming", "Voxels", _serverSettings.VoxelsPerClick));
            Sapi.SendMessage(player, groupId, sb.ToString(), EnumChatType.Notification);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            IOC.Services.Resolve<IClientNetworkService>()
                .ClientChannel("EasyClayForming")
                .RegisterMessageType<EasyClayFormingPacket>()
                .SetMessageHandler<EasyClayFormingPacket>(packet =>
                {
                    _clientSettings = packet;
                });
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemClay), nameof(ItemClay.GetToolModes))]
        public static void ClientPatch_ItemClay_GetToolModes_Postfix(ItemClay __instance, ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel, ref SkillItem[] __result, ref SkillItem[] ___toolModes)
        {
            if (__result is null || !Enabled)
            {
                if (__instance.GetToolMode(slot, forPlayer, blockSel) > 0)
                    __instance.SetToolMode(slot, forPlayer, blockSel, 0);
                ___toolModes = ___toolModes.Take(4).ToArray();
                __result = ___toolModes.Take(4).ToArray();
                return;
            }
            if (ApiEx.Side.IsServer()) return;

            if (!ModEx.IsCurrentlyOnMainThread()) return;
            if (___toolModes.Length > 4) return;
            var skillItem = new SkillItem
            {
                Code = new AssetLocation("auto"),
                Name = Lang.Get("Auto Complete", Array.Empty<object>())
            }.WithIcon(ApiEx.Client, ApiEx.Client.Gui.Icons.Drawfloodfill_svg);
            ___toolModes = ___toolModes.AddToArray(skillItem);
            __result = __result.AddToArray(skillItem);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlockEntityClayForm), nameof(BlockEntityClayForm.OnUseOver), typeof(IPlayer), typeof(Vec3i), typeof(BlockFacing), typeof(bool))]
        public static bool UniversalPatch_BlockEntityClayForm_OnUseOver_Prefix(BlockEntityClayForm __instance,
            IPlayer byPlayer, bool mouseBreakMode, Vec3i voxelPos, BlockFacing facing, ref ItemStack ___workItemStack)
        {
            var slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Itemstack is null || !__instance.CanWorkCurrent) return true;
            if (slot.Itemstack.Collectible is not ItemClay clay) return true;
            var blockSel = new BlockSelection { Position = __instance.Pos };
            var toolMode = clay.GetToolMode(slot, byPlayer, blockSel);


            if (!Enabled)
            {
                if (toolMode > 3) clay.SetToolMode(slot, byPlayer, blockSel, 0);
                return true;
            }

            if (toolMode < 4) return true;
            if (mouseBreakMode) return false;

            if (__instance.Api.Side.IsClient())
            {
                __instance.SendUseOverPacket(byPlayer, voxelPos, facing, false);
            }

            clay.SetToolMode(slot, byPlayer, blockSel, 0);

            var currentLayer = __instance.CurrentLayer();
            if (__instance.AutoCompleteLayer(currentLayer, VoxelsPerClick))
            {
                __instance.Api.World.PlaySoundAt(new AssetLocation("sounds/player/clayform.ogg"), byPlayer, byPlayer, true, 8f);
            }

            __instance.Api.World.FrameProfiler.Mark("clayform-modified");
            currentLayer = __instance.CurrentLayer();
            __instance.CallMethod("RegenMeshAndSelectionBoxes", currentLayer);
            __instance.Api.World.FrameProfiler.Mark("clayform-regenmesh");
            __instance.Api.World.BlockAccessor.MarkBlockDirty(__instance.Pos);
            __instance.Api.World.BlockAccessor.MarkBlockEntityDirty(__instance.Pos);
            if (!__instance.CallMethod<bool>("HasAnyVoxel"))
            {
                __instance.AvailableVoxels = 0;
                ___workItemStack = null;
                __instance.Api.World.BlockAccessor.SetBlock(0, __instance.Pos);
                return false;
            }
            __instance.CheckIfFinished(byPlayer, currentLayer);
            __instance.Api.World.FrameProfiler.Mark("clayform-checkfinished");
            __instance.MarkDirty();

            clay.SetToolMode(slot, byPlayer, blockSel, 4);
            return false;
        }
    }
}