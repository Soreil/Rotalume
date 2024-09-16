using emulator.input;

using System.Windows.Input;

namespace WPFFrontend.Glue;

public class KeyBoardWithInterruptHandler
{
    private readonly Dictionary<Key, Action<bool>> keyActions;

    public event EventHandler<EventArgs>? KeyWentDown;

    public bool A { get; internal set; }
    public bool Select { get; internal set; }
    public bool B { get; internal set; }
    public bool DpadDown { get; internal set; }
    public bool DpadLeft { get; internal set; }
    public bool DpadRight { get; internal set; }
    public bool DpadUp { get; internal set; }
    public bool Start { get; internal set; }

    public KeyBoardWithInterruptHandler(Dictionary<Key, JoypadKey> mappedKeys)
    {
        keyActions = [];
        foreach (var (key, value) in mappedKeys)
        {
            keyActions[key] = value switch
            {
                JoypadKey.A => (state) => A = state,
                JoypadKey.B => (state) => B = state,
                JoypadKey.Select => (state) => Select = state,
                JoypadKey.Start => (state) => Start = state,
                JoypadKey.Up => (state) => DpadUp = state,
                JoypadKey.Down => (state) => DpadDown = state,
                JoypadKey.Left => (state) => DpadLeft = state,
                JoypadKey.Right => (state) => DpadRight = state,
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unsupported JoypadKey value: {value}")
            };
        }
    }

    public void Down(object? sender, KeyEventArgs e)
    {
        if (keyActions.TryGetValue(e.Key, out var KeyDown))
        {
            KeyDown(true);
            OnAnyKeyDown(EventArgs.Empty);
        }
    }

    public void Up(object? sender, KeyEventArgs e)
    {
        if (keyActions.TryGetValue(e.Key, out var KeyUp))
        {
            KeyUp(false);
        }
    }

    protected virtual void OnAnyKeyDown(EventArgs e) => KeyWentDown?.Invoke(this, e);
}
