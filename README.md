# TotalBathhouseOverhaul

## Adding Custom Inspectables
1. Create a tiledata on the `Buildings` object layer.
2. Add a property called `Action`.
3. Set the property value to be `CustomMessage "zTotalBathhouseOverhaul_MessageKey"`. ie. `CustomMessage "zTotalBathhouseOverhaul_SafetySign"`
4. In i18n/default.json, add a new entry with the key set to the one you set above, and the value to the message you want to display. See [this](https://stardewvalleywiki.com/Modding:Dialogue#Dialogue_commands) for formatting info.

Example:
```json
{
  "zTotalBathhouseOverhaul_SafetySign": "Prismuth pls change this.",
  "zTotalBathhouseOverhaul_MensLocker.6": "This is what will show up in the dialogue box.",
}
```

For other languages, add the same key/value pair but set the message to be in the corresponding language.

### Troubleshooting
If you don't see the message appear try the following:
* Ensure the message key on the property matches the json.