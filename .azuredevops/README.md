# Azure DevOps CI/CD

A repo két Azure DevOps pipeline-t tartalmaz:

| Fájl | Mit csinál | Mikor fut |
| --- | --- | --- |
| [`azure-pipelines.yml`](../azure-pipelines.yml) | Build + teszt → Docker image-ek GHCR-be → deploy Azure Container Apps-ra | PR `main`-re (csak Build), push `main`-re (mindhárom stage) |
| [`security-pipeline.yml`](security-pipeline.yml) | Biztonsági elemzés (MSDO) – a CodeQL workflow párja | PR, push `main`-re, és hetente hétfőn 03:00 |

A `.github/workflows/` alatti GitHub Actions workflow-k változatlanul megmaradtak; ha
az Azure DevOps beállt, azok törölhetők.

## Miért GHCR + Container Apps, és nem ACR + App Service?

Ez egy gyakorló projekt, ezért kizárólag **tartósan** (nem csak az Azure Free Account
első 12 hónapjában) ingyenes szolgáltatásokat használunk:

- **GitHub Container Registry (ghcr.io)** – publikus image-eknél nincs tárhely- vagy
  időkorlát, és a repó úgyis GitHubon van, nem kell külön Azure-erőforrás.
- **Azure Container Apps (Consumption plan)** – havi **180 000 vCPU-másodperc,
  360 000 GiB-másodperc és 2 millió request ingyen, subscriptiononként, örökre**.
  Az alkalmazás **0 replikára skálázódik**, ha nincs forgalom – amíg nem használod,
  nem számláz.

Emiatt **nincs szükség ACR-re** (amit korábban felvetettünk) – azt nem kell
létrehozni.

## Pipeline stage-ek

```
Build ──────────────► Package ──────────► Deploy
(.NET + React)        (docker build       (AzureContainerApps@1:
 PR-on is fut          + push GHCR-be)     api, majd frontend)
```

A `Package` és `Deploy` stage csak `main` ágon fut (`isMain` változó), így egy pull
request nem tol fel image-et és nem deployol.

## Beállítás

### 1. GitHub Personal Access Token (image push-hoz)

**GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)**
→ **Generate new token**, jogosultságok: `write:packages`, `read:packages`,
`delete:packages`. Ezt csak az Azure DevOps push-hoz kell, a Container Apps
image-húzáshoz (lásd lent) nem szükséges hitelesítés, ha a package publikus.

### 2. Docker registry service connection (GHCR-hez)

**Project Settings → Service connections → New service connection → Docker Registry**
→ **Others**:

- **Docker Registry**: `https://ghcr.io`
- **Docker ID**: a GitHub felhasználóneved (vagy szervezeted neve)
- **Password**: az 1. pontban létrehozott PAT
- **Service connection name**: pl. `healthtracker-ghcr` (erre hivatkozik a
  `dockerRegistryServiceConnection` változó)

### 3. Azure Resource Manager service connection

**Project Settings → Service connections → New service connection → Azure Resource
Manager** (workload identity federation ajánlott) → neve pl. `healthtracker-azure`
(erre hivatkozik az `azureSubscription` változó). Ehhez kell egy Azure előfizetés,
de a Container Apps futtatása **nem kerül pénzbe**, amíg a fenti ingyenes kereten
belül maradsz.

### 4. Azure erőforrások létrehozása (egyszer, kézzel)

A pipeline csak a *futó* image-et cseréli – a Container Apps Environment-et és
magukat a Container App-okat egyszer létre kell hozni. Ehhez a legegyszerűbb az
Azure CLI (Cloud Shell-ben is futtatható, telepítés nélkül):

```bash
az group create --name healthtracker-rg --location westeurope

az containerapp env create \
  --name healthtracker-env \
  --resource-group healthtracker-rg \
  --location westeurope

# API - a Dockerfile 8080-as porton figyel
az containerapp create \
  --name healthtracker-api \
  --resource-group healthtracker-rg \
  --environment healthtracker-env \
  --image mcr.microsoft.com/k8se/quickstart:latest \
  --target-port 8080 \
  --ingress external \
  --min-replicas 0 \
  --max-replicas 1

# Frontend - az nginx image 80-as porton figyel
az containerapp create \
  --name healthtracker-web \
  --resource-group healthtracker-rg \
  --environment healthtracker-env \
  --image mcr.microsoft.com/k8se/quickstart:latest \
  --target-port 80 \
  --ingress external \
  --min-replicas 0 \
  --max-replicas 1
```

A `--min-replicas 0` a lényeg: forgalom nélkül az app leáll, nem számláz. A
kezdeti `mcr.microsoft.com/k8se/quickstart` csak placeholder – az első sikeres
pipeline-futás lecseréli a saját image-edre.

### 5. Variable group

