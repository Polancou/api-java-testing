<script setup lang="ts">
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { onMounted, ref, computed } from 'vue';
import { useAuthStore } from '@/stores/auth'
import BaseInput from '@/components/common/BaseInput.vue';
import BaseButton from '@/components/common/BaseButton.vue';
import BaseSkeleton from '@/components/common/BaseSkeleton.vue';
import ShippingForm from '@/components/profile/ShippingForm.vue';
// Actually, let's import the content of SecurityView as a component if possible, or just copy the logic.
// Since SecurityView is a view, let's treat it as a component here or refactor it.
// For now, I will assume we can use the logic from SecurityView. Ideally we should extract it to a component.
// Let's use a dynamic component approach or just v-if tabs.

import { Form } from 'vee-validate';
import { z } from 'zod';
import { toTypedSchema } from '@vee-validate/zod';
import { toast } from 'vue-sonner';
import type { ActualizarPerfilDto, CambiarPasswordDto } from '@/types/dto';

// Obtenemos la instancia de la auth store.
const authStore = useAuthStore();
// Define la ruta de la imagen de perfil
const uploadsBaseUrl = import.meta.env.VITE_UPLOADS_BASE_URL;
// Variable para controlar el modo de edición del perfil
const isEditing = ref(false);

// Tabs configuration
const activeTab = ref('general');
const tabs = [
  { id: 'general', name: 'Información General' },
  { id: 'shipping', name: 'Dirección de Envío' },
  { id: 'security', name: 'Seguridad' }
];

// --- General Profile Logic ---

// Define el esquema de validación con Zod
const updateProfileSchema = toTypedSchema(
  z.object({
    nombreCompleto: z.string()
      .nonempty('El nombre es obligatorio.')
      .min(3, 'El nombre debe tener al menos 3 caracteres.')
      .max(100, 'El nombre no puede tener más de 100 caracteres.'),
    numeroTelefono: z.string()
      .nonempty('El número de teléfono es obligatorio.')
  })
);

// Se define el estado inicial de los datos editables del perfil
const editableProfile = ref({
  nombreCompleto: '',
  numeroTelefono: ''
});

// El input donde se carga la imagen de perfil
const fileInput = ref<HTMLInputElement | null>(null);

// Función para seleccionar el archivo con validaciones
const onFileSelected = (event: Event) => {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    const file = target.files[0];

    if (!file) return;

    // 1. Validar tamaño (2MB)
    if (file.size > 2 * 1024 * 1024) {
      toast.error("La imagen es demasiado pesada (Máx 2MB).");
      target.value = ''; // Limpiar input
      return;
    }

    // 2. Validar tipo
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      toast.error("Formato no soportado (Solo JPG, PNG, WEBP).");
      target.value = '';
      return;
    }

    // Si pasa las validaciones, subimos
    authStore.uploadAvatar(file);

    // Resetea el input por si el usuario quiere subir el mismo archivo de nuevo
    if (fileInput.value) {
      fileInput.value.value = '';
    }
  }
}

// Función para activar el input de archivo
const triggerFileInput = () => {
  fileInput.value?.click();
}

/**
 * Acción que se ejecuta antes de renderizar la vista.
 */
onMounted(async () => {
  // Solamente carga los datos si el usuario está autenticado pero los datos son null.
  if (authStore.isAuthenticated && !authStore.userProfile)
    await authStore.fetchProfile();
});

/**
 * Función para habilitar el modo de edición del perfil.
 */
const enableEditing = () => {
  if (authStore.userProfile) {
    // Inicializa los datos editables con los datos actuales del perfil
    editableProfile.value.nombreCompleto = authStore.userProfile.nombreCompleto;
    editableProfile.value.numeroTelefono = authStore.userProfile.numeroTelefono;
  }
  isEditing.value = true; // Activa el modo edición
};

/**
 * Función para manejar el guardado de los cambios en el perfil.
 */
const handleSaveChanges = async (values: Record<string, unknown>) => {
  // Seguridad adicional: solo se puede guardar si el perfil está cargado
  if (!authStore.userProfile) return

  const profileData = values as unknown as ActualizarPerfilDto;

  // Guardamos los cambios en el estado local
  const success = await authStore.updateProfile(profileData)

  if (success) {
    // Salimos del modo de edición solo si fue exitoso
    isEditing.value = false
  }
};

/**
 * Función para cancelar la edición del perfil.
 */
const cancelEdit = () => {
  isEditing.value = false;
  authStore.error = null;
};

