using Matcha.BackgroundService;
//using Plugin.Permissions;
using System;
using System.Threading.Tasks;

namespace iPMCloud.Mobile.vo
{
    public class Jobs
    {
    }

    ////public class SendUpload : IPeriodicTask
    ////{
    ////    public SendUpload(int seconds)
    ////    {
    ////        Interval = TimeSpan.FromSeconds(seconds);
    ////    }

    ////    public TimeSpan Interval { get; set; }

    ////    public Task<bool> StartJob()
    ////    {
    ////        int allCount = 0;
    ////        try
    ////        {
    ////            if (!AppModel.Instance.isInBackground || !AppModel.Instance.SettingModel.SettingDTO.RunBackground)
    ////            {
    ////                AppModel.Instance.isInBackgroundUploaded = false;
    ////                return Task.FromResult(true);
    ////            }
    ////            if (AppModel.Instance.isInBackground && !AppModel.Instance.isInBackgroundUploaded ) //&& AppModel.Instance.IsOnline)
    ////            {
    ////                var objectValues = ObjektDataWSO.LoadAllFromUploadStack(AppModel.Instance);
    ////                allCount += objectValues.Count;

    ////                var objectValueBilds = ObjektDatenBildWSO.LoadAllFromUploadStack(AppModel.Instance);
    ////                allCount += objectValueBilds.Count;

    ////                var bemerkungen = BemerkungWSO.LoadAllFromUploadStack(AppModel.Instance);
    ////                allCount += bemerkungen.Count;

    ////                var pn = PNWSO.LoadAllFromUploadStack();
    ////                allCount += pn != null ? 1 : 0;

    ////                var dayOvers = DayOverWSO.LoadAllFromUploadStack(AppModel.Instance);
    ////                allCount += dayOvers.Count;

    ////                var packs = LeistungPackWSO.LoadAllFromUploadStack(AppModel.Instance);
    ////                allCount += packs.Count;

    ////                if (objectValues.Count > 0)
    ////                {
    ////                    List<string> guidsList = new List<string>();
    ////                    objectValues.ForEach(v => { guidsList.Add(v.guid); });
    ////                    var resGuidsList = AppModel.Instance.Connections.GuidsCheckA(guidsList.ToArray());
    ////                    if (resGuidsList != null && resGuidsList.Length > 0)
    ////                    {
    ////                        resGuidsList.ToList().ForEach(guid =>
    ////                        {
    ////                            var da = objectValues.Find(b => b.guid == guid);
    ////                            if (da != null)
    ////                            {
    ////                                objectValues.Remove(da);
    ////                                ObjektDataWSO.DeleteFromUploadStack(AppModel.Instance, da);
    ////                            }
    ////                        });
    ////                    }

    ////                    if (resGuidsList != null && objectValues.Count > 0)
    ////                    {
    ////                        var result = AppModel.Instance.Connections.ObjectValuesSyncA(objectValues);
    ////                        if (result != null && result.success)
    ////                        {
    ////                            objectValues.ForEach(d =>
    ////                            {
    ////                                ObjektDataWSO.DeleteFromUploadStack(AppModel.Instance, d);
    ////                            });
    ////                        }
    ////                    }
    ////                }

    ////                if (objectValueBilds.Count > 0)
    ////                {
    ////                    List<string> guidsList = new List<string>();
    ////                    objectValueBilds.ForEach(v => { guidsList.Add(v.guid); });
    ////                    var resGuidsList = AppModel.Instance.Connections.GuidsCheckA(guidsList.ToArray());
    ////                    if (resGuidsList != null && resGuidsList.Length > 0)
    ////                    {
    ////                        resGuidsList.ToList().ForEach(guid =>
    ////                        {
    ////                            var da = objectValueBilds.Find(b => b.guid == guid);
    ////                            if (da != null)
    ////                            {
    ////                                objectValueBilds.Remove(da);
    ////                                ObjektDatenBildWSO.DeleteFromUploadStack(AppModel.Instance, da);
    ////                            }
    ////                        });
    ////                    }

    ////                    if (resGuidsList != null && objectValueBilds.Count > 0)
    ////                    {
    ////                        objectValueBilds.ForEach(value => {
    ////                            var result = AppModel.Instance.Connections.ObjectValueBildSyncA(value);
    ////                            if (result != null && result.success)
    ////                            {
    ////                                ObjektDatenBildWSO.DeleteFromUploadStack(AppModel.Instance, value);
    ////                            }
    ////                        });
    ////                    }
    ////                }


