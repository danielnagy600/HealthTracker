# HealthTracker — moduláris monolit tanuló projekt

Egészség-követő alkalmazás, amely a **modularizáció és réteges architektúra** tanulására
készült. Jelenleg három funkciómodult tartalmaz:

- **Vízfogyasztás-emlékeztető** — figyeli, mennyit ittál a mai napon, és jelzi,
  **mikor mennyi vizet igyál**, hogy elérd a napi célt.
- **Napirend** — a napi elfoglaltságaid kezelése: felvitel kezdés/befejezés
  időponttal, **vizuális idővonal**, választható szín és megjegyzés. Jelzi az
  **ütközéseket** és a szabad idősávokat.
- **Kalória** — napi kalóriabevitel **étkezésenkénti** bontásban (reggeli, ebéd,
  vacsora, nasi), állítható napi kerettel. Jelzi, mennyi fér még bele, és mikor
  lépted túl a keretet.

## Technológiák

| Terület      | Technológia |
|--------------|-------------|
| Backend      | .NET 10, ASP.NET Core (minimal API) |
| Adatbázis    | PostgreSQL + Entity Framework Core (Npgsql) |
| Auth         | ASP.NET Core Identity, bearer token (`MapIdentityApi`) |
| Frontend     | React 19 + Vite (TypeScript, React Router) |
| Tesztek      | xUnit + Moq + FluentAssertions |
| Konténer     | Docker / docker-compose (PostgreSQL + API) |

## Architektúra — moduláris monolit

A megoldás egyetlen futtatható alkalmazás (monolit), de **funkciómodulokra** bontva.
Minden modul önmagát regisztrálja, és csak a `SharedKernel`-től függ — a modulok
**nem függenek egymástól**.

```
HealthTracker/
├── src/
│   ├── HealthTracker.SharedKernel        # közös absztrakciók (IClock, ICurrentUser)
│   ├── HealthTracker.Modules.Water        # 1. modul: vízfogyasztás
│   │   ├── Domain/          # entitások + tiszta üzleti logika (emlékeztető-számítás)
│   │   ├── Application/     # DTO-k, interfészek, szolgáltatás
│   │   ├── Infrastructure/  # EF Core + PostgreSQL (DbContext, repository, migrációk)
│   │   └── WaterModule.cs   # a modul önregisztrációja (DI + HTTP-végpontok)
│   ├── HealthTracker.Modules.Schedule     # 2. modul: napi elfoglaltságok
│   │   ├── Domain/          # entitás + tiszta üzleti logika (nap-terv számítás)
│   │   ├── Application/     # DTO-k, interfészek, szolgáltatás
│   │   ├── Infrastructure/  # EF Core + PostgreSQL (saját "schedule" séma)
│   │   └── ScheduleModule.cs
│   ├── HealthTracker.Modules.Calories     # 3. modul: kalóriabevitel
│   │   ├── Domain/          # entitások + tiszta üzleti logika (napi egyenleg)
│   │   ├── Application/     # DTO-k, interfészek, szolgáltatás
│   │   ├── Infrastructure/  # EF Core + PostgreSQL (saját "calories" séma)
│   │   └── CaloriesModule.cs
│   ├── HealthTracker.Modules.Identity     # 4. modul: felhasználók, regisztráció/login
│   └── HealthTracker.Api                  # host: összerakja a modulokat, CORS, auth pipeline
├── tests/
│   ├── HealthTracker.Modules.Water.Tests     # xUnit tesztek
│   ├── HealthTracker.Modules.Schedule.Tests  # xUnit tesztek
│   └── HealthTracker.Modules.Calories.Tests  # xUnit tesztek
├── frontend/                              # React alkalmazás (Vite)
├── docker-compose.yml                     # PostgreSQL + API
└── HealthTracker.slnx
```

### A függőségek iránya (a lényeg!)

```
Api ──► Modules.Water ────► SharedKernel
    ├─► Modules.Schedule ──┤
    ├─► Modules.Calories ──┤
    └─► Modules.Identity ──┘
        (a modulok NEM látják egymást)
```

- A **Domain** réteg semmitől nem függ (tiszta üzleti fogalmak).
- Az **Application** csak a Domaintől és absztrakcióktól (interfészek) függ.
- Az **Infrastructure** valósítja meg az interfészeket (EF Core, PostgreSQL).
- Az **Api** csak a modulok publikus `AddXxxModule` / `MapXxxModule` metódusait hívja.

Ez a **Dependency Inversion**: pl. a `WaterService` az `IWaterRepository` interfészt
ismeri, nem az EF Core-os megvalósítást — ezért lehet a teszteket adatbázis nélkül futtatni.

## Előfeltételek

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org) (a React frontendhez)
- PostgreSQL — a legegyszerűbb Dockerrel (lásd lent), vagy helyi telepítés

## Futtatás

### 1. PostgreSQL indítása (Dockerrel)

```bash
docker compose up -d db
```

