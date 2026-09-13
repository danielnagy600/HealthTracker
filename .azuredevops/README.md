# Azure DevOps CI/CD

A repo két Azure DevOps pipeline-t tartalmaz:

| Fájl | Mit csinál | Mikor fut |
| --- | --- | --- |
| [`azure-pipelines.yml`](../azure-pipelines.yml) | Build + teszt → Docker image-ek ACR-be → deploy App Service-re | PR `main`-re (csak Build), push `main`-re (mindhárom stage) |
| [`security-pipeline.yml`](security-pipeline.yml) | Biztonsági elemzés (MSDO) – a CodeQL workflow párja | PR, push `main`-re, és hetente hétfőn 03:00 |

A `.github/workflows/` alatti GitHub Actions workflow-k változatlanul megmaradtak; ha
az Azure DevOps beállt, azok törölhetők.

## Pipeline stage-ek

```
Build ──────────────► Package ──────────► Deploy
(.NET + React)        (docker build       (AzureWebAppContainer:
 PR-on is fut          + push ACR-be)      api, majd frontend)
```

A `Package` és `Deploy` stage csak `main` ágon fut (`isMain` változó), így egy pull
request nem tol fel image-et és nem deployol.

## Beállítás

### 1. Service connectionök

A **Project Settings → Service connections** alatt kell létrehozni:

| Név (szabadon választható) | Típus | Mire kell |
| --- | --- | --- |
| pl. `healthtracker-acr` | Docker Registry → Azure Container Registry | image push |
| pl. `healthtracker-azure` | Azure Resource Manager (workload identity federation ajánlott) | App Service deploy |

### 2. Variable group

**Pipelines → Library → + Variable group**, neve legyen pontosan `healthtracker-cd`
(erre hivatkozik a `variables: - group:` sor). Tartalma:

| Változó | Példa érték | Megjegyzés |
| --- | --- | --- |
| `dockerRegistryServiceConnection` | `healthtracker-acr` | az 1. pontban létrehozott Docker service connection neve |
| `azureSubscription` | `healthtracker-azure` | az ARM service connection neve |
| `containerRegistry` | `healthtracker.azurecr.io` | az ACR teljes login szervere |
| `apiImageRepository` | `healthtracker-api` | image név az ACR-ben |
| `frontendImageRepository` | `healthtracker-frontend` | image név az ACR-ben |
| `apiWebAppName` | `healthtracker-api` | App Service (Web App for Containers) neve |
| `frontendWebAppName` | `healthtracker-web` | App Service (Web App for Containers) neve |
| `apiBaseUrl` | `https://healthtracker-api.azurewebsites.net` | **build-időben** égetjük a frontendbe (`VITE_API_BASE`) |

A variable groupnál a **Pipeline permissions** alatt engedélyezni kell a pipeline
hozzáférését, különben a futás „variable group not found" hibával áll le.

### 3. Environment

**Pipelines → Environments → New environment**, neve `healthtracker-production`.
Ide köthető approval / check, ha a deploy előtt kézi jóváhagyást szeretnél.

### 4. Pipeline-ok felvétele

**Pipelines → New pipeline → Azure Repos Git / GitHub → Existing Azure Pipelines YAML file**,
majd külön-külön a `/azure-pipelines.yml` és a `/.azuredevops/security-pipeline.yml`.

### 5. Az App Service-ek konfigurációja

A pipeline csak az image-et cseréli, a beállításokat nem – azokat egyszer, az App
Service-en kell megadni (Configuration → Application settings):

- **API**: `ConnectionStrings__Postgres`, `Cors__AllowedOrigin` (a frontend publikus URL-je),
  `ASPNETCORE_ENVIRONMENT=Production`, `WEBSITES_PORT=8080`
- **Frontend**: `WEBSITES_PORT=80`

Az adatbázis a `docker-compose.yml`-ben helyi Postgres konténer; éles környezetben
ezt egy Azure Database for PostgreSQL váltja ki, aminek a connection stringjét a fenti
`ConnectionStrings__Postgres` beállításba (vagy Key Vault referenciába) kell tenni.

## Ami átkerült a GitHub Actionsből

- `dotnet restore / build -warnaserror / test` → `backend-job.yml`, kiegészítve TRX
  teszteredmény-publikálással és NuGet cache-sel.
- `npm ci / lint / build` → `frontend-job.yml`, kiegészítve a `npm run test`
  (vitest) lépéssel és a `dist/` artifact-publikálással.
- CodeQL → `security-pipeline.yml` (MSDO). **Az MSDO task használatához telepíteni
  kell a „Microsoft Security DevOps" extensiont** a szervezetbe a Marketplace-ről.
  Valódi CodeQL csak GitHub Advanced Security for Azure DevOps licenccel érhető el –
  az ehhez tartozó lépések kikommentezve szerepelnek a fájlban.
