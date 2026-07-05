# Ticket Chat API Integration - Beispiele

## Verwendung in MainPage.xaml.cs

### 1. Service initialisieren

```csharp
public partial class MainPage : ContentPage
{
	private TicketApiService _ticketApi;
	private Ticket currentTicket;

	public MainPage()
	{
		InitializeComponent();

		// API-Service initialisieren
		_ticketApi = new TicketApiService("https://your-backend-url.com/");
	}
}
```

---

### 2. Chat laden (von API statt lokal)

```csharp
/// <summary>
/// Lädt Chat-Historie vom Server
/// </summary>
private async void LoadTicketChatFromApi(int ticketId)
{
	try
	{
		// Loading-Indicator anzeigen
		// loadingIndicator.IsVisible = true;

		// Chats vom Server laden
		var chats = await _ticketApi.GetTicketChatsAsync(ticketId);

		// UI leeren
		editticket_vscroll.Children.Clear();

		// Chats sortiert anzeigen
		foreach (var chat in chats.OrderBy(c => c.GetDateTime()))
		{
			AddChatMessageToUI(chat);
		}

		// Zum Ende scrollen
		await ScrollToBottom();
	}
	catch (Exception ex)
	{
		await DisplayAlert("Fehler", $"Chat konnte nicht geladen werden: {ex.Message}", "OK");
	}
	finally
	{
		// loadingIndicator.IsVisible = false;
	}
}
```

---

### 3. Nachricht senden (mit API)

```csharp
/// <summary>
/// Sendet eine neue Nachricht an den Server
/// </summary>
private async void OnSendTicketMessage_Clicked(object sender, EventArgs e)
{
	string messageText = ticketMessageEditor.Text?.Trim();

	if (string.IsNullOrEmpty(messageText))
	{
		await DisplayAlert("Hinweis", "Bitte geben Sie eine Nachricht ein", "OK");
		return;
	}

	if (currentTicket == null)
	{
		await DisplayAlert("Fehler", "Kein Ticket ausgewählt", "OK");
		return;
	}

	try
	{
		// Loading-Indicator
		sendButton.IsEnabled = false;

		// Nachricht an Server senden
		var response = await _ticketApi.SendMessageAsync(
			ticketId: currentTicket.id,
			message: messageText,
			typ: "info",
			intern: true  // oder false für öffentliche Nachricht
		);

		if (response.success)
		{
			// Editor leeren
			ticketMessageEditor.Text = string.Empty;

			// Chat neu laden mit aktualisierten Daten
			if (response.chats != null && response.chats.Count > 0)
			{
				// UI leeren
				editticket_vscroll.Children.Clear();

				// Aktualisierte Chat-Liste anzeigen
				foreach (var chat in response.chats.OrderBy(c => c.GetDateTime()))
				{
					AddChatMessageToUI(chat);
				}

				await ScrollToBottom();
			}

			// Tastatur verstecken
#if ANDROID
			if (Platform.CurrentActivity != null)
			{
				var inputMethodManager = (Android.Views.InputMethods.InputMethodManager)
					Platform.CurrentActivity.GetSystemService(Android.Content.Context.InputMethodService);
				var token = Platform.CurrentActivity.CurrentFocus?.WindowToken;
				inputMethodManager?.HideSoftInputFromWindow(token, 0);
			}
#endif
		}
		else
		{
			await DisplayAlert("Fehler", response.message ?? "Nachricht konnte nicht gesendet werden", "OK");
		}
	}
	catch (Exception ex)
	{
		await DisplayAlert("Fehler", $"Fehler beim Senden: {ex.Message}", "OK");
	}
	finally
	{
		sendButton.IsEnabled = true;
	}
}
```

---

### 4. Ticket öffnen (mit API-Integration)

```csharp
/// <summary>
/// Öffnet ein Ticket und lädt Chat-Historie vom Server
/// </summary>
public async void OpenTicketChatFromApi(int ticketId)
{
	try
	{
		// Vollständiges Ticket vom Server laden (inkl. Chats)
		currentTicket = await _ticketApi.GetTicketAsync(ticketId);

		if (currentTicket == null)
		{
			await DisplayAlert("Fehler", "Ticket nicht gefunden", "OK");
			return;
		}

		// Ticket als gelesen markieren
		await _ticketApi.MarkAsReadAsync(ticketId);

		// Besitzer-Status aktualisieren (wenn ich der Besitzer bin)
		if (currentTicket.besitzerid == AppModel.Instance?.Person?.id)
		{
			await _ticketApi.UpdateBesitzerStatusAsync(ticketId, BesitzerStatus.Gesehen);
		}

		// Chat-Historie anzeigen
		if (currentTicket.chats != null && currentTicket.chats.Count > 0)
		{
			editticket_vscroll.Children.Clear();

			foreach (var chat in currentTicket.chats.OrderBy(c => c.GetDateTime()))
			{
				AddChatMessageToUI(chat);
			}

			await ScrollToBottom();
		}

		// UI-Elemente aktualisieren
		UpdateTicketUI();
	}
	catch (Exception ex)
	{
		await DisplayAlert("Fehler", $"Ticket konnte nicht geöffnet werden: {ex.Message}", "OK");
	}
}

/// <summary>
/// Aktualisiert Ticket-UI-Elemente
/// </summary>
private void UpdateTicketUI()
{
	// Beispiel: Titel und Status anzeigen
	// ticketTitleLabel.Text = currentTicket.titel;
	// ticketStatusLabel.Text = currentTicket.GetStatusText();
	// ticketPrioLabel.Text = $"Priorität: {currentTicket.prio}";
}
```

