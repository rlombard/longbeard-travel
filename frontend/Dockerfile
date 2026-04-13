# syntax=docker/dockerfile:1

FROM node:22-alpine AS build
WORKDIR /app
COPY ui/package*.json ./
RUN npm ci
COPY ui .
RUN npm run build

FROM nginx:1.27-alpine AS runtime
WORKDIR /usr/share/nginx/html
COPY --from=build /app/dist .
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY frontend/env.template.js /usr/share/nginx/html/env.template.js
COPY frontend/docker-entrypoint.sh /docker-entrypoint.d/40-env.sh
RUN chmod +x /docker-entrypoint.d/40-env.sh
EXPOSE 3000
CMD ["nginx", "-g", "daemon off;"]
