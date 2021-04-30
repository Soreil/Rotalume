using System;

namespace J2i.Net.XInputWrapper
{
    public class XboxControllerWithInterruptHandler
    {
        public readonly XboxController X;
        public XboxControllerWithInterruptHandler(XboxController x)
        {
            X = x;

            X.StateChanged += X_StateChanged;

            PreviousState = new State(false, false, false, false, false, false, false, false);
            NewState = new State(false, false, false, false, false, false, false, false);
        }

        private State PreviousState;
        private State NewState;

        public event EventHandler<EventArgs>? KeyWentDown;

        private void X_StateChanged(object? sender, XboxControllerStateChangedEventArgs e)
        {
            PreviousState = NewState;

            NewState = new(
            X.IsDPadUpPressed,
            X.IsDPadDownPressed,
            X.IsDPadLeftPressed,
            X.IsDPadRightPressed,
            X.IsAPressed,
            X.IsBPressed,
            X.IsBackPressed,
            X.IsStartPressed);

            var AnyKeyWentDown =
            (!PreviousState.IsAPressed && NewState.IsAPressed) ||
            (!PreviousState.IsBPressed && NewState.IsBPressed) ||
            (!PreviousState.IsBackPressed && NewState.IsBackPressed) ||
            (!PreviousState.IsStartPressed && NewState.IsStartPressed) ||
            (!PreviousState.IsDPadUpPressed && NewState.IsDPadUpPressed) ||
            (!PreviousState.IsDPadDownPressed && NewState.IsDPadDownPressed) ||
            (!PreviousState.IsDPadLeftPressed && NewState.IsDPadLeftPressed) ||
            (!PreviousState.IsDPadRightPressed && NewState.IsDPadRightPressed);

            if (AnyKeyWentDown) OnAnyKeyDown(EventArgs.Empty);
        }

        protected virtual void OnAnyKeyDown(EventArgs e) => KeyWentDown?.Invoke(this, e);
    }

    internal record State(
        bool IsDPadUpPressed,
        bool IsDPadDownPressed,
        bool IsDPadLeftPressed,
        bool IsDPadRightPressed,
        bool IsAPressed,
        bool IsBPressed,
        bool IsBackPressed,
        bool IsStartPressed);
}
