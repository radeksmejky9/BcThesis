# AR Digital Twins

Aplikace pro vizualizaci digitálních dvojčat stavebních projektů v rozšířené realitě. Systém umožňuje správu 3D modelů prostřednictvím webového rozhraní a jejich zobrazení v AR pomocí mobilní aplikace.

## 🏗️ Struktura projektu

```
├── Thesis/          # Bakalářská práce (LaTeX)
├── WebServer/       # Serverová část (Flask + MongoDB)
└── ARDigitalTwins/  # Mobilní aplikace (Unity)
```

## 🚀 Rychlý start

### Serverová část

**Požadavky:**
- Docker

**Instalace:**

1. Získejte Google Maps API klíč:
   - Vytvořte projekt v [Google Cloud Console](https://console.cloud.google.com/)
   - Povolte Google Maps Static API
   - Vygenerujte API klíč

2. Nastavte API klíč v `.env`:
   ```bash
   cd WebServer
   echo "GOOGLE_MAPS_API_KEY=<API_KEY>" > .env
   ```

3. Spusťte server:
   ```bash
   docker-compose up
   ```

Server běží na `http://localhost:5000`

### Mobilní aplikace

**Požadavky:**
- Unity 2022.3.48f1

**Instalace:**

1. Otevřete projekt v Unity:
   ```bash
   cd ARDigitalTwins
   ```

2. Nastavte adresu serveru:
   - V hierarchii najděte objekt `DBConnector`
   - V inspektoru upravte `Server URL` na:
     - `http://localhost:5000` (Android emulátor)
     - `http://<IP_adresa_hostujícího_zařízení>:5000` (Fyzické zařízení)

3. Sestavte aplikaci:
   - `File → Build Settings`
   - Vyberte platformu `Android`
   - Případně vyberte zařízení
   - `Build` nebo `Build And Run`

## 📱 Použití

### Webové rozhraní

1. Otevřete `http://localhost:5000`
2. Nahrajte nový projekt:
   - Klikněte na "Nahrát nový projekt"
   - Vyplňte název, popis a souřadnice
   - Nahrajte GLB soubor a náhledový obrázek
3. Projekt se automaticky zobrazí v mobilní aplikaci
4. Zobrazte si hodnocení

### Mobilní aplikace

1. Nainstalujte APK na Android zařízení
2. Spusťte aplikaci
3. Otevřete mapu
4. Na mapě vyberte projekt
5. Zobrazte 3D model v AR
6. Ohodnoťte projekt

## 🛠️ Technologie

**Backend:**
- Python
- Flask
- MongoDB
- Docker

**Frontend:**
- HTML/CSS (Tailwind CSS)
- JavaScript

**Mobile:**
- Unity 2022.3.48f1
- C#
- XR Interaction Toolkit
- GLTFast

## 📋 API Endpointy

```
GET    /files                      # Seznam všech projektů
GET    /files/<id>                 # Detail projektu
GET    /files/<filename>/download  # Stažení GLB/obrázku
GET    /files/<id>/ratings         # Hodnocení projektu
GET    /maps/staticmap             # Mapové dlaždice
POST   /files                      # Nahrání projektu
POST   /files/<id>/ratings         # Přidání hodnocení
PUT    /files/<id>                 # Aktualizace projektu
DELETE /files/<id>                 # Smazání projektu
```
## 👤 Autor

Radek Šmejkal
Bakalářská práce, 2024-2025

## 📄 Poznámky
- Server vyžaduje aktivní Google Maps API klíč
- Mobilní aplikace vyžaduje zařízení s Androidem
- Zabezpečení (autentizace/autorizace) není implementováno v prototypu