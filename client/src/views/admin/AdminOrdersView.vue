<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '@/services/api';
import type { OrderDto } from '@/types/dto';
import { toast } from 'vue-sonner';

const orders = ref<OrderDto[]>([]);
const loading = ref(false);

const statusOptions = [
    'Pending',
    'Paid',
    'Shipped',
    'Delivered',
    'Cancelled'
];

const fetchOrders = async () => {
    loading.value = true;
    try {
        const response = await apiClient.get<OrderDto[]>('/v1/orders');
        orders.value = response.data;
    } catch (error) {
        console.error('Error fetching orders:', error);
        toast.error('Error al cargar pedidos');
    } finally {
        loading.value = false;
    }
};

const updateStatus = async (orderId: number, newStatus: string) => {
    try {
        await apiClient.put(`/v1/orders/${orderId}/status`, { status: newStatus });
        toast.success('Estado actualizado');
        // Update local state
        const order = orders.value.find((o: OrderDto) => o.id === orderId);
        if (order) {
            order.status = newStatus;
        }
    } catch (error) {
        console.error('Error updating status:', error);
        toast.error('Error al actualizar estado');
    }
};

const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
};

onMounted(() => {
    fetchOrders();
});
</script>

<template>
    <div>
        <div class="sm:flex sm:items-center">
            <div class="sm:flex-auto">
                <h1 class="text-2xl font-semibold text-gray-900 dark:text-white">Pedidos</h1>
                <p class="mt-2 text-sm text-gray-700 dark:text-gray-300">Lista de todos los pedidos realizados.</p>
            </div>
        </div>

        <div class="mt-8 flow-root">
            <div class="-mx-4 -my-2 overflow-x-auto sm:-mx-6 lg:-mx-8">
                <div class="inline-block min-w-full py-2 align-middle sm:px-6 lg:px-8">
                    <div class="overflow-hidden shadow sm:rounded-lg border border-gray-200 dark:border-gray-700">
                        <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                            <thead class="bg-gray-50 dark:bg-gray-800">
                                <tr>
                                    <th scope="col"
                                        class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 dark:text-white sm:pl-6">
                                        ID</th>
                                    <th scope="col"
                                        class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">
                                        Fecha</th>
                                    <th scope="col"
                                        class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">
                                        Total</th>
                                    <th scope="col"
                                        class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">
                                        Estado</th>
                                    <th scope="col"
                                        class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">
                                        Acciones</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-gray-200 bg-white dark:divide-gray-700 dark:bg-gray-900">
                                <tr v-if="loading">
                                    <td colspan="5" class="text-center py-4 text-gray-500">Cargando...</td>
                                </tr>
                                <tr v-else-if="orders.length === 0">
                                    <td colspan="5" class="text-center py-4 text-gray-500">No hay pedidos registrados.
                                    </td>
                                </tr>
                                <tr v-for="order in orders" :key="order.id">
                                    <td
                                        class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 dark:text-white sm:pl-6">
                                        #{{ order.id }}</td>
                                    <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500 dark:text-gray-300">{{
                                        formatDate(order.orderDate) }}</td>
                                    <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500 dark:text-gray-300">${{
                                        order.totalAmount }}</td>
                                    <td class="whitespace-nowrap px-3 py-4 text-sm">
                                        <span :class="{
                                            'bg-yellow-100 text-yellow-800': order.status === 'Pending',
                                            'bg-green-100 text-green-800': order.status === 'Paid',
                                            'bg-blue-100 text-blue-800': order.status === 'Shipped',
                                            'bg-purple-100 text-purple-800': order.status === 'Delivered',
                                            'bg-red-100 text-red-800': order.status === 'Cancelled',
                                        }"
                                            class="inline-flex items-center rounded-md px-2 py-1 text-xs font-medium ring-1 ring-inset ring-gray-500/10">
                                            {{ order.status }}
                                        </span>
                                    </td>
                                    <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500 dark:text-gray-300">
                                        <select :value="order.status"
                                            @change="updateStatus(order.id, ($event.target as HTMLSelectElement).value)"
                                            class="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 dark:bg-gray-700 dark:text-white dark:ring-gray-600">
                                            <option v-for="status in statusOptions" :key="status" :value="status">
                                                {{ status }}
                                            </option>
                                        </select>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
