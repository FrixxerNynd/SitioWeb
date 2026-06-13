// dtos/MedidasResponseDto.js

class MedidasResponseDto {
    /**
     * @param {Object} data
     * @param {string} [data.referencia]
     * @param {number} [data.altura]
     * @param {number} [data.ancho]
     * @param {number} [data.largo]
     * @param {number} [data.peso]
     * @param {string} [data.medida_peso]
     * @param {number} [data.volumen]
     * @param {string} [data.medida_volumen]
     */
    constructor(data = {}) {
        this.referencia = data.referencia ?? '';
        this.altura = data.altura ?? 0;
        this.ancho = data.ancho ?? 0;
        this.largo = data.largo ?? 0;
        this.peso = data.peso ?? 0;
        this.medida_peso = data.medida_peso ?? '';
        this.volumen = data.volumen ?? 0;
        this.medida_volumen = data.medida_volumen ?? '';
    }
}

export default MedidasResponseDto;