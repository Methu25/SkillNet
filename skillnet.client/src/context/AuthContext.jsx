import { createContext, useState, useEffect, useContext, useCallback } from 'react';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    const logout = useCallback(async () => {
        const refreshToken = localStorage.getItem('refreshToken');
        if (refreshToken) {
            try {
                await fetch('/api/auth/logout', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ refreshToken })
                });
            } catch (e) {
                console.error("Failed to revoke token during logout:", e);
            }
        }

        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        setUser(null);
    }, []);

    const performTokenRefresh = useCallback(async () => {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) return false;

        try {
            const response = await fetch('/api/auth/refresh-token', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ refreshToken })
            });

            if (response.ok) {
                const data = await response.json();
                localStorage.setItem('token', data.token);
                localStorage.setItem('refreshToken', data.refreshToken);
                
                // Fetch user data again with new token
                const meResponse = await fetch('/api/auth/me', {
                    headers: {
                        'Authorization': `Bearer ${data.token}`
                    }
                });

                if (meResponse.ok) {
                    const userData = await meResponse.json();
                    setUser(userData);
                    return true;
                }
            }
        } catch (error) {
            console.error("Refresh token failed:", error);
        }
        return false;
    }, []);

    useEffect(() => {
        const initializeAuth = async () => {
            const token = localStorage.getItem('token');
            if (token) {
                try {
                    const response = await fetch('/api/auth/me', {
                        headers: {
                            'Authorization': `Bearer ${token}`
                        }
                    });

                    if (response.ok) {
                        const userData = await response.json();
                        setUser(userData);
                    } else if (response.status === 401) {
                        // Attempt token refresh
                        const refreshSuccess = await performTokenRefresh();
                        if (!refreshSuccess) {
                            await logout();
                        }
                    } else {
                        await logout();
                    }
                } catch (error) {
                    console.error("Auth initialization failed:", error);
                    await logout();
                }
            }
            setLoading(false);
        };

        initializeAuth();
    }, [logout, performTokenRefresh]);

    const login = async (email, password) => {
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        const data = await response.json();
        if (!response.ok) {
            throw new Error(data.message || 'Login failed');
        }

        localStorage.setItem('token', data.token);
        localStorage.setItem('refreshToken', data.refreshToken);

        // Fetch /me to load profile details
        const meResponse = await fetch('/api/auth/me', {
            headers: {
                'Authorization': `Bearer ${data.token}`
            }
        });

        if (meResponse.ok) {
            const userData = await meResponse.json();
            setUser(userData);
            return userData;
        } else {
            throw new Error('Failed to retrieve user profile after login.');
        }
    };

    const register = async (userData) => {
        const response = await fetch('/api/auth/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userData)
        });

        const data = await response.json();
        if (!response.ok) {
            throw new Error(data.message || 'Registration failed');
        }

        return data;
    };

    return (
        <AuthContext.Provider value={{ user, loading, login, logout, register, performTokenRefresh }}>
            {children}
        </AuthContext.Provider>
    );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => useContext(AuthContext);
