namespace ApacheTech.VintageMods.Knapster.Extensions
{
    /// <summary>
    ///     Extension methods to aid building sub-commands for EasyX features.
    /// </summary>
    public static class FluentChatExtensions
    {
        /// <summary>
        ///     Adds feature specific sub-commands to the feature command. 
        /// </summary>
        /// <param name="subCommand">The sub-command to add features to.</param>
        /// <param name="builder">The extra details to add to the command.</param>
        /// <returns></returns>
        public static IFluentChatSubCommandBuilder<IFluentChatCommand> WithFeatureSpecifics(
            this IFluentChatSubCommandBuilder<IFluentChatCommand> subCommand, Action<IFluentChatSubCommandBuilder<IFluentChatCommand>> builder)
        {
            builder(subCommand);
            return subCommand;
        }
    }
}