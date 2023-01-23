using ApacheTech.VintageMods.Knapster.Abstractions;
using ApacheTech.VintageMods.Knapster.Extensions;

namespace ApacheTech.VintageMods.Knapster.Features.EasyHarvesting.Systems
{
    [UsedImplicitly]
    public sealed class EasyHarvestingServer : FeatureServerSystemBase<EasyHarvestingSettings, EasyHarvestingPacket>
    {
        protected override string SubCommandName => "Harvesting";

        protected override void FeatureSpecificCommands(IFluentChatSubCommandBuilder<IFluentChatCommand> subCommand)
        {
            subCommand
                .HasSubCommand("speed", s => s
                    .WithAlias("s")
                    .WithHandler(OnChangeSpeedMultiplier)
                    .Build());
        }

        protected override EasyHarvestingPacket GeneratePacketPerPlayer(IPlayer player, bool enabledForPlayer)
        {
            return EasyHarvestingPacket.Create(enabledForPlayer, Settings.SpeedMultiplier);
        }

        protected override void DisplayInfo(IPlayer player, int groupId, CmdArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine(LangEx.FeatureString("Knapster", "Mode", SubCommandName, Settings.Mode));
            sb.Append(LangEx.FeatureString("Knapster", "SpeedMultiplier", SubCommandName, Settings.SpeedMultiplier));
            Sapi.SendMessage(player, groupId, sb.ToString(), EnumChatType.Notification);
        }

        private void OnChangeSpeedMultiplier(IPlayer player, int groupId, CmdArgs args)
        {
            Settings.SpeedMultiplier = GameMath.Clamp(args.PopFloat().GetValueOrDefault(1f), 0f, 2f);
            var message = LangEx.FeatureString("Knapster", "SpeedMultiplier", SubCommandName, Settings.SpeedMultiplier);
            Sapi.SendMessage(player, groupId, message, EnumChatType.Notification);
            ServerChannel?.BroadcastUniquePacket(GeneratePacket);
        }
    }
}
