import { defineStore } from 'pinia';
import apiClient from '@/services/api';
import type { WishlistItemDto, ApiErrorResponse } from '@/types/dto';
import { toast } from 'vue-sonner';
import type { AxiosError } from 'axios';

export const useWishlistStore = defineStore('wishlist', {
    state: () => ({
        items: [] as WishlistItemDto[],
        isLoading: false,
        error: null as string | null
    }),

    getters: {
        isInWishlist: (state) => (productId: number) => {
            return state.items.some(item => item.productId === productId);
        }
    },

    actions: {
        async fetchWishlist() {
            this.isLoading = true;
            this.error = null;
            try {
                const response = await apiClient.get<WishlistItemDto[]>('/v1/wishlist');
                this.items = response.data;
            } catch (err: unknown) {
                const axiosError = err as AxiosError<ApiErrorResponse>;
                this.error = axiosError.response?.data?.message || 'Error al cargar la lista de deseos';
            } finally {
                this.isLoading = false;
            }
        },

        async toggleWishlist(productId: number) {
            // Optimistic update
            const existingIndex = this.items.findIndex(i => i.productId === productId);

            try {
                if (existingIndex !== -1) {
                    // Remove
                    await apiClient.delete(`/v1/wishlist/${productId}`);
                    this.items.splice(existingIndex, 1);
                    toast.success('Eliminado de lista de deseos');
                } else {
                    // Add
                    await apiClient.post(`/v1/wishlist/${productId}`);
                    // Fetch updated list to get full details
                    await this.fetchWishlist();
                    toast.success('Agregado a lista de deseos');
                }
            } catch (err: unknown) {
                const axiosError = err as AxiosError<ApiErrorResponse>;
                toast.error(axiosError.response?.data?.message || 'Error al actualizar lista de deseos');
                // Revert on error (simplified, ideally would restore previous state)
                await this.fetchWishlist();
            }
        },

        async removeFromWishlist(productId: number) {
            try {
                await apiClient.delete(`/v1/wishlist/${productId}`);
                this.items = this.items.filter(i => i.productId !== productId);
                toast.success('Eliminado de lista de deseos');
            } catch {
                toast.error('Error al eliminar de lista de deseos');
            }
        }
    }
});
