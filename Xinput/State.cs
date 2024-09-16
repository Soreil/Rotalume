namespace J2i.Net.XInputWrapper;

internal record State(
    bool IsDPadUpPressed,
    bool IsDPadDownPressed,
    bool IsDPadLeftPressed,
    bool IsDPadRightPressed,
    bool IsAPressed,
    bool IsBPressed,
    bool IsBackPressed,
    bool IsStartPressed);
