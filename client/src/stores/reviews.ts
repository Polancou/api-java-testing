import { defineStore } from 'pinia';
import apiClient from '@/services/api';
import type { ReviewDto, CreateReviewDto, ApiErrorResponse } from '@/types/dto';
import { toast } from 'vue-sonner';
import type { AxiosError } from 'axios';

export const useReviewStore = defineStore('reviews', {
    state: () => ({
        reviews: [] as ReviewDto[],
        isLoading: false,
        error: null as string | null
    }),

    actions: {
        async fetchProductReviews(productId: number) {
            this.isLoading = true;
            this.error = null;
            try {
                const response = await apiClient.get<ReviewDto[]>(`/v1/reviews/product/${productId}`);
                this.reviews = response.data;
            } catch (err: unknown) {
                const axiosError = err as AxiosError<ApiErrorResponse>;
                this.error = axiosError.response?.data?.message || 'Error al cargar las reseñas';
                toast.error(this.error || 'Error desconocido');
            } finally {
                this.isLoading = false;
            }
        },

        async addReview(review: CreateReviewDto) {
            this.isLoading = true;
            this.error = null;
            try {
                const response = await apiClient.post<ReviewDto>('/v1/reviews', review);
                this.reviews.unshift(response.data); // Add to beginning of list
                toast.success('Reseña agregada con éxito');
                return true;
            } catch (err: unknown) {
                const axiosError = err as AxiosError<ApiErrorResponse>;
                this.error = axiosError.response?.data?.message || 'Error al agregar la reseña';
                toast.error(this.error || 'Error desconocido');
                return false;
            } finally {
                this.isLoading = false;
            }
        },

        async deleteReview(reviewId: number) {
            try {
                await apiClient.delete(`/v1/reviews/${reviewId}`);
                this.reviews = this.reviews.filter(r => r.id !== reviewId);
                toast.success('Reseña eliminada');
            } catch (err: unknown) {
                const axiosError = err as AxiosError<ApiErrorResponse>;
                const message = axiosError.response?.data?.message || 'Error al eliminar la reseña';
                toast.error(message);
            }
        }
    }
});
