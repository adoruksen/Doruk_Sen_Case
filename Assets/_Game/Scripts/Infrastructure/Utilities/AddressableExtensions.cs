using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RubyCase.Core
{
    public static class AddressableExtensions
    {
        public static UniTask ToUniTask(this AsyncOperationHandle handle)
        {
            if (handle.IsDone) return UniTask.CompletedTask;
            var tcs = new UniTaskCompletionSource();
            handle.Completed += _ => tcs.TrySetResult();
            return tcs.Task;
        }

        public static UniTask<T> ToUniTask<T>(this AsyncOperationHandle<T> handle)
        {
            if (handle.IsDone) return UniTask.FromResult(handle.Result);
            var tcs = new UniTaskCompletionSource<T>();
            handle.Completed += h => tcs.TrySetResult(h.Result);
            return tcs.Task;
        }
    }
}
