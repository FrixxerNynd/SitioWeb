import dotenv from"dotenv";
import { defineConfig } from "prisma/config";

dotenv.config();
export default defineConfig({
  schema: "src/models/schema.prisma",
  migrations: {
    path: "prisma/migrations",
  },
  datasource: {
    url: process.env.DATABASE_URL,
  },
});
