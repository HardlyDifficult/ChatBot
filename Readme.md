# Project setup

 - Clone this repro
 - Clone [HardlyCommon](https://github.com/hardlydifficult/HardlyCommon)
 - Under references:
   - HardlyCommon
     - Add the HardlyCommon project (the one you just cloned) to your solution.
     - Add a project reference HardlyCommon
   - For every other yellow icon in references, in NuGet:
     - Uninstall and reinstall each.
       - Under options, you may need to check 'force' for uninstall to work.
 - Hit play.
   - It'll fail, but create a template for the settings file.

# How-to Use the Bot

 - Update ChatBot\bin\Settings.json
    - You can leave Twitter values null


## Overlay

 - There are a couple files created by the bot you may use in OBS:
   - TODO.txt (currently your stream title)
   - Keystrokes.txt (holds the most recent keyboard combinations like "ctrl+s")