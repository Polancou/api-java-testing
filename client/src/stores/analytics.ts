import { defineStore } from 'pinia';
import apiClient from '@/services/api';
import type { AnalyticsDto, ApiErrorResponse } from '@/types/dto';
import type { AxiosError } from 'axios';

export const useAnalyticsStore = defineStore('analytics', {
    state: () => ({
        data: null as AnalyticsDto | null,
        isLoading: false,
        error: null as string | null
    }),

    actions: {
        async fetchDashboardData() {
            this.isLoading = true;
            this.error = null;
            try {
                const response = await apiClient.get<AnalyticsDto>('/v1/analytics/dashboard');
                this.data = response.data;
            } catch (err: unknown) {
                const axiosError = err as AxiosError<ApiErrorResponse>;
                this.error = axiosError.response?.data?.message || 'Error al cargar datos del dashboard';
            } finally {
                this.isLoading = false;
            }
        }
    }
});
