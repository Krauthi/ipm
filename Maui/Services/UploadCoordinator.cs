using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using iPMCloud.Mobile.vo;
using iPMCloud.Mobile.vo.wso;

namespace iPMCloud.Mobile.Services
{
    /// <summary>
    /// Coordinates processing of all upload stacks.
    /// Reads queued jobs from existing file-based stacks and uploads them sequentially.
    /// </summary>
    public class UploadCoordinator
    {
        private static UploadCoordinator _instance;
        public static UploadCoordinator Instance => _instance ??= new UploadCoordinator();

        public static event EventHandler<UploadProgressEventArgs> ProgressChanged;
        public static event EventHandler<UploadCompletedEventArgs> UploadCompleted;

        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(45);
        private volatile bool _isRunning;
        public bool IsRunning => _isRunning;

        private UploadCoordinator() { }

        public int GetPendingUploadCount()
        {
            int allCount = 0;
            allCount += CheckClass.CountFromStack();
            allCount += CheckLeistungAntwortBemImg.CountFromStack();
            allCount += BemerkungWSO.CountFromStack();
            allCount += BildWSO.CountFromStack();
            allCount += LeistungPackWSO.CountFromStack();
            allCount += AllTransSign.CountFromStack();
            allCount += DayOverWSO.CountFromStack();
            allCount += ObjektDataWSO.CountFromStack();
            allCount += ObjektDatenBildWSO.CountFromStack();
            allCount += PNWSO.CountFromStack();
            return allCount;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning)
                return;

            _isRunning = true;
            int total = 0;
            int processed = 0;
            bool failed = false;
            string errorMessage = null;

