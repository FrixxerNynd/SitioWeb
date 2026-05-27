using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Modelo para la tabla admConceptos de Adminpaq
    /// Depende de: admDocumentosModelo (CIDDOCUMENTODE)
    /// Depende de: admMonedas (CIDMONEDA)
    /// </summary>
    [Table("admConceptos")]
    public class AdmConcepto
    {
        [Key]
        [Column("CIDCONCEPTODOCUMENTO")]
        public int CIdConceptoDocumento { get; set; }

        [Required]
        [Column("CCODIGOCONCEPTO")]
        [StringLength(30)]
        public string CCodigoConcepto { get; set; } = string.Empty;

        [Required]
        [Column("CNOMBRECONCEPTO")]
        [StringLength(60)]
        public string CNombreConcepto { get; set; } = string.Empty;

        /// <summary>
        /// FK a admDocumentosModelo
        /// </summary>
        [Column("CIDDOCUMENTODE")]
        public int CIdDocumentoDe { get; set; }

        [Column("CNATURALEZA")]
        public int CNaturaleza { get; set; }

        [Column("CDOCTOACREDITO")]
        public int CDoctoACredito { get; set; }

        [Column("CTIPOFOLIO")]
        public int CTipoFolio { get; set; }

        [Column("CMAXIMOMOVTOS")]
        public int CMaximoMovtos { get; set; }

        [Column("CCREACLIENTE")]
        public int CCreaCliente { get; set; }

        [Column("CSUMARPROMOCIONES")]
        public int CSumarPromociones { get; set; }

        [Required]
        [Column("CFORMAPREIMPRESA")]
        [StringLength(253)]
        public string CFormaPreimpresa { get; set; } = string.Empty;

        [Column("CORDENCALCULO")]
        public int COrdenCalculo { get; set; }

        [Column("CUSANOMBRECTEPROV")]
        public int CUsaNombreCteProv { get; set; }

        [Column("CUSARFC")]
        public int CUsaRfc { get; set; }

        [Column("CUSAFECHAVENCIMIENTO")]
        public int CUsaFechaVencimiento { get; set; }

        [Column("CUSAFECHAENTREGARECEPCION")]
        public int CUsaFechaEntregaRecepcion { get; set; }

        [Column("CUSAMONEDA")]
        public int CUsaMoneda { get; set; }

        [Column("CUSATIPOCAMBIO")]
        public int CUsaTipoCambio { get; set; }

        [Column("CUSACODIGOAGENTE")]
        public int CUsaCodigoAgente { get; set; }

        [Column("CUSANOMBREAGENTE")]
        public int CUsaNombreAgente { get; set; }

        [Column("CUSADIRECCION")]
        public int CUsaDireccion { get; set; }

        [Column("CUSAREFERENCIA")]
        public int CUsaReferencia { get; set; }

        [Required]
        [Column("CSERIEPOROMISION")]
        [StringLength(11)]
        public string CSeriePorOmision { get; set; } = string.Empty;

        [Column("CANCHOCODIGOPRODUCTO")]
        public int CAnchoCodigoProducto { get; set; }

        [Column("CUSANOMBREPRODUCTO")]
        public int CUsaNombreProducto { get; set; }

        [Column("CANCHONOMBREPRODUCTO")]
        public int CAnchoNombreProducto { get; set; }

        [Column("CUSAALMACEN")]
        public int CUsaAlmacen { get; set; }

        [Column("CANCHOCODIGOALMACEN")]
        public int CAnchoCodigoAlmacen { get; set; }

        [Column("CANCHOIMPORTES")]
        public int CAnchoImportes { get; set; }

        [Column("CANCHOPORCENTAJES")]
        public int CAncoPorcentajes { get; set; }

        [Column("CANCHOUNIDADPESOMEDIDA")]
        public int CAnchoUnidadPesoMedida { get; set; }

        [Column("CUSAPRECIO")]
        public int CUsaPrecio { get; set; }

        [Column("CIDFORMULAPRECIO")]
        public int CIdFormulaPrecio { get; set; }

        [Column("CUSACOSTOCAPTURADO")]
        public int CUsaCostoCapturado { get; set; }

        [Column("CIDFORMULACOSTOCAPTURADO")]
        public int CIdFormulaCostoCapturado { get; set; }

        [Column("CUSAEXISTENCIA")]
        public int CUsaExistencia { get; set; }

        [Column("CUSANETO")]
        public int CUsaNeto { get; set; }

        [Column("CIDFORMULANETO")]
        public int CIdFormulaNeto { get; set; }

        [Column("CUSAPORCENTAJEIMPUESTO1")]
        public int CUsaPorcentajeImpuesto1 { get; set; }

        [Column("CIDFORMULAPORCIMPUESTO1")]
        public int CIdFormulaPorcImpuesto1 { get; set; }

        [Column("CUSAIMPUESTO1")]
        public int CUsaImpuesto1 { get; set; }

        [Column("CIDFORMULAIMPUESTO1")]
        public int CIdFormulaImpuesto1 { get; set; }

        [Column("CUSAPORCENTAJEIMPUESTO2")]
        public int CUsaPorcentajeImpuesto2 { get; set; }

        [Column("CIDFORMULAPORCIMPUESTO2")]
        public int CIdFormulaPorcImpuesto2 { get; set; }

        [Column("CUSAIMPUESTO2")]
        public int CUsaImpuesto2 { get; set; }

        [Column("CIDFORMULAIMPUESTO2")]
        public int CIdFormulaImpuesto2 { get; set; }

        [Column("CUSAPORCENTAJEIMPUESTO3")]
        public int CUsaPorcentajeImpuesto3 { get; set; }

        [Column("CIDFORMULAPORCIMPUESTO3")]
        public int CIdFormulaPorcImpuesto3 { get; set; }

        [Column("CUSAIMPUESTO3")]
        public int CUsaImpuesto3 { get; set; }

        [Column("CIDFORMULAIMPUESTO3")]
        public int CIdFormulaImpuesto3 { get; set; }

        [Column("CUSAPORCENTAJERETENCION1")]
        public int CUsaPorcentajeRetencion1 { get; set; }

        [Column("CIDFORMULAPORCRETENCION1")]
        public int CIdFormulaPorcRetencion1 { get; set; }

        [Column("CUSARETENCION1")]
        public int CUsaRetencion1 { get; set; }

        [Column("CIDFORMULARETENCION1")]
        public int CIdFormulaRetencion1 { get; set; }

        [Column("CUSAPORCENTAJERETENCION2")]
        public int CUsaPorcentajeRetencion2 { get; set; }

        [Column("CIDFORMULAPORCRETENCION2")]
        public int CIdFormulaPorcRetencion2 { get; set; }

        [Column("CUSARETENCION2")]
        public int CUsaRetencion2 { get; set; }

        [Column("CIDFORMULARETENCION2")]
        public int CIdFormulaRetencion2 { get; set; }

        [Column("CUSAPORCENTAJEDESCUENTO1")]
        public int CUsaPorcentajeDescuento1 { get; set; }

        [Column("CIDFORMULAPORCDESCUENTO1")]
        public int CIdFormulaPorcDescuento1 { get; set; }

        [Column("CUSADESCUENTO1")]
        public int CUsaDescuento1 { get; set; }

        [Column("CIDFORMULADESCUENTO1")]
        public int CIdFormulaDescuento1 { get; set; }

        [Column("CUSAPORCENTAJEDESCUENTO2")]
        public int CUsaPorcentajeDescuento2 { get; set; }

        [Column("CIDFORMULAPORCDESCUENTO2")]
        public int CIdFormulaPorcDescuento2 { get; set; }

        [Column("CUSADESCUENTO2")]
        public int CUsaDescuento2 { get; set; }

        [Column("CIDFORMULADESCUENTO2")]
        public int CIdFormulaDescuento2 { get; set; }

        [Column("CUSAPORCENTAJEDESCUENTO3")]
        public int CUsaPorcentajeDescuento3 { get; set; }

        [Column("CIDFORMULAPORCDESCUENTO3")]
        public int CIdFormulaPorcDescuento3 { get; set; }

        [Column("CUSADESCUENTO3")]
        public int CUsaDescuento3 { get; set; }

        [Column("CIDFORMULADESCUENTO3")]
        public int CIdFormulaDescuento3 { get; set; }

        [Column("CUSAPORCENTAJEDESCUENTO4")]
        public int CUsaPorcentajeDescuento4 { get; set; }

        [Column("CIDFORMULAPORCDESCUENTO4")]
        public int CIdFormulaPorcDescuento4 { get; set; }

        [Column("CUSADESCUENTO4")]
        public int CUsaDescuento4 { get; set; }

        [Column("CIDFORMULADESCUENTO4")]
        public int CIdFormulaDescuento4 { get; set; }

        [Column("CUSAPORCENTAJEDESCUENTO5")]
        public int CUsaPorcentajeDescuento5 { get; set; }

        [Column("CIDFORMULAPORCDESCUENTO5")]
        public int CIdFormulaPorcDescuento5 { get; set; }

        [Column("CUSADESCUENTO5")]
        public int CUsaDescuento5 { get; set; }

        [Column("CIDFORMULADESCUENTO5")]
        public int CIdFormulaDescuento5 { get; set; }

        [Column("CUSATOTAL")]
        public int CUsaTotal { get; set; }

        [Column("CANCHOREFERENCIA")]
        public int CAnchoReferencia { get; set; }

        [Column("CUSACLASIFICACIONMOVTO")]
        public int CUsaClasificacionMovto { get; set; }

        [Column("CANCHOVALORCLASIFICACION")]
        public int CAnchoValorClasificacion { get; set; }

        [Column("CIDFORMULATOTAL")]
        public int CIdFormulaTotal { get; set; }

        [Column("CUSADESCUENTODOC1")]
        public int CUsaDescuentoDoc1 { get; set; }

        [Column("CIDFORMULADESDOC1")]
        public int CIdFormulaDesDoc1 { get; set; }

        [Column("CUSADESCUENTODOC2")]
        public int CUsaDescuentoDoc2 { get; set; }

        [Column("CIDFORMULADESDOC2")]
        public int CIdFormulaDesDoc2 { get; set; }

        [Column("CUSAGASTO1")]
        public int CUsaGasto1 { get; set; }

        [Column("CIDFORMULAGASTO1")]
        public int CIdFormulaGasto1 { get; set; }

        [Column("CUSAGASTO2")]
        public int CUsaGasto2 { get; set; }

        [Column("CIDFORMULAGASTO2")]
        public int CIdFormulaGasto2 { get; set; }

        [Column("CUSAGASTO3")]
        public int CUsaGasto3 { get; set; }

        [Column("CIDFORMULAGASTO3")]
        public int CIdFormulaGasto3 { get; set; }

        [Column("CUSATEXTOEXTRA1")]
        public int CUsaTextoExtra1 { get; set; }

        [Column("CUSATEXTOEXTRA2")]
        public int CUsaTextoExtra2 { get; set; }

        [Column("CUSATEXTOEXTRA3")]
        public int CUsaTextoExtra3 { get; set; }

        [Column("CANCHOTEXTOEXTRA")]
        public int CAnchoTextoExtra { get; set; }

        [Column("CUSAFECHAEXTRA")]
        public int CUsaFechaExtra { get; set; }

        [Column("CANCHOFECHAEXTRA")]
        public int CAnchoFechaExtra { get; set; }

        [Column("CUSAIMPORTEEXTRA1")]
        public int CUsaImporteExtra1 { get; set; }

        [Column("CIDFORMULAEXTRA1")]
        public int CIdFormulaExtra1 { get; set; }

        [Column("CUSAIMPORTEEXTRA2")]
        public int CUsaImporteExtra2 { get; set; }

        [Column("CIDFORMULAEXTRA2")]
        public int CIdFormulaExtra2 { get; set; }

        [Column("CUSAIMPORTEEXTRA3")]
        public int CUsaImporteExtra3 { get; set; }

        [Column("CIDFORMULAEXTRA3")]
        public int CIdFormulaExtra3 { get; set; }

        [Column("CUSAIMPORTEEXTRA4")]
        public int CUsaImporteExtra4 { get; set; }

        [Column("CIDFORMULAEXTRA4")]
        public int CIdFormulaExtra4 { get; set; }

        [Column("CUSATEXTOEXTRA1DOC")]
        public int CUsaTextoExtra1Doc { get; set; }

        [Column("CUSATEXTOEXTRA2DOC")]
        public int CUsaTextoExtra2Doc { get; set; }

        [Column("CUSATEXTOEXTRA3DOC")]
        public int CUsaTextoExtra3Doc { get; set; }

        [Column("CUSAFECHAEXTRADOC")]
        public int CUsaFechaExtraDoc { get; set; }

        [Column("CUSAIMPORTEEXTRA1DOC")]
        public int CUsaImporteExtra1Doc { get; set; }

        [Column("CUSAIMPORTEEXTRA2DOC")]
        public int CUsaImporteExtra2Doc { get; set; }

        [Column("CUSAIMPORTEEXTRA3DOC")]
        public int CUsaImporteExtra3Doc { get; set; }

        [Column("CUSAIMPORTEEXTRA4DOC")]
        public int CUsaImporteExtra4Doc { get; set; }

        [Column("CUSAEXTRACOMOGASTO")]
        public int CUsaExtraComoGasto { get; set; }

        [Column("CUSAOBSERVACIONES")]
        public int CUsaObservaciones { get; set; }

        [Column("CPRESENTAFISCAL")]
        public int CPresentaFiscal { get; set; }

        [Column("CPRESENTAREFERENCIA")]
        public int CPresentaReferencia { get; set; }

        [Column("CPRESENTACONDICIONES")]
        public int CPresentaCondiciones { get; set; }

        [Column("CPRESENTAENVIO")]
        public int CPresentaEnvio { get; set; }

        [Column("CPRESENTADETALLE")]
        public int CPresentaDetalle { get; set; }

        [Column("CPRESENTAIMPRIMIR")]
        public int CPresentaImprimir { get; set; }

        [Column("CPRESENTAPAGAR")]
        public int CPresentaPagar { get; set; }

        [Column("CPRESENTASALDAR")]
        public int CPresentaSaldar { get; set; }

        [Column("CPRESENTADOCUMENTAR")]
        public int CPresentaDocumentar { get; set; }

        [Column("CPRESENTAGASTOSCOMPRA")]
        public int CPresentaGastosCompra { get; set; }

        [Required]
        [Column("CSEGCONTCONCEPTO")]
        [StringLength(50)]
        public string CSegContConcepto { get; set; } = string.Empty;

        [Column("CBANENCABEZADO")]
        public int CBanEncabezado { get; set; }

        [Column("CBANMOVIMIENTO")]
        public int CBanMovimiento { get; set; }

        [Column("CBANDESCUENTO")]
        public int CBanDescuento { get; set; }

        [Column("CBANIMPUESTO")]
        public int CBanImpuesto { get; set; }

        [Column("CBANACCIONAUTOMATICA")]
        public int CBanAccionAutomatica { get; set; }

        [Required]
        [Column("CTIMESTAMP")]
        [StringLength(23)]
        public string CTimestamp { get; set; } = string.Empty;

        [Column("CNOFOLIO")]
        public double CNoFolio { get; set; }

        [Column("CIDPROCESOSEGURIDAD")]
        public int CIdProcesoSeguridad { get; set; }

        [Column("CUSAGTOMOV")]
        public int CUsaGtoMov { get; set; }

        [Column("CUSASCMOV")]
        public int CUsaScMov { get; set; }

        [Column("CIDASTOCON")]
        public int CIdAstoCon { get; set; }

        [Required]
        [Column("CSCCPTO2")]
        [StringLength(50)]
        public string CScCpto2 { get; set; } = string.Empty;

        [Required]
        [Column("CSCCPTO3")]
        [StringLength(50)]
        public string CScCpto3 { get; set; } = string.Empty;

        [Required]
        [Column("CSCMOVTO")]
        [StringLength(50)]
        public string CScMovto { get; set; } = string.Empty;

        [Column("CIDCONAUTO")]
        public int CIdConAuto { get; set; }

        [Column("CIDALMASUM")]
        public int CIdAlmaSum { get; set; }

        [Column("CUSACOMVTA")]
        public int CUsaComVta { get; set; }

        [Column("CIDPRSEG02")]
        public int CIdPrSeg02 { get; set; }

        [Column("CIDPRSEG03")]
        public int CIdPrSeg03 { get; set; }

        [Column("CIDPRSEG04")]
        public int CIdPrSeg04 { get; set; }

        [Column("CIDPRSEG05")]
        public int CIdPrSeg05 { get; set; }

        [Column("CFORMAAJ01")]
        public int CFormaAj01 { get; set; }

        [Column("CIDPRSEG06")]
        public int CIdPrSeg06 { get; set; }

        [Column("CAPFORMULA")]
        public int CApFormula { get; set; }

        [Column("CESCFD")]
        public int CEsCfd { get; set; }

        [Column("CIDFIRMARL")]
        public int CIdFirmaRl { get; set; }

        [Column("CGDAPASSW")]
        public int CGdaPassw { get; set; }

        [Column("CEMITEYENT")]
        public int CEmiteYEnt { get; set; }

        [Column("CBANCFD")]
        public int CBanCfd { get; set; }

        [Required]
        [Column("CREPIMPCFD")]
        [StringLength(253)]
        public string CRepImpCfd { get; set; } = string.Empty;

        [Column("CIDDIRSUCU")]
        public int CIdDirSucu { get; set; }

        [Column("CBANDIRSUC")]
        public int CBanDirSuc { get; set; }

        [Column("CVERFACELE")]
        public int CVerFacEle { get; set; }

        [Column("CCALFECHAS")]
        public int CCalFechas { get; set; }

        [Column("CTIPCAMTR1")]
        public int CTipCamTr1 { get; set; }

        [Column("CTIPCAMTR2")]
        public int CTipCamTr2 { get; set; }

        [Column("CCONSOLIDA")]
        public int CConsolida { get; set; }

        [Column("CENVIODIG")]
        public int CEnvioDig { get; set; }

        [Column("CBANTRANS")]
        public int CBanTrans { get; set; }

        [Column("CCONFNOAPR")]
        public int CConfNoApr { get; set; }

        [Column("CNOAPROB")]
        public int CNoAprob { get; set; }

        [Column("CAUTOIMPR")]
        public int CAutoImpr { get; set; }

        [Column("CRECIBECFD")]
        public int CRecibeCfd { get; set; }

        [Column("CSISTORIG")]
        public int CSistOrig { get; set; }

        [Column("CIDCPTODE1")]
        public int CIdCptoDe1 { get; set; }

        [Column("CIDCPTODE2")]
        public int CIdCptoDe2 { get; set; }

        [Column("CIDCPTODE3")]
        public int CIdCptoDe3 { get; set; }

        [Required]
        [Column("CPLAMIGCFD")]
        [StringLength(253)]
        public string CPlaMigCfd { get; set; } = string.Empty;

        [Column("CIDPRSEG07")]
        public int CIdPrSeg07 { get; set; }

        [Column("CRESERVADO")]
        public int CReservado { get; set; }

        [Column("CVERREFER")]
        public int CVerRefer { get; set; }

        [Column("CVERDOCORI")]
        public int CVerDocOri { get; set; }

        [Column("CCBB")]
        public int CCbb { get; set; }

        [Column("CCARTAPOR")]
        public int CCartaPor { get; set; }

        [Column("CCOMPDONAT")]
        public int CCompDonat { get; set; }

        [Column("COBSXML")]
        public int CObsXml { get; set; }

        [Required]
        [Column("CRUTAENTREGA")]
        [StringLength(253)]
        public string CRutaEntrega { get; set; } = string.Empty;

        [Required]
        [Column("CPREFIJOCONCEPTO")]
        [StringLength(30)]
        public string CPrefijoConcepto { get; set; } = string.Empty;

        [Required]
        [Column("CREGIMFISC")]
        [StringLength(253)]
        public string CRegimFisc { get; set; } = string.Empty;

        [Column("CCOMPEDUCA")]
        public int CCompEduca { get; set; }

        [Required]
        [Column("CMETODOPAG")]
        [StringLength(100)]
        public string CMetodoPag { get; set; } = string.Empty;

        [Required]
        [Column("CVERESQUE")]
        [StringLength(6)]
        public string CVerEsque { get; set; } = string.Empty;

        [Required]
        [Column("CIDFIRMADSL")]
        [StringLength(40)]
        public string CIdFirmaDsl { get; set; } = string.Empty;

        [Required]
        [Column("CORDENCAPTURA")]
        [StringLength(52)]
        public string COrdenCaptura { get; set; } = string.Empty;

        [Column("CESTATUS")]
        public int CEstatus { get; set; }

        /// <summary>
        /// FK a admMonedas
        /// </summary>
        [Column("CIDMONEDA")]
        public int CIdMoneda { get; set; }

        [Column("CIDCUENTA")]
        public int CIdCuenta { get; set; }

        [Required]
        [Column("CCLAVESAT")]
        [StringLength(8)]
        public string CClaveSat { get; set; } = string.Empty;

        [Column("CIDPRSEG08")]
        public int CIdPrSeg08 { get; set; }

        [Column("CUSAOBJIMP")]
        public int CUsaObjImp { get; set; }

        // Propiedades de navegación
        [ForeignKey("CIdDocumentoDe")]
        public AdmDocumentoModelo? DocumentoModelo { get; set; }

        [ForeignKey("CIdMoneda")]
        public AdmMoneda? Moneda { get; set; }
    }
}
