using ApacheTech.VintageMods.Knapster.Abstractions;

namespace ApacheTech.VintageMods.Knapster.Features.EasyKnapping.Systems
{
    [UsedImplicitly]
    public sealed class EasyKnappingServer : FeatureServerSystemBase<EasyKnappingSettings, EasyKnappingPacket>
    {
        protected override string SubCommandName => "Knapping";

        protected override EasyKnappingPacket GeneratePacketPerPlayer(IPlayer player, bool enabledForPlayer)
        {
            return new EasyKnappingPacket { Enabled = enabledForPlayer };
        }
    }
}
