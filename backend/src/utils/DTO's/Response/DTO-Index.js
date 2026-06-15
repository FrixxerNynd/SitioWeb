import CategoriaResponseDto from "./Categorias/categoriaResponseDto.js";
import PedidoResponseDto from "./Pedidos/PedidoResponseDto.js";
import ProductosResponseDto from "./Productos/ProductosResponseDto.js";
import PedidoCreateDto from "../Request/Pedidos/PedidoCreateDto.js";
import ProductosCreateDto from "../Request/Productos/ProductosCreateDto.js";
import ImagenesResponseDto from "./Imagenes/ImagenesResponseDto.js";
import MedidasResponseDto from "./Productos/MedidasResponseDto.js";
import FichaTecnicaResponseDto from "./Productos/FichaProductosDto.js";
import PrecioStockResponseDto from "./Productos/PrecioStockDto.js";
import SubCategoriaDto from "./Categorias/SubCategoriaDto.js";
import MarcaResponseDto from "./Categorias/MarcaResponseDto.js";

export const responseDto = {
    categoria: CategoriaResponseDto,
    marca: MarcaResponseDto,
    subcategoria: SubCategoriaDto,
    pedido: PedidoResponseDto,
    productos: ProductosResponseDto,
    precio_stock: PrecioStockResponseDto,
    medidas: MedidasResponseDto,
    imagenes: ImagenesResponseDto,
    fichaTecnica: FichaTecnicaResponseDto
}

export const requestDto = {
    pedido: PedidoCreateDto,
    productos: ProductosCreateDto
}