using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Modelo para la tabla admDocumentos de Adminpaq
    /// Depende de: admDocumentosModelo (CIDDOCUMENTODE)
    /// Depende de: admConceptos (CIDCONCEPTODOCUMENTO)
    /// Depende de: admAgentes (CIDAGENTE)
    /// Depende de: admMonedas (CIDMONEDA, CIDMONEDCA)
    /// CIDCLIENTEPROVEEDOR apunta a admClientes o admProveedores dependiendo del contexto
    /// </summary>
    [Table("admDocumentos")]
    public class AdmDocumento
    {
        [Key]
        [Column("CIDDOCUMENTO")]
        public int CIdDocumento { get; set; }

        /// <summary>
        /// FK a admDocumentosModelo
        /// </summary>
        [Column("CIDDOCUMENTODE")]
        public int CIdDocumentoDe { get; set; }

        /// <summary>
        /// FK a admConceptos
        /// </summary>
        [Column("CIDCONCEPTODOCUMENTO")]
        public int CIdConceptoDocumento { get; set; }

        [Required]
        [Column("CSERIEDOCUMENTO")]
        [StringLength(11)]
        public string CSerieDocumento { get; set; } = string.Empty;

        [Column("CFOLIO")]
        public double CFolio { get; set; }

        [Column("CFECHA")]
        public DateTime CFecha { get; set; }

        /// <summary>
        /// FK a admClientes o admProveedores (tabla externa al schema actual)
        /// </summary>
        [Column("CIDCLIENTEPROVEEDOR")]
        public int CIdClienteProveedor { get; set; }

        [Required]
        [Column("CRAZONSOCIAL")]
        [StringLength(60)]
        public string CRazonSocial { get; set; } = string.Empty;

        [Required]
        [Column("CRFC")]
        [StringLength(20)]
        public string CRfc { get; set; } = string.Empty;

        /// <summary>
        /// FK a admAgentes
        /// </summary>
        [Column("CIDAGENTE")]
        public int CIdAgente { get; set; }

        [Column("CFECHAVENCIMIENTO")]
        public DateTime CFechaVencimiento { get; set; }

        [Column("CFECHAPRONTOPAGO")]
        public DateTime CFechaProntoPago { get; set; }

        [Column("CFECHAENTREGARECEPCION")]
        public DateTime CFechaEntregaRecepcion { get; set; }

        [Column("CFECHAULTIMOINTERES")]
        public DateTime CFechaUltimoInteres { get; set; }

        /// <summary>
        /// FK a admMonedas
        /// </summary>
        [Column("CIDMONEDA")]
        public int CIdMoneda { get; set; }

        [Column("CTIPOCAMBIO")]
        public double CTipoCambio { get; set; }

        [Required]
        [Column("CREFERENCIA")]
        [StringLength(20)]
        public string CReferencia { get; set; } = string.Empty;

        [Column("COBSERVACIONES")]
        public string? CObservaciones { get; set; }

        [Column("CNATURALEZA")]
        public int CNaturaleza { get; set; }

        [Column("CIDDOCUMENTOORIGEN")]
        public int CIdDocumentoOrigen { get; set; }

        [Column("CPLANTILLA")]
        public int CPlantilla { get; set; }

        [Column("CUSACLIENTE")]
        public int CUsaCliente { get; set; }

        [Column("CUSAPROVEEDOR")]
        public int CUsaProveedor { get; set; }

        [Column("CAFECTADO")]
        public int CAfectado { get; set; }

        [Column("CIMPRESO")]
        public int CImpreso { get; set; }

        [Column("CCANCELADO")]
        public int CCancelado { get; set; }

        [Column("CDEVUELTO")]
        public int CDevuelto { get; set; }

        [Column("CIDPREPOLIZA")]
        public int CIdPrepoliza { get; set; }

        [Column("CIDPREPOLIZACANCELACION")]
        public int CIdPrepolizaCancelacion { get; set; }

        [Column("CESTADOCONTABLE")]
        public int CEstadoContable { get; set; }

        [Column("CNETO")]
        public double CNeto { get; set; }

        [Column("CIMPUESTO1")]
        public double CImpuesto1 { get; set; }

        [Column("CIMPUESTO2")]
        public double CImpuesto2 { get; set; }

        [Column("CIMPUESTO3")]
        public double CImpuesto3 { get; set; }

        [Column("CRETENCION1")]
        public double CRetencion1 { get; set; }

        [Column("CRETENCION2")]
        public double CRetencion2 { get; set; }

        [Column("CDESCUENTOMOV")]
        public double CDescuentoMov { get; set; }

        [Column("CDESCUENTODOC1")]
        public double CDescuentoDoc1 { get; set; }

        [Column("CDESCUENTODOC2")]
        public double CDescuentoDoc2 { get; set; }

        [Column("CGASTO1")]
        public double CGasto1 { get; set; }

        [Column("CGASTO2")]
        public double CGasto2 { get; set; }

        [Column("CGASTO3")]
        public double CGasto3 { get; set; }

        [Column("CTOTAL")]
        public double CTotal { get; set; }

        [Column("CPENDIENTE")]
        public double CPendiente { get; set; }

        [Column("CTOTALUNIDADES")]
        public double CTotalUnidades { get; set; }

        [Column("CDESCUENTOPRONTOPAGO")]
        public double CDescuentoProntoPago { get; set; }

        [Column("CPORCENTAJEIMPUESTO1")]
        public double CPorcentajeImpuesto1 { get; set; }

        [Column("CPORCENTAJEIMPUESTO2")]
        public double CPorcentajeImpuesto2 { get; set; }

        [Column("CPORCENTAJEIMPUESTO3")]
        public double CPorcentajeImpuesto3 { get; set; }

        [Column("CPORCENTAJERETENCION1")]
        public double CPorcentajeRetencion1 { get; set; }

        [Column("CPORCENTAJERETENCION2")]
        public double CPorcentajeRetencion2 { get; set; }

        [Column("CPORCENTAJEINTERES")]
        public double CPorcentajeInteres { get; set; }

        [Required]
        [Column("CTEXTOEXTRA1")]
        [StringLength(50)]
        public string CTextoExtra1 { get; set; } = string.Empty;

        [Required]
        [Column("CTEXTOEXTRA2")]
        [StringLength(50)]
        public string CTextoExtra2 { get; set; } = string.Empty;

        [Required]
        [Column("CTEXTOEXTRA3")]
        [StringLength(50)]
        public string CTextoExtra3 { get; set; } = string.Empty;

        [Column("CFECHAEXTRA")]
        public DateTime CFechaExtra { get; set; }

        [Column("CIMPORTEEXTRA1")]
        public double CImporteExtra1 { get; set; }

        [Column("CIMPORTEEXTRA2")]
        public double CImporteExtra2 { get; set; }

        [Column("CIMPORTEEXTRA3")]
        public double CImporteExtra3 { get; set; }

        [Column("CIMPORTEEXTRA4")]
        public double CImporteExtra4 { get; set; }

        [Required]
        [Column("CDESTINATARIO")]
        [StringLength(60)]
        public string CDestinatario { get; set; } = string.Empty;

        [Required]
        [Column("CNUMEROGUIA")]
        [StringLength(60)]
        public string CNumeroGuia { get; set; } = string.Empty;

        [Required]
        [Column("CMENSAJERIA")]
        [StringLength(20)]
        public string CMensajeria { get; set; } = string.Empty;

        [Required]
        [Column("CCUENTAMENSAJERIA")]
        [StringLength(120)]
        public string CCuentaMensajeria { get; set; } = string.Empty;

        [Column("CNUMEROCAJAS")]
        public double CNumeroCajas { get; set; }

        [Column("CPESO")]
        public double CPeso { get; set; }

        [Column("CBANOBSERVACIONES")]
        public int CBanObservaciones { get; set; }

        [Column("CBANDATOSENVIO")]
        public int CBanDatosEnvio { get; set; }

        [Column("CBANCONDICIONESCREDITO")]
        public int CBanCondicionesCredito { get; set; }

        [Column("CBANGASTOS")]
        public int CBanGastos { get; set; }

        [Column("CUNIDADESPENDIENTES")]
        public double CUnidadesPendientes { get; set; }

        [Required]
        [Column("CTIMESTAMP")]
        [StringLength(23)]
        public string CTimestamp { get; set; } = string.Empty;

        [Column("CIMPCHEQPAQ")]
        public double CImpCheqPaq { get; set; }

        [Column("CSISTORIG")]
        public int CSistOrig { get; set; }

        /// <summary>
        /// FK a admMonedas (moneda de cancelación)
        /// </summary>
        [Column("CIDMONEDCA")]
        public int CIdMonedCa { get; set; }

        [Column("CTIPOCAMCA")]
        public double CTipoCamCa { get; set; }

        [Column("CESCFD")]
        public int CEsCfd { get; set; }

        [Column("CTIENECFD")]
        public int CTieneCfd { get; set; }

        [Required]
        [Column("CLUGAREXPE")]
        [StringLength(380)]
        public string CLugarExpe { get; set; } = string.Empty;

        [Required]
        [Column("CMETODOPAG")]
        [StringLength(100)]
        public string CMetodoPag { get; set; } = string.Empty;

        [Column("CNUMPARCIA")]
        public int CNumParcia { get; set; }

        [Column("CCANTPARCI")]
        public int CCantParci { get; set; }

        [Required]
        [Column("CCONDIPAGO")]
        [StringLength(253)]
        public string CCondiPago { get; set; } = string.Empty;

        [Required]
        [Column("CNUMCTAPAG")]
        [StringLength(100)]
        public string CNumCtaPag { get; set; } = string.Empty;

        [Required]
        [Column("CGUIDDOCUMENTO")]
        [StringLength(40)]
        public string CGuidDocumento { get; set; } = string.Empty;

        [Required]
        [Column("CUSUARIO")]
        [StringLength(15)]
        public string CUsuario { get; set; } = string.Empty;

        [Column("CIDPROYECTO")]
        public int CIdProyecto { get; set; }

        [Column("CIDCUENTA")]
        public int CIdCuenta { get; set; }

        [Required]
        [Column("CTRANSACTIONID")]
        [StringLength(26)]
        public string CTransactionId { get; set; } = string.Empty;

        [Column("CIDCOPIADE")]
        public int CIdCopiaDe { get; set; }

        [Required]
        [Column("CVERESQUE")]
        [StringLength(6)]
        public string CVerEsque { get; set; } = string.Empty;

        [Column("CIDAPERTURA")]
        public int CIdApertura { get; set; }

        // Propiedades de navegación
        [ForeignKey("CIdDocumentoDe")]
        public AdmDocumentoModelo? DocumentoModelo { get; set; }

        [ForeignKey("CIdConceptoDocumento")]
        public AdmConcepto? Concepto { get; set; }

        [ForeignKey("CIdAgente")]
        public AdmAgente? Agente { get; set; }

        [ForeignKey("CIdMoneda")]
        public AdmMoneda? Moneda { get; set; }
    }
}
