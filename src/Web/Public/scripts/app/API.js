class API {
    static async fetchAsync(request, body = {}) {
        const response = await fetch("/api/" + request, {
            method: "POST",

            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        return await response.json();
    };
};
