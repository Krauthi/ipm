# Ticket Chat API - Dokumentation

## Übersicht
Diese Dokumentation beschreibt die erweiterten API-Endpunkte für das Ticket-Chat-System.

---

## 🆕 Neue Chat-Endpunkte

### 1. **GetTicketChats** - Chat-Historie laden
Lädt die komplette Chat-Historie eines Tickets.

**Endpoint:** `POST ~/api/GetTicketChats`

**Request:**
```json
{
  "token": "1234567890AchimBlum[#]16101964",
  "ticketid": 123,
  "personid": 4508
}
```

**Response:**
```json
{
  "success": true,
  "message": "",
  "chats": [
	{
	  "id": 1,
	  "ticketid": 123,
	  "personid": 4508,
	  "personname": "Achim Blum",
	  "typ": "new",
	  "t": "Ticket wurde erstellt",
	  "info": "Erstellt am 15.01.2024 - 10:30",
	  "updateat": "2024-01-15 10:30:00",
	  "intern": true,
	  "del": 0
	},
	{
	  "id": 2,
	  "ticketid": 123,
	  "personid": 4508,
	  "personname": "Achim Blum",
	  "typ": "info",
	  "t": "Problem wurde behoben",
	  "info": "15.01.2024 - 14:45 : Problem wurde behoben - (Achim Blum)",
	  "updateat": "2024-01-15 14:45:00",
	  "intern": false,
	  "del": 0
	}
  ]
}
```

**C# Beispiel (Mobile):**
```csharp
public async Task<List<TicketChat>> LoadTicketChatsAsync(int ticketId)
{
	var request = new TicketChatRequest
	{
		token = AppModel.Instance.Token,
		ticketid = ticketId,
		personid = AppModel.Instance.Person.id
	};

	var response = await apiClient.PostAsync<TicketChatResponse>(
		"api/GetTicketChats", 
		request
	);

	return response.success ? response.chats : new List<TicketChat>();
}
```

---

### 2. **AddTicketChatMessage** - Neue Nachricht hinzufügen
Fügt eine neue Chat-Nachricht zu einem Ticket hinzu.

**Endpoint:** `POST ~/api/AddTicketChatMessage`

**Request:**
```json
{
  "token": "1234567890AchimBlum[#]16101964",
  "ticketid": 123,
  "personid": 4508,
  "personname": "Achim Blum",
  "nachricht": "Das Problem wurde behoben",
  "typ": "info",
  "intern": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "Nachricht erfolgreich hinzugefügt",
  "chats": [
	// Aktualisierte Chat-Liste
  ]
}
```

**C# Beispiel (Mobile):**
```csharp
public async Task<bool> SendTicketMessageAsync(int ticketId, string message, bool intern = true)
{
	var request = new TicketChatRequest
	{
		token = AppModel.Instance.Token,
		ticketid = ticketId,
		personid = AppModel.Instance.Person.id,
		personname = AppModel.Instance.Person.GetFullName(),
		nachricht = message,
		typ = "info",
		intern = intern
	};

	var response = await apiClient.PostAsync<TicketChatResponse>(
		"api/AddTicketChatMessage", 
		request
	);

	if (response.success && response.chats != null)
	{
		// UI aktualisieren mit response.chats
		return true;
	}

	return false;
}
```

**Message Typen (typ):**
- `"new"` - Ticket wurde erstellt
- `"info"` - Allgemeine Info-Nachricht ✅ Standard
- `"mailto"` - E-Mail wurde gesendet
- `"notificationto"` - Benachrichtigung
- `"statuschange"` - Status wurde geändert
- `"besitzerchange"` - Besitzer wurde geändert
- `"rueckfrage"` - Rückfrage
- `"erledigt"` - Erledigt
- `"geschlossen"` - Geschlossen

---

### 3. **UpdateTicketChatMessage** - Nachricht bearbeiten
Aktualisiert eine existierende Chat-Nachricht.

