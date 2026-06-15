/**
 * Clase que se encarga de mapear la respuesta del API externo de Exel del Norte
 * 
 * @param {string} id - Identificador de la marca
 * @param {string} nombre_marca - Nombre de la marca
 */
class MarcaResponseDto {
  constructor(data = {}) {
    this.id_marca = data.id_marca;
    this.nombre = data.nombre_marca;
  }
}

export default MarcaResponseDto;