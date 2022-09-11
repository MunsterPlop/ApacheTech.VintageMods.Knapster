using System;
using System.Threading.Tasks;
using ApacheTech.Common.Extensions.Harmony;
using Gantry.Core;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace ApacheTech.VintageMods.Knapster.Features.EasyClayForming
{
    public static class BlockEntityClayFormExtensions
    {
        public static void AutoCompleteByVoxelPos(this BlockEntityClayForm block)
        {
            Task.Factory.StartNew(() =>
            {
                for (var i = 0; i < 16; i++)
                {
                    for (var j = 0; j < 16; j++)
                    {
                        for (var k = 0; k < 16; k++)
                        {
                            var x = j;
                            var y = i;
                            var z = k;

                            var expected = block.SelectedRecipe.Voxels[x, y, z];
                            var actual = block.Voxels[x, y, z];
                            if (expected == actual) continue;
                            block.Voxels[x, y, z] = expected;
                        }
                    }
                }
            });
        }

        public static int CurrentLayer(this BlockEntityClayForm block, int layerStart = 0)
        {
            if (block.SelectedRecipe is null) return 0;
            for (var y = layerStart; y < 16; y++)
            {
                for (var x = 0; x < 16; x++)
                {
                    for (var z = 0; z < 16; z++)
                    {
                        if (block.Voxels[x, y, z] != block.SelectedRecipe.Voxels[x, y, z])
                        {
                            return y;
                        }
                    }
                }
            }
            return 16;
        }

        public static bool AutoCompleteLayer(this BlockEntityClayForm block, int y, int voxels)
        {
            if (y >= 16) return false;
            var result = false;
            var num = Math.Max(1, voxels);
            for (var x = 0; x < 16; x++)
            {   
                for (var z = 0; z < 16; z++)
                {
                    var expected = block.SelectedRecipe?.Voxels[x, y, z] ?? false;
                    var actual = block.Voxels[x, y, z];
                    if (expected == actual) continue;
                    result = true;
                    block.Voxels[x, y, z] = expected;
                    block.AvailableVoxels--;
                    if (--num == 0) return result;
                }
            }
            return result;
        }

        public static void AutoCompleteBySelectionBoxes(this BlockEntityClayForm block)
        {
            var selectionBoxes = block.GetField<Cuboidf[]>("selectionBoxes");

            for (var i = 0; i < block.SelectedRecipe.QuantityLayers - 1; i++)
            {
                foreach (var selectionBox in selectionBoxes)
                {
                    var voxelPos = selectionBox.GetVoxelPos();
                    var x = voxelPos.X;
                    var y = voxelPos.Y;
                    var z = voxelPos.Z;

                    var expected = block.SelectedRecipe.Voxels[x, y, z];
                    var actual = block.Voxels[x, y, z];
                    if (expected == actual) continue;
                    block.Voxels[x, y, z] = expected;
                }
            }
        }

        public static void SimulateCorrectClick(this BlockEntityClayForm block, int selectionBoxIndex, bool mouseBreakMode)
        {
            var player = ApiEx.Client.World.Player;

            var blockSel = player.CurrentBlockSelection;
            if (blockSel is null) return;
            blockSel.SelectionBoxIndex = selectionBoxIndex;
            var slot = player.InventoryManager.ActiveHotbarSlot;

            if (mouseBreakMode)
            {
                slot.Itemstack?.Item?.OnHeldAttackStop(0, slot, player.Entity, blockSel, player.CurrentEntitySelection);
                return;
            }
            slot.Itemstack?.Item?.OnHeldInteractStop(0, slot, player.Entity, blockSel, player.CurrentEntitySelection);
        }

        public static Vec3i GetVoxelPos(this Cuboidf cuboid)
        {
            return new Vec3i((int)(16f * cuboid.X1), (int)(16f * cuboid.Y1), (int)(16f * cuboid.Z1));
        }
    }
}