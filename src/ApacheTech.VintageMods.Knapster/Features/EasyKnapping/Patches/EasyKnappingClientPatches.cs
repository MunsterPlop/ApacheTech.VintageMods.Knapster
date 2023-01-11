using ApacheTech.VintageMods.Knapster.Features.EasyKnapping.Systems;

// ReSharper disable InconsistentNaming

namespace ApacheTech.VintageMods.Knapster.Features.EasyKnapping.Patches
{
    [HarmonySidedPatch(EnumAppSide.Client)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class EasyKnappingClientPatches : ClientModSystem
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlockEntityKnappingSurface), nameof(BlockEntityClayForm.OnUseOver), typeof(IPlayer), typeof(Vec3i), typeof(BlockFacing), typeof(bool))]
        public static bool ClientPatch_BlockEntityKnappingSurface_OnUseOver_Prefix(
            BlockEntityKnappingSurface __instance, ref Vec3i voxelPos)
        {
            if (!EasyKnappingClient.Settings.Enabled) return true;
            voxelPos = FindNextVoxelToRemove(__instance);
            return true;
        }

        private static Vec3i FindNextVoxelToRemove(BlockEntityKnappingSurface blockEntity)
        {
            for (var x = 0; x < 16; x++)
            {
                for (var z = 0; z < 16; z++)
                {
                    if (blockEntity.Voxels[x, z] != blockEntity.SelectedRecipe.Voxels[x, 0, z])
                    {
                        return new Vec3i(x, 0, z);
                    }
                }
            }
            return Vec3i.Zero;
        }
    }
}