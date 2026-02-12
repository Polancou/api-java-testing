<script setup lang="ts">
import { onMounted } from 'vue';
import { useAnalyticsStore } from '@/stores/analytics';
import { storeToRefs } from 'pinia';
import {
  CurrencyDollarIcon,
  ShoppingBagIcon,
  UserGroupIcon,
  TagIcon
} from '@heroicons/vue/24/outline';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

const analyticsStore = useAnalyticsStore();
const { data, isLoading, error } = storeToRefs(analyticsStore);

onMounted(() => {
  analyticsStore.fetchDashboardData();
});

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(value);
};
</script>

<template>
  <div class="space-y-8">
    <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Dashboard de Analíticas</h2>

    <div v-if="isLoading" class="flex justify-center py-12">
      <LoadingSpinner />
    </div>

    <div v-else-if="error"
      class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative dark:bg-red-900 dark:border-red-700 dark:text-red-300">
      {{ error }}
    </div>

    <div v-else-if="data" class="space-y-8">
      <!-- Stats Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <!-- Revenue -->
        <div
          class="bg-white dark:bg-gray-800 shadow rounded-lg p-6 flex items-center justify-between border border-gray-100 dark:border-gray-700">
          <div class="min-w-0">
            <p class="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">Ingresos Totales</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white mt-1 truncate"
              :title="formatCurrency(data.totalRevenue)">
              {{ formatCurrency(data.totalRevenue) }}
            </p>
          </div>
          <div
            class="flex-shrink-0 p-3 bg-indigo-100 dark:bg-indigo-900/30 rounded-full text-indigo-600 dark:text-indigo-400 ml-4">
            <CurrencyDollarIcon class="w-6 h-6" />
          </div>
        </div>

        <!-- Orders -->
        <div
          class="bg-white dark:bg-gray-800 shadow rounded-lg p-6 flex items-center justify-between border border-gray-100 dark:border-gray-700">
          <div class="min-w-0">
            <p class="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">Pedidos Totales</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white mt-1 truncate">{{ data.totalOrders }}</p>
          </div>
          <div
            class="flex-shrink-0 p-3 bg-blue-100 dark:bg-blue-900/30 rounded-full text-blue-600 dark:text-blue-400 ml-4">
            <ShoppingBagIcon class="w-6 h-6" />
          </div>
        </div>

        <!-- Users -->
        <div
          class="bg-white dark:bg-gray-800 shadow rounded-lg p-6 flex items-center justify-between border border-gray-100 dark:border-gray-700">
          <div class="min-w-0">
            <p class="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">Usuarios</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white mt-1 truncate">{{ data.totalUsers }}</p>
          </div>
          <div
            class="flex-shrink-0 p-3 bg-green-100 dark:bg-green-900/30 rounded-full text-green-600 dark:text-green-400 ml-4">
            <UserGroupIcon class="w-6 h-6" />
          </div>
        </div>

        <!-- Products -->
        <div
          class="bg-white dark:bg-gray-800 shadow rounded-lg p-6 flex items-center justify-between border border-gray-100 dark:border-gray-700">
          <div class="min-w-0">
            <p class="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">Productos</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white mt-1 truncate">{{ data.totalProducts }}</p>
          </div>
          <div
            class="flex-shrink-0 p-3 bg-purple-100 dark:bg-purple-900/30 rounded-full text-purple-600 dark:text-purple-400 ml-4">
            <TagIcon class="w-6 h-6" />
          </div>
        </div>
      </div>

      <!-- Charts / Tables Section -->
      <div class="grid grid-cols-1 xl:grid-cols-2 gap-8">
        <!-- Top Selling Products -->
        <div
          class="bg-white dark:bg-gray-800 shadow rounded-lg border border-gray-100 dark:border-gray-700 overflow-hidden">
          <div class="px-6 py-5 border-b border-gray-200 dark:border-gray-700">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white">Productos Más Vendidos</h3>
          </div>
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead class="bg-gray-50 dark:bg-gray-700/50">
                <tr>
                  <th scope="col"
                    class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Producto</th>
                  <th scope="col"
                    class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Ventas</th>
                  <th scope="col"
                    class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Ingresos</th>
                </tr>
              </thead>
              <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                <tr v-for="product in data.topSellingProducts" :key="product.productId"
                  class="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors">
                  <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">{{
                    product.productName }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-300 text-right">{{
                    product.quantitySold }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white font-mono text-right">{{
                    formatCurrency(product.revenue) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Monthly Sales -->
        <div
          class="bg-white dark:bg-gray-800 shadow rounded-lg border border-gray-100 dark:border-gray-700 overflow-hidden">
          <div class="px-6 py-5 border-b border-gray-200 dark:border-gray-700">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white">Ventas Mensuales</h3>
          </div>
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead class="bg-gray-50 dark:bg-gray-700/50">
                <tr>
                  <th scope="col"
                    class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Mes</th>
                  <th scope="col"
                    class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Pedidos</th>
                  <th scope="col"
                    class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Ingresos</th>
                </tr>
              </thead>
              <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                <tr v-for="month in data.monthlySales" :key="month.month"
                  class="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors">
                  <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">{{
                    month.month }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-300 text-right">{{
                    month.orderCount }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white font-mono text-right">{{
                    formatCurrency(month.revenue) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
