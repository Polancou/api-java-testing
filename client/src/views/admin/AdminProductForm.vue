<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import apiClient from '@/services/api';
import type { CreateProductDto, ProductDto } from '@/types/dto';
import { toast } from 'vue-sonner';
import BaseButton from '@/components/common/BaseButton.vue';
import BaseInput from '@/components/common/BaseInput.vue';
import BaseCurrencyInput from '@/components/common/BaseCurrencyInput.vue';
import BaseDropzone from '@/components/common/BaseDropzone.vue';
import { getImageUrl } from '@/utils/image';

const route = useRoute();
const router = useRouter();

const isEditMode = computed(() => route.params.id !== 'new');
const productId = computed(() => Number(route.params.id));
const loading = ref(false);
const currentStep = ref(1);
const selectedFile = ref<File | null>(null);

const form = ref<CreateProductDto>({
    name: '',
    description: '',
    price: 0,
    stock: 0,
    imageUrl: '',
    category: ''
});

const steps = [
    { id: 1, name: 'Información Básica', status: 'current' },
    { id: 2, name: 'Imagen', status: 'upcoming' },
    { id: 3, name: 'Confirmación', status: 'upcoming' }
];

const fetchProduct = async () => {
    if (!isEditMode.value) return;

    loading.value = true;
    try {
        const response = await apiClient.get<ProductDto>(`/v1/products/${productId.value}`);
        const product = response.data;
        form.value = {
            name: product.name,
            description: product.description,
            price: product.price,
            stock: product.stock,
            imageUrl: product.imageUrl || '',
            category: product.category || ''
        };
    } catch (error) {
        console.error('Error fetching product:', error);
        toast.error('Error al cargar el producto');
        router.push('/admin/products');
    } finally {
        loading.value = false;
    }
};

const nextStep = () => {
    if (currentStep.value < 3) currentStep.value++;
};

const prevStep = () => {
    if (currentStep.value > 1) currentStep.value--;
};

