using ApacheTech.Common.DependencyInjection.Abstractions;
using ApacheTech.Common.DependencyInjection.Abstractions.Extensions;
using ApacheTech.VintageMods.FluentChatCommands;
using Gantry.Core;
using Gantry.Core.DependencyInjection;
using Gantry.Core.DependencyInjection.Registration;
using Gantry.Core.ModSystems;
using Gantry.Services.FileSystem.DependencyInjection;
using Gantry.Services.Network;
using HarmonyLib;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Gantry.Services.HarmonyPatches.Annotations;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.Knapster.Features.EasyKnapping
{
    [HarmonySidedPatch(EnumAppSide.Universal)]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EasyKnapping : UniversalModSystem, IServerServiceRegistrar
    {
        private static EasyKnappingSettings _serverSettings = new();
        private static EasyKnappingPacket _clientSettings;
        private static bool Enabled => ApiEx.OneOf(_clientSettings.Enabled, _serverSettings.Enabled);
        private IServerNetworkChannel _serverChannel;

        public void ConfigureServerModServices(IServiceCollection services)
        {
            services.AddFeatureWorldSettings<EasyKnappingSettings>();
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            _serverSettings = IOC.Services.Resolve<EasyKnappingSettings>();
            FluentChat.ServerCommand("knapster")
                .HasSubCommand("knapping").WithHandler(OnKnappingCommand)
                .HasSubCommand("k").WithHandler(OnKnappingCommand);

            _serverChannel = IOC.Services.Resolve<IServerNetworkService>()
                .DefaultServerChannel
                .RegisterMessageType<EasyKnappingPacket>();

            api.Event.PlayerJoin += player =>
            {
                _serverChannel.SendPacket(EasyKnappingPacket.FromSettings(_serverSettings), player);
            };
        }

        private void OnKnappingCommand(string subCommandName, IServerPlayer player, int groupId, CmdArgs args)
        {
            var sb = new StringBuilder();
            switch (args.PopWord())
            {
                case "1":
                case "true":
                case "enable":
                case "enabled":
                    _serverSettings.Enabled = true;
                    _serverChannel.BroadcastPacket(EasyKnappingPacket.FromSettings(_serverSettings));
                    break;

                case "0":
                case "false":
                case "disable":
                case "disabled":
                    _serverSettings.Enabled = false;
                    _serverChannel.BroadcastPacket(EasyKnappingPacket.FromSettings(_serverSettings));
                    break;
            }
            sb.Append(LangEx.FeatureString("EasyKnapping", "Enabled", LangEx.BooleanString(_serverSettings.Enabled)));
            if (sb.Length > 0) Sapi.SendMessage(player, groupId, sb.ToString(), EnumChatType.Notification);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            IOC.Services.Resolve<IClientNetworkService>()
                .DefaultClientChannel
                .RegisterMessageType<EasyKnappingPacket>()
                .SetMessageHandler<EasyKnappingPacket>(packet =>
                {
                    _clientSettings = packet;
                });
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlockEntityKnappingSurface), nameof(BlockEntityClayForm.OnUseOver), typeof(IPlayer), typeof(Vec3i), typeof(BlockFacing), typeof(bool))]
        public static bool UniversalPatch_BlockEntityKnappingSurface_OnUseOver_Prefix(
            BlockEntityKnappingSurface __instance, ref Vec3i voxelPos)
        {
            if (!Enabled) return true;
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