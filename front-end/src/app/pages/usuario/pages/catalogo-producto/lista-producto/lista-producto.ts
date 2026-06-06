// front-end/src/app/pages/usuario/pages/catalogo-producto/lista-producto/lista-producto.ts
import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SecureAuthService } from '../../../../../services/secure-auth.service';
import { ExcelNorteCatalogoService } from '../../../../../services/exel-api-base.service';
import { IProduct, IBrand, ICategory } from '../../../../../interfaces/interface-excel-norte/excel-norte-interface';
import { UIProductoCard } from '../../../../../components/shared/producto-card/producto-card';

@Component({
    selector: 'app-lista-producto',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule, UIProductoCard],
    templateUrl: './lista-producto.html',
    styleUrl: './lista-producto.css'
})
export class PageListaProducto implements OnInit {
    private authService = inject(SecureAuthService);
    private router = inject(Router);
    private catalogoService = inject(ExcelNorteCatalogoService);
    
    // ========== SEÑALES ==========
    allProducts = signal<IProduct[]>([]);
    displayedProducts = signal<IProduct[]>([]);
    
    brands = signal<IBrand[]>([]);
    categories = signal<ICategory[]>([]);
    
    isLoading = signal(true);
    isLoadingMore = signal(false);
    
    // Filtros temporales
    tempSelectedBrands = signal<Set<string>>(new Set());
    tempSelectedCategories = signal<Set<string>>(new Set());
    tempSearchTerm = signal('');
    tempPrecioMinimo = signal(0);
    tempPrecioMaximo = signal(100000);
    
    // Filtros activos
    activeSelectedBrands = signal<Set<string>>(new Set());
    activeSelectedCategories = signal<Set<string>>(new Set());
    activeSearchTerm = signal('');
    activePrecioMinimo = signal(0);
    activePrecioMaximo = signal(100000);
    
    // Estado UI
    marcaDesplegable = signal(true);
    categoriaDesplegable = signal(true);
    precioDesplegable = signal(true);
    
    // Paginación
    currentPage = signal(1);
    pageSize = 24;
    totalPages = signal(1);
    
    rangoMinimoAbsoluto = 0;
    rangoMaximoAbsoluto = 100000;
    searchInputValue = '';
    
    currentUser: any = null;

    progressLeft = computed(() => {
        const min = this.tempPrecioMinimo();
        const minAbs = this.rangoMinimoAbsoluto;
        const maxAbs = this.rangoMaximoAbsoluto;
        return ((min - minAbs) / (maxAbs - minAbs)) * 100 + '%';
    });

    progressRight = computed(() => {
        const max = this.tempPrecioMaximo();
        const minAbs = this.rangoMinimoAbsoluto;
        const maxAbs = this.rangoMaximoAbsoluto;
        return 100 - ((max - minAbs) / (maxAbs - minAbs)) * 100 + '%';
    });
    
    // ========== SEÑALES COMPUTADAS ==========
    
    filteredProducts = computed(() => {
        let result = this.allProducts();
        
        const term = this.activeSearchTerm().toLowerCase();
        if (term) {
            result = result.filter(p => 
                p.nombre.toLowerCase().includes(term) ||
                p.sku.toLowerCase().includes(term)
            );
        }
        
        const selectedBrandsSet = this.activeSelectedBrands();
        if (selectedBrandsSet.size > 0) {
            result = result.filter(p => selectedBrandsSet.has(p.marca_id));
        }
        
        const selectedCategoriesSet = this.activeSelectedCategories();
        if (selectedCategoriesSet.size > 0) {
            result = result.filter(p => selectedCategoriesSet.has(p.categoria_id));
        }
        
        const min = this.activePrecioMinimo();
        const max = this.activePrecioMaximo();
        result = result.filter(p => {
            const precio = parseFloat(p.precio);
            return precio >= min && precio <= max;
        });
        
        return result;
    });
    
    paginatedProducts = computed(() => {
        const start = (this.currentPage() - 1) * this.pageSize;
        const end = start + this.pageSize;
        return this.filteredProducts().slice(start, end);
    });
    
    totalResultados = computed(() => this.filteredProducts().length);
    totalPagesCalculated = computed(() => Math.ceil(this.totalResultados() / this.pageSize));
    
