export interface IMexicoState {
  d_estado: string;
  d_codigo: string;
}

export interface IMexicoCity {
  d_ciudad: string;
  d_estado: string;
  D_mnpio: string;
  d_codigo: string;
}

export interface IMexicoZipCode {
  d_codigo: string;
  d_asenta: string;
  D_mnpio: string;
  d_estado: string;
  d_ciudad: string;
}

export interface IMexicoSettlement {
  d_asenta: string;
  d_tipo_asenta: string;
  D_mnpio: string;
  d_estado: string;
  d_ciudad: string;
  d_codigo: string;
}

export interface IApiMexicoResponse<T> {
  meta: {
    page: number;
    per_page: number;
    total: number;
    total_pages: number;
  };
  data: T[];
}