<script setup lang="ts">
import { ref } from 'vue';
import { RouterView } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import BaseButton from '@/components/common/BaseButton.vue';
import CartDrawer from '@/components/cart/CartDrawer.vue';
import { useCartStore } from '@/stores/cart';
import {
  UserIcon,
  ShoppingBagIcon,
  Cog6ToothIcon,
  ShoppingCartIcon,
  Bars3Icon,
  XMarkIcon
} from '@heroicons/vue/24/outline';

const authStore = useAuthStore();
const cartStore = useCartStore();
const isMobileMenuOpen = ref(false);

// Función para cerrar sesión
const handleLogout = () => {
  isMobileMenuOpen.value = false;
  authStore.logout()
}

const handleCartClick = () => {
  isMobileMenuOpen.value = false;
  cartStore.toggleCart();
}
</script>

<template>
  <div class="flex h-screen bg-gray-100 dark:bg-gray-900">
    <CartDrawer />

    <!-- Mobile Sidebar Overlay -->
    <div v-if="isMobileMenuOpen" class="fixed inset-0 z-40 bg-gray-600 bg-opacity-75 md:hidden"
      @click="isMobileMenuOpen = false"></div>

    <!-- Sidebar (Desktop & Mobile) -->
    <aside :class="[
      'fixed inset-y-0 left-0 z-50 w-64 bg-white dark:bg-gray-800 p-4 shadow-md transform transition-transform duration-300 ease-in-out md:translate-x-0 md:static md:inset-auto md:block',
      isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'
    ]">
      <div class="flex items-center justify-between mb-6">
        <h1 class="text-xl font-semibold text-gray-800 dark:text-gray-200 flex items-center gap-2">
          <ShoppingBagIcon class="h-6 w-6 text-indigo-600" />
          Ecommerce Core
        </h1>
        <button @click="isMobileMenuOpen = false"
          class="md:hidden text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200">
          <XMarkIcon class="h-6 w-6" />
        </button>
      </div>

      <nav class="space-y-2">
        <RouterLink to="/profile" @click="isMobileMenuOpen = false"
          class="flex items-center gap-3 py-2 px-4 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors">
          <UserIcon class="h-5 w-5" />
          Profile
        </RouterLink>
        <RouterLink to="/shop" @click="isMobileMenuOpen = false"
          class="flex items-center gap-3 py-2 px-4 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors">
          <ShoppingBagIcon class="h-5 w-5" />
          Tienda
        </RouterLink>
        <RouterLink to="/orders" @click="isMobileMenuOpen = false"
          class="flex items-center gap-3 py-2 px-4 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors">
          <ShoppingBagIcon class="h-5 w-5" />
          Mis Pedidos
        </RouterLink>
        <RouterLink v-if="authStore.isAdmin" to="/admin" @click="isMobileMenuOpen = false"
          class="flex items-center gap-3 py-2 px-4 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors">
          <Cog6ToothIcon class="h-5 w-5" />
          Admin Panel
        </RouterLink>
        <button @click="handleCartClick"
          class="w-full text-left flex items-center justify-between py-2 px-4 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors">
          <div class="flex items-center gap-3">
            <ShoppingCartIcon class="h-5 w-5" />
            <span>Carrito</span>
          </div>
          <span v-if="cartStore.totalItems > 0"
            class="bg-indigo-600 text-white text-xs font-bold px-2 py-0.5 rounded-full">
            {{ cartStore.totalItems }}
          </span>
        </button>
      </nav>
      <div
        class="mt-auto pt-4 border-t border-gray-200 dark:border-gray-700 absolute bottom-4 left-4 right-4 md:static md:bottom-auto md:left-auto md:right-auto">
        <BaseButton variant="danger-text" @click="handleLogout" class="w-full">
          Cerrar Sesión
        </BaseButton>
      </div>
    </aside>

    <div class="flex-1 flex flex-col overflow-hidden">
      <!-- Mobile Header -->
      <header class="bg-white dark:bg-gray-800 shadow-sm md:hidden flex items-center justify-between p-4">
        <button @click="isMobileMenuOpen = true"
          class="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200">
          <Bars3Icon class="h-6 w-6" />
        </button>
        <h1 class="text-lg font-semibold text-gray-800 dark:text-gray-200 flex items-center gap-2">
          <ShoppingBagIcon class="h-5 w-5 text-indigo-600" />
          Ecommerce Core
        </h1>
        <div class="w-6"></div> <!-- Spacer for centering -->
      </header>

      <main class="flex-1 overflow-y-auto p-4 md:p-6">
        <RouterView v-slot="{ Component }">
          <Transition name="fade" mode="out-in">
            <component :is="Component" />
          </Transition>
        </RouterView>
      </main>
    </div>
  </div>
</template>

<style scoped>
/* Ajuste para que el botón de logout quede abajo */
aside {
  display: flex;
  flex-direction: column;
}

.fade-enter-active,
.fade-leave-active {
  /* Duración de la animación (150ms) */
  transition: opacity 0.15s ease;
}

.fade-enter-from,
.fade-leave-to {
  /* Estado inicial (completamente invisible) */
  opacity: 0;
}
</style>
