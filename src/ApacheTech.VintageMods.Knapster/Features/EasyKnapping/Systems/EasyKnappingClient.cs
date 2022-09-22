using ApacheTech.Common.DependencyInjection.Abstractions.Extensions;
using Gantry.Core.DependencyInjection;
using Gantry.Core.ModSystems;
using Gantry.Services.Network;
using Vintagestory.API.Client;

namespace ApacheTech.VintageMods.Knapster.Features.EasyKnapping.Systems
{
    public sealed class EasyKnappingClient : ClientModSystem
    {
        internal static EasyKnappingPacket Settings = new() { Enabled = false };

        public override void StartClientSide(ICoreClientAPI api)
        {
            IOC.Services.Resolve<IClientNetworkService>()
                .DefaultClientChannel
                .RegisterMessageType<EasyKnappingPacket>()
                .SetMessageHandler<EasyKnappingPacket>(SyncSettingsWithServer);
        }

        private static void SyncSettingsWithServer(EasyKnappingPacket packet)
        {
            Settings = packet;
        }
    }
}