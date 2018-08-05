# SmashOrPass
Smash or Pass bot for Discord! Just compile, add your token to output dir as "secret.token", run and enjoy your game! Feel free to push your changes to the main repo.

Made with [Discord.Net](https://github.com/RogueException/Discord.Net), [Newtonsoft.JSON](https://github.com/JamesNK/Newtonsoft.Json) and [MimeTypes](https://github.com/khellang/MimeTypes) (last one is slight overkill, but it works!). 

## Commands
Discord-side:

|Command|Action|
|-------------|-------|
|!help        |Shows help screen|
|!start       |Starts SoP, send your picture by message attachment or just use link as argument|
|!stop        |Stops your SoP and displays the score|
|!pass @usr   |Passes selected user|
|!smash @usr  |Smashes selected user|
|!score @usr  |Shows selected user score|
|!listall     |Lists all currently running SoPs|

Bot-side:

|Command|Action|
|-------------|-------|
|help         |Shows help screen|
|score {id}   |Shows user score|
|list {str[]} |Lists all running SoPs from list|
|listall      |Lists all currently running SoPs|
---
Have fun!

FireBite, August 2018
