namespace WPFFrontend;

public class Pause
{
    private readonly Model Model;

    public Pause(Model model)
    {
        Model = model;
    }
    public void Execute() => Model.Paused = !Model.Paused;
}
