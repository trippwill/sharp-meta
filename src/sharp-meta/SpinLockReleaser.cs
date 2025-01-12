using System.Diagnostics;

namespace SharpMeta;

/// <summary>
/// A struct that provides a mechanism for releasing a <see cref="SpinLock"/> when disposed.
/// </summary>
public struct SpinLockReleaser : IDisposable
{
    private readonly bool _lockTaken;
    private SpinLock _lock;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpinLockReleaser"/> struct and enters the specified <see cref="SpinLock"/>.
    /// </summary>
    /// <param name="lock">The <see cref="SpinLock"/> to be entered.</param>
    public SpinLockReleaser(ref SpinLock @lock)
    {
        _lock = @lock;
        _lockTaken = false;
        _lock.Enter(ref _lockTaken);
    }

    /// <summary>
    /// Releases the <see cref="SpinLock"/> if it was taken.
    /// </summary>
    public void Dispose()
    {
        Debug.Assert(_lockTaken, "Releasing lock when it was not acquired.");

        if (_lockTaken)
            _lock.Exit();
    }
}

/// <summary>
/// Provides extension methods for the <see cref="SpinLock"/> struct.
/// </summary>
public static class SpinLockExtensions
{
    /// <summary>
    /// Enters the <see cref="SpinLock"/> and returns a <see cref="SpinLockReleaser"/> that will release the lock when disposed.
    /// </summary>
    /// <param name="lock">The <see cref="SpinLock"/> to be entered.</param>
    /// <returns>A <see cref="SpinLockReleaser"/> that will release the lock when disposed.</returns>
    public static SpinLockReleaser GetReleaser(this ref SpinLock @lock) => new(ref @lock);
}
