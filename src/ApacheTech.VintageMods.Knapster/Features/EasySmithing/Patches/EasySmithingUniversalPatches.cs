﻿using ApacheTech.VintageMods.Knapster.Features.EasySmithing.Systems;

// ReSharper disable InconsistentNaming

namespace ApacheTech.VintageMods.Knapster.Features.EasySmithing.Patches
{
    [HarmonySidedPatch(EnumAppSide.Universal)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EasySmithingUniversalPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlockEntityAnvil), "OnUseOver", typeof(IPlayer), typeof(Vec3i), typeof(BlockSelection))]
        public static bool UniversalPatch_BlockEntityAnvil_OnUseOver_Prefix(BlockEntityAnvil __instance,
            IPlayer byPlayer, Vec3i voxelPos, BlockSelection blockSel)
        {
            if (__instance.SelectedRecipe?.Voxels is null) return true;

            var costPerClick = ApiEx.Return(
                _ => EasySmithingClient.Settings.CostPerClick,
                _ => EasySmithingServer.Settings.CostPerClick);

            var enabled = ApiEx.Return(
                _ => EasySmithingClient.Settings.Enabled, 
                _ => EasySmithingServer.IsEnabledFor(byPlayer));

            var slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Itemstack is null || !__instance.CanWorkCurrent) return true;
            if (slot.Itemstack.Collectible is not ItemHammer hammer) return true;
            var toolMode = hammer.GetToolMode(slot, byPlayer, blockSel);

            if (!enabled)
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
            slot.Itemstack.Collectible.DamageItem(__instance.Api.World, byPlayer.Entity, slot, costPerClick);
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