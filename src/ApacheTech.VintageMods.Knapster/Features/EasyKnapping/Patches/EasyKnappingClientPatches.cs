﻿using ApacheTech.VintageMods.Knapster.Features.EasyKnapping.Systems;

// ReSharper disable InconsistentNaming

namespace ApacheTech.VintageMods.Knapster.Features.EasyKnapping.Patches
{
    [HarmonySidedPatch(EnumAppSide.Client)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class EasyKnappingClientPatches : ClientModSystem
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlockEntityKnappingSurface), "OnUseOver", typeof(IPlayer), typeof(int), typeof(BlockFacing), typeof(bool))]
        public static bool ClientPatch_BlockEntityKnappingSurface_OnUseOver_Prefix(
            BlockEntityKnappingSurface __instance, IPlayer byPlayer, BlockFacing facing, bool mouseMode)
        {
            if (!EasyKnappingClient.Settings.Enabled) return true;
            if (byPlayer.Entity.Controls.CtrlKey) return true;
            for (var i = 0; i < EasyKnappingClient.Settings.VoxelsPerClick; i++)
            {
                if (!__instance.CallMethod<bool>("HasAnyVoxel")) return true;
                var voxelPos = FindNextVoxelToRemove(__instance);

                var method = AccessTools.Method(typeof(BlockEntityKnappingSurface), "OnUseOver",
                    new[] { typeof(IPlayer), typeof(Vec3i), typeof(BlockFacing), typeof(bool) });

                method.Invoke(__instance, new object[] { byPlayer, voxelPos, facing, mouseMode });
            }
            return false;
        }

        private static Vec3i FindNextVoxelToRemove(BlockEntityKnappingSurface blockEntity)
        {
            for (var x = 0; x < 16; x++)
            {
                for (var z = 0; z < 16; z++)
                {
                    if (!IsOutlineVoxel(blockEntity, x, z)) continue;
                    if (VoxelNeedsRemoving(blockEntity, x, z))
                    {
                        return new Vec3i(x, 0, z);
                    }
                }
            }
            return Vec3i.Zero;
        }

        private static bool IsOutlineVoxel(BlockEntityKnappingSurface blockEntity, int x, int z)
        {
            for (var i = -1; i <= 1; i++)
            {
                for (var j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    var xi = x + i;
                    if (xi is < 0 or >= 16) continue;

                    var zj = z + j;
                    if (zj is < 0 or >= 16) continue;

                    if (blockEntity.SelectedRecipe.Voxels[xi, 0, zj]) return true;
                }
            }
            return false;
        }

        private static bool VoxelNeedsRemoving(BlockEntityKnappingSurface blockEntity, int x, int z)
        {
            return blockEntity.Voxels[x, z] != blockEntity.SelectedRecipe.Voxels[x, 0, z];
        }
    }
}