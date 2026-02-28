namespace RubyCase.BoxSystem
{
    public interface IBenchManager
    {
        bool IsFull { get; }
        bool TryPlace(BoxController box);
        void Release(BoxController box);
    }
}