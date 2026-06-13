import CategoriaResponseDto from "./Categorias/categoriaResponseDto.js";
import PedidoResponseDto from "./Pedidos/PedidoResponseDto.js";
import ProductosResponseDto from "./Productos/ProductosResponseDto.js";
import PedidoCreateDto from "../Request/Pedidos/PedidoCreateDto.js";
import ProductosCreateDto from "../Request/Productos/ProductosCreateDto.js";


export const responseDto = {
    categoria: CategoriaResponseDto,
    pedido: PedidoResponseDto,
    productos: ProductosResponseDto
}

export const requestDto = {
    pedido: PedidoCreateDto,
    productos: ProductosCreateDto
}