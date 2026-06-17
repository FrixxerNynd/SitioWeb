import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UiBoton } from '../../../../../components/shared/boton/boton';
import { UiIconComponent } from '../../../../../components/shared/icono/icono.component';

@Component({
    selector: 'app-pague-detalles-pre-registro',
    standalone: true,
    imports: [CommonModule, FormsModule, UiBoton, UiIconComponent],
    templateUrl: './detalles-pre-registro.html',
    styleUrl: './detalles-pre-registro.css',
})
export class PagueDetallesPreRegistro implements OnInit {
    cliente: any = null;
    loadingData: boolean = true;
    errorCarga: boolean = false;

    constructor(
        private route: ActivatedRoute,
        private router: Router
    ) { }

    clientes: any[] = [
        {
            id: 'CLI-001',
            nombre: 'José Manuel',
            apellidoPaterno: 'Díez',
            apellidoMaterno: 'Calderón',
            telefonoFijo: '6183316933',
            telefonoWhatsApp: '6183316933',
            rfc: 'LDMG270120DX2',
            estado: 'Pendiente',
            estadoContpaqi: 'SinEnlazar',
            fechaRegistro: '2024-01-15',
            fechaRegistroFormateada: '15 ene de 2024',
            direccion: {
                pais: 'México',
                codigoPostal: '30040',
                estado: 'Durango',
                municipio: 'Victoria Durango',
                calle: 'Calle de los Olivos #452',
                numeroExterior: '#129',
                numeroInterior: '#190',
            },
            cuenta: {
                correo: 'josemanueltadeo2003@gmail.com',
            },
            credito: null,
            constanciaFiscal: {
                disponible: false,
                url: null,
            },
        },
        {
            id: 'CLI-002',
            nombre: 'María Fernanda',
            apellidoPaterno: 'López',
            apellidoMaterno: 'Ramírez',
            telefonoFijo: '5551234567',
            telefonoWhatsApp: '5559876543',
            rfc: 'LOMR850620ABC',
            estado: 'Aprobado',
            estadoContpaqi: 'Enlazado',
            fechaRegistro: '2024-01-20',
            fechaRegistroFormateada: '20 ene de 2024',
            direccion: {
                pais: 'México',
                codigoPostal: '01020',
                estado: 'CDMX',
                municipio: 'Álvaro Obregón',
                calle: 'Av. Insurgentes Sur #123',
                numeroExterior: '#45',
                numeroInterior: '#12',
            },
            cuenta: {
                correo: 'maria.lopez@example.com',
            },
            credito: {
                limiteCredito: 500000.00,
                permisoExceder: true,
                diasCredito: 30,
                limiteDocumentosVencido: 15,
                estadoCredito: true,
            },
            constanciaFiscal: {
                disponible: true,
                url: '/assets/constancia-fiscal-ejemplo.pdf',
            },
        },
        {
            id: 'CLI-003',
            nombre: 'Carlos',
            apellidoPaterno: 'Hernández',
            apellidoMaterno: 'Gómez',
            telefonoFijo: '4421234567',
            telefonoWhatsApp: '4427654321',
            rfc: 'HEGC900101XYZ',
            estado: 'Denegado',
            estadoContpaqi: 'SinEnlazar',
            fechaRegistro: '2024-01-25',
            fechaRegistroFormateada: '25 ene de 2024',
            direccion: {
                pais: 'México',
                codigoPostal: '76000',
                estado: 'Querétaro',
                municipio: 'Querétaro',
                calle: 'Calle Corregidora #56',
                numeroExterior: '#78',
                numeroInterior: '#5',
            },
            cuenta: {
                correo: 'carlos.hg@example.com',
            },
            credito: null,
            constanciaFiscal: {
                disponible: false,
                url: null,
            },
        },
        {
            id: 'CLI-004',
            nombre: 'Ana Sofía',
            apellidoPaterno: 'Martínez',
            apellidoMaterno: 'Delgado',
            telefonoFijo: '3331239876',
            telefonoWhatsApp: '3336547890',
            rfc: 'MADA950505LMN',
            estado: 'Pendiente',
            estadoContpaqi: 'SinEnlazar',
            fechaRegistro: '2024-02-01',
            fechaRegistroFormateada: '1 feb de 2024',
            direccion: {
                pais: 'México',
                codigoPostal: '44100',
                estado: 'Jalisco',
                municipio: 'Guadalajara',
                calle: 'Av. Juárez #789',
                numeroExterior: '#20',
                numeroInterior: '#3',
            },
            cuenta: {
                correo: 'ana.sofia@example.com',
            },
            credito: null,
            constanciaFiscal: {
                disponible: true,
                url: '/assets/constancia-fiscal-ejemplo.pdf',
            },
        },
        {
            id: 'CLI-005',
            nombre: 'Luis Alberto',
            apellidoPaterno: 'Torres',
            apellidoMaterno: 'Morales',
            telefonoFijo: '8181234567',
            telefonoWhatsApp: '8187654321',
            rfc: 'TOML880808OPQ',
            estado: 'Aprobado',
            estadoContpaqi: 'Enlazado',
            fechaRegistro: '2024-02-10',
            fechaRegistroFormateada: '10 feb de 2024',
            direccion: {
                pais: 'México',
                codigoPostal: '64000',
                estado: 'Nuevo León',
                municipio: 'Monterrey',
                calle: 'Calle Hidalgo #321',
                numeroExterior: '#100',
                numeroInterior: '#15',
            },
            cuenta: {
                correo: 'luis.torres@example.com',
            },
            credito: {
                limiteCredito: 600000.00,
                permisoExceder: true,
                diasCredito: 40,
                limiteDocumentosVencido: 20,
                estadoCredito: true,
            },
            constanciaFiscal: {
                disponible: true,
                url: '/assets/constancia-fiscal-ejemplo.pdf',
            },
        },
    ];

    ngOnInit(): void {
        this.cargarCliente();
    }

    cargarCliente(): void {
        this.loadingData = true;
        this.errorCarga = false;

        // Obtener el ID de la URL
        const id = this.route.snapshot.paramMap.get('id');

        // Buscar el cliente por su ID en el arreglo
        setTimeout(() => {
            const clienteEncontrado = this.clientes.find(c => c.id === id);

            if (clienteEncontrado) {
                this.cliente = clienteEncontrado;
                this.loadingData = false;
            } else {
                this.errorCarga = true;
                this.loadingData = false;
            }
        }, 500); // Pequeño delay para mostrar skeleton
    }

    regresar(): void {
        this.router.navigate(['/administrador/lista-pre-registro']);
    }

    formatCurrency(value: number): string {
        return new Intl.NumberFormat('es-MX', {
            style: 'currency',
            currency: 'MXN',
            minimumFractionDigits: 2
        }).format(value);
    }

    verConstanciaFiscal(): void {
        if (this.cliente?.constanciaFiscal?.url) {
            window.open(this.cliente.constanciaFiscal.url, '_blank');
        }
    }
}