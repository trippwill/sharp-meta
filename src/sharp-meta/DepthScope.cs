namespace SharpMeta;

internal class DepthScope(int maxDepth)
{
    private int _depth = 0;
    private SpinLock _lock = new();

    public void Add()
    {
        using (_lock.GetReleaser())
        {
            if (++_depth >= maxDepth)
                throw new InvalidOperationException("Depth exceeds maximum.");
        }
    }

    public void Release()
    {
        using (_lock.GetReleaser())
        {
            if (_depth-- < 0)
                throw new InvalidOperationException("Depth is already zero.");
        }
    }

    public Releaser EnterScope() => new(this);

    public void ExecuteInScope<TArg>(TArg arg, Action<DepthScope, TArg> func)
    {
        using (this.EnterScope())
            func(this, arg);
    }

    public readonly struct Releaser : IDisposable
    {
        private readonly DepthScope _scope;

        public Releaser(DepthScope scope)
        {
            _scope = scope;
            _scope.Add();
        }

        public void Dispose() => _scope.Release();
    }
}
