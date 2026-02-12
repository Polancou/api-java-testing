<script setup lang="ts">
import { ref, watch } from 'vue';
import { useProductStore } from '@/stores/products';
import { storeToRefs } from 'pinia';
import BaseButton from '@/components/common/BaseButton.vue';
import { FunnelIcon, XMarkIcon } from '@heroicons/vue/24/outline';

const productStore = useProductStore();
const { filters } = storeToRefs(productStore);

const localSearch = ref(filters.value.searchTerm || '');
const localCategory = ref(filters.value.category || '');
const localMinPrice = ref(filters.value.minPrice);
const localMaxPrice = ref(filters.value.maxPrice);
const isOpen = ref(false);

// Debounce search
let timeout: ReturnType<typeof setTimeout>;
watch(localSearch, (newVal) => {
  clearTimeout(timeout);
  timeout = setTimeout(() => {
    productStore.setFilters({ searchTerm: newVal });
  }, 500);
});

const applyFilters = () => {
  productStore.setFilters({
    category: localCategory.value,
    minPrice: localMinPrice.value,
    maxPrice: localMaxPrice.value
  });
  isOpen.value = false; // Close on mobile after applying
};

const resetFilters = () => {
  localSearch.value = '';
  localCategory.value = '';
  localMinPrice.value = undefined;
  localMaxPrice.value = undefined;
  productStore.clearFilters();
  isOpen.value = false;
};
</script>

<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700">
    <!-- Mobile Header -->
    <div class="p-4 flex justify-between items-center lg:hidden">
      <h3 class="font-semibold text-lg text-gray-900 dark:text-white flex items-center gap-2">
        <FunnelIcon class="w-5 h-5" /> Filtros
      </h3>
      <BaseButton @click="isOpen = !isOpen" variant="secondary" class="!p-2">
        <component :is="isOpen ? XMarkIcon : FunnelIcon" class="w-5 h-5" />
      </BaseButton>
    </div>

    <!-- Filter Content (Collapsible on mobile) -->
    <div :class="{ 'hidden': !isOpen, 'block': true }"
      class="p-4 space-y-6 lg:block border-t lg:border-t-0 border-gray-200 dark:border-gray-700">
      <h3 class="font-semibold text-lg text-gray-900 dark:text-white hidden lg:flex items-center gap-2 mb-4">
        <FunnelIcon class="w-5 h-5" /> Filtros
      </h3>

      <!-- Search -->
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Buscar</label>
        <input v-model="localSearch" type="text" placeholder="Nombre o descripción..." class="input-field" />
      </div>

      <!-- Category -->
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Categoría</label>
        <select v-model="localCategory" class="input-field">
          <option value="">Todas</option>
          <option value="Electronics">Electrónica</option>
          <option value="Clothing">Ropa</option>
          <option value="Home">Hogar</option>
          <option value="Books">Libros</option>
        </select>
      </div>

      <!-- Price Range -->
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Precio</label>
        <div class="flex gap-2 items-center">
          <input v-model.number="localMinPrice" type="number" placeholder="Min" class="input-field" />
          <span class="text-gray-500">-</span>
          <input v-model.number="localMaxPrice" type="number" placeholder="Max" class="input-field" />
        </div>
      </div>

      <!-- Actions -->
      <div class="flex gap-2 pt-4">
        <BaseButton @click="applyFilters" variant="primary" class="flex-1">Aplicar</BaseButton>
        <BaseButton @click="resetFilters" variant="secondary" class="flex-1">Limpiar</BaseButton>
      </div>
    </div>
  </div>
</template>
