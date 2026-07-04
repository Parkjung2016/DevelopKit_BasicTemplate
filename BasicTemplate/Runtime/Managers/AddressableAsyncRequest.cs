#if UNITASK_INSTALLED
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public delegate void AddressableCompletedHandler<in T>(string key, T result);
    public delegate void AddressableFailedHandler(string key, Exception exception);
    public delegate void AddressableCancelledHandler(string key);

    public sealed class AddressableAsyncRequest<T>
    {
        private readonly string key;
        private readonly Func<CancellationToken, UniTask<T>> operation;
        private readonly Func<T, bool> isSuccess;
        private CancellationToken cancellationToken;

        private AddressableCompletedHandler<T> onCompleted;
        private AddressableFailedHandler onFailed;
        private AddressableCancelledHandler onCancelled;

        internal AddressableAsyncRequest(
            string key,
            Func<CancellationToken, UniTask<T>> operation,
            Func<T, bool> isSuccess,
            CancellationToken cancellationToken = default)
        {
            this.key = key;
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
            this.isSuccess = isSuccess ?? throw new ArgumentNullException(nameof(isSuccess));
            this.cancellationToken = cancellationToken;
        }

        public AddressableAsyncRequest<T> OnCompleted(AddressableCompletedHandler<T> handler)
        {
            onCompleted = handler;
            return this;
        }

        public AddressableAsyncRequest<T> OnCompleted(Action<T> handler) =>
            OnCompleted((_, result) => handler(result));

        public AddressableAsyncRequest<T> OnCompleted(Action<string, T> handler) =>
            OnCompleted((key, result) => handler(key, result));

        public AddressableAsyncRequest<T> OnFailed(AddressableFailedHandler handler)
        {
            onFailed = handler;
            return this;
        }

        public AddressableAsyncRequest<T> OnFailed(Action<string, Exception> handler) =>
            OnFailed((key, exception) => handler(key, exception));

        public AddressableAsyncRequest<T> OnFailed(Action<Exception> handler) =>
            OnFailed((_, exception) => handler(exception));

        public AddressableAsyncRequest<T> OnCancelled(AddressableCancelledHandler handler)
        {
            onCancelled = handler;
            return this;
        }

        public AddressableAsyncRequest<T> OnCancelled(Action<string> handler) =>
            OnCancelled(key => handler(key));

        public AddressableAsyncRequest<T> OnCancelled(Action handler) =>
            OnCancelled(_ => handler());

        public AddressableAsyncRequest<T> WithCancellation(CancellationToken token)
        {
            cancellationToken = token;
            return this;
        }

        public UniTask<T> RunAsync() => RunInternalAsync();

        public void Run() => RunAsync().Forget();

        public UniTask<T>.Awaiter GetAwaiter() => RunAsync().GetAwaiter();

        private async UniTask<T> RunInternalAsync()
        {
            try
            {
                T result = await operation(cancellationToken);

                if (isSuccess(result))
                {
                    onCompleted?.Invoke(key, result);
                    return result;
                }

                onFailed?.Invoke(key, CreateOperationFailedException(key));
                return result;
            }
            catch (OperationCanceledException)
            {
                onCancelled?.Invoke(key);
                throw;
            }
            catch (Exception exception)
            {
                onFailed?.Invoke(key, exception);
                throw;
            }
        }

        private static Exception CreateOperationFailedException(string operationKey) =>
            new InvalidOperationException($"Addressable operation failed: {operationKey}");
    }

    public sealed class AddressableLoadAllRequest<T> where T : Object
    {
        private readonly AddressableManager manager;
        private readonly string label;

        private AddressableManager.OnResourceLoaded onResourceLoaded;
        private Action onAllLoaded;
        private AddressableFailedHandler onFailed;

        internal AddressableLoadAllRequest(AddressableManager manager, string label)
        {
            this.manager = manager;
            this.label = label;
        }

        public AddressableLoadAllRequest<T> OnResourceLoaded(AddressableManager.OnResourceLoaded handler)
        {
            onResourceLoaded = handler;
            return this;
        }

        public AddressableLoadAllRequest<T> OnAllLoaded(Action handler)
        {
            onAllLoaded = handler;
            return this;
        }

        public AddressableLoadAllRequest<T> OnFailed(AddressableFailedHandler handler)
        {
            onFailed = handler;
            return this;
        }

        public AddressableLoadAllRequest<T> OnFailed(Action<string, Exception> handler) =>
            OnFailed((key, exception) => handler(key, exception));

        public UniTask RunAsync() =>
            manager.LoadAllInternalAsync(label, onResourceLoaded, onAllLoaded, onFailed);

        public void Run() => RunAsync().Forget();

        public UniTask.Awaiter GetAwaiter() => RunAsync().GetAwaiter();
    }
}
#endif
