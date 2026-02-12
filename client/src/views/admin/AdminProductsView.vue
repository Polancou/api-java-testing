<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import apiClient from '@/services/api';
import type { ProductDto } from '@/types/dto';
import { toast } from 'vue-sonner';
import { MagnifyingGlassIcon, ChevronLeftIcon, ChevronRightIcon } from '@heroicons/vue/24/solid';

const products = ref<ProductDto[]>([]);
const loading = ref(false);

// Pagination & Search state
const searchTerm = ref('');
const currentPage = ref(1);
const totalPages = ref(1);
const totalCount = ref(0);
const pageSize = 10;

const fetchProducts = async () => {
    loading.value = true;
    try {
        const response = await apiClient.get<{ items: ProductDto[], totalPages: number, totalCount: number }>('/v1/products', {
            params: {
                search: searchTerm.value,
                page: currentPage.value,
                pageSize: pageSize
            }
        });
        products.value = response.data.items;
        totalPages.value = response.data.totalPages;
        totalCount.value = response.data.totalCount;
    } catch (error) {
        console.error('Error fetching products:', error);
        toast.error('Error al cargar productos');
    } finally {
        loading.value = false;
    }
};

// Debounce search
let searchTimeout: ReturnType<typeof setTimeout>;
watch(searchTerm, () => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        currentPage.value = 1;
        fetchProducts();
    }, 300);
});

const changePage = (page: number) => {
    if (page >= 1 && page <= totalPages.value) {
        currentPage.value = page;
        fetchProducts();
    }
};

const deleteProduct = async (id: number) => {
    if (!confirm('¿Estás seguro de eliminar este producto?')) return;

    try {
        await apiClient.delete(`/v1/products/${id}`);
        toast.success('Producto eliminado');
        await fetchProducts();
    } catch (error) {
        console.error('Error deleting product:', error);
        toast.error('Error al eliminar el producto');
    }
};

onMounted(() => {
    fetchProducts();
});
</script>

<template>
    <div>
        <div class="sm:flex sm:items-center sm:justify-between">
            <div class="sm:flex-auto">
                <h1 class="text-2xl font-semibold text-gray-900 dark:text-white">Productos</h1>
                <p class="mt-2 text-sm text-gray-700 dark:text-gray-300">Lista de todos los productos disponibles en la
                    tienda.</p>
            </div>
            <div class="mt-4 sm:ml-16 sm:mt-0 sm:flex-none flex items-center gap-4">
                <!-- Search Bar -->
                <div class="relative rounded-md shadow-sm">
                    <div class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                        <MagnifyingGlassIcon class="h-5 w-5 text-gray-400" aria-hidden="true" />
                    </div>
                    <input v-model="searchTerm" type="text"
                        class="block w-full rounded-md border-0 py-1.5 pl-10 text-gray-900 ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 dark:bg-gray-700 dark:text-white dark:ring-gray-600"
                        placeholder="Buscar..." />
                </div>

                <router-link to="/admin/products/new"
                    class="block rounded-md bg-indigo-600 px-3 py-2 text-center text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600">
                    Agregar Producto
                </router-link>
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
                                        Nombre</th>
                                    <th scope="col"
                                        class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">
                                        Precio</th>
                                    <th scope="col"
                                        class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">
                                        Stock</th>
                                    <th scope="col"
                                        class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">
                                        Categoría</th>
                                    <th scope="col" class="relative py-3.5 pl-3 pr-4 sm:pr-6">
                                        <span class="sr-only">Acciones</span>
                                    </th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-gray-200 bg-white dark:divide-gray-700 dark:bg-gray-900">
                                <tr v-if="loading">
                                    <td colspan="5" class="text-center py-4 text-gray-500">Cargando...</td>
                                </tr>
                                <tr v-else-if="products.length === 0">
                                    <td colspan="5" class="text-center py-4 text-gray-500">No hay productos registrados.
                                    </td>
                                </tr>
                                <tr v-for="product in products" :key="product.id">
                                    <td
                                        class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 dark:text-white sm:pl-6">
                                        {{ product.name }}</td>
                                    <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500 dark:text-gray-300">${{
                                        product.price }}</td>
                                    <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500 dark:text-gray-300">{{
                                        product.stock }}</td>
                                    <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500 dark:text-gray-300">{{
                                        product.category || 'N/A' }}</td>
                                    <td
                                        class="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                                        <router-link :to="`/admin/products/${product.id}`"
                                            class="text-indigo-600 hover:text-indigo-900 dark:text-indigo-400 dark:hover:text-indigo-300 mr-4">Editar</router-link>
                                        <button @click="deleteProduct(product.id)"
                                            class="text-red-600 hover:text-red-900 dark:text-red-400 dark:hover:text-red-300">Eliminar</button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>

                        <!-- Pagination -->
                        <div
                            class="flex items-center justify-between border-t border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-3 sm:px-6">
                            <div class="flex flex-1 justify-between sm:hidden">
                                <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1"
                                    class="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 dark:bg-gray-700 dark:text-white dark:border-gray-600">
                                    Anterior
                                </button>
                                <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages"
                                    class="relative ml-3 inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 dark:bg-gray-700 dark:text-white dark:border-gray-600">
                                    Siguiente
                                </button>
                            </div>
                            <div class="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
                                <div>
                                    <p class="text-sm text-gray-700 dark:text-gray-300">
                                        Mostrando <span class="font-medium">{{ (currentPage - 1) * pageSize + 1
                                            }}</span> a
                                        <span class="font-medium">{{ Math.min(currentPage * pageSize, totalCount)
                                            }}</span>
                                        de <span class="font-medium">{{ totalCount }}</span> resultados
                                    </p>
                                </div>
                                <div>
                                    <nav class="isolate inline-flex -space-x-px rounded-md shadow-sm"
                                        aria-label="Pagination">
                                        <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1"
                                            class="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0 disabled:opacity-50 dark:ring-gray-600 dark:hover:bg-gray-700">
                                            <span class="sr-only">Anterior</span>
                                            <ChevronLeftIcon class="h-5 w-5" aria-hidden="true" />
                                        </button>
                                        <button v-for="page in totalPages" :key="page" @click="changePage(page)" :class="[
                                            page === currentPage
                                                ? 'z-10 bg-indigo-600 text-white focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600'
                                                : 'text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:outline-offset-0 dark:text-gray-300 dark:ring-gray-600 dark:hover:bg-gray-700',
                                            'relative inline-flex items-center px-4 py-2 text-sm font-semibold'
                                        ]">
                                            {{ page }}
                                        </button>
                                        <button @click="changePage(currentPage + 1)"
                                            :disabled="currentPage === totalPages"
                                            class="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0 disabled:opacity-50 dark:ring-gray-600 dark:hover:bg-gray-700">
                                            <span class="sr-only">Siguiente</span>
                                            <ChevronRightIcon class="h-5 w-5" aria-hidden="true" />
                                        </button>
                                    </nav>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
