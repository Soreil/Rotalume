namespace emulator.input;
public class InputDevices
{
    //On SelectedController change we have to unmap the event handler of the previously selected gamepad
    //and register the event handler of the newly selected gamepad for gamepad interrupt handling.
    public int SelectedController
    {
        get;
        set
        {
            var previous = field;
            field = Math.Min(value, ControllerCount);

            //We don't have to remove the previous eventhandler in case it's 0
            if (previous != 0)
            {
                mappedControllers[previous - 1].controller.RemoveEventHandler(OnUnderLyingChanged);
            }
            if (field != 0)
                mappedControllers[field - 1].controller.AddEventHandler(OnUnderLyingChanged);
        }
    }

    //Event handlers
    private void OnUnderLyingChanged(object? sender, EventArgs e) => OnAnyKeyDown(e);
    protected virtual void OnAnyKeyDown(EventArgs e) => KeyWentDown?.Invoke(this, e);
    public event EventHandler<EventArgs>? KeyWentDown;

    private readonly List<MappedController> mappedControllers;

    public int ControllerCount => mappedControllers.Count;

    private readonly MappedController Keyboard;

    public InputDevices(IGameController keyboard, IEnumerable<IGameController> gameControllers)
    {
        Keyboard = new(keyboard);
        Keyboard.controller.AddEventHandler(OnUnderLyingChanged);
        mappedControllers = [];
        foreach (var k in gameControllers) mappedControllers.Add(new MappedController(k));
    }

    //Most game controllers have two seperate (potentially of different strength) motors
    public void Vibrate(double leftMotor, double rightMotor)
    {
        if (SelectedController != 0) mappedControllers[SelectedController - 1].Rumble(leftMotor, rightMotor);
    }

    public bool this[JoypadKey index]
    {
        get
        {
            //We consider keyboard to be the controller mapped to index 0, setting the selected controller to 0 is effectively a way to turn off the joypad
            return SelectedController != 0 ? 
                mappedControllers[SelectedController - 1][index] || Keyboard[index] : 
                Keyboard[index];
        }
    }
}