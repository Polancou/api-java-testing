<script setup lang="ts">
import { onMounted } from 'vue';
import { useProductStore } from '@/stores/products';
import { useCartStore } from '@/stores/cart';
import { storeToRefs } from 'pinia';
import { ShoppingCartIcon, ChevronLeftIcon, ChevronRightIcon } from '@heroicons/vue/24/solid';
import { getImageUrl } from '@/utils/image';
import ProductFilters from '@/components/products/ProductFilters.vue';
import WishlistButton from '@/components/common/WishlistButton.vue';
import BaseButton from '@/components/common/BaseButton.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

const productStore = useProductStore();
const cartStore = useCartStore();
const { products, isLoading, currentPage, totalCount, pageSize } = storeToRefs(productStore);

const changePage = (page: number) => {
  if (page >= 1 && page <= Math.ceil(totalCount.value / pageSize.value)) {
    productStore.fetchProducts(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
};

onMounted(() => {
  productStore.fetchProducts();
});
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-8">Tienda</h1>

    <div class="flex flex-col lg:flex-row gap-8">
      <!-- Sidebar Filters -->
      <aside class="w-full lg:w-64 flex-shrink-0">
        <ProductFilters />
      </aside>

      <!-- Product Grid -->
      <main class="flex-1">
        <div v-if="isLoading" class="flex justify-center items-center h-64">
          <LoadingSpinner />
        </div>

        <div v-else-if="products.length === 0" class="text-center py-12">
          <p class="text-gray-500 dark:text-gray-400 text-lg">No se encontraron productos.</p>
        </div>

        <div v-else>
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
            <div v-for="product in products" :key="product.id"
              class="bg-white dark:bg-gray-800 shadow rounded-lg overflow-hidden hover:shadow-lg transition-shadow duration-300">
              <figure class="relative">
                <router-link :to="{ name: 'product-detail', params: { id: product.id } }">
                  <img :src="getImageUrl(product.imageUrl)" :alt="product.name"
                    class="w-full h-48 object-cover hover:opacity-90 transition-opacity">
                </router-link>
                <div class="absolute top-2 right-2">
                  <WishlistButton :productId="product.id" />
                </div>
              </figure>
              <div class="p-5">
                <h2 class="text-lg font-bold text-gray-900 dark:text-white line-clamp-1" :title="product.name">
                  {{ product.name }}
                </h2>
                <p class="text-sm text-gray-600 dark:text-gray-300 line-clamp-2 h-10 mb-4">{{ product.description }}</p>
                <div class="flex items-center justify-between mt-auto">
                  <span class="text-xl font-bold text-gray-900 dark:text-white">${{ product.price }}</span>
                  <BaseButton @click="cartStore.addItem(product)" variant="primary" size="sm">
                    <div class="flex items-center gap-2">
                      <ShoppingCartIcon class="h-4 w-4" />
                      Agregar
                    </div>
                  </BaseButton>
                </div>
              </div>
            </div>
          </div>

          <!-- Pagination -->
          <div class="flex justify-center mt-8 pb-8">
            <div class="flex items-center space-x-2">
              <BaseButton variant="secondary" :disabled="currentPage === 1" @click="changePage(currentPage - 1)"
                class="!px-3">
                <ChevronLeftIcon class="h-5 w-5" />
              </BaseButton>

              <span class="px-4 py-2 text-gray-700 dark:text-gray-200 font-medium">
                Página {{ currentPage }}
              </span>

              <BaseButton variant="secondary" :disabled="currentPage * pageSize >= totalCount"
                @click="changePage(currentPage + 1)" class="!px-3">
                <ChevronRightIcon class="h-5 w-5" />
              </BaseButton>
            </div>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>
