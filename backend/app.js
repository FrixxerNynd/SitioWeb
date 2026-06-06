import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import helmet from 'helmet';
import morgan from 'morgan';
import cookieParser from 'cookie-parser';
import swaggerUi from 'swagger-ui-express';
import swaggerSpec from './src/config/swagger.js';
import logger from './src/utils/logger.js';
import cartRouter from './src/routes/cart.routes.js';
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

/**
 * @swagger
 * /productos:
 *   get:
 *     summary: Listar todos los productos del catálogo Exel del Norte
 *     tags: [Productos]
 *     responses:
 *       200:
 *         description: Lista de productos obtenida exitosamente
 *         content:
 *           application/json:
 *             schema:
 *               type: object
 *               properties:
 *                 success:
 *                   type: boolean
 *                   example: true
 *                 message:
 *                   type: string
 *                 data:
 *                   type: array
 *                   items:
 *                     type: object
 *       500:
 *         description: Error al obtener productos
 */
app.get('/productos', async (req, res) => {
    try {
        const data = await ExelService.fetchExternalProducts();
        res.json({
            success: true,
            message: 'Productos obtenidos exitosamente',
            data: data || []
        });
    } catch (error) {
        res.status(500).json({ success: false, error: error.message });
    }
});

// ─────────────────────────────────────────────
// 4. Rutas del dominio
// ─────────────────────────────────────────────
app.use('/api/cart', cartRouter);

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
        error: {
            message: process.env.NODE_ENV === 'production'
                ? 'Internal Server Error'
                : err.message,
        }
    });
});

// ─────────────────────────────────────────────
// 7. Arranque del servidor
// ─────────────────────────────────────────────
app.listen(PORT, () => {
    logger.info(`🚀 Servidor iniciado en puerto ${PORT} - Entorno: ${process.env.NODE_ENV || 'development'}`);
    logger.info(`📖 Swagger disponible en http://localhost:${PORT}/api-docs`);
    logger.info(`🔐 Autenticación ${process.env.JWT_SECRET ? process.env.JWT_SECRET.substring(0, 5) + "..." : "UNDEFINED"}`);
});

// Cierre graceful
process.on('SIGINT',  () => { logger.info('Cerrando servidor...'); process.exit(0); });
process.on('SIGTERM', () => { logger.info('Cerrando servidor...'); process.exit(0); });