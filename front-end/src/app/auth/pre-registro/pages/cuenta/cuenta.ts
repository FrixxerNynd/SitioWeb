import { Component, signal } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './cuenta.html',
})
export class PageCuenta {
  // Señal para controlar el modal
  visualizarModal = signal<boolean>(false);

  abrirModal() {
    this.visualizarModal.set(true);
    console.log("Modal abierto:", this.visualizarModal());
  }

  cerrarModal() {
    this.visualizarModal.set(false);
    console.log("Modal cerrado:", this.visualizarModal());
  }
}