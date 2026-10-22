namespace Fbs.Lg2;

/// <summary>The radio station: name, broadcast frequency, and the daily program.</summary>
public sealed class RadioSender
{
    private static int _nextId = 1;

    private readonly int _id;
    private string _name;
    private string _uebertraegungsFrequenz;
    private readonly RadioProgram _program;

    public int SenderId => _id;
    public string Name => _name;
    public string UebertragungsFrequenz => _uebertraegungsFrequenz;
    public RadioProgram Program => _program;

    public RadioSender(string name, string uebertraegungsFrequenz)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidNameException("Station name must not be null, empty, or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(uebertraegungsFrequenz))
        {
            throw new InvalidNameException("Broadcast frequency must not be null, empty, or whitespace.");
        }

        _id = _nextId++;
        _name = name.Trim();
        _uebertraegungsFrequenz = uebertraegungsFrequenz.Trim();
        _program = new RadioProgram();
    }
}
