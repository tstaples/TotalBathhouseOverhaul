using StardewModdingAPI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Linq;

namespace TotalBathhouseOverhaul
{
    public class ScheduleLibrary : IAssetEditor
    {
        private IModHelper Helper;

        public Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>> CharacterSchedules { get; set; }

        // Factory function.
        public static ScheduleLibrary Create(IModHelper helper)
        {
            ScheduleLibrary instance = helper.ReadJsonFile<ScheduleLibrary>("CharacterScheduleLibrary.json");
            if (instance == null)
            {
                instance = new ScheduleLibrary();
                helper.WriteJsonFile("CharacterScheduleLibrary.json", instance);
            }

            instance.Helper = helper;
            return instance;
        }

        private ScheduleLibrary()
        {
            this.CharacterSchedules = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>();
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetName.Contains(@"Characters\schedules\");
        }

        public void Edit<T>(IAssetData asset)
        {
            string scheduleCharacter = asset.AssetName.Replace(@"Characters\schedules\", string.Empty);

            Dictionary<string, string> assetData = (Dictionary<string, string>)asset.AsDictionary<string, string>().Data;

            if (!this.CharacterSchedules.ContainsKey(scheduleCharacter))
            {
                Initialize(scheduleCharacter, assetData);
            }

            List<string> keysToPurge = assetData.Keys.ToList();

            //purge all vanilla schedules, we're going to load custom ones which may or may not be changes/overrides to the vanilla schedule..
            foreach(string keyToPurge in keysToPurge)
            {
                assetData.Remove(keyToPurge);
            }

            Dictionary<string, string> compressedScheduleData = Compress(scheduleCharacter);
            foreach (KeyValuePair<string, string> compressedScheduleEntry in compressedScheduleData)
            {
                asset.AsDictionary<string, string>().Data[compressedScheduleEntry.Key] = compressedScheduleEntry.Value;
            }
        }

        public Dictionary<string, string> Compress(string characterName)
        {   
            Dictionary<string, string> compressedDictionary = new Dictionary<string, string>();

            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, string>>> characterScheduleRow in this.CharacterSchedules[characterName])
            {
                //the string value for this schedule item
                string compressedSchedule = string.Empty;
                
                //the keyname of this schedule item, this is the weird unique one that can be things like [season], [day], [hearts], etc.
                string scheduleKey = characterScheduleRow.Key;

                //this is the values contained for that particular schedule key table.
                Dictionary<string, Dictionary<string, string>> characterScheduleTimeRows = characterScheduleRow.Value;
                foreach (KeyValuePair<string, Dictionary<string, string>> characterScheduleTimeRow in characterScheduleTimeRows)
                {
                    string timeSlot = string.Empty;

                    //the time key for this schedule
                    string timeKey = characterScheduleTimeRow.Key;

                    //handle the guts of the schedule, depending on what sort of time key we're dealing with.
                    if (timeKey.Equals("GOTO"))
                    {
                        string gotoLabel = string.Empty;
                        foreach (KeyValuePair<string, string> gotoKeyValuePair in characterScheduleTimeRow.Value)
                        {
                            if (gotoKeyValuePair.Key.Equals("gotoLabel"))
                            {
                                gotoLabel = gotoKeyValuePair.Value;
                                break;
                            }
                        }
                        timeSlot = $"{timeKey} {gotoLabel}";
                    } else if (timeKey.Equals("NOT friendship"))
                    {
                        string notFriendshipWithWhoAndHowMuch = string.Empty;
                        foreach (KeyValuePair<string, string> notFriendshipWithKeyValuePair in characterScheduleTimeRow.Value)
                        {
                            notFriendshipWithWhoAndHowMuch = $"{notFriendshipWithKeyValuePair.Key} {notFriendshipWithKeyValuePair.Value}";
                            break;                         
                        }
                        timeSlot = $"{timeKey} {notFriendshipWithWhoAndHowMuch}";
                    } else
                    {
                        //holds the full diretive for this time slot.
                        string concatenatedDirective = string.Empty;
                        
                        //if it's not a GOTO directive or a 'NOT friendship' directive, it's a time directive (the good stuff)
                        foreach (KeyValuePair<string, string> timeDirective in characterScheduleTimeRow.Value)
                        {
                            switch (timeDirective.Key)
                            {
                                case "facingDirection":
                                    switch (timeDirective.Value)
                                    {
                                        case "up":
                                            concatenatedDirective = $"{concatenatedDirective} 0";
                                            break;
                                        case "right":
                                            concatenatedDirective = $"{concatenatedDirective} 1";
                                            break;
                                        case "down":
                                            concatenatedDirective = $"{concatenatedDirective} 2";
                                            break;
                                        case "left":
                                            concatenatedDirective = $"{concatenatedDirective} 3";
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                default:
                                    if (concatenatedDirective == string.Empty)
                                    {
                                        concatenatedDirective = timeDirective.Value;
                                    } else
                                    {
                                        concatenatedDirective = $"{concatenatedDirective} {timeDirective.Value}";
                                    }                                    
                                    break;
                            }
                        }

                        timeSlot = $"{timeKey} {concatenatedDirective}";
                    }

                    if (compressedSchedule.Equals(string.Empty))
                    {
                        compressedSchedule = timeSlot;
                    } else
                    {
                        compressedSchedule = string.Join(@"/", compressedSchedule, timeSlot);
                    }
                }

                //add this schedule entry and key to the dictionary.
                compressedDictionary.Add(scheduleKey, compressedSchedule);
            }

            //pass back the compressed schedule dictionary
            return compressedDictionary;

        }

        internal void WriteFile()
        {
            this.Helper.WriteJsonFile("CharacterScheduleLibrary.json", this);
        }

        internal void Initialize(string scheduleCharacter, Dictionary<string, string> dictionary)
        {
            if (!this.CharacterSchedules.ContainsKey(scheduleCharacter))
                this.CharacterSchedules.Add(scheduleCharacter, null);

            if (this.CharacterSchedules[scheduleCharacter] == null)
            {
                this.CharacterSchedules[scheduleCharacter] = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                foreach (KeyValuePair<string, string> scheduleKeyValuePair in dictionary)
                {
                    string key = scheduleKeyValuePair.Key;
                    string value = scheduleKeyValuePair.Value;

                    //this is the adjusted dictionary for the character in question. Contains time key and the scheduled events for each.
                    Dictionary<string, Dictionary<string, string>> adjustedScheduleDictionary = new Dictionary<string, Dictionary<string, string>>();
                    
                    string[] scheduleTimeRows = value.Split('/');
                    foreach(string scheduleTimeRow in scheduleTimeRows)
                    {

                        //temporary holder var for the original schedule row
                        string adjustedScheduleTimeRow = scheduleTimeRow;

                        //dialog pattern for stripping dialog out of [and back into] the schedule library
                        string dialogPattern = @""".*""";

                        //check to see if we found dialog
                        Match dialogRegexMatch = Regex.Match(scheduleTimeRow, dialogPattern);

                        //declare the dialog string, if it doesn't exist we don't use it.
                        string dialogString = null;

                        //if we did, strip it from the adjusted row [you can't modify the iterator]
                        if (dialogRegexMatch.Success)
                        {                            
                            dialogString = dialogRegexMatch.ToString();

                            //strip out the leading space behind the dialog string so we don't get an extra index
                            adjustedScheduleTimeRow = scheduleTimeRow.Replace($" {dialogString}", string.Empty);
                        }

                        //split the time row on a space, because... that's the delimiter.
                        string[] scheduleTimeItems = adjustedScheduleTimeRow.Split(' ');

                        //create the dictionary entry to hold all the various schedule factors of each time key outside the loop
                        Dictionary<string, string> adjustedScheduleDictionaryEntry = new Dictionary<string, string>();

                        //this is the time of day, we need it, stored as timeKey, as int.
                        string notFriendsKey = "NOT friendship";
                        string gotoKey = "GOTO";
                        if (adjustedScheduleTimeRow.Contains(notFriendsKey))
                        {
                            //rip the "friend" out of who you're not friends enough with. index 0 is who, and index 1 is how much.
                            string[] notFriendsWithWhoAndHowMuch = adjustedScheduleTimeRow.Replace($"{notFriendsKey} ", string.Empty).Split(' ');

                            //dictionary structure under this schedule key for the being not friends with someone passthrough.
                            Dictionary<string, string> notFriendsDictionary = new Dictionary<string, string>();

                            //in this case the key is "NOT friendship" and the key is "who"
                            notFriendsDictionary.Add(notFriendsWithWhoAndHowMuch[0], notFriendsWithWhoAndHowMuch[1]);

                            //add the weird schedule item to the dictionary
                            adjustedScheduleDictionary.Add(notFriendsKey, notFriendsDictionary);                            
                        } else if (adjustedScheduleTimeRow.Contains(gotoKey))
                        {
                            //rip the "friend" out of who you're not friends enough with. index 0 is who, and index 1 is how much.
                            string gotoWhere = adjustedScheduleTimeRow.Replace($"{gotoKey} ", string.Empty);

                            //the specific GOTO instruction for this row is seemingly a pass through to a different instruction, kind of like a lazy copy paste.
                            Dictionary<string, string> gotoSpecificInstruction = new Dictionary<string, string>();

                            //in this case the key is "GOTO", the index is always 0.
                            gotoSpecificInstruction.Add("gotoLabel", gotoWhere);

                            //add the weird schedule item to the dictionary
                            adjustedScheduleDictionary.Add(gotoKey, gotoSpecificInstruction);
                        } else
                        {
                            //the schedule key in this case is a time of day, this is the most common kind
                            string scheduleKey = scheduleTimeItems[0];
                            
                            //here's where we're storing literally everything but the timeKey.
                            List <string> scheduleFactors = new List<string>();

                            //capture all the various schedule factors for this particular time key.
                            foreach (string scheduleTimeItem in scheduleTimeItems)
                            {
                                if (scheduleTimeItem.Equals(scheduleKey.ToString()))
                                    continue;

                                scheduleFactors.Add(scheduleTimeItem);                            
                            }

                            //temporary until I parse what each field means, store it as an int key.
                            int index = 0;

                            //iterate over each item in this time slot and map it.
                            foreach (string scheduleFactor in scheduleFactors)
                            {
                                string translatedKey = null;
                                switch(index)
                                {
                                    case 0:
                                        translatedKey = "location";
                                        break;
                                    case 1:
                                        translatedKey = "xCoord";
                                        break;
                                    case 2:
                                        translatedKey = "yCoord";
                                        break;
                                    case 3:
                                        translatedKey = "facingDirection";
                                        break;
                                    case 4:
                                        translatedKey = "endBehavior";
                                        break;
                                    default:
                                        translatedKey = "unknownInstruction";
                                        break;
                                }
                                string adjustedScheduleFactor = scheduleFactor;
                                if (translatedKey.Equals("facingDirection"))
                                {
                                    switch (scheduleFactor)
                                    {
                                        case "0":
                                            adjustedScheduleFactor = "up";
                                            break;
                                        case "1":
                                            adjustedScheduleFactor = "right";
                                            break;
                                        case "2":
                                            adjustedScheduleFactor = "down";
                                            break;
                                        case "3":
                                            adjustedScheduleFactor = "left";
                                            break;
                                        default:
                                            adjustedScheduleFactor = "unknownDirection";
                                            break;
                                    }
                                }
                                adjustedScheduleDictionaryEntry.Add(translatedKey, adjustedScheduleFactor);                            
                                index++;
                            }

                            //add the dialog at the end, if it exists
                            if (dialogString != null)
                            {
                                adjustedScheduleDictionaryEntry.Add("dialog", dialogString);
                            }

                            //finally, finally.. add this time slot to the character
                            adjustedScheduleDictionary.Add(scheduleKey, adjustedScheduleDictionaryEntry);
                        }
                    }

                    //add this schedule key and the dictionary in its entirety.
                    this.CharacterSchedules[scheduleCharacter].Add(key, adjustedScheduleDictionary);
                }
            }

            //save progress on each successful initialization
            WriteFile();
        }
    }
}
