<script setup lang="ts">
import { ref, watch } from 'vue';

const props = defineProps<{
    modelValue: string;
    label?: string;
    id?: string;
    required?: boolean;
    placeholder?: string;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void;
}>();

const displayValue = ref('');

const formatPhoneNumber = (value: string) => {
    if (!value) return value;
    const phoneNumber = value.replace(/[^\d]/g, '');
    const phoneNumberLength = phoneNumber.length;
    if (phoneNumberLength < 4) return phoneNumber;
    if (phoneNumberLength < 7) {
        return `(${phoneNumber.slice(0, 3)}) ${phoneNumber.slice(3)}`;
    }
    return `(${phoneNumber.slice(0, 3)}) ${phoneNumber.slice(3, 6)}-${phoneNumber.slice(6, 10)}`;
};

const handleInput = (event: Event) => {
    const target = event.target as HTMLInputElement;
    const formatted = formatPhoneNumber(target.value);
    displayValue.value = formatted;
    
    // Emit raw numbers only
    const rawValue = formatted.replace(/[^\d]/g, '');
    emit('update:modelValue', rawValue);
};

// Watch for external changes
watch(() => props.modelValue, (newValue) => {
    displayValue.value = formatPhoneNumber(newValue);
}, { immediate: true });
</script>

<template>
    <div>
        <label v-if="label" :for="id" class="block text-sm font-medium leading-6 text-gray-900 dark:text-white">
            {{ label }}
        </label>
        <div class="mt-2">
            <input 
                type="tel" 
                :id="id" 
                :required="required"
                :value="displayValue"
                @input="handleInput"
                :placeholder="placeholder || '(555) 123-4567'"
                class="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 dark:bg-gray-700 dark:text-white dark:ring-gray-600"
            />
        </div>
    </div>
</template>
