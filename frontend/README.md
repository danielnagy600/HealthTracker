# Frontend — React + Vite

A HealthTracker webes felülete: **React 19**, **TypeScript**, **Vite** és **React Router**.
A backend a `http://localhost:8080` címen futó ASP.NET Core API.

## Fejlesztői szerver

```bash
npm install
npm start          # = vite
```

Az alkalmazás a **http://localhost:4200** címen fut. A port szándékosan van rögzítve
(`vite.config.ts` → `server.port`, `strictPort: true`), mert a backend CORS-beállítása
(`Cors:AllowedOrigin`) ezt az origint engedi. Ha portot váltasz, a backendet is állítsd át.

## Build, lint, tesztek

```bash
npm run build      # típusellenőrzés (tsc -b) + éles build a dist/ mappába
npm run preview    # az elkészült build kiszolgálása
npm run lint       # oxlint (hook-szabályok, Fast Refresh)
npm test           # vitest
```

## Konfiguráció

Az API címe env-változóból jön: `VITE_API_BASE` (lásd [.env.development](.env.development)).
Éles buildhez vedd fel egy `.env.production` fájlba, vagy add meg a build parancs előtt.
Ha nincs megadva, a kód a `http://localhost:8080` alapértelmezést használja.

## Felépítés

```
src/
├── main.tsx                  # belépési pont: Router + AuthProvider + App
├── App.tsx                   # útvonalak
├── styles.css                # globális stílusok
├── vite-env.d.ts             # az env-változók típusai
├── core/
│   ├── api.ts                # API_BASE + apiFetch (bearer tokent tesz a kérésekre)
│   ├── session.ts            # token/e-mail tárolása localStorage-ban
│   ├── auth-context.ts       # a Context és a típusa
│   ├── AuthProvider.tsx      # a bejelentkezési állapot szolgáltatója
│   ├── use-auth.ts           # useAuth() hook
│   ├── RequireAuth.tsx       # védett útvonal: bejelentkezés nélkül /login-ra irányít
│   ├── water.ts              # a Water modul DTO-i és REST-hívásai
│   ├── schedule.ts           # a Schedule modul DTO-i, REST-hívásai és idő-segédei
│   ├── calories.ts           # a Calories modul DTO-i és REST-hívásai
│   └── format.ts             # időformázás (HH:mm)
└── features/
    ├── layout/AppLayout.tsx  # közös fejléc + navigáció a védett oldalakhoz
    ├── dashboard/Dashboard.tsx
    ├── schedule/
    │   ├── Schedule.tsx      # a Napirend oldal: dátumváltó, összesítés, űrlap
    │   ├── DayTimeline.tsx   # a vizuális idővonal
    │   ├── ActivityForm.tsx  # felvitel/szerkesztés színválasztóval és megjegyzéssel
    │   └── lanes.ts          # az átfedő elemek oszlopokba rendezése (tiszta függvény)
    ├── calories/
    │   ├── Calories.tsx      # a Kalória oldal: napi keret, étkezés-szekciók
    │   ├── FoodEntryForm.tsx # bejegyzés felvitele/szerkesztése
    │   └── GoalEditor.tsx    # a napi kalóriakeret módosítása
    ├── login/Login.tsx
    └── register/Register.tsx
```

Névkonvenció: **PascalCase** a komponenst exportáló fájloknak, **kebab-case** minden
másnak. A context, a provider és a hook szándékosan három fájl — egy fájl vagy
komponenst exportál, vagy egyebet, különben a Fast Refresh elveszti az állapotot
(ezt az `only-export-components` lint-szabály őrzi).

## Ha Angularról jössz

| Angular | React megfelelő |
|---|---|
| `AuthService` (`providedIn: 'root'`, signalok) | `AuthProvider` + `useAuth()` Context |
| `authInterceptor` (HTTP-interceptor) | `apiFetch()` — egy helyen teszi rá a tokent |
| `authGuard` (`CanActivateFn`) | `<RequireAuth>` wrapper komponens |
| `app.routes.ts` + `<router-outlet />` | `<Routes>` / `<Route>` az `App.tsx`-ben |
| `[(ngModel)]` | `value` + `onChange` (kontrollált input) |
| `@if` / `@for` a sablonban | `{feltétel && ...}` / `.map()` a JSX-ben |
| `\| date: 'HH:mm'` pipe | `formatTime()` a `core/format.ts`-ből |
| `HttpClient` + `Observable` | `fetch` + `Promise` / `async-await` |
| `environment.ts` | `import.meta.env.VITE_*` |
