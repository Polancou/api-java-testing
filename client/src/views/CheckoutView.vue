<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { loadStripe } from '@stripe/stripe-js';
import { useDark } from '@vueuse/core';
import apiClient from '@/services/api';
import { useCartStore } from '@/stores/cart';
import { toast } from 'vue-sonner';
import { useRouter } from 'vue-router';
import AddressSelection from '@/components/checkout/AddressSelection.vue';
import BaseButton from '@/components/common/BaseButton.vue';
import { CheckIcon } from '@heroicons/vue/24/solid';

import { type Stripe, type StripeElements, type StripePaymentElement } from '@stripe/stripe-js';

const publishableKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY;

const stripePromise = loadStripe(publishableKey);
const cartStore = useCartStore();
const router = useRouter();

const loading = ref(false);
const clientSecret = ref('');
const stripe = ref<Stripe | null>(null);
const elements = ref<StripeElements | null>(null);
const paymentElement = ref<StripePaymentElement | null>(null);

const isDark = useDark();

const steps = [
    { id: 1, name: 'Envío', status: 'current' },
    { id: 2, name: 'Pago', status: 'upcoming' },
];

const currentStep = ref(1);
const selectedAddress = ref(cartStore.selectedShippingAddress);

// Sync local selectedAddress with store
watch(selectedAddress, (newVal) => {
    cartStore.selectedShippingAddress = newVal;
});

const canProceedToPayment = computed(() => !!selectedAddress.value);

const nextStep = async () => {
    if (currentStep.value === 1) {
        if (!canProceedToPayment.value) {
            toast.error('Por favor selecciona una dirección de envío');
            return;
        }
        currentStep.value = 2;
        if (steps[0]) steps[0].status = 'complete';
        if (steps[1]) steps[1].status = 'current';

        // Initialize payment when moving to step 2
        await initializePayment();
    }
};

const goToAddressStep = () => {
    currentStep.value = 1;
    if (steps[0]) steps[0].status = 'current';
    if (steps[1]) steps[1].status = 'upcoming';
};

const initializePayment = async () => {
    if (clientSecret.value) return; // Already initialized

    try {
        loading.value = true;
        // 1. Create PaymentIntent on backend
        const { data } = await apiClient.post<{ clientSecret: string }>('/v1/payments/create-intent');
        clientSecret.value = data.clientSecret;

        // 2. Initialize Stripe Elements
        stripe.value = await stripePromise;
        if (!stripe.value) return;

        const theme = isDark.value ? 'night' : 'stripe';
        const appearance: { theme: 'night' | 'stripe', labels: 'floating' } = { theme, labels: 'floating' };
        elements.value = stripe.value.elements({ clientSecret: clientSecret.value, appearance });

        paymentElement.value = elements.value.create('payment');
        paymentElement.value.mount('#payment-element');
    } catch (error) {
        console.error("Error initializing payment:", error);
        toast.error("Error al iniciar el pago");
    } finally {
        loading.value = false;
    }
};

onMounted(async () => {
    if (cartStore.items.length === 0) {
        toast.error("El carrito está vacío");
        router.push({ name: 'shop' });
        return;
    }

    // If we already have an address selected, we could potentially restore state?
    // For now, start at step 1 always to confirm address.
});

// Watch for theme changes and update Stripe Elements
watch(isDark, (newVal) => {
    if (elements.value) {
        elements.value.update({ appearance: { theme: newVal ? 'night' : 'stripe', labels: 'floating' } });
    }
});

const handleSubmit = async () => {
    if (!stripe.value || !elements.value) return;

    loading.value = true;

    const { error } = await stripe.value.confirmPayment({
        elements: elements.value,
        confirmParams: {
            return_url: window.location.origin + '/payment-success',
        },
    });

    if (error) {
        toast.error(error.message || 'Error desconocido');
    } else {
        // Successful payment will redirect to return_url
    }
    loading.value = false;
};
</script>

