# Frontend — Angular 22

A HealthTracker webes felülete: **Angular 22**, **Angular Material 22** (Material 3), **TypeScript**.
A backend a `http://localhost:8080` címen futó ASP.NET Core API.

A kód a mai (2026-os) Angular alapértelmezéseit követi:

- **standalone komponensek** — nincs `NgModule`, minden komponens maga sorolja fel az `imports`-ban, mit használ;
- **signalok** (`signal`, `computed`, `linkedSignal`, `input`, `output`, `model`) az állapotra;
- **zoneless** change detection és **OnPush** alapértelmezés (nincs `zone.js`);
- **Signal Forms** (`@angular/forms/signals`) az űrlapokhoz;
- **`httpResource`** az adatlekérésekhez, `HttpClient` + Observable a módosításokhoz;
- beépített **control flow** a sablonokban (`@if`, `@for`, `@let`);
- **lazy loading** (`loadComponent`) az oldalakra, funkcionális **guard** és **interceptor**;
- a 2025-ös **style guide** fájlnevei (`login.ts`, nem `login.component.ts`);
- **Angular Material** komponensek, a stílus a Material 3 **design tokenekből** (`--mat-sys-*`) és a komponensek saját, scope-olt CSS-éből jön.

## Fejlesztői szerver

```bash
npm install
npm start          # = ng serve
```

Az alkalmazás a **http://localhost:4200** címen fut. A port az `angular.json`-ban van rögzítve
(`serve.options.port`), mert a backend CORS-beállítása (`Cors:AllowedOrigin`) ezt az origint
engedi. Ha portot váltasz, a backendet is állítsd át.

## Build, lint, tesztek

```bash
npm run build      # típusellenőrzés + éles build a dist/ mappába
npm run lint       # angular-eslint (TS + sablonok + akadálymentesség)
npm test           # Vitest (ng test)
```

## Konfiguráció

Az API címe build-időben égődik be az `API_BASE` konstansba: az alapértéke
(`http://localhost:8080`) az `angular.json` → `build.options.define` alatt van, a típusa a
`src/env.d.ts`-ben. Más címhez:

```bash
npx ng build --define "API_BASE='https://api.example.com'"
```

A Docker image ugyanezt teszi az `API_BASE` build argból (lásd [Dockerfile](Dockerfile)).
A kód nem közvetlenül a konstanst olvassa, hanem az `API_BASE_URL` injection tokent — így
tesztben felülírható.

## Felépítés

```
src/
├── main.ts                       # belépési pont: bootstrapApplication(App, appConfig)
├── index.html
├── styles.scss                   # Material téma (mat.theme) + közös segédosztályok (.page, .banner…)
├── env.d.ts                      # az API_BASE build-konstans típusa
└── app/
    ├── app.ts                    # gyökérkomponens: csak egy <router-outlet />
    ├── app.config.ts             # providerek: router, HttpClient + interceptor
    ├── app.routes.ts             # útvonalak (lazy loadComponent, authGuard)
    ├── core/
    │   ├── auth/
    │   │   ├── session.ts        # token/e-mail tárolása localStorage-ban
    │   │   ├── auth.ts           # Auth service: signalok + login/register/logout
    │   │   ├── auth-guard.ts     # védett útvonal: bejelentkezés nélkül /login-ra irányít
    │   │   ├── auth-interceptor.ts # API-cím + bearer token minden kérésre, 401 → kiléptetés
    │   │   └── models/login-response.ts
    │   ├── http/
    │   │   ├── api-base.ts       # API_BASE_URL injection token
    │   │   └── error-message.ts  # ProblemDetails → olvasható hibaüzenet
    │   ├── common/
    │   │   ├── dates.ts          # yyyy-MM-dd dátum-segédek
    │   │   └── times.ts          # HH:mm:ss idő-segédek (toMinutes, toHm)
    │   └── modules/              # a backend moduljai, egy mappa / modul
    │       ├── water/
    │       │   ├── water-api.ts  # WaterApi service (REST-hívások)
    │       │   └── models/       # DTO-k, fájlonként egy típus (daily-summary.ts, reminder.ts…)
    │       ├── schedule/
    │       │   ├── schedule-api.ts
    │       │   └── models/       # activity.ts, activity-color.ts, day-schedule.ts…
    │       └── calories/
    │           ├── calories-api.ts
    │           └── models/       # food-entry.ts, meal.ts, day-calories.ts…
    ├── shared/
    │   ├── date-nav.*            # nap váltó + mat-datepicker ([(date)] kétirányú kötés)
    │   ├── auth-card.ts          # a login/regisztráció kártyája (ng-content tartalomvetítés)
    │   ├── field-error.ts        # űrlapszintű (pl. szerver-) hiba; mezőn belül <mat-error>
    │   ├── autofocus.ts          # appAutofocus direktíva
    │   ├── duration-pipe.ts      # {{ 95 | duration }} → 1h 35m
    │   └── hm-pipe.ts            # {{ '09:30:00' | hm }} → 09:30
    ├── layout/app-layout.*       # közös fejléc + navigáció a védett oldalakhoz
    └── pages/
        ├── login/  register/
        ├── water-tracker/
        ├── schedule/
        │   ├── schedule.*        # a Napirend oldal: dátumváltó, összesítés, űrlap
        │   ├── day-timeline.*    # a vizuális idővonal (csak megjelenítés: input → output)
        │   ├── activity-form.*   # felvitel/szerkesztés/törlés
        │   ├── lanes.ts          # az átfedő elemek oszlopokba rendezése (+ lanes.spec.ts)
        │   └── models/           # az oldal saját típusai (form modell, timeline blokk…)
        └── calories/
            ├── calories.*        # a Kalória oldal: napi keret, étkezés-szekciók
            ├── food-entry-form.* # bejegyzés felvitele/szerkesztése/törlése
            ├── goal-editor.ts    # a napi kalóriakeret (inline sablonnal)
            └── models/           # food-entry-form-model.ts, panel.ts
```

