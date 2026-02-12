<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
    modelValue: string | Date;
    label?: string;
    id?: string;
    required?: boolean;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void;
}>();

const inputValue = computed({
    get: () => {
        if (!props.modelValue) return '';
        const date = new Date(props.modelValue);
        return date.toISOString().split('T')[0];
    },
    set: (value: string) => {
        emit('update:modelValue', value);
    }
});
</script>

<template>
    <div>
        <label v-if="label" :for="id" class="block text-sm font-medium leading-6 text-gray-900 dark:text-white">
            {{ label }}
        </label>
        <div class="mt-2">
            <input 
                type="date" 
                :id="id" 
                :required="required"
                v-model="inputValue"
                class="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 dark:bg-gray-700 dark:text-white dark:ring-gray-600"
            />
        </div>
    </div>
</template>
