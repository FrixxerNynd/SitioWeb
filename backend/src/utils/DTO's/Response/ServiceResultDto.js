
class ServiceResultDto {
    /**
     * @param {boolean} success
     * @param {*} [data]
     * @param {string} [message]
     * @param {Array<string>} [errors]
     */
    constructor(success, data = null, message = '', errors = []) {
        this.success = success;
        this.data = data;
        this.message = message;
        this.errors = errors;
    }

    static success(data, message = 'Operación exitosa') {
        return new ServiceResultDto(true, data, message, []);
    }

    static failure(message = 'Ocurrió un error', errors = []) {
        return new ServiceResultDto(false, null, message, errors);
    }
}

export default ServiceResultDto;