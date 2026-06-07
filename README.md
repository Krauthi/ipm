# ipm

## Mobile push notifications (.NET MAUI)

The MAUI app (`iPMCloud.Mobile.Maui/`) is wired for Firebase Cloud Messaging (FCM) on Android and iOS.

### Implemented client behavior
- Android:
  - FCM `FirebaseMessagingService` receives token and messages.
  - Notification channel `ipmcloud_message_channel` is created with high importance.
  - Foreground messages are explicitly shown as system notifications (heads-up capable), so users still see a first-level notification while the app is open.
  - Android 13+ `POST_NOTIFICATIONS` runtime permission is requested during push initialization.
- iOS:
  - Firebase is configured at startup.
  - APNs authorization for alert/badge/sound is requested.
  - `WillPresentNotification` returns banner/list/sound/badge so notifications are visibly presented even when the app is foregrounded.
  - FCM registration token updates are captured and queued.
- Token handling:
  - Device token refresh is written into the existing `PNWSO` upload stack.
  - Existing upload flow (`PNSync`) remains responsible for backend registration.

### Required setup (manual)
1. Firebase project:
   - Enable Cloud Messaging.
   - Keep `Platforms/Android/google-services.json` and `Platforms/iOS/GoogleService-Info.plist` aligned with the app IDs.
2. Android:
   - Ensure Firebase Android app package matches `com.ipmcloud.ipm.mobile`.
3. iOS (Apple Developer):
   - Enable Push Notifications capability for the iOS bundle ID.
   - Upload APNs key/certificate to Firebase Cloud Messaging (Apple app configuration).
   - Ensure provisioning profile includes push entitlement (`aps-environment`).
4. Backend:
   - Continue sending tokens to the existing `UpdatePushToken` endpoint via `PNSync`.
   - For foreground-visible behavior, include notification title/body (or equivalent data keys `title`/`body`) in sent payloads.
