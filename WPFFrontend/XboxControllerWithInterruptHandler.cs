using J2i.Net.XInputWrapper;

using System;

namespace WPFFrontend
{
    public class XboxControllerWithInterruptHandler
    {
        public readonly XboxController X;
        public XboxControllerWithInterruptHandler(XboxController x)
        {
            X = x;

            X.StateChanged += X_StateChanged;
        }

        private State PreviousState;

        public event EventHandler<EventArgs>? KeyWentDown;

        private void X_StateChanged(object? sender, XboxControllerStateChangedEventArgs e)
        {
            State S = new(
            X.IsDPadUpPressed,
            X.IsDPadDownPressed,
            X.IsDPadLeftPressed,
            X.IsDPadRightPressed,
            X.IsAPressed,
            X.IsBPressed,
            X.IsBackPressed,
            X.IsStartPressed);

            var AnyKeyWentDown =
            (!PreviousState.IsAPressed && S.IsAPressed) ||
            (!PreviousState.IsBPressed && S.IsBPressed) ||
            (!PreviousState.IsBackPressed && S.IsBackPressed) ||
            (!PreviousState.IsStartPressed && S.IsStartPressed) ||
            (!PreviousState.IsDPadUpPressed && S.IsDPadUpPressed) ||
            (!PreviousState.IsDPadDownPressed && S.IsDPadDownPressed) ||
            (!PreviousState.IsDPadLeftPressed && S.IsDPadLeftPressed) ||
            (!PreviousState.IsDPadRightPressed && S.IsDPadRightPressed);

            PreviousState = S;

            if (AnyKeyWentDown) OnAnyKeyDown(EventArgs.Empty);
        }

        protected virtual void OnAnyKeyDown(EventArgs e) => KeyWentDown?.Invoke(this, e);
    }

    internal readonly struct State
    {
        public readonly bool IsDPadUpPressed;
        public readonly bool IsDPadDownPressed;
        public readonly bool IsDPadLeftPressed;
        public readonly bool IsDPadRightPressed;
        public readonly bool IsAPressed;
        public readonly bool IsBPressed;
        public readonly bool IsBackPressed;
        public readonly bool IsStartPressed;
        public State(bool IsDPadUpPressed,
                     bool IsDPadDownPressed,
                     bool IsDPadLeftPressed,
                     bool IsDPadRightPressed,
                     bool IsAPressed,
                     bool IsBPressed,
                     bool IsBackPressed,
                     bool IsStartPressed)
        {
            this.IsDPadUpPressed = IsDPadUpPressed;
            this.IsDPadDownPressed = IsDPadDownPressed;
            this.IsDPadLeftPressed = IsDPadLeftPressed;
            this.IsDPadRightPressed = IsDPadRightPressed;
            this.IsAPressed = IsAPressed;
            this.IsBPressed = IsBPressed;
            this.IsBackPressed = IsBackPressed;
            this.IsStartPressed = IsStartPressed;
        }
    }
}
