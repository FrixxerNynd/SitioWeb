// backend/app.js
import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import helmet from 'helmet';
import morgan from 'morgan';
import cookieParser from 'cookie-parser';
import swaggerUi from 'swagger-ui-express';
import swaggerSpec from './src/config/swagger.js';
import logger from './src/utils/Helpers/logger.js';
import cartRouter from './src/routes/cart.routes.js';
import exelRouter from './src/routes/exel.routes.js';
import orderRouter from './src/routes/order.routes.js';
import percentageRouter from './src/routes/percentage.routes.js';
import redisClient from './src/config/redis.js';
dotenv.config();

const app = express();
const PORT = process.env.PORT || 3000;

// ─────────────────────────────────────────────
// 1. Middlewares globales
// ─────────────────────────────────────────────
app.use(helmet());

// ✅ CORS CORREGIDO - Permitir solicitudes desde el frontend
const corsOptions = {
    origin: function (origin, callback) {
        // Permitir solicitudes sin origen (como Postman) o desde localhost:4200
        if (
            !origin ||
            origin === 'http://localhost:4200' ||
            origin === 'https://localhost:4200' ||
            origin === 'http://localhost:4205' ||
            origin === 'https://localhost:4205' ||
            origin === 'http://localhost:3000' ||
            origin === 'https://localhost:3000'
        ) {
            callback(null, true);
        } else {
            callback(new Error('No permitido por CORS'));
        }
    },
    methods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'OPTIONS'],
    allowedHeaders: ['Content-Type', 'X-Content-Type-Options', 'X-Frame-Options', 'x-xsrf-token', 'X-DNS-Prefetch-Control', 'X-Download-Options', 'X-Permitted-Cross-Domain-Policies', 'X-XSS-Protection', 'Referrer-Policy', 'Authorization', 'Cookie', 'X-Requested-With', 'Referrer-Policy', 'Sec-Fetch-Site', 'Sec-Fetch-Mode', 'Sec-Fetch-Dest'],
    credentials: true, // ← Necesario para cookies HttpOnly
    optionsSuccessStatus: 200
};

app.use(cors(corsOptions));

// Para depuración - log de todas las peticiones
app.use((req, res, next) => {
    console.log(`📥 ${req.method} ${req.url} - Origin: ${req.headers.origin}`);
    next();
});

app.use(express.json());
app.use(cookieParser());

// 2. Morgan → Winston
const morganStream = { write: (message) => logger.info(message.trim()) };
app.use(morgan('combined', { stream: morganStream }));

// ─────────────────────────────────────────────
// 3. Rutas utilitarias
// ─────────────────────────────────────────────

app.get('/health', (req, res) => {
    res.status(200).json({
        status: 'UP',
        timestamp: new Date().toISOString(),
        service: 'CABS Cart Service Backend',
    });
});

// ─────────────────────────────────────────────
// 4. Rutas del dominio
// ─────────────────────────────────────────────
app.use('/api/cart', cartRouter);
app.use('/api/productos', exelRouter);
app.use('/api/orders', orderRouter);
app.use('/api/porcentajes', percentageRouter);

// ─────────────────────────────────────────────
// 5. Documentación Swagger
// ─────────────────────────────────────────────
app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerSpec));

// ─────────────────────────────────────────────
// 6. Manejador de errores global
// ─────────────────────────────────────────────
app.use((err, req, res, next) => {
    logger.error(`${err.status || 500} - ${err.message} - ${req.originalUrl} - ${req.method} - ${req.ip}`);
    res.status(err.status || 500).json({
        success: false,
        message: process.env.NODE_ENV === 'production'
            ? 'Internal Server Error'
            : err.message,
    });
});

// ─────────────────────────────────────────────
// 7. Arranque del servidor
// ─────────────────────────────────────────────
async function startServer() {
    await redisClient.connect();

    app.listen(PORT, () => {
        logger.info(`🚀 Servidor iniciado en puerto ${PORT} - Entorno: ${process.env.NODE_ENV || 'development'}`);
        logger.info(`📖 Swagger disponible en http://localhost:${PORT}/api-docs`);
        logger.info(`🔐 Autenticación ${process.env.JWT_SECRET ? process.env.JWT_SECRET.substring(0, 5) + "..." : "UNDEFINED"}`);
    });
}

startServer();

const shutdown = async () => {
    logger.info('Cerrando servidor...');
    await redisClient.disconnect();
    process.exit(0);
};
process.on('SIGINT', shutdown);
process.on('SIGTERM', shutdown);