    hasActiveFilters = computed(() => {
        return this.activeSelectedBrands().size > 0 ||
               this.activeSelectedCategories().size > 0 ||
               this.activeSearchTerm().length > 0 ||
               this.activePrecioMinimo() > this.rangoMinimoAbsoluto ||
               this.activePrecioMaximo() < this.rangoMaximoAbsoluto;
    });
    
    hasTempFilters = computed(() => {
        return this.tempSelectedBrands().size > 0 ||
               this.tempSelectedCategories().size > 0 ||
               this.tempSearchTerm().length > 0 ||
               this.tempPrecioMinimo() > this.rangoMinimoAbsoluto ||
               this.tempPrecioMaximo() < this.rangoMaximoAbsoluto;
    });
    
    // Lista de filtros activos para mostrar como etiquetas
    activeFiltersList = computed(() => {
        const filters: { type: string; label: string; value: string; id: string }[] = [];
        
        // Filtrar categorías seleccionadas
        const selectedCategories = this.activeSelectedCategories();
        if (selectedCategories.size > 0) {
            const categoryNames = this.categories()
                .filter(cat => selectedCategories.has(cat.id_categoria))
                .map(cat => cat.nombre_categoria);
            
            categoryNames.forEach(nombre => {
                filters.push({
                    type: 'categoria',
                    label: 'Categoría',
                    value: nombre,
                    id: `cat_${nombre}`
                });
            });
        }
        
        // Filtrar marcas seleccionadas
        const selectedBrands = this.activeSelectedBrands();
        if (selectedBrands.size > 0) {
            const brandNames = this.brands()
                .filter(brand => selectedBrands.has(brand.id))
                .map(brand => brand.nombre);
            
            brandNames.forEach(nombre => {
                filters.push({
                    type: 'marca',
                    label: 'Marca',
                    value: nombre,
                    id: `brand_${nombre}`
                });
            });
        }
        
        // Filtro de precio
        const min = this.activePrecioMinimo();
        const max = this.activePrecioMaximo();
        const minAbs = this.rangoMinimoAbsoluto;
        const maxAbs = this.rangoMaximoAbsoluto;
        
        if (min > minAbs || max < maxAbs) {
            filters.push({
                type: 'precio',
                label: 'Precio',
                value: `${this.formatPrice(min.toString())} - ${this.formatPrice(max.toString())}`,
                id: 'price_filter'
            });
        }
        
        // Filtro de búsqueda
        const searchTerm = this.activeSearchTerm();
        if (searchTerm) {
            filters.push({
                type: 'busqueda',
                label: 'Búsqueda',
                value: searchTerm,
                id: 'search_filter'
            });
        }
        
        return filters;
    });
    
    // ========== MÉTODOS DE PAGINACIÓN ==========
    
    getRangoInicio(): number {
        if (this.filteredProducts().length === 0) return 0;
        return (this.currentPage() - 1) * this.pageSize + 1;
    }
    
    getRangoFin(): number {
        const fin = this.currentPage() * this.pageSize;
        const total = this.filteredProducts().length;
        return Math.min(fin, total);
    }
    
    getTotalPaginas(): number {
        return this.totalPagesCalculated();
    }
    
    getPaginaActual(): number {
        return this.currentPage();
    }
    
    // ========== OTROS MÉTODOS ==========
    
    getProductCountByCategory(categoryId: string): number {
        return this.allProducts().filter(p => p.categoria_id === categoryId).length;
    }
    
    getProductCountByBrand(brandId: string): number {
        return this.allProducts().filter(p => p.marca_id === brandId).length;
    }
    
    isCategorySelected(categoryId: string): boolean {
        return this.tempSelectedCategories().has(categoryId);
    }
    
    isBrandSelected(brandId: string): boolean {
        return this.tempSelectedBrands().has(brandId);
    }
    
    ngOnInit() {
        this.currentUser = this.authService.getCurrentUser();
        console.log('Usuario autenticado:', this.currentUser);
        this.cargandoDatos();        
    }
    
    calcularRangoPrecios(products: IProduct[]) {
        if (products.length === 0) return;
        
        const precios = products.map(p => parseFloat(p.precio)).filter(p => !isNaN(p));
        if (precios.length > 0) {
            this.rangoMinimoAbsoluto = Math.min(...precios);
            this.rangoMaximoAbsoluto = Math.max(...precios);
            this.tempPrecioMinimo.set(this.rangoMinimoAbsoluto);
            this.tempPrecioMaximo.set(this.rangoMaximoAbsoluto);
        }
    }
    
