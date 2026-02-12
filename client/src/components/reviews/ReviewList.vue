<script setup lang="ts">
import { onMounted } from 'vue';
import { useReviewStore } from '@/stores/reviews';
import { useAuthStore } from '@/stores/auth';
import { storeToRefs } from 'pinia';
import { TrashIcon, StarIcon } from '@heroicons/vue/24/solid';
import { StarIcon as StarOutlineIcon } from '@heroicons/vue/24/outline';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

const props = defineProps<{
  productId: number;
}>();

const reviewStore = useReviewStore();
const authStore = useAuthStore();
const { reviews, isLoading } = storeToRefs(reviewStore);

onMounted(() => {
  reviewStore.fetchProductReviews(props.productId);
});

const handleDelete = async (reviewId: number) => {
  if (confirm('¿Estás seguro de eliminar esta reseña?')) {
    await reviewStore.deleteReview(reviewId);
  }
};

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString();
};
</script>

<template>
  <div class="space-y-6">
    <h3 class="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
      Reseñas de Clientes
      <span class="text-sm font-normal text-gray-500 dark:text-gray-400">({{ reviews.length }})</span>
    </h3>

    <div v-if="isLoading" class="flex justify-center py-8">
      <LoadingSpinner />
    </div>

    <div v-else-if="reviews.length === 0"
      class="text-center py-12 bg-gray-50 dark:bg-gray-800 rounded-lg border border-dashed border-gray-300 dark:border-gray-700">
      <p class="text-gray-500 dark:text-gray-400">No hay reseñas aún. ¡Sé el primero en opinar!</p>
    </div>

    <div v-else class="space-y-4">
      <div v-for="review in reviews" :key="review.id"
        class="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-100 dark:border-gray-700 transition-all hover:shadow-md">
        <div class="flex justify-between items-start mb-3">
          <div class="flex items-center gap-3">
            <div class="rounded-full bg-indigo-600 text-white w-10 h-10 flex items-center justify-center shadow-sm">
              <span class="text-sm font-bold">{{ review.userName.charAt(0).toUpperCase() }}</span>
            </div>
            <div>
              <p class="font-semibold text-gray-900 dark:text-white">{{ review.userName }}</p>
              <div class="flex items-center gap-1">
                <div class="flex text-yellow-400">
                  <component :is="i <= review.rating ? StarIcon : StarOutlineIcon" v-for="i in 5" :key="i"
                    class="w-4 h-4" />
                </div>
                <span class="text-xs text-gray-400">• {{ formatDate(review.createdAt) }}</span>
              </div>
            </div>
          </div>

          <button v-if="authStore.user?.id === review.userId || authStore.user?.rol === 'Admin'"
            @click="handleDelete(review.id)"
            class="text-gray-400 hover:text-red-500 transition-colors p-1 rounded-full hover:bg-red-50 dark:hover:bg-red-900/20"
            title="Eliminar reseña">
            <TrashIcon class="w-5 h-5" />
          </button>
        </div>

        <p class="text-gray-600 dark:text-gray-300 text-sm leading-relaxed pl-[52px]">{{ review.comment }}</p>
      </div>
    </div>
  </div>
</template>
