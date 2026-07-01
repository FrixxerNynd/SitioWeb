import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UiBoton } from '../../../../components/shared/boton/boton';
import { MexicoApiService, IMexicoState, IMexicoCity, IMexicoSettlement } from '../../../../services/mexico-api.service';
import { PreRegistroService } from '../../pre-registro.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule, UiBoton],
  templateUrl: './domicilio.html',
})
export class PageDomicilio implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private mexicoApi = inject(MexicoApiService);
  private preRegistroService = inject(PreRegistroService);

  states = signal<IMexicoState[]>([]);
  cities = signal<IMexicoCity[]>([]);
  settlements = signal<IMexicoSettlement[]>([]);

  isLoadingStates = signal(false);
  isLoadingCities = signal(false);
  isLoadingSettlements = signal(false);
  errorMessage = signal<string | null>(null);

  domicilioForm: FormGroup;

  constructor() {
    this.domicilioForm = this.fb.group({
      pais: ['México', Validators.required],
      estado: ['', Validators.required],
      ciudad: ['', Validators.required],
      codigoPostal: ['', [Validators.required, Validators.pattern(/^\d{5}$/)]],
      colonia: ['', Validators.required],
      calle: ['', Validators.required],
      numExterior: ['', Validators.required],
      numInterior: ['']
    });
  }

  async ngOnInit() {
    await this.loadStates();
    
    // Suscribirse a cambios de estado
    this.domicilioForm.get('estado')?.valueChanges.subscribe(async (stateName) => {
      if (stateName) {
        await this.loadCitiesByState(stateName);
        this.domicilioForm.patchValue({ ciudad: '', codigoPostal: '', colonia: '' });
      }
    });

    // Suscribirse a cambios de código postal
    this.domicilioForm.get('codigoPostal')?.valueChanges.subscribe(async (zipCode) => {
      if (zipCode && zipCode.length === 5) {
        await this.loadDataByZipCode(zipCode);
      }
    });

    // Restaurar estado si ya existe en localStorage
    const estado = this.preRegistroService.obtenerEstado();
    if (estado.direccion) {
      const dir = estado.direccion;
      
      // Cargar ciudades del estado y colonias del CP manualmente
      if (dir.estado) {
        await this.loadCitiesByState(dir.estado);
      }
      if (dir.codigoPostal) {
        await this.loadDataByZipCode(dir.codigoPostal);
      }

      this.domicilioForm.patchValue({
        pais:           dir.pais || 'México',
        estado:         dir.estado || '',
        ciudad:         dir.ciudad || '',
        codigoPostal:   dir.codigoPostal || '',
        colonia:        dir.colonia || '',
        calle:          dir.calle || '',
        numExterior:    dir.numeroExterior || '',
        numInterior:    dir.numeroInterior || '',
      }, { emitEvent: false });
    }
  }

  async loadStates() {
    this.isLoadingStates.set(true);
    this.errorMessage.set(null);
    try {
      const states = await this.mexicoApi.getStates();
      console.log('📦 Estados:', states);
      this.states.set(states);
    } catch (error) {
      console.error('Error loading states:', error);
      this.errorMessage.set('Error al cargar los estados');
    } finally {
      this.isLoadingStates.set(false);
    }
  }

  async loadCitiesByState(stateName: string) {
    this.isLoadingCities.set(true);
    try {
      const allCities = await this.mexicoApi.getCities();
      const filtered = allCities.filter(city => city.d_estado === stateName);
      console.log(`📦 Ciudades de ${stateName}:`, filtered.length);
      this.cities.set(filtered);
    } catch (error) {
      console.error('Error loading cities:', error);
    } finally {
      this.isLoadingCities.set(false);
    }
  }

  async loadDataByZipCode(zipCode: string) {
    this.isLoadingSettlements.set(true);
    try {
      const data = await this.mexicoApi.loadAddressDataByZipCode(zipCode);
      console.log('📦 Datos por CP:', data);
      
      if (data.settlements.length > 0) {
        this.settlements.set(data.settlements);
        
        if (data.state) {
          this.domicilioForm.patchValue({ estado: data.state.d_estado }, { emitEvent: false });
          await this.loadCitiesByState(data.state.d_estado);
        }
        
        if (data.city) {
          this.domicilioForm.patchValue({ ciudad: data.city.d_ciudad }, { emitEvent: false });
        }
      } else {
        this.settlements.set([]);
        this.errorMessage.set(`No se encontraron colonias para el CP ${zipCode}`);
      }
    } catch (error) {
      console.error('Error loading data by zip code:', error);
      this.errorMessage.set('Error al buscar el código postal');
      this.settlements.set([]);
    } finally {
      this.isLoadingSettlements.set(false);
    }
  }

  // ========== MÉTODOS PARA EL TEMPLATE ==========
  
  onStateChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.domicilioForm.patchValue({ estado: select.value });
  }

  onCityChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.domicilioForm.patchValue({ ciudad: select.value });
  }

  onZipCodeChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.domicilioForm.patchValue({ codigoPostal: input.value });
  }

  onSettlementChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.domicilioForm.patchValue({ colonia: select.value });
  }

  async onSubmit() {
    if (this.domicilioForm.invalid) {
      this.domicilioForm.markAllAsTouched();
      return;
    }

    const v = this.domicilioForm.value;

    // Guardar dirección en el estado del wizard
    this.preRegistroService.guardarPaso({
      direccion: {
        calle:          v.calle,
        numeroExterior: v.numExterior,
        numeroInterior: v.numInterior ?? '',
        colonia:        v.colonia,
        codigoPostal:   v.codigoPostal,
        ciudad:         v.ciudad,
        estado:         v.estado,
        pais:           v.pais,
        telefono1:      '',
      }
    });

    this.router.navigate(['/pre-registro/cuenta']);
  }

  // ========== GETTERS PARA EL TEMPLATE ==========
  
  get estadoControl() { 
    return this.domicilioForm.get('estado'); 
  }
  
  get ciudadControl() { 
    return this.domicilioForm.get('ciudad'); 
  }
  
  get codigoPostalControl() { 
    return this.domicilioForm.get('codigoPostal'); 
  }
  
  get coloniaControl() { 
    return this.domicilioForm.get('colonia'); 
  }
}