---

### 5. Status ändern

```csharp
/// <summary>
/// Ändert den Ticket-Status
/// </summary>
private async void OnChangeStatusClicked(object sender, EventArgs e)
{
	if (currentTicket == null) return;

	// Status-Auswahl anzeigen
	var action = await DisplayActionSheet(
		"Status ändern",
		"Abbrechen",
		null,
		"Neu", "Offen", "In Arbeit", "Rückfrage", "Erledigt", "Geschlossen"
	);

	TicketStatus newStatus = action switch
	{
		"Neu" => TicketStatus.Neu,
		"Offen" => TicketStatus.Offen,
		"In Arbeit" => TicketStatus.InArbeit,
		"Rückfrage" => TicketStatus.Rueckfrage,
		"Erledigt" => TicketStatus.Erledigt,
		"Geschlossen" => TicketStatus.Geschlossen,
		_ => (TicketStatus)currentTicket.status
	};

	if ((int)newStatus != currentTicket.status)
	{
		bool success = await _ticketApi.UpdateTicketStatusAsync(currentTicket.id, newStatus);

		if (success)
		{
			currentTicket.status = (int)newStatus;

			// Chat neu laden (zeigt Status-Change-Nachricht)
			LoadTicketChatFromApi(currentTicket.id);

			await DisplayAlert("Erfolg", $"Status geändert zu: {newStatus}", "OK");
		}
		else
		{
			await DisplayAlert("Fehler", "Status konnte nicht geändert werden", "OK");
		}
	}
}
```

---

### 6. Ungelesene Tickets Badge

```csharp
/// <summary>
/// Aktualisiert die Badge-Anzeige für ungelesene Tickets
/// </summary>
private async Task UpdateUnreadBadgeAsync()
{
	try
	{
		int unreadCount = await _ticketApi.GetUnreadCountAsync();

		// Badge-Label aktualisieren
		if (unreadCount > 0)
		{
			// unreadBadge.IsVisible = true;
			// unreadBadge.Text = unreadCount > 99 ? "99+" : unreadCount.ToString();
		}
		else
		{
			// unreadBadge.IsVisible = false;
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Error updating badge: {ex.Message}");
	}
}

/// <summary>
/// Periodisch ungelesene Tickets prüfen
/// </summary>
private async void StartUnreadPolling()
{
	while (true)
	{
		await UpdateUnreadBadgeAsync();
		await Task.Delay(TimeSpan.FromMinutes(1)); // Alle 1 Minute prüfen
	}
}
```

---

### 7. Nachricht bearbeiten/löschen (Long-Press)

```csharp
/// <summary>
/// Context-Menu für Chat-Nachrichten
/// </summary>
private async void OnChatMessageLongPress(TicketChat chat)
{
	// Nur eigene Nachrichten bearbeitbar
	var currentUserId = AppModel.Instance?.Person?.id ?? 0;

	if (chat.personid != currentUserId)
		return;

	var action = await DisplayActionSheet(
		"Nachricht",
		"Abbrechen",
		"Löschen",
		"Bearbeiten",
		"Intern/Öffentlich umschalten"
	);

	switch (action)
	{
		case "Bearbeiten":
			await EditMessageAsync(chat);
			break;

		case "Löschen":
			await DeleteMessageAsync(chat);
			break;

		case "Intern/Öffentlich umschalten":
			await ToggleInternAsync(chat);
			break;
	}
}

private async Task EditMessageAsync(TicketChat chat)
{
	string newText = await DisplayPromptAsync(
		"Nachricht bearbeiten",
		"Neuer Text:",
		initialValue: chat.t
	);

	if (!string.IsNullOrEmpty(newText) && newText != chat.t)
	{
		bool success = await _ticketApi.UpdateMessageAsync(
			currentTicket.id,
			chat.id,
			newText,
			chat.intern
		);

		if (success)
		{
			LoadTicketChatFromApi(currentTicket.id);
		}
	}
}

private async Task DeleteMessageAsync(TicketChat chat)
{
	bool confirm = await DisplayAlert(
		"Löschen",
		"Nachricht wirklich löschen?",
		"Ja",
		"Nein"
	);

	if (confirm)
	{
		bool success = await _ticketApi.DeleteMessageAsync(currentTicket.id, chat.id);

		if (success)
		{
			LoadTicketChatFromApi(currentTicket.id);
		}
	}
}

private async Task ToggleInternAsync(TicketChat chat)
{
	bool success = await _ticketApi.ToggleInternStatusAsync(currentTicket.id, chat.id);

	if (success)
	{
		LoadTicketChatFromApi(currentTicket.id);
	}
}
```