**Típusok:** fájlonként egy típus, a `models/` mappákban (mint .NET-ben egy osztály / fájl).
Ezek `interface`-ek, nem `class`-ok: az `HttpClient` a JSON-ból sima objektumot ad, nem
példányosít osztályt — az interface csak az adat alakját írja le (mint egy `record` DTO), és
futásidőben nem létezik, tehát nem kerül a bundle-be.

Az oldalkomponensek `export default`-tal exportálnak, így az útvonalban elég a
`loadComponent: () => import('./pages/login/login')`.

## Ha Reactból jössz

| React (a korábbi változat) | Angular megfelelő |
|---|---|
| `useState` | `signal()` — olvasás: `count()`, írás: `count.set(1)` / `count.update(n => n + 1)` |
| `useMemo` | `computed(() => ...)` — magától tudja, mitől függ, nincs dependency-tömb |
| `useEffect` a state visszaállítására (pl. űrlap ↔ `editing`) | `linkedSignal(() => ...)` |
| `useEffect` + `fetch` + `setState` | `httpResource(() => url)` — `value()`, `isLoading()`, `reload()` |
| props | `input()` / `input.required()` |
| callback prop (`onSave`) | `output()` + `(saved)="..."` a szülőben |
| `value` + `onChange` (lifting state up) | `model()` + `[(date)]="date"` |
| `AuthProvider` + `useAuth()` Context | `@Injectable({ providedIn: 'root' }) class Auth` + `inject(Auth)` |
| `apiFetch()` (token egy helyen) | `authInterceptor` (`HttpInterceptorFn`) |
| `<RequireAuth>` wrapper | `authGuard` (`CanActivateFn`) a route `canActivate`-jében |
| `<Routes>` / `<Route>` | `app.routes.ts` + `<router-outlet />` |
| `<NavLink className={({isActive}) => ...}>` | `routerLink` + `routerLinkActive="..."` |
| `{cond && <X/>}` / `.map()` | `@if (cond) { ... }` / `@for (x of xs; track x.id) { ... }` |
| kontrollált input + kézi validálás a submitban | Signal Forms: `form(model, schema)`, `[formField]`, `[formRoot]`, `submit` |
| `formatTime()` | `{{ iso \| date: 'HH:mm' }}` (beépített `DatePipe`) |
| `formatDuration()` | saját pipe: `{{ minutes \| duration }}` |
| `import.meta.env.VITE_API_BASE` | `API_BASE` build-konstans (`define`) + `API_BASE_URL` token |
| `className={...}` | `class="..."` + `[class]="..."` / `[class.x]="feltétel"` |
| sima CSS fájl, globális | komponens `styleUrl` — Angular scope-olja, csak az adott komponensre hat |
| `style={{ width: ... }}` | `[style.width.%]="..."` |

## Angular Material — ami az appban előfordul

| Hol | Material elem |
|---|---|
| fejléc | `mat-toolbar`, `mat-tab-nav-bar` + `mat-tab-link` (routerrel), `mat-icon` |
| kártyák | `mat-card appearance="outlined"` (+ `mat-card-header`, `-content`, `-actions`) |
| űrlapmezők | `mat-form-field appearance="outline"` + `matInput` + `mat-label` / `mat-hint` / `mat-error` / `matTextSuffix` |
| gombok | `mat-flat-button`, `mat-stroked-button`, `mat-button`, `mat-icon-button` |
| választó | `mat-button-toggle-group` (étkezés) |
| listák | `mat-list` (víz), `mat-action-list` (kattintható kalória-bejegyzések) |
| egyéb | `mat-progress-bar`, `mat-datepicker`, `MatSnackBar` (hibaüzenet), `matTooltip`, `cdkTextareaAutosize` |

**Signal Forms + Material:** a `[formField]` közvetlenül a `matInput`-ra kerül. A `mat-error`
magától akkor látszik, ha a mező touched és hibás. A `mat-button-toggle-group` és a többi
ControlValueAccessor-alapú Material vezérlő ugyanígy köthető. A korlátokat (`required`,
`min`, `maxLength`…) a sémában add meg, ne HTML-attribútumként — a `[formField]` állítja be őket.

**Testreszabás:** a téma a `styles.scss`-ben van (`mat.theme`: paletta, betűtípus, sűrűség).
Egy komponens színét a CSS tokenjével írod felül, pl.:

```css
.danger-button {
  --mat-button-filled-container-color: var(--mat-sys-error);
}
```

**Ikonok:** `<mat-icon>delete</mat-icon>` — a Material Symbols betűkészlet (index.html) bármely
nevét használhatod: https://fonts.google.com/icons
