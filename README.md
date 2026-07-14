⚡ ChargeTracker
Non dimenticare mai più di mettere in carica i tuoi dispositivi.

ChargeTracker è un'app desktop per Windows che tiene traccia delle ricariche di tutti i dispositivi elettronici di casa o dell'ufficio — smartphone, tablet, laptop, cuffie, smartwatch, droni, controller, spazzolini elettrici e molto altro — così non li ritrovi mai scarichi proprio quando ti servono.

✨ Funzionalità
Dashboard con riepilogo a colpo d'occhio: dispositivi scaduti, in scadenza, carichi
Gestione dispositivi completa: categoria, posizione/stanza, tipo di caricatore, capacità batteria, data di acquisto e garanzia
Categorie e posizioni personalizzabili: crea, rinomina ed elimina le tue etichette, non sei vincolato a un elenco fisso
Calendario mensile con le scadenze di ricarica di ogni dispositivo
Statistiche: ricariche totali, salute stimata delle batterie, costo energetico stimato, dispositivo più usato
Promemoria automatici tramite icona nella system tray, anche a finestra chiusa
Widget desktop sempre in primo piano con i dispositivi da caricare
Etichette QR stampabili da applicare fisicamente sui caricatori
Rilevamento dispositivi Bluetooth già associati a Windows, per importarli in un clic
Backup ed export/import in JSON
Sincronizzazione multi-PC spostando il database in una cartella OneDrive/Dropbox
Tema chiaro, scuro o automatico in base all'ora del giorno
Tutto 100% locale: nessun account, nessuna connessione internet richiesta, i tuoi dati restano sul tuo PC

🖥️ Screenshot
<img width="1166" height="753" alt="image" src="https://github.com/user-attachments/assets/8e1026d8-a564-4b13-9727-2e7a8741be36" />


🚀 Come compilare
Visual Studio 2022
Installa Visual Studio 2022 (va bene anche la versione Community) con il workload ".NET Desktop Development"
Apri `ChargeTrackerApp.sln`
Visual Studio scaricherà automaticamente i pacchetti NuGet necessari al primo build
Premi F5 per eseguire, o Ctrl+Shift+B per compilare soltanto
Riga di comando
Richiede il .NET 8 SDK:
```powershell
cd ChargeTrackerApp
dotnet build -c Release
```
Per un eseguibile singolo e autonomo (non richiede il runtime .NET installato sul PC di destinazione):
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
🛠️ Stack tecnologico
WPF (.NET 8) — interfaccia desktop nativa Windows
LiteDB — database locale embedded, nessun server richiesto
QRCoder — generazione etichette QR
System.Management — rilevamento dispositivi Bluetooth via WMI
📁 Struttura del progetto
```
ChargeTrackerApp/
  Models/        → Device, AppSettings, categorie predefinite
  Services/       → accesso dati, notifiche, tema, avvio automatico
  ViewModels/     → logica MVVM
  Views/          → finestre secondarie (dispositivo, widget, calendario...)
  Themes/         → temi chiaro e scuro
```
🗺️ Roadmap / idee future
[ ] Statistiche con grafici interattivi
[ ] Notifiche push su smartphone tramite companion app
[ ] Condivisione multi-utente/famiglia
[ ] Localizzazione in altre lingue
Contributi e segnalazioni sono benvenuti: apri pure una issue o una pull request.
📄 Licenza
Distribuito con licenza MIT — vedi il file `LICENSE` per i dettagli.
