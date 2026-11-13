# HealthTracker — Clean Architecture tanuló projekt

Egészség-követő alkalmazás, amely a **Clean Architecture és a CQRS (MediatR)** tanulására
készült. Jelenleg három funkciót tartalmaz:

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
| Backend      | .NET 10, ASP.NET Core (Controllerek), MediatR, AutoMapper |
| Adatbázis    | PostgreSQL + Entity Framework Core (Npgsql) |
| Auth         | ASP.NET Core Identity, bearer token (`MapIdentityApi`) |
| Frontend     | Angular 22 + Angular Material (TypeScript, signalok, Signal Forms) |
| Tesztek      | xUnit + Moq + FluentAssertions |
| Konténer     | Docker / docker-compose (PostgreSQL + API) |

## Architektúra — Clean Architecture

A megoldás egyetlen futtatható alkalmazás, **rétegekre** bontva. Minden réteg külön
projekt, így a fordító garantálja, hogy a függőségek csak befelé mutathatnak.

```
HealthTracker/
├── src/
│   ├── HealthTracker.Domain               # Core: üzleti fogalmak, semmitől nem függ
│   │   ├── Entities/        # WaterIntake, FoodEntry, Activity, AppUser ...
│   │   ├── Enums/           # MealType, ActivityColor ...
│   │   ├── DomainServices/  # tiszta üzleti logika (emlékeztető, napi egyenleg, nap-terv)
│   │   └── ValueObjects/    # a kalkulátorok eredményei (DayPlan, DailyCalories ...)
│   ├── HealthTracker.Application          # Core: use case-ek (mit tud az alkalmazás)
│   │   ├── Contracts/       # interfészek: IClock, ICurrentUser, Persistence/I*Repository
│   │   ├── Features/<Funkció>/
│   │   │   ├── Commands/<Név>/  # írás: XxxCommand + XxxCommandHandler (MediatR)
│   │   │   ├── Queries/<Név>/   # olvasás: XxxQuery + XxxQueryHandler (MediatR)
│   │   │   └── Models/          # kérés/válasz DTO-k
│   │   └── Profiles/        # AutoMapper: entitás → DTO leképezések
│   ├── HealthTracker.Persistence          # Infrastructure: EF Core + PostgreSQL
│   │   ├── HealthTrackerDbContext.cs  # egyetlen DbContext (Identity + minden funkció)
│   │   ├── Configuration/   # táblák beállításai (IEntityTypeConfiguration)
│   │   ├── Repositories/    # az Application repository-interfészeinek megvalósítása
│   │   └── Migrations/
│   ├── HealthTracker.Api                  # host: Controllerek, auth, CORS, OpenAPI
│   │   └── Controllers/     # HTTP → MediatR kérés (mediator.Send)
│   └── HealthTracker.UI                   # Angular frontend (VS-ben: HealthTracker.UI.esproj)
├── tests/
│   ├── HealthTracker.Domain.Tests         # a kalkulátorok (adatbázis és mock nélkül)
│   └── HealthTracker.Application.Tests    # a handlerek mockolt repository-val
├── docker-compose.yml                     # PostgreSQL + API + frontend
└── HealthTracker.slnx
```

### A függőségek iránya (a lényeg!)

```
Api ──► Persistence ──► Application ──► Domain
 └────────────────────────►┘
```

- A **Domain** semmitől nem függ (tiszta üzleti fogalmak és szabályok).
- Az **Application** csak a Domaintől függ. Az adatbázist csak interfészeken
  (`IWaterRepository` ...) keresztül ismeri, a megvalósítást nem.
- A **Persistence** valósítja meg ezeket az interfészeket (EF Core, PostgreSQL).
- Az **Api** rakja össze az egészet (`AddApplicationServices`, `AddPersistenceServices`),
  a Controllerek pedig csak MediatR-kéréseket küldenek.

Ez a **Dependency Inversion**: pl. az `AddIntakeCommandHandler` az `IWaterRepository`
interfészt ismeri, nem az EF Core-os megvalósítást — ezért lehet a teszteket adatbázis
nélkül futtatni.

### Egy kérés útja

```
POST /api/water/intakes
  → WaterController.AddIntake         (validálás, HTTP-válasz, log)
  → mediator.Send(new AddIntakeCommand(500))
  → AddIntakeCommandHandler.Handle    (üzleti lépések: ki a user, mi a mai nap)
  → IWaterRepository.AddIntakeAsync   (Persistence: EF Core → PostgreSQL)
  → mapper.Map<IntakeItem>(intake)    (AutoMapper: entitás → DTO)
```

