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
        protected override string ActionName => "Message";

        private string MessageKey;

        // Format is: Message "MessageKey"
        public override bool Parse(PropertyValue property)
        {
            if (!base.Parse(property))
                return false;

            if (Args.Length > 0)
            {
                this.MessageKey = Args[0].Trim('"');
                return true;
            }
            return false;
        }

        public override void Execute(IModHelper helper, IMonitor monitor)
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