**Endpoint:** `POST ~/api/UpdateTicketChatMessage`

**Request:**
```json
{
  "token": "1234567890AchimBlum[#]16101964",
  "ticketid": 123,
  "chatid": 5,
  "nachricht": "Korrigierter Text",
  "intern": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "Nachricht erfolgreich aktualisiert",
  "chats": [
	// Aktualisierte Chat-Liste
  ]
}
```

---

### 4. **DeleteTicketChatMessage** - Nachricht löschen
Löscht eine Chat-Nachricht (setzt `del = 2`).

**Endpoint:** `POST ~/api/DeleteTicketChatMessage`

**Request:**
```json
{
  "token": "1234567890AchimBlum[#]16101964",
  "ticketid": 123,
  "chatid": 5
}
```

**Response:**
```json
{
  "success": true,
  "message": "Nachricht erfolgreich gelöscht",
  "chats": [
	// Aktualisierte Chat-Liste ohne gelöschte Nachricht
  ]
}
```

---

### 5. **ToggleTicketChatIntern** - Intern-Status ändern
Ändert den `intern`-Status einer Nachricht (öffentlich ↔ intern).

**Endpoint:** `POST ~/api/ToggleTicketChatIntern`

**Request:**
```json
{
  "token": "1234567890AchimBlum[#]16101964",
  "ticketid": 123,
  "chatid": 5
}
```

**Response:**
```json
{
  "success": true,
  "message": "Intern-Status erfolgreich geändert",
  "chats": [
	// Aktualisierte Chat-Liste
  ]
}
```

---

### 6. **UpdateTicketStatus** - Ticket-Status ändern
Ändert den Status eines Tickets und fügt optional einen Chat-Eintrag hinzu.

**Endpoint:** `POST ~/api/UpdateTicketStatus`

**Request:**
```json
{
  "token": "1234567890AchimBlum[#]16101964",
  "ticketid": 123,
  "status": 4,
  "intern": true
}
```

**Status-Werte:**
- `1` = Neu
- `2` = Offen
- `3` = Wartend
- `4` = In Arbeit
- `5` = Rückfrage
- `9` = Erledigt
- `10` = Geschlossen

**Response:**
```json
{
  "success": true,
  "message": "Status erfolgreich aktualisiert",
  "ticket": {
	// Aktualisiertes Ticket-Objekt
  }
}
```

---

### 7. **UpdateTicketBesitzerStatus** - Besitzer-Status ändern
Aktualisiert den Besitzer-Status eines Tickets.

**Endpoint:** `POST ~/api/UpdateTicketBesitzerStatus`

**Request:**
```json
{
  "token": "1234567890AchimBlum[#]16101964",
  "ticketid": 123,
  "besitzerstatus": 0
}
```

**Besitzerstatus-Werte:**
- `-1` = Noch nicht gesehen
- `0` = Gesehen/Geöffnet
- `1` = Gestartet/In Arbeit
- `2` = Rückfrage
- `9` = Erledigt

**Response:**
```json
{
  "success": true,
  "message": "Besitzer-Status erfolgreich aktualisiert",
  "ticket": {
	// Aktualisiertes Ticket-Objekt
  }
}
```

---

### 8. **MarkTicketChatsAsRead** - Als gelesen markieren
Markiert alle Ticket-Chats als gelesen für einen Benutzer.

**Endpoint:** `POST ~/api/MarkTicketChatsAsRead`

**Request:**
```json
{
  "token": "1234567890AchimBlum[#]16101964",
  "ticketid": 123,
  "personid": 4508
}
```

**Response:**
```json
{
  "success": true,
  "message": "Ticket als gelesen markiert"
}
```

**C# Beispiel:**
```csharp
public async Task MarkTicketAsReadAsync(int ticketId)
{
	var request = new TicketChatRequest
	{
		token = AppModel.Instance.Token,
		ticketid = ticketId,
		personid = AppModel.Instance.Person.id
	};

	await apiClient.PostAsync<TicketChatResponse>(
		"api/MarkTicketChatsAsRead", 
		request
	);
}
```

