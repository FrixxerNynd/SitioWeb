// dtos/FichaTecnicaResponseDto.js

class CaracteristicaDto {
    constructor(data = {}) {
        this.texto = data.texto?.trim() ?? '';
        this.valor = data.valor?.trim() ?? '';
    }
}

class FichaTecnicaResponseDto {
    constructor(data = {}) {
        this.referencia = data.referencia ?? '';

        // separamos los 3 grupos conocidos en campos propios
        const grupos = Array.isArray(data.fichaTecnica) ? data.fichaTecnica : [];

        this.caracteristicasGenerales = this._extraerGrupo(grupos, 'Características Generales:');
        this.detalleTecnico = this._extraerGrupo(grupos, 'Detalle tecníco');
        this.especificacionesGenerales = this._extraerGrupo(grupos, 'Especificaciones  Generales');
    }

    _extraerGrupo(grupos, nombreGrupo) {
        const grupo = grupos.find(g =>
            g.grupo?.trim().toLowerCase() === nombreGrupo.trim().toLowerCase()
        );
        return grupo?.caracteristicas?.map(c => new CaracteristicaDto(c)) ?? [];
    }
}

export default FichaTecnicaResponseDto;