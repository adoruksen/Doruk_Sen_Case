using Cysharp.Threading.Tasks;
using RubyCase.Core.GameLoop;

namespace RubyCase.Core.UI
{
    public interface IUIManager
    {
        UniTask OnStateEnterAsync(GameState state);
        UniTask OnStateExitAsync(GameState state);
    }
}
