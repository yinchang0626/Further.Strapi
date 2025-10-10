# Strapi Dockerfile for Development and Testing
FROM node:20-alpine

# 設定工作目錄
WORKDIR /app

# 安裝 curl (用於 health check)
RUN apk add --no-cache curl

# 複製 package files
COPY package*.json ./

# 安裝相依套件
RUN npm ci --only=production

# 複製應用程式碼
COPY . .

# 建立必要的目錄
RUN mkdir -p .tmp

# 暴露 port
EXPOSE 1337

# 預設命令
CMD ["npm", "run", "start"]