> Az AutoMapper 15+ és a MediatR 13+ licenckulcsot kér (luckypennysoftware.com,
> egyéni/kis projektnek ingyenes community kulcs). A kulcs helye a
> `LuckyPenny:LicenseKey` beállítás; nélküle is működnek, csak figyelmeztetést logolnak.

## Előfeltételek

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org) (az Angular frontendhez)
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
- `GET  /api/water/day?date=`  – egy nap összesítése (a dátum elhagyható, token kell)
- `GET  /api/water/reminder`   – emlékeztető: mikor mennyit igyál (token kell)
- `POST /api/water/intakes`    – vízbejegyzés (`{ "amountMl": 250 }`)
- `GET  /api/water/intakes/{id}` – egy vízbejegyzés
- `DELETE /api/water/intakes/{id}` – vízbejegyzés törlése
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
cd src/HealthTracker.UI
npm install
npm start
```

Az Angular dev-szerver (`ng serve`) a **http://localhost:4200** címen fut, és a 8080-as API-t hívja
(a CORS ehhez be van állítva). A port a `src/HealthTracker.UI/angular.json`-ban van rögzítve —
ha megváltoztatod, a backend `Cors:AllowedOrigin` beállítását is írd át.

## Tesztek

```bash
dotnet test
```

`HealthTracker.Domain.Tests` – a tiszta domain-logika (adatbázis és mock nélkül):

- `WaterReminderCalculatorTests` – mikor mennyi vizet igyál.
- `DayPlanCalculatorTests` – a Napirend: ütközések, szabad sávok, az átfedés nem
  duplán számoló foglaltság.
- `CalorieCalculatorTests` – a Kalória: egyenleg, étkezésenkénti bontás, túllépés,
  a nullával osztás elkerülése.

`HealthTracker.Application.Tests` – a MediatR handlerek Moq-kal mockolt repository-val,
valódi AutoMapperrel, FluentAssertionsszel:

- `WaterHandlersTests`, `ScheduleHandlersTests`, `CaloriesHandlersTests` – a use case-ek;
  azt is ellenőrzik, hogy más felhasználó adatait nem lehet elérni.
- `MappingProfileTests` – minden AutoMapper-leképezés teljes (nincs elfelejtett mező).

## Teljes stack Dockerben (opcionális, későbbi lépés)

```bash
docker compose up --build
```

Ez felhúzza a PostgreSQL-t és az API-t is. (A frontend konténerizálása a
következő bővítés — a `docker-compose.yml` már fel van készítve rá.)

## Hogyan adj hozzá egy új funkciót? (pl. Alvás)

1. **Domain**: `Entities/SleepEntry.cs` (és ha kell, egy kalkulátor a `DomainServices/` alá).
2. **Application**:
   - `Contracts/Persistence/ISleepRepository.cs`
   - `Features/Sleep/Models/` – a kérés/válasz DTO-k
   - `Features/Sleep/Commands/AddSleep/` – `AddSleepCommand` + `AddSleepCommandHandler`
   - `Features/Sleep/Queries/GetSleepDay/` – `GetSleepDayQuery` + `GetSleepDayQueryHandler`
   - `Profiles/MappingProfile.cs` – `CreateMap<SleepEntry, SleepItem>()`

   A handlereket a MediatR magától megtalálja, nem kell regisztrálni.
3. **Persistence**:
   - `HealthTrackerDbContext`: új `DbSet<SleepEntry>`
   - `Configuration/SleepEntryConfiguration.cs` (saját `sleep` séma a `Schemas` osztályban)
   - `Repositories/SleepRepository.cs` + regisztráció a `PersistenceServiceRegistration`-ben
4. **Api**: `Controllers/SleepController.cs`, ami `mediator.Send(...)`-et hív.
5. Migráció:
   ```bash
   dotnet ef migrations add AddSleep \
     --project src/HealthTracker.Persistence \
     --startup-project src/HealthTracker.Api \
     --output-dir Migrations
   ```
   (A `--startup-project` azért kell, mert az EF Design csomag csak a hostban van.)

A `Features/Schedule` kidolgozott mintaként olvasható: minden lépése ezt a receptet követi.
