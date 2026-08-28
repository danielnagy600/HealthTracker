# Több lépcsős build: előbb Node-dal legyártjuk a statikus buildet,
# majd egy könnyű nginx image-ből szolgáljuk ki.
FROM node:22-alpine AS build
WORKDIR /app

# Előbb csak a package fájlokat másoljuk, hogy az `npm ci` réteg cache-elhető legyen.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# A Vite build-időben égeti be a VITE_* változókat – ezért build argként vehető át,
# alapértelmezésben a helyi docker-compose beállítással egyezik (api: localhost:8080).
ARG VITE_API_BASE=http://localhost:8080
ENV VITE_API_BASE=${VITE_API_BASE}
RUN npm run build

FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
