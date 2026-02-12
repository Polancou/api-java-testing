import { defineStore } from 'pinia';
import type { CartItem, ProductDto, CartDto, ShippingAddress } from '@/types/dto';
import { toast } from 'vue-sonner';
import apiClient from '@/services/api';
import { useAuthStore } from './auth';

export const useCartStore = defineStore('cart', {
    state: () => ({
        items: [] as CartItem[],
        isOpen: false,
        selectedShippingAddress: null as ShippingAddress | null,
    }),
    persist: true,
    getters: {
        totalItems: (state) => state.items.reduce((sum, item) => sum + item.quantity, 0),
        totalPrice: (state) => state.items.reduce((sum, item) => sum + (item.price * item.quantity), 0),
    },
    actions: {
        async fetchCart() {
            const authStore = useAuthStore();
            if (!authStore.isAuthenticated) return;

            try {
                const response = await apiClient.get<CartDto>('/v1/cart');
                // Map backend DTO to frontend CartItem
                this.items = response.data.items.map(i => ({
                    productId: i.productId,
                    name: i.productName,
                    price: i.unitPrice,
                    quantity: i.quantity,
                    imageUrl: '' // Backend DTO doesn't have image yet, might need to add it or fetch product details
                }));
            } catch (error) {
                console.error('Error fetching cart:', error);
            }
        },

        async syncCart() {
            const authStore = useAuthStore();
            if (!authStore.isAuthenticated) return;

            const localItems = this.items.map(i => ({
                productId: i.productId,
                quantity: i.quantity
            }));

            try {
                const response = await apiClient.post<CartDto>('/v1/cart/sync', localItems);
                this.items = response.data.items.map(i => ({
                    productId: i.productId,
                    name: i.productName,
                    price: i.unitPrice,
                    quantity: i.quantity,
                    imageUrl: '' // TODO: Add image url to DTO
                }));
                toast.success('Carrito sincronizado');
            } catch (error) {
                console.error('Error syncing cart:', error);
                toast.error('Error al sincronizar el carrito');
            }
        },

        async addItem(product: ProductDto) {
            const authStore = useAuthStore();

            // Optimistic UI Update
            const existingItem = this.items.find(item => item.productId === product.id);
            if (existingItem) {
                existingItem.quantity++;
                toast.success(`Cantidad actualizada para ${product.name}`);
            } else {
                this.items.push({
                    productId: product.id,
                    name: product.name,
                    price: product.price,
                    quantity: 1,
                    imageUrl: product.imageUrl
                });
                toast.success(`${product.name} agregado al carrito`);
            }

            // API Call if authenticated
            if (authStore.isAuthenticated) {
                try {
                    await apiClient.post('/v1/cart/items', {
                        productId: product.id,
                        quantity: 1 // API adds this quantity
                    });
                } catch (error) {
                    console.error('Error adding item to backend:', error);
                    toast.error('Error al guardar en el servidor');
                    // Revert optimistic update? For now, keep it simple.
                }
            }
        },

        async removeItem(productId: number) {
            const authStore = useAuthStore();

            // Optimistic UI Update
            const index = this.items.findIndex(item => item.productId === productId);
            if (index !== -1) {
                this.items.splice(index, 1);
                toast.info('Producto eliminado del carrito');
            }

            // API Call if authenticated
            if (authStore.isAuthenticated) {
                try {
                    await apiClient.delete(`/v1/cart/items/${productId}`);
                } catch (error) {
                    console.error('Error removing item from backend:', error);
                }
            }
        },

        async updateQuantity(productId: number, quantity: number) {
            const authStore = useAuthStore();

            const item = this.items.find(item => item.productId === productId);
            if (item) {
                if (quantity <= 0) {
                    await this.removeItem(productId);
                } else {
                    item.quantity = quantity;

                    // API Call if authenticated
                    if (authStore.isAuthenticated) {
                        try {
                            await apiClient.put('/v1/cart/items', {
                                productId: productId,
                                quantity: quantity
                            });
                        } catch (error) {
                            console.error('Error updating quantity:', error);
                        }
                    }
                }
            }
        },

        async clearCart() {
            this.items = [];
            // If we want to clear server cart too, we need an endpoint.
            // For now, logout clears local state.
            toast.info('Carrito vaciado');
        },

        toggleCart() {
            this.isOpen = !this.isOpen;
        },
        openCart() {
            this.isOpen = true;
        },
        closeCart() {
            this.isOpen = false;
        }
    }
});
