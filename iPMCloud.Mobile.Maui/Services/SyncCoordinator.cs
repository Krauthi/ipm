using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iPMCloud.Mobile.vo.GlobalObjects;

namespace iPMCloud.Mobile.Services
{
    /// <summary>
    /// Coordinates the building sync logic independently of the UI.
    /// Can be called from the UI thread (iOS / direct) or from an Android ForegroundService.
    ///
    /// iOS note: iOS does not support long-running background tasks the same way Android does.
    /// The sync will run as long as the app stays in the foreground.  For true background execution
    /// on iOS, BGProcessingTask / BGAppRefreshTask would be required (system-scheduled, not
    /// suitable for user-triggered immediate syncs of this length).  Keeping the screen on while
    /// syncing is the practical mitigation on iOS.
    /// </summary>
    public class SyncCoordinator
    {
        private static SyncCoordinator _instance;
        public static SyncCoordinator Instance => _instance ??= new SyncCoordinator();

        private volatile bool _isRunning;
        public bool IsRunning => _isRunning;

        // --- Static events so both MainPage and the Android Service can subscribe ---

        /// <summary>Fired during the sync loop with current progress (0–100).</summary>
        public static event EventHandler<SyncProgressEventArgs> ProgressChanged;

        /// <summary>Fired once when the sync finishes (success or failure).</summary>
        public static event EventHandler<SyncCompletedEventArgs> SyncCompleted;

        private SyncCoordinator() { }

        /// <summary>
        /// Runs the full building sync asynchronously.
        /// Implements retry with exponential backoff for transient network errors.
        /// </summary>
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning)
                return;

            _isRunning = true;
            try
            {
                // ── Step 1: Fetch the building list ──────────────────────────────────────
                IpmNewSyncResponse buildingResponse = null;
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        buildingResponse = await AppModel.Instance.Connections
                            .IpmNewBuildingSync()
                            .ConfigureAwait(false);
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex) when (attempt < 2)
                    {
                        AppModel.Logger.Warn($"SyncCoordinator: IpmNewBuildingSync retry {attempt + 1}: {ex.Message}");
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken)
                            .ConfigureAwait(false);
                    }
                }

                if (buildingResponse == null || !buildingResponse.success)
                {
                    OnSyncCompleted(new SyncCompletedEventArgs
                    {
                        Success = false,
                        Response = buildingResponse,
                        ErrorMessage = buildingResponse?.message ?? "IpmNewBuildingSync fehlgeschlagen"
                    });
                    return;
                }

                // Update AppControll (no UI touch needed)
                if (buildingResponse.AppControll != null)
                {
                    AppModel.Instance.AppControll = buildingResponse.AppControll;
                    if (AppModel.Instance.AppControll == null)
                        AppModel.Instance.AppControll = new AppControll();
                    AppControll.Save(AppModel.Instance, AppModel.Instance.AppControll);
                }

                // ── Step 2: Fetch Aufträge in chunks ─────────────────────────────────────
                var buildings = buildingResponse.builgings ?? new List<BuildingWSO>();
                var blist = ListExtensions.ChunkBy(buildings.Distinct().ToList(), 10);
                var bs = new List<BuildingWSO>();
                int processed = 0;
                int successful = 0;

                for (int zz = 0; zz < blist.Count; zz++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    double pr = blist.Count == 0
                        ? 100d
                        : Math.Max(1d, Convert.ToDouble(zz + 1) / Convert.ToDouble(blist.Count) * 100d);

                    OnProgressChanged(new SyncProgressEventArgs
                    {
                        ProgressPercent = pr,
                        StatusText = $"SYNCHRONISATION ({pr:###}%)"
                    });

                    string objids = string.Join(",", blist[zz].Select(b => b.id.ToString()));

                    // Retry with exponential backoff
                    IpmNewSyncResponse resp = null;
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            resp = await AppModel.Instance.Connections
                                .IpmNewAuftragSyncAsync(objids)
                                .ConfigureAwait(false);
                            break;
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex) when (attempt < 2)
                        {
                            AppModel.Logger.Warn($"SyncCoordinator: IpmNewAuftragSync chunk {zz} retry {attempt + 1}: {ex.Message}");
                            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }

                    if (resp != null && resp.auftraege != null)
                    {
                        successful++;
                        for (int z = 0; z < blist[zz].Count; z++)
                        {
                            var aufs = resp.auftraege.FindAll(a => a.objektid == blist[zz][z].id);
                            blist[zz][z].ArrayOfAuftrag = aufs;
                        }
                    }

                    bs.AddRange(blist[zz]);
                    processed++;
                }

                buildingResponse.builgings = bs;

                OnProgressChanged(new SyncProgressEventArgs { ProgressPercent = 100d, StatusText = "SYNCHRONISATION (100%)" });

                if (processed == successful || blist.Count == 0)
                {
                    OnSyncCompleted(new SyncCompletedEventArgs { Success = true, Response = buildingResponse });
                }
                else
                {
                    OnSyncCompleted(new SyncCompletedEventArgs
                    {
                        Success = false,
                        Response = buildingResponse,
                        ErrorMessage = "Nicht vollständig synchronisiert"
                    });
                }
            }
            catch (OperationCanceledException)
            {
                AppModel.Logger.Info("SyncCoordinator: Sync abgebrochen.");
                OnSyncCompleted(new SyncCompletedEventArgs { Success = false, ErrorMessage = "Sync abgebrochen." });
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("SyncCoordinator: " + ex.Message);
                OnSyncCompleted(new SyncCompletedEventArgs { Success = false, ErrorMessage = ex.Message });
            }
            finally
            {
                _isRunning = false;
            }
        }

        private static void OnProgressChanged(SyncProgressEventArgs e) =>
            ProgressChanged?.Invoke(Instance, e);

        private static void OnSyncCompleted(SyncCompletedEventArgs e) =>
            SyncCompleted?.Invoke(Instance, e);
    }
}
