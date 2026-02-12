<script setup lang="ts">
import { useRouter } from 'vue-router';
import {
    HomeIcon,
    ShoppingBagIcon,
    ClipboardDocumentListIcon
} from '@heroicons/vue/24/outline';

const router = useRouter();

const navigation = [
    { name: 'Dashboard', href: '/admin', icon: HomeIcon },
    { name: 'Productos', href: '/admin/products', icon: ShoppingBagIcon },
    { name: 'Pedidos', href: '/admin/orders', icon: ClipboardDocumentListIcon },
];

const isRouteActive = (href: string) => {
    if (href === '/admin') {
        return router.currentRoute.value.path === '/admin';
    }
    return router.currentRoute.value.path.startsWith(href);
};
</script>

<template>
    <div class="h-screen bg-gray-100 dark:bg-gray-900 flex flex-col md:flex-row overflow-hidden">
        <!-- Sidebar for Desktop -->
        <div class="hidden md:flex md:w-64 md:flex-col m-4 mr-0">
            <div
                class="flex min-h-0 flex-1 flex-col bg-white dark:bg-gray-800 rounded-2xl shadow-lg overflow-hidden border border-gray-200 dark:border-gray-700">
                <div
                    class="flex h-16 flex-shrink-0 items-center bg-gray-50 dark:bg-gray-900 px-4 border-b border-gray-200 dark:border-gray-700">
                    <h1 class="text-xl font-bold text-gray-900 dark:text-white">Admin Panel</h1>
                </div>
                <div class="flex flex-1 flex-col overflow-y-auto">
                    <nav class="flex-1 flex flex-col py-4">
                        <router-link v-for="item in navigation" :key="item.name" :to="item.href"
                            class="group flex items-center px-4 py-3 text-sm font-medium border-l-4 transition-colors gap-3"
                            :class="[
                                isRouteActive(item.href)
                                    ? 'border-indigo-500 text-indigo-700 dark:text-indigo-300 bg-indigo-50 dark:bg-indigo-900/20'
                                    : 'border-transparent text-gray-600 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-gray-200'
                            ]">
                            <component :is="item.icon" class="h-5 w-5" />
                            {{ item.name }}
                        </router-link>
                    </nav>
                </div>
            </div>
        </div>

        <!-- Mobile Tabs Navigation -->
        <div class="md:hidden bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
            <div class="flex overflow-x-auto">
                <router-link v-for="item in navigation" :key="item.name" :to="item.href"
                    class="flex-1 min-w-max text-center py-3 px-4 text-sm font-medium border-b-2 transition-colors whitespace-nowrap flex items-center justify-center gap-2"
                    :class="[
                        isRouteActive(item.href)
                            ? 'border-indigo-500 text-indigo-600 dark:text-indigo-400'
                            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-300'
                    ]">
                    <component :is="item.icon" class="h-5 w-5" />
                    {{ item.name }}
                </router-link>
            </div>
        </div>

        <!-- Main Content -->
        <div class="flex flex-1 flex-col overflow-hidden">
            <!-- Main Content Scrollable Area -->
            <main class="flex-1 overflow-y-auto">
                <div class="py-6">
                    <div class="mx-auto max-w-7xl px-4 sm:px-6 md:px-8">
                        <router-view></router-view>
                    </div>
                </div>
            </main>
        </div>
    </div>
</template>
