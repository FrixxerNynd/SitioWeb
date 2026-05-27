// =====================================================================================
// CONTEXTO LEGACY COMPAC WRITE - LegacyCompacWriteContext.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// DbContext de ESCRITURA para la base de datos legacy adCABS2016 en instancia COMPAC.
// Proporciona acceso completo (lectura/escritura) a tablas del sistema anterior.
//
// PROPÓSITO:
// - Operaciones de escritura en datos legacy (si fueran necesarias)
// - Change Tracking habilitado para operaciones de modificación
// - Separación de responsabilidades: este contexto es PARA ESCRITURA
//
// CONFIGURACIÓN:
// - ConnectionString: CompacConnection (Adrian\COMPAC - adCABS2016)
// - QueryTrackingBehavior: TrackAll (por defecto - permite escritura)
// - Timeout: 30 segundos para operaciones complejas
//
// ⚠️ USO CON PRECAUCIÓN: Modificar datos legacy puede afectar integridad del sistema
//
// =====================================================================================

using back_cabs.CRM.models.legacy;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.contexts
{
    /// <summary>
    /// Contexto de base de datos para operaciones de escritura en sistema legacy adCABS2016
    /// PERMITE MODIFICACIONES - Usar con extrema precaución
    /// </summary>
    public class LegacyCompacWriteContext : DbContext
    {
        public LegacyCompacWriteContext(DbContextOptions<LegacyCompacWriteContext> options) : base(options)
        {
        }

        // ═══════════════════════════════════════════════════════════════
        // DBSETS - TABLAS LEGACY
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Tabla admAgentes del sistema legacy
        /// Contiene el catálogo completo de agentes/vendedores
        /// </summary>
        public DbSet<AdmAgente> AdmAgentes { get; set; }

        /// <summary>
        /// Tabla admClientes del sistema legacy
        /// Contiene el catálogo completo de clientes y proveedores
        /// </summary>
        public DbSet<AdmCliente> AdmClientes { get; set; }

        /// <summary>
        /// Tabla admDomicilios del sistema legacy
        /// Contiene domicilios asociados a clientes/proveedores (relacionados por CIdCatalogo)
        /// </summary>
        public DbSet<AdmDomicilio> AdmDomicilios { get; set; }

        /// <summary>
        /// Tabla admMonedas del sistema legacy
        /// Catálogo de monedas (MXN, USD, EUR, etc.)
        /// </summary>
        public DbSet<AdmMoneda> AdmMonedas { get; set; }

        /// <summary>
        /// Tabla admAlmacenes del sistema legacy
        /// Catálogo de almacenes/ubicaciones
        /// </summary>
        public DbSet<AdmAlmacen> AdmAlmacenes { get; set; }

        /// <summary>
        /// Tabla admUnidadesMedidaPeso del sistema legacy
        /// Catálogo de unidades de medida (KG, PZA, LT, etc.)
        /// </summary>
        public DbSet<AdmUnidadMedidaPeso> AdmUnidadesMedidaPeso { get; set; }

        /// <summary>
        /// Tabla admProductos del sistema legacy
        /// Catálogo de productos/artículos
        /// </summary>
        public DbSet<AdmProducto> AdmProductos { get; set; }

        /// <summary>
        /// Tabla admDocumentosModelo del sistema legacy
        /// Modelos de documentos (Factura, Nota de Crédito, etc.)
        /// </summary>
        public DbSet<AdmDocumentoModelo> AdmDocumentosModelo { get; set; }

        /// <summary>
        /// Tabla admConceptos del sistema legacy
        /// Conceptos de documentos (configuración de tipos de documentos)
        /// </summary>
        public DbSet<AdmConcepto> AdmConceptos { get; set; }

        /// <summary>
        /// Tabla admDocumentos del sistema legacy
        /// Documentos de ventas/compras (facturas, notas, etc.)
        /// </summary>
        public DbSet<AdmDocumento> AdmDocumentos { get; set; }

        /// <summary>
        /// Tabla admMovimientos del sistema legacy
        /// Movimientos de inventario asociados a documentos
        /// </summary>
        public DbSet<AdmMovimiento> AdmMovimientos { get; set; }

        /// <summary>
        /// Tabla admNumerosSerie del sistema legacy
        /// Números de serie de productos
        /// </summary>
        public DbSet<AdmNumeroSerie> AdmNumerosSerie { get; set; }

        /// <summary>
        /// Tabla admMovimientosSerie del sistema legacy
        /// Relación entre movimientos y números de serie
        /// </summary>
        public DbSet<AdmMovimientoSerie> AdmMovimientosSerie { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // CONFIGURACIÓN DEL CONTEXTO
        // ═══════════════════════════════════════════════════════════════

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Configuración por defecto si no se inyecta desde Program.cs
                optionsBuilder.UseSqlServer(
                    "Server=Adrian\\COMPAC;Database=adCABS2016;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;",
                    sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(30); // 30 segundos para operaciones complejas
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null
                        );
                    }
                );
            }

            // ✅ ESCRITURA: Change Tracking habilitado (comportamiento por defecto)
            // optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ═══════════════════════════════════════════════════════════════
            // CONFIGURACIÓN DE ENTIDAD: AdmAgente
            // ═══════════════════════════════════════════════════════════════

            modelBuilder.Entity<AdmAgente>(entity =>
            {
                // Configuración de tabla
                entity.ToTable("admAgentes", "dbo");
                entity.HasKey(e => e.CIdAgente);

                // Configuración de columnas con tipos explícitos
                entity.Property(e => e.CIdAgente)
                    .HasColumnName("CIDAGENTE")
                    .HasColumnType("int")
                    .IsRequired();

                entity.Property(e => e.CCodigoAgente)
                    .HasColumnName("CCODIGOAGENTE")
                    .HasColumnType("varchar(30)")
                    .IsRequired();

                entity.Property(e => e.CNombreAgente)
                    .HasColumnName("CNOMBREAGENTE")
                    .HasColumnType("varchar(60)")
                    .IsRequired();

                entity.Property(e => e.CFechaAltaAgente)
                    .HasColumnName("CFECHAALTAAGENTE")
                    .HasColumnType("datetime")
                    .IsRequired();

                entity.Property(e => e.CTipoAgente)
                    .HasColumnName("CTIPOAGENTE")
                    .HasColumnType("int")
                    .IsRequired();

                entity.Property(e => e.CComisionVentaAgente)
                    .HasColumnName("CCOMISIONVENTAAGENTE")
                    .HasColumnType("float")
                    .IsRequired();

                entity.Property(e => e.CComisionCobroAgente)
                    .HasColumnName("CCOMISIONCOBROAGENTE")
                    .HasColumnType("float")
                    .IsRequired();

                entity.Property(e => e.CIdCliente)
                    .HasColumnName("CIDCLIENTE")
                    .HasColumnType("int")
                    .IsRequired();

                entity.Property(e => e.CIdProveedor)
                    .HasColumnName("CIDPROVEEDOR")
                    .HasColumnType("int")
                    .IsRequired();

                entity.Property(e => e.CIdValorClasificacion1)
                    .HasColumnName("CIDVALORCLASIFICACION1")
                    .HasColumnType("int")
                    .IsRequired();

                entity.Property(e => e.CIdValorClasificacion2)
                    .HasColumnName("CIDVALORCLASIFICACION2")
                    .HasColumnType("int")
                    .IsRequired();

                entity.Property(e => e.CIdValorClasificacion3)
                    .HasColumnName("CIDVALORCLASIFICACION3")
                    .HasColumnType("int")
                    .IsRequired();

                entity.Property(e => e.CIdValorClasificacion4)
                    .HasColumnName("CIDVALORCLASIFICACION4")
                    .HasColumnType("int")
                    .IsRequired();

                entity.Property(e => e.CIdValorClasificacion5)
                    .HasColumnName("CIDVALORCLASIFICACION5")
                    .HasColumnType("int")
                    .IsRequired();

                entity.Property(e => e.CIdValorClasificacion6)
                    .HasColumnName("CIDVALORCLASIFICACION6")
                    .HasColumnType("int")
                    .IsRequired();

                entity.Property(e => e.CSegContAgente)
                    .HasColumnName("CSEGCONTAGENTE")
                    .HasColumnType("varchar(50)")
                    .IsRequired();

                entity.Property(e => e.CTextoExtra1)
                    .HasColumnName("CTEXTOEXTRA1")
                    .HasColumnType("varchar(50)")
                    .IsRequired();

                entity.Property(e => e.CTextoExtra2)
                    .HasColumnName("CTEXTOEXTRA2")
                    .HasColumnType("varchar(50)")
                    .IsRequired();

                entity.Property(e => e.CTextoExtra3)
                    .HasColumnName("CTEXTOEXTRA3")
                    .HasColumnType("varchar(50)")
                    .IsRequired();

                entity.Property(e => e.CFechaExtra)
                    .HasColumnName("CFECHAEXTRA")
                    .HasColumnType("datetime")
                    .IsRequired();

                entity.Property(e => e.CImporteExtra1)
                    .HasColumnName("CIMPORTEEXTRA1")
                    .HasColumnType("float")
                    .IsRequired();

                entity.Property(e => e.CImporteExtra2)
                    .HasColumnName("CIMPORTEEXTRA2")
                    .HasColumnType("float")
                    .IsRequired();

                entity.Property(e => e.CImporteExtra3)
                    .HasColumnName("CIMPORTEEXTRA3")
                    .HasColumnType("float")
                    .IsRequired();

                entity.Property(e => e.CImporteExtra4)
                    .HasColumnName("CIMPORTEEXTRA4")
                    .HasColumnType("float")
                    .IsRequired();

                entity.Property(e => e.CTimestamp)
                    .HasColumnName("CTIMESTAMP")
                    .HasColumnType("varchar(23)")
                    .IsRequired();

                entity.Property(e => e.CScAgente2)
                    .HasColumnName("CSCAGENTE2")
                    .HasColumnType("varchar(50)")
                    .IsRequired();

                entity.Property(e => e.CScAgente3)
                    .HasColumnName("CSCAGENTE3")
                    .HasColumnType("varchar(50)")
                    .IsRequired();
            });

            modelBuilder.Entity<AdmCliente>(entity =>
            {
                entity.ToTable("admClientes", "dbo");
                entity.HasKey(e => e.CIdClienteProveedor);

                entity.Property(e => e.CFechaAlta)
                    .HasColumnName("CFECHAALTA")
                    .HasColumnType("datetime");

                entity.Property(e => e.CFechaBaja)
                    .HasColumnName("CFECHABAJA")
                    .HasColumnType("datetime"); 

                entity.Property(e => e.CFechaUltimaRevision)
                    .HasColumnName("CFECHAULTIMAREVISION")
                    .HasColumnType("datetime");
                
                entity.Property(e => e.CFechaExtra)
                    .HasColumnName("CFECHAEXTRA")
                    .HasColumnType("datetime");
            });

            // ═══════════════════════════════════════════════════════════════
            // CONFIGURACIÓN DE ENTIDAD: AdmMovimientoSerie
            // ═══════════════════════════════════════════════════════════════

            modelBuilder.Entity<AdmMovimientoSerie>(entity =>
            {
                entity.ToTable("admMovimientosSerie", "dbo");
                
                // Clave primaria compuesta
                entity.HasKey(e => new { e.CIdMovimiento, e.CIdSerie });

                entity.Property(e => e.CIdAutoIncSql)
                    .HasColumnName("CIDAUTOINCSQL")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CFecha)
                    .HasColumnName("CFECHA")
                    .HasColumnType("datetime");

                // Relaciones con navegación 
                entity.HasOne(e => e.Movimiento)        
                    .WithMany()                         
                    .HasForeignKey(e => e.CIdMovimiento)
                    .IsRequired(false);

                entity.HasOne(e => e.NumeroSerie)       
                    .WithMany()                         
                    .HasForeignKey(e => e.CIdSerie)     
                    .IsRequired(false);
            });

            // ═══════════════════════════════════════════════════════════════
            // CONFIGURACIÓN SIMPLIFICADA PARA TABLAS ADICIONALES
            // Se usan las anotaciones Data Annotations de los modelos
            // ═══════════════════════════════════════════════════════════════

            // Configurar ignorar propiedades de navegación para evitar ciclos de carga
            modelBuilder.Entity<AdmMoneda>(entity =>
            {
                entity.ToTable("admMonedas", "dbo");
                entity.HasKey(e => e.CIdMoneda);
            });

            modelBuilder.Entity<AdmAlmacen>(entity =>
            {
                entity.ToTable("admAlmacenes", "dbo");
                entity.HasKey(e => e.CIdAlmacen);
            });

            modelBuilder.Entity<AdmUnidadMedidaPeso>(entity =>
            {
                entity.ToTable("admUnidadesMedidaPeso", "dbo");
                entity.HasKey(e => e.CIdUnidad);
            });

            modelBuilder.Entity<AdmProducto>(entity =>
            {
                entity.ToTable("admProductos", "dbo");
                entity.HasKey(e => e.CIdProducto);
                
                // Ignorar navegación para evitar ciclos
                entity.Ignore(e => e.UnidadBase);
                entity.Ignore(e => e.Moneda);
            });

            modelBuilder.Entity<AdmDocumentoModelo>(entity =>
            {
                entity.ToTable("admDocumentosModelo", "dbo");
                entity.HasKey(e => e.CIdDocumentoDe);
            });

            modelBuilder.Entity<AdmConcepto>(entity =>
            {
                entity.ToTable("admConceptos", "dbo");
                entity.HasKey(e => e.CIdConceptoDocumento);
                
                // Ignorar navegación para evitar ciclos
                entity.Ignore(e => e.DocumentoModelo);
                entity.Ignore(e => e.Moneda);
            });

            modelBuilder.Entity<AdmDocumento>(entity =>
            {
                entity.ToTable("admDocumentos", "dbo");
                entity.HasKey(e => e.CIdDocumento);
                
                entity.Property(e => e.CIdDocumento)
                    .ValueGeneratedNever();

                // Ignorar navegación para evitar ciclos
                entity.Ignore(e => e.DocumentoModelo);
                entity.Ignore(e => e.Concepto);
                entity.Ignore(e => e.Agente);
                entity.Ignore(e => e.Moneda);
            });

            modelBuilder.Entity<AdmMovimiento>(entity =>
            {
                entity.ToTable("admMovimientos", "dbo");
                entity.HasKey(e => e.CIdMovimiento);

                entity.Property(e => e.CIdMovimiento)
                    .ValueGeneratedNever();
                
                // Ignorar navegación para evitar ciclos
                entity.Ignore(e => e.Documento);
                entity.Ignore(e => e.Producto);
                entity.Ignore(e => e.Almacen);
                entity.Ignore(e => e.Unidad);
            });

            modelBuilder.Entity<AdmNumeroSerie>(entity =>
            {
                entity.ToTable("admNumerosSerie", "dbo");
                entity.HasKey(e => e.CIdSerie);
                
                // Ignorar navegación para evitar ciclos
                entity.Ignore(e => e.Producto);
                entity.Ignore(e => e.Almacen);
            });
        }
    }
}
