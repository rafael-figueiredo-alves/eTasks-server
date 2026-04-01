window.webAuth = {
    send: async function (method, url, payload) {
        try {
            const response = await fetch(url, {
                method: method,
                headers: payload ? {
                    "Content-Type": "application/json"
                } : undefined,
                credentials: "include",
                body: payload
            });

            return {
                isNetworkError: false,
                statusCode: response.status,
                body: await response.text()
            };
        } catch (error) {
            return {
                isNetworkError: true,
                statusCode: 0,
                body: error?.message ?? "Falha de rede."
            };
        }
    },
    login: async function (url, payload) {
        try {
            const response = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                credentials: "include",
                body: JSON.stringify(payload)
            });

            return {
                isNetworkError: false,
                statusCode: response.status,
                body: await response.text()
            };
        } catch (error) {
            return {
                isNetworkError: true,
                statusCode: 0,
                body: error?.message ?? "Falha de rede."
            };
        }
    },
    logout: async function (url) {
        try {
            const response = await fetch(url, {
                method: "POST",
                credentials: "include"
            });

            return {
                isNetworkError: false,
                statusCode: response.status,
                body: await response.text()
            };
        } catch (error) {
            return {
                isNetworkError: true,
                statusCode: 0,
                body: error?.message ?? "Falha de rede."
            };
        }
    }
};
