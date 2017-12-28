using System;
using StardewModdingAPI;
using xTile.ObjectModel;
using StardewValley;

namespace TotalBathhouseOverhaul
{
    /// <summary>
    /// Similar to SDV's Message action but searches for the string in the translation table, letting you add custom messages.
    /// This is used for the custom inspectables.
    /// </summary>
    internal class MessageAction : TileAction
    {
        protected override string CommandName => "Message";
        protected override int MinimumArgs => 1;

        private string MessageKey => this.Args[0];

        // Format is: Message "MessageKey"
        protected override string ProcessArgument(string argument, int index)
        {
            return argument.Trim('"');
        }

        protected override void Execute(IModHelper helper, IMonitor monitor)
        {
            if (this.MessageKey != null)
            {
                // TODO: Debug-only verify the key doesn't exist in the vanilla game.
                Translation translation = helper.Translation.Get(this.MessageKey);
                if (translation.HasValue())
                {
                    Game1.drawObjectDialogue(translation);
                }
            }
        }
    }
}