    toggleBrand(brandId: string) {
        const current = this.tempSelectedBrands();
        const newSet = new Set(current);
        if (newSet.has(brandId)) {
            newSet.delete(brandId);
        } else {
            newSet.add(brandId);
        }
        this.tempSelectedBrands.set(newSet);
    }
    
    toggleCategory(categoryId: string) {
        const current = this.tempSelectedCategories();
        const newSet = new Set(current);
        if (newSet.has(categoryId)) {
            newSet.delete(categoryId);
        } else {
            newSet.add(categoryId);
        }
        this.tempSelectedCategories.set(newSet);
    }
    
    onSearchChange() {
        this.tempSearchTerm.set(this.searchInputValue);
    }
    
    actualizarRangoMinimo() {
        const min = this.tempPrecioMinimo();
        const max = this.tempPrecioMaximo();
        if (min > max) {
            this.tempPrecioMinimo.set(max);
        }
        this.actualizarFondoRango();
    }
    
    actualizarRangoMaximo() {
        const min = this.tempPrecioMinimo();
        const max = this.tempPrecioMaximo();
        if (max < min) {
            this.tempPrecioMaximo.set(min);
        }
        this.actualizarFondoRango();
    }
    
    actualizarFondoRango() {
        const min = this.tempPrecioMinimo();
        const max = this.tempPrecioMaximo();
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

    // Método para redondear precio a 2 decimales
    roundPrice(price: string | number): number {
        const num = typeof price === 'string' ? parseFloat(price) : price;
        if (isNaN(num)) return 0;
        return Math.round(num * 100) / 100;
    }

    // Método para formatear precio a string con moneda
    formatPrice(price: string | number): string {
        return this.catalogoService.formatPrice(price);
    }
    
    aplicarFiltros() {
        this.activeSelectedBrands.set(new Set(this.tempSelectedBrands()));
        this.activeSelectedCategories.set(new Set(this.tempSelectedCategories()));
        this.activeSearchTerm.set(this.tempSearchTerm());
        this.activePrecioMinimo.set(this.tempPrecioMinimo());
        this.activePrecioMaximo.set(this.tempPrecioMaximo());
        this.currentPage.set(1);
        
        console.log('🔍 Filtros aplicados');
    }
    
    // Método para eliminar un filtro específico
    removeFilter(filterType: string, filterValue?: string) {
        switch (filterType) {
            case 'categoria':
                const categoryToRemove = this.categories().find(cat => cat.nombre_categoria === filterValue);
                if (categoryToRemove) {
                    const newSet = new Set(this.tempSelectedCategories());
                    newSet.delete(categoryToRemove.id_categoria);
                    this.tempSelectedCategories.set(newSet);
                }
                break;
                
            case 'marca':
                const brandToRemove = this.brands().find(brand => brand.nombre === filterValue);
                if (brandToRemove) {
                    const newSet = new Set(this.tempSelectedBrands());
                    newSet.delete(brandToRemove.id);
                    this.tempSelectedBrands.set(newSet);
                }
                break;
                
            case 'precio':
                this.tempPrecioMinimo.set(this.rangoMinimoAbsoluto);
                this.tempPrecioMaximo.set(this.rangoMaximoAbsoluto);
                this.actualizarFondoRango();
                break;
                
            case 'busqueda':
                this.tempSearchTerm.set('');
                this.searchInputValue = '';
                break;
        }
        
        this.aplicarFiltros();
    }
    
    limpiarFiltros() {
        this.tempSelectedBrands.set(new Set());
        this.tempSelectedCategories.set(new Set());
        this.tempSearchTerm.set('');
        this.searchInputValue = '';
        this.tempPrecioMinimo.set(this.rangoMinimoAbsoluto);
        this.tempPrecioMaximo.set(this.rangoMaximoAbsoluto);
        
        this.activeSelectedBrands.set(new Set());
        this.activeSelectedCategories.set(new Set());
        this.activeSearchTerm.set('');
        this.activePrecioMinimo.set(this.rangoMinimoAbsoluto);
        this.activePrecioMaximo.set(this.rangoMaximoAbsoluto);
        
        this.currentPage.set(1);
        this.actualizarFondoRango();
        console.log('🧹 Filtros limpiados');
    }
    
    nextPage() {
        if (this.currentPage() < this.totalPagesCalculated()) {
            this.currentPage.update(p => p + 1);
            document.querySelector('.productos-grid')?.scrollIntoView({ behavior: 'smooth' });
        }
    }
    
    prevPage() {
        if (this.currentPage() > 1) {
            this.currentPage.update(p => p - 1);
            document.querySelector('.productos-grid')?.scrollIntoView({ behavior: 'smooth' });
        }
    }
    
    goToPage(page: number) {
        if (page >= 1 && page <= this.totalPagesCalculated()) {
            this.currentPage.set(page);
            document.querySelector('.productos-grid')?.scrollIntoView({ behavior: 'smooth' });
        }
    }
    
    toggleMarca() {
        this.marcaDesplegable.update(v => !v);
    }
    
    toggleCategoria() {
        this.categoriaDesplegable.update(v => !v);
    }
    
    togglePrecio() {
        this.precioDesplegable.update(v => !v);
    }
    
    getStockClass(stock: string): string {
        const stockNum = parseInt(stock);
        if (stockNum <= 0) return 'bg-danger text-white';
        if (stockNum < 10) return 'bg-warning text-dark';
        return 'bg-success text-white';
    }
    
    calcularDescuento(precioOriginal: string, precioOferta: string): number {
        const original = parseFloat(precioOriginal);
        const oferta = parseFloat(precioOferta);
        if (original <= 0 || oferta <= 0) return 0;
        return Math.round(((original - oferta) / original) * 100);
    }
    
    agregarAlCarrito(product: IProduct): void {
        console.log('Producto agregado al carrito:', product);
        alert(`Producto "${product.nombre}" agregado al carrito`);
    }
    
    logout() {
        this.authService.logout();
        this.router.navigate(['/inicio-sesion']);
    }
    
    getPageNumbers(): number[] {
        const total = this.totalPagesCalculated();
        const current = this.currentPage();
        const pages: number[] = [];
        
        if (total <= 7) {
            for (let i = 1; i <= total; i++) pages.push(i);
        } else {
            if (current <= 4) {
                for (let i = 1; i <= 5; i++) pages.push(i);
                pages.push(-1);
                pages.push(total);
            } else if (current >= total - 3) {
                pages.push(1);
                pages.push(-1);
                for (let i = total - 4; i <= total; i++) pages.push(i);
            } else {
                pages.push(1);
                pages.push(-1);
                for (let i = current - 1; i <= current + 1; i++) pages.push(i);
                pages.push(-1);
                pages.push(total);
            }
        }
        return pages;
    }
    
    tieneOferta(product: IProduct): boolean {
        return !!product.precio_oferta && 
               parseFloat(product.precio_oferta) < parseFloat(product.precio);
    }
    
    getPorcentajeDescuento(product: IProduct): number {
        if (!this.tieneOferta(product)) return 0;
        const original = parseFloat(product.precio);
        const oferta = parseFloat(product.precio_oferta!);
        return Math.round(((original - oferta) / original) * 100);
    }
    
    getPrecioOferta(product: IProduct): string {
        return product.precio_oferta || product.precio;
    }
    
    // METODO PARA CARGAR DATOS (CATEGORIAS, MARCAS Y PRODUCTOS)
    async cargandoDatos() {
        this.isLoading.set(true);
        try {
            const [categorias, marcas, productos] = await Promise.all([
                this.catalogoService.getAllCategories(),
                this.catalogoService.getAllBrands(),
                this.catalogoService.getAllProducts()
            ]);
            
            this.categories.set(categorias);
            this.brands.set(marcas);
            this.allProducts.set(productos);
            
            console.log('Categorías cargadas:', categorias.length);
            console.log('Marcas cargadas:', marcas.length);
            console.log('Productos cargados:', productos.length);
            
            this.calcularRangoPrecios(productos);
            this.tempPrecioMinimo.set(this.rangoMinimoAbsoluto);
            this.tempPrecioMaximo.set(this.rangoMaximoAbsoluto);
            
            this.aplicarFiltros();
            
        } catch (error) {
            console.error('Error al cargar los datos:', error);
        } finally {
            this.isLoading.set(false);
        }
    }
}