const profileImageUrl = computed(() => {
  const avatarUrl = authStore.userProfile?.avatarUrl?.trim();
  const genericAvatar = 'https://www.phoenixptsp.com/wp-content/uploads/2019/01/generic-profile-icon-10.jpg.png';

  if (!avatarUrl) {
    // 1. Si no hay URL, usa la genérica
    return genericAvatar;
  }

  if (avatarUrl.startsWith('http://') || avatarUrl.startsWith('https://')) {
    // 2. Si es una URL absoluta (como la de Google), úsala directamente
    return avatarUrl;
  }

  // 3. Si es una URL relativa (como /uploads/...), prepende el host
  return `${uploadsBaseUrl}${avatarUrl}`;
});

// --- Security Logic (Inline) ---
const passwordSchema = toTypedSchema(
  z.object({
    oldPassword: z.string().nonempty('La contraseña actual es obligatoria.'),
    newPassword: z.string()
      .nonempty("La nueva contraseña es obligatoria.")
      .min(8, "La contraseña debe tener al menos 8 caracteres.")
      .regex(/[A-Z]/, "Debe contener al menos una mayúscula.")
      .regex(/[a-z]/, "Debe contener al menos una minúscula.")
      .regex(/[0-9]/, "Debe contener al menos un número."),
    confirmPassword: z.string()
      .nonempty('Confirma tu nueva contraseña.')
  })
    .refine(data => data.newPassword === data.confirmPassword, {
      message: "Las contraseñas nuevas no coinciden.",
      path: ["confirmPassword"],
    })
)

const handleChangePassword = async (values: Record<string, unknown>, { resetForm }: { resetForm: () => void }) => {
  const passwordData = values as unknown as CambiarPasswordDto;
  const success = await authStore.changePassword(passwordData);
  if (success) {
    resetForm();
  }
}
</script>

