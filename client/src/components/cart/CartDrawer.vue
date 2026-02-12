<template>
  <div v-if="cartStore.isOpen" class="relative z-[100]" aria-labelledby="slide-over-title" role="dialog"
    aria-modal="true">
    <!-- Background backdrop -->
    <div class="fixed inset-0 bg-gray-500/75 transition-opacity" @click="cartStore.closeCart()"></div>

    <div class="fixed inset-0 overflow-hidden pointer-events-none">
      <div class="absolute inset-0 overflow-hidden">
        <div class="pointer-events-none fixed inset-y-0 right-0 flex max-w-full pl-10">
          <div class="pointer-events-auto w-screen max-w-md">
            <div class="flex h-full flex-col overflow-y-scroll bg-white shadow-xl dark:bg-gray-800">
              <div class="flex-1 overflow-y-auto px-4 py-6 sm:px-6">
                <div class="flex items-start justify-between">
                  <h2 class="text-lg font-medium text-gray-900 dark:text-white" id="slide-over-title">Tu Carrito</h2>
                  <div class="ml-3 flex h-7 items-center">
                    <button type="button" class="relative -m-2 p-2 text-gray-400 hover:text-gray-500"
                      @click="cartStore.closeCart()">
                      <span class="absolute -inset-0.5"></span>
                      <span class="sr-only">Cerrar panel</span>
                      <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor"
                        aria-hidden="true">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>
                </div>

                <div class="mt-8">
                  <div class="flow-root">
                    <ul role="list" class="-my-6 divide-y divide-gray-200 dark:divide-gray-700">
                      <li v-for="item in cartStore.items" :key="item.productId" class="flex py-6">
                        <div
                          class="h-24 w-24 flex-shrink-0 overflow-hidden rounded-md border border-gray-200 dark:border-gray-700">
                          <img :src="item.imageUrl || 'https://placehold.co/600x400'" :alt="item.name"
                            class="h-full w-full object-cover object-center">
                        </div>

                        <div class="ml-4 flex flex-1 flex-col">
                          <div>
                            <div class="flex justify-between text-base font-medium text-gray-900 dark:text-white">
                              <h3>
                                <a href="#">{{ item.name }}</a>
                              </h3>
                              <p class="ml-4">${{ item.price }}</p>
                            </div>
                          </div>
                          <div class="flex flex-1 items-end justify-between text-sm">
                            <div class="flex items-center gap-2 border rounded px-2 py-1 dark:border-gray-600">
                              <button @click="cartStore.updateQuantity(item.productId, item.quantity - 1)"
                                class="text-gray-500 hover:text-gray-700 dark:text-gray-400">-</button>
                              <p class="text-gray-500 dark:text-gray-300 w-4 text-center">{{ item.quantity }}</p>
                              <button @click="cartStore.updateQuantity(item.productId, item.quantity + 1)"
                                class="text-gray-500 hover:text-gray-700 dark:text-gray-400">+</button>
                            </div>

                            <div class="flex">
                              <button type="button"
                                class="font-medium text-indigo-600 hover:text-indigo-500 dark:text-indigo-400"
                                @click="cartStore.removeItem(item.productId)">Eliminar</button>
                            </div>
                          </div>
                        </div>
                      </li>
                      <li v-if="cartStore.items.length === 0" class="py-6 text-center text-gray-500 dark:text-gray-400">
                        Tu carrito está vacío.
                      </li>
                    </ul>
                  </div>
                </div>
              </div>

              <div class="border-t border-gray-200 px-4 py-6 sm:px-6 dark:border-gray-700">
                <div class="flex justify-between text-base font-medium text-gray-900 dark:text-white">
                  <p>Subtotal</p>
                  <p>${{ cartStore.totalPrice.toFixed(2) }}</p>
                </div>
                <p class="mt-0.5 text-sm text-gray-500 dark:text-gray-400">Envío e impuestos calculados al finalizar.
                </p>
                <div class="mt-6">
                  <router-link to="/checkout" @click="cartStore.closeCart()"
                    class="flex items-center justify-center rounded-md border border-transparent bg-indigo-600 px-6 py-3 text-base font-medium text-white shadow-sm hover:bg-indigo-700">Checkout</router-link>
                </div>
                <div class="mt-6 flex justify-center text-center text-sm text-gray-500">
                  <p>
                    o
                    <button type="button" class="font-medium text-indigo-600 hover:text-indigo-500 dark:text-indigo-400"
                      @click="cartStore.closeCart()">
                      Continuar Comprando
                      <span aria-hidden="true"> &rarr;</span>
                    </button>
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useCartStore } from '@/stores/cart';

const cartStore = useCartStore();
</script>