    ////                if (bemerkungen.Count > 0)
    ////                {
    ////                    List<string> guidsList = new List<string>();
    ////                    bemerkungen.ForEach(v => { guidsList.Add(v.guid); });
    ////                    var resGuidsList = AppModel.Instance.Connections.GuidsCheckA(guidsList.ToArray());
    ////                    if (resGuidsList != null && resGuidsList.Length > 0)
    ////                    {
    ////                        resGuidsList.ToList().ForEach(guid =>
    ////                        {
    ////                            var bem = bemerkungen.Find(b => b.guid == guid);
    ////                            if (bem != null)
    ////                            {
    ////                                bemerkungen.Remove(bem);
    ////                                BemerkungWSO.DeleteFromUploadStack(AppModel.Instance, bem);
    ////                            }
    ////                        });
    ////                    }
    ////                    if (resGuidsList != null && bemerkungen.Count > 0)
    ////                    {
    ////                        bemerkungen.ForEach(bem =>
    ////                        {
    ////                            var result = AppModel.Instance.Connections.NoticeSyncA(bem);
    ////                            if (result != null && result.success)
    ////                            {
    ////                                bem.hasSend = true;
    ////                                BemerkungWSO.DeleteFromUploadStack(AppModel.Instance, bem);
    ////                            }
    ////                        });
    ////                    }
    ////                }

    ////                if (pn != null)
    ////                {
    ////                    pn.personid = AppModel.Instance.Person.id;
    ////                    var resPN = AppModel.Instance.Connections.PNSyncA(pn);
    ////                    if (resPN.success)
    ////                    {
    ////                        PNWSO.DeleteFromUploadStack();
    ////                        AppModel.Instance.SettingModel.SettingDTO.PNToken = pn.token;
    ////                        AppModel.Instance.SettingModel.SaveSettings();
    ////                    }
    ////                }

    ////                if (dayOvers.Count > 0)
    ////                {
    ////                    List<string> guidsList = new List<string>();
    ////                    dayOvers.ForEach(v => { guidsList.Add(v.guid); });
    ////                    var resGuidsList = AppModel.Instance.Connections.GuidsCheckA(guidsList.ToArray());
    ////                    if (resGuidsList != null && resGuidsList.Length > 0)
    ////                    {
    ////                        resGuidsList.ToList().ForEach(guid =>
    ////                        {
    ////                            var da = dayOvers.Find(b => b.guid == guid);
    ////                            if (da != null)
    ////                            {
    ////                                dayOvers.Remove(da);
    ////                                DayOverWSO.DeleteFromUploadStack(AppModel.Instance, da);
    ////                            }
    ////                        });
    ////                    }

    ////                    if (resGuidsList != null && dayOvers.Count > 0)
    ////                    {
    ////                        DayOverResponse result = AppModel.Instance.Connections.DayOverSyncA(dayOvers);
    ////                        if (result != null && result.success)
    ////                        {
    ////                            dayOvers.ForEach(d =>
    ////                            {
    ////                                DayOverWSO.DeleteFromUploadStack(AppModel.Instance, d);
    ////                            });
    ////                        }
    ////                    };
    ////                }

    ////                if (packs.Count > 0)
    ////                {
    ////                    List<string> guidsList = new List<string>();
    ////                    packs.ForEach(v => { guidsList.Add(v.guid); });
    ////                    var resGuidsList = AppModel.Instance.Connections.GuidsCheckA(guidsList.ToArray());
    ////                    if (resGuidsList != null && resGuidsList.Length > 0)
    ////                    {
    ////                        resGuidsList.ToList().ForEach(guid =>
    ////                        {
    ////                            var pa = packs.Find(b => b.guid == guid);
    ////                            if (pa != null)
    ////                            {
    ////                                packs.Remove(pa);
    ////                                LeistungPackWSO.DeleteFromUploadStack(AppModel.Instance, pa);
    ////                            }
    ////                        });
    ////                    }


    ////                    if (resGuidsList != null && packs.Count > 0)
    ////                    {
    ////                        packs.ForEach(pack =>
    ////                        {
    ////                            var result = AppModel.Instance.Connections.PositionSyncA(pack);
    ////                            if (result != null && result.success)
    ////                            {
    ////                                // workat von result aktuell setzten
    ////                                var lastWorkTicks = "" + JavaScriptDateConverter.Convert(new DateTime(result.pack.endticks), -2);
    ////                                BuildingWSO building = null;
    ////                                if (AppModel.Instance.LastBuilding == null && result.pack.leistungen != null && result.pack.leistungen.Count > 0)
    ////                                {
    ////                                    building = BuildingWSO.LoadBuilding(AppModel.Instance, result.pack.leistungen[0].objektid);
    ////                                }
    ////                                if (AppModel.Instance.LastBuilding != null)
    ////                                {
    ////                                    building = AppModel.Instance.LastBuilding;
    ////                                }
    ////                                if (building != null)
    ////                                {
    ////                                    building.ArrayOfAuftrag.ForEach(o =>
    ////                                    {
    ////                                        o.kategorien.ForEach(c =>
    ////                                        {
    ////                                            c.leistungen.ForEach(p =>
    ////                                            {
    ////                                                var foundPos = result.pack.leistungen.Find(lei => lei.id == p.id);
    ////                                                if (foundPos != null)
    ////                                                {
    ////                                                    if (double.Parse(p.lastwork) > 0 && p.timevaldays > 0)
    ////                                                    {
    ////                                                        if (String.IsNullOrWhiteSpace(foundPos.workat) || foundPos.workat == "0")
    ////                                                        {
    ////                                                            foundPos.workat = "" + (double.Parse(p.lastwork) + (double.Parse("" + p.timevaldays) * 24 * 60 * 60 * 1000));
    ////                                                        }
    ////                                                        p.workat = foundPos.workat;
    ////                                                    }
    ////                                                }
    ////                                            });
    ////                                        });
    ////                                    });
    ////                                    BuildingWSO.Save(AppModel.Instance, building);
    ////                                }
    ////                                LeistungPackWSO.DeleteFromUploadStack(AppModel.Instance, pack);
    ////                            }
    ////                        });
    ////                    }
    ////                }