<template>
    <div class="container mx-auto px-4 py-8 max-w-3xl">
        <h1 class="text-2xl font-bold mb-8 text-gray-900 dark:text-white">Checkout</h1>

        <!-- Stepper -->
        <nav aria-label="Progress" class="mb-10">
            <ol role="list" class="flex items-center">
                <li v-for="(step, stepIdx) in steps" :key="step.name"
                    :class="[stepIdx !== steps.length - 1 ? 'pr-8 sm:pr-20' : '', 'relative']">
                    <template v-if="step.status === 'complete'">
                        <div class="absolute inset-0 flex items-center" aria-hidden="true">
                            <div class="h-0.5 w-full bg-indigo-600" />
                        </div>
                        <a href="#"
                            class="relative flex h-8 w-8 items-center justify-center rounded-full bg-indigo-600 hover:bg-indigo-900">
                            <CheckIcon class="h-5 w-5 text-white" aria-hidden="true" />
                            <span class="sr-only">{{ step.name }}</span>
                        </a>
                    </template>
                    <template v-else-if="step.status === 'current'">
                        <div class="absolute inset-0 flex items-center" aria-hidden="true">
                            <div class="h-0.5 w-full bg-gray-200 dark:bg-gray-700" />
                        </div>
                        <a href="#"
                            class="relative flex h-8 w-8 items-center justify-center rounded-full border-2 border-indigo-600 bg-white dark:bg-gray-900"
                            aria-current="step">
                            <span class="h-2.5 w-2.5 rounded-full bg-indigo-600" aria-hidden="true" />
                            <span class="sr-only">{{ step.name }}</span>
                        </a>
                    </template>
                    <template v-else>
                        <div class="absolute inset-0 flex items-center" aria-hidden="true">
                            <div class="h-0.5 w-full bg-gray-200 dark:bg-gray-700" />
                        </div>
                        <a href="#"
                            class="group relative flex h-8 w-8 items-center justify-center rounded-full border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 hover:border-gray-400">
                            <span class="h-2.5 w-2.5 rounded-full bg-transparent group-hover:bg-gray-300"
                                aria-hidden="true" />
                            <span class="sr-only">{{ step.name }}</span>
                        </a>
                    </template>
                    <span class="absolute top-10 left-1/2 -translate-x-1/2 text-sm font-medium"
                        :class="step.status === 'current' ? 'text-indigo-600' : 'text-gray-500'">{{ step.name }}</span>
                </li>
            </ol>
        </nav>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <!-- Main Content (Steps) -->
            <div class="lg:col-span-2">
                <!-- Step 1: Address Selection -->
                <div v-show="currentStep === 1" class="bg-white dark:bg-gray-800 p-6 rounded-lg shadow-lg">
                    <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Selecciona una dirección de
                        envío</h2>
                    <AddressSelection v-model="selectedAddress" />

                    <div class="mt-6 flex justify-end">
                        <BaseButton @click="nextStep" :disabled="!canProceedToPayment" variant="primary">
                            Continuar al Pago
                        </BaseButton>
                    </div>
                </div>

                <!-- Step 2: Payment -->
                <div v-show="currentStep === 2" class="bg-white dark:bg-gray-800 p-6 rounded-lg shadow-lg">
                    <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Información de Pago</h2>

                    <form id="payment-form" @submit.prevent="handleSubmit">
                        <div id="payment-element" class="mb-6 min-h-[200px]">
                            <!-- Stripe Element will be inserted here -->
                        </div>

                        <button id="submit" :disabled="loading || !stripe || !elements"
                            class="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-3 px-4 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed">
                            <span v-if="loading">Procesando...</span>
                            <span v-else>Pagar ${{ cartStore.totalPrice.toFixed(2) }}</span>
                        </button>
                    </form>

                    <div class="mt-4">
                        <button @click="goToAddressStep" class="text-sm text-indigo-600 hover:text-indigo-500">
                            &larr; Volver a Dirección
                        </button>
                    </div>
                </div>
            </div>

            <!-- Order Summary Sidebar -->
            <div class="lg:col-span-1">
                <div class="bg-white dark:bg-gray-800 p-6 rounded-lg shadow-lg sticky top-4">
                    <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Resumen del Pedido</h2>
                    <ul class="divide-y divide-gray-200 dark:divide-gray-700 mb-4">
                        <li v-for="item in cartStore.items" :key="item.productId" class="py-4 flex justify-between">
                            <div>
                                <p class="text-sm font-medium text-gray-900 dark:text-white">{{ item.name }}</p>
                                <p class="text-sm text-gray-500 dark:text-gray-400">Cant: {{ item.quantity }}</p>
                            </div>
                            <p class="text-sm font-medium text-gray-900 dark:text-white">${{ (item.price *
                                item.quantity).toFixed(2) }}</p>
                        </li>
                    </ul>
                    <div class="border-t border-gray-200 dark:border-gray-700 pt-4 flex justify-between">
                        <span class="text-base font-medium text-gray-900 dark:text-white">Total</span>
                        <span class="text-xl font-bold text-gray-900 dark:text-white">${{
                            cartStore.totalPrice.toFixed(2)
                            }}</span>
                    </div>

                    <div v-if="selectedAddress" class="mt-6 border-t border-gray-200 dark:border-gray-700 pt-4">
                        <h3 class="text-sm font-medium text-gray-900 dark:text-white mb-2">Enviar a:</h3>
                        <p class="text-sm text-gray-500 dark:text-gray-400">
                            {{ selectedAddress?.addressLine1 }}<br>
                            {{ selectedAddress?.city }}, {{ selectedAddress?.state }}
                        </p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