---

### 8. Pull-to-Refresh

```csharp
/// <summary>
/// Refresh-Handler für SwipeRefreshView
/// </summary>
private async void OnRefreshTicketChat(object sender, EventArgs e)
{
	if (currentTicket == null)
		return;

	try
	{
		// Chat neu vom Server laden
		LoadTicketChatFromApi(currentTicket.id);
	}
	finally
	{
		// refreshView.IsRefreshing = false;
	}
}
```

**XAML für Pull-to-Refresh:**
```xml
<RefreshView x:Name="refreshView" 
			 Refreshing="OnRefreshTicketChat"
			 RefreshColor="#25D366">
	<ScrollView x:Name="chatScrollView" BackgroundColor="#ECE5DD">
		<VerticalStackLayout x:Name="editticket_vscroll" Padding="10" />
	</ScrollView>
</RefreshView>
```

---

### 9. Offline-Support (Optional)

```csharp
/// <summary>
/// Speichert Nachricht in Queue, wenn offline
/// </summary>
private async Task<bool> SendMessageWithOfflineSupport(string message)
{
	if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
	{
		// Online - direkt senden
		var response = await _ticketApi.SendMessageAsync(currentTicket.id, message);
		return response.success;
	}
	else
	{
		// Offline - in Queue speichern
		await SaveToOfflineQueue(currentTicket.id, message);
		await DisplayAlert("Offline", "Nachricht wird gesendet, sobald Sie online sind", "OK");
		return true;
	}
}

private async Task SaveToOfflineQueue(int ticketId, string message)
{
	// In lokaler Datenbank speichern
	// await Database.SavePendingMessageAsync(ticketId, message);
}

// Später beim Verbindungsaufbau:
private async Task ProcessOfflineQueue()
{
	// var pendingMessages = await Database.GetPendingMessagesAsync();
	// foreach (var msg in pendingMessages)
	// {
	//     await _ticketApi.SendMessageAsync(msg.TicketId, msg.Message);
	//     await Database.DeletePendingMessageAsync(msg.Id);
	// }
}
```

---

## Vollständiges Beispiel: Integration in MainPage

```csharp
public partial class MainPage : ContentPage
{
	private TicketApiService _ticketApi;
	private Ticket currentTicket;

	public MainPage()
	{
		InitializeComponent();
		_ticketApi = new TicketApiService("https://your-api.com/");

		// Unread-Badge starten
		_ = StartUnreadPollingAsync();
	}

	// Ticket öffnen
	public async void OpenTicket(int ticketId)
	{
		await OpenTicketChatFromApi(ticketId);
	}

	// Nachricht senden
	private async void OnSendTicketMessage_Clicked(object sender, EventArgs e)
	{
		string messageText = ticketMessageEditor.Text?.Trim();
		if (string.IsNullOrEmpty(messageText) || currentTicket == null)
			return;

		var response = await _ticketApi.SendMessageAsync(currentTicket.id, messageText);

		if (response.success)
		{
			ticketMessageEditor.Text = "";
			editticket_vscroll.Children.Clear();

			foreach (var chat in response.chats.OrderBy(c => c.GetDateTime()))
			{
				AddChatMessageToUI(chat);
			}

			await ScrollToBottom();
		}
	}

	// Status ändern
	private async void OnChangeStatus(TicketStatus newStatus)
	{
		if (currentTicket == null) return;

		bool success = await _ticketApi.UpdateTicketStatusAsync(currentTicket.id, newStatus);
		if (success)
		{
			LoadTicketChatFromApi(currentTicket.id);
		}
	}

	// Unread-Badge aktualisieren
	private async Task StartUnreadPollingAsync()
	{
		while (true)
		{
			int count = await _ticketApi.GetUnreadCountAsync();
			// UI aktualisieren
			await Task.Delay(TimeSpan.FromMinutes(1));
		}
	}
}
```

---

## Best Practices

1. ✅ **Immer Error-Handling** - try/catch um alle API-Calls
2. ✅ **Loading-Indicatoren** - Feedback während Netzwerk-Operationen
3. ✅ **Offline-Support** - Connectivity.Current.NetworkAccess prüfen
4. ✅ **Token-Refresh** - Token regelmäßig erneuern
5. ✅ **Caching** - Häufig genutzte Daten lokal zwischenspeichern
6. ✅ **Pagination** - Bei langen Chat-Verläufen nur X Nachrichten laden
7. ✅ **Pull-to-Refresh** - Benutzer kann Chat manuell aktualisieren
8. ✅ **Real-time Updates** - Optional: SignalR für Live-Updates

---

## Debugging

```csharp
// API-Calls loggen
private async Task<T> PostWithLoggingAsync<T>(string endpoint, object request)
{
	Console.WriteLine($"[API] Calling: {endpoint}");
	Console.WriteLine($"[API] Request: {JsonSerializer.Serialize(request)}");

	var response = await _ticketApi.PostAsync<T>(endpoint, request);

	Console.WriteLine($"[API] Response: {JsonSerializer.Serialize(response)}");
	return response;
}
```
