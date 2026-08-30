# Build stage
# Angular 22 requires node ^22.22.3 || ^24.15.0 || >=26.0.0.
FROM node:24-alpine AS build
WORKDIR /app

COPY PlantKeeperWebApp/package.json PlantKeeperWebApp/package-lock.json ./
RUN npm ci

COPY PlantKeeperWebApp/ .
RUN npm run build

# Serve stage
FROM nginx:alpine
# The Angular application builder nests its output under dist/<project>/browser.
COPY --from=build /app/dist/PlantKeeperWebApp/browser /usr/share/nginx/html
COPY ci/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
