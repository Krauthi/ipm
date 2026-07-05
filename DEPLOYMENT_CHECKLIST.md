# 🚀 Ticket Chat System - Deployment Checkliste

## ✅ Backend Deployment

### 1. Controller deployen
- [ ] `Backend/Controllers/TicketController.cs` auf Server kopieren
- [ ] Namespace prüfen (`MobileService.Controllers`)
- [ ] Connection-String in `Root` konfigurieren
- [ ] Web.config / Route-Config aktualisieren

### 2. Datenbank-Schema prüfen
- [ ] Tabelle `tickets` existiert mit allen Feldern
- [ ] Tabelle `tickets_chat` existiert mit allen Feldern
- [ ] Indizes auf `ticketid`, `personid`, `besitzerid` prüfen
- [ ] Foreign-Key-Constraints (optional)

**SQL-Check:**
```sql
-- Tickets-Tabelle prüfen
DESC tickets;

-- Tickets_Chat-Tabelle prüfen
DESC tickets_chat;

-- Zähler-Test
SELECT status, COUNT(*) FROM tickets GROUP BY status;
SELECT typ, COUNT(*) FROM tickets_chat GROUP BY typ;
```

### 3. API-Endpunkte testen

**Postman/Swagger-Tests:**

```bash
# Test 1: GetTicketChats
POST https://your-api.com/api/GetTicketChats
Content-Type: application/json
{
  "token": "YOUR_TOKEN",
  "ticketid": 1,
  "personid": 4508
}

# Test 2: AddTicketChatMessage
POST https://your-api.com/api/AddTicketChatMessage
Content-Type: application/json
{
  "token": "YOUR_TOKEN",
  "ticketid": 1,
  "personid": 4508,
  "personname": "Test User",
  "nachricht": "Test Nachricht",
  "typ": "info",
  "intern": true
}

# Test 3: GetUnreadTicketCount
POST https://your-api.com/api/GetUnreadTicketCount
Content-Type: application/json
{
  "token": "YOUR_TOKEN",
  "personid": 4508
}
```

**Erwartete Responses:**
- ✅ `success: true`
- ✅ `chats` enthält Liste von TicketChat-Objekten
- ✅ Keine SQL-Fehler
- ✅ Token-Validierung funktioniert

---

## 📱 Mobile App Deployment

### 1. API-Service integrieren
- [ ] `Maui/Services/TicketApiService.cs` in Projekt kopieren
- [ ] Base-URL in TicketApiService anpassen:
  ```csharp
  private readonly string _baseUrl = "https://YOUR-BACKEND.com/";
  ```
- [ ] HttpClient-Timeout konfigurieren
- [ ] Request/Response-Klassen prüfen

### 2. MainPage.xaml.cs aktualisieren
- [ ] TicketApiService instanziieren
- [ ] Bestehende lokale Methoden durch API-Calls ersetzen:

**Alt (lokal):**
```csharp
private void LoadTicketChat(Ticket ticket)
{
	foreach (var chat in ticket.chats)
	{
		AddChatMessageToUI(chat);
	}
}
```

**Neu (API):**
```csharp
private async void LoadTicketChatFromApi(int ticketId)
{
	var chats = await _ticketApi.GetTicketChatsAsync(ticketId);
	foreach (var chat in chats)
	{
		AddChatMessageToUI(chat);
	}
}
```

### 3. UI-Tests
- [ ] Chat öffnen und laden
- [ ] Nachricht senden
- [ ] Status ändern
- [ ] Als gelesen markieren
- [ ] Error-Handling (kein Netzwerk, ungültiges Token)
- [ ] Loading-Indicatoren
- [ ] Auto-Scroll funktioniert

---

## 🔐 Sicherheit & Konfiguration

### Backend
- [ ] HTTPS aktiviert (SSL-Zertifikat)
- [ ] CORS-Policy konfiguriert (falls nötig)
- [ ] Token-Validierung aktiv
- [ ] SQL-Injection-Prevention (parametrisierte Queries)
- [ ] Rate-Limiting (optional)
- [ ] Logging aktiviert

### Mobile
- [ ] Token wird sicher gespeichert (Secure Storage)
- [ ] HTTPS-Calls erzwingen
- [ ] Token-Refresh-Logik
- [ ] Sensitive Daten nicht in Logs
- [ ] API-Fehler werden abgefangen

---

## 📊 Monitoring & Logging

### Backend
```csharp
// Logging in Controller
Console.WriteLine($"[TicketChat] User {personid} sent message to ticket {ticketid}");

// Error-Logging
try 
{
	// Operation
}
catch (Exception ex)
{
	Logger.Error($"TicketChat Error: {ex.Message}");
	throw;
}
```

### Mobile
```csharp
// API-Call-Logging
Console.WriteLine($"[API] Calling GetTicketChats for ticket {ticketId}");
Console.WriteLine($"[API] Response: {JsonSerializer.Serialize(response)}");
```

**Log-Dateien prüfen:**
- [ ] Backend-Logs auf Fehler prüfen
- [ ] Mobile-App-Logs (Visual Studio Output)
- [ ] SQL-Query-Performance prüfen

---

## 🧪 Test-Szenarien

