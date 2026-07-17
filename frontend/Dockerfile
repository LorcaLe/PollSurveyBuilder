# syntax=docker/dockerfile:1

# ---- build: install deps and produce the static bundle ----
FROM node:20-alpine AS build
WORKDIR /app

# Vite inlines VITE_* variables at build time, so the API URL has to be
# supplied as a build arg (set it in the GitHub Actions workflow / docker build --build-arg).
ARG VITE_API_URL=http://localhost:5080
ENV VITE_API_URL=$VITE_API_URL

COPY package.json package-lock.json* ./
RUN npm install

COPY . .
RUN npm run build

# ---- final: nginx serving the static build ----
FROM nginx:1.27-alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
