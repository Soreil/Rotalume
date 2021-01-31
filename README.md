# Rotalume

This is supposed to become a somewhat accurate gameboy emulator targeting the original hardware. The emulator can currently play a bunch of games.
To run the emulator one can simply run ```dotnet run -p .\emulator.csproj``` in the emulator subfolder.
This project is using WPF and .NET 5

Missing major features:
- Sound
- RAM battery emulation (no saving!)
- Serial (no multiplayer)
- Memory Bank Controllers newer than MBC3
- Sub scanline PPU controls (not possible to do many more advanced graphical effects some games use)
- Accurate timing of internal steps of CPU instructions (For instance, for a 16 bit load load high and load low should happen on their own clock cycles but we just do all at once)

Test coverage:
- bgbtest: Fully working
- Blargg CPU instruction tests: Fully working
- Mooneye GB MBC1 tests: Fully working except for multiROM support
- Mooneye GB MBC2 tests: Fully working
- Mooneye GB MBC5 tests: Fully working (doesn't test rumble)
- Mooneye GB Timer tests: Fully working except for TMA_write
- ax6 rtc3test: Basic tests and accuracy tests working. No support yet for reading back minutes and seconds above 60 and hours above 24

Working games:
- Dr Mario
- Tetris
- Zelda, Link's Awakening

Broken games:
- Asteroids (Doesn't make it to any point where it draws an image)
