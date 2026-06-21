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
app.use(cors({
    origin: process.env.CORS_ORIGIN || '*',
    methods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE'],
    allowedHeaders: ['Content-Type', 'Authorization'],
    credentials: true
}));
app.use(express.json());
app.use(cookieParser());

// 2. Morgan → Winston
const morganStream = { write: (message) => logger.info(message.trim()) };
app.use(morgan('combined', { stream: morganStream }));

// ─────────────────────────────────────────────
// 3. Rutas utilitarias
// ─────────────────────────────────────────────

/**
 * @swagger
 * /health:
 *   get:
 *     summary: Estado del servidor
 *     tags: [Sistema]
 *     responses:
 *       200:
 *         description: Servicio en línea
 */
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
// 6. Manejador de errores global (siempre al final)
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
    // Conectar a Redis antes de aceptar peticiones
    await redisClient.connect();

    app.listen(PORT, () => {
        logger.info(`🚀 Servidor iniciado en puerto ${PORT} - Entorno: ${process.env.NODE_ENV || 'development'}`);
        logger.info(`📖 Swagger disponible en http://localhost:${PORT}/api-docs`);
        logger.info(`🔐 Autenticación ${process.env.JWT_SECRET ? process.env.JWT_SECRET.substring(0, 5) + "..." : "UNDEFINED"}`);
    });
}

startServer();

// Cierre graceful
const shutdown = async () => {
    logger.info('Cerrando servidor...');
    await redisClient.disconnect();
    process.exit(0);
};
process.on('SIGINT', shutdown);
process.on('SIGTERM', shutdown);