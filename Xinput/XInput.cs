using System.Runtime.InteropServices;

namespace J2i.Net.XInputWrapper;

internal static partial class XInput
{
    [LibraryImport("xinput1_4.dll")]
    internal static partial int XInputGetState
    (
        int dwUserIndex,  // [in] Index of the gamer associated with the device
        ref XInputState pState        // [out] Receives the current state
    );

    [LibraryImport("xinput1_4.dll")]
    internal static partial int XInputSetState
    (
        int dwUserIndex,  // [in] Index of the gamer associated with the device
        ref XInputVibration pVibration    // [in, out] The vibration information to send to the controller
    );

    [LibraryImport("xinput1_4.dll")]
    internal static partial int XInputGetCapabilities
    (
        int dwUserIndex,   // [in] Index of the gamer associated with the device
        int dwFlags,       // [in] Input flags that identify the device type
        ref XInputCapabilities pCapabilities  // [out] Receives the capabilities
    );


    [LibraryImport("xinput1_4.dll")]
    internal static partial int XInputGetBatteryInformation
    (
          int dwUserIndex,        // Index of the gamer associated with the device
          byte devType,            // Which device on this user index
        ref XInputBatteryInformation pBatteryInformation // Contains the level and types of batteries
    );

    [LibraryImport("xinput1_4.dll")]
    internal static partial int XInputGetKeystroke
    (
        int dwUserIndex,              // Index of the gamer associated with the device
        int dwReserved,               // Reserved for future use
       ref XInputKeystroke pKeystroke    // Pointer to an XINPUT_KEYSTROKE structure that receives an input event.
    );
}
