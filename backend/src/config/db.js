import { PrismaClient } from '@prisma/client';

//Instanciar el cliente de Prisma
const prisma = new PrismaClient({
    log: ["Error", "Warn", "Info"],
})

//Manejar cierre de conexiones
process.on("SIGINT", async () => {
    await prisma.$disconnect();
    process.exit(0);
});

export default prisma;
