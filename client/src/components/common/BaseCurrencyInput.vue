<script setup lang="ts">
import { ref, watch } from 'vue';

const props = defineProps<{
    modelValue: number;
    label?: string;
    id?: string;
    required?: boolean;
    currency?: string;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: number): void;
}>();

const displayValue = ref('');

const formatCurrency = (value: number) => {
    if (value === null || value === undefined) return '';
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: props.currency || 'USD',
        minimumFractionDigits: 2
    }).format(value);
};

const handleInput = (event: Event) => {
    const target = event.target as HTMLInputElement;
    // Remove non-numeric chars except dot
    const rawValue = target.value.replace(/[^0-9.]/g, '');
    const numberValue = parseFloat(rawValue);
    
    if (!isNaN(numberValue)) {
        emit('update:modelValue', numberValue);
    }
};

const handleBlur = () => {
    displayValue.value = formatCurrency(props.modelValue);
};

const handleFocus = (event: Event) => {
    const target = event.target as HTMLInputElement;
    target.value = props.modelValue.toString();
};

watch(() => props.modelValue, (newValue) => {
    displayValue.value = formatCurrency(newValue);
}, { immediate: true });
</script>

<template>
    <div>
        <label v-if="label" :for="id" class="block text-sm font-medium leading-6 text-gray-900 dark:text-white">
            {{ label }}
        </label>
        <div class="mt-2 relative rounded-md shadow-sm">
            <div class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                <span class="text-gray-500 sm:text-sm">$</span>
            </div>
            <input 
                type="text" 
                :id="id" 
                :required="required"
                v-model="displayValue"
                @input="handleInput"
                @blur="handleBlur"
                @focus="handleFocus"
                class="block w-full rounded-md border-0 py-1.5 pl-7 pr-12 text-gray-900 ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 dark:bg-gray-700 dark:text-white dark:ring-gray-600"
                placeholder="0.00" 
            />
            <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3">
                <span class="text-gray-500 sm:text-sm">USD</span>
            </div>
        </div>
    </div>
</template>
