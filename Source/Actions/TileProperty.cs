using StardewModdingAPI;
using xTile.ObjectModel;

namespace TotalBathhouseOverhaul
{
    /// <summary>Base class for any tile property.</summary>
    internal abstract class TileProperty : ITileProperty
    {
        public abstract TilePropertyType PropertyType { get; }

        /// <summary>The name of the command. This is the first token of the property value (ie. "Message").</summary>
        protected abstract string CommandName { get; }

        /// <summary>The minimum number of arguments the property must have for it to be valid.</summary>
        protected abstract int MinimumArgs { get; }

        /// <summary>Additional arguments that come after the ActionName, delimited by a space.</summary>
        protected string[] Args = new string[0];

        public void RunOnProperty(PropertyValue property, IModHelper helper, IMonitor monitor)
        {
            if (Parse(property))
            {
                Execute(helper, monitor);
            }
        }

        /// <summary>Parses the data set on the tile and verifies if this property should be run.</summary>
        /// <param name="property">The property to parse.</param>
        /// <returns>True if this property should be executed.</returns>
        protected bool Parse(PropertyValue property)
        {
            string propertyString = property;
            string[] tokens = propertyString.Split(' ');
            int argCount = tokens.Length - 1;
            if (tokens.Length == 0 || argCount < this.MinimumArgs)
            {
                return false;
            }

            string actionName = tokens[0];
            if (actionName != this.CommandName)
            {
                return false;
            }

            Args = new string[argCount];
            for (int i = 0; i < argCount; ++i)
            {
                Args[i] = ProcessArgument(tokens[i + 1], i);
            }
            return true;
        }

        /// <summary>Runs the logic for this property.</summary>
        protected abstract void Execute(IModHelper helper, IMonitor monitor);

        /// <summary>Allow derived classes to manipulate each argument.</summary>
        protected virtual string ProcessArgument(string argument, int index)
        {
            return argument;
        }
    }
}
