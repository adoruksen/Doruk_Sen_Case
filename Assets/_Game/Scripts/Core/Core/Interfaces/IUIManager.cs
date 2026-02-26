using Cysharp.Threading.Tasks;

namespace RubyCase.Core
{
    public interface IUIManager
    {
        UniTask OnStateEnterAsync(GameState state);
        UniTask OnStateExitAsync(GameState state);
    }
}
