// frontend/src/app/pages/usuario/pages/catalogo-producto/lista-producto/lista-producto.ts
import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UiNavbarComponent } from '../../../../../components/layout/navbar/navbar';
import { CategoryService } from '../../../../../services/category.service';
import { BrandService } from '../../../../../services/brand.service';
import { Category } from '../../../../../models/models-excel-norte/category.model';
import { Brand } from '../../../../../models/models-excel-norte/brand.model';

@Component({
    selector: 'app-lista-producto',
    standalone: true,
    imports: [CommonModule, FormsModule, UiNavbarComponent],
    templateUrl: './lista-producto.html',
    styleUrl: './lista-producto.css'
})
export class PageListaProducto implements OnInit {
    // ========== SEÑALES ==========
    // Datos
    categories = signal<Category[]>([]);
    brands = signal<Brand[]>([]);
    
    // Estado de carga
    isLoading = signal<boolean>(false);
    
    // Filtros seleccionados
    selectedBrands = signal<{ [key: string]: boolean }>({});
    selectedCategories = signal<{ [key: string]: boolean }>({});

    // Estado desplegable
    // true = expandido, false = colapsado    
    marcaDesplegable = signal<boolean>(true);  
    categoriaDesplegable = signal<boolean>(true);
    precioDesplegable = signal<boolean>(true);  

    // Precio
    precioMinimo = signal<number>(0);
    precioMaximo = signal<number>(100000);
    rangoMinimoAbsoluto = 0;
    rangoMaximoAbsoluto = 100000;    
    
    // ========== SEÑALES COMPUTADAS ==========
    // Total de marcas
    totalBrands = computed(() => this.brands().length);
    
    // Total de categorías
    totalCategories = computed(() => this.categories().length);
    
    // Marcas seleccionadas (como array)
    selectedBrandsList = computed(() => {
        const selected = this.selectedBrands();
        return Object.keys(selected).filter(key => selected[key]);
    });
    
    // Categorías seleccionadas (como array)
    selectedCategoriesList = computed(() => {
        const selected = this.selectedCategories();
        return Object.keys(selected).filter(key => selected[key]);
    });
    
    // Hay filtros seleccionados?
    hasSelectedFilters = computed(() => {
        return this.selectedBrandsList().length > 0 || this.selectedCategoriesList().length > 0;
    });
    
    // Mitad de marcas para dos columnas
    firstHalfBrands = computed(() => {
        const allBrands = this.brands();
        const half = Math.ceil(allBrands.length / 2);
        return allBrands.slice(0, half);
    });
    
    secondHalfBrands = computed(() => {
        const allBrands = this.brands();
        const half = Math.ceil(allBrands.length / 2);
        return allBrands.slice(half);
    });
    
    // Mitad de categorías para dos columnas
    firstHalfCategories = computed(() => {
        const allCategories = this.categories();
        const half = Math.ceil(allCategories.length / 2);
        return allCategories.slice(0, half);
    });
    
    secondHalfCategories = computed(() => {
        const allCategories = this.categories();
        const half = Math.ceil(allCategories.length / 2);
        return allCategories.slice(half);
    });

    constructor(
        private categoryService: CategoryService,
        private brandService: BrandService
    ) {}

    async ngOnInit() {
        await this.loadFilters();
    }

    async loadFilters() {
        this.isLoading.set(true);
        try {
            console.log('📡 Cargando filtros...');
            
            const [categories, brands] = await Promise.all([
                this.categoryService.getAllCategories(),
                this.brandService.getAllBrands()
            ]);
            
            this.categories.set(categories);
            this.brands.set(brands);
            
            console.log('✅ Filtros cargados:', {
                categorias: categories.length,
                marcas: brands.length
            });
        } catch (error) {
            console.error('❌ Error loading filters:', error);
        } finally {
            this.isLoading.set(false);
        }
    }

    // Actualizar selección de marca
    toggleBrand(brandId: string, event: Event) {
        const isChecked = (event.target as HTMLInputElement).checked;
        const currentSelected = this.selectedBrands();
        this.selectedBrands.set({
            ...currentSelected,
            [brandId]: isChecked
        });
    }

    // Actualizar selección de categoría
    toggleCategory(categoryId: string, event: Event) {
        const isChecked = (event.target as HTMLInputElement).checked;
        const currentSelected = this.selectedCategories();
        this.selectedCategories.set({
            ...currentSelected,
            [categoryId]: isChecked
        });
    }

    // Mostrar filtros seleccionados
    showSelectedFilters() {
        const marcas = this.selectedBrandsList();
        const categorias = this.selectedCategoriesList();
        
        console.log('🔍 Filtros aplicados:', {
            marcas: marcas,
            categorias: categorias
        });
        
        if (marcas.length === 0 && categorias.length === 0) {
            alert('No has seleccionado ningún filtro');
        } else {
            alert(`Filtros aplicados:\n📌 Marcas: ${marcas.length}\n📁 Categorías: ${categorias.length}\n\nRevisa la consola para ver los detalles.`);
        }
    }

    // Limpiar todos los filtros
    clearFilters() {
        this.selectedBrands.set({});
        this.selectedCategories.set({});
        console.log('🧹 Filtros limpiados');
        alert('Todos los filtros han sido limpiados');
    }
    
    // Alternar estado desplegable
    toggleMarca() {
        this.marcaDesplegable.update(estado => !estado);
    }

    toggleCategoria() {
        this.categoriaDesplegable.update(estado => !estado);
    }
    
    togglePrecio(){
        this.precioDesplegable.update(estado => !estado);
    }


// Actualizar rango mínimo
actualizarRangoMinimo() {
    const min = this.precioMinimo();
    const max = this.precioMaximo();
    
    if (min > max) {
        this.precioMinimo.set(max);
    }
    
    this.actualizarFondoRango();
    this.aplicarFiltroPrecio();
}

// Actualizar rango máximo
actualizarRangoMaximo() {
    const min = this.precioMinimo();
    const max = this.precioMaximo();
    
    if (max < min) {
        this.precioMaximo.set(min);
    }
    
    this.actualizarFondoRango();
    this.aplicarFiltroPrecio();
}

// Actualizar el fondo del slider
actualizarFondoRango() {
    const min = this.precioMinimo();
    const max = this.precioMaximo();
    const minAbs = this.rangoMinimoAbsoluto;
    const maxAbs = this.rangoMaximoAbsoluto;
    
    const minPercent = ((min - minAbs) / (maxAbs - minAbs)) * 100;
    const maxPercent = ((max - minAbs) / (maxAbs - minAbs)) * 100;
    
    const container = document.querySelector('.double-range-container');
    if (container) {
        (container as HTMLElement).style.setProperty('--min-pos', `${minPercent}%`);
        (container as HTMLElement).style.setProperty('--max-pos', `${maxPercent}%`);
    }
}

// Aplicar filtro
aplicarFiltroPrecio() {
    console.log('Filtrando entre:', this.precioMinimo(), 'y', this.precioMaximo());
}

// Inicializar fondo del rango
ngAfterViewInit() {
    this.actualizarFondoRango();
}    
}