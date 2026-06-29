FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci --legacy-peer-deps

COPY . .
RUN npx ng build --configuration=production

FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist/taskflow/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

RUN addgroup --system appgroup && adduser --system --ingroup appgroup --no-create-home appuser \
    && chown -R appuser:appgroup /usr/share/nginx/html /var/cache/nginx /var/log/nginx /etc/nginx/conf.d \
    && touch /var/run/nginx.pid && chown appuser:appgroup /var/run/nginx.pid

EXPOSE 8080

USER appuser
CMD ["nginx", "-g", "daemon off;"]
