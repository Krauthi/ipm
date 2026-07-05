# Ticket Chat - WhatsApp-ähnliche Oberfläche (Backend-kompatibel)

## Übersicht
Die Ticket-Chat-Funktion wurde erfolgreich an die Backend-Struktur angepasst und bietet eine WhatsApp-ähnliche Chat-Oberfläche für Ticket-Verläufe.

## Wichtige Änderungen (Backend-Kompatibilität)

Die Mobile-App verwendet jetzt die gleiche Datenstruktur wie das Backend:

### **TicketChat** statt TicketMessage
```csharp
public class TicketChat
{
	public Int32 id { get; set; }
	public Int32 ticketid { get; set; }
	public Int32 personid { get; set; }
	public string personname { get; set; }
	public string typ { get; set; }  // 'new', 'info', 'mailto', 'statuschange', etc.
	public string t { get; set; }    // Text der Nachricht
	public string info { get; set; }
	public string updateat { get; set; } // "yyyy-MM-dd HH:mm:ss"
	public bool intern { get; set; }
	public int del { get; set; }
}
```

### **Ticket-Klasse** angepasst
```csharp
public class Ticket
{
	// IDs (Int32 für Backend-Kompatibilität)
	public Int32 id { get; set; }
	public Int32 gruppeid { get; set; }
	public Int32 personid { get; set; }
	public Int32 besitzerid { get; set; }
	public Int32 erstellerid { get; set; }
	public Int32 aspid { get; set; }
	public Int32 objektid { get; set; }
	public Int32 auftragid { get; set; }

	// Texte
	public string text { get; set; }      // statt "beschreibung"
	public string titel { get; set; }

	// Datumswerte (als Strings)
	public string start { get; set; }     // Unix-Timestamp
	public string end { get; set; }
	public string startab { get; set; }
	public string endbis { get; set; }
	public string updateat { get; set; }  // "yyyy-MM-dd HH:mm:ss"

	// Status
	public int status { get; set; }       // 1=Neu, 2=Offen, 4=InArbeit, etc.
	public int besitzerstatus { get; set; } // -1=Nicht gesehen, 0=Gesehen, etc.
	public int del { get; set; }          // Löschen-Flag
	public bool intern { get; set; }
	public int prio { get; set; }

	// Personen und Objekte
	public TicketPerson kunde { get; set; }
	public TicketPerson besitzer { get; set; }
	public TicketPerson ersteller { get; set; }
	public TicketPerson asp { get; set; }
	public TicketObjekt objekt { get; set; }

	// Chat-Verlauf
	public List<TicketChat> chats { get; set; }
	public TicketChat newchat { get; set; }

	// Legacy-Properties für Kompatibilität
	[JsonIgnore]
	public DateTime? datum { get; set; }  // Konvertiert start
	[JsonIgnore]
	public string beschreibung { get; set; } // Alias für text
}
```

### **Status-Enums (Backend-kompatibel)**
```csharp
public enum TicketStatus
{
	Neu = 1,
	Offen = 2,
	Wartend = 3,
	InArbeit = 4,
	Rueckfrage = 5,
	Erledigt = 9,
	Geschlossen = 10
}

public enum BesitzerStatus
{
	NochNichtGesehen = -1,
	Gesehen = 0,
	Gestartet = 1,
	Rueckfrage = 2,
	Erledigt = 9
}
```

## Verwendung

### Ticket Chat öffnen
```csharp
// In MainPage.xaml.cs oder einem Event-Handler
void OpenTicketChat(int ticketId)
{
	var ticket = Ticket.Load(ticketId);
	if (ticket != null)
	{
		// Container sichtbar machen
		// editticket_Container.IsVisible = true;

		// Chat laden
		LoadTicketChat(ticket);
	}
}
```

### Neue Nachricht hinzufügen
```csharp
// Neue Methode (Backend-kompatibel)
ticket.AddChatMessage(
	personid: currentUserId,
	personname: currentUserName,
	nachricht: "Nachrichtentext",
	typ: "info",      // oder 'new', 'statuschange', etc.
	intern: true       // sichtbar nur intern
);

// Legacy-Methode (weiterhin verfügbar)
ticket.AddMessage(absenderId, absenderName, "Nachricht");
```

