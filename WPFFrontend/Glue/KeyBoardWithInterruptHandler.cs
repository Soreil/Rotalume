using emulator;

using System.Windows.Input;

namespace WPFFrontend.Glue;

public class KeyBoardWithInterruptHandler
{
    public event EventHandler<EventArgs>? KeyWentDown;

    public bool A { get; internal set; }
    public bool Select { get; internal set; }
    public bool B { get; internal set; }
    public bool DpadDown { get; internal set; }
    public bool DpadLeft { get; internal set; }
    public bool DpadRight { get; internal set; }
    public bool DpadUp { get; internal set; }
    public bool Start { get; internal set; }

    private readonly Dictionary<Key, JoypadKey> _mappedKeys;
    public KeyBoardWithInterruptHandler(Dictionary<Key, JoypadKey> mappedKeys) => _mappedKeys = mappedKeys;

    public void Down(object? sender, KeyEventArgs e)
    {
        if (!_mappedKeys.ContainsKey(e.Key)) return;

        switch (_mappedKeys[e.Key])
        {
            case JoypadKey.A:
            A = true; break;
            case JoypadKey.B:
            B = true; break;
            case JoypadKey.Select:
            Select = true; break;
            case JoypadKey.Start:
            Start = true; break;
            case JoypadKey.Up:
            DpadUp = true; break;
            case JoypadKey.Down:
            DpadDown = true; break;
            case JoypadKey.Left:
            DpadLeft = true; break;
            case JoypadKey.Right:
            DpadRight = true; break;
        }

        OnAnyKeyDown(EventArgs.Empty);
    }

    public void Up(object? sender, KeyEventArgs e)
    {
        if (!_mappedKeys.ContainsKey(e.Key)) return;

        switch (_mappedKeys[e.Key])
        {
            case JoypadKey.A:
            A = false; break;
            case JoypadKey.B:
            B = false; break;
            case JoypadKey.Select:
            Select = false; break;
            case JoypadKey.Start:
            Start = false; break;
            case JoypadKey.Up:
            DpadUp = false; break;
            case JoypadKey.Down:
            DpadDown = false; break;
            case JoypadKey.Left:
            DpadLeft = false; break;
            case JoypadKey.Right:
            DpadRight = false; break;
        }
    }

    protected virtual void OnAnyKeyDown(EventArgs e) => KeyWentDown?.Invoke(this, e);
}
