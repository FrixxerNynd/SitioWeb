import CategoriaResponseDto from "./Categorias/categoriaResponseDto.js";
import PedidoResponseDto from "./Pedidos/PedidoResponseDto.js";
import ProductosResponseDto from "./Productos/ProductosResponseDto.js";
import PedidoCreateDto from "../Request/Pedidos/PedidoCreateDto.js";
import ProductosCreateDto from "../Request/Productos/ProductosCreateDto.js";
import ImagenesResponseDto from "./Imagenes/ImagenesResponseDto.js";
import MedidasResponseDto from "./Productos/MedidasResponseDto.js";


export const responseDto = {
    categoria: CategoriaResponseDto,
    pedido: PedidoResponseDto,
    productos: ProductosResponseDto,
    medidas: MedidasResponseDto,
    imagenes: ImagenesResponseDto
}

export const requestDto = {
    pedido: PedidoCreateDto,
    productos: ProductosCreateDto
}