<template>
  <div>
    <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100 mb-6">Mi Perfil</h1>

    <div v-if="authStore.isLoading && !authStore.userProfile"
      class="bg-white dark:bg-gray-800 shadow rounded-lg overflow-hidden p-6">
      <div class="flex flex-col items-center">
        <BaseSkeleton width="8rem" height="8rem" borderRadius="9999px" class="mb-4" />
        <BaseSkeleton width="200px" height="24px" class="mb-2" />
        <BaseSkeleton width="150px" height="16px" />
      </div>
    </div>

    <div v-else-if="authStore.error"
      class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative dark:bg-red-900 dark:border-red-700 dark:text-red-300"
      role="alert">
      <strong class="font-bold">Error:</strong>
      <span class="block sm:inline"> {{ authStore.error }}</span>
    </div>

    <div v-else-if="authStore.userProfile" class="grid grid-cols-1 lg:grid-cols-4 gap-6">

      <!-- Sidebar / Profile Card -->
      <div class="lg:col-span-1">
        <div class="bg-white dark:bg-gray-800 shadow rounded-lg p-6 text-center">
          <input type="file" ref="fileInput" @change="onFileSelected" accept="image/png, image/jpeg, image/webp"
            class="hidden" />

          <div class="relative w-32 h-32 mx-auto mb-4 group">
            <img :src="profileImageUrl" alt="Foto de perfil"
              class="w-32 h-32 rounded-full object-cover border-4 border-white dark:border-gray-700 shadow-md" />
            <div @click="triggerFileInput"
              class="absolute inset-0 w-full h-full flex items-center justify-center bg-black bg-opacity-50 rounded-full cursor-pointer opacity-0 group-hover:opacity-100 transition-opacity duration-200">
              <span class="text-white text-sm font-medium">Cambiar</span>
            </div>
            <div v-if="authStore.isLoading"
              class="absolute inset-0 w-full h-full flex items-center justify-center bg-black bg-opacity-70 rounded-full z-10">
              <LoadingSpinner />
            </div>
          </div>

          <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100 break-words">{{
            authStore.userProfile.nombreCompleto }}</h2>
          <p class="text-sm text-gray-500 dark:text-gray-400 break-all">{{ authStore.userProfile.email }}</p>
          <p class="text-xs text-gray-400 dark:text-gray-500 mt-2">Miembro desde {{ new
            Date(authStore.userProfile.fechaRegistro).toLocaleDateString() }}</p>
        </div>

        <!-- Mobile Tabs (Dropdown or Scroll) could go here, but simple vertical stack on mobile is fine for now -->
        <div class="mt-6 bg-white dark:bg-gray-800 shadow rounded-lg overflow-hidden">
          <nav class="flex flex-col">
            <button v-for="tab in tabs" :key="tab.id" @click="activeTab = tab.id"
              :class="['px-4 py-3 text-left text-sm font-medium border-l-4 transition-colors',
                activeTab === tab.id
                  ? 'border-indigo-500 text-indigo-700 dark:text-indigo-300 bg-indigo-50 dark:bg-indigo-900/20'
                  : 'border-transparent text-gray-600 dark:text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-gray-200']">
              {{ tab.name }}
            </button>
          </nav>
        </div>
      </div>

      <!-- Main Content Area -->
      <div class="lg:col-span-3">

        <!-- General Tab -->
        <div v-if="activeTab === 'general'" class="bg-white dark:bg-gray-800 shadow rounded-lg overflow-hidden">
          <div
            class="px-4 py-5 sm:px-6 border-b border-gray-200 dark:border-gray-700 flex justify-between items-center">
            <div>
              <h3 class="text-lg leading-6 font-medium text-gray-900 dark:text-gray-100">Información Personal</h3>
              <p class="mt-1 max-w-2xl text-sm text-gray-500 dark:text-gray-400">Detalles de tu cuenta</p>
            </div>
            <BaseButton v-if="!isEditing" @click="enableEditing" variant="secondary" size="sm">Editar</BaseButton>
          </div>

          <div v-if="!isEditing" class="px-4 py-5 sm:p-6">
            <dl class="grid grid-cols-1 gap-x-4 gap-y-6 sm:grid-cols-2">
              <div class="sm:col-span-1">
                <dt class="text-sm font-medium text-gray-500 dark:text-gray-400">Nombre Completo</dt>
                <dd class="mt-1 text-sm text-gray-900 dark:text-gray-100">{{ authStore.userProfile.nombreCompleto }}
                </dd>
              </div>
              <div class="sm:col-span-1">
                <dt class="text-sm font-medium text-gray-500 dark:text-gray-400">Email</dt>
                <dd class="mt-1 text-sm text-gray-900 dark:text-gray-100">{{ authStore.userProfile.email }}</dd>
              </div>
              <div class="sm:col-span-1">
                <dt class="text-sm font-medium text-gray-500 dark:text-gray-400">Teléfono</dt>
                <dd class="mt-1 text-sm text-gray-900 dark:text-gray-100">{{ authStore.userProfile.numeroTelefono }}
                </dd>
              </div>
              <div class="sm:col-span-1">
                <dt class="text-sm font-medium text-gray-500 dark:text-gray-400">Rol</dt>
                <dd class="mt-1 text-sm text-gray-900 dark:text-gray-100">{{ authStore.userProfile.rol }}</dd>
              </div>
            </dl>
          </div>

          <div v-else class="px-4 py-5 sm:p-6">
            <Form @submit="handleSaveChanges" :validation-schema="updateProfileSchema" :initial-values="editableProfile"
              class="space-y-4 max-w-lg">
              <BaseInput name="nombreCompleto" label="Nombre Completo" id="editNombreCompleto" type="text" required />
              <BaseInput name="numeroTelefono" label="Número de Teléfono" id="editNumeroTelefono" type="tel" required />

              <div class="flex justify-end space-x-3 pt-4">
                <BaseButton type="button" @click="cancelEdit" :disabled="authStore.isLoading" variant="secondary">
                  Cancelar</BaseButton>
                <BaseButton type="submit" :disabled="authStore.isLoading" variant="primary">
                  {{ authStore.isLoading ? 'Guardando...' : 'Guardar Cambios' }}
                </BaseButton>
              </div>
            </Form>
          </div>
        </div>

        <!-- Shipping Tab -->
        <div v-if="activeTab === 'shipping'">
          <ShippingForm />
        </div>

        <!-- Security Tab -->
        <div v-if="activeTab === 'security'" class="bg-white dark:bg-gray-800 shadow rounded-lg overflow-hidden">
          <div class="px-4 py-5 sm:px-6 border-b border-gray-200 dark:border-gray-700">
            <h3 class="text-lg leading-6 font-medium text-gray-900 dark:text-gray-100">Seguridad</h3>
            <p class="mt-1 max-w-2xl text-sm text-gray-500 dark:text-gray-400">Actualiza tu contraseña</p>
          </div>
          <div class="px-4 py-5 sm:p-6">
            <Form @submit="handleChangePassword" :validation-schema="passwordSchema" v-slot="{ meta }"
              class="space-y-4 max-w-lg">
              <BaseInput name="oldPassword" label="Contraseña Actual" id="oldPassword" type="password"
                placeholder="Ingresa tu contraseña actual" />
              <BaseInput name="newPassword" label="Nueva Contraseña" id="newPassword" type="password"
                placeholder="Ingresa tu nueva contraseña" />
              <BaseInput name="confirmPassword" label="Confirmar Nueva Contraseña" id="confirmPassword" type="password"
                placeholder="Confirma tu nueva contraseña" />

              <div class="flex justify-end pt-2">
                <BaseButton type="submit" :disabled="!meta.valid || authStore.isLoading" variant="primary">
                  <LoadingSpinner v-if="authStore.isLoading" class="h-5 w-5 mr-2" />
                  {{ authStore.isLoading ? 'Guardando...' : 'Cambiar Contraseña' }}
                </BaseButton>
              </div>
            </Form>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<style scoped></style>