            try
            {
                total = GetPendingUploadCount();
                ReportProgress("UPLOADS werden vorbereitet…", processed, total);

                if (total == 0)
                {
                    OnUploadCompleted(new UploadCompletedEventArgs
                    {
                        Success = true,
                        ProcessedJobs = 0,
                        TotalJobs = 0
                    });
                    return;
                }

                if (!await ProcessDayOverAsync(cancellationToken, total, () => processed, v => processed = v).ConfigureAwait(false)) failed = true;
                if (!failed && !await ProcessChecksAsync(cancellationToken, total, () => processed, v => processed = v).ConfigureAwait(false)) failed = true;
                if (!failed && !await ProcessChecksBemImgAsync(cancellationToken, total, () => processed, v => processed = v).ConfigureAwait(false)) failed = true;
                if (!failed && !await ProcessBemerkungenAsync(cancellationToken, total, () => processed, v => processed = v).ConfigureAwait(false)) failed = true;
                if (!failed && !await ProcessNoticeBilderAsync(cancellationToken, total, () => processed, v => processed = v).ConfigureAwait(false)) failed = true;
                if (!failed && !await ProcessPositionenAsync(cancellationToken, total, () => processed, v => processed = v).ConfigureAwait(false)) failed = true;
                if (!failed && !await ProcessTransSignsAsync(cancellationToken, total, () => processed, v => processed = v).ConfigureAwait(false)) failed = true;
                if (!failed && !await ProcessObjectValuesAsync(cancellationToken, total, () => processed, v => processed = v).ConfigureAwait(false)) failed = true;
                if (!failed && !await ProcessObjectValueBilderAsync(cancellationToken, total, () => processed, v => processed = v).ConfigureAwait(false)) failed = true;
                if (!failed && !await ProcessPnAsync(cancellationToken, total, () => processed, v => processed = v).ConfigureAwait(false)) failed = true;

                if (failed)
                {
                    errorMessage = "Nicht alle Uploads konnten verarbeitet werden.";
                }

                OnUploadCompleted(new UploadCompletedEventArgs
                {
                    Success = !failed,
                    ErrorMessage = errorMessage,
                    ProcessedJobs = processed,
                    TotalJobs = total
                });
            }
            catch (OperationCanceledException)
            {
                OnUploadCompleted(new UploadCompletedEventArgs
                {
                    Success = false,
                    ErrorMessage = "Upload abgebrochen.",
                    ProcessedJobs = processed,
                    TotalJobs = total
                });
            }
            catch (Exception ex)
            {
                AppModel.Logger?.Error("UploadCoordinator: " + ex.Message);
                OnUploadCompleted(new UploadCompletedEventArgs
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProcessedJobs = processed,
                    TotalJobs = total
                });
            }
            finally
            {
                _isRunning = false;
            }
        }

        private async Task<bool> ProcessChecksAsync(CancellationToken token, int total, Func<int> getProcessed, Action<int> setProcessed)
        {
            token.ThrowIfCancellationRequested();
            ReportProgress("UPLOADS: Checklisten", getProcessed(), total);

            var checklist = CheckClass.LoadAllFromUploadStack() ?? new List<Check>();
            checklist = await RemoveAlreadyUploadedByGuidAsync(
                checklist,
                c => c.guid,
                c => CheckClass.DeleteFromUploadStack(c),
                token).ConfigureAwait(false);

            if (checklist.Count == 0)
                return true;

            foreach (var check in checklist)
            {
                token.ThrowIfCancellationRequested();
                var result = await ExecuteWithRetryAsync(
                    () => AppModel.Instance.Connections.SetCheckANonePic(check),
                    r => r != null && r.success,
                    r => r?.message,
                    "SetCheckANonePic",
                    token).ConfigureAwait(false);

                if (result == null || !result.success)
                    return false;

                result.checkA?.antworten?.ForEach(ant =>
                {
                    if (ant.bem?.imgs == null || ant.bem.imgs.Count == 0)
                        return;

                    ant.bem.imgs.ForEach(bemImg =>
                    {
                        var clabis = CheckLeistungAntwortBemImg.LoadFromGuid(bemImg.guid);
                        clabis.ForEach(clabi =>
                        {
                            clabi.bem_id = bemImg.bem_id;
                            CheckLeistungAntwortBemImg.SaveToStack(clabi);
                            CheckLeistungAntwortBemImg.Delete(clabi);
                        });
                    });
                });

                CheckClass.DeleteFromUploadStack(check);
                setProcessed(getProcessed() + 1);
                ReportProgress("UPLOADS: Checklisten", getProcessed(), total);
            }

            return true;
        }

        private async Task<bool> ProcessChecksBemImgAsync(CancellationToken token, int total, Func<int> getProcessed, Action<int> setProcessed)
        {
            token.ThrowIfCancellationRequested();
            ReportProgress("UPLOADS: Checklisten-Bilder", getProcessed(), total);

            var pics = CheckLeistungAntwortBemImg.LoadAllFromStack() ?? new List<CheckLeistungAntwortBemImg>();
            pics = await RemoveAlreadyUploadedByGuidAsync(
                pics,
                p => p.guid,
                p => CheckLeistungAntwortBemImg.DeleteFromStack(p),
                token).ConfigureAwait(false);

            foreach (var pic in pics)
            {
                token.ThrowIfCancellationRequested();
                var response = await ExecuteWithRetryAsync(
                    () => AppModel.Instance.Connections.SetCheckABemImg(pic),
                    r => r != null && r.success,
                    r => r?.message,
                    "SetCheckABemImg",
                    token).ConfigureAwait(false);

                if (response == null || !response.success)
                    return false;

                CheckLeistungAntwortBemImg.DeleteFromStack(pic);
                setProcessed(getProcessed() + 1);
                ReportProgress("UPLOADS: Checklisten-Bilder", getProcessed(), total);
            }

            return true;
        }

        private async Task<bool> ProcessObjectValuesAsync(CancellationToken token, int total, Func<int> getProcessed, Action<int> setProcessed)
        {
            token.ThrowIfCancellationRequested();
            ReportProgress("UPLOADS: Zählerstände", getProcessed(), total);

            var objectValues = ObjektDataWSO.LoadAllFromUploadStack(AppModel.Instance) ?? new List<ObjektDataWSO>();
            objectValues = await RemoveAlreadyUploadedByGuidAsync(
                objectValues,
                v => v.guid,
                v => ObjektDataWSO.DeleteFromUploadStack(AppModel.Instance, v),
                token).ConfigureAwait(false);

            if (objectValues.Count == 0)
                return true;

            var result = await ExecuteWithRetryAsync(
                () => AppModel.Instance.Connections.ObjectValuesSync(objectValues),
                r => r != null && r.success,
                r => r?.message,
                "ObjectValuesSync",
                token).ConfigureAwait(false);

            if (result == null || !result.success)
                return false;

            objectValues.ForEach(v => ObjektDataWSO.DeleteFromUploadStack(AppModel.Instance, v));
            setProcessed(getProcessed() + objectValues.Count);
            ReportProgress("UPLOADS: Zählerstände", getProcessed(), total);
            return true;
        }

        private async Task<bool> ProcessObjectValueBilderAsync(CancellationToken token, int total, Func<int> getProcessed, Action<int> setProcessed)
        {
            token.ThrowIfCancellationRequested();
            ReportProgress("UPLOADS: Zählerbilder", getProcessed(), total);

            var objectValueBilds = ObjektDatenBildWSO.LoadAllFromUploadStack(AppModel.Instance) ?? new List<ObjektDatenBildWSO>();
            objectValueBilds = await RemoveAlreadyUploadedByGuidAsync(
                objectValueBilds,
                v => v.guid,
                v => ObjektDatenBildWSO.DeleteFromUploadStack(AppModel.Instance, v),
                token).ConfigureAwait(false);

            foreach (var value in objectValueBilds)
            {
                token.ThrowIfCancellationRequested();
                var response = await ExecuteWithRetryAsync(
                    () => AppModel.Instance.Connections.ObjectValueBildSync(value),
                    r => r != null && r.success,
                    r => r?.message,
                    "ObjectValueBildSync",
                    token).ConfigureAwait(false);

                if (response == null || !response.success)
                    return false;

                ObjektDatenBildWSO.DeleteFromUploadStack(AppModel.Instance, value);
                setProcessed(getProcessed() + 1);
                ReportProgress("UPLOADS: Zählerbilder", getProcessed(), total);
            }

            return true;
        }

        private async Task<bool> ProcessPnAsync(CancellationToken token, int total, Func<int> getProcessed, Action<int> setProcessed)
        {
            token.ThrowIfCancellationRequested();
            if (PNWSO.CountFromStack() <= 0)
                return true;

            ReportProgress("UPLOADS: Push-Token", getProcessed(), total);

            var pn = PNWSO.LoadFromUploadStack();
            if (pn == null)
                return true;

            pn.personid = AppModel.Instance.Person.id;
            var response = await ExecuteWithRetryAsync(
                () => AppModel.Instance.Connections.PNSync(pn),
                r => r != null && r.success,
                r => r?.message,
                "PNSync",
                token).ConfigureAwait(false);

            if (response == null || !response.success)
                return false;

            PNWSO.DeleteFromUploadStack();
            AppModel.Instance.SettingModel.SettingDTO.PNToken = pn.token;
            AppModel.Instance.SettingModel.SaveSettings();
            setProcessed(getProcessed() + 1);
            ReportProgress("UPLOADS: Push-Token", getProcessed(), total);
            return true;
        }

        private async Task<bool> ProcessDayOverAsync(CancellationToken token, int total, Func<int> getProcessed, Action<int> setProcessed)
        {
            token.ThrowIfCancellationRequested();
            ReportProgress("UPLOADS: Feierabend", getProcessed(), total);

            var dayOvers = DayOverWSO.LoadAllFromUploadStack(AppModel.Instance) ?? new List<DayOverWSO>();
            dayOvers = await RemoveAlreadyUploadedByGuidAsync(
                dayOvers,
                d => d.guid,
                d => DayOverWSO.DeleteFromUploadStack(AppModel.Instance, d),
                token).ConfigureAwait(false);

            if (dayOvers.Count == 0)
                return true;

            var response = await ExecuteWithRetryAsync(
                () => AppModel.Instance.Connections.DayOverSync(dayOvers),
                r => r != null && r.success,
                r => r?.message,
                "DayOverSync",
                token).ConfigureAwait(false);

            if (response == null || !response.success)
                return false;

            dayOvers.ForEach(d => DayOverWSO.DeleteFromUploadStack(AppModel.Instance, d));
            setProcessed(getProcessed() + dayOvers.Count);
            ReportProgress("UPLOADS: Feierabend", getProcessed(), total);
            return true;
        }

        private async Task<bool> ProcessTransSignsAsync(CancellationToken token, int total, Func<int> getProcessed, Action<int> setProcessed)
        {
            token.ThrowIfCancellationRequested();
            ReportProgress("UPLOADS: Unterschriften", getProcessed(), total);

            var transSigns = AllTransSign.LoadAllFromUploadStack() ?? new List<AllTransSignRequest>();
            transSigns = await RemoveAlreadyUploadedByGuidAsync(
                transSigns,
                s => s.guid,
                s => AllTransSign.DeleteFromUploadStack(s),
                token).ConfigureAwait(false);

            foreach (var sign in transSigns)
            {
                token.ThrowIfCancellationRequested();
                var response = await ExecuteWithRetryAsync(
                    () => AppModel.Instance.Connections.AllTransSignSync(sign),
                    r => r != null && r.success,
                    r => r?.message,
                    "AllTransSignSync",
                    token).ConfigureAwait(false);

                if (response == null || !response.success)
                    return false;

                AllTransSign.DeleteFromUploadStack(sign);
                setProcessed(getProcessed() + 1);
                ReportProgress("UPLOADS: Unterschriften", getProcessed(), total);
            }

            return true;
        }

        private async Task<bool> ProcessBemerkungenAsync(CancellationToken token, int total, Func<int> getProcessed, Action<int> setProcessed)
        {
            token.ThrowIfCancellationRequested();
            ReportProgress("UPLOADS: Bemerkungen", getProcessed(), total);

            var bemerkungen = BemerkungWSO.LoadAllFromUploadStack(AppModel.Instance) ?? new List<BemerkungWSO>();
            bemerkungen = await RemoveAlreadyUploadedByGuidAsync(
                bemerkungen,
                b => b.guid,
                b => BemerkungWSO.DeleteFromUploadStack(AppModel.Instance, b),
                token).ConfigureAwait(false);

            foreach (var bem in bemerkungen)
            {
                token.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(bem.text?.Trim()) && (bem.photos == null || bem.photos.Count == 0))
                {
                    bem.hasSend = true;
                    BemerkungWSO.DeleteFromUploadStack(AppModel.Instance, bem);
                    setProcessed(getProcessed() + 1);
                    ReportProgress("UPLOADS: Bemerkungen", getProcessed(), total);
                    continue;
                }

                var response = await ExecuteWithRetryAsync(
                    () => AppModel.Instance.Connections.SingleNoticeSync(bem),
                    r => r != null && r.success,
                    r => r?.message,
                    "SingleNoticeSync",
                    token).ConfigureAwait(false);

                if (response == null || !response.success)
                    return false;

                bem.hasSend = true;
                var pics = BildWSO.LoadFromGuid(AppModel.Instance, bem.guid);
                pics.ForEach(p =>
                {
                    p.bemId = response.bemid;
                    if (bem.prio < 2)
                    {
                        BildWSO.SaveToStack(AppModel.Instance, p);
                    }
                    BildWSO.Delete(AppModel.Instance, p);
                });
                BemerkungWSO.DeleteFromUploadStack(AppModel.Instance, bem);
                setProcessed(getProcessed() + 1);
                ReportProgress("UPLOADS: Bemerkungen", getProcessed(), total);
            }

            return true;
        }

        private async Task<bool> ProcessNoticeBilderAsync(CancellationToken token, int total, Func<int> getProcessed, Action<int> setProcessed)
        {
            token.ThrowIfCancellationRequested();
            ReportProgress("UPLOADS: Bemerkungsbilder", getProcessed(), total);

            var pics = BildWSO.LoadAllFromStack() ?? new List<BildWSO>();
            pics = await RemoveAlreadyUploadedByGuidAsync(
                pics,
                p => p.guid,
                p => BildWSO.DeleteFromStack(p),
                token).ConfigureAwait(false);

            foreach (var pic in pics)
            {
                token.ThrowIfCancellationRequested();
                var response = await ExecuteWithRetryAsync(
                    () => AppModel.Instance.Connections.NoticeBildSync(pic),
                    r => r != null && r.success,
                    r => r?.message,
                    "NoticeBildSync",
                    token).ConfigureAwait(false);

                if (response == null || !response.success)
                    return false;

                BildWSO.DeleteFromStack(pic);
                setProcessed(getProcessed() + 1);
                ReportProgress("UPLOADS: Bemerkungsbilder", getProcessed(), total);
            }

            return true;
        }

        private async Task<bool> ProcessPositionenAsync(CancellationToken token, int total, Func<int> getProcessed, Action<int> setProcessed)
        {
            token.ThrowIfCancellationRequested();
            ReportProgress("UPLOADS: Positionen", getProcessed(), total);

            var packs = LeistungPackWSO.LoadAllFromUploadStack(AppModel.Instance) ?? new List<LeistungPackWSO>();
            packs.ForEach(lp =>
            {
                if (lp.leistungen == null || lp.leistungen.Count <= 0)
                    return;

                lp.leistungen.ForEach(l =>
                {
                    if (l.bemerkungen != null && l.bemerkungen.Count > 0)
                    {
                        l.bemerkungen = l.bemerkungen
                            .Where(b => !string.IsNullOrWhiteSpace(b.text?.Trim()) || (b.photos != null && b.photos.Count > 0))
                            .ToList();
                    }
                    if (l.bemerkungen != null && l.bemerkungen.Count == 0)
                    {
                        l.bemerkungen = null;
                    }
                });
            });

            packs = await RemoveAlreadyUploadedByGuidAsync(
                packs,
                p => p.guid,
                p => LeistungPackWSO.DeleteFromUploadStack(AppModel.Instance, p),
                token).ConfigureAwait(false);

            foreach (var pack in packs)
            {
                token.ThrowIfCancellationRequested();
                var positionResponse = await ExecuteWithRetryAsync(
                    () => AppModel.Instance.Connections.PositionSync(pack),
                    r => r != null && r.success,
                    r => r?.message,
                    "PositionSync",
                    token).ConfigureAwait(false);

                if (positionResponse == null || !positionResponse.success || positionResponse.pack == null)
                    return false;

                var resultPack = positionResponse.pack;
                resultPack.leistungen?.ForEach(l =>
                {
                    if (l.bemerkungen == null || l.bemerkungen.Count <= 0)
                        return;

                    l.bemerkungen.ForEach(b =>
                    {
                        if (b.id <= 0)
                            return;

                        b.hasSend = true;
                        var pics = BildWSO.LoadFromGuid(AppModel.Instance, b.guid);
                        pics.ForEach(p =>
                        {
                            p.bemId = b.id;
                            if (b.prio < 2)
                            {
                                BildWSO.SaveToStack(AppModel.Instance, p);
                            }
                            BildWSO.Delete(AppModel.Instance, p);
                        });
                    });
                });

                var building = AppModel.Instance.LastBuilding;
                if (building == null && resultPack.leistungen != null && resultPack.leistungen.Count > 0)
                {
                    building = BuildingWSO.LoadBuilding(AppModel.Instance, resultPack.leistungen[0].objektid);
                }

                if (building != null && resultPack.leistungen != null && resultPack.leistungen.Count > 0)
                {
                    building.ArrayOfAuftrag?.ForEach(o =>
                    {
                        o.kategorien?.ForEach(c =>
                        {
                            c.leistungen?.ForEach(p =>
                            {
                                var foundPos = resultPack.leistungen.Find(lei => lei.id == p.id);
                                if (foundPos != null &&
                                    p.timevaldays > 0 &&
                                    double.TryParse(p.lastwork, NumberStyles.Any, CultureInfo.InvariantCulture, out var lastWorkValue) &&
                                    lastWorkValue > 0)
                                {
                                    if (string.IsNullOrWhiteSpace(foundPos.workat) || foundPos.workat == "0")
                                    {
                                        foundPos.workat = (lastWorkValue + (p.timevaldays * 24d * 60d * 60d * 1000d)).ToString(CultureInfo.InvariantCulture);
                                    }
                                    p.workat = foundPos.workat;
                                }
                            });
                        });
                    });
                    BuildingWSO.Save(AppModel.Instance, building);
                }

                if (AppModel.Instance.AppControll != null &&
                    AppModel.Instance.AppControll.showObjektPlans &&
                    AppModel.Instance.PlanResponse != null &&
                    positionResponse.planweek != null &&
                    AppModel.Instance.PlanResponse.selectedPerson == null)
                {
                    AppModel.Instance.PlanResponse.planweek = positionResponse.planweek;
                    ObjektPlanWeekMobile.Save(AppModel.Instance, AppModel.Instance.PlanResponse);
                }

                LeistungPackWSO.DeleteFromUploadStack(AppModel.Instance, pack);
                setProcessed(getProcessed() + 1);
                ReportProgress("UPLOADS: Positionen", getProcessed(), total);
            }

            return true;
        }

        private async Task<List<T>> RemoveAlreadyUploadedByGuidAsync<T>(
            List<T> items,
            Func<T, string> guidSelector,
            Action<T> deleteAction,
            CancellationToken token)
        {
            if (items == null || items.Count == 0)
                return new List<T>();

            var guids = items.Select(guidSelector).Where(g => !string.IsNullOrWhiteSpace(g)).ToArray();
            if (guids.Length == 0)
                return items;

            var existing = await ExecuteWithRetryAsync(
                () => AppModel.Instance.Connections.GuidsCheck(guids),
                r => r != null,
                _ => null,
                "GuidsCheck",
                token).ConfigureAwait(false);

            if (existing == null || existing.Length == 0)
                return items;

            var set = new HashSet<string>(existing);
            var filtered = new List<T>();
            foreach (var item in items)
            {
                var guid = guidSelector(item);
                if (!string.IsNullOrWhiteSpace(guid) && set.Contains(guid))
                {
                    deleteAction(item);
                }
                else
                {
                    filtered.Add(item);
                }
            }

            return filtered;
        }

        private async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            Func<T, bool> successPredicate,
            Func<T, string> messageSelector,
            string operationName,
            CancellationToken token)
        {
            const int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    var result = await operation().WaitAsync(RequestTimeout, token).ConfigureAwait(false);
                    if (successPredicate(result))
                        return result;

                    if (attempt >= maxAttempts || !IsTransientMessage(messageSelector?.Invoke(result)))
                        return result;
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    throw;
                }
                catch (TimeoutException tex)
                {
                    AppModel.Logger?.Warn($"UploadCoordinator timeout ({operationName}), attempt {attempt}/{maxAttempts}: {tex.Message}");
                    if (attempt >= maxAttempts)
                        throw;
                }
                catch (Exception ex)
                {
                    AppModel.Logger?.Warn($"UploadCoordinator retry ({operationName}), attempt {attempt}/{maxAttempts}: {ex.Message}");
                    if (attempt >= maxAttempts || !IsTransientException(ex))
                        throw;
                }

                var delaySeconds = Math.Min(8d, Math.Pow(2, attempt - 1));
                var delay = TimeSpan.FromSeconds(delaySeconds);
                await Task.Delay(delay, token).ConfigureAwait(false);
            }

            return default;
        }

        private static bool IsTransientException(Exception ex)
        {
            return ex is HttpRequestException ||
                   ex is TimeoutException ||
                   ex is IOException ||
                   ex is TaskCanceledException;
        }

        private static bool IsTransientMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;

            var msg = message.ToLowerInvariant();
            return msg.Contains("timeout") ||
                   msg.Contains("timed out") ||
                   msg.Contains("nicht erreichbar") ||
                   msg.Contains("unterbrochen") ||
                   msg.Contains("tempor") ||
                   msg.Contains("429") ||
                   msg.Contains("503");
        }

        private void ReportProgress(string statusText, int processed, int total)
        {
            var normalizedTotal = Math.Max(total, 0);
            var normalizedProcessed = Math.Max(0, processed);
            var progress = normalizedTotal <= 0
                ? 100d
                : Math.Min(100d, Math.Max(0d, normalizedProcessed * 100d / normalizedTotal));

            OnProgressChanged(new UploadProgressEventArgs
            {
                ProgressPercent = progress,
                StatusText = normalizedTotal <= 0
                    ? statusText
                    : $"{statusText} ({normalizedProcessed}/{normalizedTotal})",
                ProcessedJobs = normalizedProcessed,
                TotalJobs = normalizedTotal
            });
        }

        private static void OnProgressChanged(UploadProgressEventArgs e) =>
            ProgressChanged?.Invoke(Instance, e);

        private static void OnUploadCompleted(UploadCompletedEventArgs e) =>
            UploadCompleted?.Invoke(Instance, e);
    }
}
