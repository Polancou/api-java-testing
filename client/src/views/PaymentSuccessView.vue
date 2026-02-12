<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import apiClient from '@/services/api';
import { useCartStore } from '@/stores/cart';
import { CheckCircleIcon } from '@heroicons/vue/24/solid';
import BaseButton from '@/components/common/BaseButton.vue';

const route = useRoute();
const router = useRouter();
const cartStore = useCartStore();

const loading = ref(true);
const success = ref(false);
const error = ref('');
const orderId = ref<number | null>(null);

onMounted(async () => {
    const paymentIntentId = route.query.payment_intent;
    const clientSecret = route.query.payment_intent_client_secret;

    if (!paymentIntentId || !clientSecret) {
        error.value = 'Información de pago no válida.';
        loading.value = false;
        return;
    }

    try {
        // Confirm payment and create order
        const response = await apiClient.post<{ id: number }>('/v1/payments/confirm', {
            paymentIntentId: paymentIntentId,
            shippingAddress: cartStore.selectedShippingAddress
        });

        orderId.value = response.data.id;
        success.value = true;

        // Clear local cart
        cartStore.clearCart();
    } catch (err: unknown) {
        console.error('Error confirming payment:', err);
        if (err && typeof err === 'object' && 'response' in err) {
            const errorResponse = (err as { response: { data: { message: string } } }).response;
            error.value = errorResponse?.data?.message || 'Error al procesar el pedido.';
        } else {
            error.value = 'Error al procesar el pedido.';
        }
    } finally {
        loading.value = false;
    }
});
</script>

<template>
    <div class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 py-12 px-4 sm:px-6 lg:px-8">
        <div class="max-w-md w-full space-y-8 text-center">
            <div v-if="loading">
                <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
                <h2 class="mt-6 text-3xl font-extrabold text-gray-900 dark:text-white">Procesando tu pedido...</h2>
                <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">Por favor espera un momento.</p>
            </div>

            <div v-else-if="success">
                <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100">
                    <CheckCircleIcon class="h-6 w-6 text-green-600" aria-hidden="true" />
                </div>
                <h2 class="mt-6 text-3xl font-extrabold text-gray-900 dark:text-white">¡Pago Exitoso!</h2>
                <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
                    Tu pedido #{{ orderId }} ha sido confirmado.
                </p>
                <div class="mt-6">
                    <router-link to="/profile" class="text-indigo-600 hover:text-indigo-500 font-medium">
                        Ver mis pedidos
                    </router-link>
                </div>
                <div class="mt-4">
                    <BaseButton @click="router.push('/shop')">Seguir Comprando</BaseButton>
                </div>
            </div>

            <div v-else>
                <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100">
                    <svg class="h-6 w-6 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                            d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </div>
                <h2 class="mt-6 text-3xl font-extrabold text-gray-900 dark:text-white">Error en el Pedido</h2>
                <p class="mt-2 text-sm text-red-600">{{ error }}</p>
                <div class="mt-6">
                    <BaseButton @click="router.push('/checkout')">Intentar de nuevo</BaseButton>
                </div>
            </div>
        </div>
    </div>
</template>
