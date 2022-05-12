# Rotalume

This is supposed to become a somewhat accurate gameboy emulator targeting the original Dot Matrix Game hardware. The emulator can currently play a bunch of games.
To run the emulator one can simply run ```dotnet run``` in the WPFFrontend folder. If performance is wanting add the ```-c Release ``` flag.

This project is using WPF for the graphical front end and .NET 6 with C# 10 for the emulator core.

Savegames:
Savegames are written to ```%appdata%\rotalume```, the savefile name is based on the internal name in the game ROM.
The savegame format is simply a dump of the cartridge RAM. This is optionally followed by 8 bytes for the RTC time and another 8 bytes for the system time at which the save was made. Only games which specify both an RTC and a battery backup get these last 16 bytes saved.

Controller support:
- Controller support is provided via XInput. Tested controllers include the Xbox 360, Xbox One, Dualshock 3, Dualshock 4 and others. 
- Rumble is supported for games which have a rumble motor in the cartridge.

Missing major features:
- Serial (no multiplayer)

Test coverage:
- bgbtest: Fully working
- Blargg CPU instruction tests: Fully working
- Blargg CPU instruction timing tests: Fully working
- Mooneye GB MBC1 tests: Fully working except for multiROM support
- Mooneye GB MBC2 tests: Fully working
- Mooneye GB MBC5 tests: Fully working including rumble
- Mooneye GB Timer tests: Fully working except for TMA_write
- ax6 rtc3test: Basic tests and accuracy tests working. No support yet for reading back minutes and seconds above 60 and hours above 24
- dmg-acid2
- Runs Pokemon Red
- Runs Pokemon Silver (buggy sound)
- Runs Pokemon Pinball (full sound)

Bugs:
- Sprite conflicts are not handled correctly, sometimes this will result in a sprite not being drawn if it collides with another sprite
- Sound is not properly synchronized to the video yet, over time there will be increasing delay on the sound

Planned:
- BESS (Best Effort Save State) format support
- Extremely basic serial support for some emulated peripherals like the game boy printer
