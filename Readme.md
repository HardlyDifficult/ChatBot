# Chat Bot

 - [For Streamers](#for-streamers)
 - [For Mods](#for-mods)
 - [For Developers](#for-developers)

## For Streamers

### How-to Use the Bot

 - Update Settings
    - Changes may require restarting the app
    - You can leave Twitter values null
 - The bot likes to whisper with the streamer.  You may need to follow the bot if whispers are not appearing.

### Commands

 - Commands: !help !command 
 - Aliases: !help !alias

TODO test:

 - ETA & Live (plus tweet), Uptime
 - Title / Game / Community:
 - Tweet
 - Shoutouts (why not help) - on demand and auto

### Events

 - First message
 - OnSub and on Bits -> Thanks (do we have a bit event yet)
 - Host and Hosting -> shoutout 

### UI

 - Stream title (updated anytime you change the text)
 - Issue command (this is the same as whispering the bot)
   - Note that the "!" prefix is optional when whispering the bot.
 - Settings (saves to settings.json)

### Overlay

 - There are a couple files created by the bot you may use in OBS (under the bin directory):
   - TODO.txt (currently your stream title)
   - Keystrokes.txt (holds the most recent keyboard combinations like "ctrl+s")

## For Mods

(coming soon?)

## For Developers

### Project setup

 - Clone this repro
 - Clone [HardlyCommon](https://github.com/hardlydifficult/HardlyCommon) in the same directory (so ChatBot and HardlyCommon folders are side by side)
 - If references are broken (very likely):
    - Open menu Tools -> NuGet Package Manager
    - Run ```Update-Package -reinstall```
 - Hit play.
   - It'll prompt you for settings.  Fill these in and then restart the app.

### Sync your forked repository

**Do this frequently if you are making changes**

 - Change directory to your local repository.
 - Switch to master branch if you are not ```git checkout master```
 - Add the parent as a remote repository, ```git remote add upstream https://github.com/hardlydifficult/ChatBot.git```
 - Issue ```git fetch upstream```
 - Issue ```git rebase upstream/master```
 - Check for pending merges with ```git status```
 - Issue ```git push origin master```
 
https://stackoverflow.com/a/31836086

### Code Design

 - ChatBotEngine 
   - Why project separation? - Facade
 - Dynamic commands

### TODO list

This bot is NOT ready for another streamer to use.  Our TODO list is here: https://trello.com/b/M3Z2GerB/chatbot

If you are interested in contributing, please!  Just let me know if you have questions.

If you are a streamer interested in using this, let us know what features you want.