<script setup lang="ts">
import { computed } from 'vue';
import { useWishlistStore } from '@/stores/wishlist';
import { HeartIcon } from '@heroicons/vue/24/solid';
import { HeartIcon as HeartOutlineIcon } from '@heroicons/vue/24/outline';

const props = defineProps<{
  productId: number;
}>();

const wishlistStore = useWishlistStore();
const isInWishlist = computed(() => wishlistStore.isInWishlist(props.productId));

const toggleWishlist = async () => {
  await wishlistStore.toggleWishlist(props.productId);
};
</script>

<template>
  <button @click.stop="toggleWishlist"
    class="p-2 rounded-full hover:bg-red-50 hover:text-red-500 transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 dark:hover:bg-red-900/30"
    :class="isInWishlist ? 'text-red-500' : 'text-gray-400 dark:text-gray-500'" title="Agregar a lista de deseos">
    <component :is="isInWishlist ? HeartIcon : HeartOutlineIcon" class="w-5 h-5" />
  </button>
</template>
