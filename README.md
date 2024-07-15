
# Light Launcher

An easy, convenient, light-weight solution that allows you to launch your games and apps from your computer using a Gamepad.

<p align="center">
  <img src="docs/LIGHT_LAUNCHER_LOGO.png" height=350 alt="light_launcher_logo"/>
</p>

### Application Version: 1.2  
### Developed by: Sauraav Jayrajh

---

## Demos:

[Demo of Working Application](https://github.com/Saupernova13/lightlauncher/blob/main/docs/SAURAAV_LIGHT_LAUNCHER_DEMO.mp4)

---

## Developer Contact Details:

- **Email:** sauraavjayrajh@gmail.com

---

## Changelog:

### 1.2:
- Fixed bug that wouldn't let user add game after deleting a game
- Fixed issue where pressing triangle on the onscreen keyboard would leave double whitespaces
- Fixed issue where database would not create on systems with SQLEXPRESS

### 1.1:
- Added Emulator support to store emulator directories
- Added support for ROM files to boot via emulators they have added
- Compatibility with several PlayStation, Nintendo, Sega, and Xbox ROMS 
- (Specifically: .exe, .lnk, .iso, .gcm, .cso, .vpk, .n64, .z64, .v64, 
.wbfs, .wud, .wux, .nsp, .xci, .rvz, .3ds, .smd, .gen, .md, .chd, .cdi, .gdi)
- Emulators can be deleted from the program
- Minor tweaks to prior parts of the program have been implemented to function more efficiently

### 1.0:
- Launch games and applications directly from your Gamepad via quick menu
- Support for XInput devices
- Add and manage games through a controller-friendly interface
- On-screen keyboard for text input without the need for a physical keyboard
- Custom controller-friendly file menu for navigating directories

---

## Description:

Light Launcher is designed for fellow lazy gamers who want the versatility of launching their games and applications directly from a Gamepad, away from their device, much like the console experience. The goal is to provide a seamless gaming experience from the comfort of your couch, bed, or anywhere that isn't at your desktop.

---

## Getting Started:

### Software Requirements:
- Windows Operating System
- SQL Server or MySQL database

### Hardware Requirements:
- GamePad that supports XInput or has appropriate wrapper software (DS4Windows, x360ce)
- Bluetooth for wireless support or cable for USB support

---

## Installation Instructions:

### Releases:
1. Download and install the latest release from the [releases page](https://github.com/Saupernova13/lightlauncher/releases).

---

## Usage:

Upon running the application, use the following Gamepad inputs:

- **L1, R1, L3, R3:**  Simultaneously opens the Light Launcher menu.
- **Navigate the Menu:** Use the D-pad or thumb sticks to navigate.
- **Select Options:** Use the X button to select.
- **Settings:** Press R2 to access the settings menu.

### Adding a Game:
1. Open the menu and select "Add Game."
2. Enter the game name using the on-screen keyboard.
3. Select the game location using the custom file menu.
4. Select the game's cover image.
5. If the game requires an emulator, check the "Requires Emulator" box and add the emulator if it's not already listed.
6. Click "Add Game" to finalize.

### Launching a Game:
1. Open the menu and select the game you want to play.
2. If the game requires an emulator, choose the appropriate emulator from the list.
3. The game will launch.

---

## FAQ:

### General

**Q: How do I open the Light Launcher menu?**  
**A:** Press L1, R1, L3, R3 all at once on your Gamepad.

**Q: What controllers are supported?**  
**A:** Xbox controllers work out-of-the-box. PlayStation and Nintendo Switch controllers require wrapper programs like DS4Windows or x360ce.

**Q: How do I add games to Light Launcher?**  
**A:** Navigate to the "Add Game" option in the menu, enter the game details, and select the appropriate files using your Gamepad.

### Emulators

**Q: How do I add emulators?**  
**A:** In the R2 settings menu, press Triangle to "Add Emulator" and navigate to the emulator executable using the custom file menu.

**Q: Can Light Launcher run emulated games?**  
**A:** Yes, Light Launcher supports emulated games through added emulators.

### Customization

**Q: Can I change the color theme?**  
**A:** Custom color themes will be supported in future releases.

### Database

**Q: Do I need to install a database?**  
**A:** For now, yes. Future releases will support user accounts and online databases to eliminate the need for a local database.

---

## Plugins and Frameworks Used:

- SharpDX.XInput By Alexandre Mute
- EntityFramework By Microsoft

---

## License:

This project is licensed under the MIT License.