    ////                if (allCount == 0) { AppModel.Instance.isInBackgroundUploaded = true; };
    ////            }
    ////        }
    ////        catch (Exception ex)
    ////        {
    ////            AppModel.Logger.Error(ex, "[ERROR] Backgroundprocess StartJob:");
    ////        }
    ////        return Task.FromResult(true);
    ////    }
    ////}




    public class LocationInfo : IPeriodicTask
    {
        public bool GpsIsRunning { get; set; } = false;
        public LocationInfo(int seconds)
        {
            Interval = TimeSpan.FromSeconds(seconds);
        }

        public TimeSpan Interval { get; set; }

        public async Task<bool> StartJob()
        {
            if (!AppModel.Instance.isInBackground && !AppModel.Instance.GpsIsRunning
                && AppModel.Instance.SettingModel.SettingDTO.GPSInfoHasShow)
            {
                try
                {
                    if (!GpsIsRunning)
                    {
                        //AppModel.Logger.Info("Ping GPS");
                        GpsIsRunning = true;

                        AppModel.Instance.CheckPermissionGPS();
                        if (String.IsNullOrWhiteSpace(AppModel.Instance.checkPermissionGPSMessage))
                        {
                            //    return true;
                            //}
                            //if (CheckPermissionGPS().Result)
                            //{
                            AppModel.Instance.SetLocationGPS(true);// GetGeoLocation().Result;
                        }
                        GpsIsRunning = false;
                    }
                }
                catch (Exception ex)
                {
                    GpsIsRunning = false;
                    AppModel.Logger.Error("ERROR: Ping GPS in JOB: " + ex.Message);
                }
            }
            return true;
        }

        /*
        public async Task<string> GetGeoLocation(bool permCheck = false)
        {
            string LastLocationStr = "#LastLocation = null";
            try
            {

                var startGps = DateTime.Now;//.ToString("HH:mm:ss");
                Location lastlocation = await Geolocation.GetLastKnownLocationAsync();
                //Location lastlocationA = await Geolocation.GetLocationAsync(new GeolocationRequest { Timeout = new TimeSpan(4000), DesiredAccuracy = GeolocationAccuracy.Default });
                var endGps = DateTime.Now;//.ToString("HH:mm:ss");
                var s = (endGps - startGps).TotalSeconds;
                if (s > 4) { AppModel.Logger.Error("WARN: GPS TIME: Start:" + startGps.ToString("HH:mm:ss") + " - Ende: " + endGps.ToString("HH:mm:ss") + " = " + s + " Sekunden"); }

                if (lastlocation != null)
                {
                    LastLocationStr = ("" + lastlocation.Latitude + ";" + lastlocation.Longitude).Replace(",", ".");
                }
                //AppModel.Logger.Info("#LastLocationStr: " + LastLocationStr);

                return LastLocationStr;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                AppModel.Logger.Error("ERROR: location: " + "#Gerät hat kein GPS" + " - " + fnsEx.Message);
                return "#Gerät hat kein GPS";
            }
            catch (FeatureNotEnabledException fneEx)
            {
                AppModel.Logger.Error("ERROR: location: " + "#Nicht eingeschaltet" + " - " + fneEx.Message);
                return "#Nicht eingeschaltet";
            }
            catch (PermissionException pEx)
            {
                AppModel.Logger.Error("ERROR: location: " + "#Keine Berechtigung" + " - " + pEx.Message);
                return "#Keine Berechtigung";
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: location: " + ex.Message);
                return "#ERROR: location: " + ex.Message;
            }
        }

        private async Task<bool> CheckPermissionGPS()
        {
            try
            {
                var status = await AppModel.Instance.CheckPermissionGPS();
                if (String.IsNullOrWhiteSpace(status))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                AppModel.Logger.Error("ERROR: CheckPermissionGPS in JOB -> " + ex.Message);
                return false;
            }
        }

        */
    }







}