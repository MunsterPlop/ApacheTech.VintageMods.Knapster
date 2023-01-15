using ApacheTech.VintageMods.Knapster.Abstractions;
using ApacheTech.VintageMods.Knapster.Extensions;

namespace ApacheTech.VintageMods.Knapster.Features.EasyClayForming.Systems
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EasyClayFormingServer : FeatureServerSystemBase<EasyClayFormingSettings, EasyClayFormingPacket>
    {
        protected override string SubCommandName => "ClayForming";

        protected override void FeatureSpecificCommands(IFluentChatSubCommandBuilder<IFluentChatCommand> subCommand)
        {
            subCommand
                .HasSubCommand("voxels", v => v
                    .WithAlias("v")
                    .WithHandler(OnChangeVoxelsPerClick)
                    .Build());
        }

        protected sealed override void DisplayInfo(IPlayer player, int groupId, CmdArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine(LangEx.FeatureString("Knapster", "Mode", SubCommandName, Settings.Mode));
            sb.Append(LangEx.FeatureString("Knapster", "VoxelsPerClick", SubCommandName, Settings.VoxelsPerClick));
            Sapi.SendMessage(player, groupId, sb.ToString(), EnumChatType.Notification);
        }

        protected override EasyClayFormingPacket GeneratePacketPerPlayer(IPlayer player, bool enabledForPlayer)
        {
            return EasyClayFormingPacket.Create(enabledForPlayer, Settings.VoxelsPerClick);
        }

        private void OnChangeVoxelsPerClick(IPlayer player, int groupId, CmdArgs args)
        {
            Settings.VoxelsPerClick = GameMath.Clamp(args.PopInt().GetValueOrDefault(1), 1, 8);
            var message = LangEx.FeatureString("Knapster", "VoxelsPerClick", SubCommandName, Settings.VoxelsPerClick);
            Sapi.SendMessage(player, groupId, message, EnumChatType.Notification);
            ServerChannel?.BroadcastUniquePacket(GeneratePacket);
        }
    }
}