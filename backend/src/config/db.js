import { PrismaClient } from '@prisma/client'
import { PrismaMssql } from '@prisma/adapter-mssql'
import logger from '../utils/logger.js'

const adapter = new PrismaMssql({
  server: 'localhost',
  port: 1433,
  database: 'cabs_pruebas',
  user: 'sa',
  password: '0186',
  options: {
    encrypt: true,
    trustServerCertificate: true,
  },
})

const prisma = new PrismaClient({ adapter })

// Verificar conexión al arrancar
prisma.$connect()
  .then(() => logger.info('✅ Prisma conectado a SQL Server'))
  .catch((err) => {
    logger.error(`❌ Error al conectar Prisma: ${err.message}`)
    process.exit(1)  // Detener el servidor si no hay DB
  })

export default prisma