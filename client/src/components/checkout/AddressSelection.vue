<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '@/services/api';
import BaseButton from '@/components/common/BaseButton.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { RadioGroup, RadioGroupLabel, RadioGroupOption } from '@headlessui/vue';
import { CheckCircleIcon } from '@heroicons/vue/24/solid';
import { PlusIcon, TrashIcon } from '@heroicons/vue/24/outline';
import ShippingForm from '@/components/profile/ShippingForm.vue'; // We might need to adapt this or create a new one
import { toast } from 'vue-sonner';
import type { ShippingAddress } from '@/types/dto';

const props = defineProps<{
    modelValue: ShippingAddress | null;
}>();

const emit = defineEmits(['update:modelValue']);

const addresses = ref<ShippingAddress[]>([]);
const loading = ref(false);
const showAddForm = ref(false);

const fetchAddresses = async () => {
    loading.value = true;
    try {
        const response = await apiClient.get<ShippingAddress[]>('/v1/shipping');
        addresses.value = response.data;

        // Auto-select if only one address and none selected
        if (addresses.value.length > 0 && !props.modelValue) {
            emit('update:modelValue', addresses.value[0]);
        }
    } catch (error) {
        console.error('Error fetching addresses:', error);
    } finally {
        loading.value = false;
    }
};

onMounted(fetchAddresses);

const selectAddress = (address: ShippingAddress) => {
    emit('update:modelValue', address);
};

const handleAddressAdded = async () => {
    // The ShippingForm currently posts directly. 
    // We need to refactor ShippingForm or just refresh the list.
    // If ShippingForm posts to /v1/shipping (POST), it adds a new address now (backend updated).
    // So we just need to refresh the list.
    await fetchAddresses();
    showAddForm.value = false;

    // Select the newly added address (it should be the last one or we find it)
    // Ideally backend returns the created address.
};

const deleteAddress = async (id: number) => {
    if (!confirm('¿Estás seguro de eliminar esta dirección?')) return;

    try {
        await apiClient.delete(`/v1/shipping/${id}`);
        toast.success('Dirección eliminada');
        await fetchAddresses();
        if (props.modelValue?.id === id) {
            emit('update:modelValue', null);
        }
    } catch (error) {
        console.error('Error deleting address:', error);
        toast.error('Error al eliminar la dirección');
    }
};

</script>

<template>
    <div class="space-y-6">
        <div v-if="loading" class="flex justify-center py-4">
            <LoadingSpinner />
        </div>

        <div v-else>
            <RadioGroup :modelValue="modelValue ?? undefined" @update:modelValue="selectAddress">
                <RadioGroupLabel class="sr-only">Dirección de envío</RadioGroupLabel>
                <div class="space-y-4">
                    <RadioGroupOption as="template" v-for="address in addresses" :key="address.id" :value="address"
                        v-slot="{ active, checked }">
                        <div :class="[
                            active ? 'ring-2 ring-indigo-500' : '',
                            checked ? 'bg-indigo-50 border-indigo-200 z-10' : 'border-gray-200',
                            'relative flex cursor-pointer rounded-lg border p-4 shadow-sm focus:outline-none'
                        ]">
                            <span class="flex flex-1">
                                <span class="flex flex-col">
                                    <RadioGroupLabel as="span" class="block text-sm font-medium text-gray-900">
                                        {{ address.name }}
                                    </RadioGroupLabel>
                                    <span class="mt-1 flex items-center text-sm text-gray-500">
                                        {{ address.addressLine1 }}
                                        <span v-if="address.addressLine2">, {{ address.addressLine2 }}</span>
                                    </span>
                                    <span class="mt-1 text-sm text-gray-500">
                                        {{ address.city }}, {{ address.state }} {{ address.postalCode }}
                                    </span>
                                    <span class="mt-1 text-sm text-gray-500">{{ address.country }}</span>
                                </span>
                            </span>
                            <CheckCircleIcon :class="[!checked ? 'invisible' : '', 'h-5 w-5 text-indigo-600']"
                                aria-hidden="true" />
                            <span :class="[
                                active ? 'border' : 'border-2',
                                checked ? 'border-indigo-500' : 'border-transparent',
                                'pointer-events-none absolute -inset-px rounded-lg'
                            ]" aria-hidden="true" />

                            <button @click.stop="deleteAddress(address.id!)"
                                class="absolute top-4 right-4 text-gray-400 hover:text-red-500">
                                <TrashIcon class="h-5 w-5" />
                            </button>
                        </div>
                    </RadioGroupOption>
                </div>
            </RadioGroup>

            <div v-if="addresses.length === 0" class="text-center py-6 text-gray-500">
                No tienes direcciones guardadas.
            </div>

            <div class="mt-6">
                <BaseButton v-if="!showAddForm" @click="showAddForm = true" variant="secondary"
                    class="w-full flex items-center justify-center">
                    <PlusIcon class="h-5 w-5 mr-2" />
                    Agregar Nueva Dirección
                </BaseButton>

                <div v-else class="bg-gray-50 p-4 rounded-lg border border-gray-200">
                    <div class="flex justify-between items-center mb-4">
                        <h4 class="text-sm font-medium text-gray-900">Nueva Dirección</h4>
                        <button @click="showAddForm = false"
                            class="text-sm text-gray-500 hover:text-gray-700">Cancelar</button>
                    </div>
                    <!-- Reuse ShippingForm but we need to make sure it handles the new 'Name' field and emits success -->
                    <!-- Since ShippingForm is not yet updated to handle 'Name' or emit success properly without redirecting or just showing toast,
                         we might need to update ShippingForm first. 
                         For now, let's assume we will update ShippingForm to emit 'saved'. -->
                    <ShippingForm @saved="handleAddressAdded" :is-modal="true" />
                </div>
            </div>
        </div>
    </div>
</template>
