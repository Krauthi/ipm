using Newtonsoft.Json;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iPMCloud.Mobile.vo
{
    public class Connections
    {
        //public CancellationToken cancellation = new CancellationToken();

        internal static Uri uri_Login = null;
        internal static Uri uri_LoginSmall = null;
        internal static Uri uri_Building = null;
        internal static Uri uri_NewSync = null;
        internal static Uri uri_NewSyncAuftrag = null;
        internal static Uri uri_FastSync = null;
        internal static Uri uri_GuidCheck = null;
        internal static Uri uri_SingleNotice = null;
        internal static Uri uri_NoticeBild = null;
        internal static Uri uri_AllTransSigns = null;
        internal static Uri uri_DayOver = null;
        internal static Uri uri_Position = null;
        internal static Uri uri_PositionAgain = null;
        internal static Uri uri_ObjectValues = null;
        internal static Uri uri_ObjectValueBild = null;
        internal static Uri uri_Log = null;
        internal static Uri uri_PersonTimes = null;
        internal static Uri uri_Plan = null;
        internal static Uri uri_ChecksInfo = null;
        internal static Uri uri_StartCheck = null;
        internal static Uri uri_DelCheckA = null;
        internal static Uri uri_SetCheckANonePics = null;
        internal static Uri uri_SetCheckABemImg = null;
        internal static Uri uri_GetCheckA = null;
        internal static Uri uri_UpdatePushToken = null;

        internal static HttpClient httpClientInstance;
        internal static HttpClient httpClientInstanceChecks;
        internal static HttpClient httpClientInstanceLogin;
        internal static HttpClient httpClientInstanceSyncGuid;
        internal static HttpClient httpClientInstanceSingleNotice;
        //internal static HttpClient httpClientInstanceNoticeBild;
        internal static HttpClient httpClientInstanceSync;
        internal static HttpClient httpClientInstancePNSync;
        //internal static HttpClient httpClientInstancePlan;


        public Connections()
        {
        }

        public Connections(AppModel appModel)
        {
            InitConnections();
        }

        public void InitConnections()
        {
            bool hasUri = true;
            if (AppModel.Instance.SettingModel.SettingDTO == null)
            {
                hasUri = false;
                AppModel.Logger.Error("Method => Connections() [INIT]: SettingsDTO is NULL !!! ");
            }
            if (AppModel.Instance.SettingModel.SettingDTO != null &&
                AppModel.Instance.SettingModel.SettingDTO.ServerUrl == null)
            {
                hasUri = false;
                AppModel.Logger.Error("Method => Connections() [INIT]: ServerURL is NULL !!!: " + AppModel.Instance.SettingModel.SettingDTO.ServerUrl);
            }
            else if (AppModel.Instance.SettingModel.SettingDTO != null &&
                String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.ServerUrl))
            {
                hasUri = false;
                AppModel.Logger.Error("Method => Connections() [INIT]: ServerURL is EMPTY !!!: " + AppModel.Instance.SettingModel.SettingDTO.ServerUrl);
            }

            if (hasUri)
            {
                // Use HttpClientManager instead of ServicePointManager (obsolete)
                // Certificate validation is now per-handler instead of global
                
                CookieContainer cookieContainer = new CookieContainer();
                HttpClientHandler httpClientHandler = HttpClientManager.CreateHandler(cookieContainer, HttpClientManager.TimeoutProfile.Medium);
                httpClientInstance = HttpClientManager.CreateClient(httpClientHandler);

                CookieContainer cookieContainerLogin = new CookieContainer();
                HttpClientHandler httpClientHandlerLogin = HttpClientManager.CreateHandler(cookieContainerLogin, HttpClientManager.TimeoutProfile.Short);
                httpClientInstanceLogin = HttpClientManager.CreateClient(httpClientHandlerLogin);

                CookieContainer cookieContainerSyncGuid = new CookieContainer();
                HttpClientHandler httpClientHandlerSyncGuid = HttpClientManager.CreateHandler(cookieContainerSyncGuid, HttpClientManager.TimeoutProfile.Short);
                httpClientInstanceSyncGuid = HttpClientManager.CreateClient(httpClientHandlerSyncGuid);

                CookieContainer cookieContainerSingleNotice = new CookieContainer();
                HttpClientHandler httpClientHandlerSingleNotice = HttpClientManager.CreateHandler(cookieContainerSingleNotice, HttpClientManager.TimeoutProfile.Long);
                httpClientInstanceSingleNotice = HttpClientManager.CreateClient(httpClientHandlerSingleNotice);

                //CookieContainer cookieContainerNoticeBild = new CookieContainer();
                //HttpClientHandler httpClientHandlerNoticeBild = new HttpClientHandler() { CookieContainer = cookieContainerNoticeBild };
                //httpClientHandlerNoticeBild.ServerCertificateCustomValidationCallback += (sender, cert, chein, sslpolicyerrors) => true;
                //httpClientInstanceNoticeBild = new HttpClient(httpClientHandlerNoticeBild);
                //httpClientInstanceNoticeBild.DefaultRequestHeaders.Clear();
                //httpClientInstanceNoticeBild.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //httpClientInstanceNoticeBild.DefaultRequestHeaders.ConnectionClose = false;// Wird nie geschlossen

                CookieContainer cookieContainerSync = new CookieContainer();
                HttpClientHandler httpClientHandlerSync = HttpClientManager.CreateHandler(cookieContainerSync, HttpClientManager.TimeoutProfile.Long);
                httpClientInstanceSync = HttpClientManager.CreateClient(httpClientHandlerSync);



                CookieContainer cookieContainerChecks = new CookieContainer();
                HttpClientHandler httpClientHandlerChecks = HttpClientManager.CreateHandler(cookieContainerChecks, HttpClientManager.TimeoutProfile.Medium);
                httpClientInstanceChecks = HttpClientManager.CreateClient(httpClientHandlerChecks);



                // URI initialization - connection lifetime now managed by HttpClientManager
                // jeweils eine eigene HttpClientInstanceLogin
                uri_Login = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/Login");
                uri_LoginSmall = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/LoginSmall");

                // jeweils eine eigene HttpClientInstance
                uri_NewSync = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/NewSync");
                uri_NewSyncAuftrag = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/NewSyncAuftrag");
                uri_Building = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/Building");
                uri_FastSync = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/FastSync");
                uri_AllTransSigns = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/AllTransSigns");
                uri_DayOver = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/DayOver");
                uri_Log = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/Log");
                uri_PersonTimes = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/PersonTimes");
                uri_Plan = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/Plan");
                uri_ChecksInfo = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/ChecksInfo");
                uri_StartCheck = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/StartCheck");
                uri_DelCheckA = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/DelCheckA");
                uri_GetCheckA = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/GetCheckA");
                uri_SetCheckANonePics = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/SetCheckANonePics");
                uri_SetCheckABemImg = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/SetCheckABemImg");

                // jeweils eine eigene httpClientInstanceSyncGuid
                uri_GuidCheck = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/GuidsCheck");

                // jeweils eine eigene httpClientInstanceSync
                uri_Position = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/Position");
                uri_PositionAgain = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/PositionAgain");
                uri_ObjectValues = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/ObjectValues");
                uri_ObjectValueBild = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/ObjectValueBild");

                // jeweils eine eigene httpClientInstanceSingleNotice
                uri_SingleNotice = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/SingleNotice");
                uri_NoticeBild = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/NoticeBild");

                InitPNConnections();
            }
        }

        public void InitPNConnections()
        {
            bool hasUri = true;
            if (AppModel.Instance.SettingModel.SettingDTO == null)
            {
                hasUri = false;
                AppModel.Logger.Error("Method => Connections() [INIT]: SettingsDTO is NULL !!! ");
            }
            if (AppModel.Instance.SettingModel.SettingDTO != null &&
                AppModel.Instance.SettingModel.SettingDTO.ServerUrl == null)
            {
                hasUri = false;
                AppModel.Logger.Error("Method => Connections() [INIT]: ServerURL is NULL !!!: " + AppModel.Instance.SettingModel.SettingDTO.ServerUrl);
            }
            else if (AppModel.Instance.SettingModel.SettingDTO != null &&
                String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.ServerUrl))
            {
                hasUri = false;
                AppModel.Logger.Error("Method => Connections() [INIT]: ServerURL is EMPTY !!!: " + AppModel.Instance.SettingModel.SettingDTO.ServerUrl);
            }

            if (hasUri)
            {
                // Use HttpClientManager instead of ServicePointManager (obsolete)
                // Certificate validation is now per-handler instead of global
                
                CookieContainer cookieContainerPNSync = new CookieContainer();
                HttpClientHandler httpClientHandlerPNSync = HttpClientManager.CreateHandler(cookieContainerPNSync, HttpClientManager.TimeoutProfile.Short);
                httpClientInstancePNSync = HttpClientManager.CreateClient(httpClientHandlerPNSync);

                uri_UpdatePushToken = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/UpdatePushToken");
            }
        }

        public async Task<IpmLoginResponse> IpmLogin(bool smallcheck)
        {
            HttpResponseMessage resMsg = null;
            if (uri_Login == null) { InitConnections(); }
            if (!AppModel.Instance.IsInternet)
            {
                return new IpmLoginResponse
                {
                    success = smallcheck ? true : false,
                    message = smallcheck ? "" : "Sie brauchen für diese Anfrage eine Onlineverbindung!",
                    sessionkey = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                };
            }
            HttpRequestMessage msg;
            string args = "";
            if (smallcheck && !String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.LoginToken))
            {
                args = JsonConvert.SerializeObject(new IpmLoginSmallRequest
                {
                    token = AppModel.Instance.SettingModel.SettingDTO.LoginToken
                });
                msg = new HttpRequestMessage(HttpMethod.Post, uri_LoginSmall);
            }
            else
            {
                var a = 0;
                args = JsonConvert.SerializeObject(new IpmLoginRequest
                {
                    bn = AppModel.Instance.SettingModel.SettingDTO.LoginName,
                    pw = AppModel.Instance.SettingModel.SettingDTO.LoginPassword
                });
                msg = new HttpRequestMessage(HttpMethod.Post, uri_Login);
            }

            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceLogin.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<IpmLoginResponse>(json);
                }
                else
                {
                    var m = "Method => IpmLogin(1): httpResponseMessage.StatusCode = " + resMsg.StatusCode + " - " + resMsg.RequestMessage;
                    AppModel.Logger.Warn(m);
                    resMsg?.Dispose();
                    return new IpmLoginResponse { success = false, message = m, person = null, sessionkey = "" };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    AppModel.Logger.Error("Method => IpmLogin(2): " + "Sie sind Online, jedoch ist der Server nicht erreichbar!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message);
                    return new IpmLoginResponse { success = false, message = "Method(IpmLogin(canceled)): Sie sind Online, jedoch ist der Server nicht erreichbar!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message, person = null, sessionkey = "" };
                }
                AppModel.Logger.Error("Method => IpmLogin(3): " + "Keine Onlineverbindung oder Server nicht erreichbar!: Bei dieser Anmeldung ist eine Onlinevebindung erforderlich!" + ex.Message);
                return new IpmLoginResponse { success = false, message = "IpmLogin - Keine Onlineverbindung oder Server nicht erreichbar!: Bei dieser Anmeldung ist eine Onlinevebindung erforderlich!" + ex.Message, person = null, sessionkey = "" };
            }
        }

        public async Task<IpmNewSyncResponse> IpmNewBuildingSync()
        {
            if (uri_NewSync == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            if (!AppModel.Instance.IsInternet)
            {
                return new IpmNewSyncResponse
                {
                    success = false,
                    message = "Sie brauchen für diese Aktion eine Onlineverbindung!",
                };
            }
            string args = "";
            args = JsonConvert.SerializeObject(new IpmNewSyncRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                objids = "",
                lastsync = String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ? 0 : long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks)
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_NewSync);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstance.SendAsync(msg);//6.369.836
                if (resMsg != null && resMsg.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Tokenzeit neu setzten
                    AppModel.Instance.SettingModel.SettingDTO.LastTokenDateTimeTicks = "" + DateTime.Now.Ticks;
                    AppModel.Instance.SettingModel.SaveSettings();

                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<IpmNewSyncResponse>(json);
                }
                else
                {
                    var m = "Method => IpmNewBuildingSync: httpResponseMessage.StatusCode = " + resMsg.StatusCode + " - " + resMsg.RequestMessage;
                    AppModel.Logger.Warn(m);
                    resMsg?.Dispose();
                    return new IpmNewSyncResponse { success = false, message = m };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    AppModel.Logger.Error("Method => IpmNewBuildingSync(canceled): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message);
                    return new IpmNewSyncResponse { success = false, message = "Method(IpmNewBuildingSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                AppModel.Logger.Error("Method => IpmNewBuildingSync(catch): " + ex.Message);
                return new IpmNewSyncResponse { success = false, message = "Method(IpmNewBuildingSync(catch)): " + ex.Message };
            }
        }

        public IpmNewSyncResponse IpmNewAuftragSync(string objids)
        {
            if (uri_NewSyncAuftrag == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            if (!AppModel.Instance.IsInternet)
            {
                return new IpmNewSyncResponse
                {
                    success = false,
                    message = "Sie brauchen für diese Aktion eine Onlineverbindung!",
                };
            }
            string args = "";
            args = JsonConvert.SerializeObject(new IpmNewSyncRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                objids = objids,
                lastsync = String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ? 0 : long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks)
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_NewSyncAuftrag);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = httpClientInstance.SendAsync(msg).Result;//6.369.836
                if (resMsg != null && resMsg.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Tokenzeit neu setzten
                    AppModel.Instance.SettingModel.SettingDTO.LastTokenDateTimeTicks = "" + DateTime.Now.Ticks;
                    AppModel.Instance.SettingModel.SaveSettings();

                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<IpmNewSyncResponse>(json);
                }
                else
                {
                    var m = "Method => IpmNewAuftragSync: httpResponseMessage.StatusCode = " + resMsg.StatusCode + " - " + resMsg.RequestMessage;
                    AppModel.Logger.Warn(m);
                    resMsg?.Dispose();
                    return new IpmNewSyncResponse { success = false, message = m };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    AppModel.Logger.Error("Method => IpmNewAuftragSync(canceled): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message);
                    return new IpmNewSyncResponse { success = false, message = "Method(IpmNewAuftragSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                AppModel.Logger.Error("Method => IpmNewAuftragSync(catch): " + ex.Message);
                return new IpmNewSyncResponse { success = false, message = "Method(IpmNewAuftragSync(catch)): " + ex.Message };
            }
        }

        public async Task<IpmBuildingResponse> IpmBuildingSync()
        {
            if (uri_Building == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            if (!AppModel.Instance.IsInternet)
            {
                return new IpmBuildingResponse
                {
                    success = false,
                    message = "Sie brauchen für diese Aktion eine Onlineverbindung!",
                };
            }
            string args = "";
            args = JsonConvert.SerializeObject(new IpmBuildingRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                objids = "",
                lastsync = String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ? 0 : long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks)
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_Building);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstance.SendAsync(msg);//6.369.836
                if (resMsg != null && resMsg.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Tokenzeit neu setzten
                    AppModel.Instance.SettingModel.SettingDTO.LastTokenDateTimeTicks = "" + DateTime.Now.Ticks;
                    AppModel.Instance.SettingModel.SaveSettings();

                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<IpmBuildingResponse>(json);
                }
                else
                {
                    var m = "Method => IpmBuildingSync: httpResponseMessage.StatusCode = " + resMsg.StatusCode + " - " + resMsg.RequestMessage;
                    AppModel.Logger.Warn(m);
                    resMsg?.Dispose();
                    return new IpmBuildingResponse { success = false, message = m };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    AppModel.Logger.Error("Method => IpmBuildingSync(canceled): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message);
                    return new IpmBuildingResponse { success = false, message = "Method(IpmBuildingSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                AppModel.Logger.Error("Method => IpmBuildingSync(catch): " + ex.Message);
                return new IpmBuildingResponse { success = false, message = "Method(IpmLIpmBuildingSyncogin(catch)): " + ex.Message };
            }
        }

        public async Task<IpmBuildingResponse> IpmFastSync()
        {
            if (uri_FastSync == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            if (!AppModel.Instance.IsInternet)
            {
                return new IpmBuildingResponse
                {
                    success = false,
                    message = "Sie brauchen für diese Aktion eine Onlineverbindung!",
                };
            }

            string args = JsonConvert.SerializeObject(new IpmBuildingRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                objids = "",
                lastsync = String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ? 0 : long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks)
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_FastSync);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstance.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Tokenzeit neu setzten
                    AppModel.Instance.SettingModel.SettingDTO.LastTokenDateTimeTicks = "" + DateTime.Now.Ticks;
                    AppModel.Instance.SettingModel.SaveSettings();

                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<IpmBuildingResponse>(json);
                }
                else
                {
                    var m = "Method => IpmBuildingSync: httpResponseMessage.StatusCode = " + resMsg.StatusCode + " - " + resMsg.RequestMessage;
                    AppModel.Logger.Warn(m);
                    resMsg?.Dispose();
                    return new IpmBuildingResponse { success = false, message = m };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    AppModel.Logger.Error("Method => IpmBuildingSync(canceled): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message);
                    return new IpmBuildingResponse { success = false, message = "Method(IpmBuildingSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                AppModel.Logger.Error("Method => IpmBuildingSync(catch): " + ex.Message);
                return new IpmBuildingResponse { success = false, message = "Method(IpmLIpmBuildingSyncogin(catch)): " + ex.Message };
            }
        }

        public async Task<string[]> GuidsCheck(string[] guidsList)
        {
            if (uri_GuidCheck == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new GuidsCheckRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                guids = guidsList
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_GuidCheck);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceSyncGuid.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<GuidsCheckResponse>(json).guids;
                }
                else
                {
                    resMsg?.Dispose();
                    return null;
                }
            }
            catch (Exception)
            {
                AppModel.Logger.Error("ERROR: (GuidsCheck) Netzwerk nicht erreichbar!");
                resMsg?.Dispose();
                return null;
            }
        }

        public async Task<SingleNoticeResponse> SingleNoticeSync(BemerkungWSO bem)
        {
            if (uri_SingleNotice == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(
                new SingleNoticeRequest(AppModel.Instance.SettingModel.SettingDTO.LoginToken, bem, bem.prio > 1));// photos entfernen wenn nicht störemeldung

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_SingleNotice);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceSingleNotice.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<SingleNoticeResponse>(json);
                }
                else
                {
                    resMsg?.Dispose();
                    return new SingleNoticeResponse { success = false, message = "Method(SingleNoticeSync): resMsg is Null oder Status ist nicht OK!" };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    return new SingleNoticeResponse { success = false, message = "Method(SingleNoticeSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                return new SingleNoticeResponse { success = false, message = "Method(SingleNoticeSync(catch)): " + ex.Message };
            }
        }

        public async Task<NoticeBildResponse> NoticeBildSync(BildWSO bild)
        {
            if (uri_NoticeBild == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new NoticeBildRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                bild = bild,
                gruppeid = AppModel.Instance.Person.gruppeid
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_NoticeBild);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceSingleNotice.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<NoticeBildResponse>(json);
                }
                else
                {
                    resMsg?.Dispose();
                    return new NoticeBildResponse { success = false, message = "Method(NoticeBildSync): resMsg is Null oder Status ist nicht OK!" };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    return new NoticeBildResponse { success = false, message = "Method(NoticeBildSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                return new NoticeBildResponse { success = false, message = "Method(NoticeBildSync(catch)): " + ex.Message };
            }
        }

        public async Task<AllTransSignResponse> AllTransSignSync(AllTransSignRequest signs)
        {
            if (uri_AllTransSigns == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(signs);

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_AllTransSigns);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstance.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<AllTransSignResponse>(json);
                }
                else
                {
                    resMsg?.Dispose();
                    return new AllTransSignResponse { success = false, message = "Method(AllTransSignSync): resMsg is Null oder Status ist nicht OK!" };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    return new AllTransSignResponse { success = false, message = "Method(AllTransSignSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                return new AllTransSignResponse { success = false, message = "Method(AllTransSignSync(catch)): " + ex.Message };
            }
        }

        public async Task<DayOverResponse> DayOverSync(List<DayOverWSO> dayOvers)
        {
            if (uri_DayOver == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new DayOverRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                dayOvers = dayOvers
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_DayOver);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstance.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<DayOverResponse>(json);
                }
                else
                {
                    resMsg?.Dispose();
                    return new DayOverResponse { success = false, message = "Method(DayOverSync): resMsg is Null oder Status ist nicht OK!" };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    return new DayOverResponse { success = false, message = "Method(DayOverSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                return new DayOverResponse { success = false, message = "Method(DayOverSync(catch)): " + ex.Message };
            }
        }


        public async Task<PositionResponse> PositionSync(LeistungPackWSO pack)
        {
            if (uri_Position == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            if (pack.leistungen != null && pack.leistungen.Count > 0)
            {
                pack.leistungen.ForEach(l =>
                {
                    if (l.bemerkungen != null && l.bemerkungen.Count > 0)
                    {
                        l.bemerkungen.ForEach(b =>
                        {
                            if (b.prio < 2)
                            {
                                b.photos = null;
                            }
                        });
                    }
                });
            }

            string args = JsonConvert.SerializeObject(new PositionRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                pack = pack
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_Position);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceSync.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<PositionResponse>(json);
                }
                else
                {
                    resMsg?.Dispose();
                    return new PositionResponse { success = false, message = "Method(PositionSync): resMsg is Null oder Status ist nicht OK!" };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    return new PositionResponse { success = false, message = "Method(PositionSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                return new PositionResponse { success = false, message = "Method(PositionSync(catch)): " + ex.Message };
            }
        }

        public async Task<PositionResponse> PositionAgainSync(LeistungPackWSO pack)
        {
            if (uri_PositionAgain == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new PositionRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                pack = pack
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_PositionAgain);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceSync.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<PositionResponse>(json);
                }
                else
                {
                    resMsg?.Dispose();
                    return new PositionResponse { success = false, message = "Method(NoticeSync): resMsg is Null oder Status ist nicht OK!" };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    return new PositionResponse { success = false, message = "Method(PositionAgainSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                return new PositionResponse { success = false, message = "Method(PositionAgainSync(catch)): " + ex.Message };
            }
        }

        public async Task<ObjectValuesResponse> ObjectValuesSync(List<ObjektDataWSO> objectValues)
        {
            if (uri_ObjectValues == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;
            string args = JsonConvert.SerializeObject(new ObjectValuesRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                objectValues = objectValues,
                personid = AppModel.Instance.Person.id
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_ObjectValues);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceSync.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<ObjectValuesResponse>(json);
                }
                else
                {
                    resMsg?.Dispose();
                    return new ObjectValuesResponse { success = false, message = "Method(ObjectValuesSync): resMsg is Null oder Status ist nicht OK!" };
                }
            }
            catch (Exception ex)
            {
                if (resMsg != null) { resMsg.Dispose(); }
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    return new ObjectValuesResponse { success = false, message = "Method(ObjectValuesSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                return new ObjectValuesResponse { success = false, message = "Method(ObjectValuesSync(catch)): " + ex.Message };
            }
        }

        public async Task<ObjectValueBildResponse> ObjectValueBildSync(ObjektDatenBildWSO value)
        {
            if (uri_ObjectValueBild == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new ObjectValueBildRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                objectValueBild = value,
                personid = AppModel.Instance.Person.id
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_ObjectValueBild);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceSync.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<ObjectValueBildResponse>(json);
                }
                else
                {
                    resMsg?.Dispose();
                    return new ObjectValueBildResponse { success = false, message = "Method(ObjectValueBildSync): resMsg is Null oder Status ist nicht OK!" };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    return new ObjectValueBildResponse { success = false, message = "Method(ObjectValueBildSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                return new ObjectValueBildResponse { success = false, message = "Method(ObjectValueBildSync(catch)): " + ex.Message };
            }
        }


        public async Task<bool> LogSync(string log)
        {
            if (uri_Log == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new LogWSO
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                name = AppModel.Instance.Person.vorname + " " + AppModel.Instance.Person.name,
                mandant = AppModel.Instance.SettingModel.SettingDTO.CustomerName,
                date = JavaScriptDateConverter.Convert(DateTime.Now).ToString("dd.MM.yyyy - HH:mm:ss"),
                log = log
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_Log);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstance.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<bool>(json);
                }
                else
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg?.Dispose();
                    return false;
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                return false;
            }
        }



        public async Task<PersonTimeResponse> GetPersonTimes(int year, int month)
        {
            if (!AppModel.Instance.IsInternet)
            {
                return new PersonTimeResponse
                {
                    success = false,
                    message = "(GetPersonTimes) KEIN INTERNET! - Sie brauchen für diese Aktion eine Onlineverbindung!",
                };
            }
            if (uri_PersonTimes == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new PersonTimeRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                year = year,
                month = month,
                gruppeid = AppModel.Instance.Person.gruppeid,
                personid = AppModel.Instance.Person.id
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_PersonTimes);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstance.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<PersonTimeResponse>(json);
                }
                else
                {
                    resMsg?.Dispose();
                    return new PersonTimeResponse { success = false, message = "Method(GetPersonTimes): resMsg is Null oder Status ist nicht OK!" };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    return new PersonTimeResponse { success = false, message = "Method(GetPersonTimes(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                return new PersonTimeResponse { success = false, message = "Method(GetPersonTimes(catch)): " + ex.Message };
            }
        }


        public async Task<bool> GetPlanPersons(Int32 otherPersonId = 0, bool isOtherPerson = false)
        {
            if (!AppModel.Instance.IsInternet)
            {
                return false;
            }
            if (uri_Plan == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new PlanRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                id = otherPersonId
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_Plan);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstance.SendAsync(msg, new CancellationTokenSource(6000).Token);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    string json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    PlanResponse res = JsonConvert.DeserializeObject<PlanResponse>(json);
                    if (res != null && AppModel.Instance.Lang.lang != "de")
                    {
                        res.planweek.days.ForEach(day =>
                        {
                            day.ForEach(pm =>
                            {
                                var katNameItem = AppModel.Instance.AllKategorieNames.Find(f => f.titel == pm.katname);
                                if (katNameItem != null)
                                {
                                    pm.katname = String.IsNullOrWhiteSpace(katNameItem.titelLang) ? katNameItem.titel : katNameItem.titelLang;
                                    pm.more.ForEach(more =>
                                    {
                                        var katNameItemM = AppModel.Instance.AllKategorieNames.Find(f => f.titel == more.katname);
                                        if (katNameItemM != null)
                                        {
                                            more.katname = String.IsNullOrWhiteSpace(katNameItemM.titelLang) ? katNameItemM.titel : katNameItemM.titelLang;
                                        }
                                    });
                                }
                            });
                        });
                    }
                    if (res.success)
                    {
                        if (isOtherPerson)
                        {
                            AppModel.Instance.PlanOthePersonResponse = res;
                            AppModel.Instance.PlanOthePersonResponse.lastCall = DateTime.Now;
                        }
                        else
                        {
                            AppModel.Instance.PlanResponse = res;
                            AppModel.Instance.PlanResponse.lastCall = DateTime.Now;
                            ObjektPlanWeekMobile.Save(AppModel.Instance, AppModel.Instance.PlanResponse);
                        }
                    }
                    else
                    {
                        if (isOtherPerson)
                        {
                            AppModel.Instance.PlanOthePersonResponse = new PlanResponse();
                        }
                        else
                        {
                            var resp = ObjektPlanWeekMobile.Load(AppModel.Instance);
                            if (resp == null)
                            {
                                AppModel.Instance.PlanResponse = new PlanResponse();
                            }
                            else
                            {
                                AppModel.Instance.PlanResponse = resp;
                            }
                        }
                    }
                    return true;// res;
                }
                else
                {
                    resMsg?.Dispose();
                    if (isOtherPerson)
                    {
                        AppModel.Instance.PlanOthePersonResponse = new PlanResponse { success = false, message = "Method(GetPlanPersons): resMsg is Null oder Status ist nicht OK!" };
                    }
                    else
                    {
                        AppModel.Instance.PlanResponse = new PlanResponse { success = false, message = "Method(GetPlanPersons): resMsg is Null oder Status ist nicht OK!" };
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    if (isOtherPerson)
                    {
                        AppModel.Instance.PlanOthePersonResponse = new PlanResponse { success = false, message = "Method(GetPlanPersons(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                    }
                    else
                    {
                        AppModel.Instance.PlanResponse = new PlanResponse { success = false, message = "Method(GetPlanPersons(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                    }
                    return false;
                }
                if (isOtherPerson)
                {
                    AppModel.Instance.PlanOthePersonResponse = new PlanResponse { success = false, message = "Method(GetPlanPersons(catch)): " + ex.Message };
                }
                else
                {
                    AppModel.Instance.PlanResponse = new PlanResponse { success = false, message = "Method(GetPlanPersons(catch)): " + ex.Message };
                }
                return false;
            }
        }




        public async Task<ChecksResponse> GetChecksInfo(Int32 pId = 0, int view = 7)
        {
            if (!AppModel.Instance.IsInternet) { return null; }
            if (uri_ChecksInfo == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new ChecksRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                id = pId,
                view = view // 10 = Fällige und Offene 
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_ChecksInfo);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceChecks.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    string json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    var res = JsonConvert.DeserializeObject<ChecksResponse>(json);
                    if (res != null)
                    {
                        if (res.checks != null && res.checks.Count > 0 && AppModel.Instance.Lang.lang != "de")
                        {
                            // Übersetzung !!!
                            // TODO:
                        }
                        return new ChecksResponse { success = true, checks = res.checks, lastCall = DateTimeOffset.Now.DateTime };
                    }
                }

                return null;
            }
            catch (Exception)
            {
                resMsg?.Dispose();
                return null;
            }
        }

        public async Task<Check> GetCheckA(Int32 checkAId = 0)
        {
            if (!AppModel.Instance.IsInternet) { return null; }
            if (uri_GetCheckA == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new CheckRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                id = checkAId
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_GetCheckA);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");
            try
            {
                resMsg = await httpClientInstanceChecks.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    string json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    var res = JsonConvert.DeserializeObject<ChecksResponse>(json);
                    if (res != null)
                    {
                        if (res.checkA != null && AppModel.Instance.Lang.lang != "de")
                        {
                            // Übersetzung !!!
                            // TODO:

                            foreach (var check in AppModel.Instance.ChecksInfoResponse.checks)
                            {
                                if (check.id == checkAId)
                                {
                                    check.checkA_id = res.checkA.id;
                                    check.lastStateOfCheck_a = "Offen";
                                }
                            }
                        }
                        return res.checkA;
                    }
                    else
                    {
                        AppModel.Logger.Warn("WARN: iPM.Mobile Warning (0): GetCheckA res = NULL !");
                    }
                }
                if (resMsg != null) { resMsg.Dispose(); }
                AppModel.Logger.Warn("WARN: iPM.Mobile Warning (0): GetCheckA resMsg.StatusCode = " + (resMsg != null ? resMsg.StatusCode.ToString() : "NULL") + " !");
                return null;
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                AppModel.Logger.Warn("ERROR: iPM.Mobile Error (0): GetCheckA CATCH:" + ex.Message);
                return null;
            }
        }

        public async Task<Check> StartCheck(Int32 checkId = 0)
        {
            if (!AppModel.Instance.IsInternet) { return null; }
            if (uri_StartCheck == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new CheckRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                id = checkId
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_StartCheck);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceChecks.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    string json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    var res = JsonConvert.DeserializeObject<ChecksResponse>(json);
                    if (res != null)
                    {
                        if (res.checkA != null && AppModel.Instance.Lang.lang != "de")
                        {
                            // Übersetzung !!!
                            // TODO:

                            foreach (var check in AppModel.Instance.ChecksInfoResponse.checks)
                            {
                                if (check.id == checkId)
                                {
                                    check.checkA_id = res.checkA.id;
                                    check.lastStateOfCheck_a = "Offen";
                                }
                            }
                        }
                        return res.checkA;
                    }
                    else
                    {
                        AppModel.Logger.Warn("WARN: iPM.Mobile Warning (0): StartCheck res = NULL !");
                    }
                }
                if (resMsg != null) { resMsg.Dispose(); }
                AppModel.Logger.Warn("WARN: iPM.Mobile Warning (0): StartCheck resMsg.StatusCode = " + (resMsg != null ? resMsg.StatusCode.ToString() : "NULL") + " !");
                return null;
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                AppModel.Logger.Warn("ERROR: iPM.Mobile Error (0): StartCheck CATCH:" + ex.Message);
                return null;
            }
        }

        public async Task<bool> DelCheckA(Int32 checkAId = 0)
        {
            if (!AppModel.Instance.IsInternet) { return false; }
            if (uri_DelCheckA == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new CheckRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                id = checkAId
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_DelCheckA);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceChecks.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    string json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    var res = JsonConvert.DeserializeObject<bool>(json);
                    if (res)
                    {
                        return true;
                    }
                    else
                    {
                        AppModel.Logger.Warn("WARN: iPM.Mobile Warning (0): DelCheckA res = NULL !");
                    }
                }
                if (resMsg != null) { resMsg.Dispose(); }
                AppModel.Logger.Warn("WARN: iPM.Mobile Warning (0): DelCheckA resMsg.StatusCode = " + (resMsg != null ? resMsg.StatusCode.ToString() : "NULL") + " !");
                return false;
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                AppModel.Logger.Warn("ERROR: iPM.Mobile Error (0): DelCheckA CATCH:" + ex.Message);
                return false;
            }
        }

        /*
        public async Task<ChecksResponse> SetCheckA(Check check)
        {
            if (!AppModel.Instance.IsInternet) { return null; }
            if (uri_SetCheckA == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new CheckARequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                checkA = check
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_SetCheckA);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceChecks.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    string json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    var responseObj = JsonConvert.DeserializeObject<ChecksResponse>(json);
                    if (responseObj != null && !String.IsNullOrWhiteSpace(responseObj.message))
                    {
                        AppModel.Logger.Warn("WARN: iPM.Mobile Warning (0): SetCheckA response.Message = " + responseObj.message);
                    }
                    return responseObj;
                }
                AppModel.Logger.Warn("WARN: iPM.Mobile Warning (0): SetCheckA resMsg.StatusCode = " + (resMsg != null ? resMsg.StatusCode.ToString() : "NULL") + " !");
                resMsg?.Dispose();
                return null;
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                AppModel.Logger.Warn("ERROR: iPM.Mobile Error (0): SetCheckA CATCH:" + ex.Message);
                return null;
            }
        }
        */

        public async Task<ChecksResponse> SetCheckANonePic(Check check)
        {
            if (!AppModel.Instance.IsInternet) { return null; }
            if (uri_SetCheckANonePics == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new CheckARequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                checkA = check
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_SetCheckANonePics);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceChecks.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    string json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    var responseObj = JsonConvert.DeserializeObject<ChecksResponse>(json);
                    if (responseObj != null && !String.IsNullOrWhiteSpace(responseObj.message))
                    {
                        AppModel.Logger.Warn("WARN: iPM.Mobile Warning (0): SetCheckANonePic response.Message = " + responseObj.message);
                    }
                    return responseObj;
                }
                AppModel.Logger.Warn("WARN: iPM.Mobile Warning (0): SetCheckANonePic resMsg.StatusCode = " + (resMsg != null ? resMsg.StatusCode.ToString() : "NULL") + " !");
                resMsg?.Dispose();
                return null;
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                AppModel.Logger.Warn("ERROR: iPM.Mobile Error (0): SetCheckANonePic CATCH:" + ex.Message);
                return null;
            }
        }

        public async Task<ChecksResponse> SetCheckABemImg(CheckLeistungAntwortBemImg bemImg)
        {
            if (!AppModel.Instance.IsInternet) { return null; }
            if (uri_SetCheckABemImg == null) { InitConnections(); }
            HttpResponseMessage resMsg = null;

            string args = JsonConvert.SerializeObject(new CheckABemImgRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                BemImg = bemImg
            });

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, uri_SetCheckABemImg);
            msg.Content = new StringContent(args, Encoding.UTF8, "application/json");

            try
            {
                resMsg = await httpClientInstanceChecks.SendAsync(msg);
                if (resMsg != null && resMsg.StatusCode == HttpStatusCode.OK)
                {
                    string json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    var responseObj = JsonConvert.DeserializeObject<ChecksResponse>(json);
                    if (responseObj != null && !String.IsNullOrWhiteSpace(responseObj.message))
                    {
                        AppModel.Logger.Warn("WARN: iPM.Mobile Warning (0): SetCheckABemImg response.Message = " + responseObj.message);
                    }
                    return responseObj;
                }
                AppModel.Logger.Warn("WARN: iPM.Mobile Warning (0): SetCheckABemImg resMsg.StatusCode = " + (resMsg != null ? resMsg.StatusCode.ToString() : "NULL") + " !");
                resMsg?.Dispose();
                return null;
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                AppModel.Logger.Warn("ERROR: iPM.Mobile Error (0): SetCheckABemImg CATCH:" + ex.Message);
                return null;
            }
        }






        public async Task<UpdatePushTokenResponse> PNSync(PNWSO pn)
        {
            if (uri_UpdatePushToken == null) { InitPNConnections(); }
            HttpResponseMessage resMsg = null;
            HttpRequestMessage msg = null;
            try
            {
                pn.personid = AppModel.Instance.Person.id;

                string args = JsonConvert.SerializeObject(new UpdatePushTokenRequest
                {
                    token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                    pn = pn
                });

                msg = new HttpRequestMessage(HttpMethod.Post, uri_UpdatePushToken);
                msg.Content = new StringContent(args, Encoding.UTF8, "application/json");
            }
            catch (Exception)
            {
                return new UpdatePushTokenResponse { success = false, message = "Method(PNSync): Initialisierung war noch nicht fertig!" };
            }
            try
            {
                resMsg = await httpClientInstancePNSync.SendAsync(msg, new CancellationTokenSource(6000).Token);
                if (resMsg != null && resMsg.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = resMsg.Content.ReadAsStringAsync().Result;
                    resMsg.Dispose();
                    return JsonConvert.DeserializeObject<UpdatePushTokenResponse>(json);
                }
                else
                {
                    resMsg?.Dispose();
                    return new UpdatePushTokenResponse { success = false, message = "Method(PNSync): resMsg is Null oder Status ist nicht OK!" };
                }
            }
            catch (Exception ex)
            {
                resMsg?.Dispose();
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    return new UpdatePushTokenResponse { success = false, message = "Method(PNSync(canceled)): Der Server ist nicht erreichbar oder die Verbindung wurde unterbrochen!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                return new UpdatePushTokenResponse { success = false, message = "Method(PNSync(catch)): " + ex.Message };
            }
        }






        public async Task<IpmBuildingResponse> IpmFastSyncCount()
        {
            HttpResponseMessage httpResponseMessage = null;
            CookieContainer cookieContainerIpmBuildingSync = new CookieContainer();
            HttpClientHandler httpClientHandlerIpmBuildingSync = HttpClientManager.CreateHandler(cookieContainerIpmBuildingSync, HttpClientManager.TimeoutProfile.Medium);
            HttpClient httpClientIpmBuildingSync = HttpClientManager.CreateClient(httpClientHandlerIpmBuildingSync);
            httpClientIpmBuildingSync.Timeout = new TimeSpan(0, 0, 35);

            AppModel.Instance.FastSyncCount = 0;

            if (!AppModel.Instance.IsInternet)
            {
                return new IpmBuildingResponse
                {
                    success = false,
                    message = "0", // To Int32
                };
            }
            string args = JsonConvert.SerializeObject(new IpmBuildingRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                objids = "",
                lastsync = String.IsNullOrWhiteSpace(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks) ? 0 : long.Parse(AppModel.Instance.SettingModel.SettingDTO.LastBuildingSyncedDateTimeTicks)
            });
            Uri uri = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/FastSyncCount");
            try
            {
                httpResponseMessage = await httpClientIpmBuildingSync.PostAsync(uri, new StringContent(args, Encoding.UTF8, "application/json"));
                if (httpResponseMessage != null && httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    httpResponseMessage.Dispose();
                    return JsonConvert.DeserializeObject<IpmBuildingResponse>(json);
                }
                else
                {
                    if (httpResponseMessage != null) { httpResponseMessage.Dispose(); }
                    return new IpmBuildingResponse { success = false, message = "0" };
                }
            }
            catch (Exception ex)
            {
                if (httpResponseMessage != null) { httpResponseMessage.Dispose(); }
                return new IpmBuildingResponse { success = false, message = "0" };
            }
        }


        public async Task<NoticeResponse> NoticeSync(BemerkungWSO bem)
        {
            HttpResponseMessage httpResponseMessage = null;
            CookieContainer cookieContainerNoticeSync = new CookieContainer();
            HttpClientHandler httpClientHandlerNoticeSync = HttpClientManager.CreateHandler(cookieContainerNoticeSync, HttpClientManager.TimeoutProfile.Long);
            HttpClient httpClientNoticeSync = HttpClientManager.CreateClient(httpClientHandlerNoticeSync);
            httpClientNoticeSync.Timeout = new TimeSpan(0, 0, 120);
            //if (!AppModel.Instance.IsOnline)
            //{
            //    return new NoticeResponse
            //    {
            //        success = false,
            //        message = "Sie brauchen für diese Aktion eine Onlineverbindung!",
            //    };
            //}
            string args = "";
            //var bemerkungen = BemerkungWSO.LoadAll(model);
            args = JsonConvert.SerializeObject(new NoticeRequest
            {
                token = AppModel.Instance.SettingModel.SettingDTO.LoginToken,
                bemerkungen = new List<BemerkungWSO> { bem }
            });

            Uri uri = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/Notice");

            try
            {
                httpResponseMessage = await httpClientNoticeSync.PostAsync(uri, new StringContent(args, Encoding.UTF8, "application/json"));
                if (httpResponseMessage != null && httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    httpResponseMessage.Dispose();
                    return JsonConvert.DeserializeObject<NoticeResponse>(json);
                }
                else
                {
                    var json = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    var m = httpResponseMessage.RequestMessage;
                    if (httpResponseMessage != null) { httpResponseMessage.Dispose(); }
                    return new NoticeResponse { success = false, message = "Method(NoticeSync): " + m };
                }
            }
            catch (Exception ex)
            {
                if (httpResponseMessage != null) { httpResponseMessage.Dispose(); }
                if (ex.Message.ToLower().IndexOf("canceled") > -1)
                {
                    return new NoticeResponse { success = false, message = "Method(NoticeSync(canceled)): Sie sind Online, jedoch ist der Server nicht erreichbar!\n\nBitte versuchen Sie es später noch einmal.\n\nSollte das Problem weiter bestehen, melden Sie sich bei Ihren Sachbearbeitern.\n\n" + ex.Message };
                }
                return new NoticeResponse { success = false, message = "Method(NoticeSync(catch)): " + ex.Message };
            }
        }






        public async void IsReachableHost()
        {
            var connectivity = CrossConnectivity.Current;
            if (connectivity.IsConnected)
            {
                var res = await connectivity.IsRemoteReachable(
                    AppModel.Instance.SettingModel.SettingDTO.ServerUrl
                        .Replace("https://", "")
                        .Replace("http://", "")
                    );
                AppModel.Instance.isReachableServer = res;
                if (res) { AppModel.Instance.lastServerPing = DateTime.Now.Ticks; }
            }
        }

        public async Task<bool> PingServer()
        {
            HttpResponseMessage httpResponseMessage = null;
            CookieContainer cookieContainerPNSync = new CookieContainer();
            HttpClientHandler httpClientHandlerPNSync = HttpClientManager.CreateHandler(cookieContainerPNSync, HttpClientManager.TimeoutProfile.Short);
            HttpClient httpClientPNSync = HttpClientManager.CreateClient(httpClientHandlerPNSync);
            Uri uri = new Uri(AppModel.Instance.SettingModel.SettingDTO.ServerUrl + "/api/UpdatePushToken");
            try
            {
                httpResponseMessage = await httpClientPNSync.PostAsync(
                    uri, new StringContent("", Encoding.UTF8, "application/json"), new CancellationTokenSource(2000).Token);
                if (httpResponseMessage != null && httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> PingIp()
        {
            try
            {
                PingReply rep = await new Ping().SendPingAsync(AppModel.Instance.SettingModel.SettingDTO.ServerUrl.Replace("https://", ""), 2000);
                if (rep.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }











        public void CopyValues<T>(T target, T source)
        {
            Type t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(source, null);
                    var valueOriginal = prop.GetValue(target, null);
                    var ty = valueOriginal != null ? valueOriginal.GetType() : null;
                    if (value != null && ty != null && ty != typeof(string))
                    {
                        prop.SetValue(target, value, null);
                    }
                    else if (value != null && !String.IsNullOrEmpty(value as String) && String.IsNullOrEmpty((valueOriginal as string)))
                    {
                        prop.SetValue(target, value, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }

}
