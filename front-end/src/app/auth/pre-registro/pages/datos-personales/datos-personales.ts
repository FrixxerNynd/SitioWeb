import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PreRegistroService } from '../../pre-registro.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './datos-personales.html',
})
export class PageDatosPersonales implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private preRegistroService = inject(PreRegistroService);

  datosForm: FormGroup = this.fb.group({
    nombre:          ['', [Validators.required, Validators.minLength(2)]],
    apellidoPaterno: ['', [Validators.required, Validators.minLength(2)]],
    apellidoMaterno: ['', [Validators.required, Validators.minLength(2)]],
    telefonoFijo:    ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
    whatsapp:        ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
    curp:            ['', [Validators.required, Validators.pattern(/^[A-Z]{4}\d{6}[HM][A-Z]{5}[A-Z\d]\d$/)]],
  });

  ngOnInit(): void {
    const estado = this.preRegistroService.obtenerEstado();
    this.datosForm.patchValue({
      nombre:          estado.nombre || '',
      apellidoPaterno: estado.apellidoPaterno || '',
      apellidoMaterno: estado.apellidoMaterno || '',
      telefonoFijo:    estado.telefono || '',
      whatsapp:        estado.whatsapp || '',
      curp:            estado.CURP || '',
    });
  }

  siguiente(): void {
    if (this.datosForm.invalid) {
      this.datosForm.markAllAsTouched();
      return;
    }

    const { nombre, apellidoPaterno, apellidoMaterno, telefonoFijo, whatsapp, curp } = this.datosForm.value;

    this.preRegistroService.guardarPaso({
      nombre,
      apellidoPaterno,
      apellidoMaterno,
      telefono: telefonoFijo,
      whatsapp,
      CURP: curp,
    });

    this.router.navigate(['/pre-registro/domicilio']);
  }
}
