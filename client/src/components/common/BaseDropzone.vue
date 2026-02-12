<script setup lang="ts">
import { ref } from 'vue';
import { CloudArrowUpIcon, XMarkIcon } from '@heroicons/vue/24/outline';

const props = defineProps<{
    modelValue?: File | null;
    accept?: string;
    maxSize?: number; // in bytes
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', file: File | null): void;
    (e: 'error', message: string): void;
}>();

const isDragging = ref(false);
const previewUrl = ref<string | null>(null);
const fileInput = ref<HTMLInputElement | null>(null);

const handleDragOver = (e: DragEvent) => {
    e.preventDefault();
    isDragging.value = true;
};

const handleDragLeave = (e: DragEvent) => {
    e.preventDefault();
    isDragging.value = false;
};

const validateFile = (file: File): boolean => {
    if (props.accept) {
        const acceptedTypes = props.accept.split(',').map(t => t.trim());
        if (!acceptedTypes.some(type => file.type.match(type.replace('*', '.*')))) {
            emit('error', 'Tipo de archivo no permitido.');
            return false;
        }
    }

    if (props.maxSize && file.size > props.maxSize) {
        emit('error', `El archivo excede el tamaño máximo de ${props.maxSize / 1024 / 1024}MB.`);
        return false;
    }

    return true;
};

const handleDrop = (e: DragEvent) => {
    e.preventDefault();
    isDragging.value = false;

    if (e.dataTransfer?.files.length) {
        const file = e.dataTransfer.files[0];
        if (file && validateFile(file)) {
            processFile(file);
        }
    }
};

const handleFileSelect = (e: Event) => {
    const input = e.target as HTMLInputElement;
    if (input.files?.length) {
        const file = input.files[0];
        if (file && validateFile(file)) {
            processFile(file);
        }
    }
};

const processFile = (file: File) => {
    emit('update:modelValue', file);

    // Create preview
    if (file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = (e) => {
            previewUrl.value = e.target?.result as string;
        };
        reader.readAsDataURL(file);
    } else {
        previewUrl.value = null;
    }
};

const removeFile = () => {
    emit('update:modelValue', null);
    previewUrl.value = null;
    if (fileInput.value) {
        fileInput.value.value = '';
    }
};

const triggerInput = () => {
    fileInput.value?.click();
};
</script>

<template>
    <div class="w-full">
        <div @dragover="handleDragOver" @dragleave="handleDragLeave" @drop="handleDrop" @click="triggerInput" :class="[
            'relative border-2 border-dashed rounded-lg p-4 text-center cursor-pointer transition-colors flex flex-col items-center justify-center min-h-[200px]',
            isDragging
                ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20'
                : 'border-gray-300 dark:border-gray-600 hover:border-indigo-400 dark:hover:border-indigo-500'
        ]">
            <input ref="fileInput" type="file" class="hidden" :accept="accept" @change="handleFileSelect" />

            <div v-if="previewUrl" class="relative w-40 h-40 group">
                <img :src="previewUrl" class="w-full h-full object-cover rounded-lg shadow-sm" alt="Preview" />
                <button @click.stop="removeFile"
                    class="absolute -top-2 -right-2 p-1 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity hover:bg-red-600 focus:outline-none shadow-md">
                    <XMarkIcon class="h-4 w-4" />
                </button>
            </div>

            <div v-else class="flex flex-col items-center justify-center space-y-2">
                <CloudArrowUpIcon class="h-10 w-10 text-gray-400" />
                <p class="text-sm text-gray-600 dark:text-gray-300">
                    <span class="font-semibold text-indigo-600 dark:text-indigo-400">Haz clic para subir</span>
                    o arrastra y suelta
                </p>
                <p v-if="accept" class="text-xs text-gray-500 dark:text-gray-400">
                    {{ accept }} (Max. {{ maxSize ? maxSize / 1024 / 1024 : 5 }}MB)
                </p>
            </div>
        </div>
    </div>
</template>
