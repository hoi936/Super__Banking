# Build stage
FROM node:24-alpine AS builder

WORKDIR /app

# Copy package manifests
COPY package*.json ./

# Install all dependencies
RUN npm install

# Copy application source
COPY . .

# Build production Nuxt application
RUN npm run build

# Production runtime stage
FROM node:24-alpine AS runner

WORKDIR /app

ENV NODE_ENV=production
ENV HOST=0.0.0.0
ENV PORT=3000

EXPOSE 3000

# Copy built application output from builder
COPY --from=builder /app/.output ./.output

CMD ["node", ".output/server/index.mjs"]
