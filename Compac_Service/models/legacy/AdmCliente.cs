using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.legacy
{
    /// <summary>
    /// Modelo que representa la tabla admClientes del sistema COI
    /// Contiene información de clientes y proveedores
    /// </summary>
    [Table("admClientes")]
    public class AdmCliente
    {
        [Key]
        [Column("CIDCLIENTEPROVEEDOR")]
        public int CIdClienteProveedor { get; set; }

        [Column("CCODIGOCLIENTE")]
        [StringLength(30)]
        public string CCodigoCliente { get; set; } = string.Empty;

        [Column("CCODIGOALTERNO")]
        [StringLength(60)]
        public string CCodigoAlterno { get; set; } = string.Empty;

        [Column("CRAZONSOCIAL")]
        [StringLength(60)]
        public string CRazonSocial { get; set; } = string.Empty;

        [Column("CFECHAALTA")]
        public DateTime CFechaAlta { get; set; }

        [Column("CRFC")]
        [StringLength(20)]
        public string CRfc { get; set; } = string.Empty;

        [Column("CCURP")]
        [StringLength(20)]
        public string CCurp { get; set; } = string.Empty;

        [Column("CDENCOMERCIAL")]
        [StringLength(50)]
        public string CDenComercial { get; set; } = string.Empty;

        [Column("CREPLEGAL")]
        [StringLength(50)]
        public string CRepLegal { get; set; } = string.Empty;

        [Column("CIDMONEDA")]
        public int CIdMoneda { get; set; }

        [Column("CLISTAPRECIOCLIENTE")]
        public int CListaPrecioCliente { get; set; }

        [Column("CDESCUENTODOCTO")]
        public double CDescuentoDocto { get; set; }

        [Column("CDESCUENTOMOVTO")]
        public double CDescuentoMovto { get; set; }

        [Column("CBANVENTACREDITO")]
        public int CBanVentaCredito { get; set; }

        [Column("CIDVALORCLASIFCLIENTE1")]
        public int CIdValorClasifCliente1 { get; set; }

        [Column("CIDVALORCLASIFCLIENTE2")]
        public int CIdValorClasifCliente2 { get; set; }

        [Column("CIDVALORCLASIFCLIENTE3")]
        public int CIdValorClasifCliente3 { get; set; }

        [Column("CIDVALORCLASIFCLIENTE4")]
        public int CIdValorClasifCliente4 { get; set; }

        [Column("CIDVALORCLASIFCLIENTE5")]
        public int CIdValorClasifCliente5 { get; set; }

        [Column("CIDVALORCLASIFCLIENTE6")]
        public int CIdValorClasifCliente6 { get; set; }

        [Column("CTIPOCLIENTE")]
        public int CTipoCliente { get; set; }

        [Column("CESTATUS")]
        public int CEstatus { get; set; }

        [Column("CFECHABAJA")]
        public DateTime CFechaBaja { get; set; }

        [Column("CFECHAULTIMAREVISION")]
        public DateTime CFechaUltimaRevision { get; set; }

        [Column("CLIMITECREDITOCLIENTE")]
        public double CLimiteCreditoCliente { get; set; }

        [Column("CDIASCREDITOCLIENTE")]
        public int CDiasCreditoCliente { get; set; }

        [Column("CBANEXCEDERCREDITO")]
        public int CBanExcederCredito { get; set; }

        [Column("CDESCUENTOPRONTOPAGO")]
        public double CDescuentoProntoPago { get; set; }

        [Column("CDIASPRONTOPAGO")]
        public int CDiasProntoPago { get; set; }

        [Column("CINTERESMORATORIO")]
        public double CInteresMoratorio { get; set; }

        [Column("CDIAPAGO")]
        public int CDiaPago { get; set; }

        [Column("CDIASREVISION")]
        public int CDiasRevision { get; set; }

        [Column("CMENSAJERIA")]
        [StringLength(20)]
        public string CMensajeria { get; set; } = string.Empty;

        [Column("CCUENTAMENSAJERIA")]
        [StringLength(60)]
        public string CCuentaMensajeria { get; set; } = string.Empty;

        [Column("CDIASEMBARQUECLIENTE")]
        public int CDiasEmbarqueCliente { get; set; }

        [Column("CIDALMACEN")]
        public int CIdAlmacen { get; set; }

        [Column("CIDAGENTEVENTA")]
        public int CIdAgenteVenta { get; set; }

        [Column("CIDAGENTECOBRO")]
        public int CIdAgenteCobro { get; set; }

        [Column("CRESTRICCIONAGENTE")]
        public int CRestriccionAgente { get; set; }

        [Column("CIMPUESTO1")]
        public double CImpuesto1 { get; set; }

        [Column("CIMPUESTO2")]
        public double CImpuesto2 { get; set; }

        [Column("CIMPUESTO3")]
        public double CImpuesto3 { get; set; }

        [Column("CRETENCIONCLIENTE1")]
        public double CRetencionCliente1 { get; set; }

        [Column("CRETENCIONCLIENTE2")]
        public double CRetencionCliente2 { get; set; }

        [Column("CIDVALORCLASIFPROVEEDOR1")]
        public int CIdValorClasifProveedor1 { get; set; }

        [Column("CIDVALORCLASIFPROVEEDOR2")]
        public int CIdValorClasifProveedor2 { get; set; }

        [Column("CIDVALORCLASIFPROVEEDOR3")]
        public int CIdValorClasifProveedor3 { get; set; }

        [Column("CIDVALORCLASIFPROVEEDOR4")]
        public int CIdValorClasifProveedor4 { get; set; }

        [Column("CIDVALORCLASIFPROVEEDOR5")]
        public int CIdValorClasifProveedor5 { get; set; }

        [Column("CIDVALORCLASIFPROVEEDOR6")]
        public int CIdValorClasifProveedor6 { get; set; }

        [Column("CLIMITECREDITOPROVEEDOR")]
        public double CLimiteCreditoProveedor { get; set; }

        [Column("CDIASCREDITOPROVEEDOR")]
        public int CDiasCreditoProveedor { get; set; }

        [Column("CTIEMPOENTREGA")]
        public int CTiempoEntrega { get; set; }

        [Column("CDIASEMBARQUEPROVEEDOR")]
        public int CDiasEmbarqueProveedor { get; set; }

        [Column("CIMPUESTOPROVEEDOR1")]
        public double CImpuestoProveedor1 { get; set; }

        [Column("CIMPUESTOPROVEEDOR2")]
        public double CImpuestoProveedor2 { get; set; }

        [Column("CIMPUESTOPROVEEDOR3")]
        public double CImpuestoProveedor3 { get; set; }

        [Column("CRETENCIONPROVEEDOR1")]
        public double CRetencionProveedor1 { get; set; }

        [Column("CRETENCIONPROVEEDOR2")]
        public double CRetencionProveedor2 { get; set; }

        [Column("CBANINTERESMORATORIO")]
        public int CBanInteresMoratorio { get; set; }

        [Column("CCOMVENTAEXCEPCLIENTE")]
        public double CComVentaExcepCliente { get; set; }

        [Column("CCOMCOBROEXCEPCLIENTE")]
        public double CComCobroExcepCliente { get; set; }

        [Column("CBANPRODUCTOCONSIGNACION")]
        public int CBanProductoConsignacion { get; set; }

        [Column("CSEGCONTCLIENTE1")]
        [StringLength(50)]
        public string CSegContCliente1 { get; set; } = string.Empty;

        [Column("CSEGCONTCLIENTE2")]
        [StringLength(50)]
        public string CSegContCliente2 { get; set; } = string.Empty;

        [Column("CSEGCONTCLIENTE3")]
        [StringLength(50)]
        public string CSegContCliente3 { get; set; } = string.Empty;

        [Column("CSEGCONTCLIENTE4")]
        [StringLength(50)]
        public string CSegContCliente4 { get; set; } = string.Empty;

        [Column("CSEGCONTCLIENTE5")]
        [StringLength(50)]
        public string CSegContCliente5 { get; set; } = string.Empty;

        [Column("CSEGCONTCLIENTE6")]
        [StringLength(50)]
        public string CSegContCliente6 { get; set; } = string.Empty;

        [Column("CSEGCONTCLIENTE7")]
        [StringLength(50)]
        public string CSegContCliente7 { get; set; } = string.Empty;

        [Column("CSEGCONTPROVEEDOR1")]
        [StringLength(50)]
        public string CSegContProveedor1 { get; set; } = string.Empty;

        [Column("CSEGCONTPROVEEDOR2")]
        [StringLength(50)]
        public string CSegContProveedor2 { get; set; } = string.Empty;

        [Column("CSEGCONTPROVEEDOR3")]
        [StringLength(50)]
        public string CSegContProveedor3 { get; set; } = string.Empty;

        [Column("CSEGCONTPROVEEDOR4")]
        [StringLength(50)]
        public string CSegContProveedor4 { get; set; } = string.Empty;

        [Column("CSEGCONTPROVEEDOR5")]
        [StringLength(50)]
        public string CSegContProveedor5 { get; set; } = string.Empty;

        [Column("CSEGCONTPROVEEDOR6")]
        [StringLength(50)]
        public string CSegContProveedor6 { get; set; } = string.Empty;

        [Column("CSEGCONTPROVEEDOR7")]
        [StringLength(50)]
        public string CSegContProveedor7 { get; set; } = string.Empty;

        [Column("CTEXTOEXTRA1")]
        [StringLength(50)]
        public string CTextoExtra1 { get; set; } = string.Empty;

        [Column("CTEXTOEXTRA2")]
        [StringLength(50)]
        public string CTextoExtra2 { get; set; } = string.Empty;

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

        [Column("CBANDOMICILIO")]
        public int CBanDomicilio { get; set; }

        [Column("CBANCREDITOYCOBRANZA")]
        public int CBanCreditoYCobranza { get; set; }

        [Column("CBANENVIO")]
        public int CBanEnvio { get; set; }

        [Column("CBANAGENTE")]
        public int CBanAgente { get; set; }

        [Column("CBANIMPUESTO")]
        public int CBanImpuesto { get; set; }

        [Column("CBANPRECIO")]
        public int CBanPrecio { get; set; }

        [Column("CTIMESTAMP")]
        [StringLength(23)]
        public string CTimestamp { get; set; } = string.Empty;

        [Column("CFACTERC01")]
        public int CFacTerc01 { get; set; }

        [Column("CCOMVENTA")]
        public double CComVenta { get; set; }

        [Column("CCOMCOBRO")]
        public double CComCobro { get; set; }

        [Column("CIDMONEDA2")]
        public int CIdMoneda2 { get; set; }

        [Column("CEMAIL1")]
        [StringLength(60)]
        public string CEmail1 { get; set; } = string.Empty;

        [Column("CEMAIL2")]
        [StringLength(60)]
        public string CEmail2 { get; set; } = string.Empty;

        [Column("CEMAIL3")]
        [StringLength(60)]
        public string CEmail3 { get; set; } = string.Empty;

        [Column("CTIPOENTRE")]
        public int CTipoEntre { get; set; }

        [Column("CCONCTEEMA")]
        public int CConcTeEma { get; set; }

        [Column("CFTOADDEND")]
        public int CFtoAddend { get; set; }

        [Column("CIDCERTCTE")]
        public int CIdCertCte { get; set; }

        [Column("CENCRIPENT")]
        public int CEncripEnt { get; set; }

        [Column("CBANCFD")]
        public int CBanCfd { get; set; }

        [Column("CTEXTOEXTRA4")]
        [StringLength(50)]
        public string CTextoExtra4 { get; set; } = string.Empty;

        [Column("CTEXTOEXTRA5")]
        [StringLength(50)]
        public string CTextoExtra5 { get; set; } = string.Empty;

        [Column("CIMPORTEEXTRA5")]
        public double CImporteExtra5 { get; set; }

        [Column("CIDADDENDA")]
        public int CIdAddenda { get; set; }

        [Column("CCODPROVCO")]
        [StringLength(60)]
        public string CCodProvCo { get; set; } = string.Empty;

        [Column("CENVACUSE")]
        public int CEnvAcUse { get; set; }

        [Column("CCON1NOM")]
        [StringLength(60)]
        public string CCon1Nom { get; set; } = string.Empty;

        [Column("CCON1TEL")]
        [StringLength(15)]
        public string CCon1Tel { get; set; } = string.Empty;

        [Column("CQUITABLAN")]
        public int CQuitaBlan { get; set; }

        [Column("CFMTOENTRE")]
        public int CFmtoEntre { get; set; }

        [Column("CIDCOMPLEM")]
        public int CIdComplem { get; set; }

        [Column("CDESGLOSAI2")]
        public int CDesglosaI2 { get; set; }

        [Column("CLIMDOCTOS")]
        public int CLimDoctos { get; set; }

        [Column("CSITIOFTP")]
        [StringLength(60)]
        public string CSitioFtp { get; set; } = string.Empty;

        [Column("CUSRFTP")]
        [StringLength(60)]
        public string CUsrFtp { get; set; } = string.Empty;

        [Column("CMETODOPAG")]
        [StringLength(100)]
        public string CMetodoPag { get; set; } = string.Empty;

        [Column("CNUMCTAPAG")]
        [StringLength(100)]
        public string CNumCtaPag { get; set; } = string.Empty;

        [Column("CIDCUENTA")]
        public int CIdCuenta { get; set; }

        [Column("CUSOCFDI")]
        [StringLength(30)]
        public string CUsoCfdi { get; set; } = string.Empty;

        [Column("CREGIMFISC")]
        [StringLength(20)]
        public string CRegimFisc { get; set; } = string.Empty;

        [Column("CWHATSAPP")]
        [StringLength(15)]
        public string CWhatsapp { get; set; } = string.Empty;

        // Relación con domicilios
        public virtual ICollection<AdmDomicilio> Domicilios { get; set; } = new List<AdmDomicilio>();
    }
}
