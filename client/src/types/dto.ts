// DTOs (Data Transfer Objects) para la aplicación

// DTO para el login de usuario
export interface LoginUsuarioDto {
  email: string;
  password?: string;
}
// DTO para el registro de usuario
export interface RegistroUsuarioDto {
  nombreCompleto: string;
  email: string;
  password?: string;
  numeroTelefono: string;
}
// DTO para el perfil de usuario
export interface PerfilUsuarioDto {
  id: number;
  nombreCompleto: string;
  email: string;
  numeroTelefono: string;
  rol: string;
  fechaRegistro: string;
  avatarUrl?: string;
}
// DTO para actualizar el perfil de usuario
export interface ActualizarPerfilDto {
  nombreCompleto: string;
  numeroTelefono: string;
}
// DTO para el cambio de contraseña
export interface CambiarPasswordDto {
  oldPassword: string;
  newPassword: string;
  confirmPassword: string;
}
// DTO para el token JWT
export interface JwtPayload {
  nameid: string;
  email: string;
  role: string;
  nbf: number;
  iat: number;
  exp: number;
  iss: string;
}
// DTO para la paginación de resultados
export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
// DTO para el token de acceso y refresco
export interface TokenResponseDto {
  accessToken: string;
  refreshToken: string;
}

// DTO para actualizar el rol de un usuario (debería ir al final del archivo)
export interface ActualizarRolUsuarioDto {
  rol: string; // "Admin" o "User"
}

// DTO para solicitar el reseteo de contraseña
export interface ForgotPasswordDto {
  email: string;
}

// DTO para ejecutar el reseteo de contraseña
export interface ResetPasswordDto {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

// Tipo para la respuesta de Google
export interface GoogleCredentialResponse {
  credential: string;
}

export interface AdminUserQueryParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  rol?: string | number;
}

export interface ApiErrorResponse {
  message: string;
  statusCode: number;
  traceId: string;
}

// DTO para productos
export interface ProductDto {
  id: number;
  name: string;
  description: string;
  price: number;
  stock: number;
  imageUrl?: string;
  category?: string;
}

export interface CreateProductDto {
  name: string;
  description: string;
  price: number;
  stock: number;
  imageUrl?: string;
  category?: string;
}

export interface UpdateProductDto {
  name: string;
  description: string;
  price: number;
  stock: number;
  imageUrl?: string;
  category?: string;
}

// Interfaz para items del carrito (Frontend)
export interface CartItem {
  productId: number;
  name: string;
  price: number;
  quantity: number;
  imageUrl?: string;
}

// DTOs del Backend para el Carrito
export interface CartItemDto {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface OrderItemDto {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
}

export interface OrderDto {
  id: number;
  orderDate: string;
  totalAmount: number;
  status: string;
  items: OrderItemDto[];
}


export interface CartDto {
  id: number;
  userId: number;
  items: CartItemDto[];
  totalPrice: number;
}

// DTO para direcciones de envío
export interface ShippingAddress {
  id?: number;
  name: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

// DTOs para Reseñas
export interface ReviewDto {
  id: number;
  productId: number;
  userId: number;
  userName: string;
  rating: number;
  comment: string;
  createdAt: string;
}

export interface CreateReviewDto {
  productId: number;
  rating: number;
  comment: string;
}

// DTOs para Lista de Deseos
export interface WishlistItemDto {
  id: number;
  productId: number;
  productName: string;
  productPrice: number;
  productImageUrl?: string;
  addedAt: string;
}

// DTO para Filtros de Productos
export interface ProductFilterDto {
  searchTerm?: string;
  category?: string;
  minPrice?: number | undefined;
  maxPrice?: number | undefined;
  page: number;
  pageSize: number;
}

// DTOs para Analytics
export interface AnalyticsDto {
  totalRevenue: number;
  totalOrders: number;
  totalProducts: number;
  totalUsers: number;
  topSellingProducts: TopProductDto[];
  monthlySales: MonthlySalesDto[];
}

export interface TopProductDto {
  productId: number;
  productName: string;
  quantitySold: number;
  revenue: number;
}

export interface MonthlySalesDto {
  month: string;
  revenue: number;
  orderCount: number;
}
