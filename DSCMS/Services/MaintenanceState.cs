namespace DSCMS.Services
{
  /// <summary>
  /// Process-wide gate for backup/restore. Only one backup or restore runs at a time; while one does,
  /// <see cref="Middleware.MaintenanceModeMiddleware"/> holds back other writes (and, during a restore, all traffic).
  /// </summary>
  public class MaintenanceState
  {
    private readonly SemaphoreSlim _gate = new(1, 1);
    private volatile bool _isRestoring;

    /// <summary>
    /// A backup or restore is running; admin writes must wait.
    /// </summary>
    public bool IsBusy => _gate.CurrentCount == 0;

    /// <summary>
    /// The database and static files are being replaced; nothing else should be served.
    /// </summary>
    public bool IsRestoring => _isRestoring;

    /// <summary>
    /// Takes the gate without waiting, or returns null if another backup/restore holds it.
    /// </summary>
    public Lease? TryEnter()
    {
      return _gate.Wait(0) ? new Lease(this) : null;
    }

    public sealed class Lease : IDisposable
    {
      private MaintenanceState? _owner;

      internal Lease(MaintenanceState owner) => _owner = owner;

      /// <summary>
      /// Stop serving the site until the lease is disposed.
      /// </summary>
      public void EnterRestoringMode()
      {
        if (_owner != null)
        {
          _owner._isRestoring = true;
        }
      }

      public void Dispose()
      {
        var owner = Interlocked.Exchange(ref _owner, null);
        if (owner != null)
        {
          owner._isRestoring = false;
          owner._gate.Release();
        }
      }
    }
  }
}
