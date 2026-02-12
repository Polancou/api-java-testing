<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '@/services/api';
import BaseSkeleton from '@/components/common/BaseSkeleton.vue';
import { ShoppingBagIcon } from '@heroicons/vue/24/outline';

interface OrderItem {
    productId: number;
    productName: string;
    quantity: number;
    unitPrice: number;
}

interface Order {
    id: number;
    orderDate: string;
    totalAmount: number;
    status: string;
    items: OrderItem[];
}

const orders = ref<Order[]>([]);
const loading = ref(true);
const error = ref('');

const statusColors: Record<string, string> = {
    'Pending': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300',
    'Processing': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
    'Shipped': 'bg-indigo-100 text-indigo-800 dark:bg-indigo-900 dark:text-indigo-300',
    'Delivered': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
    'Cancelled': 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300',
};

const statusLabels: Record<string, string> = {
    'Pending': 'Pendiente',
    'Processing': 'Procesando',
    'Shipped': 'Enviado',
    'Delivered': 'Entregado',
    'Cancelled': 'Cancelado',
};

onMounted(async () => {
    try {
        const response = await apiClient.get<Order[]>('/v1/orders');
        // Sort by date descending
        orders.value = response.data.sort((a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime());
    } catch (err) {
        console.error('Error fetching orders:', err);
        error.value = 'No se pudieron cargar tus pedidos.';
    } finally {
        loading.value = false;
    }
});

const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-ES', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
};
</script>

<template>
    <div class="container mx-auto px-4 py-8 max-w-4xl">
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-8 flex items-center gap-3">
            <ShoppingBagIcon class="h-8 w-8 text-indigo-600" />
            Mis Pedidos
        </h1>

        <div v-if="loading" class="space-y-4">
            <div v-for="i in 3" :key="i" class="bg-white dark:bg-gray-800 p-6 rounded-lg shadow animate-pulse">
                <div class="flex justify-between mb-4">
                    <BaseSkeleton width="150px" height="24px" />
                    <BaseSkeleton width="100px" height="24px" />
                </div>
                <div class="space-y-2">
                    <BaseSkeleton width="100%" height="16px" />
                    <BaseSkeleton width="80%" height="16px" />
                </div>
            </div>
        </div>

        <div v-else-if="error"
            class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative dark:bg-red-900 dark:border-red-700 dark:text-red-300">
            {{ error }}
        </div>

        <div v-else-if="orders.length === 0" class="text-center py-12 bg-white dark:bg-gray-800 rounded-lg shadow">
            <ShoppingBagIcon class="h-16 w-16 mx-auto text-gray-400 mb-4" />
            <h3 class="text-lg font-medium text-gray-900 dark:text-white">No tienes pedidos aún</h3>
            <p class="text-gray-500 dark:text-gray-400 mt-2">¡Explora nuestra tienda y realiza tu primera compra!</p>
            <router-link to="/shop"
                class="mt-6 inline-block bg-indigo-600 text-white px-6 py-2 rounded-md hover:bg-indigo-700 transition-colors">
                Ir a la Tienda
            </router-link>
        </div>

        <div v-else class="space-y-6">
            <div v-for="order in orders" :key="order.id"
                class="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden border border-gray-200 dark:border-gray-700 transition-all hover:shadow-md">
                <!-- Order Header -->
                <div class="bg-gray-50 dark:bg-gray-700 px-6 py-4 flex flex-wrap items-center justify-between gap-4">
                    <div>
                        <p class="text-sm text-gray-500 dark:text-gray-400">Pedido realizado el</p>
                        <p class="font-medium text-gray-900 dark:text-white">{{ formatDate(order.orderDate) }}</p>
                    </div>
                    <div>
                        <p class="text-sm text-gray-500 dark:text-gray-400">Total</p>
                        <p class="font-medium text-gray-900 dark:text-white">${{ order.totalAmount.toFixed(2) }}</p>
                    </div>
                    <div>
                        <p class="text-sm text-gray-500 dark:text-gray-400">Pedido #</p>
                        <p class="font-medium text-gray-900 dark:text-white">{{ order.id }}</p>
                    </div>
                    <div class="ml-auto">
                        <span
                            :class="['px-3 py-1 rounded-full text-sm font-medium', statusColors[order.status] || 'bg-gray-100 text-gray-800']">
                            {{ statusLabels[order.status] || order.status }}
                        </span>
                    </div>
                </div>

                <!-- Order Items -->
                <div class="px-6 py-4">
                    <ul class="divide-y divide-gray-200 dark:divide-gray-700">
                        <li v-for="item in order.items" :key="item.productId"
                            class="py-4 flex items-center justify-between">
                            <div class="flex items-center">
                                <!-- Placeholder image since order items don't have image URL in DTO yet, or we can fetch it if available -->
                                <div class="ml-0">
                                    <p class="text-sm font-medium text-gray-900 dark:text-white">{{ item.productName }}
                                    </p>
                                    <p class="text-sm text-gray-500 dark:text-gray-400">Cantidad: {{ item.quantity }}
                                    </p>
                                </div>
                            </div>
                            <p class="text-sm font-medium text-gray-900 dark:text-white">${{ item.unitPrice.toFixed(2)
                            }}</p>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
</template>
