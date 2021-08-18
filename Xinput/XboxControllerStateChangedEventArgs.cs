namespace J2i.Net.XInputWrapper
{
    public class XboxControllerStateChangedEventArgs : EventArgs
    {
        internal XInputState CurrentInputState { get; set; }
        internal XInputState PreviousInputState { get; set; }
    }
}
