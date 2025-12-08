public interface ICollectable
{
    void Collect();
    bool IsCollected { get; }
    int GetPointValue();
}