### Test-Ticket erstellen
```csharp
// Rufe diese Methode auf für ein Test-Ticket
CreateTestTicketExample();
```

## Chat-Nachricht Typen

Die `typ`-Property in TicketChat unterstützt folgende Werte (wie im Backend):

- **'new'** - Ticket wurde erstellt
- **'info'** - Allgemeine Info-Nachricht
- **'mailto'** - E-Mail wurde gesendet
- **'notificationto'** - Benachrichtigung wurde gesendet
- **'statuschange'** - Status wurde geändert
- **'besitzerchange'** - Besitzer wurde geändert
- **'rueckfrage'** - Rückfrage
- **'erledigt'** - Ticket ist erledigt
- **'geschlossen'** - Ticket wurde geschlossen

## UI-Features (unverändert)

✅ Chat-Bubbles mit unterschiedlichen Farben (eigene/fremde)  
✅ Zeitstempel in lesbarem Format  
✅ Automatisches Scrollen zum Ende  
✅ Tastatur wird nach Senden ausgeblendet  
✅ Responsive Breite der Bubbles  
✅ Absendername bei fremden Nachrichten  
✅ Abgerundete Ecken wie bei WhatsApp  

## DateTime Konvertierung

Die Ticket-Klasse konvertiert automatisch zwischen:
- **Backend**: Unix-Timestamps (Millisekunden als String)
- **Mobile**: DateTime-Objekte

```csharp
// Zugriff über Legacy-Property
DateTime? erstellDatum = ticket.datum; // konvertiert ticket.start

// Direkter Zugriff (Backend-Format)
string startTimestamp = ticket.start; // "1704067200000"
```

## Migration von bestehendem Code

### Vorher (alte Struktur):
```csharp
ticket.beschreibung = "Text";
ticket.messages.Add(new TicketMessage(...));
var msg = ticket.messages.First();
string text = msg.nachricht;
int sender = msg.absenderId;
```

### Nachher (neue Struktur):
```csharp
ticket.text = "Text";  // oder ticket.beschreibung (Legacy)
ticket.chats.Add(new TicketChat(...));
var msg = ticket.chats.First();
string text = msg.t;
int sender = msg.personid;
```

## Backend-Synchronisation

Wenn du Tickets vom Backend lädst:
1. Die JSON-Struktur ist kompatibel
2. `chats`-Liste wird automatisch deserialisiert
3. DateTime-Konvertierung erfolgt automatisch

## Zusätzliche Klassen

### **TicketPerson**
```csharp
public class TicketPerson
{
	public Int32 id { get; set; }
	public int rolle { get; set; }
	public string anrede { get; set; }
	public string firma { get; set; }
	public string vorname { get; set; }
	public string name { get; set; }
	public string mobile { get; set; }
	public string telefon { get; set; }
	public string mail { get; set; }
	// ...

	public string GetFullName(); // Hilfsmethode
}
```

### **TicketObjekt**
```csharp
public class TicketObjekt
{
	public Int32 id { get; set; }
	public string objektnr { get; set; }
	public string objektname { get; set; }
	public string adresse { get; set; }
	public string plz { get; set; }
	public string ort { get; set; }
	// ...

	public string GetFullAddress(); // Hilfsmethode
}
```

## Fehlerbehebung

### Chat wird nicht angezeigt
- Prüfe, ob `ticket.chats` (nicht `ticket.messages`) Nachrichten hat
- Prüfe, ob `editticket_vscroll` sichtbar ist

### Backend-Daten werden nicht geladen
- Stelle sicher, dass JSON-Properties exakt übereinstimmen
- Prüfe, ob Datum-Strings im richtigen Format sind

### Zeitstempel werden falsch angezeigt
- Die Klasse konvertiert automatisch Unix-Timestamps
- Bei Problemen prüfe `ConvertStringToDateTime()` Methode

## Nächste Schritte (Optional)

1. **Backend-API-Integration**: REST-API-Calls zum Laden/Speichern
2. **Echtzeit-Updates**: SignalR für Live-Chat
3. **Anhänge**: Bilder/Dateien an Chat-Nachrichten
4. **Push-Benachrichtigungen**: Bei neuen Nachrichten
5. **Offline-Synchronisation**: Nachrichten queuen