const handleSubmit = async () => {
    loading.value = true;
    try {
        let savedProductId = productId.value;

        // 1. Save Product Data
        if (isEditMode.value) {
            await apiClient.put(`/v1/products/${productId.value}`, form.value);
            toast.success('Información actualizada');
        } else {
            const response = await apiClient.post<ProductDto>('/v1/products', form.value);
            savedProductId = response.data.id;
            toast.success('Producto creado');
        }

        // 2. Upload Image if selected
        if (selectedFile.value) {
            const formData = new FormData();
            formData.append('file', selectedFile.value);
            await apiClient.post(`/v1/products/${savedProductId}/image`, formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            toast.success('Imagen subida correctamente');
        }

        router.push('/admin/products');
    } catch (error) {
        console.error('Error saving product:', error);
        toast.error('Error al guardar el producto');
    } finally {
        loading.value = false;
    }
};

onMounted(() => {
    fetchProduct();
});

const getPreviewUrl = (file: File | null) => {
    return file ? URL.createObjectURL(file) : '';
};
</script>

<template>
    <div class="max-w-3xl mx-auto">
        <!-- Wizard Steps -->
        <nav aria-label="Progress" class="mb-8">
            <ol role="list" class="space-y-4 md:flex md:space-x-8 md:space-y-0">
                <li v-for="step in steps" :key="step.name" class="md:flex-1">
                    <div v-if="currentStep > step.id"
                        class="group flex flex-col border-l-4 border-indigo-600 py-2 pl-4 hover:border-indigo-800 md:border-l-0 md:border-t-4 md:pl-0 md:pt-4 md:pb-0">
                        <span class="text-sm font-medium text-indigo-600 group-hover:text-indigo-800">Paso {{ step.id
                        }}</span>
                        <span class="text-sm font-medium text-gray-900 dark:text-white">{{ step.name }}</span>
                    </div>
                    <div v-else-if="currentStep === step.id"
                        class="flex flex-col border-l-4 border-indigo-600 py-2 pl-4 md:border-l-0 md:border-t-4 md:pl-0 md:pt-4 md:pb-0"
                        aria-current="step">
                        <span class="text-sm font-medium text-indigo-600">Paso {{ step.id }}</span>
                        <span class="text-sm font-medium text-gray-900 dark:text-white">{{ step.name }}</span>
                    </div>
                    <div v-else
                        class="group flex flex-col border-l-4 border-gray-200 py-2 pl-4 hover:border-gray-300 md:border-l-0 md:border-t-4 md:pl-0 md:pt-4 md:pb-0">
                        <span class="text-sm font-medium text-gray-500 group-hover:text-gray-700">Paso {{ step.id
                        }}</span>
                        <span class="text-sm font-medium text-gray-500 dark:text-gray-400">{{ step.name }}</span>
                    </div>
                </li>
            </ol>
        </nav>

        <div class="bg-white dark:bg-gray-800 shadow sm:rounded-lg p-6">
            <!-- Step 1: Basic Info -->
            <div v-show="currentStep === 1" class="space-y-6">
                <h3 class="text-lg font-medium leading-6 text-gray-900 dark:text-white">Información del Producto</h3>
                <div class="grid grid-cols-1 gap-y-6 gap-x-4 sm:grid-cols-6">
                    <div class="sm:col-span-4">
                        <BaseInput v-model="form.name" label="Nombre" id="name" name="name" type="text" required />
                    </div>
                    <div class="sm:col-span-2">
                        <BaseInput v-model="form.category" label="Categoría" id="category" name="category"
                            type="text" />
                    </div>
                    <div class="sm:col-span-6">
                        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Descripción</label>
                        <textarea v-model="form.description" rows="3"
                            class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm dark:bg-gray-700 dark:border-gray-600 dark:text-white"></textarea>
                    </div>
                    <div class="sm:col-span-3">
                        <BaseCurrencyInput v-model="form.price" label="Precio" id="price" required />
                    </div>
                    <div class="sm:col-span-3">
                        <BaseInput v-model="form.stock" type="number" label="Stock" id="stock" name="stock" required />
                    </div>
                </div>
            </div>

            <!-- Step 2: Image Upload -->
            <div v-show="currentStep === 2" class="space-y-6">
                <h3 class="text-lg font-medium leading-6 text-gray-900 dark:text-white">Imagen del Producto</h3>
                <BaseDropzone v-model="selectedFile" accept="image/*" :max-size="5242880" />
                <p v-if="form.imageUrl && !selectedFile" class="text-sm text-gray-500">
                    Imagen actual: <a :href="form.imageUrl" target="_blank" class="text-indigo-600 hover:underline">Ver
                        imagen</a>
                </p>
            </div>

            <!-- Step 3: Confirmation -->
            <div v-show="currentStep === 3" class="space-y-6">
                <div class="text-center">
                    <div class="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-green-100">
                        <CheckCircleIcon class="h-6 w-6 text-green-600" aria-hidden="true" />
                    </div>
                    <h3 class="mt-2 text-base font-semibold leading-6 text-gray-900 dark:text-white">Vista Previa</h3>
                    <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
                        Así es como se verá tu producto en la tienda.
                    </p>
                </div>

                <!-- Product Card Preview -->
                <div class="flex justify-center mt-6">
                    <div
                        class="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden w-full max-w-sm border border-gray-200 dark:border-gray-700">
                        <img :src="selectedFile ? getPreviewUrl(selectedFile) : getImageUrl(form.imageUrl)"
                            :alt="form.name" class="w-full h-48 object-cover">
                        <div class="p-4">
                            <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">{{ form.name || 'Nombre del Producto' }}</h2>
                            <p class="text-gray-600 dark:text-gray-300 mb-4 line-clamp-2 text-sm">{{ form.description ||
                                'Descripción del producto...' }}</p>
                            <div class="flex items-center justify-between mt-4">
                                <span class="text-2xl font-bold text-gray-900 dark:text-white">${{ form.price }}</span>
                                <button disabled
                                    class="bg-indigo-600 text-white px-4 py-2 rounded opacity-50 cursor-not-allowed flex items-center gap-2">
                                    Agregar
                                </button>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="mt-6 border-t border-gray-100 dark:border-gray-700 pt-6 text-left">
                    <dl class="divide-y divide-gray-100 dark:divide-gray-700">
                        <div class="px-4 py-3 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                            <dt class="text-sm font-medium leading-6 text-gray-900 dark:text-white">Nombre</dt>
                            <dd class="mt-1 text-sm leading-6 text-gray-700 dark:text-gray-300 sm:col-span-2 sm:mt-0">{{
                                form.name }}</dd>
                        </div>
                        <div class="px-4 py-3 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                            <dt class="text-sm font-medium leading-6 text-gray-900 dark:text-white">Precio</dt>
                            <dd class="mt-1 text-sm leading-6 text-gray-700 dark:text-gray-300 sm:col-span-2 sm:mt-0">
                                ${{ form.price }}</dd>
                        </div>
                        <div class="px-4 py-3 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                            <dt class="text-sm font-medium leading-6 text-gray-900 dark:text-white">Stock</dt>
                            <dd class="mt-1 text-sm leading-6 text-gray-700 dark:text-gray-300 sm:col-span-2 sm:mt-0">
                                {{ form.stock }}</dd>
                        </div>
                    </dl>
                </div>
            </div>

            <!-- Actions -->
            <div class="mt-8 flex justify-between">
                <BaseButton v-if="currentStep > 1" @click="prevStep" variant="secondary">
                    Anterior
                </BaseButton>
                <div v-else></div> <!-- Spacer -->

                <BaseButton v-if="currentStep < 3" @click="nextStep">
                    Siguiente
                </BaseButton>
                <BaseButton v-else @click="handleSubmit" :disabled="loading" variant="primary">
                    {{ loading ? 'Guardando...' : 'Guardar Producto' }}
                </BaseButton>
            </div>
        </div>
    </div>
</template>
