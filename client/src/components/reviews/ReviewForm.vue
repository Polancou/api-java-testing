<script setup lang="ts">
import { ref } from 'vue';
import { useReviewStore } from '@/stores/reviews';
import BaseButton from '@/components/common/BaseButton.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { StarIcon } from '@heroicons/vue/24/solid';
import { StarIcon as StarOutlineIcon } from '@heroicons/vue/24/outline';

const props = defineProps<{
  productId: number;
}>();

const emit = defineEmits(['review-added']);

const reviewStore = useReviewStore();
const rating = ref(0);
const comment = ref('');
const isSubmitting = ref(false);

const setRating = (value: number) => {
  rating.value = value;
};

const handleSubmit = async () => {
  if (rating.value === 0) return;

  isSubmitting.value = true;
  const success = await reviewStore.addReview({
    productId: props.productId,
    rating: rating.value,
    comment: comment.value
  });

  if (success) {
    rating.value = 0;
    comment.value = '';
    emit('review-added');
  }
  isSubmitting.value = false;
};
</script>

<template>
  <div class="bg-gray-50 dark:bg-gray-800/50 p-6 rounded-xl border border-gray-200 dark:border-gray-700">
    <h4 class="text-lg font-semibold mb-4 text-gray-900 dark:text-white">Escribe una reseña</h4>

    <div class="mb-5">
      <label class="block text-sm font-medium mb-2 text-gray-700 dark:text-gray-300">Calificación</label>
      <div class="flex gap-2">
        <button v-for="i in 5" :key="i" @click="setRating(i)" type="button"
          class="focus:outline-none transition-all hover:scale-110 active:scale-95" :title="`${i} estrellas`">
          <component :is="i <= rating ? StarIcon : StarOutlineIcon" class="w-8 h-8"
            :class="i <= rating ? 'text-yellow-400 drop-shadow-sm' : 'text-gray-300 dark:text-gray-600 hover:text-yellow-400'" />
        </button>
      </div>
    </div>

    <div class="mb-5">
      <label class="block text-sm font-medium mb-2 text-gray-700 dark:text-gray-300">Comentario</label>
      <textarea v-model="comment" rows="4" class="input-field resize-none"
        placeholder="Comparte tu experiencia con este producto..."></textarea>
    </div>

    <div class="flex justify-end">
      <BaseButton @click="handleSubmit" :disabled="rating === 0 || !comment || isSubmitting" variant="primary"
        class="w-full sm:w-auto">
        <div class="flex items-center gap-2">
          <LoadingSpinner v-if="isSubmitting" class="w-4 h-4" />
          <span>{{ isSubmitting ? 'Publicando...' : 'Publicar Reseña' }}</span>
        </div>
      </BaseButton>
    </div>
  </div>
</template>
