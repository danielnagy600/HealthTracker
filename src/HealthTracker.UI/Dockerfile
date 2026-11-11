# Több lépcsős build: előbb Node-dal legyártjuk a statikus Angular buildet,
# majd egy könnyű nginx image-ből szolgáljuk ki.
FROM node:24-alpine AS build
WORKDIR /app

# Előbb csak a package fájlokat másoljuk, hogy az `npm ci` réteg cache-elhető legyen.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Az API címe build-időben égődik be az `API_BASE` konstansba (angular.json → define),
# ezért build argként vehető át. Alapértelmezésben a helyi docker-compose beállítással
# egyezik (api: localhost:8080).
ARG API_BASE=http://localhost:8080
RUN npx ng build --define "API_BASE='${API_BASE}'"

FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
