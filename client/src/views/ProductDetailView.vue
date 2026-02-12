<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import apiClient from '@/services/api';
import type { ProductDto, ApiErrorResponse } from '@/types/dto';
import { useCartStore } from '@/stores/cart';
import { getImageUrl } from '@/utils/image';
import { ShoppingCartIcon } from '@heroicons/vue/24/solid';
import WishlistButton from '@/components/common/WishlistButton.vue';
import ReviewList from '@/components/reviews/ReviewList.vue';
import ReviewForm from '@/components/reviews/ReviewForm.vue';
import { useAuthStore } from '@/stores/auth';
import type { AxiosError } from 'axios';
import BaseButton from '@/components/common/BaseButton.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

const route = useRoute();
const cartStore = useCartStore();
const authStore = useAuthStore();
const product = ref<ProductDto | null>(null);
const isLoading = ref(true);
const error = ref<string | null>(null);

// Force review list refresh
const reviewListKey = ref(0);

const fetchProduct = async () => {
  isLoading.value = true;
  try {
    const id = route.params.id;
    const response = await apiClient.get<ProductDto>(`/v1/products/${id}`);
    product.value = response.data;
  } catch (err: unknown) {
    const axiosError = err as AxiosError<ApiErrorResponse>;
    error.value = axiosError.response?.data?.message || 'Error al cargar el producto';
  } finally {
    isLoading.value = false;
  }
};

const handleReviewAdded = () => {
  reviewListKey.value++;
};

onMounted(() => {
  fetchProduct();
});
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <div v-if="isLoading" class="flex justify-center py-12">
      <LoadingSpinner />
    </div>

    <div v-else-if="error"
      class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative dark:bg-red-900 dark:border-red-700 dark:text-red-300">
      {{ error }}
    </div>

    <div v-else-if="product" class="grid grid-cols-1 md:grid-cols-2 gap-8">
      <!-- Product Image -->
      <div class="relative">
        <img :src="getImageUrl(product.imageUrl)" :alt="product.name"
          class="w-full rounded-lg shadow-lg object-cover h-96">
        <div class="absolute top-4 right-4">
          <WishlistButton :productId="product.id" />
        </div>
      </div>

      <!-- Product Info -->
      <div class="space-y-6">
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">{{ product.name }}</h1>
        <p class="text-2xl font-bold text-indigo-600 dark:text-indigo-400">${{ product.price }}</p>
        <p class="text-gray-600 dark:text-gray-300">{{ product.description }}</p>

        <div class="flex items-center gap-4">
          <BaseButton @click="cartStore.addItem(product)" variant="primary" :fullWidth="true">
            <div class="flex items-center justify-center gap-2">
              <ShoppingCartIcon class="w-5 h-5" />
              Agregar al Carrito
            </div>
          </BaseButton>
        </div>

        <div class="border-t border-gray-200 dark:border-gray-700 my-8"></div>

        <!-- Reviews Section -->
        <div class="space-y-8">
          <ReviewForm v-if="authStore.isAuthenticated" :productId="product.id" @review-added="handleReviewAdded" />
          <div v-else class="bg-blue-50 border-l-4 border-blue-500 p-4 dark:bg-blue-900/20 dark:border-blue-700">
            <p class="text-blue-700 dark:text-blue-300">
              <router-link :to="{ name: 'login' }" class="font-bold hover:underline">Inicia sesión</router-link>
              para dejar una reseña.
            </p>
          </div>

          <ReviewList :productId="product.id" :key="reviewListKey" />
        </div>
      </div>
    </div>
  </div>
</template>
