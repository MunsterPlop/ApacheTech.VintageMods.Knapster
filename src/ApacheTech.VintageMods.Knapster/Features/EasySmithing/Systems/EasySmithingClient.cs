using ApacheTech.Common.DependencyInjection.Abstractions.Extensions;
using Gantry.Core.DependencyInjection;
using Gantry.Core.ModSystems;
using Gantry.Services.Network;
using JetBrains.Annotations;
using Vintagestory.API.Client;

namespace ApacheTech.VintageMods.Knapster.Features.EasySmithing.Systems
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EasySmithingClient : ClientModSystem
    {
        internal static EasySmithingPacket Settings = new()
        {
            Enabled = false,
            CostPerClick = 5
        };

        public override void StartClientSide(ICoreClientAPI api)
        {
            IOC.Services.Resolve<IClientNetworkService>()
                .DefaultClientChannel
                .RegisterMessageType<EasySmithingPacket>()
                .SetMessageHandler<EasySmithingPacket>(SyncSettingsWithServer);
        }

        private static void SyncSettingsWithServer(EasySmithingPacket packet)
        {
            Settings = packet;
        }
    }
}