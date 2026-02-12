import { defineStore } from 'pinia';
import apiClient from '@/services/api';
import type { ProductDto, ProductFilterDto, PagedResultDto, ApiErrorResponse } from '@/types/dto';
import type { AxiosError } from 'axios';

export const useProductStore = defineStore('products', {
    state: () => ({
        products: [] as ProductDto[],
        totalCount: 0,
        currentPage: 1,
        pageSize: 10,
        isLoading: false,
        error: null as string | null,
        filters: {
            searchTerm: '',
            category: '',
            minPrice: undefined as number | undefined,
            maxPrice: undefined as number | undefined
        } as ProductFilterDto
    }),

    actions: {
        async fetchProducts(page: number = 1) {
            this.isLoading = true;
            this.error = null;
            this.currentPage = page;

            const params = {
                page: this.currentPage,
                pageSize: this.pageSize,
                searchTerm: this.filters.searchTerm,
                category: this.filters.category,
                minPrice: this.filters.minPrice,
                maxPrice: this.filters.maxPrice
            };

            try {
                const response = await apiClient.get<PagedResultDto<ProductDto>>('/v1/products', { params });
                this.products = response.data.items;
                this.totalCount = response.data.totalCount;
            } catch (err: unknown) {
                const axiosError = err as AxiosError<ApiErrorResponse>;
                this.error = axiosError.response?.data?.message || 'Error al cargar productos';
            } finally {
                this.isLoading = false;
            }
        },

        setFilters(newFilters: Partial<ProductFilterDto>) {
            this.filters = { ...this.filters, ...newFilters };
            this.fetchProducts(1); // Reset to first page on filter change
        },

        clearFilters() {
            this.filters = {
                searchTerm: '',
                category: '',
                minPrice: undefined,
                maxPrice: undefined,
                page: 1,
                pageSize: 10
            };
            this.fetchProducts(1);
        }
    }
});
