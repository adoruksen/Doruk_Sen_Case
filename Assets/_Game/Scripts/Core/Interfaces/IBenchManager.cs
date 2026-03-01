using RubyCase.Gameplay.BoxSystem;

namespace RubyCase.Core.Session
{
    public interface IBenchManager
    {
        bool IsFull { get; }
        bool TryPlace(BoxController box);
        void Release(BoxController box);
    }
}