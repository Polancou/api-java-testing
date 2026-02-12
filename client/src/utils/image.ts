export const getImageUrl = (path: string | null | undefined): string => {
    if (!path) return 'https://placehold.co/600x400';
    if (path.startsWith('http')) return path;

    // Get base URL from env or default to localhost:5272
    const baseUrl = import.meta.env.VITE_UPLOADS_BASE_URL || 'http://localhost:5272';

    // Ensure path starts with /
    const cleanPath = path.startsWith('/') ? path : `/${path}`;

    return `${baseUrl}${cleanPath}`;
};
