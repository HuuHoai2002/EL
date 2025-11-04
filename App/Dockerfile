# Stage 1: Build ứng dụng
FROM node:20-alpine AS build

ARG VITE_API_URL
ENV VITE_API_URL $VITE_API_URL

WORKDIR /app

# Copy package trước để cache dependency
COPY package*.json ./
RUN npm install

# Copy source code
COPY . .

# Build production
RUN npm run build


# Stage 2: Serve bằng Nginx
FROM nginx:alpine

# Xóa default config và copy config nginx mới
RUN rm /etc/nginx/conf.d/default.conf
COPY nginx.conf /etc/nginx/conf.d

# Copy build output từ stage 1
COPY --from=build /app/dist /usr/share/nginx/html

# Expose port
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
