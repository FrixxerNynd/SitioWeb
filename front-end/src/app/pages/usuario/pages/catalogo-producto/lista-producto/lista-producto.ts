// frontend/src/app/pages/usuario/pages/catalogo-producto/lista-producto/lista-producto.ts
import { Component, OnInit, AfterViewInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../../../../../services/category.service';
import { BrandService } from '../../../../../services/brand.service';
import { ProductService } from '../../../../../services/product.service';
import { Category } from '../../../../../models/models-excel-norte/category.model';
import { Brand } from '../../../../../models/models-excel-norte/brand.model';
import { IProduct } from '../../../../../interfaces/interface-excel-norte/excel-norte-interface';

@Component({
    selector: 'app-lista-producto',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './lista-producto.html',
    styleUrl: './lista-producto.css'
})
export class PageListaProducto implements OnInit, AfterViewInit {
    // ========== SERVICIOS ==========
    private categoryService = inject(CategoryService);
    private brandService = inject(BrandService);
    private productService = inject(ProductService);
    
    // ========== SEÑALES ==========
    // Datos
    categories = signal<Category[]>([]);
    brands = signal<Brand[]>([]);
    products = signal<IProduct[]>([]);
    
    // Estado de carga
    isLoading = signal<boolean>(false);
    isLoadingProducts = signal<boolean>(false);
    
    // Filtros seleccionados
    selectedBrands = signal<{ [key: string]: boolean }>({});
    selectedCategories = signal<{ [key: string]: boolean }>({});
    mostrarSinStock = signal<boolean>(false);

    // Estado desplegable (true = expandido, false = colapsado)    
    marcaDesplegable = signal<boolean>(true);  
    categoriaDesplegable = signal<boolean>(true);
    precioDesplegable = signal<boolean>(true);  

    // Precio
    precioMinimo = signal<number>(0);
    precioMaximo = signal<number>(100000);
    rangoMinimoAbsoluto = 0;
    rangoMaximoAbsoluto = 100000;    
    
    // ========== SEÑALES COMPUTADAS ==========
    // Total de resultados
    totalResultados = computed(() => this.products().length);
    
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
        return this.selectedBrandsList().length > 0 || 
               this.selectedCategoriesList().length > 0 ||
               this.precioMinimo() > this.rangoMinimoAbsoluto ||
               this.precioMaximo() < this.rangoMaximoAbsoluto ||
               this.mostrarSinStock();
    });
    
    // Resumen de filtros para mostrar
    filtrosResumen = computed(() => {
        const resumen: string[] = [];
        
        const marcas = this.selectedBrandsList();
        if (marcas.length > 0) {
            resumen.push(`${marcas.length} marca(s)`);
        }
        
        const categorias = this.selectedCategoriesList();
        if (categorias.length > 0) {
            resumen.push(`${categorias.length} categoría(s)`);
        }
        
        if (this.precioMinimo() > this.rangoMinimoAbsoluto || 
            this.precioMaximo() < this.rangoMaximoAbsoluto) {
            resumen.push(`$${this.precioMinimo()} - $${this.precioMaximo()}`);
        }
        
        if (this.mostrarSinStock()) {
            resumen.push('Mostrar sin stock');
        }
        
        return resumen;
    });

    constructor() {}

    async ngOnInit() {
        await this.loadFilters();
        await this.loadProducts();
    }

    ngAfterViewInit() {
        this.actualizarFondoRango();
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
            
            // Calcular rango de precios desde productos
            await this.calcularRangoPrecios();
            
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

    async loadProducts() {
        this.isLoadingProducts.set(true);
        try {
            const filters: any = {};
            
            const selectedBrands = this.selectedBrandsList();
            if (selectedBrands.length > 0) {
                filters.marca = selectedBrands.join(',');
            }
            
            const selectedCategories = this.selectedCategoriesList();
            if (selectedCategories.length > 0) {
                filters.categoria = selectedCategories.join(',');
            }
            
            filters.sin_stock = this.mostrarSinStock();
            
            // Aquí podrías agregar filtro de precio si tu API lo soporta
            // filters.precio_min = this.precioMinimo();
            // filters.precio_max = this.precioMaximo();
            
            console.log('🔍 Aplicando filtros:', filters);
            
            const products = await this.productService.getProducts(filters);
            this.products.set(products);
            
            console.log(`✅ ${products.length} productos cargados`);
        } catch (error) {
            console.error('❌ Error loading products:', error);
        } finally {
            this.isLoadingProducts.set(false);
        }
    }

    async calcularRangoPrecios() {
        try {
            const products = await this.productService.getProducts();
            if (products.length > 0) {
                const precios = products.map(p => parseFloat(p.precio)).filter(p => !isNaN(p));
                if (precios.length > 0) {
                    this.rangoMinimoAbsoluto = Math.min(...precios);
                    this.rangoMaximoAbsoluto = Math.max(...precios);
                    this.precioMinimo.set(this.rangoMinimoAbsoluto);
                    this.precioMaximo.set(this.rangoMaximoAbsoluto);
                }
            }
        } catch (error) {
            console.error('Error calculando rango de precios:', error);
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

    // Alternar mostrar sin stock
    toggleSinStock(event: Event) {
        this.mostrarSinStock.set((event.target as HTMLInputElement).checked);
    }

    // Aplicar filtros
    async aplicarFiltros() {
        console.log('🔍 Aplicando filtros:', {
            marcas: this.selectedBrandsList(),
            categorias: this.selectedCategoriesList(),
            precio: { min: this.precioMinimo(), max: this.precioMaximo() },
            sinStock: this.mostrarSinStock()
        });
        
        await this.loadProducts();
        
        // Mostrar notificación
        const total = this.totalResultados();
        const filtrosCount = this.filtrosResumen().length;
        
        if (filtrosCount > 0) {
            alert(`✅ Filtros aplicados\n📊 ${total} productos encontrados\n🔧 ${filtrosCount} filtro(s) activo(s)`);
        } else {
            alert(`✅ Mostrando todos los productos\n📊 ${total} productos disponibles`);
        }
    }

    // Limpiar todos los filtros
    async limpiarFiltros() {
        this.selectedBrands.set({});
        this.selectedCategories.set({});
        this.mostrarSinStock.set(false);
        this.precioMinimo.set(this.rangoMinimoAbsoluto);
        this.precioMaximo.set(this.rangoMaximoAbsoluto);
        
        console.log('🧹 Filtros limpiados');
        
        await this.loadProducts();
        alert('✅ Todos los filtros han sido limpiados');
    }
    
    // Alternar estado desplegable
    toggleMarca() {
        this.marcaDesplegable.update(estado => !estado);
    }

    toggleCategoria() {
        this.categoriaDesplegable.update(estado => !estado);
    }
    
    togglePrecio() {
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
    }

    // Actualizar rango máximo
    actualizarRangoMaximo() {
        const min = this.precioMinimo();
        const max = this.precioMaximo();
        
        if (max < min) {
            this.precioMaximo.set(min);
        }
        
        this.actualizarFondoRango();
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

    // Obtener nombre de marca por ID
    getBrandName(brandId: string): string {
        const brand = this.brands().find(b => b.id === brandId);
        return brand?.nombre || brandId;
    }

    // Obtener nombre de categoría por ID
    getCategoryName(categoryId: string): string {
        const category = this.categories().find(c => c.id === categoryId);
        return category?.nombre || categoryId;
    }
}