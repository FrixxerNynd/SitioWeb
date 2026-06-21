import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UiBoton } from '../../../../components/shared/boton/boton';
import { UiIconComponent } from '../../../../components/shared/icono/icono.component';
import { ExcelNorteCatalogoService } from '../../../../services/exel-api-base.service';
import { PorcentajeService } from '../../../../services/porcentaje.service';
import { ICategory } from '../../../../interfaces/interface-excel-norte/excel-norte-interface';
import { IPercentage } from '../../../../interfaces/interface-excel-norte/percentage.interface';

interface CategoriaConMargen {
  id_categoria: string;
  nombre: string;
  porcentaje: number;
  icono: string;
}

@Component({
  selector: 'app-configuracion-margenes',
  imports: [CommonModule, FormsModule, UiBoton, UiIconComponent],
  templateUrl: './configuracion-margenes.html',
  styleUrl: './configuracion-margenes.css',
})
export class PagueConfiguracionMargenes implements OnInit {
  private catalogoService = inject(ExcelNorteCatalogoService);
  private porcentajeService = inject(PorcentajeService);

  loadingData = true;
  savingData = false;

  categorias: CategoriaConMargen[] = [];
  categoriasRespaldo: CategoriaConMargen[] = [];

  private iconosPorPalabra: [RegExp, string][] = [
    [/oficin|papel|archiv|escolar/i, 'building-office-2'],
    [/comput|lap|desktop|pc|monitor/i, 'computer-desktop'],
    [/impres|toner|tinta|laser|multifuncional/i, 'printer'],
    [/electron|gadget|accesorio|smart|device/i, 'device-phone-mobile'],
    [/red|router|switch|cable|wifi/i, 'server-stack'],
    [/audio|sonido|bocina|microfono/i, 'musical-note'],
    [/video|pantalla|tv|proyector/i, 'video-camera'],
    [/segur|camara|cctv|alarma/i, 'shield-check'],
    [/herramient|taladro|martillo|industrial/i, 'wrench-screwdriver'],
    [/limpieza|quimico|higiene/i, 'sparkles'],
    [/ropa|uniforme|calzado|textil/i, 'tag'],
    [/aliment|bebida|comestible/i, 'cake'],
    [/mueble|escritorio|silla|rack/i, 'square-3-stack-3d'],
  ];

  ngOnInit(): void {
    this.cargarDatos();
  }

  private asignarIcono(nombre: string): string {
    for (const [patron, icono] of this.iconosPorPalabra) {
      if (patron.test(nombre)) return icono;
    }
    return 'cube';
  }

  async cargarDatos() {
    this.loadingData = true;

    try {
      const [categoriasApi, porcentajes] = await Promise.all([
        this.catalogoService.getAllCategories(),
        this.porcentajeService.getAll(),
      ]);

      const pctMap = new Map<string, number>();
      for (const p of porcentajes) {
        pctMap.set(p.id_categoria, p.porcentaje);
      }

      this.categorias = categoriasApi.map((cat: ICategory) => ({
        id_categoria: cat.id_categoria,
        nombre: cat.nombre_categoria,
        porcentaje: pctMap.has(cat.id_categoria) ? pctMap.get(cat.id_categoria)! : 0,
        icono: this.asignarIcono(cat.nombre_categoria),
      }));

      this.categoriasRespaldo = this.categorias.map((c) => ({ ...c }));
    } catch (error) {
      console.error('Error al cargar datos:', error);
    } finally {
      this.loadingData = false;
    }
  }

  async guardarCambios() {
    this.savingData = true;

    try {
      const pendientes = this.categorias.filter((c) => c.porcentaje > 0);

      const resultados = await Promise.all(
        pendientes.map((c) =>
          this.porcentajeService.save(c.id_categoria, c.nombre, c.porcentaje),
        ),
      );

      const exitosos = resultados.filter((r) => r !== null).length;
      console.log(`Porcentajes guardados: ${exitosos}/${pendientes.length}`);
      alert('Configuracion guardada exitosamente');
    } catch (error) {
      console.error('Error al guardar porcentajes:', error);
      alert('Error al guardar la configuracion');
    } finally {
      this.savingData = false;
    }
  }

  restablecerValores() {
    this.categorias = this.categoriasRespaldo.map((c) => ({ ...c }));
    alert('Valores restablecidos a los guardados actualmente');
  }
}
