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
 - Update ChatBot\bin\Settings.json
    - You can leave Twitter values null