---

### 9. **GetUnreadTicketCount** - Anzahl ungelesener Tickets
Lädt die Anzahl ungelesener Tickets für einen Benutzer.

**Endpoint:** `POST ~/api/GetUnreadTicketCount`

**Request:**
```json
{
  "token": "1234567890AchimBlum[#]16101964",
  "personid": 4508
}
```

**Response:**
```json
{
  "success": true,
  "unreadCount": 3
}
```

**C# Beispiel:**
```csharp
public async Task<int> GetUnreadTicketCountAsync()
{
	var request = new TicketRequest
	{
		token = AppModel.Instance.Token,
		personid = AppModel.Instance.Person.id
	};

	var response = await apiClient.PostAsync<TicketResponse>(
		"api/GetUnreadTicketCount", 
		request
	);

	return response.success ? response.unreadCount : 0;
}
```

---

## ✅ Erweiterte bestehende Endpunkte

### **GetTicket** - Vollständiges Ticket laden (mit Chats)
Der bestehende Endpunkt wurde erweitert und lädt jetzt automatisch die Chat-Historie.

**Endpoint:** `POST ~/api/GetTicket`

**Request:**
```json
{
  "token": "1234567890AchimBlum[#]16101964",
  "ticketid": 123
}
```

**Response:**
```json
{
  "success": true,
  "ticket": {
	"id": 123,
	"titel": "Support-Anfrage",
	"text": "Beschreibung...",
	"status": 2,
	"besitzerstatus": 0,
	"chats": [
	  // Chat-Historie
	],
	// ... weitere Felder
  }
}
```

---

## 📱 Mobile Integration

### API-Service-Klasse (Empfohlen)
```csharp
public class TicketApiService
{
	private readonly string _baseUrl = "https://your-api.com/";
	private readonly HttpClient _httpClient;

	public TicketApiService()
	{
		_httpClient = new HttpClient();
	}

	public async Task<TicketChatResponse> GetTicketChatsAsync(int ticketId)
	{
		var request = new TicketChatRequest
		{
			token = AppModel.Instance.Token,
			ticketid = ticketId,
			personid = AppModel.Instance.Person.id
		};

		var json = JsonSerializer.Serialize(request);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await _httpClient.PostAsync($"{_baseUrl}api/GetTicketChats", content);
		var responseJson = await response.Content.ReadAsStringAsync();

		return JsonSerializer.Deserialize<TicketChatResponse>(responseJson);
	}

	public async Task<bool> SendMessageAsync(int ticketId, string message, bool intern = true)
	{
		var request = new TicketChatRequest
		{
			token = AppModel.Instance.Token,
			ticketid = ticketId,
			personid = AppModel.Instance.Person.id,
			personname = AppModel.Instance.Person.GetFullName(),
			nachricht = message,
			typ = "info",
			intern = intern
		};

		var json = JsonSerializer.Serialize(request);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await _httpClient.PostAsync($"{_baseUrl}api/AddTicketChatMessage", content);
		var responseJson = await response.Content.ReadAsStringAsync();
		var result = JsonSerializer.Deserialize<TicketChatResponse>(responseJson);

		return result.success;
	}

	public async Task<bool> UpdateStatusAsync(int ticketId, int status)
	{
		var request = new TicketStatusRequest
		{
			token = AppModel.Instance.Token,
			ticketid = ticketId,
			status = status,
			intern = true
		};

		var json = JsonSerializer.Serialize(request);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await _httpClient.PostAsync($"{_baseUrl}api/UpdateTicketStatus", content);
		var responseJson = await response.Content.ReadAsStringAsync();
		var result = JsonSerializer.Deserialize<TicketResponse>(responseJson);

		return result.success;
	}
}
```

