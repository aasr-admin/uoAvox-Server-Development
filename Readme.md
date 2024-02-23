[![N|Solid](http://www.uoavocation.net/portal/site_image/logos/logo_0001a.png)](http://www.uoavocation.net)
***
## uoAvox-Server-Development - `Official Release` <div align="Right">Last Updated: 03/17/2023</div>

>**What Is With The New RunUO Emulator?!**
-   Welcome to uoAvocation, our primary goal is to modify the server emulator ([RunUO](https://github.com/runuo/runuo)) and the custom client ([ClassicUO](http://www.classicuo.eu/)) in an effort to create a [game engine](https://en.wikipedia.org/wiki/Game_engine) that gamers can use to develop new genres of games while maintaining the same isometric graphic aspect ratio (1:1) as  [Ultima Online](http://www.uo.com/)™.

>**Who Should Download The Game Engine?**
-   That is entirely decided on a case-by-case basis, and it is really just a matter of preference. If you are an Ultima Online™ fanatic, like most people in the community, then I would recommend you stick with any RunUO_v2.x emulator. RunUO is easier to learn and there is a lot of support out there if you run into issues. Alternatively, you can use ServUO which is very well established and further ahead of RunUO with Ultima Online™ era-specific content.

-   However, if you want a  _fresh start_  at making a game, and you don't mind moving away from Ultima Online™, then uoAvos might be a better choice for you. Whether you want to recreate the medieval landscape and storylines of Ultima Online™ in your own vision, or you want to modify the ClassicUO client to create your own genre of game, uoAvos offers you the type of flexibility where your only limitation is your creativity.

>**What To Expect From Using This Repository?**
-   What everyone should understand is that the uoAvos game engine was designed to be generic; it follows where creativity leads. This project is a foundation to developing a truly custom  [mmo/ mmorpg](https://en.wikipedia.org/wiki/Massively_multiplayer_online_game)  experience. uoAvos DOES NOT follow the RunUO, ServUO, and ModernUO standard of Ultima Online™ emulation; we have deviated from it.
***

## uoAvos Features:

<img src="http://www.uoavocation.net/portal/site_image/github/checkmark.png" width="24" height="24">**The Servers Data Folder Has Been Integrated**
- uoAvos no longer uses a Data folder with its server. The reason is because we have taken its contents, hardcoded it in C#, and merged it with the server code.

<img src="http://www.uoavocation.net/portal/site_image/github/checkmark.png" width="24" height="24">**ActionAI: Immersive NPCs For Your Servers!**
- ActionAI is a new NPC Artificial Intelligence that makes NPCs on your server come to life! It is both immersive and makes a great framework to build future expansions on. This AI is extremely addictive!!
- Long gone are the days of NPCs who just stand there waiting for players to toggle quests off them and purchase their wares!
- Laborers now work, yes they have jobs and they do them! They gather resources from real harvest veins and add slight competition for players. All laborers set up camp and store their resources into crates nearby. Players can access these crates and loot them!
- Pirates now plunder the seas! Attacking player vessels, merchant ships, privateers, and other pirates along the way; the seas are no longer boring! If a player kills the boat captain of any vessel, they will have the option to commandeer the ship or sink it!
- All boats, including player boats, now sink into the depths. Once sunk, the ship will be at the bottom of the sea in the location it sunk for a duration; when the time is up it will disappear forever.
- Sunken ships will sink with the contents of its hull in-tact, so players can use that time duration to return to the ships location, jump into the sea, and plunder the ships hull. Doing so may yeild some nice loot!
- Caravans now wander from town to town! Caravans travel along trade routes that server owners can set up in their script.

<img src="http://www.uoavocation.net/portal/site_image/github/checkmark.png" width="24" height="24">**Poly3D Regions and In-Game Region Editor**
- region independent weather.
- create parent and child regions of any shape and size.
- add rules for each region; child regions can adopt their parent region conditions or they can have their own.
- Regions now have entry restrictions added for Creatures, NPCs, and vehicles
- new regions can be added immediately (without server restart).

<img src="http://www.uoavocation.net/portal/site_image/github/checkmark.png" width="24" height="24">**Multiple Currency Trading with Vendor Compatibility!**
- regions and vendors can have their own independent currency from one another; there is a heirarchy though. if a vendor is within a region which has a currency set, that regions currency type will override the vendors within its borders.
- regions can be assigned RED, BLUE, or NEUTRAL: this provides safe havens for criminals and murderers. It is also a great way to set up a gladiatorial arena in a much more efficient way (you can set the seating to <a href="https://en.wikipedia.org/wiki/Player_versus_environment">PvE</a> and the center of the arena to <a href="https://en.wikipedia.org/wiki/Player_versus_player">PvP</a>). This allows the fight to go on without the spectator(s) becoming mincemeat. 

<img src="http://www.uoavocation.net/portal/site_image/github/checkmark.png" width="24" height="24">**Assigned Home Cities and City Loyalty**
- When players initially log in they will be asked to choose a home city; this can be modified for custom facets and custom cities so this system works for everyone with a little tweaking. 
- Home city loyalty can be used as a flag to a condition, or series of conditions, that must be met with other systems and quests added to your servers. This addition has a lot of possibilities for expansion.
- All town guards now have their own AI and have been converted to BaseCreature. There is no longer an insta-kill for commiting crimes; guards will chase and fight.

<img src="http://www.uoavocation.net/portal/site_image/github/checkmark.png" width="24" height="24">**And Many More Expansions To Come...**
- uoAvos will always be a work-in-progress project. That means, that as long as we are alive and kicking we will continue to find ways to improve the uoAvos game engine to allow our users to create game servers that could potentially compete with any pixel-art-oriented isometric game on the market!
