namespace blockProject.randomSrc;

// domyślny typ hasnlowania błędami
public struct Error(string message)
{
    public readonly string error = message;
};