> Docker nélkül: telepíts egy helyi PostgreSQL-t, és hozz létre egy `healthtracker`
> adatbázist a `postgres` / `postgres` felhasználóval, vagy írd át a kapcsolati
> sztringet a `src/HealthTracker.Api/appsettings.json`-ban.

### 2. Backend indítása

```bash
dotnet run --project src/HealthTracker.Api
```

Induláskor automatikusan lefutnak az adatbázis-migrációk. Az API a
**http://localhost:8080** címen fut.

- `GET  /`                     – életjel
- `POST /api/auth/register`    – regisztráció (`{ "email": "...", "password": "..." }`)
- `POST /api/auth/login`       – bejelentkezés → bearer token
- `GET  /api/auth/me`          – a bejelentkezett profil (token kell)
- `GET  /api/water/summary`    – mai összesítés (token kell)
- `GET  /api/water/reminder`   – emlékeztető: mikor mennyit igyál (token kell)
- `POST /api/water/intake`     – vízbejegyzés (`{ "amountMl": 250 }`)
- `GET/PUT /api/water/settings`– napi cél és ébrenléti időablak
- `GET  /api/schedule/day?date=` – egy nap elfoglaltságai + összesítés (a dátum elhagyható)
- `POST /api/schedule/activities` – új elfoglaltság
- `PUT  /api/schedule/activities/{id}` – módosítás
- `DELETE /api/schedule/activities/{id}` – törlés
- `GET  /api/schedule/colors`  – a választható színek
- `GET  /api/calories/day?date=` – egy nap étkezésenkénti bontásban (a dátum elhagyható)
- `POST /api/calories/entries` – új bejegyzés
- `PUT  /api/calories/entries/{id}` – módosítás
- `DELETE /api/calories/entries/{id}` – törlés
- `GET/PUT /api/calories/goal` – napi kalóriakeret
- `GET  /api/calories/meals`   – a választható étkezések

> **Jelszó-szabályok** (Identity alapértelmezés): min. 6 karakter, kis- és nagybetű,
> szám és szimbólum — pl. `Passw0rd!`.

### 3. Frontend indítása

```bash
cd frontend
npm install
npm start
```

A Vite dev-szerver a **http://localhost:4200** címen fut, és a 8080-as API-t hívja
(a CORS ehhez be van állítva). A port a `frontend/vite.config.ts`-ben van rögzítve —
ha megváltoztatod, a backend `Cors:AllowedOrigin` beállítását is írd át.

## Tesztek

```bash
dotnet test
```

- `WaterReminderCalculatorTests` – a tiszta domain-logika (adatbázis és mock nélkül).
- `WaterServiceTests` – a szolgáltatás Moq-kal mockolt függőségekkel, FluentAssertionsszel.
- `DayPlanCalculatorTests` – a Napirend tiszta domain-logikája: ütközések, szabad
  sávok, az átfedés nem duplán számoló foglaltság.
- `ScheduleServiceTests` – a Napirend szolgáltatása mockolt tárolóval; azt is
  ellenőrzi, hogy más felhasználó bejegyzését nem lehet elérni.
- `CalorieCalculatorTests` – a Kalória tiszta domain-logikája: egyenleg,
  étkezésenkénti bontás, túllépés, a nullával osztás elkerülése.
- `CalorieServiceTests` – a Kalória szolgáltatása mockolt tárolóval: étkezésenkénti
  csoportosítás, a napi keret módosítása, más felhasználó adatainak elzárása.

## Teljes stack Dockerben (opcionális, későbbi lépés)

```bash
docker compose up --build
```

Ez felhúzza a PostgreSQL-t és az API-t is. (A React frontend konténerizálása a
következő bővítés — a `docker-compose.yml` már fel van készítve rá.)

## Hogyan adj hozzá egy új modult? (pl. Alvás)

1. `dotnet new classlib -o src/HealthTracker.Modules.Sleep`
2. Vedd fel a `SharedKernel` referenciát és az `Microsoft.AspNetCore.App`
   framework-referenciát (lásd a Water modul `.csproj`-ját).
3. Készítsd el a `Domain / Application / Infrastructure` rétegeket és egy
   `SleepModule.cs`-t `AddSleepModule` + `MapSleepModule` metódusokkal.
4. A `Program.cs`-ben hívd meg őket:
   ```csharp
   builder.Services.AddSleepModule(connectionString);
   // ...
   app.MapSleepModule();
   ```

5. Migráció a modul saját sémájába:
   ```bash
   dotnet ef migrations add InitialCreate \
     --project src/HealthTracker.Modules.Sleep \
     --startup-project src/HealthTracker.Api \
     --context SleepDbContext \
     --output-dir Infrastructure/Migrations
   ```
   (A `--startup-project` azért kell, mert az EF Design csomag csak a hostban van.)

A meglévő modulokat **nem kell módosítani** — pontosan ez a moduláris felépítés haszna.
A `Modules.Schedule` végig ezt a receptet követi, így kidolgozott mintaként is olvasható.