### Funktionale Tests
- [ ] **Chat laden** - Vollständige Historie lädt
- [ ] **Nachricht senden** - Neue Nachricht erscheint
- [ ] **Status ändern** - Status-Change-Nachricht erscheint
- [ ] **Edit/Delete** - Nachricht kann bearbeitet/gelöscht werden
- [ ] **Intern/Öffentlich** - Toggle funktioniert
- [ ] **Unread-Counter** - Badge zeigt korrekte Anzahl
- [ ] **Als gelesen markieren** - Besitzerstatus wird aktualisiert

### Performance-Tests
- [ ] Großer Chat-Verlauf (100+ Nachrichten)
- [ ] Parallele Requests (mehrere Benutzer)
- [ ] Netzwerk-Latenz (langsame Verbindung)
- [ ] Offline → Online-Übergang

### Edge Cases
- [ ] Kein Netzwerk - Fehlerbehandlung
- [ ] Ungültiges Token - 401 Unauthorized
- [ ] Ticket existiert nicht - 404 Not Found
- [ ] Leere Nachricht senden - Validierung
- [ ] Chat-Liste leer - UI zeigt Platzhalter

---

## 📦 Build & Deployment

### Backend
```bash
# Build
msbuild MobileService.sln /p:Configuration=Release

# Publish
msbuild /t:Publish /p:Configuration=Release /p:PublishDirectory=C:\Publish

# IIS Deploy
Copy-Item C:\Publish\* "C:\inetpub\wwwroot\YourAPI\" -Recurse -Force
```

### Mobile
```bash
# Android
dotnet build -f net10.0-android -c Release

# iOS
dotnet build -f net10.0-ios -c Release

# Publish
dotnet publish -f net10.0-android -c Release -p:AndroidPackageFormat=aab
```

---

## 🎯 Go-Live Checkliste

### Vor dem Go-Live
- [ ] Alle Backend-Tests erfolgreich
- [ ] Alle Mobile-Tests erfolgreich
- [ ] Dokumentation vollständig
- [ ] Logging aktiv
- [ ] Backup-Strategie definiert
- [ ] Rollback-Plan vorhanden

### Am Go-Live-Tag
- [ ] Backend deployen (außerhalb Stoßzeiten)
- [ ] Rauchtest auf Produktion
- [ ] Mobile-App updaten
- [ ] User-Kommunikation (Release Notes)
- [ ] Monitoring aktivieren

### Nach Go-Live
- [ ] Performance-Monitoring (erste 24h)
- [ ] User-Feedback sammeln
- [ ] Logs auf Fehler prüfen
- [ ] Datenbank-Performance prüfen

---

## 🆘 Troubleshooting

### Problem: API-Calls schlagen fehl
**Lösung:**
```csharp
// Debugging in TicketApiService
try
{
	Console.WriteLine($"[DEBUG] Calling: {_baseUrl}{endpoint}");
	Console.WriteLine($"[DEBUG] Request: {json}");

	var response = await _httpClient.PostAsync(...);

	Console.WriteLine($"[DEBUG] Status: {response.StatusCode}");
	Console.WriteLine($"[DEBUG] Response: {await response.Content.ReadAsStringAsync()}");
}
catch (Exception ex)
{
	Console.WriteLine($"[ERROR] {ex.Message}");
}
```

### Problem: Token ungültig
**Lösung:**
- Token-Format prüfen: `"1234567890Name[#]PIN"`
- Token-Validierung im Backend debuggen
- Token-Refresh implementieren

### Problem: Chat lädt nicht
**Lösung:**
```sql
-- Prüfen ob Chats existieren
SELECT * FROM tickets_chat WHERE ticketid = 123;

-- Prüfen ob del != 2
SELECT * FROM tickets_chat WHERE ticketid = 123 AND del < 2;

-- Ticket-Status prüfen
SELECT * FROM tickets WHERE id = 123;
```

### Problem: Nachricht wird nicht gespeichert
**Lösung:**
- SQL-Query in `TicketChat.AddTicketChat` debuggen
- Pflichtfelder prüfen (ticketid, personid, typ, t)
- Encoding-Probleme (UTF-8) prüfen

---

## 📞 Support & Kontakt

Bei Problemen:
1. **Logs prüfen** (Backend + Mobile)
2. **SQL-Datenbank prüfen**
3. **API-Endpunkte mit Postman testen**
4. **Dokumentation konsultieren**

**Dokumentations-Dateien:**
- `README_TICKET_CHAT_SYSTEM.md` - Übersicht
- `Backend/Controllers/TICKET_API_DOCUMENTATION.md` - API-Referenz
- `Maui/Services/TICKET_API_USAGE_EXAMPLES.md` - Code-Beispiele
- `Maui/TICKET_CHAT_README.md` - Mobile-Integration

---

## ✅ Deployment Status

| Komponente | Status | Notizen |
|------------|--------|---------|
| Backend Controller | ⏳ Pending | TicketController.cs deployen |
| API-Endpunkte | ⏳ Pending | Postman-Tests durchführen |
| Mobile Service | ⏳ Pending | TicketApiService.cs integrieren |
| MainPage Integration | ⏳ Pending | API-Calls statt lokale Methoden |
| UI-Tests | ⏳ Pending | Chat öffnen/senden testen |
| Dokumentation | ✅ Done | Alle Docs erstellt |

---

**Letzte Aktualisierung:** Januar 2024  
**Version:** 1.0  
**Status:** Bereit für Deployment
