<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { Form } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { z } from 'zod';
import apiClient from '@/services/api';
import BaseInput from '@/components/common/BaseInput.vue';
import BaseButton from '@/components/common/BaseButton.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { toast } from 'vue-sonner';

const props = defineProps<{
    isModal?: boolean;
}>();

const emit = defineEmits(['saved']);

const loading = ref(false);
const initialValues = ref({
    name: 'Casa',
    addressLine1: '',
    addressLine2: '',
    city: '',
    state: '',
    postalCode: '',
    country: ''
});

const schema = toTypedSchema(z.object({
    name: z.string().nonempty('El nombre es obligatorio (ej. Casa, Oficina).'),
    addressLine1: z.string().nonempty('La dirección es obligatoria.'),
    addressLine2: z.string().optional(),
    city: z.string().nonempty('La ciudad es obligatoria.'),
    state: z.string().nonempty('El estado/provincia es obligatorio.'),
    postalCode: z.string().nonempty('El código postal es obligatorio.'),
    country: z.string().nonempty('El país es obligatorio.')
}));

onMounted(async () => {
    if (props.isModal) return; // Don't fetch if adding new in modal

    loading.value = true;
    try {
        // This endpoint now returns a list, but for Profile view we might want the "default" or first one?
        // Or we should update ProfileView to also use AddressSelection?
        // For now, let's keep it simple: ProfileView might break if we don't handle the list return.
        // The backend GET /v1/shipping now returns IEnumerable<ShippingAddressDto>.
        // So we need to handle that.
        const response = await apiClient.get('/v1/shipping');
        if (response.data && Array.isArray(response.data) && response.data.length > 0) {
            initialValues.value = response.data[0];
        }
    } catch (error) {
        console.error('Error loading shipping address:', error);
    } finally {
        loading.value = false;
    }
});

const handleSubmit = async (values: Record<string, unknown>) => {
    loading.value = true;
    try {
        await apiClient.post('/v1/shipping', values);
        toast.success('Dirección guardada correctamente.');
        emit('saved');
    } catch (error) {
        console.error('Error saving shipping address:', error);
        toast.error('Error al guardar la dirección.');
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <div :class="[isModal ? '' : 'bg-white dark:bg-gray-800 shadow overflow-hidden sm:rounded-lg']">
        <div v-if="!isModal" class="px-4 py-5 sm:px-6">
            <h3 class="text-lg leading-6 font-medium text-gray-900 dark:text-gray-100">
                Dirección de Envío
            </h3>
            <p class="mt-1 max-w-2xl text-sm text-gray-500 dark:text-gray-400">
                Esta dirección se utilizará para tus pedidos.
            </p>
        </div>
        <div :class="[isModal ? '' : 'border-t border-gray-200 dark:border-gray-700 px-4 py-5 sm:px-6']">
            <div v-if="loading && !initialValues.addressLine1 && !isModal" class="flex justify-center py-8">
                <LoadingSpinner />
            </div>

            <Form v-else @submit="handleSubmit" :validation-schema="schema" :initial-values="initialValues"
                class="space-y-4">

                <BaseInput name="name" label="Nombre (ej. Casa, Oficina)" id="name" type="text"
                    placeholder="Ej. Casa" />

                <BaseInput name="addressLine1" label="Dirección (Calle y número)" id="addressLine1" type="text"
                    placeholder="Ej. Av. Reforma 123" />
                <BaseInput name="addressLine2" label="Apartamento, suite, etc. (Opcional)" id="addressLine2" type="text"
                    placeholder="Ej. Depto 4B" />

                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <BaseInput name="city" label="Ciudad" id="city" type="text" />
                    <BaseInput name="state" label="Estado / Provincia" id="state" type="text" />
                </div>

                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <BaseInput name="postalCode" label="Código Postal" id="postalCode" type="text" />
                    <BaseInput name="country" label="País" id="country" type="text" />
                </div>

                <div class="flex justify-end pt-4">
                    <BaseButton type="submit" :disabled="loading" variant="primary">
                        <LoadingSpinner v-if="loading" class="h-5 w-5 mr-2" />
                        {{ loading ? 'Guardando...' : 'Guardar Dirección' }}
                    </BaseButton>
                </div>
            </Form>
        </div>
    </div>
</template>
