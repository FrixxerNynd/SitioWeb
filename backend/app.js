import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import helmet from 'helmet';
import morgan from 'morgan';
import swaggerUi from 'swagger-ui-express';
import swaggerSpec from './src/config/swagger.js';
import logger from './src/utils/logger.js';
import redisClient from './src/config/redis.js';
import ExelService from './src/services/exel.service.js';

dotenv.config();

const app = express();
const PORT = process.env.PORT || 3000;

//1.- Middlewares
app.use(helmet());
app.use(cors({
    origin: process.env.CORS_ORIGIN || '*',
    methods: ['GET', 'POST', 'PUT', 'DELETE'],
    allowedHeaders: ['Content-Type', 'Authorization']
}));

app.use(express.json());

//2.- Morgan para logging de solicitudes HTTP
const morganStream = {
    write: (message) => logger.info(message.trim())
};
app.use(morgan('combined', { stream: morganStream}));

//3.- Rutas de control y Health
app.get('/health', (req, res) => {
    res.status(200).json({
        status: 'UP',
        timestamp: new Date().toISOString(),
        service: 'Catalog Service Backend',
        redis: redisClient.isConnected ? 'connected ✅' : 'disconnected ⚠️'
    });
});

// Documentación API
app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerSpec));

//4.- Manejo de errores global
app.use((err, req, res, next) => {
    logger.error(`${err.status || 500} - ${err.message} - ${req.originalUrl} - ${req.method} - ${req.ip}`);
    res.status(err.status || 500).json({
        error: {
            message: process.env.NODE_ENV === 'production' ? 'Internal Server Error' : err.message,
        }
    });
});

/// end poit para ver si redis si funciona 
app.get('/test-redis', async (req, res) => {
    // const { id = '100' } = req.query;
    
    try {
        // Importar el servicio de Exel
        
        const startTime = Date.now();
        const data = await ExelService.fetchExternalProductsWithCache();
        const endTime = Date.now();
        
        const product = data?.datos?.find();
        
        res.json({
            success: true,
            message: 'Prueba de caché con Redis',
            productId: id,
            productFound: !!product,
            productName: product?.nombre || 'No encontrado',
            responseTimeMs: endTime - startTime,
            fromCache: endTime - startTime < 50 ? '✅ (Redis - Caché funcionando)' : '❌ (API externa - primera vez)'
        });
    } catch (error) {
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

app.get('/productos', async (req, res) =>{
    try {
        const data = await ExelService.fetchExternalProductsWithCache();
        res.json({
            success: true,
            message: 'Productos obtenidos con api',
            data: data?.datos || []
        });
    } catch (error) {
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});


async function startServer() {
    // Intentar conectar a Redis pero NO dejar que detenga el servidor
    try {
        await redisClient.connect();
        logger.info('✅ Redis conectado correctamente');
    } catch (error) {
        logger.warn('⚠️ Redis no disponible, el servidor funcionará sin caché');
        // El servidor sigue corriendo aunque Redis falle
    }
    
    // Iniciar el servidor SIEMPRE (con o sin Redis)
    app.listen(PORT, () => {
        logger.info(`Servidor iniciado en el puerto ${PORT} - Entorno: ${process.env.NODE_ENV || 'development'}`);
        logger.info(`📖 Documentación API disponible en http://localhost:${PORT}/api-docs`);
        logger.info(`📊 Estado de Redis: ${redisClient.isConnected ? 'Conectado ✅' : 'No disponible ⚠️'}`);
    });
}

// Iniciar el servidor
startServer();

// Cierre graceful
process.on('SIGINT', async () => {
    logger.info('Cerrando servidor...');
    await redisClient.disconnect();
    process.exit(0);
});

process.on('SIGTERM', async () => {
    logger.info('Cerrando servidor...');
    await redisClient.disconnect();
    process.exit(0);
});