namespace RubyCase.Core
{
    public interface IClickable
    {
        bool IsClickable { get; }
        void OnClicked();
    }
}