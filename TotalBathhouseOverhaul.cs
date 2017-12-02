using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;

namespace TotalBathhouseOverhaul
{
    public class TotalBathhouseOverhaul : Mod
    {
        //note locker locations female and male, respectively:
        //(7, 16) and (47, 16).

        public static TotalBathhouseOverhaul instance;

        public IModHelper helper;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            instance.helper = helper;

            helper.Content.AssetEditors.Add(new ScheduleLibrary());

            //try to initialize the character schedules library.
            ScheduleLibrary.Initialize(instance.helper);
        }
    }
}
