using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace TotalBathhouseOverhaul
{
    internal interface IInputContext
    {
        ICursorPosition CursorPosition { get; set; }

        Point GetGrabTIlePoint();
    }
}
