export interface IPercentage {
  id: number;
  id_categoria: string;
  nombre_categoria: string;
  porcentaje: number;
}

export interface IPercentageResponse {
  success: boolean;
  data: IPercentage[];
}

export interface IPercentageSingleResponse {
  success: boolean;
  data: IPercentage;
}