### Verwendung in MainPage.xaml.cs
```csharp
private TicketApiService _ticketApi = new TicketApiService();

// Chat laden
private async void LoadTicketChatFromApi(int ticketId)
{
	var response = await _ticketApi.GetTicketChatsAsync(ticketId);

	if (response.success && response.chats != null)
	{
		foreach (var chat in response.chats.OrderBy(c => c.GetDateTime()))
		{
			AddChatMessageToUI(chat);
		}

		await ScrollToBottom();
	}
}

// Nachricht senden
private async void OnSendTicketMessage_Clicked(object sender, EventArgs e)
{
	string messageText = ticketMessageEditor.Text?.Trim();

	if (string.IsNullOrEmpty(messageText))
		return;

	bool success = await _ticketApi.SendMessageAsync(
		currentTicket.id, 
		messageText, 
		intern: true
	);

	if (success)
	{
		ticketMessageEditor.Text = string.Empty;

		// Chat neu laden
		LoadTicketChatFromApi(currentTicket.id);
	}
	else
	{
		await DisplayAlert("Fehler", "Nachricht konnte nicht gesendet werden", "OK");
	}
}
```

---

## 🔐 Sicherheit

### Token-Validierung
Alle Endpunkte prüfen die Authentifizierung über:
```csharp
if (Utils.CheckPersonCredential(_root._connectionString, value.token))
{
	// Authentifizierung erfolgreich
}
```

### Best Practices
1. **Token immer mitschicken** - Jeder Request benötigt ein gültiges Token
2. **HTTPS verwenden** - Alle API-Calls sollten verschlüsselt sein
3. **Token-Refresh** - Token regelmäßig erneuern
4. **Error-Handling** - Immer `success`-Flag prüfen

---

## 📊 Fehlerbehandlung

### Standard-Fehler-Response
```json
{
  "success": false,
  "message": "Authentifizierung fehlgeschlagen"
}
```

### Mögliche Fehlermeldungen
- `"Authentifizierung fehlgeschlagen"` - Token ungültig
- `"Ticket nicht gefunden"` - Ticket-ID existiert nicht
- `"Fehler beim Hinzufügen der Nachricht"` - Datenbank-Fehler
- `"Chat-Nachricht nicht gefunden"` - Chat-ID ungültig

### Error-Handling in C#
```csharp
try
{
	var response = await _ticketApi.SendMessageAsync(ticketId, message);

	if (!response)
	{
		await DisplayAlert("Fehler", "Nachricht konnte nicht gesendet werden", "OK");
	}
}
catch (HttpRequestException ex)
{
	await DisplayAlert("Netzwerkfehler", "Keine Verbindung zum Server", "OK");
}
catch (Exception ex)
{
	await DisplayAlert("Fehler", $"Unerwarteter Fehler: {ex.Message}", "OK");
}
```

---

## 🚀 Performance-Tipps

1. **Chat-Pagination**: Bei sehr langen Chat-Verläufen nur die neuesten N Nachrichten laden
2. **Caching**: Bereits geladene Chats lokal zwischenspeichern
3. **Delta-Updates**: Nur neue Nachrichten seit dem letzten Abruf laden
4. **Batch-Operations**: Multiple Status-Updates zusammenfassen

---

## 📝 Changelog

### Version 1.0 (Januar 2024)
- ✅ GetTicketChats - Chat-Historie laden
- ✅ AddTicketChatMessage - Neue Nachricht
- ✅ UpdateTicketChatMessage - Nachricht bearbeiten
- ✅ DeleteTicketChatMessage - Nachricht löschen
- ✅ ToggleTicketChatIntern - Intern-Status ändern
- ✅ UpdateTicketStatus - Status ändern
- ✅ UpdateTicketBesitzerStatus - Besitzer-Status
- ✅ MarkTicketChatsAsRead - Als gelesen markieren
- ✅ GetUnreadTicketCount - Anzahl ungelesener Tickets
