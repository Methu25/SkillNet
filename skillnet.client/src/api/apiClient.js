const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL || '';
const apiBaseUrl = configuredBaseUrl.replace(/\/+$/, '');

export class ApiError extends Error {
    constructor(message, status, details = null) {
        super(message);
        this.name = 'ApiError';
        this.status = status;
        this.details = details;
    }
}

const normalizePath = (path) => `/${String(path).replace(/^\/+/, '')}`;
const buildUrl = (path) => `${apiBaseUrl}${normalizePath(path)}`;

export const buildAssetUrl = (path) => {
    if (!path || /^https?:\/\//i.test(path)) return path;
    return buildUrl(path);
};

export const resolveApiUrl = buildAssetUrl;

const parseResponse = async (response, responseType) => {
    if (response.status === 204) return null;
    if (responseType === 'blob') return response.blob();

    const contentType = response.headers.get('content-type') || '';
    if (contentType.includes('application/json') || contentType.includes('problem+json')) {
        return response.json();
    }

    const text = await response.text();
    return text || null;
};

const getErrorMessage = (payload, status) => {
    if (payload?.errors) {
        const validationMessages = Object.values(payload.errors).flat();
        if (validationMessages.length > 0) return validationMessages.join(' ');
    }

    return payload?.message || payload?.Message || payload?.detail || payload?.title ||
        `Request failed with status ${status}.`;
};

export const apiRequest = async (path, options = {}) => {
    const token = localStorage.getItem('token');
    const headers = new Headers(options.headers || {});
    const isFormData = options.body instanceof FormData;

    if (!isFormData && options.body != null && !headers.has('Content-Type')) {
        headers.set('Content-Type', 'application/json');
    }
    if (token) {
        headers.set('Authorization', `Bearer ${token}`);
    }

    const response = await fetch(buildUrl(path), { ...options, headers });

    const payload = await parseResponse(response, options.responseType);
    if (!response.ok) {
        throw new ApiError(getErrorMessage(payload, response.status), response.status, payload);
    }

    return { data: payload, response };
};

export const jsonRequest = (path, method = 'GET', body) => apiRequest(path, {
    method,
    body: body == null ? undefined : JSON.stringify(body)
});
