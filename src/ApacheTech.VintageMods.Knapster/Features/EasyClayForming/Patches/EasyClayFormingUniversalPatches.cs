using ApacheTech.Common.Extensions.Harmony;
using ApacheTech.VintageMods.Knapster.Features.EasyClayForming.Extensions;
using ApacheTech.VintageMods.Knapster.Features.EasyClayForming.Systems;
using Gantry.Core;
using Gantry.Services.HarmonyPatches.Annotations;
using HarmonyLib;
using JetBrains.Annotations;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming

namespace ApacheTech.VintageMods.Knapster.Features.EasyClayForming.Patches
{
    [HarmonySidedPatch(EnumAppSide.Client)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EasyClayFormingClientPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemClay), nameof(ItemClay.GetToolModes))]
        public static void ClientPatch_ItemClay_GetToolModes_Postfix(ItemClay __instance, ItemSlot slot,
            IClientPlayer forPlayer, BlockSelection blockSel, ref SkillItem[] __result, ref SkillItem[] ___toolModes)
        {
            if (__result is null) return;
            if (!EasyClayFormingClient.Settings.Enabled)
            {
                __result = ___toolModes = ___toolModes.Take(4).ToArray();
                if (__instance.GetToolMode(slot, forPlayer, blockSel) < 4) return;
                __instance.SetToolMode(slot, forPlayer, blockSel, 0);
                return;
            }

            if (!ModEx.IsCurrentlyOnMainThread()) return;
            if (___toolModes.Length > 4) return;
            var skillItem = new SkillItem
            {
                Code = new AssetLocation("auto"),
                Name = LangEx.FeatureString("Knapster", "AutoComplete")
            }.WithIcon(ApiEx.Client, ApiEx.Client.Gui.Icons.Drawfloodfill_svg);
            __result = ___toolModes = ___toolModes.AddToArray(skillItem);
        }
    }

    [HarmonySidedPatch(EnumAppSide.Universal)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EasyClayFormingUniversalPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlockEntityClayForm), nameof(BlockEntityClayForm.OnUseOver), typeof(IPlayer), typeof(Vec3i), typeof(BlockFacing), typeof(bool))]
        public static bool UniversalPatch_BlockEntityClayForm_OnUseOver_Prefix(BlockEntityClayForm __instance,
            IPlayer byPlayer, bool mouseBreakMode, Vec3i voxelPos, BlockFacing facing, ref ItemStack ___workItemStack)
        {
            var voxelsPerClick = ApiEx.Return(
                _ => EasyClayFormingClient.Settings.VoxelsPerClick,
                _ => EasyClayFormingServer.Settings.VoxelsPerClick);

            var enabled = ApiEx.Return(
                _ => EasyClayFormingClient.Settings.Enabled,
                _ => EasyClayFormingServer.IsEnabledFor(byPlayer));

            var slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Itemstack is null || !__instance.CanWorkCurrent) return true;
            if (slot.Itemstack.Collectible is not ItemClay clay) return true;
            var blockSel = new BlockSelection { Position = __instance.Pos };
            var toolMode = clay.GetToolMode(slot, byPlayer, blockSel);

            if (!enabled)
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
            if (__instance.AutoCompleteLayer(currentLayer, voxelsPerClick))
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