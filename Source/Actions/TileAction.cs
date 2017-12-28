using System;
using StardewModdingAPI;
using xTile.ObjectModel;

namespace TotalBathhouseOverhaul
{
    /// <summary>Base class for any Action properties.</summary>
    internal abstract class TileAction : ITileProperty
    {
        public TilePropertyType PropertyType => TilePropertyType.Action;

        /// <summary>The name of the derived action. This is the first token of the property value (ie. "Message").</summary>
        protected abstract string ActionName { get; }

        /// <summary>Additional arguments that come after the ActionName, delimited by a space.</summary>
        protected string[] Args = new string[0];

        // Action format is: ActionName Args separated by spaces
        public virtual bool Parse(PropertyValue property)
        {
            string propertyString = (string)property;
            string[] tokens = propertyString.Split(' ');
            if (tokens.Length == 0)
            {
                return false;
            }

            string actionName = tokens[0];
            if (actionName != this.ActionName)
            {
                return false;
            }

            Args = new string[tokens.Length - 1];
            Array.Copy(tokens, 1, Args, 0, Args.Length);
            return true;
        }

        public abstract void Execute(IModHelper helper, IMonitor monitor);
    }
}