**Pipelines → Library → + Variable group**, neve legyen pontosan `healthtracker-cd`
(erre hivatkozik a `variables: - group:` sor). Tartalma:

| Változó | Példa érték | Megjegyzés |
| --- | --- | --- |
| `dockerRegistryServiceConnection` | `healthtracker-ghcr` | a 2. pontban létrehozott service connection neve |
| `azureSubscription` | `healthtracker-azure` | a 3. pontban létrehozott ARM service connection neve |
| `containerRegistry` | `ghcr.io/<github-felhasznalonev>` | pl. `ghcr.io/danielnagy600` |
| `apiImageRepository` | `healthtracker-api` | image név a GHCR-ben |
| `frontendImageRepository` | `healthtracker-frontend` | image név a GHCR-ben |
| `containerAppsResourceGroup` | `healthtracker-rg` | a 4. pontban létrehozott resource group |
| `apiContainerAppName` | `healthtracker-api` | a 4. pontban létrehozott Container App neve |
| `frontendContainerAppName` | `healthtracker-web` | a 4. pontban létrehozott Container App neve |
| `apiBaseUrl` | `https://healthtracker-api.<random>.westeurope.azurecontainerapps.io` | **build-időben** égetjük a frontendbe (`VITE_API_BASE`); a pontos URL-t a `az containerapp show --name healthtracker-api --resource-group healthtracker-rg --query properties.configuration.ingress.fqdn` adja meg |

A variable groupnál a **Pipeline permissions** alatt engedélyezni kell a pipeline
hozzáférését, különben a futás „variable group not found" hibával áll le.

### 6. Environment

**Pipelines → Environments → New environment**, neve `healthtracker-production`.
Ide köthető approval / check, ha a deploy előtt kézi jóváhagyást szeretnél.

### 7. GHCR package láthatóság (fontos!)

Az **első sikeres pipeline-futás után** a package-ek (`healthtracker-api`,
`healthtracker-frontend`) alapból **privátak** lesznek a GitHub fiókodon. Mivel a
Container App nincs regisztrálva hitelesítővel a registry-hez, a képhúzás hibázni
fog, amíg a package-eket publikusra nem állítod:

**GitHub → profilod → Packages → az adott package → Package settings → Change
visibility → Public**

(Ha inkább privátban maradnának, a Container App-on `az containerapp registry set`
paranccsal regisztrálható a GHCR felhasználónév + PAT – ekkor a README 4. pontban
ezt is be kell állítani a `create` parancsokhoz.)

### 8. Pipeline-ok felvétele

**Pipelines → New pipeline → Azure Repos Git / GitHub → Existing Azure Pipelines YAML file**,
majd külön-külön a `/azure-pipelines.yml` és a `/.azuredevops/security-pipeline.yml`.

### 9. Az API konfigurációja (adatbázis-kapcsolat)

A `docker-compose.yml`-ben helyi Postgres konténer fut; Container Apps-hoz egy
külső elérhető adatbázis kell (pl. Azure Database for PostgreSQL Flexible Server –
ennek van "Burstable B1ms" tier-e alacsony áron, de nincs örökre ingyenes verziója;
gyakorláshoz akár egy ingyenes Neon/Supabase Postgres is megteszi). A connection
stringet a Container App-on kell beállítani:

```bash
az containerapp update \
  --name healthtracker-api \
  --resource-group healthtracker-rg \
  --set-env-vars \
    ConnectionStrings__Postgres="Host=...;Port=5432;Database=healthtracker;Username=...;Password=..." \
    Cors__AllowedOrigin="https://<frontend-fqdn>" \
    ASPNETCORE_ENVIRONMENT=Production
```

### 10. Költségvédelem

Bár a fenti setup a szabad kereten belül maradva $0, érdemes egy biztonsági hálót
beállítani: **Cost Management + Billing → Budgets → + Add**, alacsony összeggel
(pl. $5) és e-mail riasztással 50/80/100%-nál. Így ha véletlenül túllépnéd az
ingyenes keretet (pl. sok build/deploy vagy nagyobb forgalom), időben értesülsz.

## Ami átkerült a GitHub Actionsből

- `dotnet restore / build -warnaserror / test` → `backend-job.yml`, kiegészítve TRX
  teszteredmény-publikálással és NuGet cache-sel.
- `npm ci / lint / build` → `frontend-job.yml`, kiegészítve a `npm run test`
  (vitest) lépéssel és a `dist/` artifact-publikálással.
- CodeQL → `security-pipeline.yml` (MSDO). **Az MSDO task használatához telepíteni
  kell a „Microsoft Security DevOps" extensiont** a szervezetbe a Marketplace-ről.
  Valódi CodeQL csak GitHub Advanced Security for Azure DevOps licenccel érhető el –
  az ehhez tartozó lépések kikommentezve szerepelnek a fájlban.
