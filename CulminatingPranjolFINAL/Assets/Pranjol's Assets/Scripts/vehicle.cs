/// <summary>
/// Lightweight data record tracking a single vehicle's live race state.
/// Held by <see cref="Gamemanager"/> and sorted each update cycle to derive standings.
/// </summary>
public class vehicle
{
    public int    node;
    public string name;
    public bool   hasFinished;
    public int    lapCount;

    public vehicle(int node, string name, bool hasFinished, int lapCount = 0)
    {
        this.node        = node;
        this.name        = name;
        this.hasFinished = hasFinished;
        this.lapCount    = lapCount;
    }
}
