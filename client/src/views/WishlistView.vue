<script setup lang="ts">
import { onMounted } from 'vue';
import { useWishlistStore } from '@/stores/wishlist';
import { useCartStore } from '@/stores/cart';
import { storeToRefs } from 'pinia';
import { getImageUrl } from '@/utils/image';
import { TrashIcon, ShoppingCartIcon, HeartIcon } from '@heroicons/vue/24/outline';
import type { WishlistItemDto } from '@/types/dto';
import BaseButton from '@/components/common/BaseButton.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

const wishlistStore = useWishlistStore();
const cartStore = useCartStore();
const { items, isLoading } = storeToRefs(wishlistStore);

onMounted(() => {
  wishlistStore.fetchWishlist();
});

const handleAddToCart = async (item: WishlistItemDto) => {
  await cartStore.addItem({
    id: item.productId,
    name: item.productName,
    price: item.productPrice,
    stock: 10, // Assuming stock is available, or fetch details
    description: '',
    imageUrl: item.productImageUrl
  });
};
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-8 flex items-center gap-3">
      <HeartIcon class="w-8 h-8 text-red-500" />
      Mi Lista de Deseos
    </h1>

    <div v-if="isLoading" class="flex justify-center py-12">
      <LoadingSpinner />
    </div>

    <div v-else-if="items.length === 0"
      class="text-center py-16 bg-gray-50 dark:bg-gray-800 rounded-xl border border-dashed border-gray-300 dark:border-gray-700">
      <HeartIcon class="w-16 h-16 mx-auto text-gray-300 dark:text-gray-600 mb-4" />
      <h3 class="text-lg font-medium text-gray-900 dark:text-white">Tu lista de deseos está vacía</h3>
      <p class="text-gray-500 dark:text-gray-400 mt-2 mb-6">Guarda los productos que te gustan para comprarlos después.
      </p>
      <router-link to="/shop">
        <BaseButton variant="primary">Explorar Tienda</BaseButton>
      </router-link>
    </div>

    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      <div v-for="item in items" :key="item.id"
        class="bg-white dark:bg-gray-800 shadow rounded-lg overflow-hidden hover:shadow-md transition-shadow border border-gray-100 dark:border-gray-700">
        <figure class="relative pt-4 px-4">
          <img :src="getImageUrl(item.productImageUrl)" :alt="item.productName"
            class="rounded-xl h-48 w-full object-cover" />
          <button @click="wishlistStore.removeFromWishlist(item.productId)"
            class="absolute top-6 right-6 p-2 bg-white dark:bg-gray-800 rounded-full shadow-sm hover:bg-red-50 dark:hover:bg-red-900/30 text-gray-400 hover:text-red-500 transition-colors"
            title="Eliminar de lista de deseos">
            <TrashIcon class="w-5 h-5" />
          </button>
        </figure>
        <div class="p-4">
          <h2 class="text-base font-semibold text-gray-900 dark:text-white line-clamp-1" :title="item.productName">
            {{ item.productName }}
          </h2>
          <p class="text-lg font-bold text-indigo-600 dark:text-indigo-400 mt-1">${{ item.productPrice }}</p>
          <div class="mt-4">
            <BaseButton @click="handleAddToCart(item)" variant="primary" size="sm" :fullWidth="true">
              <div class="flex items-center justify-center gap-2">
                <ShoppingCartIcon class="w-4 h-4" />
                Agregar al Carrito
              </div>
            </BaseButton>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
